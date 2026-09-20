using Voyago.BusService.DTOs.Schedules;

namespace Voyago.BusService.Services.Interfaces;

public interface IScheduleService
{
    Task<ScheduleResponseDto> CreateAsync(CreateScheduleRequestDto request);

    Task<List<ScheduleResponseDto>> GetAllAsync();

    Task<ScheduleResponseDto?> GetByIdAsync(Guid id);

    Task<ScheduleResponseDto?> UpdateAsync(Guid id, UpdateScheduleRequestDto request);

    Task<bool> DeleteAsync(Guid id);

    Task<List<ScheduleSearchResponseDto>> SearchAsync(ScheduleSearchRequestDto request);
}