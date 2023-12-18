using Leap.API.DB.Entities;
using Leap.Common.DTO.API;
using Semver;

namespace Leap.API.Interfaces;

public interface IUploadEndpointGenerator
{
	public Task<UploadEndpointData> GenerateUploadEndpointDataAsync(
		HttpRequest request,
		PendingLibraryVersion pendingVersion,
		CancellationToken cancellationToken = default
	);
}
