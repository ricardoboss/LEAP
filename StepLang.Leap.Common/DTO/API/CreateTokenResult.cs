namespace Leap.Common.DTO.API;

public record CreateTokenResult(string Code, string Message, string? Token, IEnumerable<string>? Errors = null) : DefaultResult(Code, Message, Errors)
{
	public static CreateTokenResult Unauthorized()
	{
		return new("invalid_credentials", "Invalid username or password", null);
	}

	public static CreateTokenResult InvalidRequest(IEnumerable<string> errors)
	{
		return new(CodeInvalidRequest, "There were errors with your request.", null, errors);
	}

	public static CreateTokenResult Success(string token)
	{
		return new(CodeSuccess, "Token created successfully", token);
	}
}
