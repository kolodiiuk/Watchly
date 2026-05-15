using CloudinaryDotNet;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using Watchly.Application.Models.AdminContent;
using Watchly.Application.Services;
using Watchly.Domain.Entities;
using Watchly.Domain.Enums;
using Watchly.Infrastructure.DbContexts;
using Watchly.Tests.Fixtures;

namespace Watchly.Tests.IntegrationTests;

public class AdminContentServiceIntegrationTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private const int MovieTitleId = 1000;
    private const int DeletedTitleId = 1001;
    private const int TvShowTitleId = 1100;
    private const int ExistingSeasonId = 2100;
    private const int ExistingEpisodeId = 3100;

    private readonly DatabaseFixture _fixture;
    private WatchlyDbContext _dbContext = null!;
    private AdminContentService _sut = null!;

    public AdminContentServiceIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<WatchlyDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new WatchlyDbContext(options);
        await _dbContext.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        await _dbContext.Database.MigrateAsync();
        await DatabaseFixture.ResetDatabaseAsync(_dbContext);

        var cloudinary = new Cloudinary(new Account("test", "test", "test"));
        _sut = new AdminContentService(_dbContext, cloudinary);

        await SeedBaselineAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task AddTitleAsync_WhenValidRequest_CreatesTitle()
    {
        var request = new CreateTitleRequest(
            Name: "New Title",
            Overview: "Overview",
            ContentType: TitleType.Movie,
            Runtime: 123,
            IsAdult: false,
            ReleaseDate: new DateTime(2024, 1, 10).AsUtc(),
            PosterUrl: null,
            Tagline: null,
            Director: null,
            Actors: null,
            LocalizationLanguages: null,
            HomePage: null,
            AvgTmdbRating: 8.7f);

        var result = await _sut.AddTitleAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var saved = await _dbContext.Titles.SingleAsync(t => t.Id == result.Value);
        Assert.Equal("New Title", saved.Name);
        Assert.Equal(string.Empty, saved.PosterUrl);
        Assert.Equal(string.Empty, saved.Tagline);
        Assert.Equal(string.Empty, saved.Director);
        Assert.Equal(string.Empty, saved.Actors);
        Assert.Equal(string.Empty, saved.LocalizationLanguages);
        Assert.Equal(string.Empty, saved.HomePage);
        Assert.False(saved.IsDeleted);
    }

    [Fact]
    public async Task UpdateTitleAsync_WhenTitleExists_UpdatesFields()
    {
        var request = new UpdateTitleRequest(
            Name: "Updated Movie",
            Overview: "Updated overview",
            ContentType: TitleType.Series,
            Runtime: 95,
            IsAdult: true,
            ReleaseDate: new DateTime(2023, 5, 5).AsUtc(),
            Tagline: null,
            Director: null,
            Actors: null,
            LocalizationLanguages: null,
            HomePage: null,
            AvgTmdbRating: 7.1f);

        var result = await _sut.UpdateTitleAsync(MovieTitleId, request, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var updated = await _dbContext.Titles.SingleAsync(t => t.Id == MovieTitleId);
        Assert.Equal("Updated Movie", updated.Name);
        Assert.Equal("Updated overview", updated.Overview);
        Assert.Equal(TitleType.Series, updated.ContentType);
        Assert.Equal(95, updated.Runtime);
        Assert.True(updated.IsAdult);
        Assert.Equal(string.Empty, updated.Tagline);
        Assert.Equal(string.Empty, updated.Director);
    }

    [Fact]
    public async Task UpdateTitleAsync_WhenTitleNotFound_ReturnsFailure()
    {
        var request = new UpdateTitleRequest("x", "y", TitleType.Movie, 10, false, null, null, null, null, null, null, null);

        var result = await _sut.UpdateTitleAsync(999999, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Title is not found", result.Error);
    }

    [Fact]
    public async Task UpdateTitleAsync_WhenTitleDeleted_ReturnsFailure()
    {
        var request = new UpdateTitleRequest("x", "y", TitleType.Movie, 10, false, null, null, null, null, null, null, null);

        var result = await _sut.UpdateTitleAsync(DeletedTitleId, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Title is deleted", result.Error);
    }

    [Fact]
    public async Task SoftDeleteTitleAsync_WhenTitleExists_MarksAsDeleted()
    {
        var result = await _sut.SoftDeleteTitleAsync(MovieTitleId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var title = await _dbContext.Titles.SingleAsync(t => t.Id == MovieTitleId);
        Assert.True(title.IsDeleted);
    }

    [Fact]
    public async Task SoftDeleteTitleAsync_WhenAlreadyDeletedOrMissing_ReturnsFailure()
    {
        var deletedResult = await _sut.SoftDeleteTitleAsync(DeletedTitleId, CancellationToken.None);
        Assert.False(deletedResult.IsSuccess);
        Assert.Contains("title is not found", deletedResult.Error);

        var missingResult = await _sut.SoftDeleteTitleAsync(999999, CancellationToken.None);
        Assert.False(missingResult.IsSuccess);
        Assert.Contains("title is not found", missingResult.Error);
    }

    [Fact]
    public async Task AddSeasonAsync_WhenTitleExistsAndNotDeleted_CreatesSeason()
    {
        var request = new CreateSeasonRequest(2, "Season 2");

        var result = await _sut.AddSeasonAsync(TvShowTitleId, request, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var saved = await _dbContext.Seasons.SingleAsync(s => s.Id == result.Value);
        Assert.Equal(TvShowTitleId, saved.TitleId);
        Assert.Equal(2, saved.OrdinalNumber);
        Assert.Equal("Season 2", saved.Name);
    }

    [Fact]
    public async Task AddSeasonAsync_WhenTitleMissingOrDeleted_ReturnsFailure()
    {
        var request = new CreateSeasonRequest(1, "S");

        var missingResult = await _sut.AddSeasonAsync(999999, request, CancellationToken.None);
        Assert.False(missingResult.IsSuccess);
        Assert.Contains("title is not found", missingResult.Error);

        var deletedResult = await _sut.AddSeasonAsync(DeletedTitleId, request, CancellationToken.None);
        Assert.False(deletedResult.IsSuccess);
        Assert.Contains("title is not found", deletedResult.Error);
    }

    [Fact]
    public async Task UpdateSeasonAsync_WhenSeasonExists_UpdatesFields()
    {
        var request = new UpdateSeasonRequest(9, "Renamed Season");

        var result = await _sut.UpdateSeasonAsync(ExistingSeasonId, request, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var season = await _dbContext.Seasons.SingleAsync(s => s.Id == ExistingSeasonId);
        Assert.Equal(9, season.OrdinalNumber);
        Assert.Equal("Renamed Season", season.Name);
    }

    [Fact]
    public async Task UpdateSeasonAsync_WhenSeasonNotFound_ReturnsFailure()
    {
        var request = new UpdateSeasonRequest(1, "S");

        var result = await _sut.UpdateSeasonAsync(999999, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("season is not found for title", result.Error);
    }

    [Fact]
    public async Task RemoveSeasonAsync_WhenSeasonExists_RemovesSeasonAndChildren()
    {
        var result = await _sut.RemoveSeasonAsync(ExistingSeasonId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var seasonExists = await _dbContext.Seasons.AnyAsync(s => s.Id == ExistingSeasonId);
        var episodeExists = await _dbContext.Episodes.AnyAsync(e => e.Id == ExistingEpisodeId);
        Assert.False(seasonExists);
        Assert.False(episodeExists);
    }

    [Fact]
    public async Task RemoveSeasonAsync_WhenSeasonNotFound_ReturnsFailure()
    {
        var result = await _sut.RemoveSeasonAsync(999999, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("season is not found for title", result.Error);
    }

    [Fact]
    public async Task AddEpisodeAsync_WhenSeasonExists_CreatesEpisode()
    {
        var dateTime = new DateTime(2025, 2, 2);
        var withUtc = dateTime.AsUtc();
        var request = new CreateEpisodeRequest(2, 42, TvShowTitleId, "Episode 2", null, withUtc);

        var result = await _sut.AddEpisodeAsync(ExistingSeasonId, request, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var saved = await _dbContext.Episodes.SingleAsync(e => e.Id == result.Value);
        Assert.Equal(ExistingSeasonId, saved.SeasonId);
        Assert.Equal(2, saved.OrdinalNumber);
        Assert.Equal(42, saved.Runtime);
        Assert.Equal("Episode 2", saved.Name);
        Assert.Equal(string.Empty, saved.PosterUrl);
    }

    [Fact]
    public async Task AddEpisodeAsync_WhenSeasonNotFound_ReturnsFailure()
    {
        var request = new CreateEpisodeRequest(1, 40, TvShowTitleId,"E", null, null);

        var result = await _sut.AddEpisodeAsync(999999, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("season is not found for title", result.Error);
    }

    [Fact]
    public async Task UpdateEpisodeAsync_WhenEpisodeExists_UpdatesFields()
    {
        var request = new UpdateEpisodeRequest(7, 59, "Updated Episode", null, new DateTime(2025, 5, 1).AsUtc());

        var result = await _sut.UpdateEpisodeAsync(ExistingEpisodeId, request, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var episode = await _dbContext.Episodes.SingleAsync(e => e.Id == ExistingEpisodeId);
        Assert.Equal(7, episode.OrdinalNumber);
        Assert.Equal(59, episode.Runtime);
        Assert.Equal("Updated Episode", episode.Name);
        Assert.Equal(string.Empty, episode.PosterUrl);
    }

    [Fact]
    public async Task UpdateEpisodeAsync_WhenEpisodeNotFound_ReturnsFailure()
    {
        var request = new UpdateEpisodeRequest(1, 20, "E", null, null);

        var result = await _sut.UpdateEpisodeAsync(999999, request, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("episode is not found for season and title", result.Error);
    }

    [Fact]
    public async Task RemoveEpisodeAsync_WhenEpisodeExists_RemovesEpisode()
    {
        var result = await _sut.RemoveEpisodeAsync(ExistingEpisodeId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        var exists = await _dbContext.Episodes.AnyAsync(e => e.Id == ExistingEpisodeId);
        Assert.False(exists);
    }

    [Fact]
    public async Task RemoveEpisodeAsync_WhenEpisodeNotFound_ReturnsFailure()
    {
        var result = await _sut.RemoveEpisodeAsync(999999, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("episode is not found for season and title", result.Error);
    }

    private async Task SeedBaselineAsync()
    {
        _dbContext.Titles.AddRange(
            new Title
            {
                Id = MovieTitleId,
                Name = "Movie",
                Overview = "Movie overview",
                ContentType = TitleType.Movie,
                Runtime = 120,
                IsAdult = false,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                PosterUrl = string.Empty,
                Tagline = string.Empty,
                Director = string.Empty,
                Actors = string.Empty,
                LocalizationLanguages = string.Empty,
                HomePage = string.Empty
            },
            new Title
            {
                Id = DeletedTitleId,
                Name = "Deleted",
                Overview = "Deleted overview",
                ContentType = TitleType.Movie,
                Runtime = 100,
                IsAdult = false,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = true,
                PosterUrl = string.Empty,
                Tagline = string.Empty,
                Director = string.Empty,
                Actors = string.Empty,
                LocalizationLanguages = string.Empty,
                HomePage = string.Empty
            },
            new TvShow
            {
                Id = TvShowTitleId,
                Name = "TV Show",
                Overview = "TV show overview",
                ContentType = TitleType.Series,
                Runtime = 50,
                IsAdult = false,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
                PosterUrl = string.Empty,
                Tagline = string.Empty,
                Director = string.Empty,
                Actors = string.Empty,
                LocalizationLanguages = string.Empty,
                HomePage = string.Empty,
                NumberOfSeasons = 1,
                NumberOfEpisodes = 1,
                InProduction = false,
                OriginalName = "TV Show"
            });

        _dbContext.Seasons.Add(new Season
        {
            Id = ExistingSeasonId,
            TitleId = TvShowTitleId,
            OrdinalNumber = 1,
            Name = "Season 1"
        });

        _dbContext.Episodes.Add(new Episode
        {
            Id = ExistingEpisodeId,
            SeasonId = ExistingSeasonId,
            TvShowId = TvShowTitleId,
            OrdinalNumber = 1,
            Runtime = 50,
            Name = "Episode 1",
            PosterUrl = string.Empty,
            UpdatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync();
    }
}
