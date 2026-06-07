using BatterySwap.Domain.Common;

namespace BatterySwap.Domain.Entities;

/// <summary>
/// System login account for administrators, station operators and supervisors.
/// Distinct from <see cref="User"/>, which represents an EV driver.
/// </summary>
public class Account : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>ADMIN | OPERATOR | SUPERVISOR</summary>
    public string Role { get; set; } = "OPERATOR";
    public bool IsActive { get; set; } = true;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
}
