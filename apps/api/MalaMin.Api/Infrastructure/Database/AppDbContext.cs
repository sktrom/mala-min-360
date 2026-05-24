using MalaMin.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

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
    }
}
