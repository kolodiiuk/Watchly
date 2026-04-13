using Watchly.Application.Models.Auth;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IAuthService
{
    Task<Result> SignUpAsync(string email, string password);

    Task<Result<SignInResponse>> SignInAsync(string email, string password, string ipAddress, CancellationToken ct);

    Task<Result<RefreshTokenResponse>> RefreshTokenAsync(string token, string ipAddress, CancellationToken ct);

    Task<Result> SignOutAsync(string token, string ipAddress, CancellationToken ct);
}
