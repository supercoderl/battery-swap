using BatterySwap.Domain.Common;
using BatterySwap.Domain.Enums;

namespace BatterySwap.Domain.Entities;

public class Cabinet : BaseEntity
{
    public long StationId { get; set; }
    public string CabinetModel { get; set; } = string.Empty;
    public CabinetStatus Status { get; set; } = CabinetStatus.Offline;

    // Navigation
    public Station Station { get; set; } = null!;
    public ICollection<Slot> Slots { get; set; } = new List<Slot>();
    public ICollection<SwappingTransaction> Transactions { get; set; } = new List<SwappingTransaction>();
    public ICollection<ActiveSwappingSession> Sessions { get; set; } = new List<ActiveSwappingSession>();
}
