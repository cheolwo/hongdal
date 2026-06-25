namespace 홍달.Services
{
    public interface IDriverLocationStore
    {
        void Upsert(DriverLocationSnapshot snapshot);
        bool TryGetLatest(string driverId, out DriverLocationSnapshot snapshot);
    }

    public sealed record DriverLocationSnapshot(
        string DriverId,
        decimal 위도,
        decimal 경도,
        decimal? 정확도_m,
        string 운행상태,
        DateTime 기록시각,
        DateTime 수신시각);
}
