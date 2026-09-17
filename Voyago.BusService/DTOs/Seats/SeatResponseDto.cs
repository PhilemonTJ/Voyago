using Voyago.BusService.Models;

namespace Voyago.BusService.DTOs.Seats;

public class SeatResponseDto
{
    public Guid Id { get; set; }

    public Guid BusId { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public SeatType SeatType { get; set; }

    public int RowNumber { get; set; }

    public int ColumnNumber { get; set; }

    public bool IsActive { get; set; }
}