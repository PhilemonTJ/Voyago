using Microsoft.AspNetCore.Mvc;
using Voyago.BusService.DTOs.Buses;
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

    [HttpPost]
    public async Task<ActionResult<BusResponseDto>> Create(CreateBusRequestDto request)
    {
        try
        {
            var bus = await _busService.CreateAsync(request);

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

    [HttpGet]
    public async Task<ActionResult<List<BusResponseDto>>> GetAll()
    {
        var buses = await _busService.GetAllAsync();

        return Ok(buses);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BusResponseDto>> GetById(Guid id)
    {
        var bus = await _busService.GetByIdAsync(id);

        if (bus is null)
        {
            return NotFound();
        }

        return Ok(bus);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BusResponseDto>> Update(Guid id, UpdateBusRequestDto request)
    {
        try
        {
            var bus = await _busService.UpdateAsync(id, request);

            if (bus is null)
            {
                return NotFound();
            }

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
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _busService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}