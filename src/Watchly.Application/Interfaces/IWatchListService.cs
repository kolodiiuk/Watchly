using Watchly.Domain.Utils;
using Watchly.Application.Models.Content;
using Watchly.Application.Models.WatchList;

namespace Watchly.Application.Interfaces;

public interface IWatchListService
{
    public Task<Result> AddTitleToWatchListAsync(int titleId, Guid userId, CancellationToken ct);
    public Task<Result> AddTitleToCustWatchListAsync(int titleId, int watchListId, Guid userId, CancellationToken ct);
    public Task<Result> RemoveTitleFromWatchListAsync(int titleId, Guid userId, CancellationToken ct);
    public Task<Result> RemoveTitleFromCustWatchListAsync(int titleId, int watchListId, Guid userId, CancellationToken ct);

    public Task<Result> CreateCustWatchListAsync(string name, Guid userId, CancellationToken ct);
    public Task<Result> DeleteCustWatchListAsync(int watchListId, Guid userId, CancellationToken ct);
    public Task<Result> RenameCustWatchListAsync(int watchListId, string newName, Guid userId, CancellationToken ct);

    public Task<Result<IEnumerable<TitleShortInfo>>> GetTitlesInWatchListAsync(Guid userId, CancellationToken ct);
    public Task<Result<IEnumerable<TitleShortInfo>>> GetTitlesInCustWatchListAsync(int watchListId, Guid userId, CancellationToken ct);

    public Task<Result<IEnumerable<WatchListInfo>>> GetUserWatchListsAsync(Guid userId, CancellationToken ct);
    public Task<Result<IEnumerable<WatchListShortInfo>>> GetWatchListsWithTitleAsync(int titleId, Guid userId, CancellationToken ct);
}