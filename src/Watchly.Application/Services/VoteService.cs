using Npgsql;
using Microsoft.EntityFrameworkCore;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Votes;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public sealed class VoteService : IVoteService
{
    private readonly WatchlyDbContext _dbContext;

    public VoteService(WatchlyDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Result> VoteTitleAsync(int titleId, short value, Guid userId, CancellationToken ct)
    {
        return await VoteAsync(titleId, value, userId, true, ct);
    }

    public async Task<Result> VoteEpisodeAsync(int episodeId, short value, Guid userId, CancellationToken ct)
    {
        return await VoteAsync(episodeId, value, userId, false, ct);
    }

    public async Task<Result> ChangeVoteAsync(int principalId, short value, Guid userId, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            var user = await _dbContext.Users.FindAsync([userId], ct);
            if (user is null)
            {
                return Result.Fail("No user");
            }

            var vote = await _dbContext.Votes.FindAsync([principalId], ct);
            if (vote is null)
            {
                return Result.Fail("No existing vote");
            }

            if (vote.UserId != userId)
            {
                return Result.Fail("Not user's vote");
            }

            vote.Value = value;
            vote.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"{e.Message}");
        }
    }

    public Task<Result<UserVote>> GetTitleVoteAsync(int titleId, Guid userId, CancellationToken ct)
    {
        return GetVoteAsync(titleId, userId, true, ct);
    }

    public Task<Result<UserVote>> GetEpisodeVoteAsync(int episodeId, Guid userId, CancellationToken ct)
    {
        return GetVoteAsync(episodeId, userId, false, ct);
    }

    private async Task<Result<UserVote>> GetVoteAsync(int principalId, Guid userId, bool isTitle, CancellationToken ct)
    {
        try
        {
            var vote = await _dbContext.Votes
                .AsNoTracking()
                .Where(v => v.UserId == userId)
                .Where(v => isTitle ? v.TitleId == principalId : v.EpisodeId == principalId)
                .OrderByDescending(v => v.UpdatedAt)
                .Select(v => new UserVote(v.Id, v.Value))
                .FirstOrDefaultAsync(ct);

            return Result<UserVote>.Success(vote);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result<UserVote>.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result<UserVote>.Fail(e.Message);
        }
    }

    private async Task<Result> VoteAsync(int principalId, short value, Guid userId, bool isTitle, CancellationToken ct)
    {
        try
        {
            ct.ThrowIfCancellationRequested();
            var user = await _dbContext.Users.FindAsync([userId], ct);
            if (user is null)
            {
                return Result.Fail("No user");
            }

            var vote = new Vote
            {
                UpdatedAt = DateTime.UtcNow,
                TitleId = isTitle ? principalId : null,
                EpisodeId = isTitle ? null : principalId,
                UserId = userId,
                Value = value
            };
            await _dbContext.AddAsync(vote, ct);
            await _dbContext.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (NpgsqlException e)
        {
            return Result.Fail($"DB problems: {e.Message}");
        }
        catch (Exception e)
        {
            return Result.Fail($"{e.Message}");
        }
    }
}
