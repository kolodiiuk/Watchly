namespace Watchly.Domain.Entities;

public sealed class WatchList
{
    public int Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; }
}
