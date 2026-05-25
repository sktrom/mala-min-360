namespace MalaMin.Api.Application.Public;

public sealed record PublicTourResponse(
    Guid PropertyId,
    string PropertyTitle,
    string PropertySlug,
    string TenantName,
    string TenantSlug,
    Guid? StartRoomId,
    List<PublicTourRoomResponse> Rooms);

public sealed record PublicTourRoomResponse(
    Guid Id,
    string Name,
    string PanoramaUrl,
    string OriginalFileName,
    string MimeType,
    long SizeBytes,
    int? Width,
    int? Height,
    int SortOrder,
    bool IsStartRoom,
    List<PublicTourHotspotResponse> Hotspots);

public sealed record PublicTourHotspotResponse(
    Guid Id,
    Guid RoomId,
    Guid? TargetRoomId,
    string Type,
    string Label,
    decimal Yaw,
    decimal Pitch);
