using Microsoft.EntityFrameworkCore;
using 살뜰.도메인.공통;
using 살뜰.도메인.기사;
using 살뜰.도메인.운송;

namespace Ssalddel.Application.Shipper.Request;

internal sealed record 화주운송실행정보(
    운송원장 운송원장,
    용달기사? 기사,
    기사위치기록? 최근위치);

internal static class 화주운송실행정보조회
{
    internal static async Task<IReadOnlyDictionary<string, 화주운송실행정보>> 조회Async(
        SsalddelContext db,
        IReadOnlyCollection<string> requestIds,
        CancellationToken cancellationToken)
    {
        if (requestIds.Count == 0)
        {
            return new Dictionary<string, 화주운송실행정보>(StringComparer.OrdinalIgnoreCase);
        }

        var ledgers = await db.운송원장
            .AsNoTracking()
            .Where(x => requestIds.Contains(x.의뢰Id))
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        var latestLedgers = ledgers
            .GroupBy(x => x.의뢰Id, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var driverIds = latestLedgers.Values
            .Select(x => x.확정기사Id)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var drivers = driverIds.Count == 0
            ? new Dictionary<string, 용달기사>(StringComparer.OrdinalIgnoreCase)
            : (await db.용달기사
                .AsNoTracking()
                .Where(x => driverIds.Contains(x.기사Id))
                .ToListAsync(cancellationToken))
                .GroupBy(x => x.기사Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var locations = driverIds.Count == 0
            ? new Dictionary<string, 기사위치기록>(StringComparer.OrdinalIgnoreCase)
            : (await db.기사위치기록
                .AsNoTracking()
                .Where(x => driverIds.Contains(x.기사Id))
                .GroupBy(x => x.기사Id)
                .Select(group => group
                    .OrderByDescending(x => x.기록시각)
                    .ThenByDescending(x => x.Id)
                    .First())
                .ToListAsync(cancellationToken))
                .ToDictionary(x => x.기사Id, x => x, StringComparer.OrdinalIgnoreCase);

        return latestLedgers.ToDictionary(
            pair => pair.Key,
            pair =>
            {
                var ledger = pair.Value;
                var driverId = ledger.확정기사Id;
                var hasDriver = !string.IsNullOrWhiteSpace(driverId);
                var canExposeLocation = hasDriver && 기사위치공개가능(ledger.상태);

                drivers.TryGetValue(driverId ?? string.Empty, out var driver);
                locations.TryGetValue(driverId ?? string.Empty, out var location);

                return new 화주운송실행정보(
                    ledger,
                    driver,
                    canExposeLocation ? location : null);
            },
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool 기사위치공개가능(string? transportStatus)
        => transportStatus is not null
           && !string.Equals(transportStatus, 상태값.배차상태.인수완료, StringComparison.Ordinal)
           && !string.Equals(transportStatus, 상태값.배차상태.하차완료, StringComparison.Ordinal)
           && !string.Equals(transportStatus, 상태값.배차상태.취소, StringComparison.Ordinal);
}
