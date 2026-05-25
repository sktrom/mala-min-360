namespace MalaMin.Api.Domain.Entities;

public class PropertyImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Guid PropertyId { get; set; }

    public Property Property { get; set; } = null!;

    public Guid MediaFileId { get; set; }

    public MediaFile MediaFile { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool IsCover { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeletedAt { get; set; }
}
