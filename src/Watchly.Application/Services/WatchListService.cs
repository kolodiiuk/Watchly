using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Content;
using Watchly.Application.Models.WatchList;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public sealed class WatchListService : IWatchListService
{
    private readonly WatchlyDbContext _dbContext;

    public WatchListService(WatchlyDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Result> AddTitleToDefaultWatchListAsync(int titleId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            int watchListId = GetUserDefaultWatchListId(userId);

            return await AddTitleToWatchListByIdAsync(titleId, watchListId, userId, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"{e.Message}");
        }
    }

    public async Task<Result> AddTitleToWatchListByIdAsync(int titleId, int watchListId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            if(IsUserWatchListOwner(watchListId, userId) == false)
            {
                return Result.Fail("User does not exist or does not own a watchlist with given id");
            }

            var title = await _dbContext.Titles.FindAsync([titleId], ct);
            if (title is null)
            {
                return Result.Fail("No such title");
            }

            var watchList = await _dbContext.WatchLists
                .FirstOrDefaultAsync(
                    w => w.Id == watchListId,
                    ct);
            if (watchList is null)
            {
                return Result.Fail("No such watch list");
            }

            var item = await _dbContext.WatchListItems.FirstOrDefaultAsync(
                i => i.TitleId == titleId &&
                i.WatchList.Id == watchListId,
                ct);

            if (item is not null)
            {
                return Result.Fail("This title is already in the watch list");
            }

            item = new WatchListItem
            {
                TitleId = titleId,
                WatchListId = watchList.Id
            };

            await _dbContext.AddAsync(item, ct);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"{e.Message}");
        }
    }

    public async Task<Result> RemoveTitleFromDefaultWatchListAsync(int titleId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            int watchListId = GetUserDefaultWatchListId(userId);

            return await RemoveTitleFromWatchListByIdAsync(titleId, watchListId, userId, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> RemoveTitleFromWatchListByIdAsync(int titleId, int watchListId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            if (IsUserWatchListOwner(watchListId, userId) == false)
            {
                return Result.Fail("User does not exist or does not own a watchlist with given id");
            }

            var item = await _dbContext.WatchListItems.FirstOrDefaultAsync(
                i => i.Title.Id == titleId &&
                i.WatchList.Id == watchListId,
                ct);

            if (item is null)
            {
                return Result.Fail("There is no such movie in user's watchlist");
            }

            _dbContext.Remove(item);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> ClearDefaultWatchListAsync(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            int watchListId = GetUserDefaultWatchListId(userId);
            return await ClearWatchListByIdAsync(watchListId, userId, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> ClearWatchListByIdAsync(int watchListId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            if (IsUserWatchListOwner(watchListId, userId) == false)
            {
                return Result.Fail("User does not exist or does not own a watchlist with given id");
            }
            var items = await _dbContext.WatchListItems
                .Where(i => i.WatchList.Id == watchListId)
                .ToListAsync(ct);
            _dbContext.RemoveRange(items);
            await _dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> CreateCustWatchListAsync(string name, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {   if(name.IsNullOrEmpty())
            {
                return Result.Fail("Watch list name cannot be empty");
            }

            if(name == "Default")
            {
                return Result.Fail("Custom watch list name cannot be 'Default'");
            }

            var newList = new WatchList
            {
                Name = name,
                UserId = userId
            };

            await _dbContext.AddAsync(newList, ct);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"{e.Message}");
        }
    }
 
    public async Task<Result> DeleteCustWatchListAsync(int watchListId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            if (IsUserWatchListOwner(watchListId, userId) == false)
            {
                return Result.Fail("User does not exist or does not own a watchlist with given id");
            }

            var watchList = await _dbContext.WatchLists
                .FirstOrDefaultAsync(
                    w => w.Id == watchListId,
                    ct);
         
            if (watchList.Name == "Default")
            {
                return Result.Fail("Cannot delete default watch list");
            }

            _dbContext.Remove(watchList);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"{e.Message}");
        }
    }

    public async Task<Result> RenameCustWatchListAsync(int watchListId, string newName, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            if(string.IsNullOrWhiteSpace(newName))
            {
                return Result.Fail("Watch list name cannot be empty");
            }
            if(newName == "Default")
            {
                return Result.Fail("Custom watch list name cannot be 'Default'");
            }

            var watchList = await _dbContext.WatchLists
                .FirstOrDefaultAsync(
                    w => w.Id == watchListId,
                    ct);

            if (watchList is null)
            {
                return Result.Fail("No such watch list");
            }

            watchList.Name = newName;
            _dbContext.Update(watchList);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"{e.Message}");
        }
    }

    public async Task<Result<IEnumerable<TitleShortInfo>>> GetTitlesInDefaultWatchListAsync(Guid userId, CancellationToken ct)
    {
        var defaultWatchListId = GetUserDefaultWatchListId(userId);

        return await GetTitlesInWatchListByIdAsync(defaultWatchListId, userId, ct);
    }

    public async Task<Result<IEnumerable<TitleShortInfo>>> GetTitlesInWatchListByIdAsync(int watchListId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            if (IsUserWatchListOwner(watchListId, userId) == false)
            {
                return Result<IEnumerable<TitleShortInfo>>.Fail("User does not exist or does not own a watchlist with given id");
            }

            var titles = await _dbContext.WatchListItems
                .Where(i => i.WatchList.Id == watchListId && i.WatchList.UserId == userId)
                .Select(i => new TitleShortInfo(
                    i.Title.Id,
                    i.Title.Name,
                    i.Title.PosterUrl,
                    i.Title.AvgTmdbRating))
                .ToListAsync(ct);

            return Result<IEnumerable<TitleShortInfo>>.Success(titles);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<IEnumerable<TitleShortInfo>>.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<IEnumerable<TitleShortInfo>>.Fail($"{e.Message}");
        }
    }

    public async Task<Result<IEnumerable<WatchListInfo>>> GetUserWatchListsAsync(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var watchLists = await _dbContext.WatchLists
                .Where(w => w.UserId == userId)
                .Select(w => new WatchListInfo(
                    w.Id, 
                    w.Name,
                    w.WatchListItems.Select(i => new TitleShortInfo(
                        i.Title.Id,
                        i.Title.Name,
                        i.Title.PosterUrl,
                        i.Title.AvgTmdbRating)
                    ))
                ).ToListAsync(ct);

            return Result<IEnumerable<WatchListInfo>>.Success(watchLists);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<IEnumerable<WatchListInfo>>.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<IEnumerable<WatchListInfo>>.Fail($"{e.Message}");
        }
    }

    public async Task<Result<IEnumerable<WatchListShortInfo>>> GetWatchListsWithTitleAsync(int titleId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var watchLists = await _dbContext.WatchListItems
                .Where(i => i.Title.Id == titleId && i.WatchList.UserId == userId)
                .Select(i => new WatchListShortInfo(i.WatchList.Id, i.WatchList.Name))
                .ToListAsync(ct);

            return Result<IEnumerable<WatchListShortInfo>>.Success(watchLists);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<IEnumerable<WatchListShortInfo>>.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<IEnumerable<WatchListShortInfo>>.Fail($"{e.Message}");
        }
    }

    private int GetUserDefaultWatchListId(Guid userId)
    {
        var watchList = _dbContext.WatchLists
            .FirstOrDefault(w => w.UserId == userId && w.Name == "Default");

        if (watchList is null)
        {
            watchList = new WatchList
            {
                Name = "Default",
                UserId = userId
            };
            _dbContext.Add(watchList);
            _dbContext.SaveChanges();
        }
        return watchList.Id;
    }

    private bool IsUserWatchListOwner(int watchListId, Guid userId)
    {
        var user = _dbContext.Users
            .FirstOrDefault(u => u.Id == userId);

        if (user is null)
        {
            return false;
        }

        var watchList = _dbContext.WatchLists
            .FirstOrDefault(w => w.Id == watchListId);

        return watchList is not null && watchList.UserId == userId;
    }
}

