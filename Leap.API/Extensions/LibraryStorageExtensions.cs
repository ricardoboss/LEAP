using Leap.API.Interfaces;
using Leap.API.Services;

namespace Leap.API.Extensions;

public static class LibraryStorageExtensions
{
	public static async Task UpdateMetadataAsync(this ILibraryStorage storage, string author, string name,
		string version, Action<StorageMetadata> callback, CancellationToken cancellationToken = default)
	{
		StorageMetadata metadata = await storage.GetMetadataAsync(author, name, version, cancellationToken) ?? new();
		callback(metadata);
		await storage.SetMetadataAsync(author, name, version, metadata, cancellationToken);
	}
}
