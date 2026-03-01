using Watchly.Application.Models;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.DbContexts;

namespace Watchly.Application.Interfaces;

public sealed class CommentService : ICommentService
{
    private readonly WatchlyDbContext _dbContext;

    public CommentService(WatchlyDbContext context)
    {
        _dbContext = context;
    }
    
    public async Task<Result<IEnumerable<Comment>>> GetCommentsAsync(int id, bool isTitle, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> LeaveCommentAsync(LeaveCommentRequest request, Guid userId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> UpdateCommentAsync(int commentId, string text, Guid userId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> DeleteCommentAsync(int commentId, Guid userId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
