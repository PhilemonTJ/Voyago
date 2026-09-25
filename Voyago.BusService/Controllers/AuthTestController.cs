using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Voyago.BusService.Controllers;

[ApiController]
[Route("api/auth-test")]
public class AuthTestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new
        {
            message = "Public endpoint works."
        });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new
        {
            message = "Authentication works.",
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier),
            role = User.FindFirstValue(
                ClaimTypes.Role),
            name = User.Identity?.Name
        });
    }

    [Authorize(Roles = "Operator")]
    [HttpGet("operator")]
    public IActionResult Operator()
    {
        return Ok(new
        {
            message = "Operator authorization works.",
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier),
            role = User.FindFirstValue(
                ClaimTypes.Role)
        });
    }
}