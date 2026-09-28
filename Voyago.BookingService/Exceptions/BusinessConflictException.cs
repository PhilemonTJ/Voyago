namespace Voyago.BookingService.Exceptions;

public sealed class BusinessConflictException : Exception
{
    public BusinessConflictException(string message)
        : base(message)
    {
    }
}