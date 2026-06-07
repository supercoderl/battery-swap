using BatterySwap.Domain.Common;

namespace BatterySwap.Domain.Entities;

public class BatteryLog : BaseEntity
{
    public long BatteryId { get; set; }
    public int Soc { get; set; }
    public double Temperature { get; set; }
    public double Voltage { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Battery Battery { get; set; } = null!;
}
