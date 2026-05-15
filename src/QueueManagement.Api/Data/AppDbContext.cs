using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Entities;

namespace QueueManagement.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<QueueLocation> QueueLocations => Set<QueueLocation>();
    public DbSet<UserLocation> UserLocations => Set<UserLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Name).HasMaxLength(100);
            entity.Property(user => user.Email).HasMaxLength(254);
            entity.Property(user => user.Mobile).HasMaxLength(30);
            entity.Property(user => user.PasswordHash).HasMaxLength(128);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => user.Mobile).IsUnique();
        });

        modelBuilder.Entity<QueueLocation>(entity =>
        {
            entity.Property(location => location.BusinessName).HasMaxLength(150);
            entity.Property(location => location.LocationName).HasMaxLength(150);
            entity.Property(location => location.Address).HasMaxLength(300);
            entity.Property(location => location.Mobile).HasMaxLength(30);
            entity.Property(location => location.LocationCode).HasMaxLength(12);
            entity.HasIndex(location => location.LocationCode).IsUnique();
        });

        modelBuilder.Entity<UserLocation>(entity =>
        {
            entity.Property(userLocation => userLocation.Role).HasMaxLength(20);
            entity.HasIndex(userLocation => new
            {
                userLocation.UserId,
                userLocation.QueueLocationId,
                userLocation.Role
            }).IsUnique();
        });
    }
}
