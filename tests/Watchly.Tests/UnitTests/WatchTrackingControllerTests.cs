using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Watchly.Api.Controllers;
using Watchly.Application.Interfaces;
using Watchly.Domain.Utils;

namespace Watchly.Tests.UnitTests;

public class WatchTrackingControllerTests
{
    [Fact]
    public async Task GetEpisodeWatchCountAsync_WhenServiceReturnsCount_ReturnsOkWithCount()
    {
        var userId = Guid.NewGuid();
        const int episodeId = 42;
        const int expectedCount = 3;

        var service = new Mock<IWatchTrackingService>();
        service
            .Setup(s => s.GetWatchCountInfoEpisodeAsync(episodeId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(expectedCount));

        var logger = new Mock<ILogger<WatchTrackingController>>();
        var controller = new WatchTrackingController(service.Object, logger.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
                        "test"))
                }
            }
        };

        var result = await controller.GetEpisodeWatchCountAsync(episodeId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(expectedCount, ok.Value);
    }
}
