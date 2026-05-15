namespace Watchly.Domain.Entities;

public sealed class TvShow : Title
{
    public int NumberOfSeasons { get; set; }

    public int NumberOfEpisodes { get; set; }

    public string OriginalLanguage { get; set; }

    public float? VoteAverage { get; set; }

    public DateTime? FirstAirDate { get; set; }

    public DateTime? LastAirDate { get; set; }

    public bool InProduction { get; set; }

    public string OriginalName { get; set; }

    public string Type { get; set; }

    public string Status { get; set; }

    public string CreatedBy { get; set; }

    public int? EpisodeRunTime { get; set; }

    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
