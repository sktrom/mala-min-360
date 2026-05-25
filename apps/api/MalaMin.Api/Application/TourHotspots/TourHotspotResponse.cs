namespace MalaMin.Api.Application.TourHotspots;

public sealed record TourHotspotResponse(
    Guid Id,
    Guid RoomId,
    Guid? TargetRoomId,
    string Type,
    string Label,
    decimal Yaw,
    decimal Pitch,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
