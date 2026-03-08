namespace Watchly.Application.Models;

public sealed class SeriesStatsResponse
{
    /// <summary>
    /// Added, not necessarily finished
    /// </summary>
    public int TvSeriesCount { get; set; }
    
    public int EpisodesCount { get; set; }
    
    public int DaysWatched { get; set; }
    
    public int HoursWatched { get; set; }
    
    public int MonthsWatched { get; set; }
    
    public IEnumerable<string> TopGenres { get; set; }
}
