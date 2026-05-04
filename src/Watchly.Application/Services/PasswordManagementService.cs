using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Watchly.Application.Interfaces;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;
using Watchly.Infrastructure.Interfaces;
using Watchly.Infrastructure.Models;

namespace Watchly.Application.Services;

public sealed class PasswordManagementService : LoggingService<PasswordManagementService>, IPasswordManagementService
{
    private readonly UserManager<User> _userManager;

    private readonly IEmailService _emailService;

    private readonly WatchlyDbContext _dbContext;

    public PasswordManagementService(
        UserManager<User> userManager,
        IEmailService emailService,
        WatchlyDbContext watchlyDbContext,
        ILogger<PasswordManagementService> logger)
        : base(logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _dbContext = watchlyDbContext;
    }

    public async Task<Result> ChangePasswordAsync(
        Guid userId,
        string oldPassword,
        string newPassword,
        CancellationToken ct)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return Result.Fail("User not found");
            }

            var res = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

            return res.Succeeded switch
            {
                false => Result.Fail(string.Join('\n', res.Errors.Select(e => e.Description))),
                _ => Result.Success()
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            return Result.Fail($"Problem changing password: {e.Message}");
        }
    }

    public async Task<Result> SendPasswordResetConfirmationAsync(string userEmail, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user is null)
            {
                return Result.Fail("User is not found");
            }

            var body = await GenBodyPasswordResetAsync(user.UserName, user.Id, ct);
            var message = new EmailMessage
            {
                To = userEmail, IsHtml = true, Subject = "Password Reset", Body = body
            };
            var res = await _emailService.SendAsync(message, ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Result<bool>> ValidateResetPasswordRequestAsync(string token, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            throw new NotImplementedException();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            return Result<bool>.Fail($"Problem validating reset password request: {e.Message}");
        }
    }

    private async Task<string> GenBodyPasswordResetAsync(string userName, Guid userId, CancellationToken ct)
    {
        var token = await CreateTokenAsync(userId, ct);
        if (token == null)
        {
            return null;
        }

        var url = $"http://localhost:5171/api/users/reset-password?token={token}";

        return $"""
                Hello, {userName}

                Someone has requested a link to change your password, and you can do this through the link below.
                {url}
                If you didn't request this, please ignore this email.

                Your password won't change until you access the link above and create a new one.
                """;
    }

    private async Task<string> CreateTokenAsync(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var tokenValue = GenerateToken();
        var passwordResetToken = new PasswordResetToken
        {
            TokenHash = HashToken(tokenValue),
            UserId = userId,
            Expires = DateTime.UtcNow + TimeSpan.FromDays(3)
        };
        try
        {
            await _dbContext.PasswordResetTokens.AddAsync(passwordResetToken, ct);
            var rows = await _dbContext.SaveChangesAsync(ct);
            if (rows == 0)
            {
                return null;
            }

            return tokenValue;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    private static string GenerateToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        var tokenValue = Convert.ToHexString(randomNumber).ToLower();

        return tokenValue;
    }

    public static string HashToken(string rawToken)
    {
        var inputBytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(inputBytes);

        return Convert.ToHexString(hashBytes).ToLower();
    }
}
