using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Watchly.Api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private const string CatalogGetTitle = "Watchly.Api.Controllers.CatalogController.GetTitleAsync (Watchly.Api)";
    
    private readonly Dictionary<string, Func<IDictionary<string, object>, bool>> _handlersMap = new()
    {
        [CatalogGetTitle] = (map) => map.TryGetValue("titleId", out var r) && (int)r >= 1
    };
    
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var displayName = context.HttpContext.GetEndpoint()?.DisplayName;
        _handlersMap.TryGetValue(displayName ?? "", out var handler);
        var isValid = handler.Invoke(context.ActionArguments);
        if (isValid)
        {
            next.Invoke();
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
