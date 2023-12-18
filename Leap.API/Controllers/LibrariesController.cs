using System.Security.Claims;
using Leap.API.DB;
using Leap.API.DB.Entities;
using Leap.API.Extensions;
using Leap.API.Interfaces;
using Leap.API.Services;
using Leap.Common.DTO.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semver;

namespace Leap.API.Controllers;

[ApiController]
[Route("[controller]/{author}/{name}")]
public class LibrariesController
(
	LeapApiDbContext context,
	ILibraryStorage storage,
	ILogger<LibrariesController> logger,
	IUploadEndpointGenerator uploadEndpointGenerator,
	LinkGenerator linkGenerator
)
	: LibraryControllerBase
{
	[HttpGet("{version?}")]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(SparseLibraryVersionDto), StatusCodes.Status200OK)]
	public async Task<IActionResult> Get(string author, string name, string? version, CancellationToken cancellationToken = default)
	{
		var library = await context.Libraries
			.Include(library => library.LatestVersion)
			.ThenInclude(v => v!.Links)
			.Include(library => library.Versions)
			.ThenInclude(v => v.Links)
			.FirstOrDefaultAsync(l => l.Author == author && l.Name == name, cancellationToken);

		if (library is null)
			return NotFound();

		LibraryVersion? libraryVersion;
		if (version is null)
		{
			libraryVersion = library.LatestVersion;
		}
		else if (SemVersionRange.TryParse(version, out var semVersionRange))
		{
			libraryVersion = library.Versions
				.Where(v => semVersionRange.Contains(SemVersion.Parse(v.Version, SemVersionStyles.Strict)))
				.MaxBy(v => SemVersion.Parse(v.Version, SemVersionStyles.Strict));
		}
		else if (SemVersion.TryParse(version, SemVersionStyles.Strict, out var semVersion))
		{
			libraryVersion =
				library.Versions.FirstOrDefault(
					v =>
						SemVersion.Parse(v.Version, SemVersionStyles.Strict) == semVersion
				);
		}
		else
		{
			logger.LogInformation(
				"Malformed version requested: '{Version}' (not a SemVersion nor a SemVersionRange)",
				version
			);

			return BadRequest(
				new
				{
					code = "invalid_version",
					message = "The requested version is not a valid semantic version nor a valid semantic version range.",
				}
			);
		}

		if (libraryVersion is null)
			return NotFound();

		var downloadUrl = GetDownloadUrl(linkGenerator, author, name, libraryVersion.Version);

		return Ok(libraryVersion.ToSparse(downloadUrl));
	}

	[HttpGet("{version}/download")]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
	public async Task<IActionResult> Download(string author, string name, string version, CancellationToken cancellationToken = default)
	{
		logger.LogTrace("Incoming download request for {Author}/{Name}@{Version}", author, name, version);

		try
		{
			if (!await storage.ExistsAsync(author, name, version, cancellationToken))
			{
				logger.LogWarning(
					"Requested version does not exist in storage: {Author}/{Name}@{Version}",
					author,
					name,
					version
				);

				return NotFound();
			}

			await storage.UpdateMetadataAsync(
				author,
				name,
				version,
				metadata =>
				{
					metadata.Downloads++;

					logger.LogTrace(
						"Increased download count for {Author}/{Name}@{Version} to {Downloads} downloads",
						author,
						name,
						version,
						metadata.Downloads
					);
				},
				cancellationToken
			);

			var stream = await storage.OpenReadAsync(author, name, version, cancellationToken);

			HttpContext.Response.RegisterForDisposeAsync(stream);

			var filename = $"{author}-{name}-{version}.zip";

			logger.LogTrace("Starting download of zip archive with filename {Filename}", filename);

			return File(stream, "application/zip", filename);
		}
		catch (Exception e)
		{
			logger.LogError(
				e,
				"Error when trying to load version {Author}/{Name}@{Version} from storage",
				author,
				name,
				version
			);

			return StatusCode(StatusCodes.Status500InternalServerError);
		}
	}

	[Authorize]
	[HttpPost("{version}")]
	[ProducesResponseType(typeof(NewVersionResult), StatusCodes.Status403Forbidden)]
	[ProducesResponseType(typeof(NewVersionResult), StatusCodes.Status411LengthRequired)]
	[ProducesResponseType(typeof(NewVersionResult), StatusCodes.Status413PayloadTooLarge)]
	[ProducesResponseType(typeof(NewVersionResult), StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(typeof(NewVersionResult), StatusCodes.Status200OK)]
	public async Task<IActionResult> NewVersion(string author, string name, string version, CancellationToken cancellationToken = default)
	{
		logger.LogTrace("Incoming upload request for {Author}/{Name} with version {Version}", author, name, version);

		if (!User.HasClaim(c => c.Type == TokenGenerator.IdClaim))
		{
			logger.LogTrace("Upload request rejected because of a missing ID claim");

			return Unauthorized(NewVersionResult.Unauthorized("Missing ID claim."));
		}

		var authorIdStr = User.FindFirstValue(TokenGenerator.IdClaim);
		if (authorIdStr is null || !Guid.TryParse(authorIdStr, out var authorId))
		{
			logger.LogTrace("Upload request rejected because of an invalid ID claim (ID: '{Id}')", authorIdStr);

			return Unauthorized(NewVersionResult.Unauthorized("Invalid ID claim."));
		}

		var uploader = await context.Authors
			.Include(a => a.Libraries)
			.FirstOrDefaultAsync(a => a.Id == authorId, cancellationToken);

		if (uploader is null)
		{
			logger.LogTrace("Upload request rejected because no user with ID {Id} could be found", authorId);

			return Unauthorized(NewVersionResult.Unauthorized("User not found."));
		}

		logger.LogTrace("Found uploader {Uploader}", uploader);

		var library = await context.Libraries
			.Include(l => l.LatestVersion)
			.Include(l => l.Versions)
			.Include(l => l.Maintainers)
			.FirstOrDefaultAsync(l => l.Author == author && l.Name == name, cancellationToken);

		if (library is null)
		{
			logger.LogTrace(
				"No library with the name {Author}/{Name} could be found. Trying to create it",
				author,
				name
			);

			if (uploader.Username != author)
			{
				logger.LogInformation(
					"Upload request rejected because the uploader username ({Uploader}) doesn't match the library author name ('{Author}')",
					uploader,
					author
				);

				return Unauthorized(NewVersionResult.Unauthorized("Cannot create a library for another author."));
			}

			library = new()
			{
				Name = name,
				Author = author,
				Maintainers = new List<Author>
				{
					uploader,
				},
				Versions = new List<LibraryVersion>(),
			};

			await context.Libraries.AddAsync(library, cancellationToken);
			uploader.Libraries.Add(library);

			await context.SaveChangesAsync(cancellationToken);

			logger.LogInformation("Created a new library {Library} for uploader {Uploader}", library, uploader);
		}
		else if (!library.Maintainers.Contains(uploader))
		{
			logger.LogWarning(
				"Rejected upload request because the uploader ({Uploader}) is not a maintainer of the library {Library}",
				uploader,
				library
			);

			return StatusCode(StatusCodes.Status403Forbidden, NewVersionResult.NotAMaintainer());
		}
		else
		{
			logger.LogTrace("Found library {Library} and verified uploader {Uploader} as a maintainer", library, uploader);
		}

		SemVersion newVersionSemVer;
		try
		{
			newVersionSemVer = SemVersion.Parse(version, SemVersionStyles.Strict);

			logger.LogTrace(
				"Parsed new library version '{Version}' as semantic version '{SemVersion}'",
				version,
				newVersionSemVer
			);
		}
		catch (FormatException e)
		{
			logger.LogInformation(
				"Failed to parse new library version '{Version}' as semantic version (Error: {Message})",
				version,
				e.Message
			);

			return UnprocessableEntity(NewVersionResult.VersionInvalid(version));
		}

		var existingVersion = library.Versions.FirstOrDefault(v => v.Version == version);
		if (existingVersion is not null)
		{
			logger.LogInformation(
				"Rejected upload request because the new version ({Version}) already exists ({ExistingVersion})",
				newVersionSemVer,
				existingVersion
			);

			return UnprocessableEntity(NewVersionResult.VersionAlreadyExists(version));
		}

		var latestVersion = library.LatestVersion;
		if (latestVersion is not null)
		{
			var latestVersionSemVer = SemVersion.Parse(latestVersion.Version, SemVersionStyles.Strict);

			if (newVersionSemVer.ComparePrecedenceTo(latestVersionSemVer) != 1)
			{
				logger.LogInformation(
					"Rejected upload request because the new version ({NewVersion}) is older or equal to the current latest version {LatestVersion}",
					newVersionSemVer,
					latestVersionSemVer
				);

				return UnprocessableEntity(NewVersionResult.VersionMustBeNewer(version, latestVersion.Version));
			}
		}

		var pendingVersion = new PendingLibraryVersion
		{
			Version = newVersionSemVer.ToString(),
			Library = library,
			Uploader = uploader,
			CreatedAt = DateTimeOffset.UtcNow,
		};

		await context.PendingLibraryVersions.AddAsync(pendingVersion, cancellationToken);

		await context.SaveChangesAsync(cancellationToken);

		var uploadEndpointData = await uploadEndpointGenerator.GenerateUploadEndpointDataAsync(Request, pendingVersion, cancellationToken);

		return Ok(NewVersionResult.Success(uploadEndpointData));
	}
}
