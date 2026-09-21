using System.ComponentModel.DataAnnotations;

namespace Voyago.BusService.Models;

public class Seat
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid BusId { get; set; }

    [Required, MaxLength(20)]
    public string SeatNumber { get; set; } = string.Empty;

    [Required]
    public SeatType SeatType { get; set; }

    [Required]
    public SeatLevel Level { get; set; }

    public int RowNumber { get; set; }

    public int ColumnNumber { get; set; }

    [Required]
    public bool IsActive { get; set; }

    public Bus Bus { get; set; } = null!;

    public ICollection<ScheduleSeat> ScheduleSeats { get; set; } = new List<ScheduleSeat>();
}