using Watchly.Domain.Utils;
using Watchly.Application.Models.Content;
using Watchly.Application.Models.WatchList;

namespace Watchly.Application.Interfaces;

public interface IWatchListService
{
    public Task<Result> AddTitleToDefaultWatchListAsync(int titleId, Guid userId, CancellationToken ct);
    public Task<Result> AddTitleToWatchListByIdAsync(int titleId, int watchListId, Guid userId, CancellationToken ct);
    public Task<Result> RemoveTitleFromDefaultWatchListAsync(int titleId, Guid userId, CancellationToken ct);
    public Task<Result> RemoveTitleFromWatchListByIdAsync(int titleId, int watchListId, Guid userId, CancellationToken ct);
    public Task<Result> ClearDefaultWatchListAsync(Guid userId, CancellationToken ct);
    public Task<Result> ClearWatchListByIdAsync(int watchListId, Guid userId, CancellationToken ct);

    public Task<Result> CreateCustWatchListAsync(string name, Guid userId, CancellationToken ct);
    public Task<Result> DeleteCustWatchListAsync(int watchListId, Guid userId, CancellationToken ct);
    public Task<Result> RenameCustWatchListAsync(int watchListId, string newName, Guid userId, CancellationToken ct);

    public Task<Result<IEnumerable<TitleShortInfo>>> GetTitlesInDefaultWatchListAsync(Guid userId, CancellationToken ct);
    public Task<Result<IEnumerable<TitleShortInfo>>> GetTitlesInWatchListByIdAsync(int watchListId, Guid userId, CancellationToken ct);

    public Task<Result<IEnumerable<WatchListInfo>>> GetUserWatchListsAsync(Guid userId, CancellationToken ct);
    public Task<Result<IEnumerable<WatchListShortInfo>>> GetWatchListsWithTitleAsync(int titleId, Guid userId, CancellationToken ct);
}