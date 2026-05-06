using Microsoft.EntityFrameworkCore;
using Npgsql;
using Watchly.Application.Interfaces;
using Watchly.Domain.Entities;
using Watchly.Domain.Enums;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public class WatchTrackingService : IWatchTrackingService
{
    private readonly WatchlyDbContext _dbContext;

    public WatchTrackingService(WatchlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> IncrWatchingCountMovieAsync(int id, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var doesMovieExists = await _dbContext.Titles
                .AnyAsync(title => title.Id == id && title.ContentType == TitleType.Movie, ct);
            if (!doesMovieExists)
            {
                return Result.Fail("Movie doesn't exist");
            }

            var existingActivity = await _dbContext.UserContentActivities
                .Where(activity => activity.ContentId == id && activity.UserId == userId)
                .OrderByDescending(a => a.WatchedAt)
                .Take(1)
                .FirstOrDefaultAsync(ct);
            if (existingActivity == null)
            {
                var newActivity = new UserContentActivity
                {
                    ContentType = ContentType.Movie,
                    ContentId = id,
                    UserId = userId,
                    ActivityType = ActivityType.Watched,
                    WatchedAt = DateTime.UtcNow,
                    WatchCount = 1
                };
                var newTitleProgress = new UserTitleProgress
                {
                    TitleId = id,
                    UserId = userId,
                    Status = WatchStatus.Completed
                };
                _dbContext.Add(newActivity);
                _dbContext.Add(newTitleProgress);
                await _dbContext.SaveChangesAsync(ct);

                return Result.Success();
            }

            var newActivityR = new UserContentActivity
            {
                ContentType = ContentType.Movie,
                ContentId = id,
                UserId = userId,
                ActivityType = ActivityType.Watched,
                WatchedAt = DateTime.UtcNow,
                WatchCount = existingActivity.WatchCount + 1
            };
            _dbContext.Add(newActivityR);
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

    public async Task<Result> IncrWatchingCountEpisodeAsync(int id, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            throw new NotImplementedException();
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

    public async Task<Result> DecrWatchingCountMovieAsync(int id, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var doesMovieExists = await _dbContext.Titles
                .AnyAsync(title => title.Id == id && title.ContentType == TitleType.Movie, ct);
            if (!doesMovieExists)
            {
                return Result.Fail("Movie doesn't exist");
            }

            var existingActivity = await _dbContext.UserContentActivities
                .Where(activity => activity.ContentId == id && activity.UserId == userId)
                .OrderByDescending(a => a.WatchedAt)
                .Take(1)
                .FirstOrDefaultAsync(ct);
            if (existingActivity == null)
            {
                return Result.Fail("Never watched");
            }

            switch (existingActivity.WatchCount)
            {
                case 0:
                    return Result.Fail("Already unwatched");
                case 1:
                {
                    var titleProgress = await _dbContext.UserTitleProgresses
                        .Where(utp => utp.UserId == userId && utp.TitleId == id)
                        .FirstOrDefaultAsync(ct);
                    titleProgress.Status = WatchStatus.NotWatched;
                    var newActivityUnwatched = new UserContentActivity
                    {
                        ContentType = ContentType.Movie,
                        ContentId = id,
                        UserId = userId,
                        WatchCount = 0,
                        ActivityType = ActivityType.Watched,
                        WatchedAt = DateTime.UtcNow
                    };
                    _dbContext.Add(newActivityUnwatched);
                    break;
                }
                default:
                {
                    var newActivityRU = new UserContentActivity
                    {
                        ContentType = ContentType.Movie,
                        ContentId = id,
                        UserId = userId,
                        ActivityType = ActivityType.Watched,
                        WatchedAt = DateTime.UtcNow,
                        WatchCount = existingActivity.WatchCount - 1
                    };
                    _dbContext.Add(newActivityRU);
                    break;
                }
            }

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

    public async Task<Result> DecrWatchingCountEpisodeAsync(int id, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            throw new NotImplementedException();
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

    public async Task<Result<IEnumerable<TvShowWatchInfo>>> GetWatchCountInfoTvShowAsync(int id, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            throw new NotImplementedException();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<IEnumerable<TvShowWatchInfo>>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<IEnumerable<TvShowWatchInfo>>.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result<int>> GetWatchCountInfoMovieAsync(int id, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            throw new NotImplementedException();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<int>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<int>.Fail($"Error: {e.Message}");
        }
    }
}
