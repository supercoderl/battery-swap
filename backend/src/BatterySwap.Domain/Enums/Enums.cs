namespace BatterySwap.Domain.Enums;

public enum CabinetStatus
{
    Online = 0,
    Offline = 1,
    Maintenance = 2
}

public enum BatteryHealthState
{
    Good = 0,
    Degraded = 1
}

public enum BatteryLocationType
{
    InSlot = 0,
    RentedByUser = 1
}

public enum SwappingSessionStatus
{
    Processing = 0,
    HardwareWaiting = 1
}

public enum SwappingTransactionStatus
{
    Success = 0,
    FailedSlotOut = 1
}
