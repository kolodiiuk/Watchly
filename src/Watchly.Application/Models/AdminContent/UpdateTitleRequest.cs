using Watchly.Domain.Enums;

namespace Watchly.Application.Models.AdminContent;

public sealed record UpdateTitleRequest(
    string Name,
    string Overview,
    TitleType ContentType,
    int Runtime,
    bool IsAdult,
    DateTime? ReleaseDate,
    string? Tagline,
    string? Director,
    string? Actors,
    string? LocalizationLanguages,
    string? HomePage,
    float? AvgTmdbRating
);
