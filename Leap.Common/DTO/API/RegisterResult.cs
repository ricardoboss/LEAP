namespace Leap.Common.DTO.API;

public record RegisterResult
	(string Code, string Message, string? Token = null, IEnumerable<string>? Errors = null) : DefaultResult(Code, Message, Errors)
{
	public static RegisterResult UsernameExists()
	{
		return new("username_exists", "Username already exists");
	}

	public static RegisterResult InvalidRequest(IEnumerable<string> errors)
	{
		return new(CodeInvalidRequest, "There were errors with your request.", null, errors);
	}

	public static RegisterResult Success(string token)
	{
		return new(CodeSuccess, "User created successfully", token);
	}
}
