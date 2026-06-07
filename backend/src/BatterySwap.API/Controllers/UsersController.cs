using BatterySwap.Application.DTOs.Users;
using BatterySwap.Application.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

public class UsersController : BaseApiController
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] UserQueryParameters query, CancellationToken ct)
        => Success(await _service.GetPagedAsync(query, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
        => Success(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserDto dto, CancellationToken ct)
        => Success(await _service.CreateAsync(dto, ct), "User created.");

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, UpdateUserDto dto, CancellationToken ct)
        => Success(await _service.UpdateAsync(id, dto, ct), "User updated.");

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return Success();
    }
}
