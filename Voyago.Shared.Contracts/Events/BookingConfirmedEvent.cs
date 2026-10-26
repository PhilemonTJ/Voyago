namespace Voyago.Shared.Contracts.Events;

public class BookingConfirmedEvent
{
    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public Guid ScheduleId { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTimeOffset PaidAt { get; set; }

    public List<Guid> ScheduleSeatIds { get; set; } = new();
}
