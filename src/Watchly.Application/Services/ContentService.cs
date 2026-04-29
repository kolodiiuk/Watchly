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

    public async Task<Result<Title>> GetTitleByIdAsync(int titleId, CancellationToken ct)
    {
        try
        {
            var title = await _dbContext.Titles
                .Where(t => t.Id == titleId)
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            return title is null
                ? Result<Title>.Fail($"Title with id {titleId} was not found.")
                : Result<Title>.Success(title);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            return Result<Title>.Fail(e.Message);
        }
    }

    public async Task<Result<IEnumerable<TitleShortInfo>>> SearchTitlesAsync(
        string searchTerm, 
        int pageSize, 
        int page, 
        CancellationToken ct = default)
    {
        var pattern = $"%{searchTerm}%";
        try
        {
            var query = _dbContext.Titles
                .Where(t => EF.Functions.ILike(t.Name, pattern))
                .OrderBy(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TitleShortInfo(t.Id, t.Name, t.PosterUrl, t.AvgTmdbRating))
                .AsNoTracking();

            return Result<IEnumerable<TitleShortInfo>>.Success(await query.ToListAsync(ct));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            return Result<IEnumerable<TitleShortInfo>>.Fail(e.Message);
        }
    }

    public async Task<Result<IEnumerable<TitleShortInfo>>> FilterTitlesAsync(
        FilterRequest filterOptions, CancellationToken ct = default)
    {
        try
        {
            IQueryable<Title> query = _dbContext.Titles;
            query = ApplyFilters(query, filterOptions);
            var tsi = query
                .OrderBy(t => t.Id)
                .Skip((filterOptions.Page - 1) * filterOptions.Size)
                .Take(filterOptions.Size)
                .Select(t => new TitleShortInfo(t.Id, t.Name, t.PosterUrl, t.AvgTmdbRating))
                .AsNoTracking();
            var results = await tsi.ToListAsync(ct);

            return Result<IEnumerable<TitleShortInfo>>.Success(results);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            return Result<IEnumerable<TitleShortInfo>>.Fail(e.Message);
        }
    }

    private static IQueryable<Title> ApplyFilters(IQueryable<Title> query, FilterRequest filterOptions)
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
