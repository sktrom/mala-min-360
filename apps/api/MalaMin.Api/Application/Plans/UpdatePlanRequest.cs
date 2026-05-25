namespace MalaMin.Api.Application.Plans;

public sealed record UpdatePlanRequest(
    string Name,
    int MaxProperties,
    int MaxTours,
    long StorageLimitMb,
    decimal MonthlyPrice,
    bool IsActive);
