using BatterySwap.Domain.Common;
using BatterySwap.Domain.Enums;

namespace BatterySwap.Domain.Entities;

public class SwappingTransaction : BaseEntity
{
    public long UserId { get; set; }
    public long CabinetId { get; set; }

    // Battery returned by the user (goes IN to a slot)
    public long? SlotInId { get; set; }
    public long? BatteryInId { get; set; }
    public DateTime? ReturnedAt { get; set; }

    // Battery dispensed to the user (goes OUT of a slot)
    public long? SlotOutId { get; set; }
    public long? BatteryOutId { get; set; }
    public DateTime? DispensedAt { get; set; }

    public SwappingTransactionStatus Status { get; set; } = SwappingTransactionStatus.Success;

    // Navigation
    public User User { get; set; } = null!;
    public Cabinet Cabinet { get; set; } = null!;
    public Slot? SlotIn { get; set; }
    public Slot? SlotOut { get; set; }
    public Battery? BatteryIn { get; set; }
    public Battery? BatteryOut { get; set; }
}
