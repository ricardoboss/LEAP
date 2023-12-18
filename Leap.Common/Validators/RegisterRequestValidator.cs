using Leap.Common.DTO.API;
using Leap.Common.Extensions;
using Leap.Common.Validators.Interfaces;
using PasswordRulesSharp.Builder;
using PasswordRulesSharp.Rules;
using PasswordRulesSharp.Validator;

namespace Leap.Common.Validators;

public class RegisterRequestValidator : IValidator<RegisterRequest>
{
	private static Lazy<IRule> PasswordRule { get; } = new(
		() =>
			new RuleBuilder()
				.RequireLower()
				.RequireUpper()
				.RequireDigit()
				.MinLength(6)
				.MaxConsecutive(3)
				.Build()
	);

	private static Lazy<Validator> PasswordValidator { get; } = new(() => new(PasswordRule.Value));

	public IEnumerable<ValidationError> GetErrors(RegisterRequest request)
	{
		if (!ReserveLibraryNameRequestValidator.IsUsernameValid(request.Username))
			yield return new(
				"User name must only contain lowercase characters and hyphens (-) and must start and end with a character with a minimum length of 2 characters."
			);

		if (PasswordValidator.Value.PasswordIsValid(request.Password, out var requirements))
			yield break;

		foreach (var requirement in requirements.Where(t => !t.Success).Select(t => t.Requirement))
			yield return new(requirement.GetFailedMessage());
	}
}
