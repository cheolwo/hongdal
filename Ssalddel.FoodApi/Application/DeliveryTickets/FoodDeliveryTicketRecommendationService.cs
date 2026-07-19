using Ssalddel.FoodApi.Services;

namespace Ssalddel.FoodApi.Application.DeliveryTickets;

public sealed class FoodDeliveryTicketRecommendationService : IFoodDeliveryTicketRecommendationService
{
    private readonly IFoodDeliveryTicketMemoryIndex _ticketIndex;
    private readonly IKakao좌표변환Service _kakaoAddressService;

    public FoodDeliveryTicketRecommendationService(
        IFoodDeliveryTicketMemoryIndex ticketIndex,
        IKakao좌표변환Service kakaoAddressService)
    {
        _ticketIndex = ticketIndex;
        _kakaoAddressService = kakaoAddressService;
    }

    public async Task<IReadOnlyList<FoodDeliveryTicketRecommendation>> RecommendAsync(
        FoodDeliveryTicketRecommendationRequest request,
        CancellationToken cancellationToken = default)
    {
        var take = Math.Clamp(request.Take, 1, 100);
        var region = await ResolveRegionAsync(request, cancellationToken);
        var candidates = _ticketIndex.GetPendingByRegion(region, take * 3);

        return candidates
            .Select(ticket => ToRecommendation(ticket, request.DriverLat, request.DriverLng))
            .OrderBy(x => x.DistanceKm ?? double.MaxValue)
            .ThenByDescending(x => x.PriorityScore)
            .ThenBy(x => x.PickupReadyAtUtc)
            .Take(take)
            .ToArray();
    }

    private async Task<AddressRegionKey> ResolveRegionAsync(
        FoodDeliveryTicketRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.Region2) || !string.IsNullOrWhiteSpace(request.Region3))
        {
            return AddressRegionKey.FromKakao(request.Region1, request.Region2, request.Region3);
        }

        if (request.DriverLat is decimal lat && request.DriverLng is decimal lng)
        {
            var kakao = await _kakaoAddressService.좌표지역정보조회Async(lat, lng, cancellationToken);
            if (kakao is not null)
            {
                return AddressRegionKey.FromKakao(kakao.Region1, kakao.Region2, kakao.Region3);
            }
        }

        return AddressRegionKey.Empty;
    }

    private static FoodDeliveryTicketRecommendation ToRecommendation(
        FoodDeliveryTicket ticket,
        decimal? driverLat,
        decimal? driverLng)
    {
        return new FoodDeliveryTicketRecommendation
        {
            TicketId = ticket.TicketId,
            FoodOrderNo = ticket.FoodOrderNo,
            RestaurantId = ticket.RestaurantId,
            PickupAddress = ticket.PickupAddress,
            DropoffAddress = ticket.DropoffAddress,
            PickupRegion2Key = ticket.PickupRegion.Region2Key,
            PickupRegion3Key = ticket.PickupRegion.Region3Key,
            PriorityScore = ticket.PriorityScore,
            DistanceKm = CalculateDistanceKm(driverLat, driverLng, ticket.PickupLat, ticket.PickupLng),
            PickupReadyAtUtc = ticket.PickupReadyAtUtc
        };
    }

    private static double? CalculateDistanceKm(decimal? lat1, decimal? lng1, decimal? lat2, decimal? lng2)
    {
        if (lat1 is null || lng1 is null || lat2 is null || lng2 is null)
        {
            return null;
        }

        const double earthRadiusKm = 6371.0088;
        var dLat = ToRadians((double)(lat2.Value - lat1.Value));
        var dLng = ToRadians((double)(lng2.Value - lng1.Value));
        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians((double)lat1.Value)) *
            Math.Cos(ToRadians((double)lat2.Value)) *
            Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180d;
}
