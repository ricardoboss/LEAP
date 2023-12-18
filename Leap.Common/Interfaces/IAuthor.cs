namespace Leap.Common.Interfaces;

/// <summary>
/// Represents an author of a library.
/// </summary>
public interface IAuthor
{
	/// <summary>
	/// The author's username.
	/// </summary>
	string UserName { get; }

	/// <summary>
	/// An optional URL to the author's website.
	/// </summary>
	string? Homepage { get; }

	/// <summary>
	/// An optional email address to contact the author.
	/// </summary>
	string? Email { get; }
}
