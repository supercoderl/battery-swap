using AutoMapper;
using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Extensions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Slots;
using BatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Slots;

public interface ISlotService
{
    Task<PagedResult<SlotDto>> GetPagedAsync(SlotQueryParameters query, CancellationToken ct = default);
    Task<SlotDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<SlotDto> CreateAsync(CreateSlotDto dto, CancellationToken ct = default);
    Task<SlotDto> UpdateAsync(long id, UpdateSlotDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}

public class SlotQueryParameters : QueryParameters
{
    public long? CabinetId { get; set; }
    public bool? IsHardwareLocked { get; set; }
}

public class SlotService : ISlotService
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SlotService(IApplicationDbContext db, IUnitOfWork uow, IMapper mapper)
    {
        _db = db;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<SlotDto>> GetPagedAsync(SlotQueryParameters query, CancellationToken ct = default)
    {
        var q = _db.Slots.Include(s => s.Cabinet).Include(s => s.CurrentBattery).AsNoTracking().AsQueryable();

        if (query.CabinetId.HasValue)
            q = q.Where(s => s.CabinetId == query.CabinetId.Value);
        if (query.IsHardwareLocked.HasValue)
            q = q.Where(s => s.IsHardwareLocked == query.IsHardwareLocked.Value);

        q = q.ApplySort(query.SortBy, query.SortDescending, nameof(Slot.SlotNumber));
        return await q.ToPagedResultAsync<Slot, SlotDto>(_mapper.ConfigurationProvider, query.Page, query.PageSize, ct);
    }

    public async Task<SlotDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var slot = await _db.Slots.Include(s => s.Cabinet).Include(s => s.CurrentBattery).AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException(nameof(Slot), id);
        return _mapper.Map<SlotDto>(slot);
    }

    public async Task<SlotDto> CreateAsync(CreateSlotDto dto, CancellationToken ct = default)
    {
        if (!await _db.Cabinets.AnyAsync(c => c.Id == dto.CabinetId, ct))
            throw new NotFoundException(nameof(Cabinet), dto.CabinetId);

        if (await _db.Slots.AnyAsync(s => s.CabinetId == dto.CabinetId && s.SlotNumber == dto.SlotNumber, ct))
            throw new ConflictException($"Slot number {dto.SlotNumber} already exists in this cabinet.");

        var slot = _mapper.Map<Slot>(dto);
        await _uow.Repository<Slot>().AddAsync(slot, ct);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(slot.Id, ct);
    }

    public async Task<SlotDto> UpdateAsync(long id, UpdateSlotDto dto, CancellationToken ct = default)
    {
        var slot = await _uow.Repository<Slot>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Slot), id);

        if (await _db.Slots.AnyAsync(s => s.CabinetId == slot.CabinetId && s.SlotNumber == dto.SlotNumber && s.Id != id, ct))
            throw new ConflictException($"Slot number {dto.SlotNumber} already exists in this cabinet.");

        _mapper.Map(dto, slot);
        _uow.Repository<Slot>().Update(slot);
        await _uow.SaveChangesAsync(ct);
        return await GetByIdAsync(slot.Id, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var slot = await _uow.Repository<Slot>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(Slot), id);
        _uow.Repository<Slot>().Remove(slot);
        await _uow.SaveChangesAsync(ct);
    }
}
