namespace Watchly.Application.Models.Content
{
    public record SeasonShortInfo(
        int SeasonId,
        int OrdinalNumber,
        string Name,
        int TitleId,
        string TitleName
    );
}