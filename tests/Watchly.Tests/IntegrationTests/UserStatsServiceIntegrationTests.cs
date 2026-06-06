using Microsoft.EntityFrameworkCore;
using Watchly.Application.Services;
using Watchly.Domain.Entities;
using Watchly.Domain.Enums;
using Watchly.Infrastructure.DbContexts;
using Watchly.Tests.Fixtures;

namespace Watchly.Tests.IntegrationTests;

public class UserStatsServiceIntegrationTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private const int Movie1Id = 5100;
    private const int Movie2Id = 5200;
    private const int DeletedMovieId = 5300;

    private const int TvShow1Id = 6100;
    private const int DeletedTvShowId = 6200;

    private const int Season1Id = 7100;
    private const int DeletedShowSeasonId = 7200;
    private const int Episode1Id = 8101;
    private const int Episode2Id = 8102;
    private const int DeletedEpisodeId = 8103;

    private readonly DatabaseFixture _fixture;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    private WatchlyDbContext _dbContext = null!;
    private UserStatsService _sut = null!;

    public UserStatsServiceIntegrationTests(DatabaseFixture fixture)
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

        _sut = new UserStatsService(_dbContext);
        await SeedBaselineAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task GetUserMovieStatsAsync_WhenNoActivityExists_ReturnsZeroes()
    {
        var result = await _sut.GetUserMovieStatsAsync(_userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(0, result.Value.MovieCount);
        Assert.Equal(0, result.Value.HoursWatched);
        Assert.Empty(result.Value.TopGenres);
    }

    [Fact]
    public async Task GetUserMovieStatsAsync_UsesLatestActivityPerMovieAndFiltersZeroLatest()
    {
        await AddActivityAsync(Movie1Id, _userId, ContentType.Movie, 1, DateTime.UtcNow.AddMinutes(-40));
        await AddActivityAsync(Movie1Id, _userId, ContentType.Movie, 3, DateTime.UtcNow.AddMinutes(-20));

        await AddActivityAsync(Movie2Id, _userId, ContentType.Movie, 2, DateTime.UtcNow.AddMinutes(-30));
        await AddActivityAsync(Movie2Id, _userId, ContentType.Movie, 0, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.GetUserMovieStatsAsync(_userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(1, result.Value.MovieCount);
        Assert.Equal(6, result.Value.HoursWatched); // 3 * 120 minutes
        Assert.Equal(["Action", "Drama"], result.Value.TopGenres);
    }

    [Fact]
    public async Task GetUserMovieStatsAsync_IgnoresDeletedTitlesAndOtherUsers()
    {
        await AddActivityAsync(DeletedMovieId, _userId, ContentType.Movie, 5, DateTime.UtcNow.AddMinutes(-5));
        await AddActivityAsync(Movie1Id, _otherUserId, ContentType.Movie, 7, DateTime.UtcNow.AddMinutes(-2));

        var result = await _sut.GetUserMovieStatsAsync(_userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(0, result.Value.MovieCount);
        Assert.Empty(result.Value.TopGenres);
    }

    [Fact]
    public async Task GetUserTvSeriesStatsAsync_UsesDeterministicLatestRowWhenTimestampsTie()
    {
        var tieTime = DateTime.UtcNow.AddMinutes(-15);
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 1, tieTime);
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 2, tieTime);

        var result = await _sut.GetUserTvSeriesStatsAsync(_userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(1, result.Value.TvSeriesCount);
        Assert.Equal(1, result.Value.EpisodesCount);
        Assert.Equal(1, result.Value.HoursWatched); // 2 * 45 minutes
    }

    [Fact]
    public async Task GetUserTvSeriesStatsAsync_FiltersDeletedEpisodesAndDeletedShows()
    {
        await AddActivityAsync(DeletedEpisodeId, _userId, ContentType.Episode, 4, DateTime.UtcNow.AddMinutes(-10));

        var deletedShowEpisodeId = await _dbContext.Episodes
            .Where(e => e.TvShowId == DeletedTvShowId)
            .Select(e => e.Id)
            .SingleAsync();
        await AddActivityAsync(deletedShowEpisodeId, _userId, ContentType.Episode, 3, DateTime.UtcNow.AddMinutes(-8));

        var result = await _sut.GetUserTvSeriesStatsAsync(_userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(0, result.Value.TvSeriesCount);
        Assert.Equal(0, result.Value.EpisodesCount);
        Assert.Empty(result.Value.TopGenres);
    }

    [Fact]
    public async Task GetUserTvSeriesStatsAsync_CountsDistinctSeriesAndExcludesOtherUsers()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 2, DateTime.UtcNow.AddMinutes(-30));
        await AddActivityAsync(Episode2Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-25));

        await AddActivityAsync(Episode1Id, _otherUserId, ContentType.Episode, 9, DateTime.UtcNow.AddMinutes(-1));

        var result = await _sut.GetUserTvSeriesStatsAsync(_userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(1, result.Value.TvSeriesCount);
        Assert.Equal(2, result.Value.EpisodesCount);
        Assert.Equal(["Action", "Drama"], result.Value.TopGenres);
    }

    private async Task SeedBaselineAsync()
    {
        _dbContext.Users.AddRange(
            new User { Id = _userId, UserName = "stats-user", Email = "stats-user@example.com" },
            new User { Id = _otherUserId, UserName = "stats-other", Email = "stats-other@example.com" });

        _dbContext.Titles.AddRange(
            new Title
            {
                Id = Movie1Id,
                Name = "Movie One",
                Overview = "Movie One",
                UpdatedAt = DateTime.UtcNow,
                Runtime = 120,
                ContentType = TitleType.Movie,
                IsDeleted = false,
                IsAdult = false
            },
            new Title
            {
                Id = Movie2Id,
                Name = "Movie Two",
                Overview = "Movie Two",
                UpdatedAt = DateTime.UtcNow,
                Runtime = 100,
                ContentType = TitleType.Movie,
                IsDeleted = false,
                IsAdult = false
            },
            new Title
            {
                Id = DeletedMovieId,
                Name = "Deleted Movie",
                Overview = "Deleted Movie",
                UpdatedAt = DateTime.UtcNow,
                Runtime = 130,
                ContentType = TitleType.Movie,
                IsDeleted = true,
                IsAdult = false
            },
            new TvShow
            {
                Id = TvShow1Id,
                Name = "Series One",
                Overview = "Series One",
                UpdatedAt = DateTime.UtcNow,
                Runtime = 45,
                ContentType = TitleType.Series,
                IsDeleted = false,
                IsAdult = false,
                NumberOfSeasons = 1,
                NumberOfEpisodes = 3,
                InProduction = false,
                OriginalName = "Series One"
            },
            new TvShow
            {
                Id = DeletedTvShowId,
                Name = "Deleted Series",
                Overview = "Deleted Series",
                UpdatedAt = DateTime.UtcNow,
                Runtime = 50,
                ContentType = TitleType.Series,
                IsDeleted = true,
                IsAdult = false,
                NumberOfSeasons = 1,
                NumberOfEpisodes = 1,
                InProduction = false,
                OriginalName = "Deleted Series"
            });

        _dbContext.Seasons.AddRange(
            new Season { Id = Season1Id, TitleId = TvShow1Id, Name = "Season 1", OrdinalNumber = 1 },
            new Season { Id = DeletedShowSeasonId, TitleId = DeletedTvShowId, Name = "Season 1", OrdinalNumber = 1 });

        _dbContext.Episodes.AddRange(
            new Episode
            {
                Id = Episode1Id,
                Name = "Episode 1",
                OrdinalNumber = 1,
                Runtime = 45,
                SeasonId = Season1Id,
                TvShowId = TvShow1Id,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new Episode
            {
                Id = Episode2Id,
                Name = "Episode 2",
                OrdinalNumber = 2,
                Runtime = 40,
                SeasonId = Season1Id,
                TvShowId = TvShow1Id,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            },
            new Episode
            {
                Id = DeletedEpisodeId,
                Name = "Deleted Episode",
                OrdinalNumber = 3,
                Runtime = 35,
                SeasonId = Season1Id,
                TvShowId = TvShow1Id,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = true
            },
            new Episode
            {
                Id = 8201,
                Name = "Deleted Series Episode",
                OrdinalNumber = 1,
                Runtime = 30,
                SeasonId = DeletedShowSeasonId,
                TvShowId = DeletedTvShowId,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            });

        var dramaGenre = new Genre { Name = "Drama" };
        var actionGenre = new Genre { Name = "Action" };
        _dbContext.Genres.AddRange(dramaGenre, actionGenre);
        await _dbContext.SaveChangesAsync();

        _dbContext.TitleGenres.AddRange(
            new TitleGenre { TitleId = Movie1Id, GenreId = dramaGenre.Id, IsTvShow = false },
            new TitleGenre { TitleId = Movie1Id, GenreId = actionGenre.Id, IsTvShow = false },
            new TitleGenre { TitleId = TvShow1Id, GenreId = dramaGenre.Id, IsTvShow = true },
            new TitleGenre { TitleId = TvShow1Id, GenreId = actionGenre.Id, IsTvShow = true });

        await _dbContext.SaveChangesAsync();
    }

    private async Task AddActivityAsync(int contentId, Guid userId, ContentType contentType, int watchCount, DateTime watchedAt)
    {
        _dbContext.UserContentActivities.Add(new UserContentActivity
        {
            ContentId = contentId,
            UserId = userId,
            ContentType = contentType,
            ActivityType = ActivityType.Watched,
            WatchCount = watchCount,
            WatchedAt = watchedAt
        });

        await _dbContext.SaveChangesAsync();
    }
}
