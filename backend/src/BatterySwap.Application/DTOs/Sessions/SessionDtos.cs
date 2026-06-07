using BatterySwap.Domain.Enums;

namespace BatterySwap.Application.DTOs.Sessions;

public class ActiveSessionDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public long CabinetId { get; set; }
    public string CabinetModel { get; set; } = string.Empty;
    public SwappingSessionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
