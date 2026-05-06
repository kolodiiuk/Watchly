using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    private const string PasswordLowercase = "abcdefghijklmnopqrstuvwxyz";

    private const string PasswordUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private const string PasswordDigits = "0123456789";

    private const string PasswordSymbols = "!@#$%^&*()-_=+[]{}|;:,.<>?";

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
            var tokenHash = HashToken(token);
            var resetToken = await _dbContext.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
            if (resetToken is null)
            {
                return Result<bool>.Fail("No token with such hash");
            }

            if (resetToken.Expires <= DateTime.UtcNow)
            {
                return Result<bool>.Fail("Token expired");
            }

            var user = await _userManager.FindByIdAsync(resetToken.UserId.ToString());
            if (user is null)
            {
                return Result<bool>.Fail("User does not exist");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var newUser = new User
            {
                Email = user.Email,
                NormalizedEmail = user.NormalizedEmail,
                CreatedAt = user.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                UserName = user.UserName,
                NormalizedUserName = user.NormalizedUserName,
                AccessFailedCount = user.AccessFailedCount,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                ProfilePictureUrl = user.ProfilePictureUrl,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            await using var tx = await _dbContext.Database.BeginTransactionAsync(ct);

            var deleteUserResult = await _userManager.DeleteAsync(user);
            if (!deleteUserResult.Succeeded)
            {
                await tx.RollbackAsync(ct);

                return Result<bool>.Fail(string.Join('\n', deleteUserResult.Errors.Select(e => e.Description)));
            }

            var password = GenerateTemporaryPassword();
            var createUserResult = await _userManager.CreateAsync(newUser, password);
            if (!createUserResult.Succeeded)
            {
                await tx.RollbackAsync(ct);

                return Result<bool>.Fail(string.Join('\n', createUserResult.Errors.Select(e => e.Description)));
            }

            if (currentRoles.Count > 0)
            {
                var addRolesResult = await _userManager.AddToRolesAsync(newUser, currentRoles);
                if (!addRolesResult.Succeeded)
                {
                    await tx.RollbackAsync(ct);
                    return Result<bool>.Fail(string.Join('\n', addRolesResult.Errors.Select(e => e.Description)));
                }
            }

            var userTokens = await _dbContext.PasswordResetTokens
                .Where(t => t.UserId == resetToken.UserId)
                .ToListAsync(ct);
            _dbContext.PasswordResetTokens.RemoveRange(userTokens);
            await _dbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            var body = GenBodyPasswordResetAsync(newUser.UserName!, password, ct);
            var message = new EmailMessage
            {
                To = newUser.Email!, IsHtml = true, Subject = "Password Reset", Body = body
            };

            await _emailService.SendAsync(message, ct);

            return Result<bool>.Success(true);
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

        var url = $"http://localhost:5173/users/reset-password?token={token}";

        return $"""
                Hello, {userName}

                Someone has requested a link to change your password, and you can do this through the link below.
                {url}
                If you didn't request this, please ignore this email.

                Your password won't change until you access the link above and create a new one.
                """;
    }

    private string GenBodyPasswordResetAsync(string userName, string password, CancellationToken ct)
    {
        return $"""
                Hello, {userName}

                You've successfully changed password. New password: {password}
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

    private static string GenerateTemporaryPassword(int length = 15)
    {
        if (length < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Password length must be at least 4.");
        }

        var allChars = PasswordLowercase + PasswordUppercase + PasswordDigits + PasswordSymbols;
        var passwordChars = new char[length];

        passwordChars[0] = PasswordUppercase[RandomNumberGenerator.GetInt32(PasswordUppercase.Length)];
        passwordChars[1] = PasswordSymbols[RandomNumberGenerator.GetInt32(PasswordSymbols.Length)];
        passwordChars[2] = PasswordLowercase[RandomNumberGenerator.GetInt32(PasswordLowercase.Length)];
        passwordChars[3] = PasswordDigits[RandomNumberGenerator.GetInt32(PasswordDigits.Length)];

        for (var i = 4; i < length; i++)
        {
            passwordChars[i] = allChars[RandomNumberGenerator.GetInt32(allChars.Length)];
        }

        for (var i = passwordChars.Length - 1; i > 0; i--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(i + 1);
            (passwordChars[i], passwordChars[swapIndex]) = (passwordChars[swapIndex], passwordChars[i]);
        }

        return new string(passwordChars);
    }

    private static string HashToken(string rawToken)
    {
        var inputBytes = Encoding.UTF8.GetBytes(rawToken);
        var hashBytes = SHA256.HashData(inputBytes);

        return Convert.ToHexString(hashBytes).ToLower();
    }
}
