using BatterySwap.Domain.Common;
using BatterySwap.Domain.Enums;

namespace BatterySwap.Domain.Entities;

/// <summary>
/// Guards against a user opening multiple simultaneous swap requests.
/// </summary>
public class ActiveSwappingSession : BaseEntity
{
    public long UserId { get; set; }
    public long CabinetId { get; set; }
    public SwappingSessionStatus Status { get; set; } = SwappingSessionStatus.Processing;

    // Navigation
    public User User { get; set; } = null!;
    public Cabinet Cabinet { get; set; } = null!;
}
