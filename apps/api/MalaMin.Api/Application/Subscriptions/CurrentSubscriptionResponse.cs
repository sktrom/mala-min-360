using MalaMin.Api.Application.Plans;

namespace MalaMin.Api.Application.Subscriptions;

public sealed record CurrentSubscriptionResponse(
    string Status,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    PlanResponse Plan,
    int CurrentProperties,
    int CurrentTours,
    long CurrentStorageMb);
