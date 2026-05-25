using MalaMin.Api.Application.Common;
using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.Stats;

public sealed class StatsService(AppDbContext db, ITenantContext tenantContext)
{
    public async Task<bool> TrackViewAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await TrackAsync(propertyId, StatsCounter.Views, cancellationToken);
    }

    public async Task<bool> TrackTourViewAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await TrackAsync(propertyId, StatsCounter.TourViews, cancellationToken);
    }

    public async Task<bool> TrackWhatsAppClickAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await TrackAsync(propertyId, StatsCounter.WhatsAppClicks, cancellationToken);
    }

    public async Task<bool> TrackQrScanAsync(Guid propertyId, CancellationToken cancellationToken = default)
    {
        return await TrackAsync(propertyId, StatsCounter.QrScans, cancellationToken);
    }

    public async Task<PropertyStatsOverviewResponse?> GetPropertyStatsAsync(
        Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;

        var property = await db.Properties
            .AsNoTracking()
            .Where(existingProperty => existingProperty.Id == propertyId
                && existingProperty.TenantId == tenantId
                && existingProperty.DeletedAt == null)
            .Select(existingProperty => new
            {
                existingProperty.Id,
                existingProperty.Title
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (property is null)
        {
            return null;
        }

        var dailyStats = await db.PropertyStats
            .AsNoTracking()
            .Where(stats => stats.PropertyId == propertyId && stats.TenantId == tenantId)
            .OrderBy(stats => stats.StatDate)
            .Select(stats => new PropertyStatsResponse(
                stats.StatDate,
                stats.Views,
                stats.TourViews,
                stats.WhatsAppClicks,
                stats.QrScans))
            .ToListAsync(cancellationToken);

        return new PropertyStatsOverviewResponse(
            property.Id,
            property.Title,
            dailyStats.Sum(stats => stats.Views),
            dailyStats.Sum(stats => stats.TourViews),
            dailyStats.Sum(stats => stats.WhatsAppClicks),
            dailyStats.Sum(stats => stats.QrScans),
            dailyStats);
    }

    public async Task<TenantStatsOverviewResponse> GetTenantOverviewAsync(
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;

        var totals = await db.PropertyStats
            .AsNoTracking()
            .Where(stats => stats.TenantId == tenantId
                && stats.Property.DeletedAt == null)
            .GroupBy(_ => 1)
            .Select(group => new TenantStatsOverviewResponse(
                group.Sum(stats => stats.Views),
                group.Sum(stats => stats.TourViews),
                group.Sum(stats => stats.WhatsAppClicks),
                group.Sum(stats => stats.QrScans)))
            .SingleOrDefaultAsync(cancellationToken);

        return totals ?? new TenantStatsOverviewResponse(0, 0, 0, 0);
    }

    private async Task<bool> TrackAsync(
        Guid propertyId,
        StatsCounter counter,
        CancellationToken cancellationToken)
    {
        var property = await db.Properties
            .Where(existingProperty => existingProperty.Id == propertyId
                && existingProperty.IsPublished
                && existingProperty.DeletedAt == null)
            .Select(existingProperty => new
            {
                existingProperty.Id,
                existingProperty.TenantId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (property is null)
        {
            return false;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var now = DateTimeOffset.UtcNow;

        var stats = await db.PropertyStats
            .SingleOrDefaultAsync(existingStats => existingStats.PropertyId == property.Id
                && existingStats.StatDate == today,
                cancellationToken);

        if (stats is null)
        {
            stats = new PropertyStats
            {
                TenantId = property.TenantId,
                PropertyId = property.Id,
                StatDate = today,
                CreatedAt = now,
                UpdatedAt = now
            };

            db.PropertyStats.Add(stats);
        }

        Increment(stats, counter);
        stats.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void Increment(PropertyStats stats, StatsCounter counter)
    {
        switch (counter)
        {
            case StatsCounter.Views:
                stats.Views++;
                break;
            case StatsCounter.TourViews:
                stats.TourViews++;
                break;
            case StatsCounter.WhatsAppClicks:
                stats.WhatsAppClicks++;
                break;
            case StatsCounter.QrScans:
                stats.QrScans++;
                break;
        }
    }

    private enum StatsCounter
    {
        Views,
        TourViews,
        WhatsAppClicks,
        QrScans
    }
}
