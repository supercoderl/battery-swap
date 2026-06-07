using AutoMapper;
using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Extensions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Stations;
using BatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Stations;

public class StationService : IStationService
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StationService(IApplicationDbContext db, IUnitOfWork uow, IMapper mapper)
    {
        _db = db;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<StationDto>> GetPagedAsync(QueryParameters query, CancellationToken ct = default)
    {
        var q = _db.Stations.Include(s => s.Cabinets).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => s.Address.Contains(term));
        }

        q = q.ApplySort(query.SortBy, query.SortDescending);
        return await q.ToPagedResultAsync<Station, StationDto>(_mapper.ConfigurationProvider, query.Page, query.PageSize, ct);
    }

    public async Task<StationDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var station = await _db.Stations.Include(s => s.Cabinets).AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(nameof(Station), id);
        return _mapper.Map<StationDto>(station);
    }

    public async Task<StationDto> CreateAsync(CreateStationDto dto, CancellationToken ct = default)
    {
        var station = _mapper.Map<Station>(dto);
        await _uow.Repository<Station>().AddAsync(station, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<StationDto>(station);
    }

    public async Task<StationDto> UpdateAsync(long id, UpdateStationDto dto, CancellationToken ct = default)
    {
        var station = await _uow.Repository<Station>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Station), id);

        _mapper.Map(dto, station);
        _uow.Repository<Station>().Update(station);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<StationDto>(station);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var station = await _uow.Repository<Station>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Station), id);

        var hasCabinets = await _db.Cabinets.AnyAsync(c => c.StationId == id, ct);
        if (hasCabinets)
            throw new ConflictException("Cannot delete a station that still has cabinets.");

        _uow.Repository<Station>().Remove(station);
        await _uow.SaveChangesAsync(ct);
    }
}
