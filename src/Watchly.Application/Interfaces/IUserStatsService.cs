using Watchly.Application.Models.Stats;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IUserStatsService
{
    Task<Result<MovieStatsResponse>> GetUserMovieStatsAsync(Guid userId, CancellationToken ct);

    Task<Result<SeriesStatsResponse>> GetUserTvSeriesStatsAsync(Guid userId, CancellationToken ct);
}