namespace Voyago.Shared.Contracts.Bus;

public class ScheduleSeatInfo
{
    public Guid ScheduleSeatId { get; set; }

    public Guid ScheduleId { get; set; }

    public Guid SeatId { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;
}