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
                _dbContext.Add(newActivity);
                await UpsertTitleProgressAsync(id, userId, WatchStatus.Completed, ct);
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
            await UpsertTitleProgressAsync(id, userId, WatchStatus.Completed, ct);
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

    public async Task<Result> IncrWatchingCountSeasonAsync(int seasonId, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var seasonInfo = await _dbContext.Seasons
                .Where(s => s.Id == seasonId)
                .Select(s => new
                {
                    s.TitleId,
                    EpisodeIds = s.Episodes.Select(e => e.Id).ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (seasonInfo == null)
            {
                return Result.Fail("Season doesn't exist");
            }

            if (seasonInfo.EpisodeIds.Count == 0)
            {
                return Result.Fail("Season has no episodes");
            }

            var latestActivities = await GetLatestWatchedActivitiesByContentIdAsync(
                seasonInfo.EpisodeIds,
                userId,
                ContentType.Episode,
                ct);
            var now = DateTime.UtcNow;

            foreach (var episodeId in seasonInfo.EpisodeIds)
            {
                var nextWatchCount = latestActivities.TryGetValue(episodeId, out var activity)
                    ? activity.WatchCount + 1
                    : 1;

                _dbContext.Add(new UserContentActivity
                {
                    ContentId = episodeId,
                    ActivityType = ActivityType.Watched,
                    ContentType = ContentType.Episode,
                    UserId = userId,
                    WatchCount = nextWatchCount,
                    WatchedAt = now
                });
            }

            await _dbContext.SaveChangesAsync(ct);
            await UpdateSeriesProgressAsync(seasonInfo.TitleId, userId, ct);
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
            var episodeInfo = await _dbContext.Episodes
                .Where(e => e.Id == id)
                .Select(e => new { e.TvShowId })
                .FirstOrDefaultAsync(ct);
            if (episodeInfo == null)
            {
                return Result.Fail("No episode with such id");
            }

            var existingActivity = await _dbContext.UserContentActivities
                .Where(uca => uca.ContentId == id
                              && uca.ContentType == ContentType.Episode
                              && uca.UserId == userId
                              && uca.ActivityType == ActivityType.Watched)
                .OrderByDescending(uca => uca.WatchedAt)
                .ThenByDescending(uca => uca.Id)
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
                await UpdateSeriesProgressAsync(episodeInfo.TvShowId, userId, ct);
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
            await UpdateSeriesProgressAsync(episodeInfo.TvShowId, userId, ct);
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
                                   && activity.ContentType == ContentType.Movie
                                   && activity.ActivityType == ActivityType.Watched)
                .OrderByDescending(a => a.WatchedAt)
                .ThenByDescending(a => a.Id)
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
                    await UpsertTitleProgressAsync(id, userId, WatchStatus.NotWatched, ct);
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
                    await UpsertTitleProgressAsync(id, userId, WatchStatus.Completed, ct);
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
        ct.ThrowIfCancellationRequested();
        try
        {
            var seasonInfo = await _dbContext.Seasons
                .Where(s => s.Id == seasonId)
                .Select(s => new
                {
                    s.TitleId,
                    EpisodeIds = s.Episodes.Select(e => e.Id).ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (seasonInfo == null)
            {
                return Result.Fail("Season doesn't exist");
            }

            if (seasonInfo.EpisodeIds.Count == 0)
            {
                return Result.Fail("Season has no episodes");
            }

            var latestActivities = await GetLatestWatchedActivitiesByContentIdAsync(
                seasonInfo.EpisodeIds,
                userId,
                ContentType.Episode,
                ct);

            var watchedActivities = latestActivities
                .Where(pair => pair.Value.WatchCount > 0)
                .ToList();

            if (watchedActivities.Count == 0)
            {
                return Result.Fail("No watched activities");
            }

            var now = DateTime.UtcNow;
            foreach (var (episodeId, activity) in watchedActivities)
            {
                _dbContext.Add(new UserContentActivity
                {
                    ContentId = episodeId,
                    ActivityType = ActivityType.Watched,
                    ContentType = ContentType.Episode,
                    UserId = userId,
                    WatchCount = activity.WatchCount - 1,
                    WatchedAt = now
                });
            }

            await _dbContext.SaveChangesAsync(ct);
            await UpdateSeriesProgressAsync(seasonInfo.TitleId, userId, ct);
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
            var episodeInfo = await _dbContext.Episodes
                .Where(e => e.Id == id)
                .Select(e => new { e.TvShowId })
                .FirstOrDefaultAsync(ct);
            if (episodeInfo == null)
            {
                return Result.Fail("No such episode");
            }

            var existingUCA = await _dbContext.UserContentActivities
                .Where(uca => uca.ContentId == id
                              && uca.ContentType == ContentType.Episode
                              && uca.ActivityType == ActivityType.Watched
                              && uca.UserId == userId)
                .OrderByDescending(uca => uca.WatchedAt)
                .ThenByDescending(uca => uca.Id)
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
            await UpdateSeriesProgressAsync(episodeInfo.TvShowId, userId, ct);
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
                                  ROW_NUMBER() OVER (
                                    PARTITION BY uca.content_id
                                    ORDER BY uca.watched_at DESC, uca.id DESC) rank
                              FROM 
                                  user_content_activities uca
                              WHERE
                                  uca.user_id = {userId} AND uca.activity_type = 1 AND uca.content_type = 1 AND uca.content_id IN (SELECT * FROM episode_ids)
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
                .Where(uca => uca.ContentId == id
                              && uca.UserId == userId
                              && uca.ContentType == ContentType.Movie
                              && uca.ActivityType == ActivityType.Watched)
                .OrderByDescending(uca => uca.WatchedAt)
                .ThenByDescending(uca => uca.Id)
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
                .Where(uca => uca.ContentId == episodeId
                              && uca.UserId == userId
                              && uca.ContentType == ContentType.Episode
                              && uca.ActivityType == ActivityType.Watched)
                .OrderByDescending(uca => uca.WatchedAt)
                .ThenByDescending(uca => uca.Id)
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

    public async Task<Result<WatchStatus>> GetTitleWatchStatusAsync(int titleId, Guid userId, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var titleExists = await _dbContext.Titles.AnyAsync(t => t.Id == titleId, ct);
            if (!titleExists)
            {
                return Result<WatchStatus>.Fail("Title doesn't exist");
            }

            var status = await _dbContext.UserTitleProgresses
                .Where(progress => progress.TitleId == titleId && progress.UserId == userId)
                .Select(progress => progress.Status)
                .FirstOrDefaultAsync(ct);

            return Result<WatchStatus>.Success(status);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<WatchStatus>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<WatchStatus>.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> SetTitleWatchStatusAsync(
        int titleId,
        Guid userId,
        WatchStatus status,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var titleExists = await _dbContext.Titles.AnyAsync(t => t.Id == titleId, ct);
            if (!titleExists)
            {
                return Result.Fail("Title doesn't exist");
            }

            await UpsertTitleProgressAsync(titleId, userId, status, ct);
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

    private async Task<Dictionary<int, UserContentActivity>> GetLatestWatchedActivitiesByContentIdAsync(
        IEnumerable<int> contentIds,
        Guid userId,
        ContentType contentType,
        CancellationToken ct)
    {
        var ids = contentIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<int, UserContentActivity>();
        }

        var activities = await _dbContext.UserContentActivities
            .Where(uca => uca.UserId == userId
                          && uca.ContentType == contentType
                          && uca.ActivityType == ActivityType.Watched
                          && ids.Contains(uca.ContentId))
            .OrderByDescending(uca => uca.WatchedAt)
            .ThenByDescending(uca => uca.Id)
            .ToListAsync(ct);

        return activities
            .GroupBy(uca => uca.ContentId)
            .ToDictionary(group => group.Key, group => group.First());
    }

    private async Task UpsertTitleProgressAsync(
        int titleId,
        Guid userId,
        WatchStatus status,
        CancellationToken ct)
    {
        var progress = await _dbContext.UserTitleProgresses
            .FirstOrDefaultAsync(utp => utp.UserId == userId && utp.TitleId == titleId, ct);

        if (progress == null)
        {
            _dbContext.UserTitleProgresses.Add(new UserTitleProgress
            {
                TitleId = titleId,
                UserId = userId,
                Status = status
            });

            return;
        }

        progress.Status = status;
    }

    private async Task UpdateSeriesProgressAsync(int tvShowId, Guid userId, CancellationToken ct)
    {
        var episodeIds = await _dbContext.Episodes
            .Where(e => e.TvShowId == tvShowId)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (episodeIds.Count == 0)
        {
            await UpsertTitleProgressAsync(tvShowId, userId, WatchStatus.NotWatched, ct);
            return;
        }

        var latestActivities = await GetLatestWatchedActivitiesByContentIdAsync(
            episodeIds,
            userId,
            ContentType.Episode,
            ct);
        var watchedEpisodeCount = episodeIds.Count(episodeId =>
            latestActivities.TryGetValue(episodeId, out var activity) && activity.WatchCount > 0);
        var status = watchedEpisodeCount switch
        {
            0 => WatchStatus.NotWatched,
            var count when count == episodeIds.Count => WatchStatus.Completed,
            _ => WatchStatus.Watching
        };

        await UpsertTitleProgressAsync(tvShowId, userId, status, ct);
    }
}
