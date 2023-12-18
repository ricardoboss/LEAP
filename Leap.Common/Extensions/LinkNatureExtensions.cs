using Leap.Common.Interfaces;

namespace Leap.Common.Extensions;

public static class LinkNatureExtensions
{
	public static string ToDescription(this ILink.LinkNature nature)
	{
		return nature switch
		{
			ILink.LinkNature.Requires => "requires",
			ILink.LinkNature.Conflicts => "conflicts with",
			_ => throw new ArgumentOutOfRangeException(nameof(nature), nature, null),
		};
	}
}
