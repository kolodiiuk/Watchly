using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IRefreshTokenService
{
    Task<Result<RefreshToken>> GetRefreshTokenByValueAsync(string token, CancellationToken ct);

    Task<Result> AddRefreshTokenWithRevocationAsync(RefreshToken newRefreshToken, RefreshToken oldRefreshToken,
        string ipAddress, CancellationToken ct);

    Task<Result> RevokeRefreshTokenByValueAsync(RefreshToken rt, CancellationToken ct);

    Task<Result> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct);

    Task<Result> RevokeTokenFamilyAsync(Guid userId, DateTime revoked, string revokedByIp, CancellationToken ct);
}
