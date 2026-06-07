using BatterySwap.Application.DTOs.Batteries;
using BatterySwap.Application.DTOs.Sessions;
using BatterySwap.Application.DTOs.Transactions;

namespace BatterySwap.Application.DTOs.Dashboard;

public class DashboardStatsDto
{
    public int TotalStations { get; set; }
    public int TotalCabinets { get; set; }
    public int TotalSlots { get; set; }
    public int TotalBatteries { get; set; }
    public int BatteriesInSlot { get; set; }
    public int BatteriesRented { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalTransactionsToday { get; set; }
}

public class ChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class DashboardChartsDto
{
    public List<ChartPointDto> DailySwaps { get; set; } = new();
    public List<ChartPointDto> MonthlyTrend { get; set; } = new();
    public List<ChartPointDto> BatteryHealthDistribution { get; set; } = new();
    public List<ChartPointDto> CabinetStatusDistribution { get; set; } = new();
    public List<ChartPointDto> BatterySocDistribution { get; set; } = new();
    public List<ChartPointDto> MostActiveStations { get; set; } = new();
}

public class DashboardOverviewDto
{
    public DashboardStatsDto Stats { get; set; } = new();
    public DashboardChartsDto Charts { get; set; } = new();
    public List<TransactionDto> LatestTransactions { get; set; } = new();
    public List<BatteryLogDto> LatestBatteryLogs { get; set; } = new();
    public List<ActiveSessionDto> ActiveSessions { get; set; } = new();
}
