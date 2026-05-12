using Watchly.Application.Models.Content;

namespace Watchly.Application.Models.WatchList;

public record WatchListInfo(
    int Id, 
    string Name,
    IEnumerable<TitleShortInfo> Titles
    );