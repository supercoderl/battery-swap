using BatterySwap.Domain.Entities;

namespace BatterySwap.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public record TokenResult(string AccessToken, DateTime AccessTokenExpiresAt, string RefreshToken, DateTime RefreshTokenExpiresAt);

public interface IJwtTokenService
{
    TokenResult GenerateTokens(Account account);
}

public interface ICurrentUserService
{
    long? AccountId { get; }
    string? Username { get; }
    string? Role { get; }
}
