using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;

namespace Leap.Client;

public class LeapApiClientFactory(LeapApiCredentialManager? manager, IConfiguration configuration)
	: ITypedHttpClientFactory<LeapApiClient>
{
	private string? GetConfiguredBaseAddress()
	{
		return configuration["LeapApi:BaseAddress"];
	}

	public LeapApiClient CreateClient(HttpClient httpClient)
	{
		Credentials? credentials = manager?.TryReadCredentials();

		httpClient.BaseAddress =
			new(GetConfiguredBaseAddress() ?? credentials?.BaseAddress ?? Credentials.DefaultApiBaseAddress);

		if (credentials?.Token is { } token)
			httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

		return new(httpClient);
	}
}
