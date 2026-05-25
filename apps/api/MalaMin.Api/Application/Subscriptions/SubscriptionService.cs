using MalaMin.Api.Application.Common;
using MalaMin.Api.Application.Plans;
using MalaMin.Api.Domain.Constants;
using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.Subscriptions;

public sealed class SubscriptionService(AppDbContext db, ITenantContext tenantContext)
{
    private const long BytesPerMegabyte = 1024 * 1024;

    public async Task<IReadOnlyList<SubscriptionResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await db.Subscriptions
            .AsNoTracking()
            .Include(subscription => subscription.Tenant)
            .Include(subscription => subscription.Plan)
            .OrderByDescending(subscription => subscription.CreatedAt)
            .Select(subscription => ToResponse(subscription))
            .ToListAsync(cancellationToken);
    }

    public async Task<SubscriptionServiceResult<SubscriptionResponse>> CreateAsync(
        CreateSubscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateStatusAndDates(request.Status, request.StartsAt, request.EndsAt);

        if (validationError is not null)
        {
            return SubscriptionServiceResult<SubscriptionResponse>.Validation(validationError);
        }

        var tenantExists = await db.Tenants.AnyAsync(tenant => tenant.Id == request.TenantId, cancellationToken);

        if (!tenantExists)
        {
            return SubscriptionServiceResult<SubscriptionResponse>.NotFound("TENANT_NOT_FOUND", "Tenant was not found.");
        }

        var planExists = await db.Plans.AnyAsync(plan => plan.Id == request.PlanId, cancellationToken);

        if (!planExists)
        {
            return SubscriptionServiceResult<SubscriptionResponse>.NotFound("PLAN_NOT_FOUND", "Plan was not found.");
        }

        var now = DateTimeOffset.UtcNow;
        var subscription = new Subscription
        {
            TenantId = request.TenantId,
            PlanId = request.PlanId,
            Status = request.Status.Trim(),
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync(cancellationToken);

        subscription = await FindSubscriptionWithDetailsAsync(subscription.Id, cancellationToken)
            ?? subscription;

        return SubscriptionServiceResult<SubscriptionResponse>.Success(ToResponse(subscription));
    }

    public async Task<SubscriptionServiceResult<SubscriptionResponse>> UpdateStatusAsync(
        Guid id,
        UpdateSubscriptionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!SubscriptionStatuses.All.Contains(request.Status))
        {
            return SubscriptionServiceResult<SubscriptionResponse>.Validation("Status is not valid.");
        }

        var subscription = await db.Subscriptions
            .Include(existingSubscription => existingSubscription.Tenant)
            .Include(existingSubscription => existingSubscription.Plan)
            .SingleOrDefaultAsync(existingSubscription => existingSubscription.Id == id, cancellationToken);

        if (subscription is null)
        {
            return SubscriptionServiceResult<SubscriptionResponse>.NotFound("SUBSCRIPTION_NOT_FOUND", "Subscription was not found.");
        }

        subscription.Status = request.Status.Trim();
        subscription.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return SubscriptionServiceResult<SubscriptionResponse>.Success(ToResponse(subscription));
    }

    public async Task<CurrentSubscriptionResponse?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = tenantContext.TenantId;
        var subscription = await GetCurrentSubscriptionQuery(tenantId)
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (subscription is null)
        {
            return null;
        }

        var currentProperties = await db.Properties.CountAsync(
            property => property.TenantId == tenantId && property.DeletedAt == null,
            cancellationToken);

        var currentTours = await db.TourRooms.CountAsync(
            room => room.TenantId == tenantId && room.DeletedAt == null,
            cancellationToken);

        var currentStorageBytes = await db.MediaFiles
            .Where(mediaFile => mediaFile.TenantId == tenantId && mediaFile.DeletedAt == null)
            .SumAsync(mediaFile => (long?)mediaFile.SizeBytes, cancellationToken)
            ?? 0;

        var currentStorageMb = currentStorageBytes == 0
            ? 0
            : (currentStorageBytes + BytesPerMegabyte - 1) / BytesPerMegabyte;

        return new CurrentSubscriptionResponse(
            subscription.Status,
            subscription.StartsAt,
            subscription.EndsAt,
            PlanService.ToResponse(subscription.Plan),
            currentProperties,
            currentTours,
            currentStorageMb);
    }

    public async Task<int?> GetCurrentMaxPropertiesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await GetCurrentSubscriptionQuery(tenantId)
            .AsNoTracking()
            .Select(subscription => (int?)subscription.Plan.MaxProperties)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private IQueryable<Subscription> GetCurrentSubscriptionQuery(Guid tenantId)
    {
        var now = DateTimeOffset.UtcNow;

        return db.Subscriptions
            .Include(subscription => subscription.Plan)
            .Where(subscription => subscription.TenantId == tenantId
                && subscription.StartsAt <= now
                && subscription.EndsAt >= now
                && (subscription.Status == SubscriptionStatuses.Trial
                    || subscription.Status == SubscriptionStatuses.Active))
            .OrderByDescending(subscription => subscription.EndsAt)
            .Take(1);
    }

    private async Task<Subscription?> FindSubscriptionWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await db.Subscriptions
            .Include(subscription => subscription.Tenant)
            .Include(subscription => subscription.Plan)
            .SingleOrDefaultAsync(subscription => subscription.Id == id, cancellationToken);
    }

    private static string? ValidateStatusAndDates(string status, DateTimeOffset startsAt, DateTimeOffset endsAt)
    {
        if (!SubscriptionStatuses.All.Contains(status))
        {
            return "Status is not valid.";
        }

        if (endsAt <= startsAt)
        {
            return "EndsAt must be after StartsAt.";
        }

        return null;
    }

    private static SubscriptionResponse ToResponse(Subscription subscription)
    {
        return new SubscriptionResponse(
            subscription.Id,
            subscription.TenantId,
            subscription.Tenant.Name,
            subscription.PlanId,
            subscription.Plan.Name,
            subscription.Plan.Code,
            subscription.Status,
            subscription.StartsAt,
            subscription.EndsAt);
    }
}

public sealed record SubscriptionServiceResult<T>(T? Data, string? ErrorCode, string? ErrorMessage)
{
    public bool IsSuccess => ErrorCode is null;

    public static SubscriptionServiceResult<T> Success(T data)
    {
        return new SubscriptionServiceResult<T>(data, null, null);
    }

    public static SubscriptionServiceResult<T> NotFound(string code, string message)
    {
        return new SubscriptionServiceResult<T>(default, code, message);
    }

    public static SubscriptionServiceResult<T> Validation(string message)
    {
        return new SubscriptionServiceResult<T>(default, "VALIDATION_ERROR", message);
    }
}
