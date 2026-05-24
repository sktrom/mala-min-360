namespace MalaMin.Api.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? WhatsAppNumber { get; set; }

    public string? LogoUrl { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string Status { get; set; } = "Trial";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<AppUser> Users { get; set; } = [];

    public ICollection<Property> Properties { get; set; } = [];

    public ICollection<MediaFile> MediaFiles { get; set; } = [];
}
