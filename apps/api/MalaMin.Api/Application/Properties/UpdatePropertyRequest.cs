namespace MalaMin.Api.Application.Properties;

public sealed record UpdatePropertyRequest(
    string Title,
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
    bool IsPublished);
