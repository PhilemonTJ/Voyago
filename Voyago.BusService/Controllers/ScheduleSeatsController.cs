using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.BusService.DTOs.ScheduleSeats;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Controllers;

[ApiController]
[Route("api/schedules/{scheduleId:guid}/seats")]
public class ScheduleSeatsController : ControllerBase
{
    private readonly IScheduleSeatService _scheduleSeatService;

    public ScheduleSeatsController(IScheduleSeatService scheduleSeatService)
    {
        _scheduleSeatService = scheduleSeatService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ScheduleSeatResponseDto>>> GetByScheduleId(Guid scheduleId)
    {
        try
        {
            var seats = await _scheduleSeatService.GetByScheduleIdAsync(scheduleId);

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
            return Conflict(new ProblemDetails
            {
                Title = "Schedule Seat conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}