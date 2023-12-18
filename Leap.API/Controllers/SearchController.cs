using Leap.API.DB;
using Leap.API.Extensions;
using Leap.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Leap.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SearchController(LeapApiDbContext context, LinkGenerator linkGenerator) : LibraryControllerBase
{
	[HttpGet]
	[ProducesResponseType(typeof(IEnumerable<ISparseLibraryVersion>), StatusCodes.Status200OK)]
	public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
	{
		var libraries = await context.Libraries
			.Include(library => library.LatestVersion)
			.ThenInclude(v => v!.Links)
			.Include(library => library.Versions)
			.ThenInclude(v => v.Links)
			.ToListAsync(cancellationToken);

		var sparseLibraries = libraries.Select(
			library =>
			{
				var downloadUrl = GetDownloadUrl(linkGenerator, library.Author, library.Name, library.LatestVersion!.Version);

				return library.LatestVersion!.ToSparse(downloadUrl);
			}
		);

		return Ok(sparseLibraries);
	}
}
