using System.ComponentModel.DataAnnotations;

namespace Voyago.UserService.DTOs.OperatorProfiles;

public class CreateOperatorProfileRequestDto
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [MaxLength(20)]
    public string ContactNumber { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? LogoImageUrl { get; set; }
}