namespace Watchly.Application.Models;

public record LeaveCommentRequest(int EpisodeId, string Text, bool IsTitle);
