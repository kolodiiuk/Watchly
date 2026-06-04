using Microsoft.EntityFrameworkCore;
using Watchly.Application.Services;
using Watchly.Domain.Entities;
using Watchly.Domain.Enums;
using Watchly.Infrastructure.DbContexts;
using Watchly.Tests.Fixtures;

namespace Watchly.Tests.IntegrationTests;

public class WatchTrackingServiceIntegrationTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private const int MovieId = 1100;
    private const int TvShowId = 2100;
    private const int Season1Id = 3100;
    private const int Season2Id = 3200;
    private const int Episode1Id = 4101;
    private const int Episode2Id = 4102;
    private const int Episode3Id = 4201;

    private readonly DatabaseFixture _fixture;
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    private WatchlyDbContext _dbContext = null!;
    private WatchTrackingService _sut = null!;

    public WatchTrackingServiceIntegrationTests(DatabaseFixture fixture)
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

        _sut = new WatchTrackingService(_dbContext);
        await SeedBaselineAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task IncrWatchingCountMovieAsync_WhenNoActivityExists_CreatesWatchRecordAndCompletedProgress()
    {
        var result = await _sut.IncrWatchingCountMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Movie && a.ContentId == MovieId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        Assert.Single(activities);
        Assert.Equal(1, activities[0].WatchCount);

        var progress = await _dbContext.UserTitleProgresses
            .SingleOrDefaultAsync(p => p.UserId == _userId && p.TitleId == MovieId);
        Assert.NotNull(progress);
        Assert.Equal(WatchStatus.Completed, progress!.Status);
    }

    [Fact]
    public async Task IncrWatchingCountMovieAsync_WhenMovieDoesNotExist_ReturnsFailure()
    {
        var result = await _sut.IncrWatchingCountMovieAsync(999_999, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("doesn't exist", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IncrWatchingCountMovieAsync_WhenActivityExists_AddsIncrementedRecord()
    {
        await AddActivityAsync(MovieId, _userId, ContentType.Movie, 2, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.IncrWatchingCountMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Movie && a.ContentId == MovieId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        Assert.Equal(2, activities.Count);
        Assert.Equal(3, activities[^1].WatchCount);
    }

    [Fact]
    public async Task SetTitleWatchStatusAsync_WhenTitleExists_UpsertsStatus()
    {
        var result = await _sut.SetTitleWatchStatusAsync(MovieId, _userId, WatchStatus.Dropped, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var progress = await _dbContext.UserTitleProgresses
            .SingleAsync(p => p.UserId == _userId && p.TitleId == MovieId);
        Assert.Equal(WatchStatus.Dropped, progress.Status);
    }

    [Fact]
    public async Task GetTitleWatchStatusAsync_WhenNoProgressExists_ReturnsPlanToWatch()
    {
        var result = await _sut.GetTitleWatchStatusAsync(MovieId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(WatchStatus.PlanToWatch, result.Value);
    }

    [Fact]
    public async Task DecrWatchingCountMovieAsync_WhenNoActivityExists_ReturnsFailureAndDoesNotWrite()
    {
        var result = await _sut.DecrWatchingCountMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Never watched", result.Error, StringComparison.OrdinalIgnoreCase);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Movie && a.ContentId == MovieId)
            .ToListAsync();
        Assert.Empty(activities);
    }

    [Fact]
    public async Task DecrWatchingCountMovieAsync_WhenOneActivityExists_SetsNotWatchedAndAddsZeroCountRecord()
    {
        await AddActivityAsync(MovieId, _userId, ContentType.Movie, 1, DateTime.UtcNow.AddMinutes(-10));
        _dbContext.UserTitleProgresses.Add(new UserTitleProgress
        {
            UserId = _userId,
            TitleId = MovieId,
            Status = WatchStatus.Completed
        });
        await _dbContext.SaveChangesAsync();

        var result = await _sut.DecrWatchingCountMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Movie && a.ContentId == MovieId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        Assert.Equal(2, activities.Count);
        Assert.Equal(0, activities[^1].WatchCount);

        var progress = await _dbContext.UserTitleProgresses
            .SingleAsync(p => p.UserId == _userId && p.TitleId == MovieId);
        Assert.Equal(WatchStatus.NotWatched, progress.Status);
    }

    [Fact]
    public async Task DecrWatchingCountMovieAsync_WhenMovieDoesNotExist_ReturnsFailure()
    {
        var result = await _sut.DecrWatchingCountMovieAsync(999_999, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("doesn't exist", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DecrWatchingCountMovieAsync_WhenCountAlreadyZero_ReturnsAlreadyUnwatched()
    {
        await AddActivityAsync(MovieId, _userId, ContentType.Movie, 0, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.DecrWatchingCountMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Already unwatched", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DecrWatchingCountMovieAsync_WhenCountGreaterThanOne_AddsDecrementedRecord()
    {
        await AddActivityAsync(MovieId, _userId, ContentType.Movie, 4, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.DecrWatchingCountMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Movie && a.ContentId == MovieId)
            .OrderBy(a => a.Id)
            .ToListAsync();

        Assert.Equal(2, activities.Count);
        Assert.Equal(3, activities[^1].WatchCount);
    }

    [Fact]
    public async Task IncrWatchingCountEpisodeAsync_WhenNoActivityExists_CreatesWatchCountOne()
    {
        var result = await _sut.IncrWatchingCountEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .ToListAsync();

        Assert.Single(activities);
        Assert.Equal(1, activities[0].WatchCount);

        var progress = await _dbContext.UserTitleProgresses
            .SingleAsync(p => p.UserId == _userId && p.TitleId == TvShowId);
        Assert.Equal(WatchStatus.Watching, progress.Status);
    }

    [Fact]
    public async Task IncrWatchingCountEpisodeAsync_WhenOnlyAnotherUserHasHistory_StartsFromOneForCurrentUser()
    {
        await AddActivityAsync(Episode1Id, _otherUserId, ContentType.Episode, 7, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.IncrWatchingCountEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var myActivities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();

        Assert.Single(myActivities);
        Assert.Equal(1, myActivities[0].WatchCount);
    }

    [Fact]
    public async Task IncrWatchingCountEpisodeAsync_WhenEpisodeDoesNotExist_ReturnsFailure()
    {
        var result = await _sut.IncrWatchingCountEpisodeAsync(999_999, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("No episode", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IncrWatchingCountEpisodeAsync_WhenCurrentUserHasHistory_AddsIncrementedRecord()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 2, DateTime.UtcNow.AddMinutes(-15));

        var result = await _sut.IncrWatchingCountEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();

        Assert.Equal(2, activities.Count);
        Assert.Equal(3, activities[^1].WatchCount);
    }

    [Fact]
    public async Task DecrWatchingCountEpisodeAsync_WhenCurrentUserHasNoHistory_ReturnsFailureEvenIfAnotherUserHasRecord()
    {
        await AddActivityAsync(Episode1Id, _otherUserId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.DecrWatchingCountEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Never watched", result.Error, StringComparison.OrdinalIgnoreCase);

        var myActivities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .ToListAsync();
        Assert.Empty(myActivities);
    }

    [Fact]
    public async Task DecrWatchingCountEpisodeAsync_WhenEpisodeDoesNotExist_ReturnsFailure()
    {
        var result = await _sut.DecrWatchingCountEpisodeAsync(999_999, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("No such episode", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DecrWatchingCountEpisodeAsync_WhenWatchCountIsOne_AddsZeroCountRecord()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.DecrWatchingCountEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();

        Assert.Equal(2, activities.Count);
        Assert.Equal(0, activities[^1].WatchCount);
    }

    [Fact]
    public async Task DecrWatchingCountEpisodeAsync_WhenWatchCountIsZero_ReturnsAlreadyUnwatched()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 0, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.DecrWatchingCountEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("Already unwatched", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DecrWatchingCountEpisodeAsync_WhenWatchCountGreaterThanOne_AddsDecrementedRecord()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 5, DateTime.UtcNow.AddMinutes(-10));

        var result = await _sut.DecrWatchingCountEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();

        Assert.Equal(2, activities.Count);
        Assert.Equal(4, activities[^1].WatchCount);
    }

    [Fact]
    public async Task IncrWatchingCountSeasonAsync_WhenNoEpisodeHistoryExists_CreatesOneRecordPerEpisodeInSeason()
    {
        var result = await _sut.IncrWatchingCountSeasonAsync(Season1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var season1Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId
                        && a.ContentType == ContentType.Episode
                        && (a.ContentId == Episode1Id || a.ContentId == Episode2Id))
            .ToListAsync();
        Assert.Equal(2, season1Activities.Count);
        Assert.All(season1Activities, a => Assert.Equal(1, a.WatchCount));

        var season2Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode3Id)
            .ToListAsync();
        Assert.Empty(season2Activities);

        var progress = await _dbContext.UserTitleProgresses
            .SingleAsync(p => p.UserId == _userId && p.TitleId == TvShowId);
        Assert.Equal(WatchStatus.Watching, progress.Status);
    }

    [Fact]
    public async Task IncrWatchingCountSeasonAsync_WhenOneRecordPerEpisodeExists_IncrementsEachEpisodeExactlyOnce()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-30));
        await AddActivityAsync(Episode2Id, _userId, ContentType.Episode, 3, DateTime.UtcNow.AddMinutes(-20));

        var result = await _sut.IncrWatchingCountSeasonAsync(Season1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var episode1Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Equal(2, episode1Activities.Count);
        Assert.Equal(2, episode1Activities[^1].WatchCount);

        var episode2Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode2Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Equal(2, episode2Activities.Count);
        Assert.Equal(4, episode2Activities[^1].WatchCount);
    }

    [Fact]
    public async Task IncrWatchingCountSeasonAsync_WhenSomeEpisodesAlreadyWatched_WritesOneLatestRecordPerEpisode()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 2, DateTime.UtcNow.AddMinutes(-30));

        var result = await _sut.IncrWatchingCountSeasonAsync(Season1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var episode1Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Equal(2, episode1Activities.Count);
        Assert.Equal(3, episode1Activities[^1].WatchCount);

        var episode2Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode2Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Single(episode2Activities);
        Assert.Equal(1, episode2Activities[0].WatchCount);
    }

    [Fact]
    public async Task IncrWatchingCountSeasonAsync_WhenOtherUserHasHistory_IgnoresIt()
    {
        await AddActivityAsync(Episode1Id, _otherUserId, ContentType.Episode, 7, DateTime.UtcNow.AddMinutes(-30));

        var result = await _sut.IncrWatchingCountSeasonAsync(Season1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var myActivities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId
                        && a.ContentType == ContentType.Episode
                        && (a.ContentId == Episode1Id || a.ContentId == Episode2Id))
            .OrderBy(a => a.ContentId)
            .ToListAsync();

        Assert.Equal(2, myActivities.Count);
        Assert.All(myActivities, activity => Assert.Equal(1, activity.WatchCount));
    }

    [Fact]
    public async Task IncrWatchingCountSeasonAsync_WhenMultiplePriorRowsExist_UsesLatestRow()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-30));
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 3, DateTime.UtcNow.AddMinutes(-10));
        await AddActivityAsync(Episode2Id, _userId, ContentType.Episode, 2, DateTime.UtcNow.AddMinutes(-20));

        var result = await _sut.IncrWatchingCountSeasonAsync(Season1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var episode1Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Equal(3, episode1Activities.Count);
        Assert.Equal(4, episode1Activities[^1].WatchCount);

        var episode2Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode2Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Equal(2, episode2Activities.Count);
        Assert.Equal(3, episode2Activities[^1].WatchCount);
    }

    [Fact]
    public async Task DecrWatchingCountSeasonAsync_WhenOneRecordPerEpisodeExists_DecrementsAllEpisodesAndPersists()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 2, DateTime.UtcNow.AddMinutes(-30));
        await AddActivityAsync(Episode2Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-20));

        var result = await _sut.DecrWatchingCountSeasonAsync(Season1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var episode1Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Equal(2, episode1Activities.Count);
        Assert.Equal(1, episode1Activities[^1].WatchCount);

        var episode2Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode2Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Equal(2, episode2Activities.Count);
        Assert.Equal(0, episode2Activities[^1].WatchCount);

        var progress = await _dbContext.UserTitleProgresses
            .SingleAsync(p => p.UserId == _userId && p.TitleId == TvShowId);
        Assert.Equal(WatchStatus.Watching, progress.Status);
    }

    [Fact]
    public async Task IncrWatchingCountSeasonAsync_WhenAllSeriesEpisodesWatched_SetsProgressCompleted()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-30));
        await AddActivityAsync(Episode2Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-20));

        var result = await _sut.IncrWatchingCountSeasonAsync(Season2Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var progress = await _dbContext.UserTitleProgresses
            .SingleAsync(p => p.UserId == _userId && p.TitleId == TvShowId);
        Assert.Equal(WatchStatus.Completed, progress.Status);
    }

    [Fact]
    public async Task DecrWatchingCountSeasonAsync_WhenOnlySomeEpisodesHaveHistory_DecrementsWatchedEpisodesOnly()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 2, DateTime.UtcNow.AddMinutes(-30));

        var result = await _sut.DecrWatchingCountSeasonAsync(Season1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        var episode1Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode1Id)
            .OrderBy(a => a.Id)
            .ToListAsync();
        Assert.Equal(2, episode1Activities.Count);
        Assert.Equal(1, episode1Activities[^1].WatchCount);

        var episode2Activities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode && a.ContentId == Episode2Id)
            .ToListAsync();
        Assert.Empty(episode2Activities);
    }

    [Fact]
    public async Task DecrWatchingCountSeasonAsync_WhenCurrentUserHasNoHistory_ReturnsFailureAndLeavesDataUntouched()
    {
        await AddActivityAsync(Episode1Id, _otherUserId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-30));
        await AddActivityAsync(Episode2Id, _otherUserId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-20));

        var result = await _sut.DecrWatchingCountSeasonAsync(Season1Id, _userId, CancellationToken.None);

        Assert.False(result.IsSuccess);

        var myActivities = await _dbContext.UserContentActivities
            .Where(a => a.UserId == _userId && a.ContentType == ContentType.Episode)
            .ToListAsync();
        Assert.Empty(myActivities);
    }

    [Fact]
    public async Task GetWatchCountInfoMovieAsync_WhenNoActivityExists_ReturnsZero()
    {
        var result = await _sut.GetWatchCountInfoMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task GetWatchCountInfoMovieAsync_WhenMultipleActivitiesExist_ReturnsLatestCountByWatchedAt()
    {
        await AddActivityAsync(MovieId, _userId, ContentType.Movie, 5, DateTime.UtcNow.AddMinutes(-2));
        await AddActivityAsync(MovieId, _userId, ContentType.Movie, 2, DateTime.UtcNow.AddMinutes(-30));

        var result = await _sut.GetWatchCountInfoMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task GetWatchCountInfoMovieAsync_WhenAnotherUserHasNewerActivity_IgnoresIt()
    {
        await AddActivityAsync(MovieId, _userId, ContentType.Movie, 2, DateTime.UtcNow.AddMinutes(-20));
        await AddActivityAsync(MovieId, _otherUserId, ContentType.Movie, 9, DateTime.UtcNow.AddMinutes(-1));

        var result = await _sut.GetWatchCountInfoMovieAsync(MovieId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(2, result.Value);
    }

    [Fact]
    public async Task GetWatchCountInfoEpisodeAsync_WhenNoActivityExists_ReturnsZero()
    {
        var result = await _sut.GetWatchCountInfoEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(0, result.Value);
    }

    [Fact]
    public async Task GetWatchCountInfoEpisodeAsync_WhenMultipleActivitiesExist_ReturnsLatestCountByWatchedAt()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 4, DateTime.UtcNow.AddMinutes(-3));
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-40));

        var result = await _sut.GetWatchCountInfoEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(4, result.Value);
    }

    [Fact]
    public async Task GetWatchCountInfoEpisodeAsync_WhenAnotherUserHasNewerActivity_IgnoresIt()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 3, DateTime.UtcNow.AddMinutes(-25));
        await AddActivityAsync(Episode1Id, _otherUserId, ContentType.Episode, 8, DateTime.UtcNow.AddMinutes(-1));

        var result = await _sut.GetWatchCountInfoEpisodeAsync(Episode1Id, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(3, result.Value);
    }

    [Fact]
    public async Task GetWatchCountInfoTvShowAsync_WhenNoEpisodeActivityExists_ReturnsEmptyInfo()
    {
        var result = await _sut.GetWatchCountInfoTvShowAsync(TvShowId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(TvShowId, result.Value.TvShowId);
        Assert.Empty(result.Value.EpisodeWatchInfos);
    }

    [Fact]
    public async Task GetWatchCountInfoTvShowAsync_WhenEpisodeActivityExists_ReturnsLatestCountsForEpisodesInRequestedShowOnly()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 2, DateTime.UtcNow.AddMinutes(-15));
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 4, DateTime.UtcNow.AddMinutes(-5));
        await AddActivityAsync(Episode2Id, _userId, ContentType.Episode, 1, DateTime.UtcNow.AddMinutes(-10));
        await AddActivityAsync(Episode3Id, _userId, ContentType.Episode, 3, DateTime.UtcNow.AddMinutes(-7));

        var result = await _sut.GetWatchCountInfoTvShowAsync(TvShowId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        Assert.Equal(TvShowId, result.Value.TvShowId);
        var episodeInfos = result.Value.EpisodeWatchInfos
            .OrderBy(info => info.EpisodeId)
            .ToList();

        Assert.Equal(3, episodeInfos.Count);
        Assert.Collection(
            episodeInfos,
            item =>
            {
                Assert.Equal(Episode1Id, item.EpisodeId);
                Assert.Equal(4, item.Count);
            },
            item =>
            {
                Assert.Equal(Episode2Id, item.EpisodeId);
                Assert.Equal(1, item.Count);
            },
            item =>
            {
                Assert.Equal(Episode3Id, item.EpisodeId);
                Assert.Equal(3, item.Count);
            });
    }

    [Fact]
    public async Task GetWatchCountInfoTvShowAsync_WhenAnotherUserHasActivity_IgnoresIt()
    {
        await AddActivityAsync(Episode1Id, _userId, ContentType.Episode, 2, DateTime.UtcNow.AddMinutes(-20));
        await AddActivityAsync(Episode2Id, _otherUserId, ContentType.Episode, 7, DateTime.UtcNow.AddMinutes(-1));

        var result = await _sut.GetWatchCountInfoTvShowAsync(TvShowId, _userId, CancellationToken.None);

        Assert.True(result.IsSuccess, result.Error);

        Assert.Equal(TvShowId, result.Value.TvShowId);
        var episodeInfos = result.Value.EpisodeWatchInfos.ToList();

        Assert.Single(episodeInfos);
        Assert.Equal(Episode1Id, episodeInfos[0].EpisodeId);
        Assert.Equal(2, episodeInfos[0].Count);
    }

    private async Task SeedBaselineAsync()
    {
        _dbContext.Users.AddRange(
            new User { Id = _userId, UserName = "watch-user", Email = "watch-user@example.com" },
            new User { Id = _otherUserId, UserName = "watch-other", Email = "watch-other@example.com" });

        _dbContext.Titles.Add(new Title
        {
            Id = MovieId,
            Name = "Test Movie",
            Overview = "Movie for watch tracking integration tests.",
            UpdatedAt = DateTime.UtcNow,
            Runtime = 120,
            IsAdult = false,
            ContentType = TitleType.Movie
        });

        _dbContext.Titles.Add(new TvShow
        {
            Id = TvShowId,
            Name = "Test TV Show",
            Overview = "Series for watch tracking integration tests.",
            UpdatedAt = DateTime.UtcNow,
            Runtime = 45,
            IsAdult = false,
            ContentType = TitleType.Series,
            NumberOfSeasons = 2,
            NumberOfEpisodes = 3,
            InProduction = false,
            OriginalName = "Test TV Show"
        });

        _dbContext.Seasons.AddRange(
            new Season { Id = Season1Id, TitleId = TvShowId, Name = "Season 1", OrdinalNumber = 1 },
            new Season { Id = Season2Id, TitleId = TvShowId, Name = "Season 2", OrdinalNumber = 2 });

        _dbContext.Episodes.AddRange(
            new Episode
            {
                Id = Episode1Id,
                Name = "Episode 1",
                OrdinalNumber = 1,
                Runtime = 50,
                SeasonId = Season1Id,
                TvShowId = TvShowId,
                UpdatedAt = DateTime.UtcNow
            },
            new Episode
            {
                Id = Episode2Id,
                Name = "Episode 2",
                OrdinalNumber = 2,
                Runtime = 51,
                SeasonId = Season1Id,
                TvShowId = TvShowId,
                UpdatedAt = DateTime.UtcNow
            },
            new Episode
            {
                Id = Episode3Id,
                Name = "Episode 3",
                OrdinalNumber = 1,
                Runtime = 49,
                SeasonId = Season2Id,
                TvShowId = TvShowId,
                UpdatedAt = DateTime.UtcNow
            });

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
