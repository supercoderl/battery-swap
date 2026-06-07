using AutoMapper;
using AutoMapper.QueryableExtensions;
using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Extensions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Batteries;
using BatterySwap.Domain.Entities;
using BatterySwap.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Batteries;

public interface IBatteryService
{
    Task<PagedResult<BatteryDto>> GetPagedAsync(BatteryQueryParameters query, CancellationToken ct = default);
    Task<BatteryDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<BatteryDto> CreateAsync(CreateBatteryDto dto, CancellationToken ct = default);
    Task<BatteryDto> UpdateAsync(long id, UpdateBatteryDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<List<BatteryLogDto>> GetLogsAsync(long batteryId, int take, CancellationToken ct = default);
}

public class BatteryQueryParameters : QueryParameters
{
    public BatteryHealthState? HealthState { get; set; }
    public BatteryLocationType? LocationType { get; set; }
}

public class BatteryService : IBatteryService
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public BatteryService(IApplicationDbContext db, IUnitOfWork uow, IMapper mapper)
    {
        _db = db;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<BatteryDto>> GetPagedAsync(BatteryQueryParameters query, CancellationToken ct = default)
    {
        var q = _db.Batteries.AsNoTracking().AsQueryable();

        if (query.HealthState.HasValue)
            q = q.Where(b => b.HealthState == query.HealthState.Value);
        if (query.LocationType.HasValue)
            q = q.Where(b => b.LocationType == query.LocationType.Value);
        if (!string.IsNullOrWhiteSpace(query.Search) && long.TryParse(query.Search.Trim(), out var bid))
            q = q.Where(b => b.Id == bid);

        q = q.ApplySort(query.SortBy, query.SortDescending);
        return await q.ToPagedResultAsync<Battery, BatteryDto>(_mapper.ConfigurationProvider, query.Page, query.PageSize, ct);
    }

    public async Task<BatteryDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var battery = await _db.Batteries.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct)
            ?? throw new NotFoundException(nameof(Battery), id);
        return _mapper.Map<BatteryDto>(battery);
    }

    public async Task<BatteryDto> CreateAsync(CreateBatteryDto dto, CancellationToken ct = default)
    {
        var battery = _mapper.Map<Battery>(dto);
        battery.UpdatedAt = DateTime.UtcNow;
        await _uow.Repository<Battery>().AddAsync(battery, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<BatteryDto>(battery);
    }

    public async Task<BatteryDto> UpdateAsync(long id, UpdateBatteryDto dto, CancellationToken ct = default)
    {
        var battery = await _uow.Repository<Battery>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Battery), id);

        _mapper.Map(dto, battery);
        battery.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Battery>().Update(battery);

        // Record telemetry snapshot on every update.
        await _uow.Repository<BatteryLog>().AddAsync(new BatteryLog
        {
            BatteryId = battery.Id,
            Soc = battery.Soc,
            Temperature = battery.Temperature,
            Voltage = battery.Voltage,
            RecordedAt = DateTime.UtcNow
        }, ct);

        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<BatteryDto>(battery);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var battery = await _uow.Repository<Battery>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Battery), id);
        _uow.Repository<Battery>().Remove(battery);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<List<BatteryLogDto>> GetLogsAsync(long batteryId, int take, CancellationToken ct = default)
    {
        if (!await _db.Batteries.AnyAsync(b => b.Id == batteryId, ct))
            throw new NotFoundException(nameof(Battery), batteryId);

        return await _db.BatteryLogs.AsNoTracking()
            .Where(l => l.BatteryId == batteryId)
            .OrderByDescending(l => l.RecordedAt)
            .Take(take <= 0 ? 100 : take)
            .ProjectTo<BatteryLogDto>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);
    }
}
