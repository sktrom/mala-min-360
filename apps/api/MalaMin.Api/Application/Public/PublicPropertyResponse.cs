namespace MalaMin.Api.Application.Public;

public sealed record PublicPropertyResponse(
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
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    PublicTenantSummary Tenant);

public sealed record PublicTenantSummary(
    string Name,
    string Slug,
    string? Phone,
    string? WhatsAppNumber,
    string? LogoUrl,
    string? City);

public sealed record PublicPropertyCardResponse(
    Guid Id,
    string Title,
    string Slug,
    string City,
    string AreaName,
    decimal Price,
    string Currency,
    string ListingType,
    string PropertyType,
    int? Bedrooms,
    int? Bathrooms,
    int AreaSqm,
    string TenantName,
    string TenantSlug,
    string? CoverImageUrl);
