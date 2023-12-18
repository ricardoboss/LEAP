namespace Leap.Common.DTO.API;

public class SparseLibraryVersionDto
{
	public string Name { get; init; }

	public string Author { get; init; }

	public string Version { get; init; }

	public DateTimeOffset? ReleaseDate { get; init; }

	public string? DistUrl { get; init; }

	public IReadOnlyDictionary<string, LinkDto> Links { get; init; }

	public override string ToString()
	{
		return $"{Author}/{Name}@{Version}";
	}
}
