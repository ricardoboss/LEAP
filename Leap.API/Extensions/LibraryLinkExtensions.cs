using Leap.API.DB.Entities;
using Leap.Common.DTO.API;

namespace Leap.API.Extensions;

public static class LibraryLinkExtensions
{
	public static LinkDto ToLink(this LibraryLink link)
	{
		return new(
			Source: link.Source.Library.Name,
			SourceVersion: link.Source.Version,
			Target: link.Target.Name,
			Constraint: link.VersionRange,
			Nature: link.Nature
		);
	}
}
