using Microsoft.AspNetCore.Http.Extensions;
using SignedUrl.Abstractions;

namespace Leap.API.Extensions;

public static class UrlSignerExtensions
{
	public static bool Validate(this IUrlSigner signer, HttpRequest request)
	{
		var url = request.GetEncodedUrl();

		return signer.Validate(url);
	}
}
