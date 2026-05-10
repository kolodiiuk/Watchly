using Microsoft.EntityFrameworkCore;
using Npgsql;
using Watchly.Application.Interfaces;
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
}

