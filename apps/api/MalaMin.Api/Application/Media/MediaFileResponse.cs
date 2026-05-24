namespace MalaMin.Api.Application.Media;

public sealed record MediaFileResponse(
    Guid Id,
    string Url,
    string StorageKey,
    string FileType,
    string OriginalFileName,
    string MimeType,
    long SizeBytes,
    int? Width,
    int? Height,
    string ProcessingStatus,
    DateTimeOffset CreatedAt);
