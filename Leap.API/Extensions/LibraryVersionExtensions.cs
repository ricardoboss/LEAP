using System.Collections.Frozen;
using Leap.API.DB.Entities;
using Leap.Common.DTO.API;

namespace Leap.API.Extensions;

public static class LibraryVersionExtensions
{
	public static SparseLibraryVersionDto ToSparse(this LibraryVersion version, string? downloadUrl = null)
	{
		return new()
		{
			Author = version.Library.Author,
			Name = version.Library.Name,
			Version = version.Version,
			Links = version.Links.ToFrozenDictionary(l => l.Target.Name, l => l.ToLink()),
			DistUrl = downloadUrl,
			ReleaseDate = version.ReleaseDate,
		};
	}
}
