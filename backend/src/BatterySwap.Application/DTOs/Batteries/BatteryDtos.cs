using BatterySwap.Domain.Enums;

namespace BatterySwap.Application.DTOs.Batteries;

public class BatteryDto
{
    public long Id { get; set; }
    public int Soc { get; set; }
    public double Temperature { get; set; }
    public double Voltage { get; set; }
    public BatteryHealthState HealthState { get; set; }
    public BatteryLocationType LocationType { get; set; }
    public long? HolderId { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBatteryDto
{
    public int Soc { get; set; }
    public double Temperature { get; set; }
    public double Voltage { get; set; }
    public BatteryHealthState HealthState { get; set; } = BatteryHealthState.Good;
    public BatteryLocationType LocationType { get; set; } = BatteryLocationType.InSlot;
    public long? HolderId { get; set; }
}

public class UpdateBatteryDto
{
    public int Soc { get; set; }
    public double Temperature { get; set; }
    public double Voltage { get; set; }
    public BatteryHealthState HealthState { get; set; }
    public BatteryLocationType LocationType { get; set; }
    public long? HolderId { get; set; }
}

public class BatteryLogDto
{
    public long Id { get; set; }
    public long BatteryId { get; set; }
    public int Soc { get; set; }
    public double Temperature { get; set; }
    public double Voltage { get; set; }
    public DateTime RecordedAt { get; set; }
}
