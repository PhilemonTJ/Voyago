using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.BusService.DTOs.Schedules;
using Voyago.BusService.Exceptions;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Controllers;

[ApiController]
[Route("api/schedules")]
public class SchedulesController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public SchedulesController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<ActionResult<ScheduleResponseDto>> Create(CreateScheduleRequestDto request)
    {
        try
        {
            var schedule = await _scheduleService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = schedule.Id },
                schedule);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Schedule dependency not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Schedule conflict",
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
    [HttpGet]
    public async Task<ActionResult<List<ScheduleResponseDto>>> GetAll()
    {
        var schedules = await _scheduleService.GetAllAsync();

        return Ok(schedules);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ScheduleResponseDto>> GetById(Guid id)
    {
        var schedule = await _scheduleService.GetByIdAsync(id);

        if (schedule is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Schedule not found",
                Detail = "The requested schedule does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(schedule);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ScheduleResponseDto>> Update(Guid id, UpdateScheduleRequestDto request)
    {
        try
        {
            var schedule = await _scheduleService.UpdateAsync(id, request);

            if (schedule is null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Schedule not found",
                    Detail = "The requested schedule does not exist.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(schedule);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Schedule dependency not found",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Schedule conflict",
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
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _scheduleService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Schedule not found",
                    Detail = "The requested schedule does not exist.",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Schedule dependency not found",
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

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<ActionResult<List<ScheduleSearchResponseDto>>> Search([FromQuery] ScheduleSearchRequestDto request)
    {
        try
        {
            var schedules = await _scheduleService.SearchAsync(request);

            return Ok(schedules);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid schedule search",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid schedule search",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}