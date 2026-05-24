using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.Public;

public sealed class PublicPropertyService(AppDbContext db)
{
    public async Task<PublicPropertyResponse?> GetPublishedPropertyAsync(
        string tenantSlug,
        string propertySlug,
        CancellationToken cancellationToken = default)
    {
        return await db.Properties
            .AsNoTracking()
            .Where(property => property.Tenant.Slug == tenantSlug
                && property.Slug == propertySlug
                && property.IsPublished
                && property.DeletedAt == null)
            .Select(property => new PublicPropertyResponse(
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
                property.CreatedAt,
                property.UpdatedAt,
                new PublicTenantSummary(
                    property.Tenant.Name,
                    property.Tenant.Slug,
                    property.Tenant.Phone,
                    property.Tenant.WhatsAppNumber,
                    property.Tenant.LogoUrl,
                    property.Tenant.City)))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
