using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Drivers;
using Ssalddel.Contracts.Common.Transport;
using 살뜰.Data;
using 살뜰.도메인.공통;
using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Recommendation;
using 살뜰.Services.DeliveryZones;
using 살뜰.Services.Storage.Local;

namespace 살뜰.Services.Dispatch.Queue;

public interface I배차실행인덱스예열Service
{
    Task<배차실행인덱스예열결과> 예열Async(CancellationToken cancellationToken = default);
}

public sealed record 배차실행인덱스예열결과(
    DateTime 기준시각Utc,
    int 운행중기사수,
    int 기사상태인덱스예열수,
    int 위치인덱스예열수,
    int 근무큐예열수,
    int 미처리운송의뢰수);

public sealed class 배차실행인덱스예열Service : I배차실행인덱스예열Service
{
    private const int 최대예열기사수 = 1000;

    private readonly SsalddelContext _db;
    private readonly IDriverWorkQueueStore _기사근무큐Store;
    private readonly IDriverLocationStore _기사위치Store;
    private readonly I국내화물운송기사상태Store _국내화물운송기사상태Store;
    private readonly I운송원장배달권연결Service _운송원장배달권연결Service;
    private readonly I음식배달권실행공간Store _음식배달권실행공간Store;
    private readonly I국내화물배달권실행공간Store _국내화물배달권실행공간Store;

    public 배차실행인덱스예열Service(
        SsalddelContext db,
        IDriverWorkQueueStore 기사근무큐Store,
        IDriverLocationStore 기사위치Store,
        I국내화물운송기사상태Store 국내화물운송기사상태Store,
        I운송원장배달권연결Service 운송원장배달권연결Service,
        I음식배달권실행공간Store 음식배달권실행공간Store,
        I국내화물배달권실행공간Store 국내화물배달권실행공간Store)
    {
        _db = db;
        _기사근무큐Store = 기사근무큐Store;
        _기사위치Store = 기사위치Store;
        _국내화물운송기사상태Store = 국내화물운송기사상태Store;
        _운송원장배달권연결Service = 운송원장배달권연결Service;
        _음식배달권실행공간Store = 음식배달권실행공간Store;
        _국내화물배달권실행공간Store = 국내화물배달권실행공간Store;
    }

    public async Task<배차실행인덱스예열결과> 예열Async(CancellationToken cancellationToken = default)
    {
        var 기준시각Utc = DateTime.UtcNow;
        var 운행중기사목록 = await _db.용달기사
            .AsNoTracking()
            .Where(x => x.상태 == "활동중")
            .Where(x => x.운행상태 == 상태값.기사운행상태.운행중)
            .OrderBy(x => x.UpdatedAt)
            .Take(최대예열기사수)
            .ToListAsync(cancellationToken);

        var 기사상태인덱스예열수 = 0;
        var 위치인덱스예열수 = 0;
        var 근무큐예열수 = 0;

        foreach (var 기사 in 운행중기사목록)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var 근무 = await _db.기사근무
                .AsNoTracking()
                .Where(x => x.기사Id == 기사.기사Id)
                .OrderByDescending(x => x.시작시각 ?? x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var 위치 = await _db.기사위치기록
                .AsNoTracking()
                .Where(x => x.기사Id == 기사.기사Id)
                .OrderByDescending(x => x.기록시각)
                .FirstOrDefaultAsync(cancellationToken);

            if (근무 is not null)
            {
                await _기사근무큐Store.UpsertAsync(
                    new DriverWorkQueueEntry(
                        기사.기사Id,
                        근무.Id,
                        AsUtc(근무.시작시각 ?? 근무.CreatedAt),
                        근무.시작모드,
                        근무.시작위치,
                        근무.오늘의복귀지주소 ?? 근무.복귀지),
                    cancellationToken);
                근무큐예열수++;
            }

            if (위치 is not null)
            {
                _기사위치Store.Upsert(new DriverLocationSnapshot(
                    기사.기사Id,
                    위치.위도,
                    위치.경도,
                    위치.정확도_m,
                    상태값.기사운행상태.운행중,
                    AsUtc(위치.기록시각),
                    AsUtc(위치.CreatedAt)));
                위치인덱스예열수++;
            }

            var 이전기사상태 = await _국내화물운송기사상태Store.GetAsync(
                기사.기사Id,
                cancellationToken);
            var Aging기준시각 = AsUtc(근무?.시작시각 ?? 근무?.CreatedAt ?? 기사.UpdatedAt);
            var 기사상태 = new 국내화물운송기사상태Snapshot(
                기사.기사Id,
                근무?.Id,
                상태값.기사운행상태.운행중,
                근무 is null ? null : AsUtc(근무.시작시각 ?? 근무.CreatedAt),
                Aging기준시각,
                기사대기Aging점수정책.계산(Aging기준시각, 기준시각Utc),
                위치?.위도,
                위치?.경도,
                위치?.정확도_m,
                위치 is null ? null : AsUtc(위치.기록시각),
                위치 is null ? null : AsUtc(위치.CreatedAt),
                마지막추천시각Utc: null,
                마지막후보없음시각Utc: null,
                후보없음횟수: 0,
                근무?.시작모드,
                근무?.시작위치,
                근무?.오늘의복귀지주소 ?? 근무?.복귀지,
                AppKey: ResolveAppKey(근무?.운송실행유형, 이전기사상태?.AppKey));

            await _국내화물운송기사상태Store.UpsertAsync(기사상태, cancellationToken);
            기사상태인덱스예열수++;

            var 음식배달기사 = string.Equals(
                기사상태.AppKey,
                기사앱식별자.FoodDeliveryDriverApp,
                StringComparison.Ordinal);
            var 기사좌표 = 위치 is null ? null : new 배차경로좌표(위치.위도, 위치.경도);
            var 기사배달권 = 음식배달기사
                ? 음식배달권정책.판정(기사좌표, 기사.주_활동지역)
                : 국내화물배달권정책.판정(기사좌표, 기사.주_활동지역);
            if (string.Equals(기사배달권.배달권키, "unknown", StringComparison.Ordinal))
            {
                await Remove기사모든실행공간Async(기사.기사Id, cancellationToken);
                continue;
            }

            if (음식배달기사)
            {
                await _국내화물배달권실행공간Store.Remove기사Async(기사.기사Id, cancellationToken);
                await _음식배달권실행공간Store.Upsert기사Async(
                    기사배달권.배달권키,
                    기사.기사Id,
                    음식배달권정책.인접배달권키조회(기사배달권.배달권키),
                    cancellationToken);
            }
            else
            {
                await _음식배달권실행공간Store.Remove기사Async(기사.기사Id, cancellationToken);
                await _국내화물배달권실행공간Store.Upsert기사Async(
                    기사배달권.배달권키,
                    기사.기사Id,
                    국내행정구역배달권Catalog.인접배달권키조회(기사배달권.배달권키),
                    cancellationToken);
            }
        }

        var 미처리운송의뢰목록 = await _db.운송원장
            .AsNoTracking()
            .미처리운송의뢰쿼리(기준시각Utc)
            .ToListAsync(cancellationToken);

        foreach (var 배차대기 in 미처리운송의뢰목록)
        {
            var 배달권연결 = await _운송원장배달권연결Service.투영추적Async(
                배차대기,
                cancellationToken);
            var 음식배달의뢰 = 배차대기.배차업무유형 == 상태값.배차업무유형.음식배달;
            var 실행배달권 = 음식배달의뢰
                ? 음식배달권정책.판정(
                    CreatePoint(배차대기.픽업_위도, 배차대기.픽업_경도),
                    배차대기.픽업_도로명주소)
                : 배달권연결.픽업배달권;
            if (string.Equals(실행배달권.배달권키, "unknown", StringComparison.Ordinal))
            {
                await Remove운송의뢰모든실행공간Async(
                    배차대기.의뢰Id,
                    cancellationToken);
                continue;
            }

            if (음식배달의뢰)
            {
                await _국내화물배달권실행공간Store.Remove운송의뢰Async(
                    배차대기.의뢰Id,
                    cancellationToken);
                await _음식배달권실행공간Store.Upsert운송의뢰Async(
                    실행배달권.배달권키,
                    배차대기.의뢰Id,
                    음식배달권정책.인접배달권키조회(실행배달권.배달권키),
                    cancellationToken);
            }
            else
            {
                await _음식배달권실행공간Store.Remove운송의뢰Async(
                    배차대기.의뢰Id,
                    cancellationToken);
                await _국내화물배달권실행공간Store.Upsert운송의뢰Async(
                    실행배달권.배달권키,
                    배차대기.의뢰Id,
                    국내행정구역배달권Catalog.인접배달권키조회(실행배달권.배달권키),
                    cancellationToken);
            }
        }

        if (_db.ChangeTracker.HasChanges())
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new 배차실행인덱스예열결과(
            기준시각Utc,
            운행중기사목록.Count,
            기사상태인덱스예열수,
            위치인덱스예열수,
            근무큐예열수,
            미처리운송의뢰목록.Count);
    }

    private static DateTime AsUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    private static string ResolveAppKey(string? 운송실행유형, string? 이전AppKey)
    {
        if (string.Equals(운송실행유형, 운송실행유형코드.음식배달, StringComparison.Ordinal))
        {
            return 기사앱식별자.FoodDeliveryDriverApp;
        }

        if (string.Equals(운송실행유형, 운송실행유형코드.화물운송, StringComparison.Ordinal))
        {
            return 기사앱식별자.CargoYongdalDriverApp;
        }

        return string.IsNullOrWhiteSpace(이전AppKey)
            ? 기사앱식별자.CargoYongdalDriverApp
            : 이전AppKey;
    }

    private async Task Remove기사모든실행공간Async(
        string 기사Id,
        CancellationToken cancellationToken)
    {
        await _음식배달권실행공간Store.Remove기사Async(기사Id, cancellationToken);
        await _국내화물배달권실행공간Store.Remove기사Async(기사Id, cancellationToken);
    }

    private async Task Remove운송의뢰모든실행공간Async(
        string 의뢰Id,
        CancellationToken cancellationToken)
    {
        await _음식배달권실행공간Store.Remove운송의뢰Async(의뢰Id, cancellationToken);
        await _국내화물배달권실행공간Store.Remove운송의뢰Async(의뢰Id, cancellationToken);
    }

    private static 배차경로좌표? CreatePoint(decimal? latitude, decimal? longitude)
        => latitude.HasValue && longitude.HasValue
            ? new 배차경로좌표(latitude.Value, longitude.Value)
            : null;
}
