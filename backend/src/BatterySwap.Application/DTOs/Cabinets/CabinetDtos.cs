using BatterySwap.Domain.Enums;

namespace BatterySwap.Application.DTOs.Cabinets;

public class CabinetDto
{
    public long Id { get; set; }
    public long StationId { get; set; }
    public string StationAddress { get; set; } = string.Empty;
    public string CabinetModel { get; set; } = string.Empty;
    public CabinetStatus Status { get; set; }
    public int SlotCount { get; set; }
    public int OccupiedSlots { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCabinetDto
{
    public long StationId { get; set; }
    public string CabinetModel { get; set; } = string.Empty;
    public CabinetStatus Status { get; set; } = CabinetStatus.Offline;
}

public class UpdateCabinetDto
{
    public long StationId { get; set; }
    public string CabinetModel { get; set; } = string.Empty;
    public CabinetStatus Status { get; set; }
}
