using AutoMapper;
using AutoMapper.QueryableExtensions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.DTOs.Batteries;
using BatterySwap.Application.DTOs.Dashboard;
using BatterySwap.Application.DTOs.Sessions;
using BatterySwap.Application.DTOs.Transactions;
using BatterySwap.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Dashboard;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default);
    Task<DashboardChartsDto> GetChartsAsync(CancellationToken ct = default);
    Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken ct = default);
}

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public DashboardService(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<DashboardStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var todayStart = DateTime.UtcNow.Date;
        var todayEnd = todayStart.AddDays(1);

        return new DashboardStatsDto
        {
            TotalStations = await _db.Stations.CountAsync(ct),
            TotalCabinets = await _db.Cabinets.CountAsync(ct),
            TotalSlots = await _db.Slots.CountAsync(ct),
            TotalBatteries = await _db.Batteries.CountAsync(ct),
            BatteriesInSlot = await _db.Batteries.CountAsync(b => b.LocationType == BatteryLocationType.InSlot, ct),
            BatteriesRented = await _db.Batteries.CountAsync(b => b.LocationType == BatteryLocationType.RentedByUser, ct),
            ActiveUsers = await _db.Users.CountAsync(u => u.IsActive, ct),
            TotalTransactionsToday = await _db.SwappingTransactions.CountAsync(t => t.CreatedAt >= todayStart && t.CreatedAt < todayEnd, ct)
        };
    }

    public async Task<DashboardChartsDto> GetChartsAsync(CancellationToken ct = default)
    {
        var charts = new DashboardChartsDto();

        // Daily swaps for the last 7 days
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-6);
        var dailyRaw = await _db.SwappingTransactions
            .Where(t => t.CreatedAt >= sevenDaysAgo)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        for (var i = 0; i < 7; i++)
        {
            var day = sevenDaysAgo.AddDays(i);
            charts.DailySwaps.Add(new ChartPointDto
            {
                Label = day.ToString("MM-dd"),
                Value = dailyRaw.FirstOrDefault(x => x.Day == day)?.Count ?? 0
            });
        }

        // Monthly trend for the last 6 months
        var sixMonthsAgo = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
        var monthlyRaw = await _db.SwappingTransactions
            .Where(t => t.CreatedAt >= sixMonthsAgo)
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .ToListAsync(ct);

        for (var i = 0; i < 6; i++)
        {
            var month = sixMonthsAgo.AddMonths(i);
            charts.MonthlyTrend.Add(new ChartPointDto
            {
                Label = month.ToString("yyyy-MM"),
                Value = monthlyRaw.FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0
            });
        }

        // Battery health distribution
        var healthRaw = await _db.Batteries
            .GroupBy(b => b.HealthState)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);
        charts.BatteryHealthDistribution = healthRaw
            .Select(x => new ChartPointDto { Label = x.Key.ToString(), Value = x.Count })
            .ToList();

        // Cabinet status distribution
        var cabinetRaw = await _db.Cabinets
            .GroupBy(c => c.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToListAsync(ct);
        charts.CabinetStatusDistribution = cabinetRaw
            .Select(x => new ChartPointDto { Label = x.Key.ToString(), Value = x.Count })
            .ToList();

        // Battery SOC distribution (buckets of 20%)
        var socBuckets = new[] { "0-20", "21-40", "41-60", "61-80", "81-100" };
        var socValues = await _db.Batteries.Select(b => b.Soc).ToListAsync(ct);
        foreach (var bucket in socBuckets)
        {
            var parts = bucket.Split('-');
            var lo = int.Parse(parts[0]);
            var hi = int.Parse(parts[1]);
            charts.BatterySocDistribution.Add(new ChartPointDto
            {
                Label = bucket + "%",
                Value = socValues.Count(s => s >= lo && s <= hi)
            });
        }

        // Most active stations (top 5 by transaction count)
        charts.MostActiveStations = await _db.SwappingTransactions
            .Include(t => t.Cabinet).ThenInclude(c => c.Station)
            .GroupBy(t => t.Cabinet.Station.Address)
            .Select(g => new ChartPointDto { Label = g.Key, Value = g.Count() })
            .OrderByDescending(x => x.Value)
            .Take(5)
            .ToListAsync(ct);

        return charts;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var overview = new DashboardOverviewDto
        {
            Stats = await GetStatsAsync(ct),
            Charts = await GetChartsAsync(ct),
            LatestTransactions = await _db.SwappingTransactions
                .Include(t => t.User).Include(t => t.Cabinet)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct),
            LatestBatteryLogs = await _db.BatteryLogs
                .OrderByDescending(l => l.RecordedAt)
                .Take(10)
                .ProjectTo<BatteryLogDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct),
            ActiveSessions = await _db.ActiveSwappingSessions
                .Include(s => s.User).Include(s => s.Cabinet)
                .OrderByDescending(s => s.CreatedAt)
                .Take(10)
                .ProjectTo<ActiveSessionDto>(_mapper.ConfigurationProvider)
                .ToListAsync(ct)
        };
        return overview;
    }
}
