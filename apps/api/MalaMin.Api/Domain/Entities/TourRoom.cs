namespace MalaMin.Api.Domain.Entities;

public class TourRoom
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Guid PropertyId { get; set; }

    public Property Property { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public Guid PanoramaMediaId { get; set; }

    public MediaFile PanoramaMedia { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsStartRoom { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeletedAt { get; set; }
}
