using System.Text.RegularExpressions;
using Leap.Common.DTO.API;
using Leap.Common.Validators.Interfaces;

namespace Leap.Common.Validators;

public partial class ReserveLibraryNameRequestValidator : IValidator<ReserveLibraryNameRequest>
{
	[GeneratedRegex("^[a-z][-a-z]{0,16}[a-z]$")]
	private static partial Regex UsernameRegex();

	[GeneratedRegex("^[a-z][-a-z]{0,16}[a-z]$")]
	private static partial Regex LibraryNameRegex();

	public static bool IsUsernameValid(string username)
	{
		return UsernameRegex().IsMatch(username);
	}

	public static bool IsLibraryNameValid(string name)
	{
		return LibraryNameRegex().IsMatch(name);
	}

	public IEnumerable<ValidationError> GetErrors(ReserveLibraryNameRequest obj)
	{
		var parts = obj.Name.Split('/');
		if (parts.Length != 2)
		{
			yield return new("Library name must match '<author>/<library>' pattern");
			yield break;
		}

		var (author, name) = (parts[0], parts[1]);

		if (!IsUsernameValid(author))
			yield return new(
				"Author must only contain lowercase characters and hyphens (-) and must start and end with a character."
			);

		if (!IsLibraryNameValid(name))
			yield return new(
				"Library name must only contain lowercase characters and hyphens (-) and must start and end with a character"
			);
	}
}
