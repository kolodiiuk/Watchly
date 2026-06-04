using Watchly.Infrastructure.Models;

namespace Watchly.Application.Interfaces;

public record TvShowWatchInfo(int TvShowId, IEnumerable<EpisodeWatchInfo> EpisodeWatchInfos);
