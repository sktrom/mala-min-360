namespace MalaMin.Api.Application.Plans;

public sealed record CreatePlanRequest(
    string Name,
    string Code,
    int MaxProperties,
    int MaxTours,
    long StorageLimitMb,
    decimal MonthlyPrice);
