using AutoMapper;
using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Extensions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Cabinets;
using BatterySwap.Domain.Entities;
using BatterySwap.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Cabinets;

public interface ICabinetService
{
    Task<PagedResult<CabinetDto>> GetPagedAsync(CabinetQueryParameters query, CancellationToken ct = default);
    Task<CabinetDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<CabinetDto> CreateAsync(CreateCabinetDto dto, CancellationToken ct = default);
    Task<CabinetDto> UpdateAsync(long id, UpdateCabinetDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}

public class CabinetQueryParameters : QueryParameters
{
    public long? StationId { get; set; }
    public CabinetStatus? Status { get; set; }
}

public class CabinetService : ICabinetService
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CabinetService(IApplicationDbContext db, IUnitOfWork uow, IMapper mapper)
    {
        _db = db;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<CabinetDto>> GetPagedAsync(CabinetQueryParameters query, CancellationToken ct = default)
    {
        var q = _db.Cabinets.Include(c => c.Station).Include(c => c.Slots).AsNoTracking().AsQueryable();

        if (query.StationId.HasValue)
            q = q.Where(c => c.StationId == query.StationId.Value);
        if (query.Status.HasValue)
            q = q.Where(c => c.Status == query.Status.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(c => c.CabinetModel.Contains(term));
        }

        q = q.ApplySort(query.SortBy, query.SortDescending);
        return await q.ToPagedResultAsync<Cabinet, CabinetDto>(_mapper.ConfigurationProvider, query.Page, query.PageSize, ct);
    }

    public async Task<CabinetDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var cabinet = await _db.Cabinets.Include(c => c.Station).Include(c => c.Slots).AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException(nameof(Cabinet), id);
        return _mapper.Map<CabinetDto>(cabinet);
    }

    public async Task<CabinetDto> CreateAsync(CreateCabinetDto dto, CancellationToken ct = default)
    {
        if (!await _db.Stations.AnyAsync(s => s.Id == dto.StationId, ct))
            throw new NotFoundException(nameof(Station), dto.StationId);

        var cabinet = _mapper.Map<Cabinet>(dto);
        await _uow.Repository<Cabinet>().AddAsync(cabinet, ct);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(cabinet.Id, ct);
    }

    public async Task<CabinetDto> UpdateAsync(long id, UpdateCabinetDto dto, CancellationToken ct = default)
    {
        var cabinet = await _uow.Repository<Cabinet>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cabinet), id);

        if (!await _db.Stations.AnyAsync(s => s.Id == dto.StationId, ct))
            throw new NotFoundException(nameof(Station), dto.StationId);

        _mapper.Map(dto, cabinet);
        _uow.Repository<Cabinet>().Update(cabinet);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(cabinet.Id, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var cabinet = await _uow.Repository<Cabinet>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Cabinet), id);

        if (await _db.Slots.AnyAsync(s => s.CabinetId == id, ct))
            throw new ConflictException("Cannot delete a cabinet that still has slots.");

        _uow.Repository<Cabinet>().Remove(cabinet);
        await _uow.SaveChangesAsync(ct);
    }
}
