using Leap.API.DB;
using Leap.API.DB.Entities;
using Leap.API.Extensions;
using Leap.API.Interfaces;
using Leap.Common.DTO.API;
using Microsoft.AspNetCore.Mvc;
using SignedUrl.Abstractions;
using SignedUrl.AspNet;

namespace Leap.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class StorageController
	(ILogger<StorageController> logger, LeapApiDbContext context, IQuerySigner signer, ILibraryStorage storage, LinkGenerator linkGenerator)
	: LibraryControllerBase, IUploadEndpointGenerator
{
	[NonAction]
	public Task<UploadEndpointData> GenerateUploadEndpointDataAsync(
		HttpRequest request,
		PendingLibraryVersion pendingVersion,
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var endpoint = linkGenerator.GetPathByAction("Upload", "Storage") ??
		               throw new InvalidOperationException("Failed to generate upload endpoint URL");

		var query = new Dictionary<string, string?>
		{
			{ "pid", pendingVersion.Id.ToString() },
		};

		var url = signer.GenerateFullyQualified(request, endpoint, query, true);

		return Task.FromResult(new UploadEndpointData(url));
	}

	[HttpPost]
	[ProducesResponseType(typeof(UploadResult), StatusCodes.Status204NoContent)]
	[ProducesResponseType(typeof(UploadResult), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(UploadResult), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(UploadResult), StatusCodes.Status411LengthRequired)]
	[ProducesResponseType(typeof(UploadResult), StatusCodes.Status413PayloadTooLarge)]
	[ProducesResponseType(typeof(UploadResult), StatusCodes.Status422UnprocessableEntity)]
	public async Task<IActionResult> Upload(
		[FromQuery] Guid pid,
		[FromQuery] string s,
		[FromForm] UploadArchiveForm? form,
		CancellationToken cancellationToken = default
	)
	{
		if (!signer.ValidateSignature(Request.QueryString.ToUriComponent()))
			return BadRequest(UploadResult.InvalidSignature());

		var pendingVersion = await context.PendingLibraryVersions.FindAsync(new object?[] { pid }, cancellationToken);
		if (pendingVersion is null)
			return NotFound(UploadResult.PendingVersionNotFound());

		const long maxUploadSize = 5_000_000; // 5 MB

		var size = Request.Headers.ContentLength;
		switch (size)
		{
			case null:
				return StatusCode(StatusCodes.Status411LengthRequired, UploadResult.LengthRequired());

			case > maxUploadSize:
				return StatusCode(StatusCodes.Status413PayloadTooLarge, UploadResult.TooLarge(size.Value, maxUploadSize));

			case <= 0:
				return UnprocessableEntity(UploadResult.NoContent());
		}

		var libraryArchive = form?.Archive;
		if (libraryArchive is null)
			return UnprocessableEntity(UploadResult.MissingLibraryArchive());

		logger.LogTrace("Validated upload request for {PendingVersion}", pendingVersion);

		await using (var storageArchive = storage.OpenWrite(
			             pendingVersion.Library.Author,
			             pendingVersion.Library.Name,
			             pendingVersion.Version,
			             cancellationToken
		             ))
		{
			// FIXME: check actual length before writing to storage as bad actors could fake the content-length header and flood the storage if unchecked
			await libraryArchive.CopyToAsync(storageArchive, cancellationToken);
		}

		var finalizeEndpoint = Url.ActionLink("Finalize", "Storage") ??
		                       throw new InvalidOperationException("Failed to generate finalize endpoint URL");

		var finalizeUrl = signer.GenerateFullyQualified(
			Request,
			finalizeEndpoint,
			new Dictionary<string, string?>
			{
				{ "pid", pendingVersion.Id.ToString() },
			},
			true
		);

		logger.LogTrace("Generated finalize url for archive {PendingId} to {FinalizeUrl}", pendingVersion.Id, finalizeUrl);

		Response.Headers.Location = finalizeUrl;
		return NoContent();
	}

	[HttpGet]
	[ProducesResponseType(typeof(FinalizeResult), StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(FinalizeResult), StatusCodes.Status424FailedDependency)]
	public async Task<IActionResult> Finalize([FromQuery] Guid pid, [FromQuery] string s, CancellationToken cancellationToken = default)
	{
		if (!signer.ValidateSignature(Request.QueryString.ToUriComponent()))
			return BadRequest(FinalizeResult.InvalidSignature());

		var pendingVersion = await context.PendingLibraryVersions.FindAsync(new object?[] { pid }, cancellationToken);
		if (pendingVersion is null)
			return StatusCode(StatusCodes.Status424FailedDependency, FinalizeResult.PendingVersionNotFound());

		// TODO: make sure the pending version is still newer than the latest version

		var libraryVersion = new LibraryVersion
		{
			Version = pendingVersion.Version,
			Library = pendingVersion.Library,
			ReleaseDate = DateTimeOffset.UtcNow,
			Links = new List<LibraryLink>(),
		};

		// TODO: read the library's metadata from the archive and store it in the database (like dependencies)

		// add this new version to the global list of versions
		await context.LibraryVersions.AddAsync(libraryVersion, cancellationToken);

		// update the library's latest version
		pendingVersion.Library.LatestVersion = libraryVersion;

		// remove the pending version
		context.PendingLibraryVersions.Remove(pendingVersion);

		await context.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Updated latest version of {Library} to {Version}", libraryVersion.Library, libraryVersion);
		logger.LogDebug("Removed {PendingVersion} to finalize {LibraryVersion}", pendingVersion, libraryVersion);

		var downloadUrl = GetDownloadUrl(linkGenerator, libraryVersion.Library.Author, libraryVersion.Library.Name, libraryVersion.Version);

		return Ok(FinalizeResult.Success(libraryVersion.ToSparse(downloadUrl)));
	}
}
