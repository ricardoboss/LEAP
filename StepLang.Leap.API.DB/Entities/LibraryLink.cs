using Leap.Common.Interfaces;

namespace Leap.API.DB.Entities;

public class LibraryLink
{
	public Guid SourceId { get; set; }

	public LibraryVersion Source { get; set; } = null!;

	public Guid TargetId { get; set; }

	public Library Target { get; set; } = null!;

	public string VersionRange { get; set; } = null!;

	public ILink.LinkNature Nature { get; set; }

	/// <inheritdoc />
	public override string ToString()
	{
		return $"LibraryLink(Source={SourceId}, Target={TargetId}, Nature={Nature}, VersionRange={VersionRange})";
	}
}
