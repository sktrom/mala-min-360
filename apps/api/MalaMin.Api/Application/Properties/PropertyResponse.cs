namespace MalaMin.Api.Application.Properties;

public sealed record PropertyResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    string City,
    string AreaName,
    string? AddressText,
    decimal Price,
    string Currency,
    string ListingType,
    string PropertyType,
    int? Bedrooms,
    int? Bathrooms,
    int? FloorNumber,
    int AreaSqm,
    string Status,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
