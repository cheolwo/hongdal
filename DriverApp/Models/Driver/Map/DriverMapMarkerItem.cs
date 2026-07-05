using System.Text.Json.Serialization;

namespace DriverApp.Models.Driver.Map;

public sealed record DriverMapMarkerItem(
    [property: JsonPropertyName("requestId")] string RequestId,
    [property: JsonPropertyName("pickupLatitude")] double PickupLatitude,
    [property: JsonPropertyName("pickupLongitude")] double PickupLongitude,
    [property: JsonPropertyName("dropoffLatitude")] double DropoffLatitude,
    [property: JsonPropertyName("dropoffLongitude")] double DropoffLongitude,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("pickupAddress")] string PickupAddress);
