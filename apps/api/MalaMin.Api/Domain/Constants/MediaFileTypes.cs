namespace MalaMin.Api.Domain.Constants;

public static class MediaFileTypes
{
    public const string NormalImage = "NormalImage";
    public const string Panorama360 = "Panorama360";
    public const string Logo = "Logo";
    public const string Other = "Other";

    public static readonly string[] All = [NormalImage, Panorama360, Logo, Other];
}
