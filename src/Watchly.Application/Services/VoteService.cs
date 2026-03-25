using Watchly.Application.Interfaces;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public sealed class VoteService : IVoteService
{
    private WatchlyDbContext _dbContext;

    public VoteService(WatchlyDbContext context)
    {
        _dbContext = context;
    }

    public async Task<Result> VoteTitleAsync(int titleId, VoteDto voteDto, Guid result, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> ChangeVoteTitleAsync(int titleId, ChangeVoteDto changeVoteDto, Guid result, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> VoteEpisodeAsync(int episodeId, VoteDto voteDto, Guid result, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> ChangeVoteEpisodeAsync(int episodeId, ChangeVoteDto changeVoteDto, Guid result, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
