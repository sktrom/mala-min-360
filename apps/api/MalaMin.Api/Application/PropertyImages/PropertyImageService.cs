using MalaMin.Api.Application.Common;
using MalaMin.Api.Domain.Constants;
using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.PropertyImages;

public sealed class PropertyImageService(AppDbContext db, ITenantContext tenantContext)
{
    public async Task<IReadOnlyList<PropertyImageResponse>?> ListAsync(
        Guid propertyId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;

        if (!await PropertyExistsAsync(propertyId, tenantId, cancellationToken))
        {
            return null;
        }

        return await db.PropertyImages
            .AsNoTracking()
            .Include(propertyImage => propertyImage.MediaFile)
            .Where(propertyImage => propertyImage.TenantId == tenantId
                && propertyImage.PropertyId == propertyId
                && propertyImage.DeletedAt == null)
            .OrderBy(propertyImage => propertyImage.SortOrder)
            .ThenBy(propertyImage => propertyImage.CreatedAt)
            .Select(propertyImage => new PropertyImageResponse(
                propertyImage.Id,
                propertyImage.PropertyId,
                propertyImage.MediaFileId,
                propertyImage.MediaFile.Url,
                propertyImage.MediaFile.OriginalFileName,
                propertyImage.MediaFile.MimeType,
                propertyImage.MediaFile.SizeBytes,
                propertyImage.MediaFile.Width,
                propertyImage.MediaFile.Height,
                propertyImage.SortOrder,
                propertyImage.IsCover,
                propertyImage.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PropertyImageServiceResult<PropertyImageResponse>> AddAsync(
        Guid propertyId,
        AddPropertyImageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.SortOrder < 0)
        {
            return PropertyImageServiceResult<PropertyImageResponse>.Validation("SortOrder cannot be negative.");
        }

        var tenantId = tenantContext.TenantId;

        if (!await PropertyExistsAsync(propertyId, tenantId, cancellationToken))
        {
            return PropertyImageServiceResult<PropertyImageResponse>.NotFound("PROPERTY_NOT_FOUND", "Property was not found.");
        }

        var mediaFile = await db.MediaFiles.SingleOrDefaultAsync(
            file => file.Id == request.MediaFileId
                && file.TenantId == tenantId
                && file.DeletedAt == null,
            cancellationToken);

        if (mediaFile is null)
        {
            return PropertyImageServiceResult<PropertyImageResponse>.NotFound("MEDIA_NOT_FOUND", "Media file was not found.");
        }

        if (mediaFile.FileType != MediaFileTypes.NormalImage)
        {
            return PropertyImageServiceResult<PropertyImageResponse>.Validation("Only NormalImage media can be linked as a property image.");
        }

        var hasImages = await db.PropertyImages.AnyAsync(
            propertyImage => propertyImage.TenantId == tenantId
                && propertyImage.PropertyId == propertyId
                && propertyImage.DeletedAt == null,
            cancellationToken);
        var isCover = request.IsCover == true || !hasImages;
        var now = DateTimeOffset.UtcNow;
        var propertyImage = new PropertyImage
        {
            TenantId = tenantId,
            PropertyId = propertyId,
            MediaFileId = mediaFile.Id,
            SortOrder = request.SortOrder ?? 0,
            IsCover = isCover,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (isCover)
        {
            await UnsetCoverImagesAsync(propertyId, tenantId, now, cancellationToken);
        }

        db.PropertyImages.Add(propertyImage);
        await db.SaveChangesAsync(cancellationToken);
        propertyImage.MediaFile = mediaFile;

        return PropertyImageServiceResult<PropertyImageResponse>.Success(ToResponse(propertyImage));
    }

    public async Task<PropertyImageServiceResult<IReadOnlyList<PropertyImageResponse>>> ReorderAsync(
        Guid propertyId,
        ReorderPropertyImagesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Images is null || request.Images.Count == 0)
        {
            return PropertyImageServiceResult<IReadOnlyList<PropertyImageResponse>>.Validation("Images are required.");
        }

        if (request.Images.Any(image => image.SortOrder < 0))
        {
            return PropertyImageServiceResult<IReadOnlyList<PropertyImageResponse>>.Validation("SortOrder cannot be negative.");
        }

        var tenantId = tenantContext.TenantId;

        if (!await PropertyExistsAsync(propertyId, tenantId, cancellationToken))
        {
            return PropertyImageServiceResult<IReadOnlyList<PropertyImageResponse>>.NotFound("PROPERTY_NOT_FOUND", "Property was not found.");
        }

        var imageIds = request.Images.Select(image => image.PropertyImageId).ToHashSet();
        var propertyImages = await db.PropertyImages
            .Where(propertyImage => propertyImage.TenantId == tenantId
                && propertyImage.PropertyId == propertyId
                && propertyImage.DeletedAt == null
                && imageIds.Contains(propertyImage.Id))
            .ToListAsync(cancellationToken);

        if (propertyImages.Count != imageIds.Count)
        {
            return PropertyImageServiceResult<IReadOnlyList<PropertyImageResponse>>.NotFound("PROPERTY_IMAGE_NOT_FOUND", "Property image was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var sortOrders = request.Images.ToDictionary(image => image.PropertyImageId, image => image.SortOrder);

        foreach (var propertyImage in propertyImages)
        {
            propertyImage.SortOrder = sortOrders[propertyImage.Id];
            propertyImage.UpdatedAt = now;
        }

        await db.SaveChangesAsync(cancellationToken);

        var images = await ListAsync(propertyId, cancellationToken);

        return PropertyImageServiceResult<IReadOnlyList<PropertyImageResponse>>.Success(images ?? []);
    }

    public async Task<PropertyImageServiceResult<PropertyImageResponse>> SetCoverAsync(
        Guid propertyId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var propertyImage = await FindPropertyImageAsync(propertyId, imageId, tenantId, cancellationToken);

        if (propertyImage is null)
        {
            return PropertyImageServiceResult<PropertyImageResponse>.NotFound("PROPERTY_IMAGE_NOT_FOUND", "Property image was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        await UnsetCoverImagesAsync(propertyId, tenantId, now, cancellationToken);

        propertyImage.IsCover = true;
        propertyImage.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return PropertyImageServiceResult<PropertyImageResponse>.Success(ToResponse(propertyImage));
    }

    public async Task<bool> SoftDeleteAsync(
        Guid propertyId,
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var propertyImage = await FindPropertyImageAsync(propertyId, imageId, tenantId, cancellationToken);

        if (propertyImage is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var wasCover = propertyImage.IsCover;
        propertyImage.DeletedAt = now;
        propertyImage.UpdatedAt = now;
        propertyImage.IsCover = false;

        if (wasCover)
        {
            var nextImage = await db.PropertyImages
                .Where(image => image.TenantId == tenantId
                    && image.PropertyId == propertyId
                    && image.Id != imageId
                    && image.DeletedAt == null)
                .OrderBy(image => image.SortOrder)
                .ThenBy(image => image.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (nextImage is not null)
            {
                nextImage.IsCover = true;
                nextImage.UpdatedAt = now;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<bool> PropertyExistsAsync(Guid propertyId, Guid tenantId, CancellationToken cancellationToken)
    {
        return await db.Properties.AnyAsync(
            property => property.Id == propertyId
                && property.TenantId == tenantId
                && property.DeletedAt == null,
            cancellationToken);
    }

    private async Task<PropertyImage?> FindPropertyImageAsync(
        Guid propertyId,
        Guid imageId,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        return await db.PropertyImages
            .Include(propertyImage => propertyImage.MediaFile)
            .SingleOrDefaultAsync(
                propertyImage => propertyImage.Id == imageId
                    && propertyImage.TenantId == tenantId
                    && propertyImage.PropertyId == propertyId
                    && propertyImage.DeletedAt == null,
                cancellationToken);
    }

    private async Task UnsetCoverImagesAsync(
        Guid propertyId,
        Guid tenantId,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var currentCovers = await db.PropertyImages
            .Where(propertyImage => propertyImage.TenantId == tenantId
                && propertyImage.PropertyId == propertyId
                && propertyImage.IsCover
                && propertyImage.DeletedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var currentCover in currentCovers)
        {
            currentCover.IsCover = false;
            currentCover.UpdatedAt = updatedAt;
        }
    }

    private static PropertyImageResponse ToResponse(PropertyImage propertyImage)
    {
        return new PropertyImageResponse(
            propertyImage.Id,
            propertyImage.PropertyId,
            propertyImage.MediaFileId,
            propertyImage.MediaFile.Url,
            propertyImage.MediaFile.OriginalFileName,
            propertyImage.MediaFile.MimeType,
            propertyImage.MediaFile.SizeBytes,
            propertyImage.MediaFile.Width,
            propertyImage.MediaFile.Height,
            propertyImage.SortOrder,
            propertyImage.IsCover,
            propertyImage.CreatedAt);
    }
}

public sealed record PropertyImageServiceResult<T>(T? Data, string? ErrorCode, string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode is null;

    public static PropertyImageServiceResult<T> Success(T data)
    {
        return new PropertyImageServiceResult<T>(data, null, null);
    }

    public static PropertyImageServiceResult<T> NotFound(string code, string message)
    {
        return new PropertyImageServiceResult<T>(default, code, message);
    }

    public static PropertyImageServiceResult<T> Validation(string message)
    {
        return new PropertyImageServiceResult<T>(default, "VALIDATION_ERROR", message);
    }
}
