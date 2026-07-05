namespace DriverApp.Avalonia.Models;

public sealed class DriverRequestItem
{
    public string RequestId { get; set; } = string.Empty;
    public string CargoType { get; set; } = string.Empty;
    public string Pickup { get; set; } = string.Empty;
    public string Dropoff { get; set; } = string.Empty;
    public decimal? RecommendedScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}
