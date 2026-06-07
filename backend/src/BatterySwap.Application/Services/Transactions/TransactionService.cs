using AutoMapper;
using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Extensions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Transactions;
using BatterySwap.Domain.Entities;
using BatterySwap.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Transactions;

public interface ITransactionService
{
    Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionQueryParameters query, CancellationToken ct = default);
    Task<TransactionDto> GetByIdAsync(long id, CancellationToken ct = default);
}

public class TransactionQueryParameters : QueryParameters
{
    public long? UserId { get; set; }
    public long? CabinetId { get; set; }
    public SwappingTransactionStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class TransactionService : ITransactionService
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public TransactionService(IApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResult<TransactionDto>> GetPagedAsync(TransactionQueryParameters query, CancellationToken ct = default)
    {
        var q = _db.SwappingTransactions.Include(t => t.User).Include(t => t.Cabinet).AsNoTracking().AsQueryable();

        if (query.UserId.HasValue) q = q.Where(t => t.UserId == query.UserId.Value);
        if (query.CabinetId.HasValue) q = q.Where(t => t.CabinetId == query.CabinetId.Value);
        if (query.Status.HasValue) q = q.Where(t => t.Status == query.Status.Value);
        if (query.FromDate.HasValue) q = q.Where(t => t.CreatedAt >= query.FromDate.Value);
        if (query.ToDate.HasValue) q = q.Where(t => t.CreatedAt <= query.ToDate.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(t => t.User.FullName.Contains(term) || t.Cabinet.CabinetModel.Contains(term));
        }

        q = q.ApplySort(query.SortBy, query.SortDescending, nameof(SwappingTransaction.CreatedAt));
        return await q.ToPagedResultAsync<SwappingTransaction, TransactionDto>(_mapper.ConfigurationProvider, query.Page, query.PageSize, ct);
    }

    public async Task<TransactionDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var tx = await _db.SwappingTransactions.Include(t => t.User).Include(t => t.Cabinet).AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new NotFoundException(nameof(SwappingTransaction), id);
        return _mapper.Map<TransactionDto>(tx);
    }
}
