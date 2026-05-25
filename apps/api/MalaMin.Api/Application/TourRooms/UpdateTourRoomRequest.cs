namespace MalaMin.Api.Application.TourRooms;

public sealed record UpdateTourRoomRequest(
    string Name,
    Guid PanoramaMediaId,
    int SortOrder,
    bool IsStartRoom);
