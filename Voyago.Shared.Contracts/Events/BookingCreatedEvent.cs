namespace Voyago.Shared.Contracts.Events;

public class BookingCreatedEvent
{
    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public Guid ScheduleId { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public List<Guid> ScheduleSeatIds { get; set; } = new();
}