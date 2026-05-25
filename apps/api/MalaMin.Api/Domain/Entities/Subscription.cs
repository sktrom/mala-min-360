using MalaMin.Api.Domain.Constants;

namespace MalaMin.Api.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid PlanId { get; set; }

    public Plan Plan { get; set; } = null!;

    public string Status { get; set; } = SubscriptionStatuses.Trial;

    public DateTimeOffset StartsAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset EndsAt { get; set; } = DateTimeOffset.UtcNow.AddDays(14);

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
