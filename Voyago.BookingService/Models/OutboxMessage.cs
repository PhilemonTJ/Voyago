using System.ComponentModel.DataAnnotations;

namespace Voyago.BookingService.Models;

public class OutboxMessage
{
    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public string Payload { get; set; } = string.Empty;

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
}