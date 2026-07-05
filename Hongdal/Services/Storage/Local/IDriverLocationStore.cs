namespace 홍달.Services.Storage.Local
{
    public interface IDriverLocationStore
    {
        void Upsert(DriverLocationSnapshot snapshot);
        bool TryGetLatest(string driverId, out DriverLocationSnapshot snapshot);
    }

    public sealed record DriverLocationSnapshot(
        string DriverId,
        decimal Latitude,
        decimal Longitude,
        decimal? AccuracyM,
        string DrivingStatus,
        DateTime RecordedAtUtc,
        DateTime ReceivedAtUtc);
}



