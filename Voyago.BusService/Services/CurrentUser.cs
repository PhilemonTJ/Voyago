using System.Security.Claims;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Services;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User => _httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
    public bool IsAuthenticated => User.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("The authenticated user ID is missing or invalid.");
            }

            return userId;
        }
    }

    public bool IsPassenger => User.IsInRole("Passenger") == true;

    public bool IsOperator => User.IsInRole("Operator") == true;

    public bool IsAdmin => User.IsInRole("Admin") == true;
}