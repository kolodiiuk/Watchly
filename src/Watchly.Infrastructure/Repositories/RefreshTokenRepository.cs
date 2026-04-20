using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;
using Watchly.Infrastructure.Interfaces;

namespace Watchly.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : LoggingService<RefreshTokenRepository>, IRefreshTokenRepository
{
    private readonly WatchlyDbContext _dbContext;

    public RefreshTokenRepository(WatchlyDbContext dbContext, ILogger<RefreshTokenRepository> logger) : base(logger)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<RefreshToken>> GetRefreshTokenByValueAsync(string token, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            var rt = await _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token, ct);

            return rt switch
            {
                null => Result<RefreshToken>.Fail("No such token"),
                _ => Result<RefreshToken>.Success(rt)
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<RefreshToken>.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<RefreshToken>.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> AddRefreshTokenWithRevocationAsync(RefreshToken newRefreshToken,
        RefreshToken oldRefreshToken,
        string ipAddress,
        CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            oldRefreshToken.ReplacedByToken = newRefreshToken.Token;
            oldRefreshToken.Revoked = DateTime.UtcNow;
            oldRefreshToken.RevokedByIp = ipAddress;
            await _dbContext.RefreshTokens.AddAsync(newRefreshToken, ct);
            _dbContext.RefreshTokens.Update(oldRefreshToken);
            var rows = await _dbContext.SaveChangesAsync(ct);

            return rows switch
            {
                0 => Result.Fail("No rows affected"),
                _ => Result.Success()
            };
        }
        catch (OperationCanceledException e) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            await _dbContext.RefreshTokens.AddAsync(refreshToken, ct);
            var rows = await _dbContext.SaveChangesAsync(ct);

            return rows switch
            {
                0 => Result.Fail("No rows inserted"),
                _ => Result.Success()
            };
        }
        catch (OperationCanceledException e) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> RevokeRefreshTokenByValueAsync(RefreshToken rt, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            _dbContext.RefreshTokens.Update(rt);
            var rows = await _dbContext.SaveChangesAsync(ct);

            return rows switch
            {
                0 => Result.Fail("No rows affected"),
                _ => Result.Success()
            };
        }
        catch (OperationCanceledException e) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }

    public async Task<Result> RevokeTokenFamilyAsync(Guid userId, DateTime revoked, string revokedByIp,
        CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            var rows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                       update "refresh_tokens"
                       set "revoked" = {revoked},
                           "revoked_by_ip" = {revokedByIp},
                           "replaced_by_token" = NULL
                       where "user_id" = {userId}
                 """, ct);

            return rows switch
            {
                0 => Result.Fail("No rows affected"),
                _ => Result.Success()
            };
        }
        catch (OperationCanceledException e) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB error: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"Error: {e.Message}");
        }
    }
}
