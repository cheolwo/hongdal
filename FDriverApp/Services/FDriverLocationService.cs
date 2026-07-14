namespace FDriverApp.Services;

public sealed record FDriverLocationSnapshot(
    decimal Latitude,
    decimal Longitude,
    decimal? AccuracyMeters,
    DateTime RecordedAtUtc);

public interface IFDriverLocationService
{
    Task<FDriverLocationSnapshot?> GetCurrentAsync(CancellationToken cancellationToken = default);
}

public sealed class FDriverLocationService : IFDriverLocationService
{
    public async Task<FDriverLocationSnapshot?> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permission != PermissionStatus.Granted)
            {
                permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (permission != PermissionStatus.Granted)
            {
                return null;
            }

            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(12)),
                cancellationToken);
            return location is null
                ? null
                : new FDriverLocationSnapshot(
                    (decimal)location.Latitude,
                    (decimal)location.Longitude,
                    location.Accuracy.HasValue ? (decimal)location.Accuracy.Value : null,
                    location.Timestamp.UtcDateTime);
        }
        catch (Exception ex) when (ex is FeatureNotSupportedException
                                   or FeatureNotEnabledException
                                   or PermissionException)
        {
            return null;
        }
    }
}
