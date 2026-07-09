namespace 홍달.Services.Storage.Local;

public interface I국내화물운송기사상태Store
{
    Task UpsertAsync(국내화물운송기사상태Snapshot snapshot, CancellationToken cancellationToken = default);
    Task<국내화물운송기사상태Snapshot?> GetAsync(string driverId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<국내화물운송기사상태Snapshot>> 위치반경조회Async(
        decimal latitude,
        decimal longitude,
        decimal radiusKm,
        int take,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<국내화물운송기사상태Snapshot>> 활성기사조회Async(
        int take,
        CancellationToken cancellationToken = default);
    Task RemoveAsync(string driverId, CancellationToken cancellationToken = default);
}

public sealed record 국내화물운송기사상태Snapshot(
    string DriverId,
    long? ShiftId,
    string 운행상태,
    DateTime? 운행시작시각Utc,
    DateTime Aging기준시각Utc,
    decimal Aging점수,
    decimal? Latitude,
    decimal? Longitude,
    decimal? AccuracyM,
    DateTime? 위치기록시각Utc,
    DateTime? 위치수신시각Utc,
    DateTime? 마지막추천시각Utc,
    DateTime? 마지막후보없음시각Utc,
    int 후보없음횟수,
    string? StartMode,
    string? StartLocation,
    string? ReturnDestination,
    decimal? 상차접근허용반경Km = null,
    string? 복귀콜선호 = null);
