using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.Plans;

public sealed class PlanService(AppDbContext db)
{
    public async Task<IReadOnlyList<PlanResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await db.Plans
            .AsNoTracking()
            .OrderBy(plan => plan.MonthlyPrice)
            .ThenBy(plan => plan.Name)
            .Select(plan => ToResponse(plan))
            .ToListAsync(cancellationToken);
    }

    public async Task<PlanResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.Plans
            .AsNoTracking()
            .Where(plan => plan.Id == id)
            .Select(plan => ToResponse(plan))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PlanServiceResult<PlanResponse>> CreateAsync(
        CreatePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(request.Name, request.Code, request.MaxProperties, request.MaxTours, request.StorageLimitMb, request.MonthlyPrice);

        if (validationError is not null)
        {
            return PlanServiceResult<PlanResponse>.Validation(validationError);
        }

        var code = NormalizeCode(request.Code);
        var codeExists = await db.Plans.AnyAsync(plan => plan.Code == code, cancellationToken);

        if (codeExists)
        {
            return PlanServiceResult<PlanResponse>.Validation("Plan code must be unique.");
        }

        var now = DateTimeOffset.UtcNow;
        var plan = new Plan
        {
            Name = request.Name.Trim(),
            Code = code,
            MaxProperties = request.MaxProperties,
            MaxTours = request.MaxTours,
            StorageLimitMb = request.StorageLimitMb,
            MonthlyPrice = request.MonthlyPrice,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Plans.Add(plan);
        await db.SaveChangesAsync(cancellationToken);

        return PlanServiceResult<PlanResponse>.Success(ToResponse(plan));
    }

    public async Task<PlanServiceResult<PlanResponse>> UpdateAsync(
        Guid id,
        UpdatePlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = Validate(request.Name, "existing", request.MaxProperties, request.MaxTours, request.StorageLimitMb, request.MonthlyPrice);

        if (validationError is not null)
        {
            return PlanServiceResult<PlanResponse>.Validation(validationError);
        }

        var plan = await db.Plans.SingleOrDefaultAsync(existingPlan => existingPlan.Id == id, cancellationToken);

        if (plan is null)
        {
            return PlanServiceResult<PlanResponse>.NotFound();
        }

        plan.Name = request.Name.Trim();
        plan.MaxProperties = request.MaxProperties;
        plan.MaxTours = request.MaxTours;
        plan.StorageLimitMb = request.StorageLimitMb;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.IsActive = request.IsActive;
        plan.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return PlanServiceResult<PlanResponse>.Success(ToResponse(plan));
    }

    public static PlanResponse ToResponse(Plan plan)
    {
        return new PlanResponse(
            plan.Id,
            plan.Name,
            plan.Code,
            plan.MaxProperties,
            plan.MaxTours,
            plan.StorageLimitMb,
            plan.MonthlyPrice,
            plan.IsActive);
    }

    private static string? Validate(
        string name,
        string code,
        int maxProperties,
        int maxTours,
        long storageLimitMb,
        decimal monthlyPrice)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            return "Name is required and must be 100 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(code) || code.Length > 50)
        {
            return "Code is required and must be 50 characters or fewer.";
        }

        if (maxProperties < 0)
        {
            return "MaxProperties must be greater than or equal to 0.";
        }

        if (maxTours < 0)
        {
            return "MaxTours must be greater than or equal to 0.";
        }

        if (storageLimitMb < 0)
        {
            return "StorageLimitMb must be greater than or equal to 0.";
        }

        if (monthlyPrice < 0)
        {
            return "MonthlyPrice must be greater than or equal to 0.";
        }

        return null;
    }

    private static string NormalizeCode(string code)
    {
        return code.Trim().ToLowerInvariant();
    }
}

public sealed record PlanServiceResult<T>(T? Data, string? ErrorCode, string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode is null;

    public static PlanServiceResult<T> Success(T data)
    {
        return new PlanServiceResult<T>(data, null, null);
    }

    public static PlanServiceResult<T> NotFound()
    {
        return new PlanServiceResult<T>(default, "PLAN_NOT_FOUND", "Plan was not found.");
    }

    public static PlanServiceResult<T> Validation(string message)
    {
        return new PlanServiceResult<T>(default, "VALIDATION_ERROR", message);
    }
}
