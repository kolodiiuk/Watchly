using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Watchly.Application.Models.Comments;
using Watchly.Application.Services;
using Watchly.Domain.Entities;
using Watchly.Infrastructure.DbContexts;
using Watchly.Tests.Fixtures;

namespace Watchly.Tests.IntegrationTests;

public class CommentServiceIntegrationTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _dbFixture;
    private WatchlyDbContext _dbContext;
    private CommentService _commentService;

    private Guid UserId1 = Guid.NewGuid();
    private Guid UserId2 = Guid.NewGuid();
    private int TitleId = 100;
    private const int TvShowId = 300;
    private int EpisodeId = 200;

    public CommentServiceIntegrationTests(DatabaseFixture fixture)
    {
        _dbFixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _dbFixture.InitializeAsync();

        _dbContext = new WatchlyDbContext(
            new ServiceCollection()
                .AddDbContext<WatchlyDbContext>(options => options.UseNpgsql(_dbFixture.ConnectionString))
                .BuildServiceProvider().GetService<DbContextOptions<WatchlyDbContext>>()
        );
        await _dbContext.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        await _dbContext.Database.MigrateAsync();
        await DatabaseFixture.ResetDatabaseAsync(_dbContext);
        _commentService = new CommentService(_dbContext);
        await _dbContext.Database.EnsureCreatedAsync();
        await SeedDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbFixture.DisposeAsync();
    }
    
    private async Task SeedDataAsync()
    {
        var user1 = new User { Id = UserId1, UserName = "user1", Email = "user1@example.com" };
        var user2 = new User { Id = UserId2, UserName = "user2", Email = "user2@example.com" };

        _dbContext.Users.AddRange(user1, user2);
        await _dbContext.SaveChangesAsync();


        // 2. Seed Content (Title, Season, and Episode)
        var title = new Title
            { Id = TitleId, Name = "Test Movie", Overview = "Test overview", UpdatedAt = DateTime.UtcNow };
        var tvShow = new Title
            { Id = TvShowId, Name = "Tv show", Overview = "Test overview tv show", UpdatedAt = DateTime.UtcNow };
        var season = new Season { Id = 1, OrdinalNumber = 1, Name = "Season 1", TitleId = TitleId };
        var episode = new Episode
        {
            Id = EpisodeId, SeasonId = season.Id, TvShowId = TvShowId, OrdinalNumber = 1, Name = "Test Scene", Runtime = 30,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Titles.Add(title);
        _dbContext.Titles.Add(tvShow);
        _dbContext.Seasons.Add(season);
        _dbContext.Episodes.Add(episode);
        await _dbContext.SaveChangesAsync();


        var initialComments = new List<Comment>
        {
            // Comment on Title (User 1 owns, should be visible for title checks)
            new Comment
            {
                TitleId = TitleId, EpisodeId = null, IsDeleted = false, UserId = UserId1, Text = "Great movie!",
                UpdatedAt = DateTime.UtcNow
            },
            // Comment on Title (User 2 owns, should be visible for title checks)
            new Comment
            {
                TitleId = TitleId, EpisodeId = null, IsDeleted = false, UserId = UserId2, Text = "Love the acting.",
                UpdatedAt = DateTime.UtcNow
            },

            // Comment on Episode (User 1 owns, should be visible for episode checks)
            new Comment
            {
                TitleId = null, EpisodeId = EpisodeId, IsDeleted = false, UserId = UserId1, Text = "Best season yet!",
                UpdatedAt = DateTime.UtcNow
            }
        };

        _dbContext.Comments.AddRange(initialComments);
        await _dbContext.SaveChangesAsync();
    }

    #region GetCommentsAsync Tests

    [Fact]
    public async Task GetCommentsAsync_WhenIsTitleIsTrueAndContentExists_ReturnsListOfTitlesComments()
    {
        // Arrange: TitleId 100 has two comments (IDs 1 and 2)
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _commentService.GetCommentsAsync(TitleId, true, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        var comments = (IEnumerable<CommentDto>)result.Value;

        // Expect exactly 2 comments for the title
        Assert.Equal(2, comments.Count());
    }

    [Fact]
    public async Task GetCommentsAsync_WhenIsTitleIsFalseAndContentExists_ReturnsListOfEpisodeComments()
    {
        // Arrange: EpisodeId 200 has one comment (ID 3)
        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _commentService.GetCommentsAsync(EpisodeId, false, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        var comments = (IEnumerable<CommentDto>)result.Value;

        // Expect exactly 1 comment for the episode
        Assert.Equal(1, comments.Count());
    }

    [Fact]
    public async Task GetCommentsAsync_WhenContentHasNoComments_ReturnsEmptyList()
    {
        // Arrange: Use a guaranteed non-existent/empty content ID (e.g., 999)
        int emptyContentId = 999;
        var cancellationToken = CancellationToken.None;

        // Act - Test Title path (false positive check)
        var titleResult = await _commentService.GetCommentsAsync(emptyContentId, true, cancellationToken);
        Assert.True(titleResult.IsSuccess);
        Assert.NotNull((IEnumerable<CommentDto>)titleResult.Value);
        Assert.Empty((IEnumerable<CommentDto>)titleResult.Value);

        // Act - Test Episode path
        var episodeResult = await _commentService.GetCommentsAsync(emptyContentId, false, cancellationToken);
        Assert.True(episodeResult.IsSuccess);
        Assert.NotNull((IEnumerable<CommentDto>)episodeResult.Value);
        Assert.Empty((IEnumerable<CommentDto>)episodeResult.Value);
    }

    #endregion

    #region LeaveCommentAsync Tests

    [Fact]
    public async Task LeaveCommentAsync_WhenSuccessfulAndIsTitleIsTrue_CreatesNewTitleComment()
    {
        // Arrange
        string newText = "This is a brand new title comment.";
        var request = new LeaveCommentRequest(IsTitle: true, ContentId: TitleId, Text: newText);

        // Act
        var result = await _commentService.LeaveCommentAsync(request, UserId1, CancellationToken.None);

// Assert Service Result
        Console.WriteLine($"IsSuccess: {result.IsSuccess}, Error: {result.Error}");

        if (!result.IsSuccess && result.Error.Contains("saving"))
        {
            var comments = await _dbContext.Comments.ToListAsync();
            Console.WriteLine($"Comments in DB: {comments.Count}");
            foreach (var c in comments)
            {
                Console.WriteLine($"Comment: {c.Id}, UserId: {c.UserId}, TitleId: {c.TitleId}, Text: {c.Text}");
            }
        }

        Assert.True(result.IsSuccess, $"Result failed: {result.Error}");

        // Verify DB State (Check for the newly created record)
        var newComment = await _dbContext.Comments
            .Where(c => c.UserId == UserId1 && c.TitleId == TitleId && c.Text == newText)
            .ToListAsync();

        Assert.Single(newComment);
    }

    [Fact]
    public async Task LeaveCommentAsync_WhenSuccessfulAndIsTitleIsFalse_CreatesNewEpisodeComment()
    {
        // Arrange
        string newText = "Awesome episode analysis.";
        var request = new LeaveCommentRequest(IsTitle: false, ContentId: EpisodeId, Text: newText);

        // Act
        var result = await _commentService.LeaveCommentAsync(request, UserId2, CancellationToken.None);

        // Assert Service Result
        Assert.True(result.IsSuccess, $"Result failed: {result.Error}");

        // Verify DB State
        var newComment = await _dbContext.Comments
            .Where(c => c.UserId == UserId2 && c.EpisodeId == EpisodeId && c.Text == newText)
            .ToListAsync();

        Assert.Single(newComment);
    }

    [Fact]
    public async Task LeaveCommentAsync_WhenUserDoesNotExist_ReturnsFailure()
    {
        // Arrange
        var request = new LeaveCommentRequest(IsTitle: true, ContentId: TitleId, Text: "jvg");
        Guid nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await _commentService.LeaveCommentAsync(request, nonExistentUserId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("user is not found", result.Error);
    }

    #endregion

    #region UpdateCommentAsync Tests

    [Fact]
    public async Task UpdateCommentAsync_WhenOwnerUpdatesOwnComment_Succeeds()
    {
        // Arrange: Target Comment ID 1 (Owned by User1, Title)
        int commentIdToUpdate = 1;
        string newText = "Updated thoughts on the movie!";

        // Act
        var result =
            await _commentService.UpdateCommentAsync(commentIdToUpdate, newText, UserId1, CancellationToken.None);

        // Assert Service Result
        Assert.True(result.IsSuccess, $"Result failed: {result.Error}");

        // Verify DB State
        var updatedComment = await _dbContext.Comments
            .Where(c => c.Id == commentIdToUpdate)
            .ToListAsync();

        Assert.Single(updatedComment);
        Assert.Equal(newText, updatedComment[0].Text);
    }

    [Fact]
    public async Task UpdateCommentAsync_WhenNonOwnerTriesToUpdateComment_Fails()
    {
        // Arrange: Target Comment ID 2 (Owned by User2)
        int commentId = 2; // Owned by UserId2
        string maliciousText = "Hacked!";

        // Act: Attempting to update with User1's ID
        var result =
            await _commentService.UpdateCommentAsync(commentId, maliciousText, UserId1, CancellationToken.None);

        // Assert Service Result
        Assert.False(result.IsSuccess);
        Assert.Contains("user doesn't own comment", result.Error);
    }

    [Fact]
    public async Task UpdateCommentAsync_WhenCommentDoesNotExist_Fails()
    {
        // Arrange
        int nonExistentId = 99;

        // Act
        var result = await _commentService.UpdateCommentAsync(nonExistentId, "text", UserId1, CancellationToken.None);

        // Assert Service Result (The service checks for user first, but if the comment is null later it usually implies failure)
        // In this specific implementation, if comment does not exist, we assume EF Core returns null/default behavior which leads to a generic fail or success depending on transaction isolation.
        // For robust testing, we ensure the explicit checks for existence are covered (User exists, Comment doesn't).
        var dbComment = await _dbContext.Comments.FindAsync(nonExistentId);
        if (dbComment == null)
        {
            // The service will try to fetch it and fail gracefully or throw if not handled properly by EF tracking.
            // Assuming the current implementation handles non-existent comment IDs safely:
            var resultEmpty =
                await _commentService.UpdateCommentAsync(nonExistentId, "text", UserId1, CancellationToken.None);
            Assert.False(resultEmpty.IsSuccess);
            Assert.Contains("comment is not found", resultEmpty.Error);
        }
    }

    #endregion

    #region DeleteCommentAsync Tests

    [Fact]
    public async Task DeleteCommentAsync_WhenOwnerDeletesOwnComment_Succeeds()
    {
        // Arrange: Target Comment ID 1 (Owned by User1)
        int commentIdToDelete = 1;

        // Act
        var result = await _commentService.DeleteCommentAsync(commentIdToDelete, UserId1, CancellationToken.None);

        // Assert Service Result
        Assert.True(result.IsSuccess, $"Result failed: {result.Error}");

        // Verify DB State (Check if the flag was set)
        var deletedComment = await _dbContext.Comments
            .Where(c => c.Id == commentIdToDelete)
            .ToListAsync();

        Assert.Single(deletedComment);
        Assert.True(deletedComment[0].IsDeleted);
    }

    [Fact]
    public async Task DeleteCommentAsync_WhenNonOwnerTriesToDeleteComment_Fails()
    {
        // Arrange: Target Comment ID 2 (Owned by User2)
        int commentId = 2; // Owned by UserId2

        // Act: Attempting to delete with User1's ID
        var result = await _commentService.DeleteCommentAsync(commentId, UserId1, CancellationToken.None);

        // Assert Service Result
        Assert.False(result.IsSuccess);
        Assert.Contains("user doesn't own comment", result.Error);
    }

    [Fact]
    public async Task DeleteCommentAsync_WhenCommentDoesNotExist_Fails()
    {
        // Arrange
        int nonExistentId = 99;

        // Act
        var result = await _commentService.DeleteCommentAsync(nonExistentId, UserId1, CancellationToken.None);

        // Assert Service Result
        Assert.False(result.IsSuccess);
        Assert.Contains("comment is not found", result.Error);
    }

    #endregion
}