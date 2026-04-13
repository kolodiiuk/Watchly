namespace Watchly.Application.Models.Comments;

public record LeaveCommentRequest(int ContentId, string Text, bool IsTitle);
