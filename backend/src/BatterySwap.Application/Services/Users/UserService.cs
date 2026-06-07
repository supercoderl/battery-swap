using AutoMapper;
using BatterySwap.Application.Common.Exceptions;
using BatterySwap.Application.Common.Extensions;
using BatterySwap.Application.Common.Interfaces;
using BatterySwap.Application.Common.Models;
using BatterySwap.Application.DTOs.Users;
using BatterySwap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Services.Users;

public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(UserQueryParameters query, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(long id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(long id, UpdateUserDto dto, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
}

public class UserQueryParameters : QueryParameters
{
    public bool? IsActive { get; set; }
}

public class UserService : IUserService
{
    private readonly IApplicationDbContext _db;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserService(IApplicationDbContext db, IUnitOfWork uow, IMapper mapper)
    {
        _db = db;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(UserQueryParameters query, CancellationToken ct = default)
    {
        var q = _db.Users.Include(u => u.Transactions).AsNoTracking().AsQueryable();

        if (query.IsActive.HasValue)
            q = q.Where(u => u.IsActive == query.IsActive.Value);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(u => u.FullName.Contains(term) || u.Phone.Contains(term));
        }

        q = q.ApplySort(query.SortBy, query.SortDescending);
        return await q.ToPagedResultAsync<User, UserDto>(_mapper.ConfigurationProvider, query.Page, query.PageSize, ct);
    }

    public async Task<UserDto> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var user = await _db.Users.Include(u => u.Transactions).AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException(nameof(User), id);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(u => u.Phone == dto.Phone, ct))
            throw new ConflictException($"A user with phone '{dto.Phone}' already exists.");

        var user = _mapper.Map<User>(dto);
        await _uow.Repository<User>().AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateAsync(long id, UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);

        if (await _db.Users.AnyAsync(u => u.Phone == dto.Phone && u.Id != id, ct))
            throw new ConflictException($"A user with phone '{dto.Phone}' already exists.");

        _mapper.Map(dto, user);
        _uow.Repository<User>().Update(user);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var user = await _uow.Repository<User>().GetByIdAsync(id, ct)
            ?? throw new NotFoundException(nameof(User), id);
        _uow.Repository<User>().Remove(user);
        await _uow.SaveChangesAsync(ct);
    }
}
