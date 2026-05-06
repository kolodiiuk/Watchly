using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IWatchTrackingService
{
    Task<Result> IncrWatchingCountMovieAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result> IncrWatchingCountEpisodeAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result> DecrWatchingCountMovieAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result> DecrWatchingCountEpisodeAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result<IEnumerable<TvShowWatchInfo>>> GetWatchCountInfoTvShowAsync(int id, Guid userId, CancellationToken ct = default);
    Task<Result<int>> GetWatchCountInfoMovieAsync(int id, Guid userId, CancellationToken ct = default);
}

public class TvShowWatchInfo
{

}