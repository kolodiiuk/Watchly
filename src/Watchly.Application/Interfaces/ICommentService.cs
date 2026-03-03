using Watchly.Application.Models;
using Watchly.Domain.Entities;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface ICommentService
{
    Task<Result<IEnumerable<CommentDto>>> GetCommentsAsync(int id, bool isTitle, CancellationToken ct);

    Task<Result> LeaveCommentAsync(LeaveCommentRequest request, Guid userId, CancellationToken ct);

    Task<Result> UpdateCommentAsync(int commentId, string text, Guid userId, CancellationToken ct);

    Task<Result> DeleteCommentAsync(int commentId, Guid userId, CancellationToken ct);
}

