using System.ComponentModel.DataAnnotations;

namespace Voyago.BusService.DTOs.Schedules;

public class ScheduleSearchRequestDto
{
    [Required, MaxLength(100)]
    public string Origin { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public DateOnly Date { get; set; }
}