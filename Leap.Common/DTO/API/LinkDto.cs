using Leap.Common.Extensions;
using Leap.Common.Interfaces;

namespace Leap.Common.DTO.API;

public class LinkDto
{
	public string Source { get; init; }

	public string SourceVersion { get; init; }

	public string Target { get; init; }

	public string Constraint { get; init; }

	public ILink.LinkNature Nature { get; init; }

	public override string ToString()
	{
		return $"{Source}@{SourceVersion} {Nature.ToDescription()} {Target} ({Constraint})";
	}
}
