using MalaMin.Api.Domain.Constants;

namespace MalaMin.Api.Domain.Entities;

public class MediaFile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public string Url { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public string ProcessingStatus { get; set; } = MediaProcessingStatuses.Completed;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeletedAt { get; set; }
}
