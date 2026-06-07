namespace BatterySwap.Application.DTOs.Stations;

public class StationDto
{
    public long Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int CabinetCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStationDto
{
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class UpdateStationDto
{
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
