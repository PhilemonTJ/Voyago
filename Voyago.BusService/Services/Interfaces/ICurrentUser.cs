namespace Voyago.BusService.Services.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    bool IsPassenger { get; }

    bool IsOperator { get; }

    bool IsAdmin { get; }
}