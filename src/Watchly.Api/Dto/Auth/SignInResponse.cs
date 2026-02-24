namespace Watchly.Api.Dto.Auth;

public class SignInResponse
{
    public string Token { get; set; }

    public string RefreshToken { get; set; }

    public DateTime Expiration { get; set; }

    public UserDto User { get; set; }
}
