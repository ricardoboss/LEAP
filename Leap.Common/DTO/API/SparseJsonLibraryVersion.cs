namespace Leap.Common.DTO.API;

public record SparseLibraryVersionDto(string Name, string Author, string Version, DateTimeOffset? ReleaseDate, string? DistUrl, IReadOnlyDictionary<string, LinkDto> Links)
{
	public override string ToString()
	{
		return $"{Author}/{Name}@{Version}";
	}
}
