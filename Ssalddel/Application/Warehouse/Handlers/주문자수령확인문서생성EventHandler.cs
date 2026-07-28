using System.Text;
using System.Text.Json;
using MediatR;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Services.Documents;

namespace Ssalddel.Application.Warehouse.Handlers;

public sealed class 주문자수령확인문서생성EventHandler(
    I문서생성OutboxService 문서생성OutboxService) :
    INotificationHandler<주문자상품입고완료됨Event>
{
    public async Task Handle(
        주문자상품입고완료됨Event notification,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.주문참조번호)
            || notification.입고요청Ids.Count == 0)
        {
            return;
        }

        var orderReference = notification.주문참조번호.Trim();
        var stableIds = new List<string>
        {
            문서StableId.만들기(문서StableId종류코드.주문참조, orderReference)
        };
        stableIds.AddRange(notification.입고요청Ids
            .Where(id => id > 0)
            .Distinct()
            .Select(id => 문서StableId.만들기(문서StableId종류코드.입고요청, id)));

        var request = new 문서생성요청
        {
            의뢰Id = orderReference,
            문서코드 = "수령확인서",
            문서명 = "수령 확인서",
            파일명 = $"수령확인서-{orderReference}.txt",
            ContentType = "text/plain; charset=utf-8",
            암호화여부 = true,
            다운로드허용여부 = true,
            생성자 = notification.주문자UserId,
            문서분류코드 = 문서분류코드.수행증빙,
            생명주기상태코드 = 문서생명주기상태코드.수령확인,
            원천원장Id = orderReference,
            원천원장종류코드 = 문서StableId종류코드.주문참조,
            원천문서종류코드 = "ORDER_RECEIPT_ACKNOWLEDGEMENT",
            템플릿버전 = "1.0",
            생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
            발급주체코드 = 문서발급주체코드.업무담당자,
            구조화스냅샷Json = JsonSerializer.Serialize(new
            {
                notification.주문Id,
                주문참조번호 = orderReference,
                notification.주문자UserId,
                notification.입고요청Ids,
                notification.발생시각Utc
            }),
            관련StableId목록Json = JsonSerializer.Serialize(stableIds)
        };
        var lines = new List<string>
        {
            "살뜰 수령 확인서",
            $"주문 참조번호: {orderReference}",
            $"수령 확인자: {notification.주문자UserId}",
            $"입고 요청 ID: {string.Join(", ", notification.입고요청Ids)}",
            $"수령 확인 시각: {notification.발생시각Utc:yyyy-MM-dd HH:mm:ss} UTC",
            $"TraceId: {notification.TraceId}"
        };

        await 문서생성OutboxService.예약후즉시처리Async(
            request,
            Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)),
            $"order-receipt:{orderReference}",
            cancellationToken);
    }
}
