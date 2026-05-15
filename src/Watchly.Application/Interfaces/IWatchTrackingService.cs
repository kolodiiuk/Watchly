using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IWatchTrackingService
{
    Task<Result> IncrWatchingCountMovieAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result> IncrWatchingCountSeasonAsync(int seasonId, Guid userId, CancellationToken ct);
    Task<Result> IncrWatchingCountEpisodeAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result> DecrWatchingCountMovieAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result> DecrWatchingCountSeasonAsync(int seasonId, Guid userId, CancellationToken ct);
    Task<Result> DecrWatchingCountEpisodeAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result<TvShowWatchInfo>> GetWatchCountInfoTvShowAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result<int>> GetWatchCountInfoMovieAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result<int>> GetWatchCountInfoEpisodeAsync(int episodeId, Guid userId, CancellationToken ct);
}
