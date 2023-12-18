namespace Leap.Common.DTO.API;

public record ReserveLibraryNameResult(string Code, string Message, IEnumerable<string>? Errors = null) : DefaultResult(Code, Message, Errors)
{
	public static ReserveLibraryNameResult Success(string name)
	{
		return new(CodeSuccess, $"Library name '{name}' is available.");
	}

	public static ReserveLibraryNameResult InvalidName(string name)
	{
		return new("invalid_name", $"Library name '{name}' is invalid.");
	}

	public static ReserveLibraryNameResult InvalidLibraryName(string name)
	{
		return new("invalid_library", $"The library part of the library name '{name}' is invalid.");
	}

	public static ReserveLibraryNameResult AlreadyExists(string name)
	{
		return new("already_exists", $"Library name '{name}' already exists.");
	}

	public static ReserveLibraryNameResult InvalidAuthor(string name)
	{
		return new("invalid_author", $"The author part of the library name '{name}' is invalid.");
	}

	public static ReserveLibraryNameResult Unauthorized(string message)
	{
		return new(CodeUnauthorized, $"Unauthorized. {message}");
	}

	public static ReserveLibraryNameResult InvalidRequest(IEnumerable<string> errors)
	{
		return new(CodeInvalidRequest, "There were errors with your request.", errors);
	}
}
