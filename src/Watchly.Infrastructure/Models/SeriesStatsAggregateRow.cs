namespace Watchly.Infrastructure.Models;

public sealed class SeriesStatsAggregateRow
{
    public int TvSeriesCount { get; set; }

    public int EpisodesCount { get; set; }

    public int MinutesWatched { get; set; }
}
