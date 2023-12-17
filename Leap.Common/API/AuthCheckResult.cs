namespace Leap.Common.API;

public record AuthCheckResult(string Code, string Message)
{
	public static AuthCheckResult Success(string username)
	{
		return new("success", $"Successfully authenticated as {username}.");
	}

	public static AuthCheckResult NoIdClaim()
	{
		return new("no_id_claim", "No id claim was found.");
	}

	public static AuthCheckResult InvalidIdClaim()
	{
		return new("invalid_id_claim", "Invalid id claim.");
	}

	public static AuthCheckResult NoAuthor()
	{
		return new("no_author", "No author was found.");
	}
}
