namespace Leap.Common.Interfaces;

/// <summary>
/// Defines the essential information a library has that is used for search results.
/// </summary>
public interface ISparseLibrary
{
	/// <summary>
	/// The normalized name of this library.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// The normalized name of the author of this library.
	/// </summary>
	string Author { get; }

	/// <summary>
	/// The name of this library that is displayed to the user.
	/// </summary>
	string DisplayName => $"{Author}/{Name}";

	/// <summary>
	/// A string representing the latest version of this library.
	/// </summary>
	string? LatestVersionName { get; }
}
