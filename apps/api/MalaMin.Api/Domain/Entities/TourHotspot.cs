using MalaMin.Api.Domain.Constants;

namespace MalaMin.Api.Domain.Entities;

public class TourHotspot
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Guid RoomId { get; set; }

    public TourRoom Room { get; set; } = null!;

    public Guid? TargetRoomId { get; set; }

    public TourRoom? TargetRoom { get; set; }

    public string Type { get; set; } = TourHotspotTypes.Navigate;

    public string Label { get; set; } = string.Empty;

    public decimal Yaw { get; set; }

    public decimal Pitch { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeletedAt { get; set; }
}
