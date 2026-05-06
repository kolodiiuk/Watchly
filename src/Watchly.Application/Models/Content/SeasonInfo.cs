namespace Watchly.Application.Models.Content
{
    public record SeasonInfo(
        int SeasonId,
        int OrdinalNumber,
        string Name,
        int TitleId,
        string TitleName,
        ICollection<EpisodeShortInfo> Episodes
    );
}