using Watchly.Application.Models.UserProfile;

namespace Watchly.Application.Models.Comments;

public sealed record RelevantCommentRequest(
    string Topic,
    bool ExcludeOwnComments
    );

