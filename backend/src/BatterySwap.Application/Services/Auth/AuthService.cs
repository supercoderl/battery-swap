using AutoMapper;
using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.DTOs.Auth;
using BatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken ct = default);
    Task LogoutAsync(long accountId, CancellationToken ct = default);
    Task ChangePasswordAsync(long accountId, ChangePasswordRequestDto dto, CancellationToken ct = default);
    Task<AccountDto> GetProfileAsync(long accountId, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenService _jwt;
    private readonly IPasswordHasher _hasher;
    private readonly IMapper _mapper;

    public AuthService(IApplicationDbContext db, IJwtTokenService jwt, IPasswordHasher hasher, IMapper mapper)
    {
        _db = db;
        _jwt = jwt;
        _hasher = hasher;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(
            a => a.Username == dto.Username || a.Email == dto.Username, ct);

        if (account is null || !account.IsActive || !_hasher.Verify(dto.Password, account.PasswordHash))
            throw new ConflictException("Invalid username or password.");

        return await IssueTokensAsync(account, ct);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken ct = default)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.RefreshToken == dto.RefreshToken, ct);

        if (account is null || account.RefreshTokenExpiresAt is null || account.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new ConflictException("Invalid or expired refresh token.");

        return await IssueTokensAsync(account, ct);
    }

    public async Task LogoutAsync(long accountId, CancellationToken ct = default)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct)
            ?? throw new NotFoundException(nameof(Account), accountId);
        account.RefreshToken = null;
        account.RefreshTokenExpiresAt = null;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ChangePasswordAsync(long accountId, ChangePasswordRequestDto dto, CancellationToken ct = default)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct)
            ?? throw new NotFoundException(nameof(Account), accountId);

        if (!_hasher.Verify(dto.CurrentPassword, account.PasswordHash))
            throw new ConflictException("Current password is incorrect.");

        account.PasswordHash = _hasher.Hash(dto.NewPassword);
        account.RefreshToken = null;
        account.RefreshTokenExpiresAt = null;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AccountDto> GetProfileAsync(long accountId, CancellationToken ct = default)
    {
        var account = await _db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == accountId, ct)
            ?? throw new NotFoundException(nameof(Account), accountId);
        return _mapper.Map<AccountDto>(account);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(Account account, CancellationToken ct)
    {
        var tokens = _jwt.GenerateTokens(account);
        account.RefreshToken = tokens.RefreshToken;
        account.RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt;
        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto
        {
            AccessToken = tokens.AccessToken,
            AccessTokenExpiresAt = tokens.AccessTokenExpiresAt,
            RefreshToken = tokens.RefreshToken,
            RefreshTokenExpiresAt = tokens.RefreshTokenExpiresAt,
            Account = _mapper.Map<AccountDto>(account)
        };
    }
}
