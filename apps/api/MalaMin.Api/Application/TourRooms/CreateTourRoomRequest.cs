namespace MalaMin.Api.Application.TourRooms;

public sealed record CreateTourRoomRequest(
    string Name,
    Guid PanoramaMediaId,
    int? SortOrder,
    bool? IsStartRoom);
