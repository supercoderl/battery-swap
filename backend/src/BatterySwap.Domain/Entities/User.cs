using BatterySwap.Domain.Common;

namespace BatterySwap.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int BalanceTrips { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<SwappingTransaction> Transactions { get; set; } = new List<SwappingTransaction>();
    public ICollection<ActiveSwappingSession> Sessions { get; set; } = new List<ActiveSwappingSession>();
}
