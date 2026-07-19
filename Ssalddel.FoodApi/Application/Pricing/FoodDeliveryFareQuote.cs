namespace Ssalddel.FoodApi.Application.Pricing;

public sealed record FoodDeliveryFareQuote(
    int DistanceMeters,
    decimal PlatformDeliveryFee,
    decimal DriverPayout,
    decimal PlatformMargin,
    string PricingUnitDescription);
