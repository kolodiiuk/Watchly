using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Watchly.Application.Interfaces;
using Watchly.Application.Models;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.Interfaces;

namespace Watchly.Application.Services;

public class AuthService : LoggingService<AuthService>, IAuthService
{
    private readonly IJwtService _jwtService;

    private readonly IRefreshTokenRepository _refreshTokenRepository;

    private readonly UserManager<User> _userManager;

    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<User> userManager,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
        : base(logger)
    {
        _userManager = userManager;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result> SignUpAsync(string email, string password)
    {
        var user = new User { Email = email };
        user.UserName = user.Email ?? "";
        try
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return Result.Fail($"Failed to create a user: {string.Join(", ",
                    result.Errors.Select(e => e.Description))}");
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Fail($"Error registering user: {e.Message}");
        }
    }

    public async Task<Result<SignInResponse>> SignInAsync(
        string email, string password, string ipAddress, CancellationToken ct)
    {
        var validationResult = await ValidateUserCredentialsAsync(email, password);
        if (validationResult.Failure)
        {
            return Result<SignInResponse>.Fail("User credentials validation failure");
        }

        var tokensRes = await GenerateTokensAsync(validationResult.Value, ipAddress, ct);
        if (tokensRes.Failure)
        {
            return Result<SignInResponse>.Fail(tokensRes.Error);
        }

        var response = CreateSignInResponse(tokensRes.Value, email, validationResult.Value.Id);

        return Result<SignInResponse>.Success(response);
    }

    public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(
        string token, string ipAddress, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var storedRefreshTokenRes = await _refreshTokenRepository.GetRefreshTokenByValueAsync(token, ct);
        if (storedRefreshTokenRes.Failure || storedRefreshTokenRes.Value == null)
        {
            return Result<RefreshTokenResponse>.Fail("Invalid refresh token");
        }

        var user = await _userManager.FindByIdAsync(storedRefreshTokenRes.Value.UserId.ToString());
        if (user == null)
        {
            return Result<RefreshTokenResponse>.Fail("User not found");
        }

        var storedRefreshToken = storedRefreshTokenRes.Value;
        if (storedRefreshToken.Revoked != null)
        {
            if (!string.IsNullOrEmpty(storedRefreshToken.ReplacedByToken))
            {
                var revoked = DateTime.UtcNow;
                var revokedByIp = ipAddress;
                await _refreshTokenRepository.RevokeTokenFamilyAsync(
                    storedRefreshToken.UserId, revoked, revokedByIp, ct);

                return Result<RefreshTokenResponse>.Fail("Token reuse detected");
            }

            return Result<RefreshTokenResponse>.Fail("Token revoked");
        }

        if (storedRefreshToken.Expires <= DateTime.UtcNow)
        {
            // automatic revocation by expiration date
            return Result<RefreshTokenResponse>.Fail("Token expired");
        }

        var newToken = _jwtService.GenerateToken(storedRefreshToken.User);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        storedRefreshToken.Revoked = DateTime.UtcNow;
        storedRefreshToken.RevokedByIp = ipAddress;
        storedRefreshToken.ReplacedByToken = newRefreshToken;

        var userRefreshToken = new RefreshToken
        {
            UserId = storedRefreshToken.UserId,
            Token = newRefreshToken,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        var addTokenRes = await _refreshTokenRepository.AddRefreshTokenWithRevocationAsync(
            userRefreshToken, storedRefreshToken, ipAddress, ct);
        if (addTokenRes.Failure)
        {
            return Result<RefreshTokenResponse>.Fail(addTokenRes.Error);
        }

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            Id = storedRefreshToken.User.Id,
            Email = storedRefreshToken.User.Email,
        });
    }

    public async Task<Result> SignOutAsync(string token, string ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Result.Fail("Token cannot be empty");
        }

        ct.ThrowIfCancellationRequested();
        var refreshTokenRes = await _refreshTokenRepository.GetRefreshTokenByValueAsync(token, ct);

        if (refreshTokenRes.IsSuccess && refreshTokenRes.Value != null)
        {
            refreshTokenRes.Value.Revoked = DateTime.UtcNow;
            refreshTokenRes.Value.RevokedByIp = ipAddress;
            var rtUpdateRes = await _refreshTokenRepository.RevokeRefreshTokenByValueAsync(refreshTokenRes.Value, ct);
            if (rtUpdateRes.Failure)
            {
                return Result.Fail($"Couldn't revoke token: {rtUpdateRes.Error}");
            }
        }

        return Result.Success();
    }

    private async Task<Result<User>> ValidateUserCredentialsAsync(string email, string password)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _userManager.PasswordHasher.HashPassword(new User(), password);

                return Result<User>.Fail("Invalid email or password");
            }

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, password);

            return isPasswordCorrect switch
            {
                false => Result<User>.Fail("Invalid email or password"),
                _ => Result<User>.Success(user)
            };
        }
        catch (Exception e)
        {
            return Result<User>.Fail($"Error validating user credentials: {e.Message}");
        }
    }

    private async Task<Result<TokensResponse>> GenerateTokensAsync(
        User user, string ipAddress, CancellationToken ct)
    {
        if (user == null)
        {
            return Result<TokensResponse>.Fail("User is null");
        }

        ct.ThrowIfCancellationRequested();
        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            Expires = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };

        var addTokenRes = await _refreshTokenRepository.AddRefreshTokenAsync(newRefreshToken, ct);

        return addTokenRes.Failure switch
        {
            true => Result<TokensResponse>.Fail(addTokenRes.Error),
            _ => Result<TokensResponse>.Success(new TokensResponse(token, refreshToken))
        };
    }

    private SignInResponse CreateSignInResponse(TokensResponse tokens, string email, Guid userId)
    {
        var tokenExpiration = DateTime.UtcNow.AddMinutes(
            Convert.ToDouble(_jwtOptions.TokenExpirationMinutes));

        return new SignInResponse
        {
            Token = tokens.Token,
            RefreshToken = tokens.RefreshToken,
            Expiration = tokenExpiration,
            User = new UserDto
            {
                Id = userId,
                Email = email,
            }
        };
    }
}
