namespace BatterySwap.Application.DTOs.Users;

public class UserDto
{
    public long Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int BalanceTrips { get; set; }
    public bool IsActive { get; set; }
    public int TransactionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int BalanceTrips { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int BalanceTrips { get; set; }
    public bool IsActive { get; set; }
}
