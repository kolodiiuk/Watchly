using Watchly.Domain.Enums;
using Watchly.Domain.Utils;

namespace Watchly.Infrastructure.Interfaces;

public class EmailSendResult
{
    public Result Result { get; set; }

    public EmailSendError Error { get; set; }
}
