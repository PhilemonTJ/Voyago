using System.Security.Claims;

namespace Voyago.BusService.Services.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    bool IsOperator { get; }

    bool IsAdmin { get; }
}