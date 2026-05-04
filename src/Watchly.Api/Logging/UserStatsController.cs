namespace Watchly.Api.Logging;

internal static class UserStatsControllerEventIds
{
    internal static readonly EventId GetMovieAttempt = new(4001, nameof(GetMovieAttempt));
    internal static readonly EventId GetMovieFailed = new(4002, nameof(GetMovieFailed));

    internal static readonly EventId GetSeriesAttempt = new(4003, nameof(GetSeriesAttempt));
    internal static readonly EventId GetSeriesFailed = new(4004, nameof(GetSeriesFailed));
}
