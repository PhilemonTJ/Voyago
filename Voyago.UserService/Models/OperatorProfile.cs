using System.ComponentModel.DataAnnotations;

namespace Voyago.UserService.Models;

public class OperatorProfile
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string ContactNumber { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? LogoImageUrl { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}