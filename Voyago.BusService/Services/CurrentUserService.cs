using System.Security.Claims;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException(
                    "The authenticated user ID is missing or invalid.");
            }

            return userId;
        }
    }

    public bool IsOperator => _httpContextAccessor.HttpContext?.User?.IsInRole("Operator") == true;

    public bool IsAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") == true;
}