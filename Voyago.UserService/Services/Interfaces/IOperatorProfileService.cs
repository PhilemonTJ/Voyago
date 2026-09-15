using Voyago.UserService.DTOs.OperatorProfiles;

namespace Voyago.UserService.Services.Interfaces;

public interface IOperatorProfileService
{
    Task<OperatorProfileResponse?> GetMyProfileAsync(Guid userId);

    Task<OperatorProfileResponse> CreateAsync(Guid userId, CreateOperatorProfileRequestDto request);

    Task<OperatorProfileResponse?> UpdateAsync(Guid userId, UpdateOperatorProfileRequestDto request);
}