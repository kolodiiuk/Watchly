using Watchly.Application.Interfaces;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Services;

public sealed class VoteService : IVoteService
{
    private WatchlyDbContext _dbContext;

    public VoteService(WatchlyDbContext context)
    {
        _dbContext = context;
    }
}
