using Microsoft.EntityFrameworkCore;
using Voyago.BusService.Models;

namespace Voyago.BusService.Data;

public class BusDbContext : DbContext
{
    public BusDbContext(DbContextOptions<BusDbContext> options): base(options)
    {
    }

    public DbSet<Bus> Buses => Set<Bus>();

    public DbSet<Seat> Seats => Set<Seat>();

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

            entity.Property(bus => bus.TotalRows)
                .IsRequired();

            entity.Property(bus => bus.TotalColumns)
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

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(seat => seat.Id);

            entity.Property(seat => seat.SeatNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(seat => seat.SeatType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(seat => seat.Level)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(seat => seat.RowNumber)
                .IsRequired();

            entity.Property(seat => seat.ColumnNumber)
                .IsRequired();

            entity.Property(seat => seat.IsActive)
                .IsRequired();

            entity.HasIndex(
                    seat => new
                    {
                        seat.BusId,
                        seat.SeatNumber
                    })
                .IsUnique();

            entity.HasIndex(seat => new
                    {
                        seat.BusId,
                        seat.Level,
                        seat.RowNumber,
                        seat.ColumnNumber
                    })
                .IsUnique();

            entity.HasOne(seat => seat.Bus)
                .WithMany(bus => bus.Seats)
                .HasForeignKey(seat => seat.BusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}