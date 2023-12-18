using PasswordRulesSharp.Builder;
using PasswordRulesSharp.Models;
using PasswordRulesSharp.Rules;
using PasswordRulesSharp.Validator;
using PasswordRulesSharp.Validator.Requirements;

namespace Leap.Common.DTO.API;

public record RegisterRequest(string Username, string Password);
