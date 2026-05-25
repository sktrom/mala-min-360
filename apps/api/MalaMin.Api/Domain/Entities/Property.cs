using MalaMin.Api.Domain.Constants;

namespace MalaMin.Api.Domain.Entities;

public class Property
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string City { get; set; } = string.Empty;

    public string AreaName { get; set; } = string.Empty;

    public string? AddressText { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "USD";

    public string ListingType { get; set; } = ListingTypes.Sale;

    public string PropertyType { get; set; } = PropertyTypes.Apartment;

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }

    public int? FloorNumber { get; set; }

    public int AreaSqm { get; set; }

    public string Status { get; set; } = PropertyStatuses.Available;

    public bool IsPublished { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<PropertyImage> Images { get; set; } = [];

    public ICollection<TourRoom> TourRooms { get; set; } = [];

    public ICollection<PropertyStats> Stats { get; set; } = [];
}
