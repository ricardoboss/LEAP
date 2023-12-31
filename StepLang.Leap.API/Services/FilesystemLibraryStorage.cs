using System.Collections.Concurrent;
using System.Text.Json;
using Leap.API.Interfaces;

namespace Leap.API.Services;

public class FilesystemLibraryStorage(IConfiguration configuration, ILogger<FilesystemLibraryStorage> logger)
	: ILibraryStorage
{
	private readonly ConcurrentDictionary<string, DirectoryInfo> libraryVersionDirectoryCache = new();

	private string BasePath =>
		configuration.GetValue<string>("Storage:BasePath") ??
		throw new("Storage:BasePath is not configured.");

	private DirectoryInfo GetLibraryDirectory(string author, string name, bool create = false)
	{
		var libraryDirectory = new DirectoryInfo(Path.Combine(BasePath, author, name));

		if (libraryDirectory.Exists)
			return libraryDirectory;

		if (create)
			libraryDirectory.Create();

		return libraryDirectory;
	}

	private static string GetCacheKey(string author, string name, string version)
	{
		return $"{author}/{name}/{version}";
	}

	private DirectoryInfo GetLibraryVersionDirectory(string author, string name, string version, bool create = false)
	{
		var cacheKey = GetCacheKey(author, name, version);
		if (libraryVersionDirectoryCache.TryGetValue(cacheKey, out var dir))
			return dir;

		var libraryDirectory = GetLibraryDirectory(author, name, create);
		var libraryVersionDirectory = new DirectoryInfo(Path.Combine(libraryDirectory.FullName, version));

		libraryVersionDirectoryCache[cacheKey] = libraryVersionDirectory;

		if (libraryVersionDirectory.Exists)
			return libraryVersionDirectory;

		if (create)
			libraryVersionDirectory.Create();

		return libraryVersionDirectory;
	}

	/// <inheritdoc />
	public Task<bool> ExistsAsync(string author, string name, string version, CancellationToken cancellationToken = default)
	{
		var libraryVersionDirectory = GetLibraryVersionDirectory(author, name, version);

		return Task.FromResult(libraryVersionDirectory.Exists);
	}

	/// <inheritdoc />
	public async Task<StorageMetadata?> GetMetadataAsync(string author, string name, string version, CancellationToken cancellationToken = default)
	{
		if (!await ExistsAsync(author, name, version, cancellationToken))
			return null;

		var libraryVersionDirectory = GetLibraryVersionDirectory(author, name, version);
		var metadataFile = new FileInfo(Path.Combine(libraryVersionDirectory.FullName, "metadata.json"));

		if (!metadataFile.Exists)
			return null;

		logger.LogTrace(
			"Reading metadata for {Author}/{Name}@{Version} from {Path}",
			author,
			name,
			version,
			metadataFile.FullName
		);

		await using var stream = metadataFile.OpenRead();

		var metadata = await JsonSerializer.DeserializeAsync<StorageMetadata>(stream, cancellationToken: cancellationToken);

		if (metadata is null)
			throw new("Failed to deserialize metadata.");

		return metadata;
	}

	/// <inheritdoc />
	public async Task SetMetadataAsync(
		string author,
		string name,
		string version,
		StorageMetadata metadata,
		CancellationToken cancellationToken = default
	)
	{
		var libraryVersionDirectory = GetLibraryVersionDirectory(author, name, version, true);
		var metadataFile = new FileInfo(Path.Combine(libraryVersionDirectory.FullName, "metadata.json"));

		logger.LogTrace(
			"Writing metadata for {Author}/{Name}@{Version} to {Path}",
			author,
			name,
			version,
			metadataFile.FullName
		);

		await using var stream = metadataFile.OpenWrite();

		await JsonSerializer.SerializeAsync(stream, metadata, cancellationToken: cancellationToken);

		await stream.FlushAsync(cancellationToken);
	}

	/// <inheritdoc />
	public async Task<Stream> OpenReadAsync(string author, string name, string version, CancellationToken cancellationToken = default)
	{
		if (!await ExistsAsync(author, name, version, cancellationToken))
			throw new("Library version does not exist.");

		var libraryVersionDirectory = GetLibraryVersionDirectory(author, name, version);

		var libraryFile = new FileInfo(Path.Combine(libraryVersionDirectory.FullName, "library.zip"));

		logger.LogTrace(
			"Reading library for {Author}/{Name}@{Version} from {Path}",
			author,
			name,
			version,
			libraryFile.FullName
		);

		if (!libraryFile.Exists)
			throw new("Library version does not have a library file.");

		return libraryFile.OpenRead();
	}

	/// <inheritdoc />
	public Stream OpenWrite(string author, string name, string version, CancellationToken cancellationToken = default)
	{
		var libraryVersionDirectory = GetLibraryVersionDirectory(author, name, version, true);

		logger.LogTrace(
			"Writing library for {Author}/{Name}@{Version} to {Path}",
			author,
			name,
			version,
			libraryVersionDirectory.FullName
		);

		var libraryFile = new FileInfo(Path.Combine(libraryVersionDirectory.FullName, "library.zip"));

		return libraryFile.OpenWrite();
	}

	/// <inheritdoc />
	public async Task DeleteAsync(string author, string name, string version, CancellationToken cancellationToken = default)
	{
		if (!await ExistsAsync(author, name, version, cancellationToken))
			return;

		var libraryVersionDirectory = GetLibraryVersionDirectory(author, name, version);

		logger.LogTrace(
			"Deleting library version {Author}/{Name}@{Version} from {Path}",
			author,
			name,
			version,
			libraryVersionDirectory.FullName
		);

		libraryVersionDirectory.Delete(true);

		var cacheKey = GetCacheKey(author, name, version);

		libraryVersionDirectoryCache.Remove(cacheKey, out _);
	}

	/// <inheritdoc />
	public Task DeleteAllAsync(string author, string name, CancellationToken cancellationToken = default)
	{
		var libraryDirectory = GetLibraryDirectory(author, name);
		if (!libraryDirectory.Exists)
			throw new("Library does not exist.");

		logger.LogTrace("Deleting library {Author}/{Name} from {Path}", author, name, libraryDirectory.FullName);

		libraryDirectory.Delete(true);

		// don't bother checking each entry, just clear the whole thing
		libraryVersionDirectoryCache.Clear();

		return Task.CompletedTask;
	}
}
