using Microsoft.EntityFrameworkCore;
using 홍달.도메인.공통;
using 홍달.도메인.배차;

namespace 홍달.Services.Dispatch.Coordination;

public sealed partial class 국내화물배차조율적용Service
{
    private async Task<국내화물배차추천잠금?> 추천잠금시도Async(
        국내화물배차제안 배차제안,
        int? timeoutSeconds,
        int 최대수락운송건수,
        CancellationToken cancellationToken)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        // 추천 잠금은 메모리 큐가 아니라 DB 원장인 배차대기에 기록한다.
        // 서버가 재시작되면 만료되지 않은 추천중 의뢰는 이 상태를 보고 중복 추천을 막는다.
        var 배차대기 = await _db.배차대기
            .FirstOrDefaultAsync(x => x.의뢰Id == 배차제안.의뢰Id, cancellationToken);
        if (배차대기 is null || !국내화물배차후보금지정책.추천잠금가능(배차대기, DateTime.UtcNow))
        {
            await tx.RollbackAsync(cancellationToken);
            return null;
        }

        var 현재수락운송건수 = await _db.배송_운송
            .AsNoTracking()
            .CountAsync(
                x => x.기사_운송자 == 배차제안.기사Id && x.상태 != "인수완료",
                cancellationToken);
        var 현재추천잠금건수 = await _db.배차대기
            .AsNoTracking()
            .CountAsync(
                x => x.현재추천대상기사Id == 배차제안.기사Id
                     && x.상태 == 상태값.배차대기상태.대기
                     && x.배차큐단계 == 상태값.배차큐단계.배차추천
                     && x.배차노출상태 == 상태값.배차노출상태.추천중
                     && (!x.추천만료시각.HasValue || x.추천만료시각 > DateTime.UtcNow),
                cancellationToken);
        if (현재수락운송건수 + 현재추천잠금건수 >= 최대수락운송건수)
        {
            await tx.RollbackAsync(cancellationToken);
            return null;
        }

        var 기준시각Utc = DateTime.UtcNow;
        배차대기.배차큐단계 = 상태값.배차큐단계.배차추천;
        배차대기.배차노출상태 = 상태값.배차노출상태.추천중;
        배차대기.현재추천대상기사Id = 배차제안.기사Id;
        배차대기.추천라운드 += 1;
        배차대기.추천시작시각 = 기준시각Utc;
        배차대기.추천만료시각 = 기준시각Utc.AddSeconds(timeoutSeconds ?? _options.추천유지시간초);
        배차대기.UpdatedAt = 기준시각Utc;

        await _db.SaveChangesAsync(cancellationToken);

        await _알림Service.추천알림요청생성Async(
            배차대기.Id,
            배차대기.의뢰Id,
            배차제안.기사Id,
            배차대기.추천라운드,
            cancellationToken);

        await _기사상태Service.추천기록Async(
            배차제안.기사Id,
            기준시각Utc,
            cancellationToken);
        await _배달권실행공간Store.Remove운송의뢰Async(배차대기.의뢰Id, cancellationToken);

        await tx.CommitAsync(cancellationToken);

        return new 국내화물배차추천잠금(
            배차대기.의뢰Id,
            배차제안.기사Id,
            배차대기.추천라운드,
            기준시각Utc,
            배차대기.추천만료시각!.Value);
    }
}
