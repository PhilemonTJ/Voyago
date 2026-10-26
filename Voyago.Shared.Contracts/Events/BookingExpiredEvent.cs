namespace Voyago.Shared.Contracts.Events;

public class BookingExpiredEvent
{
    public Guid BookingId { get; set; }

    public Guid UserId { get; set; }

    public Guid ScheduleId { get; set; }

    public string BookingReference { get; set; } = string.Empty;

    public DateTimeOffset ExpiredAt { get; set; }

    public List<Guid> ScheduleSeatIds { get; set; } = new();
}
