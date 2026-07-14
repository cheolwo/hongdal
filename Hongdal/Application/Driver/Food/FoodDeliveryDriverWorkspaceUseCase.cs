using Hongdal.Contracts.Common.Drivers;
using Hongdal.Contracts.Driver.Food;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.Dispatch.Recommendation;
using 홍달.도메인.공통;

namespace Hongdal.Application.Driver.Food;

public interface IFoodDeliveryDriverWorkspaceUseCase
{
    Task<FoodDeliveryDriverWorkspaceDto> GetAsync(string driverId, CancellationToken cancellationToken);
}

public sealed class FoodDeliveryDriverWorkspaceUseCase : IFoodDeliveryDriverWorkspaceUseCase
{
    private const decimal MaxPickupSeparationKm = 1.5m;
    private const decimal MaxDropoffSeparationKm = 3m;
    private const decimal MaxBundleRouteKm = 6m;
    private static readonly TimeSpan MaxReadyTimeGap = TimeSpan.FromMinutes(12);

    private readonly HongdalContext _db;
    private readonly I음식배달기사업무Service _driverWork;
    private readonly I배달기사월정산UseCase _settlements;

    public FoodDeliveryDriverWorkspaceUseCase(
        HongdalContext db,
        I음식배달기사업무Service driverWork,
        I배달기사월정산UseCase settlements)
    {
        _db = db;
        _driverWork = driverWork;
        _settlements = settlements;
    }

    public async Task<FoodDeliveryDriverWorkspaceDto> GetAsync(
        string driverId,
        CancellationToken cancellationToken)
    {
        var workItems = await _driverWork.제안조회Async(driverId, cancellationToken);
        var offers = workItems
            .Where(x => x.Status == DriverWorkOfferStatus.Recommended)
            .Select(ToOffer)
            .ToArray();
        var activeWork = workItems
            .Where(x => x.Status is DriverWorkOfferStatus.Accepted
                or DriverWorkOfferStatus.MovingToPickup
                or DriverWorkOfferStatus.MovingToDropoff)
            .ToDictionary(x => x.OfferId, StringComparer.Ordinal);
        var activeIds = activeWork.Keys.ToArray();
        var activeQueues = activeIds.Length == 0
            ? []
            : await _db.운송원장
                .AsNoTracking()
                .Where(x => activeIds.Contains(x.의뢰Id)
                            && x.배차업무유형 == 상태값.배차업무유형.음식배달
                            && (x.기사_운송자 == driverId || x.확정기사Id == driverId))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        var active = activeQueues
            .Where(x => activeWork.ContainsKey(x.의뢰Id))
            .Select(x => ToActiveDelivery(x, activeWork[x.의뢰Id]))
            .ToArray();

        var settlementResult = await _settlements.당월조회Async(driverId, driverId, cancellationToken);
        var settlement = settlementResult.IsSuccess
            ? settlementResult.Value
            : new 배달기사월정산응답
            {
                기사Id = driverId,
                년도 = DateTime.UtcNow.Year,
                월 = DateTime.UtcNow.Month
            };

        return new FoodDeliveryDriverWorkspaceDto
        {
            DriverId = driverId,
            Recommendations = offers,
            ActiveDeliveries = active,
            BundleCandidates = BuildBundleCandidates(offers),
            Settlement = settlement,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private static FoodDeliveryDriverOfferDto ToOffer(DriverWorkOfferDto offer)
        => new()
        {
            OfferId = offer.OfferId,
            OrderSummary = offer.Title,
            RestaurantName = offer.Pickup.Label,
            Pickup = ToStop(offer.Pickup),
            Dropoff = ToStop(offer.Dropoff),
            DriverPayout = offer.DriverPayout,
            DistanceKm = offer.DistanceKm.HasValue ? (decimal)offer.DistanceKm.Value : null,
            RecommendationReason = offer.RecommendationReason,
            ExpiresAtUtc = offer.ExpiresAtUtc?.UtcDateTime
        };

    private static FoodDeliveryDriverActiveDeliveryDto ToActiveDelivery(
        홍달.도메인.운송.운송원장 transport,
        DriverWorkOfferDto offer)
        => new()
        {
            TransportId = transport.Id,
            OfferId = offer.OfferId,
            OrderSummary = offer.Title,
            RestaurantName = offer.Pickup.Label,
            Pickup = ToStop(offer.Pickup),
            Dropoff = ToStop(offer.Dropoff),
            DriverPayout = offer.DriverPayout,
            TransportStatus = transport.상태,
            WorkStatus = offer.Status,
            UpdatedAtUtc = transport.UpdatedAt
        };

    private static FoodDeliveryDriverStopDto ToStop(DriverWorkStopDto stop)
        => new()
        {
            Label = stop.Label,
            Address = stop.Address,
            Latitude = (decimal)stop.Latitude,
            Longitude = (decimal)stop.Longitude,
            TargetAtUtc = stop.TargetTime?.UtcDateTime
        };

    private static IReadOnlyList<FoodDeliveryBundleCandidateDto> BuildBundleCandidates(
        IReadOnlyList<FoodDeliveryDriverOfferDto> offers)
    {
        var remaining = offers
            .Where(HasCoordinates)
            .OrderBy(x => x.Pickup.TargetAtUtc ?? DateTime.MaxValue)
            .ToList();
        var result = new List<FoodDeliveryBundleCandidateDto>();

        while (remaining.Count > 1)
        {
            var first = remaining[0];
            remaining.RemoveAt(0);
            var second = remaining.FirstOrDefault(candidate => CanBundle(first, candidate));
            if (second is null)
            {
                continue;
            }

            remaining.Remove(second);
            var estimatedRouteKm = EstimateBundleRouteKm(first, second);
            result.Add(new FoodDeliveryBundleCandidateDto
            {
                BundleId = $"bundle:{first.OfferId}:{second.OfferId}",
                OfferIds = [first.OfferId, second.OfferId],
                Title = $"{first.RestaurantName} + {second.RestaurantName}",
                Reason = "조리 완료 시각과 픽업·전달 동선이 가까운 2건 묶음입니다.",
                TotalPayout = first.DriverPayout + second.DriverPayout,
                EstimatedRouteKm = Math.Round(estimatedRouteKm, 1)
            });
        }

        return result;
    }

    private static bool CanBundle(FoodDeliveryDriverOfferDto first, FoodDeliveryDriverOfferDto second)
    {
        var readyGap = first.Pickup.TargetAtUtc.HasValue && second.Pickup.TargetAtUtc.HasValue
            ? (first.Pickup.TargetAtUtc.Value - second.Pickup.TargetAtUtc.Value).Duration()
            : TimeSpan.Zero;
        return readyGap <= MaxReadyTimeGap
               && DistanceKm(first.Pickup, second.Pickup) <= MaxPickupSeparationKm
               && DistanceKm(first.Dropoff, second.Dropoff) <= MaxDropoffSeparationKm
               && EstimateBundleRouteKm(first, second) <= MaxBundleRouteKm;
    }

    private static decimal EstimateBundleRouteKm(
        FoodDeliveryDriverOfferDto first,
        FoodDeliveryDriverOfferDto second)
        => DistanceKm(first.Pickup, second.Pickup)
           + DistanceKm(second.Pickup, first.Dropoff)
           + DistanceKm(first.Dropoff, second.Dropoff);

    private static decimal DistanceKm(FoodDeliveryDriverStopDto first, FoodDeliveryDriverStopDto second)
    {
        var lat1 = DegreesToRadians((double)first.Latitude!.Value);
        var lat2 = DegreesToRadians((double)second.Latitude!.Value);
        var dLat = lat2 - lat1;
        var dLon = DegreesToRadians((double)(second.Longitude!.Value - first.Longitude!.Value));
        var a = Math.Pow(Math.Sin(dLat / 2d), 2d)
                + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(dLon / 2d), 2d);
        return (decimal)(6371d * 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a)));
    }

    private static bool HasCoordinates(FoodDeliveryDriverOfferDto offer)
        => offer.Pickup.Latitude.HasValue && offer.Pickup.Longitude.HasValue
           && offer.Dropoff.Latitude.HasValue && offer.Dropoff.Longitude.HasValue;

    private static double DegreesToRadians(double value) => value * Math.PI / 180d;
}
