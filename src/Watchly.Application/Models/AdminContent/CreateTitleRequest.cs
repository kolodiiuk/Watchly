using Watchly.Domain.Enums;

namespace Watchly.Application.Models.AdminContent;

public sealed record CreateTitleRequest(
    string Name,
    string Overview,
    TitleType ContentType,
    int Runtime,
    bool IsAdult,
    DateTime? ReleaseDate,
    string? PosterUrl,
    string? Tagline,
    string? Director,
    string? Actors,
    string? LocalizationLanguages,
    string? HomePage,
    float? AvgTmdbRating,
    IReadOnlyCollection<int>? GenreIds = null,
    IReadOnlyCollection<int>? SpokenLanguageIds = null,
    IReadOnlyCollection<int>? ProductionCompanyIds = null
);
