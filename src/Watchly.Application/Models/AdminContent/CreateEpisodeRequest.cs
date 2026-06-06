namespace Watchly.Application.Models.AdminContent;

public sealed record CreateEpisodeRequest(
    int OrdinalNumber,
    int Runtime,
    int TvShowId,
    string Name,
    string PosterUrl,
    DateTime? ReleaseDate
);
