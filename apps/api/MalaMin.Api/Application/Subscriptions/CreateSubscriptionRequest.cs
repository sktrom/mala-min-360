namespace MalaMin.Api.Application.Subscriptions;

public sealed record CreateSubscriptionRequest(
    Guid TenantId,
    Guid PlanId,
    string Status,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt);
