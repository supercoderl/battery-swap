using BatterySwap.Domain.Common;
using BatterySwap.Domain.Enums;

namespace BatterySwap.Domain.Entities;

public class Battery : BaseEntity
{
    /// <summary>State of charge, percentage 0-100.</summary>
    public int Soc { get; set; }

    /// <summary>Temperature in Celsius.</summary>
    public double Temperature { get; set; }

    /// <summary>Voltage in Volts.</summary>
    public double Voltage { get; set; }

    public BatteryHealthState HealthState { get; set; } = BatteryHealthState.Good;
    public BatteryLocationType LocationType { get; set; } = BatteryLocationType.InSlot;

    /// <summary>Slot id when IN_SLOT, or User id when RENTED_BY_USER.</summary>
    public long? HolderId { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<BatteryLog> Logs { get; set; } = new List<BatteryLog>();
}
