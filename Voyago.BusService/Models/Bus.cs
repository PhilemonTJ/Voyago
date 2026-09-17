using System.ComponentModel.DataAnnotations;

namespace Voyago.BusService.Models;

public class Bus
{
    [Key]
    public Guid Id { get; set; }

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

    [Range(1, 100)]
    public int TotalRows { get; set; }

    [Range(1, 20)]
    public int TotalColumns { get; set; }

    public string? ImageUrl { get; set; }

    [Required]
    public bool IsActive { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}