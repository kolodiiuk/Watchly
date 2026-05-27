using Watchly.Application.Models.Comments;
using Watchly.Domain.Utils;

namespace Watchly.Application.Interfaces;

public interface IAssistantService
{
    Task<Result<IEnumerable<CommentHighlightDto>>> GetRelevantCommentInfoAsync(int id, bool isTitle, RelevantCommentRequest request, Guid userId, CancellationToken ct);

}

