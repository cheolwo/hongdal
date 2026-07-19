using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using 살뜰.Services.Dispatch.Recommendation;
using 살뜰.Services.External.Naver;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Dispatch.Recommendation;

public class 배차추천경로ServiceTests
{
    [Fact]
    public async Task EstimateRouteAsync_네이버응답이없으면_fallback_경로를_반환한다()
    {
        var service = CreateService((_, _, _, _, _, _) => Task.FromResult<NaverCloudDrivingRoute?>(null));

        var route = await service.EstimateRouteAsync(new 배차경로좌표(37.5m, 127.0m), new 배차경로좌표(37.6m, 127.1m));

        Assert.NotNull(route);
        Assert.True(route!.DistanceKm > 0m);
        Assert.True(route.Duration > TimeSpan.Zero);
        Assert.Null(route.TollFare);
        Assert.False(route.실제경로여부);
        Assert.Equal("좌표기반도로보정", route.계산방식);
    }

    [Fact]
    public async Task EstimateRouteAsync_네이버호출이실패해도_fallback_경로를_반환한다()
    {
        var service = CreateService((_, _, _, _, _, _) => throw new HttpRequestException("naver failed"));

        var route = await service.EstimateRouteAsync(new 배차경로좌표(37.5m, 127.0m), new 배차경로좌표(37.6m, 127.1m));

        Assert.NotNull(route);
        Assert.True(route!.DistanceKm > 0m);
        Assert.True(route.Duration > TimeSpan.Zero);
    }

    [Fact]
    public async Task EstimateRouteAsync_같은좌표는_요청스코프안에서_한번만_호출한다()
    {
        var callCount = 0;
        var service = CreateService((_, _, _, _, _, _) =>
        {
            callCount++;
            return Task.FromResult<NaverCloudDrivingRoute?>(new NaverCloudDrivingRoute
            {
                DistanceMeters = 12000m,
                DurationMilliseconds = 600000m,
                TollFare = 0m
            });
        });
        var origin = new 배차경로좌표(37.5m, 127.0m);
        var destination = new 배차경로좌표(37.6m, 127.1m);

        var first = await service.EstimateRouteAsync(origin, destination);
        var second = await service.EstimateRouteAsync(origin, destination);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first!.Duration, second!.Duration);
        Assert.Equal(1, callCount);
        Assert.True(first.실제경로여부);
        Assert.Equal("Directions5", first.계산방식);
    }

    [Fact]
    public async Task EstimateOrderedRouteAsync_경유지_경로는_한번의_호출로_요약을_반환한다()
    {
        var callCount = 0;
        var service = CreateService((_, _, _, _, _, _) =>
        {
            callCount++;
            return Task.FromResult<NaverCloudDrivingRoute?>(new NaverCloudDrivingRoute
            {
                DistanceMeters = 3500m,
                DurationMilliseconds = 900000m,
                TollFare = 0m
            });
        });

        var route = await service.EstimateOrderedRouteAsync(
            new 배차경로좌표(37.5m, 127.0m),
            [
                new 배차경로좌표(37.51m, 127.01m),
                new 배차경로좌표(37.52m, 127.02m),
                new 배차경로좌표(37.53m, 127.03m)
            ]);

        Assert.NotNull(route);
        Assert.Equal(3.5m, route!.DistanceKm);
        Assert.Equal(TimeSpan.FromMinutes(15), route.Duration);
        Assert.Equal(1, callCount);
        Assert.True(route.실제경로여부);
        Assert.Equal("Directions5", route.계산방식);
    }

    private static 배차추천경로Service CreateService(
        Func<decimal, decimal, decimal, decimal, string?, CancellationToken, Task<NaverCloudDrivingRoute?>> routeFunc)
    {
        return new 배차추천경로Service(
            null!,
            null!,
            null!,
            new FakeNaverCloudDirectionsService(routeFunc),
            Options.Create(new NaverCloudDirectionsOptions
            {
                EnableFallbackRouteEstimate = true,
                FallbackDistanceMultiplier = 1.25m,
                FallbackAverageSpeedKmH = 45m
            }),
            NullLogger<배차추천경로Service>.Instance);
    }

    private sealed class FakeNaverCloudDirectionsService : INaverCloudDirectionsService
    {
        private readonly Func<decimal, decimal, decimal, decimal, string?, CancellationToken, Task<NaverCloudDrivingRoute?>> _routeFunc;

        public FakeNaverCloudDirectionsService(
            Func<decimal, decimal, decimal, decimal, string?, CancellationToken, Task<NaverCloudDrivingRoute?>> routeFunc)
        {
            _routeFunc = routeFunc;
        }

        public Task<NaverCloudDrivingRoute?> GetDrivingRouteAsync(
            decimal startLat,
            decimal startLng,
            decimal goalLat,
            decimal goalLng,
            string? option = null,
            CancellationToken cancellationToken = default)
        {
            return _routeFunc(startLat, startLng, goalLat, goalLng, option, cancellationToken);
        }

        public Task<NaverCloudDrivingRoute?> GetDrivingRouteAsync(
            decimal startLat,
            decimal startLng,
            decimal goalLat,
            decimal goalLng,
            IReadOnlyList<NaverCloudRouteWaypoint> waypoints,
            string? option = null,
            CancellationToken cancellationToken = default)
        {
            return _routeFunc(startLat, startLng, goalLat, goalLng, option, cancellationToken);
        }
    }
}
