namespace Watchly.Application.Models;

public sealed record CommentDto(
    int Id,
    int ContentId,
    bool IsTitle,
    Guid UserId,
    DateTime UpdatedAt,
    string Text,
    UserDto User);

