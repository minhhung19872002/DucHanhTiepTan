using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Common;

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(this IQueryable<T> query, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 500);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<T>(items, total, page, pageSize);
    }

    /// <summary>Accent/case-insensitive "contains" using the PostgreSQL unaccent extension.</summary>
    public static IQueryable<T> WhereUnaccentLike<T>(this IQueryable<T> query, System.Linq.Expressions.Expression<Func<T, string?>> selector, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return query;
        var pattern = "%" + VietnameseText.Normalize(keyword).Replace("%", "\\%") + "%";
        var param = selector.Parameters[0];
        var body = System.Linq.Expressions.Expression.Call(
            typeof(NpgsqlFullTextSearchDbFunctionsExtensions).GetMethod(nameof(NpgsqlFullTextSearchDbFunctionsExtensions.Unaccent), [typeof(DbFunctions), typeof(string)])!,
            System.Linq.Expressions.Expression.Constant(EF.Functions),
            selector.Body);
        var ilike = System.Linq.Expressions.Expression.Call(
            typeof(NpgsqlDbFunctionsExtensions).GetMethod(nameof(NpgsqlDbFunctionsExtensions.ILike), [typeof(DbFunctions), typeof(string), typeof(string)])!,
            System.Linq.Expressions.Expression.Constant(EF.Functions),
            body,
            System.Linq.Expressions.Expression.Constant(pattern));
        var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(ilike, param);
        return query.Where(lambda);
    }
}
