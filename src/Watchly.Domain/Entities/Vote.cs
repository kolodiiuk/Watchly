namespace Watchly.Domain.Entities;

public sealed class Vote
{
    public int Id { get; set; }

    public int TitleId { get; set; }

    public short Value { get; set; }

    public DateTime UpdatedAt { get; set; }

    // public Episode Episode { get; set; }

    public Title Title { get; set; }
}
