using System.Security.Authentication;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Watchly.Domain.Enums;
using Watchly.Domain.Utils;
using Watchly.Infrastructure.Interfaces;
using Watchly.Infrastructure.Logging;
using Watchly.Infrastructure.Models;

namespace Watchly.Infrastructure.Services;

public sealed class EmailService : LoggingService<EmailService>, IEmailService
{
    private readonly EmailOptions _options;

    public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger) : base(logger)
    {
        _options = options.Value;
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        var mimeMessage = new MimeKit.MimeMessage();
        mimeMessage.From.Add(new MimeKit.MailboxAddress(_options.FromName, _options.FromAddress));
        mimeMessage.To.Add(MimeKit.MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new MimeKit.TextPart(message.IsHtml ? "html" : "plain") { Text = message.Body };

        ct.ThrowIfCancellationRequested();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_options.Host, _options.Port, _options.UseSsl, ct);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(_options.Username, _options.Password, ct);
            }

            await client.SendAsync(mimeMessage, ct);
        }
        catch (SmtpCommandException ex)
        {
            Log(LogLevel.Error, EmailServiceEventIds.SendFailSmtpCommandError,
                "SMTP command failed when sending email to {message.To} via {_options.Host}:{_options.Port}",
                message.To, _options.Host, _options.Port);

            var res = Result.Fail(
                $"SMTP command failed when sending email to {message.To} via {_options.Host}:{_options.Port}");

            return new EmailSendResult { Result = res, Error = EmailSendError.SmtpCommandError };
        }
        catch (SmtpProtocolException ex)
        {
            Log(LogLevel.Error, EmailServiceEventIds.SendFailSmtpCommandError,
                "SMTP protocol error when sending email to {To} via {Host}:{Port}",
                message.To, _options.Host, _options.Port);

            var res = Result.Fail("SMTP protocol error while sending email.");

            return new EmailSendResult { Result = res, Error = EmailSendError.SmtpProtocolError };
        }
        catch (AuthenticationException ex)
        {
            Log(LogLevel.Error, EmailServiceEventIds.SendFailAuthenticationError,
                "SMTP authentication failed for host {Host}",
                _options.Host);

            var res = Result.Fail("SMTP authentication failed.");

            return new EmailSendResult { Result = res, Error = EmailSendError.AuthenticationError };
        }
        catch (IOException ex)
        {
            Log(LogLevel.Error, EmailServiceEventIds.SendFailIOError,
                "I/O error while sending email to {To}",
                message.To);

            var res = Result.Fail("I/O error while sending email.");

            return new EmailSendResult { Result = res, Error = EmailSendError.IOError };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, EmailServiceEventIds.SendFailUnexpectedError,
                "Unexpected error while sending email to {To}", message.To);

            var res = Result.Fail("Unexpected error while sending email.");

            return new EmailSendResult { Result = res, Error = EmailSendError.UnexpectedError };
        }
        finally
        {
            if (client.IsConnected)
            {
                try
                {
                    await client.DisconnectAsync(true, ct);
                }
                catch (Exception ex)
                {
                    Log(LogLevel.Warning, EmailServiceEventIds.SendFailGracefulDisconnect,
                        "Failed to disconnect SMTP client cleanly.");
                }
            }
        }

        return new EmailSendResult { Result = Result.Success() };
    }
}
