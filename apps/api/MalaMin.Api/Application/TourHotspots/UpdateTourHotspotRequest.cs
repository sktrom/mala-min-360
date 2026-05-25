namespace MalaMin.Api.Application.TourHotspots;

public sealed record UpdateTourHotspotRequest(
    Guid? TargetRoomId,
    string Type,
    string Label,
    decimal Yaw,
    decimal Pitch);
