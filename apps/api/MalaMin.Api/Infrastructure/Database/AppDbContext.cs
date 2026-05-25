using MalaMin.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Property> Properties => Set<Property>();

    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

    public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>();

    public DbSet<TourRoom> TourRooms => Set<TourRoom>();

    public DbSet<TourHotspot> TourHotspots => Set<TourHotspot>();

    public DbSet<PropertyStats> PropertyStats => Set<PropertyStats>();

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

        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Properties");

            entity.HasKey(property => property.Id);

            entity.Property(property => property.TenantId)
                .IsRequired();

            entity.Property(property => property.Title)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(property => property.Slug)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(property => property.Description)
                .IsRequired(false);

            entity.Property(property => property.City)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(property => property.AreaName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(property => property.AddressText)
                .IsRequired(false);

            entity.Property(property => property.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(property => property.Currency)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(property => property.ListingType)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(property => property.PropertyType)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(property => property.Bedrooms)
                .IsRequired(false);

            entity.Property(property => property.Bathrooms)
                .IsRequired(false);

            entity.Property(property => property.FloorNumber)
                .IsRequired(false);

            entity.Property(property => property.AreaSqm)
                .IsRequired();

            entity.Property(property => property.Status)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(property => property.IsPublished)
                .IsRequired();

            entity.Property(property => property.CreatedAt)
                .IsRequired();

            entity.Property(property => property.UpdatedAt)
                .IsRequired();

            entity.Property(property => property.DeletedAt)
                .IsRequired(false);

            entity.HasIndex(property => property.TenantId);
            entity.HasIndex(property => property.Slug);
            entity.HasIndex(property => new { property.TenantId, property.Slug })
                .IsUnique();
            entity.HasIndex(property => property.Status);
            entity.HasIndex(property => property.IsPublished);

            entity.HasOne(property => property.Tenant)
                .WithMany(tenant => tenant.Properties)
                .HasForeignKey(property => property.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MediaFile>(entity =>
        {
            entity.ToTable("MediaFiles");

            entity.HasKey(mediaFile => mediaFile.Id);

            entity.Property(mediaFile => mediaFile.TenantId)
                .IsRequired();

            entity.Property(mediaFile => mediaFile.Url)
                .IsRequired();

            entity.Property(mediaFile => mediaFile.StorageKey)
                .IsRequired();

            entity.Property(mediaFile => mediaFile.FileType)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(mediaFile => mediaFile.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(mediaFile => mediaFile.MimeType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(mediaFile => mediaFile.SizeBytes)
                .IsRequired();

            entity.Property(mediaFile => mediaFile.Width)
                .IsRequired(false);

            entity.Property(mediaFile => mediaFile.Height)
                .IsRequired(false);

            entity.Property(mediaFile => mediaFile.ProcessingStatus)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(mediaFile => mediaFile.CreatedAt)
                .IsRequired();

            entity.Property(mediaFile => mediaFile.UpdatedAt)
                .IsRequired();

            entity.Property(mediaFile => mediaFile.DeletedAt)
                .IsRequired(false);

            entity.HasIndex(mediaFile => mediaFile.TenantId);
            entity.HasIndex(mediaFile => mediaFile.FileType);
            entity.HasIndex(mediaFile => mediaFile.ProcessingStatus);

            entity.HasOne(mediaFile => mediaFile.Tenant)
                .WithMany(tenant => tenant.MediaFiles)
                .HasForeignKey(mediaFile => mediaFile.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PropertyImage>(entity =>
        {
            entity.ToTable("PropertyImages");

            entity.HasKey(propertyImage => propertyImage.Id);

            entity.Property(propertyImage => propertyImage.TenantId)
                .IsRequired();

            entity.Property(propertyImage => propertyImage.PropertyId)
                .IsRequired();

            entity.Property(propertyImage => propertyImage.MediaFileId)
                .IsRequired();

            entity.Property(propertyImage => propertyImage.SortOrder)
                .IsRequired();

            entity.Property(propertyImage => propertyImage.IsCover)
                .IsRequired();

            entity.Property(propertyImage => propertyImage.CreatedAt)
                .IsRequired();

            entity.Property(propertyImage => propertyImage.UpdatedAt)
                .IsRequired();

            entity.Property(propertyImage => propertyImage.DeletedAt)
                .IsRequired(false);

            entity.HasIndex(propertyImage => propertyImage.TenantId);
            entity.HasIndex(propertyImage => propertyImage.PropertyId);
            entity.HasIndex(propertyImage => propertyImage.MediaFileId);
            entity.HasIndex(propertyImage => new { propertyImage.PropertyId, propertyImage.SortOrder });
            entity.HasIndex(propertyImage => new { propertyImage.PropertyId, propertyImage.IsCover });

            entity.HasOne(propertyImage => propertyImage.Property)
                .WithMany(property => property.Images)
                .HasForeignKey(propertyImage => propertyImage.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(propertyImage => propertyImage.MediaFile)
                .WithMany(mediaFile => mediaFile.PropertyImages)
                .HasForeignKey(propertyImage => propertyImage.MediaFileId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TourRoom>(entity =>
        {
            entity.ToTable("TourRooms");

            entity.HasKey(tourRoom => tourRoom.Id);

            entity.Property(tourRoom => tourRoom.TenantId)
                .IsRequired();

            entity.Property(tourRoom => tourRoom.PropertyId)
                .IsRequired();

            entity.Property(tourRoom => tourRoom.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(tourRoom => tourRoom.PanoramaMediaId)
                .IsRequired();

            entity.Property(tourRoom => tourRoom.SortOrder)
                .IsRequired();

            entity.Property(tourRoom => tourRoom.IsStartRoom)
                .IsRequired();

            entity.Property(tourRoom => tourRoom.CreatedAt)
                .IsRequired();

            entity.Property(tourRoom => tourRoom.UpdatedAt)
                .IsRequired();

            entity.Property(tourRoom => tourRoom.DeletedAt)
                .IsRequired(false);

            entity.HasIndex(tourRoom => tourRoom.TenantId);
            entity.HasIndex(tourRoom => tourRoom.PropertyId);
            entity.HasIndex(tourRoom => tourRoom.PanoramaMediaId);
            entity.HasIndex(tourRoom => new { tourRoom.PropertyId, tourRoom.SortOrder });
            entity.HasIndex(tourRoom => new { tourRoom.PropertyId, tourRoom.IsStartRoom });

            entity.HasOne(tourRoom => tourRoom.Property)
                .WithMany(property => property.TourRooms)
                .HasForeignKey(tourRoom => tourRoom.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(tourRoom => tourRoom.PanoramaMedia)
                .WithMany(mediaFile => mediaFile.TourRooms)
                .HasForeignKey(tourRoom => tourRoom.PanoramaMediaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TourHotspot>(entity =>
        {
            entity.ToTable("TourHotspots");

            entity.HasKey(hotspot => hotspot.Id);

            entity.Property(hotspot => hotspot.TenantId)
                .IsRequired();

            entity.Property(hotspot => hotspot.RoomId)
                .IsRequired();

            entity.Property(hotspot => hotspot.TargetRoomId)
                .IsRequired(false);

            entity.Property(hotspot => hotspot.Type)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(hotspot => hotspot.Label)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(hotspot => hotspot.Yaw)
                .IsRequired()
                .HasPrecision(10, 6);

            entity.Property(hotspot => hotspot.Pitch)
                .IsRequired()
                .HasPrecision(10, 6);

            entity.Property(hotspot => hotspot.CreatedAt)
                .IsRequired();

            entity.Property(hotspot => hotspot.UpdatedAt)
                .IsRequired();

            entity.Property(hotspot => hotspot.DeletedAt)
                .IsRequired(false);

            entity.HasIndex(hotspot => hotspot.TenantId);
            entity.HasIndex(hotspot => hotspot.RoomId);
            entity.HasIndex(hotspot => hotspot.TargetRoomId);
            entity.HasIndex(hotspot => hotspot.Type);

            entity.HasOne(hotspot => hotspot.Room)
                .WithMany(room => room.Hotspots)
                .HasForeignKey(hotspot => hotspot.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(hotspot => hotspot.TargetRoom)
                .WithMany(room => room.IncomingHotspots)
                .HasForeignKey(hotspot => hotspot.TargetRoomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PropertyStats>(entity =>
        {
            entity.ToTable("PropertyStats");

            entity.HasKey(stats => stats.Id);

            entity.Property(stats => stats.TenantId)
                .IsRequired();

            entity.Property(stats => stats.PropertyId)
                .IsRequired();

            entity.Property(stats => stats.StatDate)
                .IsRequired();

            entity.Property(stats => stats.Views)
                .IsRequired();

            entity.Property(stats => stats.TourViews)
                .IsRequired();

            entity.Property(stats => stats.WhatsAppClicks)
                .IsRequired();

            entity.Property(stats => stats.QrScans)
                .IsRequired();

            entity.Property(stats => stats.CreatedAt)
                .IsRequired();

            entity.Property(stats => stats.UpdatedAt)
                .IsRequired();

            entity.HasIndex(stats => stats.TenantId);
            entity.HasIndex(stats => stats.PropertyId);
            entity.HasIndex(stats => stats.StatDate);
            entity.HasIndex(stats => new { stats.PropertyId, stats.StatDate })
                .IsUnique();

            entity.HasOne(stats => stats.Property)
                .WithMany(property => property.Stats)
                .HasForeignKey(stats => stats.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
