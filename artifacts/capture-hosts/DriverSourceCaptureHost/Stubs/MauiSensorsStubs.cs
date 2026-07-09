namespace Microsoft.Maui.Devices.Sensors;

public enum GeolocationAccuracy
{
    Low,
    Medium,
    High
}

public sealed class GeolocationRequest
{
    public GeolocationRequest(GeolocationAccuracy accuracy, TimeSpan timeout)
    {
        Accuracy = accuracy;
        Timeout = timeout;
    }

    public GeolocationAccuracy Accuracy { get; }

    public TimeSpan Timeout { get; }
}

public sealed class Location
{
    public double Latitude { get; init; } = 37.5665;

    public double Longitude { get; init; } = 126.9780;
}

public sealed class Geolocation
{
    public static Geolocation Default { get; } = new();

    public Task<Location?> GetLocationAsync(GeolocationRequest request)
        => Task.FromResult<Location?>(new Location());
}
