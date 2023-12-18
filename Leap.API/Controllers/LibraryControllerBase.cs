using Microsoft.AspNetCore.Mvc;

namespace Leap.API.Controllers;

public class LibraryControllerBase : ControllerBase
{
	[NonAction]
	protected string GetDownloadUrl(LinkGenerator linkGenerator, string author, string name, string version)
	{
		return linkGenerator.GetPathByAction(
			       "Download",
			       "Libraries",
			       new
			       {
				       author,
				       name,
				       version,
			       }
		       ) ??
		       throw new("Failed to generate download URL.");
	}
}
