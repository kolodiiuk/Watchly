using System.Collections.Frozen;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Watchly.Application.Interfaces;
using Watchly.Domain.Entities;
using Watchly.Domain.Enums;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;
using Watchly.Infrastructure.Models;

namespace Watchly.Application.Services;

//todo: update user title progress
// todo: fix incr and decr season
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
                .Where(activity => activity.UserId == userId
                                   && activity.ContentId == id
                                   && activity.ActivityType == ActivityType.Watched)
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

    // done
    public async Task<Result> IncrWatchingCountSeasonAsync(int seasonId, Guid userId, CancellationToken ct)
    {
        try
        {
            var episodeIds = await _dbContext.Episodes
                .Where(e => e.SeasonId == seasonId)
                .Select(e => e.Id)
                .ToListAsync(ct);

            var activities = await _dbContext.UserContentActivities
                .Where(uca => uca.ContentType == ContentType.Episode
                              && episodeIds.Contains(uca.ContentId)
                              && uca.ActivityType == ActivityType.Watched)
                .ToListAsync(ct);
            var alreadyWatchedSet = activities.Select(uca => uca.ContentId).ToFrozenSet();
            var activityDictionary = activities.ToFrozenDictionary(uca => uca.ContentId);
            foreach (var episodeId in episodeIds)
            {
                if (alreadyWatchedSet.Contains(episodeId))
                {
                    var exists = activityDictionary.TryGetValue(episodeId, out var activity);
                    if (!exists)
                    {
                        return Result.Fail("Issue");
                    }

                    var newActivityR = new UserContentActivity
                    {
                        ContentId = episodeId,
                        ActivityType = ActivityType.Watched,
                        ContentType = ContentType.Episode,
                        UserId = userId,
                        WatchCount = activity.WatchCount + 1,
                        WatchedAt = DateTime.UtcNow
                    };
                    _dbContext.Add(newActivityR);
                }

                var newActivity = new UserContentActivity
                {
                    ContentId = episodeId,
                    ActivityType = ActivityType.Watched,
                    ContentType = ContentType.Episode,
                    UserId = userId,
                    WatchCount = 1,
                    WatchedAt = DateTime.UtcNow
                };
                _dbContext.Add(newActivity);
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

    public async Task<Result> IncrWatchingCountEpisodeAsync(int id, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var doesEpisodeExists = await _dbContext.Episodes.AnyAsync(e => e.Id == id, ct);
            if (!doesEpisodeExists)
            {
                return Result.Fail("No episode with such id");
            }

            var existingActivity = await _dbContext.UserContentActivities
                .Where(uca => uca.ContentId == id)
                .Where(uca => uca.UserId == userId)
                .Where(uca => uca.ActivityType == ActivityType.Watched)
                .OrderByDescending(uca => uca.Id)
                .FirstOrDefaultAsync(ct);
            if (existingActivity == null)
            {
                var newUCA = new UserContentActivity
                {
                    ActivityType = ActivityType.Watched,
                    ContentId = id,
                    ContentType = ContentType.Episode,
                    UserId = userId,
                    WatchCount = 1,
                    WatchedAt = DateTime.UtcNow
                };
                _dbContext.Add(newUCA);
                await _dbContext.SaveChangesAsync(ct);

                return Result.Success();
            }

            var newRewatch = new UserContentActivity
            {
                ActivityType = ActivityType.Watched,
                ContentId = id,
                ContentType = ContentType.Episode,
                UserId = userId,
                WatchCount = existingActivity.WatchCount + 1,
                WatchedAt = DateTime.UtcNow
            };
            _dbContext.Add(newRewatch);
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
                .Where(activity => activity.ContentId == id
                                   && activity.UserId == userId
                                   && activity.ActivityType == ActivityType.Watched)
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

    public async Task<Result> DecrWatchingCountSeasonAsync(int seasonId, Guid userId, CancellationToken ct)
    {
        try
        {
            FormattableString sql = $"""
                                     ;WITH episode_ids AS (
                                        SELECT
                                            id
                                        FROM
                                            episodes
                                        WHERE
                                            episodes.season_id = {seasonId}
                                     ), activities AS (
                                        SELECT
                                            DENSE_RANK() OVER (
                                                PARTITION BY uca.content_id
                                                ORDER BY uca.watched_at) AS rank,
                                            uca.*
                                        FROM
                                            user_content_activities uca
                                        WHERE
                                            uca.user_id = {userId} 
                                            AND uca.activity_type = 1 
                                            AND uca.content_id IN (SELECT * FROM episode_ids)
                                     )
                                     SELECT
                                        a.id,
                                        a.content_id,
                                        a.user_id,
                                        a.content_type,
                                        a.activity_type,
                                        a.watched_at,
                                        a.watch_count
                                     FROM 
                                        activities a
                                     WHERE
                                        a.rank = 1
                                     """;

            var episodeIds = await _dbContext.Episodes
                .Where(e => e.SeasonId == seasonId)
                .Select(e => e.Id)
                .ToListAsync(ct);
            var activities = await _dbContext.UserContentActivities.FromSql(sql).ToListAsync(ct);
            var activityDictionary = activities.ToFrozenDictionary(uca => uca.ContentId);
            if (activityDictionary.Count == 0)
            {
                return Result.Fail("No Watched activities");
            }


            foreach (var episodeId in episodeIds)
            {
                if (!activityDictionary.ContainsKey(episodeId))
                {
                    return Result.Success();
                }

                var exists = activityDictionary.TryGetValue(episodeId, out var activity);
                if (!exists)
                {
                    return Result.Fail("Issue");
                }

                switch (activity.WatchCount)
                {
                    case 0:
                        continue;
                    case 1:
                    {
                        var newActivityU = new UserContentActivity
                        {
                            ContentId = episodeId,
                            ActivityType = ActivityType.Watched,
                            ContentType = ContentType.Episode,
                            UserId = userId,
                            WatchCount = 0,
                            WatchedAt = DateTime.UtcNow
                        };
                        _dbContext.Add(newActivityU);
                        break;
                    }
                    default:
                    {
                        var newActivity = new UserContentActivity
                        {
                            ContentId = episodeId,
                            ActivityType = ActivityType.Watched,
                            ContentType = ContentType.Episode,
                            UserId = userId,
                            WatchCount = activity.WatchCount - 1,
                            WatchedAt = DateTime.UtcNow
                        };
                        _dbContext.Add(newActivity);
                        break;
                    }
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
            var doesEpisodeExists = await _dbContext.Episodes.AnyAsync(e => e.Id == id, ct);
            if (!doesEpisodeExists)
            {
                return Result.Fail("No such episode");
            }

            var existingUCA = await _dbContext.UserContentActivities
                .Where(uca => uca.ContentId == id
                              && uca.ActivityType == ActivityType.Watched
                              && uca.UserId == userId)
                .FirstOrDefaultAsync(ct);

            if (existingUCA == null)
            {
                return Result.Fail("Never watched");
            }

            switch (existingUCA.WatchCount)
            {
                case 0: return Result.Fail("Already unwatched");
                case 1:
                {
                    var now = DateTime.UtcNow;
                    var newUnwatched = new UserContentActivity()
                    {
                        ActivityType = ActivityType.Watched,
                        ContentId = id,
                        ContentType = ContentType.Episode,
                        UserId = userId,
                        WatchCount = 0,
                        WatchedAt = now
                    };
                    _dbContext.Add(newUnwatched);
                    break;
                }
                default:
                {
                    var newDecremented = new UserContentActivity
                    {
                        ActivityType = ActivityType.Watched,
                        ContentId = id,
                        ContentType = ContentType.Episode,
                        UserId = userId,
                        WatchCount = existingUCA.WatchCount - 1,
                        WatchedAt = DateTime.UtcNow
                    };
                    _dbContext.Add(newDecremented);
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

    // done
    public async Task<Result<TvShowWatchInfo>> GetWatchCountInfoTvShowAsync(int id, Guid userId,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            FormattableString sql = $"""
                          ;WITH episode_ids AS (
                               SELECT 
                                   id AS episode_id
                               FROM
                                   episodes
                               WHERE
                                   episodes.tv_show_id = {id}
                          ), activities AS (
                              SELECT 
                                  uca.watch_count as "count", 
                                  uca.content_id as "episode_id",
                                  DENSE_RANK() OVER (
                                    PARTITION BY uca.content_id
                                    ORDER BY uca.watched_at DESC) rank
                              FROM 
                                  user_content_activities uca
                              WHERE
                                  uca.user_id = {userId} AND uca.activity_type = 1 AND uca.content_id IN (SELECT * FROM episode_ids)
                          )
                          SELECT
                                a.count,
                                a.episode_id
                          FROM
                                activities a
                          WHERE
                                a.rank = 1
                          """;
            var episodeWatchInfos = await _dbContext.EpisodeWatchInfos
                .FromSql(sql)
                .AsNoTracking()
                .ToListAsync(ct);
            var res = new TvShowWatchInfo(id, episodeWatchInfos);

            return Result<TvShowWatchInfo>.Success(res);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<TvShowWatchInfo>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<TvShowWatchInfo>.Fail($"Error: {e.Message}");
        }
    }

    // done
    public async Task<Result<int>> GetWatchCountInfoMovieAsync(int id, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var count = await _dbContext.UserContentActivities
                .Where(uca => uca.ContentId == id && uca.UserId == userId)
                .OrderByDescending(uca => uca.WatchedAt)
                .Select(uca => uca.WatchCount)
                .FirstOrDefaultAsync(ct);

            return Result<int>.Success(count);
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

    // done
    public async Task<Result<int>> GetWatchCountInfoEpisodeAsync(int episodeId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var count = await _dbContext.UserContentActivities
                .Where(uca => uca.ContentId == episodeId && uca.UserId == userId)
                .OrderByDescending(uca => uca.WatchedAt)
                .Select(uca => uca.WatchCount)
                .FirstOrDefaultAsync(ct);

            return Result<int>.Success(count);
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
