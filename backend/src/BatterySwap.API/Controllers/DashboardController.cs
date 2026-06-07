using BatterySwap.Application.Services.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class DashboardController : BaseApiController
{
    private readonly IDashboardService _service;
    public DashboardController(IDashboardService service) => _service = service;

    [HttpGet("stats")]
    public async Task<IActionResult> Stats(CancellationToken ct)
        => Success(await _service.GetStatsAsync(ct));

    [HttpGet("charts")]
    public async Task<IActionResult> Charts(CancellationToken ct)
        => Success(await _service.GetChartsAsync(ct));

    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken ct)
        => Success(await _service.GetOverviewAsync(ct));
}
