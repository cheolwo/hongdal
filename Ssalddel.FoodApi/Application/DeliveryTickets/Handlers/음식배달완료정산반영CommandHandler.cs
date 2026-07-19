using Ssalddel.FoodApi.Application;
using Ssalddel.FoodApi.Application.DeliveryTickets.Commands;
using Ssalddel.FoodApi.Application.Pricing;
using Ssalddel.FoodApi.Application.Settlements;

namespace Ssalddel.FoodApi.Application.DeliveryTickets.Handlers;

public sealed class 음식배달완료정산반영CommandHandler
    : IFoodCommandHandler<음식배달완료정산반영Command, FoodDeliverySettlementApplyResult>
{
    private readonly IFoodDeliveryTicketMemoryIndex _ticketIndex;
    private readonly IFoodDeliveryPricingService _pricingService;
    private readonly IFoodDeliverySettlementStore _settlementStore;

    public 음식배달완료정산반영CommandHandler(
        IFoodDeliveryTicketMemoryIndex ticketIndex,
        IFoodDeliveryPricingService pricingService,
        IFoodDeliverySettlementStore settlementStore)
    {
        _ticketIndex = ticketIndex;
        _pricingService = pricingService;
        _settlementStore = settlementStore;
    }

    public Task<FoodDeliverySettlementApplyResult> HandleAsync(
        음식배달완료정산반영Command command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.TicketId))
        {
            throw new ArgumentException("TicketId가 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(command.DriverId))
        {
            throw new ArgumentException("DriverId가 필요합니다.");
        }

        var ticket = _ticketIndex.GetById(command.TicketId)
            ?? throw new InvalidOperationException("배달권을 찾을 수 없습니다.");

        var distanceMeters = command.ActualDistanceMeters ?? EstimateDistanceMeters(ticket);
        var quote = _pricingService.Quote(distanceMeters);
        var completedAt = command.CompletedAtUtc ?? DateTime.UtcNow;

        ticket.Status = FoodDeliveryTicketStatus.Delivered;
        _ticketIndex.AddOrUpdate(ticket);

        var entry = _settlementStore.AddOrReplace(new FoodDeliverySettlementEntry(
            ticket.TicketId,
            command.DriverId,
            DateOnly.FromDateTime(completedAt),
            quote.DistanceMeters,
            quote.PlatformDeliveryFee,
            quote.DriverPayout,
            quote.PlatformMargin,
            completedAt));

        return Task.FromResult(new FoodDeliverySettlementApplyResult(entry, quote));
    }

    private static int EstimateDistanceMeters(FoodDeliveryTicket ticket)
    {
        var distanceKm = CalculateDistanceKm(ticket.PickupLat, ticket.PickupLng, ticket.DropoffLat, ticket.DropoffLng);
        return distanceKm is null ? 0 : (int)Math.Ceiling(distanceKm.Value * 1000d);
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
