namespace Watchly.Infrastructure.Models;

public sealed record EmailMessage
{
    public string To { get; init; } = null;

    public string Subject { get; init; } = null;
    
    public string Body { get; init; } = null;
    
    public bool IsHtml { get; init; }
}
