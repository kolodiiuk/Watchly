using Watchly.Domain.Enums;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.Models;
using Watchly.Infrastructure.Services;

namespace Watchly.Infrastructure.Interfaces;

public interface IEmailService
{
    Task<ResultV<EmailSendError>> SendAsync(EmailMessage message, CancellationToken ct);
}
