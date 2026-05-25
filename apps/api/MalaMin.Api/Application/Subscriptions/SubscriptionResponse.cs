namespace MalaMin.Api.Application.Subscriptions;

public sealed record SubscriptionResponse(
    Guid Id,
    Guid TenantId,
    string TenantName,
    Guid PlanId,
    string PlanName,
    string PlanCode,
    string Status,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
