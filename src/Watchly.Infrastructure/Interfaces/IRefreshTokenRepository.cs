using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Infrastructure.Interfaces;

public interface IRefreshTokenRepository
{
    Task<Result<RefreshToken>> GetRefreshTokenByValueAsync(string token, CancellationToken ct);

    Task<Result> AddRefreshTokenWithRevocationAsync(
        RefreshToken newRefreshToken, RefreshToken oldRefreshToken, CancellationToken ct);

    Task<Result> RevokeRefreshTokenByValueAsync(RefreshToken rt, CancellationToken ct);

    Task<Result> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct);

    Task<Result> RevokeTokenFamilyAsync(Guid userId, DateTime revoked, string revokedByIp, CancellationToken ct);
}

public class RefreshTokenRepoStub : IRefreshTokenRepository
{
    public async Task<Result<RefreshToken>> GetRefreshTokenByValueAsync(string token, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> AddRefreshTokenWithRevocationAsync(RefreshToken newRefreshToken, RefreshToken oldRefreshToken,
        CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> RevokeRefreshTokenByValueAsync(RefreshToken rt, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> RevokeTokenFamilyAsync(Guid userId, DateTime revoked, string revokedByIp, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}