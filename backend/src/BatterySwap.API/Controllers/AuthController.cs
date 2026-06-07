using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.DTOs.Auth;
using BatterySwap.Application.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BatterySwap.API.Controllers;

[AllowAnonymous]
public class AuthController : BaseApiController
{
    private readonly IAuthService _auth;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService auth, ICurrentUserService currentUser)
    {
        _auth = auth;
        _currentUser = currentUser;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto, CancellationToken ct)
        => Success(await _auth.LoginAsync(dto, ct), "Login successful.");

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto dto, CancellationToken ct)
        => Success(await _auth.RefreshAsync(dto, ct), "Token refreshed.");

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _auth.LogoutAsync(RequireAccountId(), ct);
        return Success();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto dto, CancellationToken ct)
    {
        await _auth.ChangePasswordAsync(RequireAccountId(), dto, ct);
        return Success();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
        => Success(await _auth.GetProfileAsync(RequireAccountId(), ct));

    private long RequireAccountId() =>
        _currentUser.AccountId ?? throw new NotFoundException("Authenticated account not found.");
}
