using Voyago.BusService.Models;

namespace Voyago.BusService.DTOs.Buses;

public class BusResponseDto
{
    public Guid Id { get; set; }

    public Guid OperatorId { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string BusNumber { get; set; } = string.Empty;

    public string? BusName { get; set; }

    public BusType BusType { get; set; }

    public int TotalSeats { get; set; }

    public int TotalRows { get; set; }

    public int TotalColumns { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}