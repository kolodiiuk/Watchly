using System.Security.Claims;
using Watchly.Domain.Entities;

namespace Watchly.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles);

    string GenerateRefreshToken();

    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
