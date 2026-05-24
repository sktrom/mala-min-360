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

        await db.SaveChangesAsync(cancellationToken);
    }
}
