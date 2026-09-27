using System.ComponentModel.DataAnnotations;

namespace Voyago.BookingService.Models;

public class Invoice
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid BookingId { get; set; }

    [Required, MaxLength(30)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTimeOffset IssuedAt { get; set; }

    public Booking Booking { get; set; } = null!;
}