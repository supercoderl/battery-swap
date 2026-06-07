namespace BatterySwap.Application.DTOs.Slots;

public class SlotDto
{
    public long Id { get; set; }
    public long CabinetId { get; set; }
    public string CabinetModel { get; set; } = string.Empty;
    public int SlotNumber { get; set; }
    public bool IsHardwareLocked { get; set; }
    public long? CurrentBatteryId { get; set; }
    public int? BatterySoc { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSlotDto
{
    public long CabinetId { get; set; }
    public int SlotNumber { get; set; }
    public bool IsHardwareLocked { get; set; }
    public long? CurrentBatteryId { get; set; }
}

public class UpdateSlotDto
{
    public int SlotNumber { get; set; }
    public bool IsHardwareLocked { get; set; }
    public long? CurrentBatteryId { get; set; }
}
