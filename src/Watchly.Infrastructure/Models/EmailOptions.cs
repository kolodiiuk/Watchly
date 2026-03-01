namespace Watchly.Infrastructure.Models;

public sealed class EmailOptions
{
    public string Host { get; init; } = default!;

    public int Port { get; init; }
    
    public bool UseSsl { get; init; }
    
    public string Username { get; init; } = default!;
    
    public string Password { get; init; } = default!;
    
    public string FromAddress { get; init; } = default!;
    
    public string FromName { get; init; } = default!;
}
