namespace MalaMin.Api.Application.TourHotspots;

public sealed record CreateTourHotspotRequest(
    Guid? TargetRoomId,
    string Type,
    string Label,
    decimal Yaw,
    decimal Pitch);
