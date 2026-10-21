using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.BusService.Data;
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
            ConcurrencyToken.SetEtag(Response, stop.Version);

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

        ConcurrencyToken.SetEtag(Response, stop.Version);
        return Ok(stop);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StopResponseDto>> Update(
        Guid id,
        UpdateStopRequestDto request,
        [FromHeader(Name = "If-Match")] string? ifMatch)
    {
        if (!ConcurrencyToken.TryReadIfMatch(ifMatch, out var expectedVersion))
        {
            return Problem(
                statusCode: StatusCodes.Status428PreconditionRequired,
                title: "Precondition required",
                detail: "Send the current ETag in the If-Match header.");
        }

        try
        {
            var stop = await _stopService.UpdateAsync(id, request, expectedVersion);

            if (stop is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Stop not found",
                    Detail = "The requested stop does not exist.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            ConcurrencyToken.SetEtag(Response, stop.Version);
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
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        [FromHeader(Name = "If-Match")] string? ifMatch)
    {
        if (!ConcurrencyToken.TryReadIfMatch(ifMatch, out var expectedVersion))
        {
            return Problem(
                statusCode: StatusCodes.Status428PreconditionRequired,
                title: "Precondition required",
                detail: "Send the current ETag in the If-Match header.");
        }

        var deleted = await _stopService.DeleteAsync(id, expectedVersion);

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