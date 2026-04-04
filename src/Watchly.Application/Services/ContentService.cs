using Microsoft.EntityFrameworkCore;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Content;
using Watchly.Domain.Entities;
using Watchly.Domain.Enums;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public class ContentService : IContentService
{
    private readonly WatchlyDbContext _dbContext;

    public ContentService(WatchlyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Result<Title>> GetTitleByIdAsync(int titleId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<Result<IEnumerable<TitleShortInfo>>> SearchTitlesAsync(string searchTerm, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<IEnumerable<TitleShortInfo>>> FilterTitlesAsync(
        FilterRequest filterOptions, CancellationToken ct)
    {
        IQueryable<Title> query = _dbContext.Titles;
        query = ApplyFilters(filterOptions, query);
        var results = query.Select(t => new TitleShortInfo(t.Id, t.Name, t.PosterUrl, t.AvgTmdbRating));

        return Result<IEnumerable<TitleShortInfo>>.Success(await results.ToListAsync(ct));
    }

    private static IQueryable<Title> ApplyFilters(FilterRequest filterOptions, IQueryable<Title> query)
    {
        if (filterOptions.Genres?.Any() == true)
        {
            query = query.Where(t => t.TitleGenres.Any(tg => filterOptions.Genres.Contains(tg.GenreId)));
        }

        if (filterOptions.Keywords?.Any() == true)
        {
            query = query.Where(t => t.KeywordTitles.Any(kt => filterOptions.Keywords.Contains(kt.KeywordId)));
        }

        if (filterOptions.SpokenLanguages?.Any() == true)
        {
            query = query.Where(t => t.TitleSpokenLanguages.Any(
                tsl => filterOptions.SpokenLanguages.Contains(tsl.SpokenLanguageId)));
        }

        if (filterOptions.TitleTypes != null)
        {
            var casted = filterOptions.TitleTypes.Select(tt => (TitleType)tt);
            query = query.Where(t => casted.Contains(t.ContentType));
        }

        var rRange = filterOptions.RatingRange;
        if (rRange.End != 0)
        {
            query = query.Where(t => t.AvgTmdbRating >= rRange.Start && t.AvgTmdbRating <= rRange.End);
        }
        else
        {
            query = query.Where(t => t.AvgTmdbRating >= rRange.Start);
        }

        var yRange = filterOptions.YearsRange;
        if (yRange.End != 0)
        {
            query = query.Where(
                t => t.ReleaseDate.Value.Year >= yRange.Start && t.ReleaseDate.Value.Year <= yRange.End);
        }
        else
        {
            query = query.Where(
                t => t.ReleaseDate.Value.Year >= yRange.Start);
        }

        return query;
    }
}
