using Watchly.Application.Models.UserProfile;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IUserManagementService
{
    Task<Result> ChangeUserNameAsync(Guid userId, string userName);

    Task<Result<UserInfo>> GetUserAsync(Guid userId);

    Task<Result> UpdateProfilePictureAsync(Stream stream, Guid userId, CancellationToken ct = default);
}
