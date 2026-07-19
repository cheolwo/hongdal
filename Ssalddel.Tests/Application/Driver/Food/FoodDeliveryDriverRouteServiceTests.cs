using Ssalddel.Application.Driver.Food;
using Ssalddel.Contracts.Driver.Food;
using 살뜰.Services.External.Naver;

namespace Ssalddel.Tests.Application.Driver.Food;

public sealed class FoodDeliveryDriverRouteServiceTests
{
    [Fact]
    public async Task 네이버경로가_있으면_실제_도로좌표와_거리시간을_반환한다()
    {
        var service = new FoodDeliveryDriverRouteService(new FakeDirectionsService(
            new NaverCloudDrivingRoute
            {
                DistanceMeters = 3200m,
                DurationMilliseconds = 720000m,
                Path =
                [
                    new NaverCloudRoutePathPoint(37.50m, 127.00m),
                    new NaverCloudRoutePathPoint(37.51m, 127.01m),
                    new NaverCloudRoutePathPoint(37.52m, 127.02m)
                ]
            }));

        var result = await service.GetRouteAsync(CreateRequest(), CancellationToken.None);

        Assert.False(result.IsEstimated);
        Assert.Equal("NaverDirections5", result.Source);
        Assert.Equal(3.2m, result.DistanceKm);
        Assert.Equal(12, result.DurationMinutes);
        Assert.Equal(3, result.Points.Count);
    }

    [Fact]
    public async Task 네이버키가_없으면_경유지를_보존한_추정경로를_반환한다()
    {
        var service = new FoodDeliveryDriverRouteService(new FakeDirectionsService(null));

        var result = await service.GetRouteAsync(CreateRequest(), CancellationToken.None);

        Assert.True(result.IsEstimated);
        Assert.Equal("CoordinateEstimate", result.Source);
        Assert.True(result.DistanceKm > 0m);
        Assert.Equal(3, result.Points.Count);
    }

    private static FoodDeliveryDriverRouteRequestDto CreateRequest()
        => new()
        {
            StartLatitude = 37.50m,
            StartLongitude = 127.00m,
            Stops =
            [
                new FoodDeliveryDriverRouteStopDto
                {
                    Label = "음식점",
                    Latitude = 37.51m,
                    Longitude = 127.01m
                },
                new FoodDeliveryDriverRouteStopDto
                {
                    Label = "고객",
                    Latitude = 37.52m,
                    Longitude = 127.02m
                }
            ]
        };

    private sealed class FakeDirectionsService(NaverCloudDrivingRoute? result) : INaverCloudDirectionsService
    {
        public Task<NaverCloudDrivingRoute?> GetDrivingRouteAsync(
            decimal startLat,
            decimal startLng,
            decimal goalLat,
            decimal goalLng,
            string? option = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);

        public Task<NaverCloudDrivingRoute?> GetDrivingRouteAsync(
            decimal startLat,
            decimal startLng,
            decimal goalLat,
            decimal goalLng,
            IReadOnlyList<NaverCloudRouteWaypoint> waypoints,
            string? option = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
