using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.BusService.DTOs.Stops;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Controllers;

[ApiController]
[Route("api/stops")]
public class StopsController : ControllerBase
{
    private readonly IStopService _stopService;

    public StopsController(IStopService stopService)
    {
        _stopService = stopService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<StopResponseDto>> Create(CreateStopRequestDto request)
    {
        try
        {
            var stop = await _stopService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = stop.Id },
                stop);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Stop conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<StopResponseDto>>> GetAll()
    {
        var stops = await _stopService.GetAllAsync();

        return Ok(stops);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StopResponseDto>> GetById(Guid id)
    {
        var stop = await _stopService.GetByIdAsync(id);

        if (stop is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Stop not found",
                Detail = "The requested stop does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(stop);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StopResponseDto>> Update(Guid id, UpdateStopRequestDto request)
    {
        try
        {
            var stop = await _stopService.UpdateAsync(id, request);

            if (stop is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Stop not found",
                    Detail = "The requested stop does not exist.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(stop);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Stop conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _stopService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Stop not found",
                Detail = "The requested stop does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return NoContent();
    }
}