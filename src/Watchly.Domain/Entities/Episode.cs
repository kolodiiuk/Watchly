namespace Watchly.Domain.Entities;

public sealed class Episode
{
    public int Id { get; set; }

    public int SeasonId { get; set; }

    /// <summary>
    /// In minutes
    /// </summary>
    public int Runtime { get; set; }

    public string Name { get; set; }

    public string PosterUrl { get; set; }

    public DateTime UpdatedAt { get; set; }
}