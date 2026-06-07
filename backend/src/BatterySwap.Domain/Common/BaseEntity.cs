namespace BatterySwap.Domain.Common;

/// <summary>
/// Base type for all persisted aggregate roots / entities.
/// Exposes a strongly-typed primary key and audit timestamp.
/// </summary>
public abstract class BaseEntity
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
