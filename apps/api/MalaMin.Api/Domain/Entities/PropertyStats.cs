namespace MalaMin.Api.Domain.Entities;

public class PropertyStats
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Guid PropertyId { get; set; }

    public Property Property { get; set; } = null!;

    public DateOnly StatDate { get; set; }

    public int Views { get; set; }

    public int TourViews { get; set; }

    public int WhatsAppClicks { get; set; }

    public int QrScans { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
