namespace Watchly.Domain.Entities;

public sealed class Comment
{
    public int Id { get; set; }

    public int TitleId { get; set; }

    public int EpisodeId { get; set; }

    public Guid UserId { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Text { get; set; }

    public User User { get; set; }

    public Title Title { get; set; }

    public Episode Episode { get; set; }
}
