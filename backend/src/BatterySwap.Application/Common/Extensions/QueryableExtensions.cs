using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BatterySwap.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace BatterySwap.Application.Common.Extensions;

public static class QueryableExtensions
{
    /// <summary>Applies reflection-based ordering by property name with a stable fallback.</summary>
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string? sortBy, bool descending, string defaultProperty = "Id")
    {
        var property = ResolveProperty<T>(sortBy) ?? ResolveProperty<T>(defaultProperty);
        if (property is null) return query;

        var param = Expression.Parameter(typeof(T), "x");
        var member = Expression.Property(param, property);
        var keySelector = Expression.Lambda(member, param);

        var method = descending ? "OrderByDescending" : "OrderBy";
        var call = Expression.Call(
            typeof(Queryable), method,
            new[] { typeof(T), property.PropertyType },
            query.Expression, Expression.Quote(keySelector));

        return query.Provider.CreateQuery<T>(call);
    }

    private static PropertyInfo? ResolveProperty<T>(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return typeof(T).GetProperty(name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>Projects to a DTO and materialises a single page.</summary>
    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        IConfigurationProvider mapperConfig,
        int page, int pageSize,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<TDto>(mapperConfig)
            .ToListAsync(ct);

        return new PagedResult<TDto>(items, totalCount, page, pageSize);
    }
}
