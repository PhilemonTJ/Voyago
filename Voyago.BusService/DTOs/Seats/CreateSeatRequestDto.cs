using System.ComponentModel.DataAnnotations;
using Voyago.BusService.Models;

namespace Voyago.BusService.DTOs.Seats;

public class CreateSeatRequestDto
{
    [Required, MaxLength(20)]
    public string SeatNumber { get; set; } = string.Empty;

    [Required]
    public SeatType SeatType { get; set; }

    public int RowNumber { get; set; }

    public int ColumnNumber { get; set; }
}