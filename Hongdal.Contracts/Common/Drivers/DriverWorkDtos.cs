using System.Text.Json.Serialization;

namespace Hongdal.Contracts.Common.Drivers;

public static class DriverWorkOfferStatus
{
    public const string Recommended = "Recommended";
    public const string Accepted = "Accepted";
    public const string MovingToPickup = "MovingToPickup";
    public const string PickupConfirmed = "PickupConfirmed";
    public const string MovingToDropoff = "MovingToDropoff";
    public const string Completed = "Completed";
    public const string Rejected = "Rejected";
}

public sealed record DriverWorkProfile(
    string AppKey,
    string DriverDomain,
    string WorkType,
    string DisplayName,
    string Description,
    string Focus);

public sealed record DriverWorkStopDto(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("address")] string Address,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude,
    [property: JsonPropertyName("targetTime")] DateTimeOffset? TargetTime = null);

public sealed record DriverWorkOfferDto(
    [property: JsonPropertyName("offerId")] string OfferId,
    [property: JsonPropertyName("appKey")] string AppKey,
    [property: JsonPropertyName("driverDomain")] string DriverDomain,
    [property: JsonPropertyName("workType")] string WorkType,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("pickup")] DriverWorkStopDto Pickup,
    [property: JsonPropertyName("dropoff")] DriverWorkStopDto Dropoff,
    [property: JsonPropertyName("driverPayout")] decimal DriverPayout,
    [property: JsonPropertyName("distanceKm")] double? DistanceKm,
    [property: JsonPropertyName("recommendationReason")] string RecommendationReason,
    [property: JsonPropertyName("status")] string Status = DriverWorkOfferStatus.Recommended);
