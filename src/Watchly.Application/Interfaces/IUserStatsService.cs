using Watchly.Application.Models;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IUserStatsService
{
    Task<Result<MovieStatsResponse>> GetUserMovieStatsAsync(Guid userId, CancellationToken ct);

    Task<Result<SeriesStatsResponse>> GetUserTvSeriesStatsAsync(Guid userId, CancellationToken ct);
}
