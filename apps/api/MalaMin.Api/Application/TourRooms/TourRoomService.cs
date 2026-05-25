using MalaMin.Api.Application.Common;
using MalaMin.Api.Domain.Constants;
using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.TourRooms;

public sealed class TourRoomService(AppDbContext db, ITenantContext tenantContext)
{
    public async Task<IReadOnlyList<TourRoomResponse>?> ListAsync(
        Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;

        if (!await PropertyExistsAsync(propertyId, tenantId, cancellationToken))
        {
            return null;
        }

        return await db.TourRooms
            .AsNoTracking()
            .Where(room => room.TenantId == tenantId
                && room.PropertyId == propertyId
                && room.DeletedAt == null)
            .OrderBy(room => room.SortOrder)
            .ThenBy(room => room.CreatedAt)
            .Select(room => new TourRoomResponse(
                room.Id,
                room.PropertyId,
                room.Name,
                room.PanoramaMediaId,
                room.PanoramaMedia.Url,
                room.PanoramaMedia.OriginalFileName,
                room.PanoramaMedia.MimeType,
                room.PanoramaMedia.SizeBytes,
                room.PanoramaMedia.Width,
                room.PanoramaMedia.Height,
                room.SortOrder,
                room.IsStartRoom,
                room.CreatedAt,
                room.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TourRoomServiceResult<TourRoomResponse>> GetAsync(
        Guid propertyId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var room = await FindTourRoomAsync(propertyId, roomId, tenantId, cancellationToken);

        return room is null
            ? TourRoomServiceResult<TourRoomResponse>.NotFound("TOUR_ROOM_NOT_FOUND", "Tour room was not found.")
            : TourRoomServiceResult<TourRoomResponse>.Success(ToResponse(room));
    }

    public async Task<TourRoomServiceResult<TourRoomResponse>> CreateAsync(
        Guid propertyId,
        CreateTourRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCommon(request.Name, request.SortOrder ?? 0);

        if (validationError is not null)
        {
            return TourRoomServiceResult<TourRoomResponse>.Validation(validationError);
        }

        var tenantId = tenantContext.TenantId;

        if (!await PropertyExistsAsync(propertyId, tenantId, cancellationToken))
        {
            return TourRoomServiceResult<TourRoomResponse>.NotFound("PROPERTY_NOT_FOUND", "Property was not found.");
        }

        var panoramaMedia = await FindPanoramaMediaAsync(request.PanoramaMediaId, tenantId, cancellationToken);

        if (panoramaMedia is null)
        {
            return TourRoomServiceResult<TourRoomResponse>.NotFound("MEDIA_NOT_FOUND", "Panorama media file was not found.");
        }

        if (panoramaMedia.FileType != MediaFileTypes.Panorama360)
        {
            return TourRoomServiceResult<TourRoomResponse>.Validation("Only Panorama360 media can be linked as a tour room panorama.");
        }

        var hasRooms = await db.TourRooms.AnyAsync(
            room => room.TenantId == tenantId
                && room.PropertyId == propertyId
                && room.DeletedAt == null,
            cancellationToken);
        var isStartRoom = request.IsStartRoom == true || !hasRooms;
        var now = DateTimeOffset.UtcNow;
        var room = new TourRoom
        {
            TenantId = tenantId,
            PropertyId = propertyId,
            Name = request.Name.Trim(),
            PanoramaMediaId = panoramaMedia.Id,
            SortOrder = request.SortOrder ?? 0,
            IsStartRoom = isStartRoom,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (isStartRoom)
        {
            await UnsetStartRoomsAsync(propertyId, tenantId, now, cancellationToken);
        }

        db.TourRooms.Add(room);
        await db.SaveChangesAsync(cancellationToken);
        room.PanoramaMedia = panoramaMedia;

        return TourRoomServiceResult<TourRoomResponse>.Success(ToResponse(room));
    }

    public async Task<TourRoomServiceResult<TourRoomResponse>> UpdateAsync(
        Guid propertyId,
        Guid roomId,
        UpdateTourRoomRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCommon(request.Name, request.SortOrder);

        if (validationError is not null)
        {
            return TourRoomServiceResult<TourRoomResponse>.Validation(validationError);
        }

        var tenantId = tenantContext.TenantId;
        var room = await FindTourRoomAsync(propertyId, roomId, tenantId, cancellationToken);

        if (room is null)
        {
            return TourRoomServiceResult<TourRoomResponse>.NotFound("TOUR_ROOM_NOT_FOUND", "Tour room was not found.");
        }

        var panoramaMedia = await FindPanoramaMediaAsync(request.PanoramaMediaId, tenantId, cancellationToken);

        if (panoramaMedia is null)
        {
            return TourRoomServiceResult<TourRoomResponse>.NotFound("MEDIA_NOT_FOUND", "Panorama media file was not found.");
        }

        if (panoramaMedia.FileType != MediaFileTypes.Panorama360)
        {
            return TourRoomServiceResult<TourRoomResponse>.Validation("Only Panorama360 media can be linked as a tour room panorama.");
        }

        var now = DateTimeOffset.UtcNow;

        if (request.IsStartRoom)
        {
            await UnsetStartRoomsAsync(propertyId, tenantId, now, cancellationToken);
        }

        room.Name = request.Name.Trim();
        room.PanoramaMediaId = panoramaMedia.Id;
        room.PanoramaMedia = panoramaMedia;
        room.SortOrder = request.SortOrder;
        room.IsStartRoom = request.IsStartRoom;
        room.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return TourRoomServiceResult<TourRoomResponse>.Success(ToResponse(room));
    }

    public async Task<TourRoomServiceResult<IReadOnlyList<TourRoomResponse>>> ReorderAsync(
        Guid propertyId,
        ReorderTourRoomsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Rooms is null || request.Rooms.Count == 0)
        {
            return TourRoomServiceResult<IReadOnlyList<TourRoomResponse>>.Validation("Rooms are required.");
        }

        if (request.Rooms.Any(room => room.SortOrder < 0))
        {
            return TourRoomServiceResult<IReadOnlyList<TourRoomResponse>>.Validation("SortOrder cannot be negative.");
        }

        var tenantId = tenantContext.TenantId;

        if (!await PropertyExistsAsync(propertyId, tenantId, cancellationToken))
        {
            return TourRoomServiceResult<IReadOnlyList<TourRoomResponse>>.NotFound("PROPERTY_NOT_FOUND", "Property was not found.");
        }

        var roomIds = request.Rooms.Select(room => room.TourRoomId).ToHashSet();
        var rooms = await db.TourRooms
            .Where(room => room.TenantId == tenantId
                && room.PropertyId == propertyId
                && room.DeletedAt == null
                && roomIds.Contains(room.Id))
            .ToListAsync(cancellationToken);

        if (rooms.Count != roomIds.Count)
        {
            return TourRoomServiceResult<IReadOnlyList<TourRoomResponse>>.NotFound("TOUR_ROOM_NOT_FOUND", "Tour room was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var sortOrders = request.Rooms.ToDictionary(room => room.TourRoomId, room => room.SortOrder);

        foreach (var room in rooms)
        {
            room.SortOrder = sortOrders[room.Id];
            room.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        var orderedRooms = await ListAsync(propertyId, cancellationToken);

        return TourRoomServiceResult<IReadOnlyList<TourRoomResponse>>.Success(orderedRooms ?? []);
    }

    public async Task<TourRoomServiceResult<TourRoomResponse>> SetStartRoomAsync(
        Guid propertyId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var room = await FindTourRoomAsync(propertyId, roomId, tenantId, cancellationToken);

        if (room is null)
        {
            return TourRoomServiceResult<TourRoomResponse>.NotFound("TOUR_ROOM_NOT_FOUND", "Tour room was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        await UnsetStartRoomsAsync(propertyId, tenantId, now, cancellationToken);

        room.IsStartRoom = true;
        room.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return TourRoomServiceResult<TourRoomResponse>.Success(ToResponse(room));
    }

    public async Task<bool> SoftDeleteAsync(
        Guid propertyId,
        Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var room = await FindTourRoomAsync(propertyId, roomId, tenantId, cancellationToken);

        if (room is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var wasStartRoom = room.IsStartRoom;
        room.DeletedAt = now;
        room.UpdatedAt = now;
        room.IsStartRoom = false;

        if (wasStartRoom)
        {
            var nextRoom = await db.TourRooms
                .Where(next => next.TenantId == tenantId
                    && next.PropertyId == propertyId
                    && next.Id != roomId
                    && next.DeletedAt == null)
                .OrderBy(next => next.SortOrder)
                .ThenBy(next => next.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextRoom is not null)
            {
                nextRoom.IsStartRoom = true;
                nextRoom.UpdatedAt = now;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string? ValidateCommon(string name, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
        {
            return "Name is required and must be 150 characters or fewer.";
        }

        if (sortOrder < 0)
        {
            return "SortOrder cannot be negative.";
        }

        return null;
    }

    private async Task<bool> PropertyExistsAsync(Guid propertyId, Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.Properties.AnyAsync(
            property => property.Id == propertyId
                && property.TenantId == tenantId
                && property.DeletedAt == null,
            cancellationToken);
    }

    private async Task<MediaFile?> FindPanoramaMediaAsync(Guid mediaFileId, Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.MediaFiles.SingleOrDefaultAsync(
            file => file.Id == mediaFileId
                && file.TenantId == tenantId
                && file.DeletedAt == null,
            cancellationToken);
    }

    private async Task<TourRoom?> FindTourRoomAsync(
        Guid propertyId,
        Guid roomId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await db.TourRooms
            .Include(room => room.PanoramaMedia)
            .SingleOrDefaultAsync(
                room => room.Id == roomId
                    && room.TenantId == tenantId
                    && room.PropertyId == propertyId
                    && room.DeletedAt == null,
                cancellationToken);
    }

    private async Task UnsetStartRoomsAsync(
        Guid propertyId,
        Guid tenantId,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var currentStartRooms = await db.TourRooms
            .Where(room => room.TenantId == tenantId
                && room.PropertyId == propertyId
                && room.IsStartRoom
                && room.DeletedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var currentStartRoom in currentStartRooms)
        {
            currentStartRoom.IsStartRoom = false;
            currentStartRoom.UpdatedAt = updatedAt;
        }
    }

    private static TourRoomResponse ToResponse(TourRoom room)
    {
        return new TourRoomResponse(
            room.Id,
            room.PropertyId,
            room.Name,
            room.PanoramaMediaId,
            room.PanoramaMedia.Url,
            room.PanoramaMedia.OriginalFileName,
            room.PanoramaMedia.MimeType,
            room.PanoramaMedia.SizeBytes,
            room.PanoramaMedia.Width,
            room.PanoramaMedia.Height,
            room.SortOrder,
            room.IsStartRoom,
            room.CreatedAt,
            room.UpdatedAt);
    }
}

public sealed record TourRoomServiceResult<T>(T? Data, string? ErrorCode, string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode is null;

    public static TourRoomServiceResult<T> Success(T data)
    {
        return new TourRoomServiceResult<T>(data, null, null);
    }

    public static TourRoomServiceResult<T> NotFound(string code, string message)
    {
        return new TourRoomServiceResult<T>(default, code, message);
    }

    public static TourRoomServiceResult<T> Validation(string message)
    {
        return new TourRoomServiceResult<T>(default, "VALIDATION_ERROR", message);
    }
}
