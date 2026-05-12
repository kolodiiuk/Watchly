using Microsoft.EntityFrameworkCore;
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

    public async Task<Result> AddTitleToWatchListAsync(int titleId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var title = await _dbContext.Titles.FindAsync([titleId], ct);
            if (title is null)
            {
                return Result.Fail("No such title");
            }

            var watchList = await _dbContext.WatchLists
                .FirstOrDefaultAsync(
                    w => w.UserId == userId &&
                    w.Name == "Default",
                    ct);
            if (watchList is null)
            {
                watchList = new WatchList
                {
                    Name = "Default",
                    UserId = userId
                };
                await _dbContext.AddAsync(watchList, ct);
                await _dbContext.SaveChangesAsync(ct);
            }

            var item = new WatchListItem
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
    public async Task<Result> RemoveTitleFromWatchListAsync(int titleId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
            {
                return Result.Fail("user is not found");
            }

            var item = await _dbContext.WatchListItems.FirstOrDefaultAsync(
                i => i.Id == titleId &&
                i.WatchList.UserId == userId,
                ct);

            if (item is null)
            {
                return Result.Fail("There is no such title in user's watchlist");
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
    public async Task<Result> CreateCustWatchListAsync(string name, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {   
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

    public async Task<Result> AddTitleToCustWatchListAsync(int titleId, int watchListId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
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

            var item = new WatchListItem
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

    public async Task<Result> RemoveTitleFromCustWatchListAsync(int titleId, int watchListId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var item = await _dbContext.WatchListItems.FirstOrDefaultAsync(
                i => i.Id == titleId &&
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

    public async Task<Result> DeleteCustWatchListAsync(int watchListId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var watchList = await _dbContext.WatchLists
                .FirstOrDefaultAsync(
                    w => w.Id == watchListId,
                    ct);
         
            if (watchList is null)
            {
                return Result.Fail("No such watch list");
            }
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

    public async Task<Result> RenameCustWatchListAsync(int watchListId, string newName, CancellationToken ct)
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
            if (watchList.Name == "Default")
            {
                return Result.Fail("Cannot rename default watch list");
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

    public async Task<Result<IEnumerable<TitleShortInfo>>> GetTitlesInWatchListAsync(Guid userId, CancellationToken ct)
    {
        var defaultWatchList = await _dbContext.WatchLists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Name == "Default", ct);

        if (defaultWatchList is null)
        {
            return Result<IEnumerable<TitleShortInfo>>.Success(Enumerable.Empty<TitleShortInfo>());
        }

        return await GetTitlesInCustWatchListAsync(defaultWatchList.Id, userId, ct);
    }

    public async Task<Result<IEnumerable<TitleShortInfo>>> GetTitlesInCustWatchListAsync(int watchListId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
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
            var watchList = await _dbContext.WatchLists
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

            return Result<IEnumerable<WatchListInfo>>.Success(watchList);
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
}

