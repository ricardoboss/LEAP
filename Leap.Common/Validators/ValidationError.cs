namespace Leap.Common.Validators;

public class ValidationError(string message)
{
	public override string ToString()
	{
		return message;
	}
}
