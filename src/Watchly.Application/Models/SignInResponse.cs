namespace Watchly.Application.Models;

public class SignInResponse
{
    public string Token { get; set; }

    public string RefreshToken { get; set; }

    public DateTime Expiration { get; set; }

    public UserDto User { get; set; }
}
