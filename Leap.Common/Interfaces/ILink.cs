using Leap.Common.Extensions;
using Semver;

namespace Leap.Common.Interfaces;

/// <summary>
/// Represents a link between two libraries.
/// </summary>
public interface ILink
{
	/// <summary>
	/// A <see cref="ILink"/>'s nature (the type of relationship it represents).
	/// </summary>
	enum LinkNature
	{
		/// <summary>
		/// This <see cref="ILink"/> represents a dependency from <see cref="ILink.Source"/> on <see cref="ILink.Target"/>.
		/// </summary>
		Requires,

		/// <summary>
		/// This <see cref="ILink"/> represents a conflict between <see cref="ILink.Source"/> and <see cref="ILink.Target"/>.
		/// </summary>
		Conflicts,
	}

	/// <summary>
	/// The source library name.
	/// </summary>
	string Source { get; }

	/// <summary>
	/// The target library name.
	/// </summary>
	string Target { get; }

	/// <summary>
	/// Represents the version constraint imposed by this link.
	/// </summary>
	SemVersionRange Constraint { get; }

	/// <summary>
	/// The nature of this link.
	/// </summary>
	/// <seealso cref="ILink.LinkNature"/>
	LinkNature Nature { get; }

	/// <summary>
	/// Returns a string representation of this <see cref="Nature"/>.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	string NatureDescription => Nature.ToDescription();
}
