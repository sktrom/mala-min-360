using MalaMin.Api.Application.Common;
using MalaMin.Api.Domain.Constants;
using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Database;
using MalaMin.Api.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace MalaMin.Api.Application.Media;

public sealed class MediaService(
    AppDbContext db,
    ITenantContext tenantContext,
    LocalMediaStorageService storageService,
    IOptions<LocalStorageOptions> storageOptions)
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp"];

    public async Task<MediaServiceResult<MediaFileResponse>> UploadAsync(
        IFormFile file,
        string fileType,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateUpload(file, fileType);

        if (validationError is not null)
        {
            return MediaServiceResult<MediaFileResponse>.Validation(validationError);
        }

        var tenantId = tenantContext.TenantId;
        var storedFile = await storageService.SaveAsync(file, tenantId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var mediaFile = new MediaFile
        {
            TenantId = tenantId,
            Url = storedFile.Url,
            StorageKey = storedFile.StorageKey,
            FileType = fileType.Trim(),
            OriginalFileName = storedFile.OriginalFileName,
            MimeType = storedFile.MimeType,
            SizeBytes = storedFile.SizeBytes,
            ProcessingStatus = MediaProcessingStatuses.Completed,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.MediaFiles.Add(mediaFile);
        await db.SaveChangesAsync(cancellationToken);

        return MediaServiceResult<MediaFileResponse>.Success(ToResponse(mediaFile));
    }

    public async Task<IReadOnlyList<MediaFileResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;

        return await db.MediaFiles
            .AsNoTracking()
            .Where(mediaFile => mediaFile.TenantId == tenantId && mediaFile.DeletedAt == null)
            .OrderByDescending(mediaFile => mediaFile.CreatedAt)
            .Select(mediaFile => new MediaFileResponse(
                mediaFile.Id,
                mediaFile.Url,
                mediaFile.StorageKey,
                mediaFile.FileType,
                mediaFile.OriginalFileName,
                mediaFile.MimeType,
                mediaFile.SizeBytes,
                mediaFile.Width,
                mediaFile.Height,
                mediaFile.ProcessingStatus,
                mediaFile.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<MediaFileResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mediaFile = await FindCurrentTenantMediaFileAsync(id, cancellationToken);

        return mediaFile is null ? null : ToResponse(mediaFile);
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mediaFile = await FindCurrentTenantMediaFileAsync(id, cancellationToken);

        if (mediaFile is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        mediaFile.DeletedAt = now;
        mediaFile.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private string? ValidateUpload(IFormFile file, string fileType)
    {
        if (file.Length <= 0)
        {
            return "Uploaded file is required.";
        }

        if (file.Length > storageOptions.Value.MaxFileSizeBytes)
        {
            return "Uploaded file is too large.";
        }

        if (!MediaFileTypes.All.Contains(fileType))
        {
            return "FileType is not valid.";
        }

        if (!AllowedMimeTypes.Contains(file.ContentType))
        {
            return "MimeType is not allowed.";
        }

        return null;
    }

    private async Task<MediaFile?> FindCurrentTenantMediaFileAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;

        return await db.MediaFiles
            .SingleOrDefaultAsync(
                mediaFile => mediaFile.Id == id
                    && mediaFile.TenantId == tenantId
                    && mediaFile.DeletedAt == null,
                cancellationToken);
    }

    private static MediaFileResponse ToResponse(MediaFile mediaFile)
    {
        return new MediaFileResponse(
            mediaFile.Id,
            mediaFile.Url,
            mediaFile.StorageKey,
            mediaFile.FileType,
            mediaFile.OriginalFileName,
            mediaFile.MimeType,
            mediaFile.SizeBytes,
            mediaFile.Width,
            mediaFile.Height,
            mediaFile.ProcessingStatus,
            mediaFile.CreatedAt);
    }
}

public sealed record MediaServiceResult<T>(T? Data, string? ErrorCode, string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode is null;

    public static MediaServiceResult<T> Success(T data)
    {
        return new MediaServiceResult<T>(data, null, null);
    }

    public static MediaServiceResult<T> Validation(string message)
    {
        return new MediaServiceResult<T>(default, "VALIDATION_ERROR", message);
    }
}
