using BatterySwap.Application.DTOs.Cabinets;
using BatterySwap.Application.Services.Cabinets;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class CabinetsController : BaseApiController
{
    private readonly ICabinetService _service;
    public CabinetsController(ICabinetService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CabinetQueryParameters query, CancellationToken ct)
        => Success(await _service.GetPagedAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateCabinetDto dto, CancellationToken ct)
        => Success(await _service.CreateAsync(dto, ct), "Cabinet created.");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, UpdateCabinetDto dto, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, dto, ct), "Cabinet updated.");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Success();
    }
}
