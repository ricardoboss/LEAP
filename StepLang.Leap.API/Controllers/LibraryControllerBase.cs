﻿using Microsoft.AspNetCore.Mvc;

namespace Leap.API.Controllers;

public class LibraryControllerBase : ControllerBase
{
	/// <summary>
	/// This property is always <c>null</c> at runtime.
	/// </summary>
	protected new IUrlHelper? Url { get; } = null;

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
