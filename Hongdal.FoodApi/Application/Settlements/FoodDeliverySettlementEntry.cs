using Hongdal.FoodApi.Application.Pricing;

namespace Hongdal.FoodApi.Application.Settlements;

public sealed record FoodDeliverySettlementEntry(
    string TicketId,
    string DriverId,
    DateOnly BusinessDate,
    int DistanceMeters,
    decimal PlatformDeliveryFee,
    decimal DriverPayout,
    decimal PlatformMargin,
    DateTime CompletedAtUtc);

public sealed class FoodDeliverySettlementSummary
{
    public string DriverId { get; init; } = string.Empty;
    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }
    public int DeliveryCount { get; init; }
    public decimal TotalPlatformDeliveryFee { get; init; }
    public decimal TotalDriverPayout { get; init; }
    public decimal TotalPlatformMargin { get; init; }
    public IReadOnlyList<FoodDeliverySettlementEntry> Entries { get; init; } = [];
}

public sealed record FoodDeliverySettlementApplyResult(
    FoodDeliverySettlementEntry Entry,
    FoodDeliveryFareQuote FareQuote);
