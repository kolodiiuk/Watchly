using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Watchly.Domain.Entities;
using Watchly.Infrastructure.DbContexts;
using Watchly.Infrastructure.Repositories;
using Watchly.Tests.Fixtures;

namespace Watchly.Tests.IntegrationTests;

public class RefreshTokenRepositoryTests : IClassFixture<DatabaseFixture>, IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private WatchlyDbContext _dbContext;
    private RefreshTokenRepository _sut;

    public RefreshTokenRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<WatchlyDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _dbContext = new WatchlyDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();

        var loggerMock = new Mock<ILogger<RefreshTokenRepository>>();
        _sut = new RefreshTokenRepository(_dbContext, loggerMock.Object);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    private async Task<User> CreateTestUserAsync()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", UserName = "testuser" };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task AddRefreshTokenAsync_ShouldSaveTokenToDatabase()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var token = new RefreshToken
        {
            Token = "test_token",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _sut.AddRefreshTokenAsync(token, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var savedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "test_token");
        savedToken.Should().NotBeNull();
        savedToken!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetRefreshTokenByValueAsync_ShouldReturnToken_WhenExists()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var token = new RefreshToken
        {
            Token = "existing_token",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.RefreshTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetRefreshTokenByValueAsync("existing_token", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("existing_token");
        result.Value.User.Should().NotBeNull();
        result.Value.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetRefreshTokenByValueAsync_ShouldReturnFail_WhenTokenDoesNotExist()
    {
        // Act
        var result = await _sut.GetRefreshTokenByValueAsync("missing_token", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("No such token");
    }

    [Fact]
    public async Task RevokeRefreshTokenByValueAsync_ShouldUpdateTokenRevokedStatus()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var token = new RefreshToken
        {
            Token = "token_to_revoke",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.RefreshTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();

        // Act
        token.Revoked = DateTime.UtcNow;
        token.RevokedByIp = "192.168.1.1";
        var result = await _sut.RevokeRefreshTokenByValueAsync(token, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedToken = await _dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.Token == "token_to_revoke");
        updatedToken!.Revoked.Should().NotBeNull();
        updatedToken.RevokedByIp.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task AddRefreshTokenWithRevocationAsync_ShouldRevokeOldAndInsertNew()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        var oldToken = new RefreshToken
        {
            Token = "old_token",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        await _dbContext.RefreshTokens.AddAsync(oldToken);
        await _dbContext.SaveChangesAsync();

        var newToken = new RefreshToken
        {
            Token = "new_token",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _sut.AddRefreshTokenWithRevocationAsync(newToken, oldToken, "192.168.1.1", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var savedOldToken = await _dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.Token == "old_token");
        savedOldToken!.Revoked.Should().NotBeNull();
        savedOldToken.RevokedByIp.Should().Be("192.168.1.1");
        savedOldToken.ReplacedByToken.Should().Be("new_token");

        var savedNewToken = await _dbContext.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(rt => rt.Token == "new_token");
        savedNewToken.Should().NotBeNull();
        savedNewToken!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task RevokeTokenFamilyAsync_ShouldRevokeAllActiveUserTokens()
    {
        // Arrange
        var user = await CreateTestUserAsync();
        
        var token1 = new RefreshToken
        {
            Token = "token1",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };
        var token2 = new RefreshToken
        {
            Token = "token2",
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = "127.0.0.1",
            CreatedAt = DateTime.UtcNow
        };

        await _dbContext.RefreshTokens.AddRangeAsync(token1, token2);
        await _dbContext.SaveChangesAsync();

        var revocationDate = DateTime.UtcNow;
        var revocationIp = "192.168.1.100";

        // Act
        var result = await _sut.RevokeTokenFamilyAsync(user.Id, revocationDate, revocationIp, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var tokens = await _dbContext.RefreshTokens.AsNoTracking().Where(rt => rt.UserId == user.Id).ToListAsync();
        tokens.Should().HaveCount(2);
        tokens.Should().AllSatisfy(rt => 
        {
            rt.Revoked.Should().BeCloseTo(revocationDate, TimeSpan.FromMilliseconds(100)); // PG resolution might vary slightly
            rt.RevokedByIp.Should().Be(revocationIp);
            rt.ReplacedByToken.Should().BeNull();
        });
    }
}
