using System.ComponentModel.DataAnnotations;
using Voyago.BusService.Models;

namespace Voyago.BusService.DTOs.Buses;

public class CreateBusRequestDto
{
    [Required]
    public Guid OperatorId { get; set; }

    [Required, MaxLength(50)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string BusNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? BusName { get; set; }

    [Required]
    public BusType BusType { get; set; }

    [Range(1, 200)]
    public int TotalSeats { get; set; }

    public string? ImageUrl { get; set; }
}