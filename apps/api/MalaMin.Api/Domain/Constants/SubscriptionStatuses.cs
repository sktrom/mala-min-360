namespace MalaMin.Api.Domain.Constants;

public static class SubscriptionStatuses
{
    public const string Trial = "Trial";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Expired = "Expired";

    public static readonly string[] All = [Trial, Active, Suspended, Expired];
}
