using 홍달.Services.Storage.Local;
using 홍달.Services.Dispatch.Coordination;

namespace 홍달.Services.Dispatch.Queue;

public interface I국내화물운송기사상태Service
{
    Task<국내화물운송기사상태Snapshot> 운행시작Async(
        string driverId,
        long shiftId,
        DateTime startedAtUtc,
        string startMode,
        string startLocation,
        string? returnDestination,
        string? 복귀콜선호 = null,
        CancellationToken cancellationToken = default);

    Task<국내화물운송기사상태Snapshot> 위치갱신Async(
        DriverLocationSnapshot location,
        long? shiftId = null,
        decimal? 상차접근허용반경Km = null,
        string? appKey = null,
        CancellationToken cancellationToken = default);

    Task<국내화물운송기사상태Snapshot?> 추천기록Async(
        string driverId,
        DateTime 추천시각Utc,
        CancellationToken cancellationToken = default);

    Task<국내화물운송기사상태Snapshot?> 후보없음기록Async(
        string driverId,
        DateTime 기준시각Utc,
        CancellationToken cancellationToken = default);

    Task 운행종료Async(string driverId, CancellationToken cancellationToken = default);
}

public sealed class 국내화물운송기사상태Service : I국내화물운송기사상태Service
{
    private readonly I국내화물운송기사상태Store _store;

    public 국내화물운송기사상태Service(I국내화물운송기사상태Store store)
    {
        _store = store;
    }

    public async Task<국내화물운송기사상태Snapshot> 운행시작Async(
        string driverId,
        long shiftId,
        DateTime startedAtUtc,
        string startMode,
        string startLocation,
        string? returnDestination,
        string? 복귀콜선호 = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var startedAt = AsUtc(startedAtUtc);
        var snapshot = new 국내화물운송기사상태Snapshot(
            driverId,
            shiftId,
            상태값.기사운행상태.운행중,
            startedAt,
            startedAt,
            Aging점수계산(startedAt, now),
            Latitude: null,
            Longitude: null,
            AccuracyM: null,
            위치기록시각Utc: null,
            위치수신시각Utc: null,
            마지막추천시각Utc: null,
            마지막후보없음시각Utc: null,
            후보없음횟수: 0,
            startMode,
            startLocation,
            returnDestination,
            복귀콜선호: 기사복귀선호코드.Normalize(복귀콜선호));

        await _store.UpsertAsync(snapshot, cancellationToken);
        return snapshot;
    }

    public async Task<국내화물운송기사상태Snapshot> 위치갱신Async(
        DriverLocationSnapshot location,
        long? shiftId = null,
        decimal? 상차접근허용반경Km = null,
        string? appKey = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync(location.DriverId, cancellationToken);
        var now = DateTime.UtcNow;
        var agingBasis = existing?.Aging기준시각Utc
                         ?? existing?.운행시작시각Utc
                         ?? now;

        var snapshot = new 국내화물운송기사상태Snapshot(
            location.DriverId,
            shiftId ?? existing?.ShiftId,
            string.IsNullOrWhiteSpace(location.DrivingStatus)
                ? existing?.운행상태 ?? 상태값.기사운행상태.운행중
                : location.DrivingStatus,
            existing?.운행시작시각Utc,
            agingBasis,
            Aging점수계산(agingBasis, now),
            location.Latitude,
            location.Longitude,
            location.AccuracyM,
            AsUtc(location.RecordedAtUtc),
            AsUtc(location.ReceivedAtUtc),
            existing?.마지막추천시각Utc,
            existing?.마지막후보없음시각Utc,
            existing?.후보없음횟수 ?? 0,
            existing?.StartMode,
            existing?.StartLocation,
            existing?.ReturnDestination,
            Normalize상차접근허용반경(상차접근허용반경Km) ?? existing?.상차접근허용반경Km,
            existing?.복귀콜선호,
            string.IsNullOrWhiteSpace(appKey) ? existing?.AppKey : appKey.Trim());

        await _store.UpsertAsync(snapshot, cancellationToken);
        return snapshot;
    }

    public async Task<국내화물운송기사상태Snapshot?> 추천기록Async(
        string driverId,
        DateTime 추천시각Utc,
        CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync(driverId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var basis = AsUtc(추천시각Utc);
        var snapshot = existing with
        {
            Aging기준시각Utc = basis,
            Aging점수 = 0m,
            마지막추천시각Utc = basis,
            마지막후보없음시각Utc = null,
            후보없음횟수 = 0
        };

        await _store.UpsertAsync(snapshot, cancellationToken);
        return snapshot;
    }

    public async Task<국내화물운송기사상태Snapshot?> 후보없음기록Async(
        string driverId,
        DateTime 기준시각Utc,
        CancellationToken cancellationToken = default)
    {
        var existing = await _store.GetAsync(driverId, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var now = AsUtc(기준시각Utc);
        var snapshot = existing with
        {
            마지막후보없음시각Utc = now,
            후보없음횟수 = existing.후보없음횟수 + 1,
            Aging점수 = Aging점수계산(existing.Aging기준시각Utc, now)
        };

        await _store.UpsertAsync(snapshot, cancellationToken);
        return snapshot;
    }

    public async Task 운행종료Async(string driverId, CancellationToken cancellationToken = default)
    {
        await _store.RemoveAsync(driverId, cancellationToken);
    }

    private static decimal Aging점수계산(DateTime agingBasisUtc, DateTime 기준시각Utc)
        => 기사대기Aging점수정책.계산(agingBasisUtc, 기준시각Utc);

    private static DateTime AsUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    private static decimal? Normalize상차접근허용반경(decimal? value)
    {
        if (!value.HasValue || value.Value <= 0m)
        {
            return null;
        }

        return Math.Round(value.Value, 1);
    }
}
