using MediatR;
using Microsoft.EntityFrameworkCore;
using 살뜰.도메인.통관;
using Ssalddel.Application.Warehouse;

namespace 살뜰.Services.External.Customs;

public sealed class 통관상태동기화Service
{
    private readonly SsalddelContext _db;
    private readonly I화물통관진행조회Service _조회Service;
    private readonly IPublisher _publisher;

    public 통관상태동기화Service(
        SsalddelContext db,
        I화물통관진행조회Service 조회Service,
        IPublisher publisher)
    {
        _db = db;
        _조회Service = 조회Service;
        _publisher = publisher;
    }

    public async Task<int> 동기화Async(int batchSize, CancellationToken cancellationToken)
    {
        var size = Math.Max(1, batchSize);

        var 대상목록 = await _db.통관조회연동
            .Where(x => x.사용자조회동의여부)
            .Where(x => x.연동상태 == 통관연동상태.조회대기 || x.연동상태 == 통관연동상태.정상조회 || x.연동상태 == 통관연동상태.조회실패)
            .Where(x => x.화물관리번호 != null || x.MasterBl != null || x.HouseBl != null)
            .OrderBy(x => x.마지막조회시각)
            .Take(size)
            .ToListAsync(cancellationToken);

        foreach (var 연동 in 대상목록)
        {
            var 이전단계 = 연동.마지막진행단계;
            연동.연동상태 = 통관연동상태.조회중;
            연동.UpdatedAt = DateTime.UtcNow;

            var result = await _조회Service.조회Async(
                new 화물통관진행조회Request
                {
                    화물관리번호 = 연동.화물관리번호,
                    MasterBl = 연동.MasterBl,
                    HouseBl = 연동.HouseBl
                },
                cancellationToken);

            if (!result.조회성공여부)
            {
                연동.연동상태 = 통관연동상태.조회실패;
                연동.마지막오류 = result.오류메시지 ?? "화물통관 진행정보 조회에 실패했습니다.";
                연동.마지막조회시각 = result.조회시각;
                연동.UpdatedAt = DateTime.UtcNow;
                continue;
            }

            연동.연동상태 = 통관연동상태.정상조회;
            연동.마지막오류 = null;
            연동.마지막조회시각 = result.조회시각;
            연동.마지막진행단계 = result.진행단계;
            연동.UpdatedAt = DateTime.UtcNow;

            if (이전단계 != result.진행단계)
            {
                await _publisher.Publish(
                    new 통관상태변경감지됨Event(
                        연동.주문Id,
                        연동.통관절차Id,
                        이전단계,
                        result.진행단계,
                        result.처리단계명,
                        DateTime.UtcNow,
                        System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty),
                    cancellationToken);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return 대상목록.Count;
    }
}
