using Voyago.BusService.DTOs.Seats;

namespace Voyago.BusService.Services.Interfaces;

public interface ISeatService
{
    Task<SeatResponseDto> CreateAsync(Guid busId, CreateSeatRequestDto request);

    Task<List<SeatResponseDto>> GetByBusIdAsync(Guid busId);

    Task<SeatResponseDto?> GetByIdAsync(Guid seatId);

    Task<SeatResponseDto?> UpdateAsync(Guid seatId, UpdateSeatRequestDto request, uint expectedVersion);

    Task<bool> DeleteAsync(Guid seatId, uint expectedVersion);
}