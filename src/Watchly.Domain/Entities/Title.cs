using Watchly.Domain.Enums;

namespace Watchly.Domain.Entities;

public class Title
{
    public int Id { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// In minutes
    /// </summary>
    public int Runtime { get; set; }

    public TitleType ContentType { get; set; }

    public float? AvgTmdbRating { get; set; }

    public bool IsAdult { get; set; }

    public string Name { get; set; }

    public string HomePage { get; set; }

    public string Overview { get; set; }

    public string PosterUrl { get; set; }

    public string Tagline { get; set; }

    public string Director { get; set; }

    /// <summary>
    /// Separated by ", "
    /// </summary>
    public string Actors { get; set; }

    /// <summary>
    /// Separated by ", "
    /// </summary>
    public string LocalizationLanguages { get; set; }

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public ICollection<TitleProductionCompany> TitleProductionCompanies { get; set; } =
        new List<TitleProductionCompany>();

    public ICollection<TitleGenre> TitleGenres { get; set; } = new List<TitleGenre>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<KeywordTitle> KeywordTitles { get; set; } = new List<KeywordTitle>();

    public ICollection<Season> Seasons { get; set; } = new List<Season>();

    public ICollection<TitleSpokenLanguage> TitleSpokenLanguages { get; set; }
        = new List<TitleSpokenLanguage>();

    public ICollection<WatchListItem> WatchListItems { get; set; } = new List<WatchListItem>();
}
