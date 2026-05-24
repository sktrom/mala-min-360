using MalaMin.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");

            entity.HasKey(tenant => tenant.Id);

            entity.Property(tenant => tenant.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(tenant => tenant.Slug)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(tenant => tenant.Slug)
                .IsUnique();

            entity.Property(tenant => tenant.Phone)
                .HasMaxLength(50);

            entity.Property(tenant => tenant.WhatsAppNumber)
                .HasMaxLength(50);

            entity.Property(tenant => tenant.City)
                .HasMaxLength(100);

            entity.Property(tenant => tenant.Status)
                .IsRequired()
                .HasMaxLength(30);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(user => user.Id);

            entity.Property(user => user.TenantId)
                .IsRequired();

            entity.Property(user => user.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.HasIndex(user => user.TenantId);

            entity.Property(user => user.Phone)
                .HasMaxLength(50);

            entity.Property(user => user.PasswordHash)
                .IsRequired();

            entity.Property(user => user.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(user => user.IsActive)
                .IsRequired();

            entity.Property(user => user.LastLoginAt)
                .IsRequired(false);

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.Property(user => user.UpdatedAt)
                .IsRequired();

            entity.HasOne(user => user.Tenant)
                .WithMany(tenant => tenant.Users)
                .HasForeignKey(user => user.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
