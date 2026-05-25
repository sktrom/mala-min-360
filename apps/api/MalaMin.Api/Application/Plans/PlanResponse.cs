namespace MalaMin.Api.Application.Plans;

public sealed record PlanResponse(
    Guid Id,
    string Name,
    string Code,
    int MaxProperties,
    int MaxTours,
    long StorageLimitMb,
    decimal MonthlyPrice,
    bool IsActive);
