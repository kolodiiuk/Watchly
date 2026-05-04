using Watchly.Infrastructure.Models;

namespace Watchly.Infrastructure.Interfaces;

public interface IEmailService
{
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct);
}