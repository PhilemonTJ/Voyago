using Voyago.BusService.DTOs.Buses;

namespace Voyago.BusService.Services.Interfaces;

public interface IBusService
{
    Task<BusResponseDto> CreateAsync(CreateBusRequestDto request);

    Task<List<BusResponseDto>> GetAllAsync();

    Task<BusResponseDto?> GetByIdAsync(Guid id);

    Task<BusResponseDto?> UpdateAsync(Guid id, UpdateBusRequestDto request);

    Task<bool> DeleteAsync(Guid id);
}