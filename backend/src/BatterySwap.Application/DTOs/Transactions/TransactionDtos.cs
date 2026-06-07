using BatterySwap.Domain.Enums;

namespace BatterySwap.Application.DTOs.Transactions;

public class TransactionDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public long CabinetId { get; set; }
    public string CabinetModel { get; set; } = string.Empty;

    public long? SlotInId { get; set; }
    public long? BatteryInId { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public long? SlotOutId { get; set; }
    public long? BatteryOutId { get; set; }
    public DateTime? DispensedAt { get; set; }

    public SwappingTransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Total swap duration in seconds, when both timestamps exist.</summary>
    public double? DurationSeconds { get; set; }
}
