namespace Watchly.Application.Models.Content;

using Watchly.Domain.Enums;

public record TitleShortInfo(
    int Id,
    string Name,
    string PosterUrl,
    float? AvgTmdbRating,
    DateTime? ReleaseDate,
    TitleType TitleType
);
