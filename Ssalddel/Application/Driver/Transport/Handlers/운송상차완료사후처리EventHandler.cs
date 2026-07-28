using MediatR;
using System.Text.Json;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Services.Documents;

namespace Ssalddel.Application.Driver.Transport;

public sealed class 운송상차완료사후처리EventHandler : INotificationHandler<운송상차완료됨Event>
{
    private readonly I문서생성OutboxService _문서생성OutboxService;

    public 운송상차완료사후처리EventHandler(I문서생성OutboxService 문서생성OutboxService)
    {
        _문서생성OutboxService = 문서생성OutboxService;
    }

    public async Task Handle(운송상차완료됨Event notification, CancellationToken cancellationToken)
    {
        if (notification.인수증증빙 is null)
        {
            return;
        }

        await _문서생성OutboxService.예약후즉시처리Async(
            new 문서생성요청
            {
                의뢰Id = notification.운송번호,
                운송원장Id = notification.운송Id,
                문서코드 = "상차인수확인서",
                문서명 = "상차 인수 확인서",
                파일명 = $"상차인수확인서-{notification.운송번호}.txt",
                ContentType = "text/plain; charset=utf-8",
                암호화여부 = true,
                다운로드허용여부 = true,
                생성자 = notification.기사Id,
                문서분류코드 = 문서분류코드.수행증빙,
                생명주기상태코드 = notification.인수증증빙.서명확보됨
                    ? 문서생명주기상태코드.서명완료
                    : 문서생명주기상태코드.확인완료,
                원천원장Id = notification.운송Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                원천원장종류코드 = "TransportExecution",
                원천문서종류코드 = "PICKUP_HANDOVER_CONFIRMATION",
                템플릿버전 = "1.0",
                생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
                발급주체코드 = 문서발급주체코드.플랫폼,
                구조화스냅샷Json = JsonSerializer.Serialize(new
                {
                    notification.운송Id,
                    notification.운송번호,
                    notification.현재상태,
                    notification.발생시각Utc
                }),
                관련StableId목록Json = JsonSerializer.Serialize(
                    new[]
                    {
                        문서StableId.만들기(문서StableId종류코드.운송실행, notification.운송Id),
                        문서StableId.만들기(문서StableId종류코드.운송의뢰, notification.운송번호)
                    })
            },
            운송인수증문서Factory.Create상차인수확인서Bytes(notification),
            $"transport-pickup-handover:{notification.운송Id}",
            cancellationToken);
    }

}
