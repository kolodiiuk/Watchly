namespace Watchly.Application.Models;

public record LeaveCommentRequest(int ContentId, string Text, bool IsTitle);
