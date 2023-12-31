using System.Net.Http.Json;
using System.Text.Json;
using Leap.Common.DTO.API;

namespace Leap.Client;

public class LeapApiClient(HttpClient client)
{
	public async Task<SparseLibraryVersionDto?> GetLibraryAsync(
		string author,
		string name,
		string? version = null,
		CancellationToken cancellationToken = default
	)
	{
		var uri = $"libraries/{author}/{name}";
		if (version is not null)
			uri += $"/{version}";

		try
		{
			return await GetJsonAsync<SparseLibraryVersionDto?>(uri, cancellationToken);
		}
		catch (LeapApiException)
		{
			return null;
		}
	}

	public Task<NewVersionResult?> UploadLibraryAsync(
		string author,
		string name,
		string version,
		Stream stream,
		CancellationToken cancellationToken = default
	)
	{
		var uri = $"libraries/{author}/{name}/{version}";

		return PostStreamAsync<NewVersionResult>(uri, stream, cancellationToken);
	}

	public Task<RegisterResult?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
	{
		return PostJsonAsync<RegisterResult, RegisterRequest>("auth/register", request, cancellationToken);
	}

	public Task<CreateTokenResult?> CreateTokenAsync(CreateTokenRequest request, CancellationToken cancellationToken = default)
	{
		return PostJsonAsync<CreateTokenResult, CreateTokenRequest>("auth/token", request, cancellationToken);
	}

	public async Task<AuthCheckResult?> CheckAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			return await GetJsonAsync<AuthCheckResult>("auth/check", cancellationToken);
		}
		catch (LeapApiException e)
		{
			return new("unknown", e.Message);
		}
	}

	private Task<T?> GetJsonAsync<T>(string uri, CancellationToken cancellationToken = default)
	{
		return WrapAsync(
			async () =>
			{
				var response = await client.GetAsync(uri, cancellationToken);

				response.EnsureSuccessStatusCode();

				return await response.Content.ReadFromJsonAsync<T>(cancellationToken);
			},
			cancellationToken
		);
	}

	private Task<TResponse?> PostJsonAsync<TResponse, TRequest>(string uri, TRequest? content, CancellationToken cancellationToken = default)
	{
		return WrapAsync(
			async () =>
			{
				var response = await client.PostAsJsonAsync(uri, content, cancellationToken);

				response.EnsureSuccessStatusCode();

				return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken);
			},
			cancellationToken
		);
	}

	private Task<T?> PostStreamAsync<T>(string uri, Stream stream, CancellationToken cancellationToken = default)
	{
		return WrapAsync(
			async () =>
			{
				var response =
					await client.PostAsync(uri, new StreamContent(stream), cancellationToken);

				response.EnsureSuccessStatusCode();

				var body = await response.Content.ReadFromJsonAsync<T>(cancellationToken);

				return body;
			},
			cancellationToken
		);
	}

	private static async Task<T> WrapAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
	{
		try
		{
			cancellationToken.ThrowIfCancellationRequested();

			return await func();
		}
		catch (HttpRequestException e)
		{
			throw new LeapApiException($"Request failed with status {e.StatusCode}", e);
		}
		catch (JsonException e)
		{
			throw new LeapApiException("Failed to parse response.", e);
		}
		catch (Exception e)
		{
			throw new LeapApiException("An unknown error occurred.", e);
		}
	}
}
