using System.Text;
using System.Text.Json;
using MediatR;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Services.Documents;

namespace Ssalddel.Application.Warehouse.Handlers;

public sealed class 주문확인문서생성EventHandler(
    I문서생성OutboxService 문서생성OutboxService) :
    INotificationHandler<주문결제완료됨Event>
{
    public async Task Handle(
        주문결제완료됨Event notification,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(notification.주문참조번호)
            || notification.상품목록.Count == 0)
        {
            return;
        }

        var orderReference = notification.주문참조번호.Trim();
        var stableIds = new[]
        {
            문서StableId.만들기(문서StableId종류코드.주문참조, orderReference)
        };
        var request = new 문서생성요청
        {
            의뢰Id = orderReference,
            문서코드 = "주문확인서",
            문서명 = "주문 확인서",
            파일명 = $"주문확인서-{orderReference}.txt",
            ContentType = "text/plain; charset=utf-8",
            암호화여부 = true,
            다운로드허용여부 = true,
            생성자 = notification.주문자UserId,
            문서분류코드 = 문서분류코드.거래명세,
            생명주기상태코드 = 문서생명주기상태코드.발행완료,
            원천원장Id = orderReference,
            원천원장종류코드 = 문서StableId종류코드.주문참조,
            원천문서종류코드 = "ORDER_CONFIRMATION",
            템플릿버전 = "1.0",
            생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
            발급주체코드 = 문서발급주체코드.플랫폼,
            구조화스냅샷Json = JsonSerializer.Serialize(new
            {
                notification.주문Id,
                주문참조번호 = orderReference,
                notification.주문자UserId,
                notification.판매자UserId,
                notification.상품목록,
                notification.수령창고Id,
                notification.수령지표시명,
                notification.발생시각Utc
            }),
            관련StableId목록Json = JsonSerializer.Serialize(stableIds)
        };

        var lines = new List<string>
        {
            "살뜰 주문 확인서",
            $"주문 참조번호: {orderReference}",
            $"주문자: {notification.주문자UserId}",
            $"판매자: {notification.판매자UserId}",
            $"확인 시각: {notification.발생시각Utc:yyyy-MM-dd HH:mm:ss} UTC"
        };
        lines.AddRange(notification.상품목록.Select((item, index) =>
            $"{index + 1}. {item.상품명} / SKU {item.SKU} / 수량 {item.수량}"));
        lines.Add($"TraceId: {notification.TraceId}");

        await 문서생성OutboxService.예약후즉시처리Async(
            request,
            Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)),
            $"order-confirmation:{orderReference}",
            cancellationToken);
    }
}
