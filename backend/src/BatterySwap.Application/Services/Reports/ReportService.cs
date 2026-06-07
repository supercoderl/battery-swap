using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.DTOs.Dashboard;
using BatterySwap.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Reports;

public interface IReportService
{
    Task<List<ChartPointDto>> DailySwapsAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<ChartPointDto>> MonthlySwapsAsync(int year, CancellationToken ct = default);
    Task<List<ChartPointDto>> BatteryUtilizationAsync(CancellationToken ct = default);
    Task<List<ChartPointDto>> BatteryHealthReportAsync(CancellationToken ct = default);
    Task<List<ChartPointDto>> CabinetUtilizationAsync(CancellationToken ct = default);
}

public class ReportService : IReportService
{
    private readonly IApplicationDbContext _db;

    public ReportService(IApplicationDbContext db) => _db = db;

    public async Task<List<ChartPointDto>> DailySwapsAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var raw = await _db.SwappingTransactions
            .Where(t => t.CreatedAt >= from.Date && t.CreatedAt < to.Date.AddDays(1))
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .OrderBy(x => x.Day)
            .ToListAsync(ct);

        return raw.Select(x => new ChartPointDto { Label = x.Day.ToString("yyyy-MM-dd"), Value = x.Count }).ToList();
    }

    public async Task<List<ChartPointDto>> MonthlySwapsAsync(int year, CancellationToken ct = default)
    {
        var raw = await _db.SwappingTransactions
            .Where(t => t.CreatedAt.Year == year)
            .GroupBy(t => t.CreatedAt.Month)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return Enumerable.Range(1, 12)
            .Select(m => new ChartPointDto
            {
                Label = new DateTime(year, m, 1).ToString("MMM"),
                Value = raw.FirstOrDefault(x => x.Month == m)?.Count ?? 0
            }).ToList();
    }

    public async Task<List<ChartPointDto>> BatteryUtilizationAsync(CancellationToken ct = default)
    {
        var inSlot = await _db.Batteries.CountAsync(b => b.LocationType == BatteryLocationType.InSlot, ct);
        var rented = await _db.Batteries.CountAsync(b => b.LocationType == BatteryLocationType.RentedByUser, ct);
        return new List<ChartPointDto>
        {
            new() { Label = "In Slot", Value = inSlot },
            new() { Label = "Rented", Value = rented }
        };
    }

    public async Task<List<ChartPointDto>> BatteryHealthReportAsync(CancellationToken ct = default)
    {
        var raw = await _db.Batteries
            .GroupBy(b => b.HealthState)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);
        return raw.Select(x => new ChartPointDto { Label = x.Key.ToString(), Value = x.Count }).ToList();
    }

    public async Task<List<ChartPointDto>> CabinetUtilizationAsync(CancellationToken ct = default)
    {
        var raw = await _db.Cabinets
            .Select(c => new
            {
                c.CabinetModel,
                c.Id,
                Total = c.Slots.Count,
                Occupied = c.Slots.Count(s => s.CurrentBatteryId != null)
            })
            .ToListAsync(ct);

        return raw.Select(x => new ChartPointDto
        {
            Label = $"{x.CabinetModel} #{x.Id}",
            Value = x.Total == 0 ? 0 : Math.Round(x.Occupied * 100.0 / x.Total, 1)
        }).ToList();
    }
}
