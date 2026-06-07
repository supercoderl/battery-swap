using BatterySwap.Application.DTOs.Slots;
using BatterySwap.Application.Services.Slots;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class SlotsController : BaseApiController
{
    private readonly ISlotService _service;
    public SlotsController(ISlotService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SlotQueryParameters query, CancellationToken ct)
        => Success(await _service.GetPagedAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateSlotDto dto, CancellationToken ct)
        => Success(await _service.CreateAsync(dto, ct), "Slot created.");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, UpdateSlotDto dto, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, dto, ct), "Slot updated.");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Success();
    }
}
