using Microsoft.AspNetCore.Http;

namespace Leap.Common.DTO.API;

public record UploadArchiveForm(IFormFile Archive);
