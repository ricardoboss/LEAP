namespace Leap.Common.API;

public record CreateTokenResult(string Code, string Message, string? Token)
{
	public static CreateTokenResult Unauthorized()
	{
		return new("invalid_credentials", "Invalid username or password", null);
	}

	public static CreateTokenResult Success(string token)
	{
		return new("success", "Token created successfully", token);
	}
}
