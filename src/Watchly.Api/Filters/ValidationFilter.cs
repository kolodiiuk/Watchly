using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Watchly.Api.Validators;

namespace Watchly.Api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private static readonly Dictionary<string, Func<IDictionary<string, object>, bool>> _handlersMap = new()
    {
        [CatalogValidator.GetTitle] = CatalogValidator.ValidateGetTitle,
        [CatalogValidator.GetEpisode] = CatalogValidator.ValidateGetEpisode,
        [CatalogValidator.Search] = CatalogValidator.ValidateSearch,
        [CatalogValidator.GetKeywordSuggestions] = CatalogValidator.ValidateGetKeywordSuggestions,

        [AuthValidator.SignUp] = AuthValidator.ValidateSignUp,
        [AuthValidator.SignIn] = AuthValidator.ValidateSignIn,
        [AuthValidator.SignOut] = AuthValidator.ValidateSignOut,
        [AuthValidator.Refresh] = AuthValidator.ValidateRefresh,

        [CommentValidator.GetCommentsTitle] = CommentValidator.ValidateGetCommentsTitle,
        [CommentValidator.GetCommentsEpisode] = CommentValidator.ValidateGetCommentsEpisode,
        [CommentValidator.LeaveComment] = CommentValidator.ValidateLeaveComment,
        [CommentValidator.UpdateComment] = CommentValidator.ValidateUpdateComment,
        [CommentValidator.DeleteComment] = CommentValidator.ValidateDeleteComment,

        [VoteValidator.VoteTitle] = VoteValidator.ValidateVoteTitle,
        [VoteValidator.ChangeVoteTitle] = VoteValidator.ValidateChangeVoteTitle,
        [VoteValidator.VoteEpisode] = VoteValidator.ValidateVoteEpisode,
        [VoteValidator.ChangeVoteEpisode] = VoteValidator.ValidateChangeVoteEpisode,

        [UserProfileValidator.ChangeUsername] = UserProfileValidator.ValidateChangeUsername,
        [UserProfileValidator.ChangePassword] = UserProfileValidator.ValidateChangePassword,
        [UserProfileValidator.ForgetPassword] = UserProfileValidator.ValidateForgetPassword,
        [UserProfileValidator.ResetPassword] = UserProfileValidator.ValidateResetPassword,
        [UserProfileValidator.UpdatePictureProfile] = UserProfileValidator.ValidateUpdatePictureProfile
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var displayName = context.HttpContext.GetEndpoint()?.DisplayName;
        _handlersMap.TryGetValue(displayName ?? "", out var handler);
        var isValid = handler.Invoke(context.ActionArguments);
        if (isValid)
        {
            await next.Invoke();
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Validation error",
            Status = 400,
            Detail = "Parameters are not valid",
            Instance = context.HttpContext.Request.Path
        };
        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = 400
        };
    }
}
