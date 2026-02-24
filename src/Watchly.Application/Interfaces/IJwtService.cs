using System.Security.Claims;
using Watchly.Domain.Entities;

namespace Watchly.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);

    string GenerateRefreshToken();

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
