namespace MalaMin.Api.Infrastructure.Storage;

public sealed class LocalStorageOptions
{
    public string RootPath { get; set; } = "storage/uploads";

    public string PublicBasePath { get; set; } = "/uploads";

    public long MaxFileSizeBytes { get; set; } = 10_485_760;
}
