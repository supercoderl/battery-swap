using BatterySwap.Application.Common.Models;
using BatterySwap.Application.Services.Sessions;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class SessionsController : BaseApiController
{
    private readonly ISessionService _service;
    public SessionsController(ISessionService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
        => Success(await _service.GetPagedAsync(query, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> ForceClose(long id, CancellationToken ct)
    {
        await _service.ForceCloseAsync(id, ct);
        return Success();
    }
}
