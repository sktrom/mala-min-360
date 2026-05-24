namespace MalaMin.Api.Application.Media;

public sealed record UploadMediaRequest(IFormFile File, string FileType);
