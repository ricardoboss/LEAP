using PasswordRulesSharp.Models;
using PasswordRulesSharp.Validator.Requirements;

namespace Leap.Common.Extensions;

internal static class PasswordRulesSharpExtensions
{
	public static string GetFailedMessage(this Requirement requirement)
	{
		switch (requirement)
		{
			case MinimumLengthRequirement minimumLength:
				return $"Password must be at least {minimumLength.Length} characters long.";

			case MaximumLengthRequirement maximumLength:
				return $"Password must be at most {maximumLength.Length} characters long.";

			case MaxConsecutiveRequirement maxConsecutive:
				return $"Password must not contain more than {maxConsecutive.Value} consecutive characters.";

			case CharacterClassRequirement requiredChars:
				if (requiredChars.CharacterClass == CharacterClass.Upper)
					return "Password must contain at least one uppercase character.";

				if (requiredChars.CharacterClass == CharacterClass.Lower)
					return "Password must contain at least one lowercase character.";

				if (requiredChars.CharacterClass == CharacterClass.AsciiPrintable)
					return "Password must contain at least one ASCII printable character.";

				if (requiredChars.CharacterClass == CharacterClass.Digit)
					return "Password must contain at least one digit.";

				if (requiredChars.CharacterClass == CharacterClass.Unicode)
					return "Password must contain at least one character.";

				if (requiredChars.CharacterClass is SpecificCharacterClass specific)
					return $"Password must contain at least one of {string.Join(' ', specific.Chars)}.";

				return "Password does not meet requirements.";

			default:
				throw new NotSupportedException("Unknown requirement type: " + requirement.Type);
		}
	}
}
