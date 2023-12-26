namespace Leap.Common.DTO.API;

public abstract record DefaultResult(string Code, string Message, IEnumerable<string>? Errors)
{
	public const string CodeSuccess = "success";
	public const string CodeInvalidRequest = "invalid_request";
	public const string CodeUnauthorized = "unauthorized";
}
