using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Watchly.Api.Controllers;
using Watchly.Application.Interfaces;
using Watchly.Application.Models.Stats;
using Watchly.Domain.Utils;

namespace Watchly.Tests.UnitTests;

public class UserStatsControllerTests
{
    [Fact]
    public async Task GetMovieStatsAsync_WhenUserIsAuthorized_ReturnsStatsPayload()
    {
        var userId = Guid.NewGuid();
        var expected = new MovieStatsResponse
        {
            MovieCount = 4,
            HoursWatched = 10,
            DaysWatched = 1,
            MonthsWatched = 0,
            TopGenres = ["Drama", "Sci-Fi"]
        };

        var service = new Mock<IUserStatsService>();
        service
            .Setup(s => s.GetUserMovieStatsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<MovieStatsResponse>.Success(expected));

        var controller = CreateController(service.Object, userId);

        var result = await controller.GetMovieStatsAsync(CancellationToken.None);

        var ok = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        var payload = Assert.IsType<MovieStatsResponse>(ok.Value);
        Assert.Equal(expected.MovieCount, payload.MovieCount);
        Assert.Equal(expected.HoursWatched, payload.HoursWatched);
        Assert.Equal(expected.DaysWatched, payload.DaysWatched);
        Assert.Equal(expected.MonthsWatched, payload.MonthsWatched);
        Assert.Equal(expected.TopGenres, payload.TopGenres);
    }

    [Fact]
    public async Task GetMovieStatsAsync_WhenUserIsMissing_ReturnsUnauthorized()
    {
        var service = new Mock<IUserStatsService>();
        var controller = CreateController(service.Object, null);

        var result = await controller.GetMovieStatsAsync(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
        service.Verify(s => s.GetUserMovieStatsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSeriesStatsAsync_WhenUserIsAuthorized_ReturnsStatsPayload()
    {
        var userId = Guid.NewGuid();
        var expected = new SeriesStatsResponse
        {
            TvSeriesCount = 2,
            EpisodesCount = 24,
            HoursWatched = 16,
            DaysWatched = 0,
            MonthsWatched = 0,
            TopGenres = ["Action", "Mystery"]
        };

        var service = new Mock<IUserStatsService>();
        service
            .Setup(s => s.GetUserTvSeriesStatsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SeriesStatsResponse>.Success(expected));

        var controller = CreateController(service.Object, userId);

        var result = await controller.GetSeriesStatsAsync(CancellationToken.None);

        var ok = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, ok.StatusCode);
        var payload = Assert.IsType<SeriesStatsResponse>(ok.Value);
        Assert.Equal(expected.TvSeriesCount, payload.TvSeriesCount);
        Assert.Equal(expected.EpisodesCount, payload.EpisodesCount);
        Assert.Equal(expected.HoursWatched, payload.HoursWatched);
        Assert.Equal(expected.DaysWatched, payload.DaysWatched);
        Assert.Equal(expected.MonthsWatched, payload.MonthsWatched);
        Assert.Equal(expected.TopGenres, payload.TopGenres);
    }

    [Fact]
    public async Task GetSeriesStatsAsync_WhenUserIsMissing_ReturnsUnauthorized()
    {
        var service = new Mock<IUserStatsService>();
        var controller = CreateController(service.Object, null);

        var result = await controller.GetSeriesStatsAsync(CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(result.Result);
        service.Verify(s => s.GetUserTvSeriesStatsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static UserStatsController CreateController(IUserStatsService service, Guid? userId)
    {
        var logger = new Mock<ILogger<UserStatsController>>();
        var controller = new UserStatsController(service, logger.Object);

        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"))
            }
        };

        return controller;
    }
}
