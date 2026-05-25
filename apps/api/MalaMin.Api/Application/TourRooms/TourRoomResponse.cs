namespace MalaMin.Api.Application.TourRooms;

public sealed record TourRoomResponse(
    Guid Id,
    Guid PropertyId,
    string Name,
    Guid PanoramaMediaId,
    string PanoramaUrl,
    string OriginalFileName,
    string MimeType,
    long SizeBytes,
    int? Width,
    int? Height,
    int SortOrder,
    bool IsStartRoom,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
