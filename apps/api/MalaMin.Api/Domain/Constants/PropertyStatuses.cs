namespace MalaMin.Api.Domain.Constants;

public static class PropertyStatuses
{
    public const string Available = "Available";
    public const string Reserved = "Reserved";
    public const string Sold = "Sold";
    public const string Rented = "Rented";

    public static readonly string[] All = [Available, Reserved, Sold, Rented];
}
