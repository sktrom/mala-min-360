using System.Text.RegularExpressions;
using MalaMin.Api.Application.Common;
using MalaMin.Api.Domain.Constants;
using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.Properties;

public sealed class PropertyService(AppDbContext db, ITenantContext tenantContext)
{
    public async Task<IReadOnlyList<PropertyResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;

        return await db.Properties
            .AsNoTracking()
            .Where(property => property.TenantId == tenantId && property.DeletedAt == null)
            .OrderByDescending(property => property.CreatedAt)
            .Select(property => new PropertyResponse(
                property.Id,
                property.Title,
                property.Slug,
                property.Description,
                property.City,
                property.AreaName,
                property.AddressText,
                property.Price,
                property.Currency,
                property.ListingType,
                property.PropertyType,
                property.Bedrooms,
                property.Bathrooms,
                property.FloorNumber,
                property.AreaSqm,
                property.Status,
                property.IsPublished,
                property.CreatedAt,
                property.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PropertyResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var property = await FindCurrentTenantPropertyAsync(id, cancellationToken);

        return property is null ? null : ToResponse(property);
    }

    public async Task<PropertyServiceResult<PropertyResponse>> CreateAsync(
        CreatePropertyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCreate(request);

        if (validationError is not null)
        {
            return PropertyServiceResult<PropertyResponse>.Validation(validationError);
        }

        var tenantId = tenantContext.TenantId;
        var now = DateTimeOffset.UtcNow;
        var property = new Property
        {
            TenantId = tenantId,
            Title = request.Title.Trim(),
            Slug = await GenerateUniqueSlugAsync(request.Title, tenantId, null, cancellationToken),
            Description = NormalizeOptional(request.Description),
            City = request.City.Trim(),
            AreaName = request.AreaName.Trim(),
            AddressText = NormalizeOptional(request.AddressText),
            Price = request.Price,
            Currency = request.Currency.Trim(),
            ListingType = request.ListingType.Trim(),
            PropertyType = request.PropertyType.Trim(),
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            FloorNumber = request.FloorNumber,
            AreaSqm = request.AreaSqm,
            Status = PropertyStatuses.Available,
            IsPublished = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Properties.Add(property);
        await db.SaveChangesAsync(cancellationToken);

        return PropertyServiceResult<PropertyResponse>.Success(ToResponse(property));
    }

    public async Task<PropertyServiceResult<PropertyResponse>> UpdateAsync(
        Guid id,
        UpdatePropertyRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateUpdate(request);

        if (validationError is not null)
        {
            return PropertyServiceResult<PropertyResponse>.Validation(validationError);
        }

        var property = await FindCurrentTenantPropertyAsync(id, cancellationToken);

        if (property is null)
        {
            return PropertyServiceResult<PropertyResponse>.NotFound();
        }

        property.Title = request.Title.Trim();
        property.Slug = await GenerateUniqueSlugAsync(request.Title, property.TenantId, property.Id, cancellationToken);
        property.Description = NormalizeOptional(request.Description);
        property.City = request.City.Trim();
        property.AreaName = request.AreaName.Trim();
        property.AddressText = NormalizeOptional(request.AddressText);
        property.Price = request.Price;
        property.Currency = request.Currency.Trim();
        property.ListingType = request.ListingType.Trim();
        property.PropertyType = request.PropertyType.Trim();
        property.Bedrooms = request.Bedrooms;
        property.Bathrooms = request.Bathrooms;
        property.FloorNumber = request.FloorNumber;
        property.AreaSqm = request.AreaSqm;
        property.Status = request.Status.Trim();
        property.IsPublished = request.IsPublished;
        property.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return PropertyServiceResult<PropertyResponse>.Success(ToResponse(property));
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var property = await FindCurrentTenantPropertyAsync(id, cancellationToken);

        if (property is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        property.DeletedAt = now;
        property.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<Property?> FindCurrentTenantPropertyAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        return await db.Properties
            .SingleOrDefaultAsync(
                property => property.Id == id
                    && property.TenantId == tenantId
                    && property.DeletedAt == null,
                cancellationToken);
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string title,
        Guid tenantId,
        Guid? currentPropertyId,
        CancellationToken cancellationToken)
    {
        var baseSlug = GenerateSlug(title);
        var slug = baseSlug;
        var suffix = 2;

        while (await db.Properties.AnyAsync(
            property => property.TenantId == tenantId
                && property.Slug == slug
                && (!currentPropertyId.HasValue || property.Id != currentPropertyId.Value),
            cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string GenerateSlug(string value)
    {
        var slug = value.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9-]", string.Empty);
        slug = Regex.Replace(slug, "-{2,}", "-").Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "property" : slug;
    }

    private static string? ValidateCreate(CreatePropertyRequest request)
    {
        return ValidateCommon(
            request.Title,
            request.City,
            request.AreaName,
            request.Price,
            request.Currency,
            request.ListingType,
            request.PropertyType,
            request.Bedrooms,
            request.Bathrooms,
            request.FloorNumber,
            request.AreaSqm,
            null);
    }

    private static string? ValidateUpdate(UpdatePropertyRequest request)
    {
        return ValidateCommon(
            request.Title,
            request.City,
            request.AreaName,
            request.Price,
            request.Currency,
            request.ListingType,
            request.PropertyType,
            request.Bedrooms,
            request.Bathrooms,
            request.FloorNumber,
            request.AreaSqm,
            request.Status);
    }

    private static string? ValidateCommon(
        string title,
        string city,
        string areaName,
        decimal price,
        string currency,
        string listingType,
        string propertyType,
        int? bedrooms,
        int? bathrooms,
        int? floorNumber,
        int areaSqm,
        string? status)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 250)
        {
            return "Title is required and must be 250 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(city) || city.Length > 100)
        {
            return "City is required and must be 100 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(areaName) || areaName.Length > 150)
        {
            return "AreaName is required and must be 150 characters or fewer.";
        }

        if (price < 0)
        {
            return "Price must be greater than or equal to 0.";
        }

        if (areaSqm <= 0)
        {
            return "AreaSqm must be greater than 0.";
        }

        if (string.IsNullOrWhiteSpace(currency) || currency.Length > 10)
        {
            return "Currency is required and must be 10 characters or fewer.";
        }

        if (!ListingTypes.All.Contains(listingType))
        {
            return "ListingType must be Sale or Rent.";
        }

        if (!PropertyTypes.All.Contains(propertyType))
        {
            return "PropertyType is not valid.";
        }

        if (status is not null && !PropertyStatuses.All.Contains(status))
        {
            return "Status is not valid.";
        }

        if (bedrooms < 0 || bathrooms < 0 || floorNumber < 0)
        {
            return "Bedrooms, Bathrooms, and FloorNumber cannot be negative.";
        }

        return null;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static PropertyResponse ToResponse(Property property)
    {
        return new PropertyResponse(
            property.Id,
            property.Title,
            property.Slug,
            property.Description,
            property.City,
            property.AreaName,
            property.AddressText,
            property.Price,
            property.Currency,
            property.ListingType,
            property.PropertyType,
            property.Bedrooms,
            property.Bathrooms,
            property.FloorNumber,
            property.AreaSqm,
            property.Status,
            property.IsPublished,
            property.CreatedAt,
            property.UpdatedAt);
    }
}

public sealed record PropertyServiceResult<T>(T? Data, string? ErrorCode, string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode is null;

    public static PropertyServiceResult<T> Success(T data)
    {
        return new PropertyServiceResult<T>(data, null, null);
    }

    public static PropertyServiceResult<T> NotFound()
    {
        return new PropertyServiceResult<T>(default, "PROPERTY_NOT_FOUND", "Property was not found.");
    }

    public static PropertyServiceResult<T> Validation(string message)
    {
        return new PropertyServiceResult<T>(default, "VALIDATION_ERROR", message);
    }
}
