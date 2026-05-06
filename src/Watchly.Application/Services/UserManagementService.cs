using System.Net;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.UserProfile;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Application.Services;

public sealed class UserManagementService : LoggingService<UserManagementService>, IUserManagementService
{
    private readonly UserManager<User> _userManager;

    private readonly Cloudinary _cloudinary;

    public UserManagementService(
        UserManager<User> userManager,
        Cloudinary cloudinary,
        ILogger<UserManagementService> logger) : base(logger)
    {
        _userManager = userManager;
        _cloudinary = cloudinary;
    }

    public async Task<Result> ChangeUserNameAsync(Guid userId, string userName)
    {
        try
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

    public async Task<Result> UpdateProfilePictureAsync(Stream stream, Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Result.Fail("No such user");
        }

        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(Guid.NewGuid().ToString(), stream),
            PublicId = Guid.NewGuid().ToString(),
            Overwrite = true,
            Folder = "uploads/"
        };
        var uploadResult = await _cloudinary.UploadAsync(uploadParams, ct);
        if (uploadResult.StatusCode != HttpStatusCode.OK)
        {
            return Result.Fail("Upload failed.");
        }

        user.ProfilePictureUrl = uploadResult.Url.AbsoluteUri;
        var updateRes = await _userManager.UpdateAsync(user);
        if (!updateRes.Succeeded)
        {
            return Result.Fail("Error updating user");
        }

        return Result.Success();
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
