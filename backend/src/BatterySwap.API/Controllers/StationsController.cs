using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Stations;
using BatterySwap.Application.Services.Stations;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class StationsController : BaseApiController
{
    private readonly IStationService _service;
    public StationsController(IStationService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters query, CancellationToken ct)
        => Success(await _service.GetPagedAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateStationDto dto, CancellationToken ct)
        => Success(await _service.CreateAsync(dto, ct), "Station created.");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, UpdateStationDto dto, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, dto, ct), "Station updated.");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Success();
    }
}
