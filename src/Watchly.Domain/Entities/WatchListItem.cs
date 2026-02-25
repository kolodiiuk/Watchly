namespace Watchly.Domain.Entities;

public sealed class WatchListItem
{
    public int Id { get; set; }

    public int WatchListId { get; set; }

    public int TitleId { get; set; }

    public Title Title { get; set; }

    public WatchList WatchList { get; set; }
}
