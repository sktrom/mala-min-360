namespace MalaMin.Api.Domain.Entities;

public class Plan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public int MaxProperties { get; set; }

    public int MaxTours { get; set; }

    public long StorageLimitMb { get; set; }

    public decimal MonthlyPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Subscription> Subscriptions { get; set; } = [];
}
