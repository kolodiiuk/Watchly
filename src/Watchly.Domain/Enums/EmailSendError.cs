namespace Watchly.Domain.Enums;

public enum EmailSendError
{
    Success = 0,

    SmtpCommandError = 1,

    SmtpProtocolError = 2,

    AuthenticationError = 3,

    IOError = 4,

    UnexpectedError = 5
}
