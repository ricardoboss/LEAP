using System.Text.Json.Nodes;

namespace Leap.Common.Interfaces;

/// <summary>
/// Defines library metadata that is not necessary for solving/installation.
/// </summary>
public interface ICompleteLibraryVersion : ISparseLibraryVersion
{
	/// <summary>
	/// The license of the library version.
	/// </summary>
	/// <returns>A string representing the license of the library (e.g. MIT).</returns>
	string? License { get; }

	/// <summary>
	/// A list of keywords that can be used to search for the library.
	/// </summary>
	IEnumerable<string>? Keywords { get; }

	/// <summary>
	/// A description of the library.
	/// </summary>
	string? Description { get; }

	/// <summary>
	/// Returns the URL of the library's repository, if any.
	/// </summary>
	string? Repository { get; }

	/// <summary>
	/// A list of authors of the library.
	/// </summary>
	IEnumerable<IAuthor>? Authors { get; }

	/// <summary>
	/// Stores any extra data that is not part of the library's metadata.
	/// </summary>
	JsonObject? Extra { get; }
}
