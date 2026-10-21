using Voyago.BusService.DTOs.Stops;

namespace Voyago.BusService.Services.Interfaces;

public interface IStopService
{
    Task<StopResponseDto> CreateAsync(CreateStopRequestDto request);

    Task<List<StopResponseDto>> GetAllAsync();

    Task<StopResponseDto?> GetByIdAsync(Guid id);

    Task<StopResponseDto?> UpdateAsync(Guid id, UpdateStopRequestDto request, uint expectedVersion);

    Task<bool> DeleteAsync(Guid id, uint expectedVersion);
}