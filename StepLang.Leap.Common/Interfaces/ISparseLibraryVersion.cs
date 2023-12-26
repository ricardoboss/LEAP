using Semver;

namespace Leap.Common.Interfaces;

/// <summary>
/// Defines the essential information a library's version has that is used during solving/installation.
/// </summary>
public interface ISparseLibraryVersion
{
	/// <summary>
	/// The normalized name of a library without version information.
	/// </summary>
	string LibraryName { get; }

	/// <summary>
	/// The library's pretty name, i.e. the name that is displayed to the user.
	/// </summary>
	string LibraryDisplayName => LibraryName;

	/// <summary>
	/// The library's version.
	/// </summary>
	SemVersion Version { get; }

	/// <summary>
	/// The library's version as a string that is displayed to the user.
	/// </summary>
	string DisplayVersion => Version.ToString();

	/// <summary>
	/// Returns the library's unique name, constructed from <see cref="LibraryName"/> and <see cref="Version"/> that is used to identify this library
	/// version and can be used to identify, sort, compare and hash this library version.
	/// </summary>
	string UniqueName => $"{LibraryName}@{Version}";

	/// <summary>
	/// This version's release date, if any.
	/// </summary>
	DateTimeOffset? ReleaseDate { get; }

	/// <summary>
	/// Returns a set of names that could refer to this library.
	/// </summary>
	/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>s each representing this library or null if no aliases exist.</returns>
	IEnumerable<string>? Aliases { get; }

	/// <summary>
	/// Returns the URL of the library's distribution archive of this version.
	/// </summary>
	string? DistUrl { get; }

	/// <summary>
	/// Returns a list of <see cref="ILink"/>s related to this library.
	/// </summary>
	IReadOnlyDictionary<string, ILink> Links { get; }

	/// <summary>
	/// Returns a set of links to libraries with the specified nature.
	/// </summary>
	/// <param name="nature">The nature of the links to return.</param>
	/// <returns>
	/// A dictionary of <see cref="string"/>s representing the name of the library and <see cref="ILink"/>s representing the link.
	/// </returns>
	IReadOnlyDictionary<string, ILink> GetLinks(ILink.LinkNature nature)
	{
		return Links.Where(l => l.Value.Nature == nature).ToDictionary(l => l.Key, l => l.Value);
	}

	/// <summary>
	/// Returns a set of links to libraries that this library requires and need to be installed before this library can be installed.
	/// </summary>
	/// <returns>
	/// A dictionary of <see cref="string"/>s representing the name of the library and <see cref="ILink"/>s representing the link.
	/// </returns>
	IReadOnlyDictionary<string, ILink> Requires => GetLinks(ILink.LinkNature.Requires);

	/// <summary>
	/// Returns a set of links to libraries that this library conflicts with which must not be installed at the same time as this library.
	/// </summary>
	/// <returns>
	/// A dictionary of <see cref="string"/>s representing the name of the library and <see cref="ILink"/>s representing the link.
	/// </returns>
	IReadOnlyDictionary<string, ILink> Conflicts => GetLinks(ILink.LinkNature.Conflicts);
}
