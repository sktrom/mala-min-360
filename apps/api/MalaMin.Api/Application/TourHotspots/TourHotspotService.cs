using MalaMin.Api.Application.Common;
using MalaMin.Api.Domain.Constants;
using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.TourHotspots;

public sealed class TourHotspotService(AppDbContext db, ITenantContext tenantContext)
{
    public async Task<IReadOnlyList<TourHotspotResponse>?> ListAsync(
        Guid propertyId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;

        if (!await RoomExistsAsync(propertyId, roomId, tenantId, cancellationToken))
        {
            return null;
        }

        return await db.TourHotspots
            .AsNoTracking()
            .Where(hotspot => hotspot.TenantId == tenantId
                && hotspot.RoomId == roomId
                && hotspot.DeletedAt == null)
            .OrderBy(hotspot => hotspot.CreatedAt)
            .Select(hotspot => new TourHotspotResponse(
                hotspot.Id,
                hotspot.RoomId,
                hotspot.TargetRoomId,
                hotspot.Type,
                hotspot.Label,
                hotspot.Yaw,
                hotspot.Pitch,
                hotspot.CreatedAt,
                hotspot.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TourHotspotServiceResult<TourHotspotResponse>> GetAsync(
        Guid propertyId,
        Guid roomId,
        Guid hotspotId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var hotspot = await FindHotspotAsync(propertyId, roomId, hotspotId, tenantId, cancellationToken);

        return hotspot is null
            ? TourHotspotServiceResult<TourHotspotResponse>.NotFound("TOUR_HOTSPOT_NOT_FOUND", "Tour hotspot was not found.")
            : TourHotspotServiceResult<TourHotspotResponse>.Success(ToResponse(hotspot));
    }

    public async Task<TourHotspotServiceResult<TourHotspotResponse>> CreateAsync(
        Guid propertyId,
        Guid roomId,
        CreateTourHotspotRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var room = await FindRoomAsync(propertyId, roomId, tenantId, cancellationToken);

        if (room is null)
        {
            return TourHotspotServiceResult<TourHotspotResponse>.NotFound("TOUR_ROOM_NOT_FOUND", "Tour room was not found.");
        }

        var validation = await ValidateRequestAsync(
            propertyId,
            tenantId,
            request.Type,
            request.Label,
            request.Yaw,
            request.Pitch,
            request.TargetRoomId,
            cancellationToken);

        if (validation.ErrorMessage is not null)
        {
            return TourHotspotServiceResult<TourHotspotResponse>.Validation(validation.ErrorMessage);
        }

        var now = DateTimeOffset.UtcNow;
        var hotspot = new TourHotspot
        {
            TenantId = tenantId,
            RoomId = room.Id,
            TargetRoomId = validation.TargetRoomId,
            Type = request.Type.Trim(),
            Label = request.Label.Trim(),
            Yaw = request.Yaw,
            Pitch = request.Pitch,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.TourHotspots.Add(hotspot);
        await db.SaveChangesAsync(cancellationToken);

        return TourHotspotServiceResult<TourHotspotResponse>.Success(ToResponse(hotspot));
    }

    public async Task<TourHotspotServiceResult<TourHotspotResponse>> UpdateAsync(
        Guid propertyId,
        Guid roomId,
        Guid hotspotId,
        UpdateTourHotspotRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var hotspot = await FindHotspotAsync(propertyId, roomId, hotspotId, tenantId, cancellationToken);

        if (hotspot is null)
        {
            return TourHotspotServiceResult<TourHotspotResponse>.NotFound("TOUR_HOTSPOT_NOT_FOUND", "Tour hotspot was not found.");
        }

        var validation = await ValidateRequestAsync(
            propertyId,
            tenantId,
            request.Type,
            request.Label,
            request.Yaw,
            request.Pitch,
            request.TargetRoomId,
            cancellationToken);

        if (validation.ErrorMessage is not null)
        {
            return TourHotspotServiceResult<TourHotspotResponse>.Validation(validation.ErrorMessage);
        }

        hotspot.TargetRoomId = validation.TargetRoomId;
        hotspot.Type = request.Type.Trim();
        hotspot.Label = request.Label.Trim();
        hotspot.Yaw = request.Yaw;
        hotspot.Pitch = request.Pitch;
        hotspot.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return TourHotspotServiceResult<TourHotspotResponse>.Success(ToResponse(hotspot));
    }

    public async Task<bool> SoftDeleteAsync(
        Guid propertyId,
        Guid roomId,
        Guid hotspotId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var hotspot = await FindHotspotAsync(propertyId, roomId, hotspotId, tenantId, cancellationToken);

        if (hotspot is null)
        {
            return false;
        }

        hotspot.DeletedAt = DateTimeOffset.UtcNow;
        hotspot.UpdatedAt = hotspot.DeletedAt.Value;

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<HotspotValidationResult> ValidateRequestAsync(
        Guid propertyId,
        Guid tenantId,
        string type,
        string label,
        decimal yaw,
        decimal pitch,
        Guid? targetRoomId,
        CancellationToken cancellationToken)
    {
        if (!TourHotspotTypes.All.Contains(type))
        {
            return new HotspotValidationResult(null, "Type must be Navigate or Info.");
        }

        if (string.IsNullOrWhiteSpace(label) || label.Length > 150)
        {
            return new HotspotValidationResult(null, "Label is required and must be 150 characters or fewer.");
        }

        if (yaw is < -180 or > 180)
        {
            return new HotspotValidationResult(null, "Yaw must be between -180 and 180.");
        }

        if (pitch is < -90 or > 90)
        {
            return new HotspotValidationResult(null, "Pitch must be between -90 and 90.");
        }

        if (type == TourHotspotTypes.Navigate)
        {
            if (!targetRoomId.HasValue)
            {
                return new HotspotValidationResult(null, "TargetRoomId is required for Navigate hotspots.");
            }

            var targetRoomExists = await db.TourRooms.AnyAsync(
                room => room.Id == targetRoomId.Value
                    && room.TenantId == tenantId
                    && room.PropertyId == propertyId
                    && room.DeletedAt == null,
                cancellationToken);

            if (!targetRoomExists)
            {
                return new HotspotValidationResult(null, "Target room was not found for this property.");
            }

            return new HotspotValidationResult(targetRoomId.Value, null);
        }

        return new HotspotValidationResult(null, null);
    }

    private async Task<bool> RoomExistsAsync(
        Guid propertyId,
        Guid roomId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await db.TourRooms.AnyAsync(
            room => room.Id == roomId
                && room.PropertyId == propertyId
                && room.TenantId == tenantId
                && room.DeletedAt == null
                && room.Property.DeletedAt == null,
            cancellationToken);
    }

    private async Task<TourRoom?> FindRoomAsync(
        Guid propertyId,
        Guid roomId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await db.TourRooms
            .Include(room => room.Property)
            .SingleOrDefaultAsync(
                room => room.Id == roomId
                    && room.PropertyId == propertyId
                    && room.TenantId == tenantId
                    && room.DeletedAt == null
                    && room.Property.DeletedAt == null,
                cancellationToken);
    }

    private async Task<TourHotspot?> FindHotspotAsync(
        Guid propertyId,
        Guid roomId,
        Guid hotspotId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await db.TourHotspots
            .Include(hotspot => hotspot.Room)
            .SingleOrDefaultAsync(
                hotspot => hotspot.Id == hotspotId
                    && hotspot.RoomId == roomId
                    && hotspot.TenantId == tenantId
                    && hotspot.DeletedAt == null
                    && hotspot.Room.PropertyId == propertyId
                    && hotspot.Room.DeletedAt == null
                    && hotspot.Room.Property.DeletedAt == null,
                cancellationToken);
    }

    private static TourHotspotResponse ToResponse(TourHotspot hotspot)
    {
        return new TourHotspotResponse(
            hotspot.Id,
            hotspot.RoomId,
            hotspot.TargetRoomId,
            hotspot.Type,
            hotspot.Label,
            hotspot.Yaw,
            hotspot.Pitch,
            hotspot.CreatedAt,
            hotspot.UpdatedAt);
    }

    private sealed record HotspotValidationResult(Guid? TargetRoomId, string? ErrorMessage);
}

public sealed record TourHotspotServiceResult<T>(T? Data, string? ErrorCode, string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode is null;

    public static TourHotspotServiceResult<T> Success(T data)
    {
        return new TourHotspotServiceResult<T>(data, null, null);
    }

    public static TourHotspotServiceResult<T> NotFound(string code, string message)
    {
        return new TourHotspotServiceResult<T>(default, code, message);
    }

    public static TourHotspotServiceResult<T> Validation(string message)
    {
        return new TourHotspotServiceResult<T>(default, "VALIDATION_ERROR", message);
    }
}
