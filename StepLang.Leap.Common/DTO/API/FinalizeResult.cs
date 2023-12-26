namespace Leap.Common.DTO.API;

public record FinalizeResult(string Code, string Message, SparseLibraryVersionDto? Version = null) : DefaultResult(Code, Message, null)
{
	public static FinalizeResult Success(SparseLibraryVersionDto version)
	{
		return new(CodeSuccess, $"Library version {version} successfully finalized.", version);
	}

	public static UploadResult InvalidSignature()
	{
		return new("invalid_signature", "Invalid query signature");
	}

	public static UploadResult PendingVersionNotFound()
	{
		return new("pending_version_not_found", "Pending version not found");
	}
}
