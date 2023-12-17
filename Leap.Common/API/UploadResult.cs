namespace Leap.Common.API;

public record UploadResult(string Code, string Message, BriefLibraryVersion? Version = null)
{
	public static UploadResult VersionInvalid(string version)
	{
		return new("version_invalid", $"Version '{version}' is invalid.");
	}

	public static UploadResult VersionAlreadyExists(string version)
	{
		return new("version_already_exists", $"Version '{version}' already exists.");
	}

	public static UploadResult VersionMustBeNewer(string version, string latest)
	{
		return new("version_must_be_newer", $"Version '{version}' must be newer than the latest version '{latest}'.");
	}

	public static UploadResult LengthRequired()
	{
		return new("length_required", $"Content-Length header is required.");
	}

	public static UploadResult TooLarge(long size, long max)
	{
		return new("too_large", $"Payload is too large ({size} bytes). Maximum allowed size is {max} bytes.");
	}

	public static UploadResult LibraryFileMissing()
	{
		return new("library_file_missing", "Library file is missing.");
	}

	public static UploadResult Success(string name, string version, BriefLibraryVersion brief)
	{
		return new("success", $"Version '{version}' of {name} uploaded successfully.", brief);
	}

	public static UploadResult Unauthorized(string message)
	{
		return new("unauthorized", $"Unauthorized. {message}");
	}

	public static UploadResult NotAMaintainer()
	{
		return new("not_a_maintainer", "The current user is not a maintainer of the library.");
	}
}
