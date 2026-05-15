namespace Watchly.Application.Models.AdminContent;

public sealed record UpdateEpisodeRequest(
    int OrdinalNumber,
    int Runtime,
    string Name,
    string? PosterUrl,
    DateTime? ReleaseDate
);
