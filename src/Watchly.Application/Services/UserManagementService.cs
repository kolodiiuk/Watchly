using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Watchly.Application.Interfaces;
using Watchly.Application.Models;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Application.Services;

public sealed class UserManagementService : LoggingService<UserManagementService>, IUserManagementService
{
    private readonly UserManager<User> _userManager;

    public UserManagementService(UserManager<User> userManager,
        ILogger<UserManagementService> logger) : base(logger)
    {
        _userManager = userManager;
    }

    public async Task<Result> ChangeUserNameAsync(Guid userId, string userName)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Fail("User is null");
        }

        var res = await _userManager.SetUserNameAsync(user, userName);
        if (!res.Succeeded)
        {
            return Result.Fail(FormatIdentityError(res));
        }

        try
        {
            var updateRes = await _userManager.UpdateAsync(user);
            if (!updateRes.Succeeded)
            {
                return Result.Fail(FormatIdentityError(res));
            }

            return Result.Success();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Result<UserInfo>> GetUserAsync(Guid userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            return user switch
            {
                null => Result<UserInfo>.Fail("User is null"),
                _ => Result<UserInfo>.Success(new UserInfo { Id = user.Id, Email = user.Email })
            };
        }
        catch (Exception e)
        {
            return Result<UserInfo>.Fail(e.Message);
        }
    }

    private string FormatIdentityError(IdentityResult error)
    {
        var sb = new StringBuilder();
        foreach (var identityError in error.Errors)
        {
            sb.Append(identityError.Code)
                .Append('\n')
                .Append(identityError.Description)
                .Append('\n');
        }

        return sb.ToString();
    }
}
