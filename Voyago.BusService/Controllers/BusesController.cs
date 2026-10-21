using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voyago.BusService.Data;
using Voyago.BusService.DTOs.Buses;
using Voyago.BusService.Exceptions;
using Voyago.BusService.Services.Interfaces;

namespace Voyago.BusService.Controllers;

[ApiController]
[Route("api/buses")]
public class BusesController : ControllerBase
{
    private readonly IBusService _busService;

    public BusesController(IBusService busService)
    {
        _busService = busService;
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPost]
    public async Task<ActionResult<BusResponseDto>> Create(CreateBusRequestDto request)
    {
        try
        {
            var bus = await _busService.CreateAsync(request);
            ConcurrencyToken.SetEtag(Response, bus.Version);

            return CreatedAtAction(
                nameof(GetById),
                new { id = bus.Id },
                bus);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Bus conflict",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<BusResponseDto>>> GetAll()
    {
        var buses = await _busService.GetAllAsync();

        return Ok(buses);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BusResponseDto>> GetById(Guid id)
    {
        var bus = await _busService.GetByIdAsync(id);

        if (bus is null)
        {
            return NotFound();
        }

        ConcurrencyToken.SetEtag(Response, bus.Version);
        return Ok(bus);
    }

    [Authorize(Roles = "Operator, Admin")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BusResponseDto>> Update(
        Guid id,
        UpdateBusRequestDto request,
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
            var bus = await _busService.UpdateAsync(id, request, expectedVersion);

            if (bus is null)
            {
                return NotFound();
            }

            ConcurrencyToken.SetEtag(Response, bus.Version);
            return Ok(bus);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Bus conflict",
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

        try
        {
            var deleted = await _busService.DeleteAsync(id, expectedVersion);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
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