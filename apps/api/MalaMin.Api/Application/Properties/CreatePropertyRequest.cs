namespace MalaMin.Api.Application.Properties;

public sealed record CreatePropertyRequest(
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
    int AreaSqm);
