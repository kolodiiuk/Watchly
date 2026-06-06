using Microsoft.EntityFrameworkCore;
using Npgsql;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Stats;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;
using Watchly.Infrastructure.Models;

namespace Watchly.Application.Services;

public class UserStatsService : IUserStatsService
{
    private readonly WatchlyDbContext _dbContext;

    public UserStatsService(WatchlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<MovieStatsResponse>> GetUserMovieStatsAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            FormattableString movieAggregateSql = $"""
                                                    WITH latest_movies AS (
                                                        SELECT
                                                            ranked.content_id AS title_id,
                                                            ranked.watch_count
                                                        FROM (
                                                            SELECT
                                                                uca.id,
                                                                uca.content_id,
                                                                uca.watch_count,
                                                                ROW_NUMBER() OVER (
                                                                    PARTITION BY uca.content_id
                                                                    ORDER BY uca.watched_at DESC, uca.id DESC
                                                                ) AS row_num
                                                            FROM user_content_activities uca
                                                            WHERE
                                                                uca.user_id = {userId}
                                                                AND uca.content_type = 0
                                                                AND uca.activity_type = 1
                                                        ) ranked
                                                        WHERE ranked.row_num = 1 AND ranked.watch_count > 0
                                                    )
                                                    SELECT
                                                         COUNT(*)::int AS movie_count,
                                                         COALESCE(SUM(lm.watch_count * t.runtime), 0)::int AS minutes_watched
                                                    FROM latest_movies lm
                                                    JOIN titles t ON t.id = lm.title_id
                                                    WHERE t.content_type = 1 AND t.is_deleted = FALSE
                                                    """;
            var aggregateRow = await _dbContext.MovieStatsAggregateRows
                .FromSql(movieAggregateSql)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct) ?? new MovieStatsAggregateRow();

            FormattableString movieTopGenresSql = $"""
                                                    WITH latest_movies AS (
                                                        SELECT
                                                            ranked.content_id AS title_id,
                                                            ranked.watch_count
                                                        FROM (
                                                            SELECT
                                                                uca.id,
                                                                uca.content_id,
                                                                uca.watch_count,
                                                                ROW_NUMBER() OVER (
                                                                    PARTITION BY uca.content_id
                                                                    ORDER BY uca.watched_at DESC, uca.id DESC
                                                                ) AS row_num
                                                            FROM user_content_activities uca
                                                            WHERE
                                                                uca.user_id = {userId}
                                                                AND uca.content_type = 0
                                                                AND uca.activity_type = 1
                                                        ) ranked
                                                        WHERE ranked.row_num = 1 AND ranked.watch_count > 0
                                                    )
                                                    SELECT
                                                        g.name
                                                    FROM latest_movies lm
                                                    JOIN titles t ON t.id = lm.title_id
                                                    JOIN title_genres tg ON tg.title_id = lm.title_id
                                                    JOIN genres g ON g.id = tg.genre_id
                                                    WHERE tg.is_tv_show = FALSE
                                                      AND t.content_type = 1
                                                      AND t.is_deleted = FALSE
                                                    GROUP BY g.name
                                                    ORDER BY SUM(lm.watch_count) DESC, COUNT(*) DESC, g.name
                                                    LIMIT 5
                                                    """;
            var topGenres = await _dbContext.GenreNameRows
                .FromSql(movieTopGenresSql)
                .AsNoTracking()
                .Select(x => x.Name)
                .ToListAsync(ct);
            var movieStats = new MovieStatsResponse
            {
                MovieCount = aggregateRow.MovieCount,
                TopGenres = topGenres
            };
            movieStats.FillDurationStats(aggregateRow.MinutesWatched);

            return Result<MovieStatsResponse>.Success(movieStats);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<MovieStatsResponse>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<MovieStatsResponse>.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result<SeriesStatsResponse>> GetUserTvSeriesStatsAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            FormattableString seriesAggregateSql = $"""
                                                    WITH latest_episodes AS (
                                                        SELECT
                                                            ranked.content_id,
                                                            ranked.watch_count
                                                        FROM (
                                                            SELECT
                                                                uca.id,
                                                                uca.content_id,
                                                                uca.watch_count,
                                                                ROW_NUMBER() OVER (
                                                                    PARTITION BY uca.content_id
                                                                    ORDER BY uca.watched_at DESC, uca.id DESC
                                                                ) AS row_num
                                                            FROM user_content_activities uca
                                                            WHERE
                                                                uca.user_id = {userId}
                                                                AND uca.content_type = 1
                                                                AND uca.activity_type = 1
                                                        ) ranked
                                                        WHERE ranked.row_num = 1 AND ranked.watch_count > 0
                                                    ), active_episodes AS (
                                                        SELECT
                                                            le.content_id,
                                                            le.watch_count,
                                                            e.tv_show_id,
                                                            e.runtime
                                                        FROM latest_episodes le
                                                        JOIN episodes e ON e.id = le.content_id
                                                        JOIN titles t ON t.id = e.tv_show_id
                                                        WHERE e.is_deleted = FALSE AND t.is_deleted = FALSE
                                                    )
                                                    SELECT
                                                        COUNT(DISTINCT ae.tv_show_id)::int AS tv_series_count,
                                                        COUNT(*)::int AS episodes_count,
                                                        COALESCE(SUM(ae.watch_count * ae.runtime), 0)::int AS minutes_watched
                                                    FROM active_episodes ae
                                                    """;
            var aggregateRow = await _dbContext.SeriesStatsAggregateRows
                .FromSql(seriesAggregateSql)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct) ?? new SeriesStatsAggregateRow();

            FormattableString seriesTopGenresSql = $"""
                                                    WITH latest_episodes AS (
                                                        SELECT
                                                            ranked.content_id,
                                                            ranked.watch_count
                                                        FROM (
                                                            SELECT
                                                                uca.id,
                                                                uca.content_id,
                                                                uca.watch_count,
                                                                ROW_NUMBER() OVER (
                                                                    PARTITION BY uca.content_id
                                                                    ORDER BY uca.watched_at DESC, uca.id DESC
                                                                ) AS row_num
                                                            FROM user_content_activities uca
                                                            WHERE
                                                                uca.user_id = {userId}
                                                                AND uca.content_type = 1
                                                                AND uca.activity_type = 1
                                                        ) ranked
                                                        WHERE ranked.row_num = 1 AND ranked.watch_count > 0
                                                    ), active_episodes AS (
                                                        SELECT
                                                            le.content_id,
                                                            le.watch_count,
                                                            e.tv_show_id
                                                        FROM latest_episodes le
                                                        JOIN episodes e ON e.id = le.content_id
                                                        JOIN titles t ON t.id = e.tv_show_id
                                                        WHERE e.is_deleted = FALSE AND t.is_deleted = FALSE
                                                    )
                                                    SELECT
                                                        g.name
                                                    FROM active_episodes ae
                                                    JOIN title_genres tg ON tg.title_id = ae.tv_show_id
                                                    JOIN genres g ON g.id = tg.genre_id
                                                    WHERE tg.is_tv_show = TRUE
                                                    GROUP BY g.name
                                                    ORDER BY SUM(ae.watch_count) DESC, COUNT(*) DESC, g.name
                                                    LIMIT 5
                                                    """;
            var topGenres = await _dbContext.GenreNameRows
                .FromSql(seriesTopGenresSql)
                .AsNoTracking()
                .Select(x => x.Name)
                .ToListAsync(ct);
            var seriesStats = new SeriesStatsResponse
            {
                TvSeriesCount = aggregateRow.TvSeriesCount,
                EpisodesCount = aggregateRow.EpisodesCount,
                TopGenres = topGenres
            };
            seriesStats.FillDurationStats(aggregateRow.MinutesWatched);

            return Result<SeriesStatsResponse>.Success(seriesStats);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<SeriesStatsResponse>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<SeriesStatsResponse>.Fail($"Error: {e.Message}");
        }
    }
}
