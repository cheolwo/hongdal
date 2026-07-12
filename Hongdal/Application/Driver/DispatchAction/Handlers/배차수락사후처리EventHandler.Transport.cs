using Hongdal.Application.Driver.Transport;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.운송;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed partial class 배차수락사후처리EventHandler
{
    private async Task 운송진행건생성또는보정Async(배차수락됨Event notification, CancellationToken cancellationToken)
    {
        var dispatchRequest = await _db.화주운송의뢰
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.의뢰Id == notification.의뢰Id, cancellationToken);
        if (dispatchRequest is null)
        {
            return;
        }

        var existing = await _db.운송원장
            .FirstOrDefaultAsync(x => x.운송번호 == notification.의뢰Id || x.의뢰Id == notification.의뢰Id, cancellationToken);

        var now = notification.발생시각Utc;
        if (existing is null)
        {
            existing = new 운송원장
            {
                운송번호 = notification.의뢰Id,
                의뢰Id = notification.의뢰Id,
                화주Id = dispatchRequest.화주Id,
                상태 = 기사운송상태코드.배차확정,
                기사_운송자 = notification.기사Id,
                확정기사Id = notification.기사Id,
                픽업_도로명주소 = dispatchRequest.픽업_도로명주소,
                픽업_상세주소 = dispatchRequest.픽업_상세주소,
                픽업_위도 = dispatchRequest.픽업_위도,
                픽업_경도 = dispatchRequest.픽업_경도,
                하차_도로명주소 = dispatchRequest.하차_도로명주소,
                하차_상세주소 = dispatchRequest.하차_상세주소,
                하차_위도 = dispatchRequest.하차_위도,
                하차_경도 = dispatchRequest.하차_경도,
                출발지 = dispatchRequest.픽업_도로명주소,
                도착지 = dispatchRequest.하차_도로명주소,
                운임 = dispatchRequest.최종운임,
                첨부_json = "[]",
                메모 = "배차 수락으로 생성된 기사 운송 진행 건",
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.운송원장.Add(existing);
        }
        else
        {
            existing.운송번호 = string.IsNullOrWhiteSpace(existing.운송번호) ? notification.의뢰Id : existing.운송번호;
            existing.의뢰Id = string.IsNullOrWhiteSpace(existing.의뢰Id) ? notification.의뢰Id : existing.의뢰Id;
            existing.화주Id = string.IsNullOrWhiteSpace(existing.화주Id) ? dispatchRequest.화주Id : existing.화주Id;
            existing.기사_운송자 = notification.기사Id;
            existing.확정기사Id = notification.기사Id;
            existing.픽업_도로명주소 = dispatchRequest.픽업_도로명주소;
            existing.픽업_상세주소 = dispatchRequest.픽업_상세주소;
            existing.픽업_위도 = dispatchRequest.픽업_위도;
            existing.픽업_경도 = dispatchRequest.픽업_경도;
            existing.하차_도로명주소 = dispatchRequest.하차_도로명주소;
            existing.하차_상세주소 = dispatchRequest.하차_상세주소;
            existing.하차_위도 = dispatchRequest.하차_위도;
            existing.하차_경도 = dispatchRequest.하차_경도;
            existing.출발지 = dispatchRequest.픽업_도로명주소;
            existing.도착지 = dispatchRequest.하차_도로명주소;
            existing.운임 = dispatchRequest.최종운임;
            if (string.Equals(existing.상태, 기사운송상태코드.배차대기, StringComparison.Ordinal)
                || string.Equals(existing.상태, 기사운송상태코드.매칭중, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(existing.상태))
            {
                existing.상태 = 기사운송상태코드.배차확정;
            }

            existing.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await _원장동기화Service.운송실행투영동기화Async(existing, notification.기사Id, cancellationToken);
    }
}
