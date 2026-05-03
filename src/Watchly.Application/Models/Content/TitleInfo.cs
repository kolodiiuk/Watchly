using Watchly.Domain.Entities;
using Watchly.Domain.Enums;

namespace Watchly.Application.Models.Content;

public record TitleInfo(
    int Id,
    DateTime? ReleaseDate,
    int Runtime,
    TitleType TitleType,
    float? AvgTmdbRating,
    bool IsAdult,
    string Name,
    string Overview,
    string PosterUrl,
    string Tagline,
    string Director,
    string Actors,
    string LocalizationLanguages,
    float AvgVote,
    int VoteCount,
    IEnumerable<string> ProductionCompanies,
    IEnumerable<string> Genres,
    IEnumerable<SeasonInfo> Seasons,
    IEnumerable<string> SpokenLanguages
    );
