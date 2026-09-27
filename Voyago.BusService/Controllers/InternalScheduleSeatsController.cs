using Microsoft.AspNetCore.Mvc;
using Voyago.BusService.Services;
using Voyago.BusService.Services.Interfaces;
using Voyago.Shared.Contracts.Bus;

namespace Voyago.BusService.Controllers;

[ApiController]
[Route("api/internal/schedules/{scheduleId:guid}/seats")]
public class InternalScheduleSeatsController : ControllerBase
{
    private readonly IScheduleSeatService _scheduleSeatService;

    public InternalScheduleSeatsController(IScheduleSeatService scheduleSeatService)
    {
        _scheduleSeatService = scheduleSeatService;
    }

    [HttpPost("validate")]
    public async Task<ActionResult<List<ScheduleSeatInfo>>> Validate(
        Guid scheduleId,
        [FromBody] List<Guid> scheduleSeatIds)
    {
        if (scheduleSeatIds is null || scheduleSeatIds.Count == 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "No seats provided.",
                detail: "At least one schedule seat must be provided.");
        }

        try
        {
            var requestedIds = scheduleSeatIds.Distinct().ToList();

            var seats = await _scheduleSeatService.GetForBookingAsync(scheduleId, requestedIds);

            if (seats.Count != requestedIds.Count)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid schedule seats.",
                    detail: "One or more schedule seats do not belong to this schedule.");
            }

            return Ok(seats);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Schedule not found.",
                detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Schedule cannot be booked.",
                detail: ex.Message);
        }
    }

    [HttpPost("reserve")]
    public async Task<ActionResult<List<ScheduleSeatInfo>>> Reserve(
       Guid scheduleId,
       [FromBody] List<Guid> scheduleSeatIds)
    {
        if (scheduleSeatIds is null || scheduleSeatIds.Count == 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "No seats provided.",
                detail: "At least one schedule seat must be provided.");
        }

        try
        {
            var requestedIds = scheduleSeatIds.Distinct().ToList();

            var seats = await _scheduleSeatService.ReserveForBookingAsync(scheduleId, requestedIds);

            return Ok(seats);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid reservation request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Schedule not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Seat reservation conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPost("release")]
    public async Task<IActionResult> Release(
        Guid scheduleId, 
        [FromBody] List<Guid> scheduleSeatIds)
    {
        if (scheduleSeatIds is null || scheduleSeatIds.Count == 0)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "No seats provided.",
                detail: "At least one schedule seat must be provided.");
        }

        try
        {
            var requestedIds = scheduleSeatIds.Distinct().ToList();

            await _scheduleSeatService.ReleaseAsync(scheduleId, requestedIds);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid release request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Schedule not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Seat release conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}