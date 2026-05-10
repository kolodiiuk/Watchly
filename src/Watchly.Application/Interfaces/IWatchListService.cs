using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IWatchListService
{
    public Task<Result> AddTitleToWatchListAsync(int titleId, Guid userId, CancellationToken ct);
    public Task<Result> RemoveTitleFromWatchListAsync(int titleId, Guid userId, CancellationToken ct);
    public Task<Result> CreateCustWatchListAsync(string name, Guid userId, CancellationToken ct);
    public Task<Result> AddTitleToCustWatchListAsync(
        int titleId, int watchListId, CancellationToken ct);
    public Task<Result> RemoveTitleFromCustWatchListAsync(
        int titleId, int watchListId, CancellationToken ct);
}

