using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IPasswordManagementService
{
    Task<Result<bool>> ValidateResetPasswordRequestAsync(string token, CancellationToken ct);

    Task<Result> SendPasswordResetConfirmationAsync(string userEmail, CancellationToken ct);

    Task<Result> ChangePasswordAsync(Guid parsedUserId, string reqOldPassword, string reqNewPassword, CancellationToken ct);
}
