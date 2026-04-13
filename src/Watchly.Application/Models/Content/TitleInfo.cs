namespace Watchly.Application.Models.Content;

public record TitleInfo(
    int Id,
    string Name,
    string Overview,
    string PosterUrl,
    DateTime? ReleaseDate,
    int Runtime,
    float? AvgTmdbRating);
