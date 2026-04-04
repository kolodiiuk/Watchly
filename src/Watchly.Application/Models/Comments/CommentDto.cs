using Watchly.Application.Models.UserProfile;

namespace Watchly.Application.Models.Comments;

public sealed record CommentDto(
    int Id,
    int ContentId,
    bool IsTitle,
    Guid UserId,
    DateTime UpdatedAt,
    string Text,
    UserDto User);

