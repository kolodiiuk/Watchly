using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.AdminContent;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public sealed class AdminContentService : IAdminContentService
{
    private readonly WatchlyDbContext _dbContext;

    private readonly Cloudinary _cloudinary;

    public AdminContentService(WatchlyDbContext dbContext, Cloudinary cloudinary)
    {
        _dbContext = dbContext;
        _cloudinary = cloudinary;
    }

    public async Task<Result<int>> AddTitleAsync(CreateTitleRequest request, CancellationToken ct = default)
    {
        try
        {
            var title = new Title
            {
                Name = request.Name,
                Overview = request.Overview,
                ContentType = request.ContentType,
                Runtime = request.Runtime,
                IsAdult = request.IsAdult,
                ReleaseDate = request.ReleaseDate,
                PosterUrl = request.PosterUrl ?? string.Empty,
                Tagline = request.Tagline ?? string.Empty,
                Director = request.Director ?? string.Empty,
                Actors = request.Actors ?? string.Empty,
                LocalizationLanguages = request.LocalizationLanguages ?? string.Empty,
                HomePage = request.HomePage ?? string.Empty,
                AvgTmdbRating = request.AvgTmdbRating,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _dbContext.Titles.AddAsync(title, ct);
            await _dbContext.SaveChangesAsync(ct);

            return Result<int>.Success(title.Id);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<int>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<int>.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> UpdateTitleAsync(int titleId, UpdateTitleRequest request, CancellationToken ct = default)
    {
        try
        {
            var title = await _dbContext.Titles.FirstOrDefaultAsync(t => t.Id == titleId, ct);
            if (title is null)
            {
                return Result.Fail("Title is not found");
            }

            if (title.IsDeleted)
            {
                return Result.Fail("Title is deleted");
            }

            title.Name = request.Name;
            title.Overview = request.Overview;
            title.ContentType = request.ContentType;
            title.Runtime = request.Runtime;
            title.IsAdult = request.IsAdult;
            title.ReleaseDate = request.ReleaseDate;
            title.Tagline = request.Tagline ?? string.Empty;
            title.Director = request.Director ?? string.Empty;
            title.Actors = request.Actors ?? string.Empty;
            title.LocalizationLanguages = request.LocalizationLanguages ?? string.Empty;
            title.HomePage = request.HomePage ?? string.Empty;
            title.AvgTmdbRating = request.AvgTmdbRating;
            title.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> UploadPosterAsync(int titleId, Stream posterStream, CancellationToken ct = default)
    {
        try
        {
            var title = await _dbContext.Titles.FirstOrDefaultAsync(t => t.Id == titleId, ct);
            if (title is null)
            {
                return Result.Fail("title is not found");
            }

            if (title.IsDeleted)
            {
                return Result.Fail("Title is deleted");
            }

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(Guid.NewGuid().ToString(), posterStream),
                PublicId = Guid.NewGuid().ToString(),
                Overwrite = true,
                Folder = "uploads/"
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);
            if (uploadResult.StatusCode != HttpStatusCode.OK)
            {
                return Result.Fail("Upload failed.");
            }

            title.PosterUrl = uploadResult.Url.AbsoluteUri;
            title.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> SoftDeleteTitleAsync(int titleId, CancellationToken ct = default)
    {
        try
        {
            var title = await _dbContext.Titles.FirstOrDefaultAsync(t => t.Id == titleId && !t.IsDeleted, ct);
            if (title is null)
            {
                return Result.Fail("title is not found");
            }

            title.IsDeleted = true;
            title.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result<int>> AddSeasonAsync(int titleId, CreateSeasonRequest request, CancellationToken ct = default)
    {
        try
        {
            var titleExists = await _dbContext.Titles.AnyAsync(t => t.Id == titleId && !t.IsDeleted, ct);
            if (!titleExists)
            {
                return Result<int>.Fail("title is not found");
            }

            var season = new Season
            {
                TitleId = titleId,
                OrdinalNumber = request.OrdinalNumber,
                Name = request.Name
            };

            await _dbContext.Seasons.AddAsync(season, ct);
            await _dbContext.SaveChangesAsync(ct);

            return Result<int>.Success(season.Id);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<int>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<int>.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> UpdateSeasonAsync(int seasonId, UpdateSeasonRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var season = await _dbContext.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId, ct);
            if (season is null)
            {
                return Result.Fail("season is not found for title");
            }

            season.OrdinalNumber = request.OrdinalNumber;
            season.Name = request.Name;

            await _dbContext.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> RemoveSeasonAsync(int seasonId, CancellationToken ct = default)
    {
        try
        {
            var season = await _dbContext.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId, ct);
            if (season is null)
            {
                return Result.Fail("season is not found for title");
            }

            _dbContext.Seasons.Remove(season);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result<int>> AddEpisodeAsync(int seasonId, CreateEpisodeRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var season = await _dbContext.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId, ct);
            if (season is null)
            {
                return Result<int>.Fail("season is not found for title");
            }

            var episode = new Episode
            {
                SeasonId = seasonId,
                OrdinalNumber = request.OrdinalNumber,
                Runtime = request.Runtime,
                Name = request.Name,
                PosterUrl = request.PosterUrl ?? string.Empty,
                ReleaseDate = request.ReleaseDate,
                UpdatedAt = DateTime.UtcNow,
                TvShowId = request.TvShowId
            };

            await _dbContext.Episodes.AddAsync(episode, ct);
            await _dbContext.SaveChangesAsync(ct);

            return Result<int>.Success(episode.Id);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<int>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<int>.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> UpdateEpisodeAsync(int episodeId, UpdateEpisodeRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var episode = await _dbContext.Episodes
                .Where(e => e.Id == episodeId)
                .FirstOrDefaultAsync(ct);
            if (episode is null)
            {
                return Result.Fail("episode is not found for season and title");
            }

            episode.OrdinalNumber = request.OrdinalNumber;
            episode.Runtime = request.Runtime;
            episode.Name = request.Name;
            episode.PosterUrl = request.PosterUrl ?? string.Empty;
            episode.ReleaseDate = request.ReleaseDate;
            episode.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> RemoveEpisodeAsync(int episodeId, CancellationToken ct = default)
    {
        try
        {
            var episode = await _dbContext.Episodes
                .Where(e => e.Id == episodeId)
                .FirstOrDefaultAsync(ct);
            if (episode is null)
            {
                return Result.Fail("episode is not found for season and title");
            }

            _dbContext.Episodes.Remove(episode);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }
}
