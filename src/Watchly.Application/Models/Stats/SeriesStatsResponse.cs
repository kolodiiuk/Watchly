namespace Watchly.Application.Models.Stats;

public sealed class SeriesStatsResponse
{
    private const int MinutesInHour = 60;

    private const int MinutesInDay = MinutesInHour * 24;

    private const int MinutesInMonth = MinutesInDay * 30;

    /// <summary>
    /// Distinct series with at least one currently watched episode
    /// </summary>
    public int TvSeriesCount { get; set; }
    
    public int EpisodesCount { get; set; }
    
    public int DaysWatched { get; set; }
    
    public int HoursWatched { get; set; }
    
    public int MonthsWatched { get; set; }
    
    public IEnumerable<string> TopGenres { get; set; } = Array.Empty<string>();

    public void FillDurationStats(int totalMinutes)
    {
        var mins = totalMinutes;
        MonthsWatched = mins / MinutesInMonth;
        mins %= MinutesInMonth;

        DaysWatched = mins / MinutesInDay;
        mins %= MinutesInDay;

        HoursWatched = mins / MinutesInHour;
    }
}
