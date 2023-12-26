namespace Leap.API.DB.Entities;

public class LibraryVersion
{
	public Guid Id { get; set; }

	public Guid LibraryId { get; set; }

	public Library Library { get; set; } = null!;

	public string Version { get; set; } = null!;

	public DateTimeOffset ReleaseDate { get; set; }

	public ICollection<LibraryLink> Links { get; set; } = null!;

	/// <inheritdoc />
	public override string ToString()
	{
		return $"LibraryVersion(Id={Id}, Library={Library.Author}/{Library.Name}, Version={Version})";
	}
}
