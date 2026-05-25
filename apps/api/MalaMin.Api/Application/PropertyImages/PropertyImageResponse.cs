namespace MalaMin.Api.Application.PropertyImages;

public sealed record PropertyImageResponse(
    Guid Id,
    Guid PropertyId,
    Guid MediaFileId,
    string Url,
    string OriginalFileName,
    string MimeType,
    long SizeBytes,
    int? Width,
    int? Height,
    int SortOrder,
    bool IsCover,
    DateTimeOffset CreatedAt);
