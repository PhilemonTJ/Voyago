namespace Voyago.BookingService.DTOs.Invoices;

public class InvoiceResponseDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
}