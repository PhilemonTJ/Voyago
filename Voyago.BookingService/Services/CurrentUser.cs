using System.Security.Claims;
using Voyago.BookingService.Services.Interfaces;

namespace Voyago.BookingService.Services;

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
            var value =  User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("Authenticated user ID is missing.");
            }

            return userId;
        }
    }

    public bool IsPassenger => User.IsInRole("Passenger");

    public bool IsOperator => User.IsInRole("Operator");

    public bool IsAdmin => User.IsInRole("Admin");
}