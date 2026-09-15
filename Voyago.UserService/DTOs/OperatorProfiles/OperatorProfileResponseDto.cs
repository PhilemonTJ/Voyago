using System;

namespace Voyago.UserService.DTOs.OperatorProfiles;

public class OperatorProfileResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string ContactNumber { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? LogoImageUrl { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}