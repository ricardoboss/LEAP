namespace Leap.API.DB.Entities;

public class PendingLibraryVersion
{
	public Guid Id { get; set; }

	public Guid LibraryId { get; set; }

	public Library Library { get; set; } = null!;

	public string Version { get; set; } = null!;

	public Guid UploaderId { get; set; }

	public Author Uploader { get; set; } = null!;

	public DateTimeOffset CreatedAt { get; set; }

	public override string ToString()
	{
		return
			$"PendingLibraryVersion(Id={Id}, Library={Library.Author}/{Library.Name}, Version={Version}, Uploader={Uploader.Username}, CreatedAt={CreatedAt})";
	}
}
