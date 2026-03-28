using Microsoft.EntityFrameworkCore;
using NexusGrid.Shared.Models;

namespace NexusGrid.Shared.Extensions;

public static class PaginationExtensions
{
    public static async Task<PaginatedResponse<T>> ToPaginatedResponseAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<T>(items, page, pageSize, totalCount, totalPages);
    }
}
