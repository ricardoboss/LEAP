using Leap.Common.Extensions;
using Leap.Common.Interfaces;

namespace Leap.Common.DTO.API;

public record LinkDto(string Source, string SourceVersion, string Target, string Constraint, ILink.LinkNature Nature)
{
	public override string ToString()
	{
		return $"{Source}@{SourceVersion} {Nature.ToDescription()} {Target} ({Constraint})";
	}
}
