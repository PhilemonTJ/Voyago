using System.ComponentModel.DataAnnotations;

namespace Voyago.BusService.DTOs.Stops;

public class CreateStopRequestDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;
}