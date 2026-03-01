using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Watchly.Application.Interfaces;
using Watchly.Application.Models;
using Watchly.Application.Services;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.Interfaces;

namespace Watchly.Tests.UnitTests;

public class AuthServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenServiceMock;

    private readonly Mock<UserManager<User>> _userManagerMock;

    private readonly Mock<IJwtService> _jwtServiceMock;

    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;

    private readonly Mock<ILogger<AuthService>> _loggerMock;

    private readonly JwtOptions _jwtOptions;

    private readonly AuthService _sut;

    private const string IpAddress = "127.0.0.1";

    public AuthServiceTests()
    {
        _refreshTokenServiceMock = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();

        var userStoreMock = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object,
            null,
            _passwordHasherMock.Object,
            null, null, null, null, null, null);

        _jwtServiceMock = new Mock<IJwtService>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _jwtOptions = new JwtOptions { RefreshTokenExpirationDays = 7 };

        _sut = new AuthService(
            _userManagerMock.Object,
            _refreshTokenServiceMock.Object,
            _jwtServiceMock.Object,
            new OptionsWrapper<JwtOptions>(_jwtOptions),
            _loggerMock.Object
        );
    }

    // ─── SignUpAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SignUpAsync_ShouldReturnSuccess_WhenRegistrationIsSuccessful()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";

        _userManagerMock
            .Setup(x => x.CreateAsync(It.Is<User>(u => u.Email == email), password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.SignUpAsync(email, password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _userManagerMock.Verify(
            x => x.CreateAsync(It.Is<User>(u => u.Email == email), password),
            Times.Once);
    }

    [Fact]
    public async Task SignUpAsync_ShouldReturnFail_WhenCreateAsyncFails()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var error = new IdentityError { Description = "Creation failed" };

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), password))
            .ReturnsAsync(IdentityResult.Failed(error));

        // Act
        var result = await _sut.SignUpAsync(email, password);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Creation failed");
        _userManagerMock.Verify(
            x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SignUpAsync_ShouldReturnFail_WhenExceptionIsThrown()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("DB unavailable"));

        // Act
        var result = await _sut.SignUpAsync("user@test.com", "pass");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("DB unavailable");
    }

    // ─── SignInAsync (exercises ValidateUserCredentialsAsync + GenerateTokensAsync) ──

    [Fact]
    public async Task SignInAsync_ShouldReturnSuccess_WhenCredentialsAreValidAndTokensGenerated()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var user = new User { Id = Guid.NewGuid(), Email = email };
        const string accessToken = "access_token";
        const string refreshToken = "refresh_token";

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(true);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns(accessToken);
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(refreshToken);
        _refreshTokenServiceMock
            .Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.SignInAsync(email, password, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be(refreshToken);
        result.Value.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        var email = "ghost@example.com";
        var password = "Password123!";

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);

        // Act
        var result = await _sut.SignInAsync(email, password, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        // The error message is masked for security at the SignIn level
        result.Error.Should().Contain("validation failure");
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnFail_WhenPasswordIsWrong()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";
        var user = new User { Id = Guid.NewGuid(), Email = email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(false);

        // Act
        var result = await _sut.SignInAsync(email, password, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("validation failure");
    }

    [Fact]
    public async Task SignInAsync_ShouldReturnFail_WhenTokenStorageFails()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var user = new User { Id = Guid.NewGuid(), Email = email };

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password)).ReturnsAsync(true);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh");
        _refreshTokenServiceMock
            .Setup(x => x.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Storage error"));

        // Act
        var result = await _sut.SignInAsync(email, password, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Storage error");
    }

    [Fact]
    public async Task SignInAsync_ShouldDummyHashPassword_WhenUserNotFound_ToPreventTimingAttacks()
    {
        // Arrange
        var email = "ghost@example.com";
        var password = "Password123!";

        _userManagerMock.Setup(x => x.FindByEmailAsync(email)).ReturnsAsync((User)null);

        // Act
        await _sut.SignInAsync(email, password, IpAddress, CancellationToken.None);

        // Assert – the password hasher must be called even when user is not found
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<User>(), password), Times.Once);
    }

    // ─── RefreshTokenAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFail_WhenTokenIsInvalid()
    {
        // Arrange
        var token = "invalid_token";
        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Fail("Invalid refresh token"));

        // Act
        var result = await _sut.RefreshTokenAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid refresh token");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        var token = "valid_token";
        var refreshToken = new RefreshToken
        {
            Expires = DateTime.UtcNow.AddMinutes(1),
            UserId = Guid.NewGuid()
        };

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
        _userManagerMock
            .Setup(x => x.FindByIdAsync(refreshToken.UserId.ToString()))
            .ReturnsAsync((User)null);

        // Act
        var result = await _sut.RefreshTokenAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFail_WhenTokenIsRevoked()
    {
        // Arrange – revoked, no replacement token (simple revocation)
        var token = "revoked_token";
        var user = new User { Id = Guid.NewGuid() };
        var refreshToken = new RefreshToken
        {
            Revoked = DateTime.UtcNow.AddMinutes(-5),
            ReplacedByToken = null,
            UserId = user.Id,
            User = user,
            Expires = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.RefreshTokenAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Token revoked");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFail_WhenTokenExpired()
    {
        // Arrange – not revoked, but expired
        var token = "expired_token";
        var user = new User { Id = Guid.NewGuid() };
        var refreshToken = new RefreshToken
        {
            Expires = DateTime.UtcNow.AddMinutes(-1),
            Revoked = null,
            User = user,
            UserId = user.Id
        };

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.RefreshTokenAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Token expired");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldFailAndRevokeFamily_WhenTokenReused()
    {
        // Arrange – revoked AND has a ReplacedByToken → reuse detected
        var token = "reused_token";
        var user = new User { Id = Guid.NewGuid() };
        var refreshToken = new RefreshToken
        {
            Revoked = DateTime.UtcNow.AddMinutes(-1),
            ReplacedByToken = "some_newer_token",
            UserId = user.Id,
            User = user,
            Expires = DateTime.UtcNow.AddDays(1)
        };

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _refreshTokenServiceMock
            .Setup(x => x.RevokeTokenFamilyAsync(
                user.Id,
                It.IsAny<DateTime>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.RefreshTokenAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Token reuse detected");
        _refreshTokenServiceMock.Verify(
            x => x.RevokeTokenFamilyAsync(user.Id, It.IsAny<DateTime>(), IpAddress, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFail_WhenAddRefreshTokenWithRevocationFails()
    {
        // Arrange
        var token = "valid_token";
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        var refreshToken = new RefreshToken
        {
            Expires = DateTime.UtcNow.AddMinutes(1),
            Revoked = null,
            User = user,
            UserId = user.Id
        };

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("new_access_token");
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("new_refresh_token");
        _refreshTokenServiceMock
            .Setup(x => x.AddRefreshTokenWithRevocationAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<RefreshToken>(),
                IpAddress,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Database error"));

        // Act
        var result = await _sut.RefreshTokenAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Database error");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnSuccess_WhenTokenIsValid()
    {
        // Arrange
        var token = "valid_token";
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com" };
        var refreshToken = new RefreshToken
        {
            Expires = DateTime.UtcNow.AddDays(1),
            Revoked = null,
            User = user,
            UserId = user.Id
        };
        const string newAccessToken = "new_access_token";
        const string newRefreshToken = "new_refresh_token";

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
        _userManagerMock
            .Setup(x => x.FindByIdAsync(user.Id.ToString()))
            .ReturnsAsync(user);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns(newAccessToken);
        _jwtServiceMock.Setup(x => x.GenerateRefreshToken()).Returns(newRefreshToken);
        _refreshTokenServiceMock
            .Setup(x => x.AddRefreshTokenWithRevocationAsync(
                It.IsAny<RefreshToken>(),
                It.IsAny<RefreshToken>(),
                IpAddress,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.RefreshTokenAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be(newAccessToken);
        result.Value.RefreshToken.Should().Be(newRefreshToken);
        result.Value.Email.Should().Be(user.Email);
        result.Value.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnFail_WhenCancellationIsRequested()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.RefreshTokenAsync("some_token", IpAddress, cts.Token));
    }

    // ─── SignOutAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SignOutAsync_ShouldReturnFail_WhenTokenIsEmpty()
    {
        // Act
        var result = await _sut.SignOutAsync("", IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Token cannot be empty");
    }

    [Fact]
    public async Task SignOutAsync_ShouldReturnFail_WhenTokenIsWhitespace()
    {
        // Act
        var result = await _sut.SignOutAsync("   ", IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Token cannot be empty");
    }

    [Fact]
    public async Task SignOutAsync_ShouldRevokeToken_WhenTokenIsFound()
    {
        // Arrange
        var token = "valid_token";
        var refreshToken = new RefreshToken();

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
        _refreshTokenServiceMock
            .Setup(x => x.RevokeRefreshTokenByValueAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _sut.SignOutAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refreshToken.Revoked.Should().NotBeNull();
        refreshToken.RevokedByIp.Should().Be(IpAddress);
    }

    [Fact]
    public async Task SignOutAsync_ShouldReturnSuccess_WhenTokenNotFound()
    {
        // Arrange – token not found → still success (idempotent sign-out)
        var token = "unknown_token";
        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Fail("Token not found"));

        // Act
        var result = await _sut.SignOutAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenServiceMock.Verify(
            x => x.RevokeRefreshTokenByValueAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SignOutAsync_ShouldReturnFail_WhenRevocationFails()
    {
        // Arrange
        var token = "valid_token";
        var refreshToken = new RefreshToken();

        _refreshTokenServiceMock
            .Setup(x => x.GetRefreshTokenByValueAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(refreshToken));
        _refreshTokenServiceMock
            .Setup(x => x.RevokeRefreshTokenByValueAsync(refreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("Database error"));

        // Act
        var result = await _sut.SignOutAsync(token, IpAddress, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Couldn't revoke token: Database error");
    }

    [Fact]
    public async Task SignOutAsync_ShouldReturnFail_WhenCancellationIsRequested()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.SignOutAsync("some_token", IpAddress, cts.Token));
    }
}
