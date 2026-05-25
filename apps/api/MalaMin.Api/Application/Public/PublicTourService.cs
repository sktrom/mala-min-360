using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.Public;

public sealed class PublicTourService(AppDbContext db)
{
    public async Task<PublicTourResponse?> GetPublishedTourAsync(
        string tenantSlug,
        string propertySlug,
        CancellationToken cancellationToken = default)
    {
        var property = await db.Properties
            .AsNoTracking()
            .Where(existingProperty => existingProperty.Tenant.Slug == tenantSlug
                && existingProperty.Slug == propertySlug
                && existingProperty.IsPublished
                && existingProperty.DeletedAt == null)
            .Select(existingProperty => new
            {
                existingProperty.Id,
                existingProperty.Title,
                existingProperty.Slug,
                TenantName = existingProperty.Tenant.Name,
                TenantSlug = existingProperty.Tenant.Slug
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (property is null)
        {
            return null;
        }

        var rooms = await db.TourRooms
            .AsNoTracking()
            .Where(room => room.PropertyId == property.Id && room.DeletedAt == null)
            .OrderBy(room => room.SortOrder)
            .ThenBy(room => room.CreatedAt)
            .Select(room => new
            {
                room.Id,
                room.Name,
                PanoramaUrl = room.PanoramaMedia.Url,
                room.PanoramaMedia.OriginalFileName,
                room.PanoramaMedia.MimeType,
                room.PanoramaMedia.SizeBytes,
                room.PanoramaMedia.Width,
                room.PanoramaMedia.Height,
                room.SortOrder,
                room.IsStartRoom,
                room.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var roomIds = rooms
            .Select(room => room.Id)
            .ToList();

        var hotspots = roomIds.Count == 0
            ? []
            : await db.TourHotspots
                .AsNoTracking()
                .Where(hotspot => roomIds.Contains(hotspot.RoomId) && hotspot.DeletedAt == null)
                .OrderBy(hotspot => hotspot.CreatedAt)
                .Select(hotspot => new PublicTourHotspotResponse(
                    hotspot.Id,
                    hotspot.RoomId,
                    hotspot.TargetRoomId,
                    hotspot.Type,
                    hotspot.Label,
                    hotspot.Yaw,
                    hotspot.Pitch))
                .ToListAsync(cancellationToken);

        var hotspotsByRoomId = hotspots
            .GroupBy(hotspot => hotspot.RoomId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var roomResponses = rooms
            .Select(room => new PublicTourRoomResponse(
                room.Id,
                room.Name,
                room.PanoramaUrl,
                room.OriginalFileName,
                room.MimeType,
                room.SizeBytes,
                room.Width,
                room.Height,
                room.SortOrder,
                room.IsStartRoom,
                hotspotsByRoomId.GetValueOrDefault(room.Id) ?? []))
            .ToList();

        var startRoomId = rooms.FirstOrDefault(room => room.IsStartRoom)?.Id
            ?? rooms.FirstOrDefault()?.Id;

        return new PublicTourResponse(
            property.Id,
            property.Title,
            property.Slug,
            property.TenantName,
            property.TenantSlug,
            startRoomId,
            roomResponses);
    }
}
