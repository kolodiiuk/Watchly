namespace Watchly.Domain.Entities;

public sealed class TitleGenre
{
    public int Id { get; set; }

    public int TitleId { get; set; }

    public int GenreId { get; set; }

    public Title Title { get; set; }

    public Genre Genre { get; set; }
}
