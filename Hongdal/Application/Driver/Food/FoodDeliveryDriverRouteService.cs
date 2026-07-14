using Hongdal.Contracts.Driver.Food;
using 홍달.Services.External.Naver;

namespace Hongdal.Application.Driver.Food;

public interface IFoodDeliveryDriverRouteService
{
    Task<FoodDeliveryDriverRouteResponseDto> GetRouteAsync(
        FoodDeliveryDriverRouteRequestDto request,
        CancellationToken cancellationToken);
}

public sealed class FoodDeliveryDriverRouteService : IFoodDeliveryDriverRouteService
{
    private readonly INaverCloudDirectionsService _directions;

    public FoodDeliveryDriverRouteService(INaverCloudDirectionsService directions)
    {
        _directions = directions;
    }

    public async Task<FoodDeliveryDriverRouteResponseDto> GetRouteAsync(
        FoodDeliveryDriverRouteRequestDto request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var stops = request.Stops
            .Where(x => IsCoordinateValid(x.Latitude, x.Longitude))
            .Take(6)
            .ToArray();
        if (!IsCoordinateValid(request.StartLatitude, request.StartLongitude) || stops.Length == 0)
        {
            throw new ArgumentException("현재 위치와 한 개 이상의 유효한 목적지가 필요합니다.", nameof(request));
        }

        var goal = stops[^1];
        var waypoints = stops
            .Take(stops.Length - 1)
            .Select(x => new NaverCloudRouteWaypoint(x.Latitude, x.Longitude))
            .ToArray();
        var route = await _directions.GetDrivingRouteAsync(
            request.StartLatitude,
            request.StartLongitude,
            goal.Latitude,
            goal.Longitude,
            waypoints,
            cancellationToken: cancellationToken);

        if (route is not null)
        {
            var routePoints = route.Path.Count > 1
                ? route.Path.Select(x => new FoodDeliveryDriverRoutePointDto
                {
                    Latitude = x.Latitude,
                    Longitude = x.Longitude
                }).ToArray()
                : BuildFallbackPoints(request, stops);
            return new FoodDeliveryDriverRouteResponseDto
            {
                Source = "NaverDirections5",
                IsEstimated = route.Path.Count <= 1,
                DistanceKm = Math.Round(
                    route.DistanceKm.HasValue
                        ? (decimal)route.DistanceKm.Value
                        : CalculateDistanceKm(request, stops),
                    1),
                DurationMinutes = Math.Max(
                    1,
                    (int)Math.Ceiling(
                        route.Duration?.TotalMinutes
                        ?? (double)CalculateDurationMinutes(request, stops))),
                Points = routePoints
            };
        }

        var distanceKm = CalculateDistanceKm(request, stops);
        return new FoodDeliveryDriverRouteResponseDto
        {
            Source = "CoordinateEstimate",
            IsEstimated = true,
            DistanceKm = Math.Round(distanceKm, 1),
            DurationMinutes = Math.Max(1, (int)Math.Ceiling(distanceKm / 22m * 60m)),
            Points = BuildFallbackPoints(request, stops)
        };
    }

    private static FoodDeliveryDriverRoutePointDto[] BuildFallbackPoints(
        FoodDeliveryDriverRouteRequestDto request,
        IReadOnlyList<FoodDeliveryDriverRouteStopDto> stops)
        => new[]
        {
            new FoodDeliveryDriverRoutePointDto
            {
                Latitude = request.StartLatitude,
                Longitude = request.StartLongitude
            }
        }.Concat(stops.Select(x => new FoodDeliveryDriverRoutePointDto
        {
            Latitude = x.Latitude,
            Longitude = x.Longitude
        })).ToArray();

    private static decimal CalculateDistanceKm(
        FoodDeliveryDriverRouteRequestDto request,
        IReadOnlyList<FoodDeliveryDriverRouteStopDto> stops)
    {
        var distance = 0m;
        var latitude = request.StartLatitude;
        var longitude = request.StartLongitude;
        foreach (var stop in stops)
        {
            distance += HaversineKm(latitude, longitude, stop.Latitude, stop.Longitude);
            latitude = stop.Latitude;
            longitude = stop.Longitude;
        }

        return distance;
    }

    private static decimal CalculateDurationMinutes(
        FoodDeliveryDriverRouteRequestDto request,
        IReadOnlyList<FoodDeliveryDriverRouteStopDto> stops)
        => CalculateDistanceKm(request, stops) / 22m * 60m;

    private static decimal HaversineKm(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
    {
        var latitude1 = DegreesToRadians((double)lat1);
        var latitude2 = DegreesToRadians((double)lat2);
        var latitudeDelta = latitude2 - latitude1;
        var longitudeDelta = DegreesToRadians((double)(lon2 - lon1));
        var a = Math.Pow(Math.Sin(latitudeDelta / 2d), 2d)
                + Math.Cos(latitude1) * Math.Cos(latitude2) * Math.Pow(Math.Sin(longitudeDelta / 2d), 2d);
        return (decimal)(6371d * 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a)));
    }

    private static bool IsCoordinateValid(decimal latitude, decimal longitude)
        => latitude is >= -90m and <= 90m && longitude is >= -180m and <= 180m
           && latitude != 0m && longitude != 0m;

    private static double DegreesToRadians(double value) => value * Math.PI / 180d;
}
