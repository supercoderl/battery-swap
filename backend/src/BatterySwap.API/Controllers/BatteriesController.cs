using BatterySwap.Application.DTOs.Batteries;
using BatterySwap.Application.Services.Batteries;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class BatteriesController : BaseApiController
{
    private readonly IBatteryService _service;
    public BatteriesController(IBatteryService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BatteryQueryParameters query, CancellationToken ct)
        => Success(await _service.GetPagedAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct));

    [HttpGet("{id:long}/logs")]
    public async Task<IActionResult> GetLogs(long id, [FromQuery] int take = 100, CancellationToken ct = default)
        => Success(await _service.GetLogsAsync(id, take, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateBatteryDto dto, CancellationToken ct)
        => Success(await _service.CreateAsync(dto, ct), "Battery created.");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, UpdateBatteryDto dto, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, dto, ct), "Battery updated.");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Success();
    }
}
