using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Models;

namespace Voyago.BusService.Data;

public class BusDbContext : DbContext
{
    public BusDbContext(DbContextOptions<BusDbContext> options): base(options)
    {
    }

    public DbSet<Bus> Buses => Set<Bus>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(bus => bus.Id);

            entity.Property(bus => bus.RegistrationNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(bus => bus.BusNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(bus => bus.BusName)
                .HasMaxLength(100);

            entity.Property(bus => bus.BusType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(bus => bus.TotalSeats)
                .IsRequired();

            entity.Property(bus => bus.ImageUrl);

            entity.Property(bus => bus.IsActive)
                .IsRequired();

            entity.Property(bus => bus.CreatedAt)
                .IsRequired();

            entity.Property(bus => bus.UpdatedAt);

            entity.HasIndex(bus => bus.RegistrationNumber)
                .IsUnique();

            entity.HasIndex(
                    bus => new
                    {
                        bus.OperatorId,
                        bus.BusNumber
                    })
                .IsUnique();

            entity.HasIndex(bus => bus.OperatorId);
        });
    }
}