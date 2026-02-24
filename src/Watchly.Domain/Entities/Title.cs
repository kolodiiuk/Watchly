namespace Watchly.Domain.Entities;

public sealed class Title
{
    public int Id { get; set; }

    public DateTime ReleaseDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// In minutes
    /// </summary>
    public int Runtime { get; set; }

    public string Name { get; set; }

    public string AvgTmdbRating { get; set; }

    public string HomePage { get; set; }

    public string Overview { get; set; }

    public string PosterUrl { get; set; }

    public string Tagline { get; set; }

    public ICollection<Genre> Genres { get; set; } = new List<Genre>();

    public ICollection<ProductionCompany> ProductionCompanies { get; set; } = new List<ProductionCompany>();

    public ICollection<SpokenLanguage> SpokenLanguages { get; set; } = new List<SpokenLanguage>();

    public ICollection<Keyword> Keywords { get; set; } = new List<Keyword>();

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
