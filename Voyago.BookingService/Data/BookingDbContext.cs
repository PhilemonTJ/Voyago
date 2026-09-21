using Microsoft.EntityFrameworkCore;
using Voyago.BookingService.Models;

namespace Voyago.BookingService.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingSeat> BookingSeats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(booking => booking.Id);

            entity.Property(booking => booking.BookingReference)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(booking => booking.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(booking => booking.TotalAmount)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(booking => booking.CreatedAt)
                .IsRequired();

            entity.HasIndex(booking => booking.BookingReference)
                .IsUnique();

            entity.HasIndex(booking => booking.UserId);

            entity.HasIndex(booking => booking.ScheduleId);
        });

        modelBuilder.Entity<BookingSeat>(entity =>
        {
            entity.HasKey(bookingSeat => bookingSeat.Id);

            entity.Property(bookingSeat => bookingSeat.SeatNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(bookingSeat => bookingSeat.Price)
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(bookingSeat => bookingSeat.CreatedAt)
                .IsRequired();

            entity.HasOne(bookingSeat => bookingSeat.Booking)
                .WithMany(booking => booking.BookingSeats)
                .HasForeignKey(bookingSeat => bookingSeat.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(bookingSeat => new
            {
                bookingSeat.BookingId,
                bookingSeat.ScheduleSeatId
            })
            .IsUnique();

            entity.HasIndex(bookingSeat => bookingSeat.ScheduleSeatId);
        });
    }
}