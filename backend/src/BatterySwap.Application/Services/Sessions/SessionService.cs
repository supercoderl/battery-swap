using AutoMapper;
using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Extensions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Sessions;
using BatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Sessions;

public interface ISessionService
{
    Task<PagedResult<ActiveSessionDto>> GetPagedAsync(QueryParameters query, CancellationToken ct = default);
    Task ForceCloseAsync(long id, CancellationToken ct = default);
}

public class SessionService : ISessionService
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public SessionService(IApplicationDbContext db, IUnitOfWork uow, IMapper mapper)
    {
        _db = db;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ActiveSessionDto>> GetPagedAsync(QueryParameters query, CancellationToken ct = default)
    {
        var q = _db.ActiveSwappingSessions.Include(s => s.User).Include(s => s.Cabinet).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => s.User.FullName.Contains(term));
        }

        q = q.ApplySort(query.SortBy, query.SortDescending, nameof(ActiveSwappingSession.CreatedAt));
        return await q.ToPagedResultAsync<ActiveSwappingSession, ActiveSessionDto>(_mapper.ConfigurationProvider, query.Page, query.PageSize, ct);
    }

    public async Task ForceCloseAsync(long id, CancellationToken ct = default)
    {
        var session = await _uow.Repository<ActiveSwappingSession>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(ActiveSwappingSession), id);
        _uow.Repository<ActiveSwappingSession>().Remove(session);
        await _uow.SaveChangesAsync(ct);
    }
}
