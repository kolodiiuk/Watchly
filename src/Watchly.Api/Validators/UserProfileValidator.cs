using Microsoft.IdentityModel.Tokens;
using Watchly.Api.Dto.Auth;
using Watchly.Api.Dto.UserProfile;
using Watchly.Domain.Extensions;

namespace Watchly.Api.Validators;

internal class UserProfileValidator
{
    internal const string ChangeUsername = "Watchly.Api.Controllers.UserProfileController.ChangeUsernameAsync (Watchly.Api)";
    internal const string ChangePassword = "Watchly.Api.Controllers.UserProfileController.ChangePasswordAsync (Watchly.Api)";
    internal const string ForgetPassword = "Watchly.Api.Controllers.UserProfileController.ForgetPasswordAsync (Watchly.Api)";
    internal const string ResetPassword = "Watchly.Api.Controllers.UserProfileController.ResetPasswordAsync (Watchly.Api)";
    internal const string UpdatePictureProfile = "Watchly.Api.Controllers.UserProfileController.UpdatePictureProfileAsync (Watchly.Api)";

    internal static bool ValidateChangeUsername(IDictionary<string, object> map)
    {
        return map.TryGetValue("req", out var request) 
               && request is not null
               && !string.IsNullOrWhiteSpace(((ChangeUserNameRequest)request).Name);
    }

    internal static bool ValidateChangePassword(IDictionary<string, object> map)
    {
        map.TryGetValue("request", out var request);
        var cpRequest = request as ChangePasswordRequest;

        return !cpRequest.OldPassword.IsNullOrEmpty()
               && !cpRequest.NewPassword.IsNullOrEmpty()
               && cpRequest.OldPassword != cpRequest.NewPassword;
    }

    internal static bool ValidateForgetPassword(IDictionary<string, object> map)
    {
        map.TryGetValue("req", out var request);
        var fpRequest = request as ForgetPasswordRequest;

        return fpRequest?.Email != null
               && fpRequest.Email.IsValidEmail();
    }

    internal static bool ValidateResetPassword(IDictionary<string, object> map)
    {
        return map.TryGetValue("token", out var token)
            && token is not null
            && !string.IsNullOrWhiteSpace((string)token);
    }

    internal static bool ValidateUpdatePictureProfile(IDictionary<string, object> map)
    {
        return map.TryGetValue("file", out var file)
               && file is not null
               && ((IFormFile)file).Length != 0;
    }
}
