using System.ComponentModel.DataAnnotations;

namespace Voyago.UserService.DTOs.Auth;

public class LoginRequest
{
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}