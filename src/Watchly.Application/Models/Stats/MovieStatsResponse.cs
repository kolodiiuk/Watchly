namespace Watchly.Application.Models.Stats;

public sealed class MovieStatsResponse
{
    public int MovieCount { get; set; }

    public int DaysWatched { get; set; }

    public int HoursWatched { get; set; }

    public int MonthsWatched { get; set; }

    public IEnumerable<string> TopGenres { get; set; }
}
