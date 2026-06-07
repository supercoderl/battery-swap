using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("is-live")]
    public IActionResult TestIsLive()
    {
        return Ok("Application's living.");
    }
}