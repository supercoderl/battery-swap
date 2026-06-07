using BatterySwap.Application.Services.Reports;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class ReportsController : BaseApiController
{
    private readonly IReportService _service;
    public ReportsController(IReportService service) => _service = service;

    [HttpGet("daily-swaps")]
    public async Task<IActionResult> DailySwaps([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var f = from ?? DateTime.UtcNow.Date.AddDays(-30);
        var t = to ?? DateTime.UtcNow.Date;
        return Success(await _service.DailySwapsAsync(f, t, ct));
    }

    [HttpGet("monthly-swaps")]
    public async Task<IActionResult> MonthlySwaps([FromQuery] int? year, CancellationToken ct)
        => Success(await _service.MonthlySwapsAsync(year ?? DateTime.UtcNow.Year, ct));

    [HttpGet("battery-utilization")]
    public async Task<IActionResult> BatteryUtilization(CancellationToken ct)
        => Success(await _service.BatteryUtilizationAsync(ct));

    [HttpGet("battery-health")]
    public async Task<IActionResult> BatteryHealth(CancellationToken ct)
        => Success(await _service.BatteryHealthReportAsync(ct));

    [HttpGet("cabinet-utilization")]
    public async Task<IActionResult> CabinetUtilization(CancellationToken ct)
        => Success(await _service.CabinetUtilizationAsync(ct));
}
