using BatterySwap.Domain.Common;

namespace BatterySwap.Domain.Entities;

public class Station : BaseEntity
{
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Navigation
    public ICollection<Cabinet> Cabinets { get; set; } = new List<Cabinet>();
}
