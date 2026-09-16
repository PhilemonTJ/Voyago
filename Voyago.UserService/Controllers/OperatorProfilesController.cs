using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.UserService.DTOs.OperatorProfiles;
using Voyago.UserService.Services.Interfaces;

namespace Voyago.UserService.Controllers;

[ApiController]
[Route("api/operator-profile")]
[Authorize(Roles = "Operator")]
public class OperatorProfilesController : ControllerBase
{
    private readonly IOperatorProfileService _operatorProfileService;

    public OperatorProfilesController(IOperatorProfileService operatorProfileService)
    {
        _operatorProfileService = operatorProfileService;
    }

    [HttpGet]
    public async Task<ActionResult<OperatorProfileResponse>> GetMyProfile()
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Unable to determine the current user.");
        }

        var profile = await _operatorProfileService.GetMyProfileAsync(userId);

        if (profile is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: "Operator profile not found.");
        }

        return Ok(profile);
    }

    [HttpPost]
    public async Task<ActionResult<OperatorProfileResponse>> CreateProfile(CreateOperatorProfileRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Unable to determine the current user.");
        }

        try
        {
            var profile = await _operatorProfileService.CreateAsync(userId, request);

            return CreatedAtAction(
                nameof(GetMyProfile),
                profile);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
    }

    [HttpPut]
    public async Task<ActionResult<OperatorProfileResponse>> UpdateProfile(UpdateOperatorProfileRequestDto request)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Unable to determine the current user.");
        }

        try
        {
            var profile = await _operatorProfileService.UpdateAsync(userId, request);

            if (profile is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found",
                    detail: "Operator profile not found.");
            }

            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: ex.Message);
        }
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userIdClaim, out userId);
    }
}