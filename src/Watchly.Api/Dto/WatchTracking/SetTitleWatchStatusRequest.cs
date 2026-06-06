using Watchly.Domain.Enums;

namespace Watchly.Api.Dto.WatchTracking;

public class SetTitleWatchStatusRequest
{
    public WatchStatus Status { get; set; }
}
