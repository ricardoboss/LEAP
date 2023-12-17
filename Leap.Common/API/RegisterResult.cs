namespace Leap.Common.API;

public record RegisterResult(string Code, string Message, string? Token)
{
	public static RegisterResult UsernameExists()
	{
		return new("username_exists", "Username already exists", null);
	}

	public static RegisterResult Invalid(string message)
	{
		return new("invalid", message, null);
	}

	public static RegisterResult Success(string token)
	{
		return new("success", "User created successfully", token);
	}
}
