using Microsoft.AspNetCore.Mvc;

namespace Watchly.Api.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid();

            _logger.LogError(ex, $"{errorId} : {ex.Message}");

            httpContext.Response.StatusCode = 500;

            var requestPath = httpContext.Request.Path.ToString();
            var isFileRequest = requestPath.Contains("/files", StringComparison.OrdinalIgnoreCase) ||
                                 requestPath.EndsWith(".pdf") ||
                                 requestPath.EndsWith(".zip") ||
                                 requestPath.EndsWith(".csv") ||
                                 requestPath.EndsWith(".png") ||
                                 requestPath.EndsWith(".jpg");

            if (isFileRequest)
            {
                httpContext.Response.ContentType = "text/plain";
                await httpContext.Response.WriteAsync("Error: Unable to process file request.");
            }

            var problem = new ProblemDetails
            {
                Title = "Unhandled Exception",
                Detail = $"Unhandled Error: {ex.Message}{Environment.NewLine}{ex.Source}{Environment.NewLine}{ex.StackTrace}",
                Instance = requestPath,
                Status = 500
            };

            await httpContext.Response.WriteAsJsonAsync(problem);
        }
    }
}
