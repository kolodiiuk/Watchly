using Watchly.Api.Dto.Auth;

namespace Watchly.Api.Validators;

internal class AuthValidator
{
    internal const string SignUp = "Watchly.Api.Controllers.AuthController.SignUpAsync (Watchly.Api)";
    internal const string SignIn = "Watchly.Api.Controllers.AuthController.SignInAsync (Watchly.Api)";
    internal const string SignOut = "Watchly.Api.Controllers.AuthController.SignOutAsync (Watchly.Api)";
    internal const string Refresh = "Watchly.Api.Controllers.AuthController.RefreshAsync (Watchly.Api)";

    internal static bool ValidateSignUp(IDictionary<string, object> map)
    {
        return map.TryGetValue("signUpRequest", out var request) 
               && request is not null
               && ((SignUpRequest)request).IsValid();
    }

    internal static bool ValidateSignIn(IDictionary<string, object> map)
    {
        return map.TryGetValue("request", out var request)
            && request is not null
            && ((SignInRequest)request).IsValid();
    }

    internal static bool ValidateSignOut(IDictionary<string, object> map)
    {
        return map.TryGetValue("request", out var request) 
               && request is not null 
               && !string.IsNullOrWhiteSpace(((SignOutDto)request).RefreshToken);
    }

    internal static bool ValidateRefresh(IDictionary<string, object> map)
    {
        return map.TryGetValue("request", out var request)
            && request is not null
            && !string.IsNullOrWhiteSpace(((RefreshTokenRequest)request).RefreshToken);
    }
}
