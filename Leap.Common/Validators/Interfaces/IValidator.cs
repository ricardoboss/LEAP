namespace Leap.Common.Validators.Interfaces;

public interface IValidator<in T>
{
	public bool IsValid(T obj)
	{
		using var errorEnumerator = GetErrors(obj).GetEnumerator();

		return errorEnumerator.MoveNext();
	}

	public IEnumerable<ValidationError> GetErrors(T obj);
}
