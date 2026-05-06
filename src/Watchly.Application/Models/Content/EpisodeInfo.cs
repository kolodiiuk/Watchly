namespace Watchly.Application.Models.Content;

public record EpisodeInfo(
    int EpisodeId,
    int SeasonId,
    int OrdinalNumber,
    int Runtime,
    string Name,
    string PosterUrl,
    SeasonShortInfo Season,
    float AvgVote,
    int VoteCount
    );  
