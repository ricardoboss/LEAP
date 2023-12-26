namespace Leap.Common.DTO.API;

public record NewVersionResult(string Code, string Message, UploadEndpointData? Data = null) : DefaultResult(Code, Message, null)
{
	public static NewVersionResult VersionInvalid(string version)
	{
		return new("version_invalid", $"Version '{version}' is invalid.");
	}

	public static NewVersionResult VersionAlreadyExists(string version)
	{
		return new("version_already_exists", $"Version '{version}' already exists.");
	}

	public static NewVersionResult VersionMustBeNewer(string version, string latest)
	{
		return new("version_must_be_newer", $"Version '{version}' must be newer than the latest version '{latest}'.");
	}

	public static NewVersionResult Success(UploadEndpointData uploadData)
	{
		return new(CodeSuccess, $"Version can now be uploaded to {uploadData.Url}.", uploadData);
	}

	public static NewVersionResult Unauthorized(string message)
	{
		return new(CodeUnauthorized, $"Unauthorized. {message}");
	}

	public static NewVersionResult NotAMaintainer()
	{
		return new("not_a_maintainer", "The current user is not a maintainer of the library.");
	}
}
