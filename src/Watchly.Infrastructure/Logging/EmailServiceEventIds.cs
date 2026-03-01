using Microsoft.Extensions.Logging;

namespace Watchly.Infrastructure.Logging;

internal static class EmailServiceEventIds
{
    internal static readonly EventId SendFailGracefulDisconnect = new(7000, nameof(SendFailGracefulDisconnect));
    
    internal static readonly EventId SendFailUnexpectedError = new(7001, nameof(SendFailUnexpectedError));
    
    internal static readonly EventId SendFailIOError = new(7002, nameof(SendFailIOError));
    
    internal static readonly EventId SendFailAuthenticationError = new(7003, nameof(SendFailAuthenticationError));
    
    internal static readonly EventId SendFailSmtpCommandError = new(7004, nameof(SendFailSmtpCommandError));
}
