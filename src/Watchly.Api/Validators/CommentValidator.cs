using Watchly.Api.Dto.Comments;
using Watchly.Application.Models.Comments;

namespace Watchly.Api.Validators;

internal static class CommentValidator
{
    internal const string GetCommentsTitle = "Watchly.Api.Controllers.CommentController.GetCommentsTitleAsync (Watchly.Api)";
    internal const string GetCommentsEpisode = "Watchly.Api.Controllers.CommentController.GetCommentsEpisodeAsync (Watchly.Api)";
    internal const string LeaveComment = "Watchly.Api.Controllers.CommentController.LeaveCommentAsync (Watchly.Api)";
    internal const string UpdateComment = "Watchly.Api.Controllers.CommentController.UpdateCommentAsync (Watchly.Api)";
    internal const string DeleteComment = "Watchly.Api.Controllers.CommentController.DeleteCommentAsync (Watchly.Api)";

    private const string Request = "req";
    private const string CommentId = "commentId";
    private const string EpisodeId = "episodeId";
    private const string TitleId = "titleId";

    internal static bool ValidateGetCommentsTitle(IDictionary<string, object> map)
    {
        return map.TryGetValue(TitleId, out var titleId) && (int)titleId >= 1;
    }

    internal static bool ValidateGetCommentsEpisode(IDictionary<string, object> map)
    {
        return map.TryGetValue(EpisodeId, out var episodeId) && (int)episodeId >= 1;
    }

    internal static bool ValidateLeaveComment(IDictionary<string, object> map)
    {
        return map.TryGetValue(Request, out var request)
               && request is LeaveCommentRequest leaveCommentRequest
               && !string.IsNullOrWhiteSpace(leaveCommentRequest.Text)
               && leaveCommentRequest.ContentId >= 1;
    }

    internal static bool ValidateUpdateComment(IDictionary<string, object> map)
    {
        return map.TryGetValue(Request, out var request)
               && request is UpdateCommentRequest updateCommentRequest
               && !string.IsNullOrWhiteSpace(updateCommentRequest.Text)
               && updateCommentRequest.CommentId >= 1;
    }

    internal static bool ValidateDeleteComment(IDictionary<string, object> map)
    {
        return map.TryGetValue(CommentId, out var commentId)
               && commentId is int parsedCommentId
               && parsedCommentId >= 1;
    }
}
