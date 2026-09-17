namespace Voyago.BusService.Rules;

using Voyago.BusService.Models;

public static class BusSeatTypeRules
{
    public static bool IsAllowed(BusType busType, SeatType seatType)
    {
        return busType switch
        {
            BusType.Seater =>
                seatType == SeatType.Seater,

            BusType.Sleeper =>
                seatType == SeatType.SleeperLower ||
                seatType == SeatType.SleeperUpper,

            BusType.SemiSleeper =>
                seatType == SeatType.Seater ||
                seatType == SeatType.SleeperLower ||
                seatType == SeatType.SleeperUpper,

            _ => false
        };
    }

    public static void Validate(BusType busType, SeatType seatType)
    {
        if (!IsAllowed(busType, seatType))
        {
            throw new InvalidOperationException($"Seat type '{seatType}' is not allowed for a '{busType}' bus.");
        }
    }
}