namespace Leap.Common.DTO.API;

public record UploadResult(string Code, string Message, SparseLibraryVersionDto? Version = null) : DefaultResult(Code, Message, null)
{
	public static UploadResult Success(SparseLibraryVersionDto libraryVersion)
	{
		return new(CodeSuccess, "New library version successfully uploaded", libraryVersion);
	}

	public static UploadResult InvalidSignature()
	{
		return new("invalid_signature", "Invalid query signature");
	}

	public static UploadResult PendingVersionNotFound()
	{
		return new("pending_version_not_found", "No pending version with the given pid was not found");
	}

	public static UploadResult MissingLibraryArchive()
	{
		return new("missing_library_archive", "The library archive is missing.");
	}

	public static UploadResult LengthRequired()
	{
		return new("length_required", "Content-Length header is required.");
	}

	public static UploadResult NoContent()
	{
		return new("no_content", "No content was provided.");
	}

	public static UploadResult TooLarge(long size, long max)
	{
		return new("too_large", $"Payload is too large ({size} bytes). Maximum allowed size is {max} bytes.");
	}
}
