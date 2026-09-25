using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.BusService.DTOs.Seats;
using Voyago.BusService.Exceptions;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Controllers;

[ApiController]
[Route("api")]
public class SeatsController : ControllerBase
{
    private readonly ISeatService _seatService;

    public SeatsController(ISeatService seatService)
    {
        _seatService = seatService;
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost("buses/{busId:guid}/seats")]
    public async Task<ActionResult<SeatResponseDto>> Create(Guid busId, CreateSeatRequestDto request)
    {
        try
        {
            var seat = await _seatService.CreateAsync(busId, request);

            return CreatedAtAction(
                nameof(GetById),
                new { seatId = seat.Id },
                seat);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Bus not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Seat conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (ForbiddenException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: ex.Message);
        }
    }

    [AllowAnonymous]
    [HttpGet("buses/{busId:guid}/seats")]
    public async Task<ActionResult<List<SeatResponseDto>>> GetByBusId(Guid busId)
    {
        try
        {
            var seats = await _seatService.GetByBusIdAsync(busId);

            return Ok(seats);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Bus not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("seats/{seatId:guid}")]
    public async Task<ActionResult<SeatResponseDto>> GetById(Guid seatId)
    {
        var seat = await _seatService.GetByIdAsync(seatId);

        if (seat is null)
        {
            return NotFound();
        }

        return Ok(seat);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("seats/{seatId:guid}")]
    public async Task<ActionResult<SeatResponseDto>> Update(Guid seatId, UpdateSeatRequestDto request)
    {
        try
        {
            var seat = await _seatService.UpdateAsync(seatId, request);

            if (seat is null)
            {
                return NotFound();
            }

            return Ok(seat);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Bus not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Seat conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (ForbiddenException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: ex.Message);
        }

    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpDelete("seats/{seatId:guid}")]
    public async Task<IActionResult> Delete(Guid seatId)
    {
        try
        {
            var deleted = await _seatService.DeleteAsync(seatId);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Bus not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (ForbiddenException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: ex.Message);
        }
    }
}