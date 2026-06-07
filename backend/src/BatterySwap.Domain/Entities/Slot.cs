using BatterySwap.Domain.Common;

namespace BatterySwap.Domain.Entities;

public class Slot : BaseEntity
{
    public long CabinetId { get; set; }
    public int SlotNumber { get; set; }
    public bool IsHardwareLocked { get; set; }
    public long? CurrentBatteryId { get; set; }

    // Navigation
    public Cabinet Cabinet { get; set; } = null!;
    public Battery? CurrentBattery { get; set; }
}
