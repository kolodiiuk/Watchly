namespace Watchly.Domain.Entities;

public sealed class Episode
{
    public int Id { get; set; }

    public int SeasonId { get; set; }

    public int OrdinalNumber { get; set; }

    /// <summary>
    /// In minutes
    /// </summary>
    public int Runtime { get; set; }

    public string Name { get; set; }

    public string PosterUrl { get; set; }

    public DateTime? ReleaseDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Season Season { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
