using Microsoft.Extensions.Options;

namespace MalaMin.Api.Infrastructure.Storage;

public sealed class LocalMediaStorageService(
    IWebHostEnvironment environment,
    IOptions<LocalStorageOptions> options)
{
    private readonly LocalStorageOptions options = options.Value;

    public async Task<StoredMediaFile> SaveAsync(
        IFormFile file,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var rootPath = GetStorageRootPath();
        var year = DateTimeOffset.UtcNow.ToString("yyyy");
        var month = DateTimeOffset.UtcNow.ToString("MM");
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var relativeDirectory = Path.Combine(tenantId.ToString(), year, month);
        var targetDirectory = Path.GetFullPath(Path.Combine(rootPath, relativeDirectory));
        var targetPath = Path.GetFullPath(Path.Combine(targetDirectory, fileName));

        if (!targetPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid storage path.");
        }

        Directory.CreateDirectory(targetDirectory);

        await using var output = File.Create(targetPath);
        await file.CopyToAsync(output, cancellationToken);

        var storageKey = Path.Combine(relativeDirectory, fileName).Replace('\\', '/');
        var publicBasePath = options.PublicBasePath.TrimEnd('/');

        return new StoredMediaFile(
            Url: $"{publicBasePath}/{storageKey}",
            StorageKey: storageKey,
            OriginalFileName: Path.GetFileName(file.FileName),
            MimeType: file.ContentType,
            SizeBytes: file.Length);
    }

    public string GetStorageRootPath()
    {
        var configuredRoot = options.RootPath;
        var rootPath = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);

        return Path.GetFullPath(rootPath);
    }
}

public sealed record StoredMediaFile(
    string Url,
    string StorageKey,
    string OriginalFileName,
    string MimeType,
    long SizeBytes);
