using System.Text;
using System.Text.Json;
using MediatR;
using Ssalddel.Application.Warehouse.Events;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Services.Documents;

namespace Ssalddel.Application.Warehouse.Handlers;

public sealed class 창고출고문서생성EventHandler(
    I문서생성OutboxService 문서생성OutboxService) :
    INotificationHandler<창고출고인계준비완료됨Event>,
    INotificationHandler<창고출고운송인계완료됨Event>
{
    public Task Handle(
        창고출고인계준비완료됨Event notification,
        CancellationToken cancellationToken)
        => TryCreateAsync(
            new 문서생성요청
            {
                의뢰Id = notification.출고예정Id.ToString(),
                문서코드 = "출고예정목록",
                문서명 = "출고 예정 목록",
                파일명 = $"출고예정목록-{notification.출고예정Id}.txt",
                ContentType = "text/plain; charset=utf-8",
                암호화여부 = true,
                다운로드허용여부 = true,
                생성자 = notification.사용자Id,
                문서분류코드 = 문서분류코드.업무작업지,
                생명주기상태코드 = 문서생명주기상태코드.확인완료,
                원천원장Id = notification.출고예정Id.ToString(),
                원천원장종류코드 = "WarehouseOutboundPlan",
                원천문서종류코드 = "OUTBOUND_EXPECTED_ITEMS",
                템플릿버전 = "1.0",
                생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
                발급주체코드 = 문서발급주체코드.플랫폼,
                구조화스냅샷Json = JsonSerializer.Serialize(new
                {
                    notification.주문참조번호,
                    notification.입고요청Id,
                    notification.출고예정Id,
                    notification.입고상품Id,
                    notification.인계수량,
                    notification.발생시각Utc
                }),
                관련StableId목록Json = JsonSerializer.Serialize(
                    StableIds(
                        notification.주문참조번호,
                        notification.입고요청Id,
                        notification.입고상품Id,
                        notification.출고예정Id,
                        notification.커뮤니티원장Id))
            },
            [
                "살뜰 출고 예정 목록",
                $"출고예정 ID: {notification.출고예정Id}",
                $"입고상품 ID: {notification.입고상품Id}",
                $"인계 예정 수량: {notification.인계수량}",
                $"확인 시각: {notification.발생시각Utc:yyyy-MM-dd HH:mm:ss} UTC",
                $"담당자: {notification.사용자Id}",
                $"TraceId: {notification.TraceId}"
            ],
            $"outbound-expected-items:{notification.출고예정Id}",
            cancellationToken);

    public Task Handle(
        창고출고운송인계완료됨Event notification,
        CancellationToken cancellationToken)
        => TryCreateAsync(
            new 문서생성요청
            {
                의뢰Id = notification.운송의뢰Id,
                문서코드 = "출고인계확인서",
                문서명 = "출고 인계 확인서",
                파일명 = $"출고인계확인서-{notification.출고예정Id}.txt",
                ContentType = "text/plain; charset=utf-8",
                암호화여부 = true,
                다운로드허용여부 = true,
                생성자 = notification.사용자Id,
                문서분류코드 = 문서분류코드.수행증빙,
                생명주기상태코드 = 문서생명주기상태코드.발행완료,
                원천원장Id = notification.출고예정Id.ToString(),
                원천원장종류코드 = "WarehouseOutboundPlan",
                원천문서종류코드 = "OUTBOUND_HANDOFF_CONFIRMATION",
                템플릿버전 = "1.0",
                생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
                발급주체코드 = 문서발급주체코드.플랫폼,
                구조화스냅샷Json = JsonSerializer.Serialize(new
                {
                    notification.주문참조번호,
                    notification.입고요청Id,
                    notification.출고예정Id,
                    notification.입고상품Id,
                    notification.운송의뢰Id,
                    notification.인계수량,
                    notification.발생시각Utc
                }),
                관련StableId목록Json = JsonSerializer.Serialize(
                    StableIds(
                        notification.주문참조번호,
                        notification.입고요청Id,
                        notification.입고상품Id,
                        notification.출고예정Id,
                        notification.커뮤니티원장Id,
                        notification.운송의뢰Id))
            },
            [
                "살뜰 출고 인계 확인서",
                $"출고예정 ID: {notification.출고예정Id}",
                $"운송의뢰 ID: {notification.운송의뢰Id}",
                $"기사 ID: {notification.기사Id}",
                $"차량: {notification.차량}",
                $"인계 수량: {notification.인계수량}",
                $"인계 시각: {notification.발생시각Utc:yyyy-MM-dd HH:mm:ss} UTC",
                $"담당자: {notification.사용자Id}",
                $"TraceId: {notification.TraceId}"
            ],
            $"outbound-handoff-confirmation:{notification.출고예정Id}",
            cancellationToken);

    private static IReadOnlyList<string> StableIds(
        string 주문참조번호,
        long? 입고요청Id,
        long 입고상품Id,
        long 출고예정Id,
        string 커뮤니티원장Id,
        string? 운송의뢰Id = null)
    {
        var values = new List<string>
        {
            문서StableId.만들기(문서StableId종류코드.입고상품, 입고상품Id),
            문서StableId.만들기(문서StableId종류코드.출고예정, 출고예정Id)
        };
        if (!string.IsNullOrWhiteSpace(주문참조번호))
        {
            values.Add(문서StableId.만들기(문서StableId종류코드.주문참조, 주문참조번호));
        }

        if (입고요청Id is > 0)
        {
            values.Add(문서StableId.만들기(문서StableId종류코드.입고요청, 입고요청Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(커뮤니티원장Id))
        {
            values.Add(문서StableId.만들기(문서StableId종류코드.커뮤니티원장, 커뮤니티원장Id));
        }

        if (!string.IsNullOrWhiteSpace(운송의뢰Id))
        {
            values.Add(문서StableId.만들기(문서StableId종류코드.운송의뢰, 운송의뢰Id));
        }

        return values;
    }

    private async Task TryCreateAsync(
        문서생성요청 request,
        IReadOnlyList<string> lines,
        string deduplicationKey,
        CancellationToken cancellationToken)
    {
        await 문서생성OutboxService.예약후즉시처리Async(
            request,
            Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines)),
            deduplicationKey,
            cancellationToken);
    }
}
