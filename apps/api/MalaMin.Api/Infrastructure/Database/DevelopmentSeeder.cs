using MalaMin.Api.Domain.Constants;
using MalaMin.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Infrastructure.Database;

public static class DevelopmentSeeder
{
    public const string DemoTenantSlug = "demo-agency";
    public const string DemoUserEmail = "owner@demo.local";
    public const string DemoUserPassword = "Demo12345!";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher<AppUser>>();
        var trialPlan = await EnsurePlanAsync(
            db,
            "Trial",
            "trial",
            maxProperties: 3,
            maxTours: 3,
            storageLimitMb: 500,
            monthlyPrice: 0,
            cancellationToken);

        await EnsurePlanAsync(
            db,
            "Starter",
            "starter",
            maxProperties: 25,
            maxTours: 25,
            storageLimitMb: 5120,
            monthlyPrice: 0,
            cancellationToken);

        await EnsurePlanAsync(
            db,
            "Pro",
            "pro",
            maxProperties: 100,
            maxTours: 100,
            storageLimitMb: 20480,
            monthlyPrice: 0,
            cancellationToken);

        await EnsurePlanAsync(
            db,
            "Business",
            "business",
            maxProperties: 300,
            maxTours: 300,
            storageLimitMb: 76800,
            monthlyPrice: 0,
            cancellationToken);

        var tenant = await db.Tenants
            .SingleOrDefaultAsync(existingTenant => existingTenant.Slug == DemoTenantSlug, cancellationToken);

        if (tenant is null)
        {
            tenant = new Tenant
            {
                Name = "Demo Real Estate Agency",
                Slug = DemoTenantSlug,
                City = "Damascus",
                Status = "Trial"
            };

            db.Tenants.Add(tenant);
        }

        var userExists = await db.Users
            .AnyAsync(existingUser => existingUser.Email == DemoUserEmail, cancellationToken);

        if (!userExists)
        {
            var user = new AppUser
            {
                Tenant = tenant,
                FullName = "Demo Owner",
                Email = DemoUserEmail,
                Role = UserRoles.TenantOwner,
                IsActive = true
            };

            user.PasswordHash = passwordHasher.HashPassword(user, DemoUserPassword);

            db.Users.Add(user);
        }

        var subscriptionExists = await db.Subscriptions
            .AnyAsync(existingSubscription => existingSubscription.TenantId == tenant.Id, cancellationToken);

        if (!subscriptionExists)
        {
            var now = DateTimeOffset.UtcNow;

            db.Subscriptions.Add(new Subscription
            {
                Tenant = tenant,
                Plan = trialPlan,
                Status = SubscriptionStatuses.Trial,
                StartsAt = now,
                EndsAt = now.AddDays(14),
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Plan> EnsurePlanAsync(
        AppDbContext db,
        string name,
        string code,
        int maxProperties,
        int maxTours,
        long storageLimitMb,
        decimal monthlyPrice,
        CancellationToken cancellationToken)
    {
        var plan = await db.Plans
            .SingleOrDefaultAsync(existingPlan => existingPlan.Code == code, cancellationToken);

        if (plan is not null)
        {
            return plan;
        }

        plan = new Plan
        {
            Name = name,
            Code = code,
            MaxProperties = maxProperties,
            MaxTours = maxTours,
            StorageLimitMb = storageLimitMb,
            MonthlyPrice = monthlyPrice,
            IsActive = true
        };

        db.Plans.Add(plan);

        return plan;
    }
}
