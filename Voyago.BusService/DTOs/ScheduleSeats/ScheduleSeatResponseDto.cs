namespace Voyago.BusService.DTOs.ScheduleSeats;

public class ScheduleSeatResponseDto
{
    public Guid ScheduleSeatId { get; set; }

    public Guid SeatId { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public string SeatType { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public int RowNumber { get; set; }

    public int ColumnNumber { get; set; }

    public decimal Price { get; set; }

    public string Status { get; set; } = string.Empty;
}