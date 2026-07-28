using System.Text;
using System.Text.Json;
using MediatR;
using Ssalddel.Application.Warehouse.Events;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Services.Documents;

namespace Ssalddel.Application.Warehouse.Handlers;

public sealed class 창고피킹포장문서생성EventHandler(
    I문서생성OutboxService 문서생성OutboxService) :
    INotificationHandler<창고피킹완료됨Event>,
    INotificationHandler<창고포장완료됨Event>
{
    public Task Handle(
        창고피킹완료됨Event notification,
        CancellationToken cancellationToken)
        => TryCreateAsync(
            new 문서생성요청
            {
                의뢰Id = notification.피킹작업Key,
                문서코드 = "피킹완료표",
                문서명 = "피킹 완료표",
                파일명 = $"피킹완료표-{notification.피킹작업Key}.txt",
                ContentType = "text/plain; charset=utf-8",
                암호화여부 = true,
                다운로드허용여부 = true,
                생성자 = notification.사용자Id,
                문서분류코드 = 문서분류코드.업무작업지,
                생명주기상태코드 = 문서생명주기상태코드.확인완료,
                원천원장Id = notification.피킹작업Key,
                원천원장종류코드 = "WarehousePickingTask",
                원천문서종류코드 = "PICKING_COMPLETION_SHEET",
                템플릿버전 = "1.0",
                생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
                발급주체코드 = 문서발급주체코드.플랫폼,
                구조화스냅샷Json = JsonSerializer.Serialize(new
                {
                    notification.피킹작업Key,
                    notification.창고Id,
                    notification.입고상품Id,
                    notification.출고예정Id,
                    notification.주문참조번호,
                    notification.라인Key,
                    notification.상품명,
                    notification.SKU,
                    notification.피킹수량,
                    notification.적재대코드,
                    notification.묶음바코드,
                    notification.발생시각Utc
                }),
                관련StableId목록Json = JsonSerializer.Serialize(
                    StableIds(
                        notification.주문참조번호,
                        null,
                        notification.입고상품Id,
                        notification.출고예정Id,
                        notification.커뮤니티원장Id))
            },
            [
                "살뜰 피킹 완료표",
                $"피킹 작업: {notification.피킹작업Key}",
                $"주문 참조번호: {notification.주문참조번호}",
                $"상품: {notification.상품명} / SKU {notification.SKU}",
                $"피킹 수량: {notification.피킹수량}",
                $"적재대: {notification.적재대코드}",
                $"묶음 바코드: {notification.묶음바코드}",
                $"완료 시각: {notification.발생시각Utc:yyyy-MM-dd HH:mm:ss} UTC",
                $"담당자: {notification.사용자Id}",
                $"TraceId: {notification.TraceId}"
            ],
            $"picking-completion:{notification.피킹작업Key}",
            cancellationToken);

    public Task Handle(
        창고포장완료됨Event notification,
        CancellationToken cancellationToken)
        => TryCreateAsync(
            new 문서생성요청
            {
                의뢰Id = notification.입고상품Id.ToString(),
                문서코드 = "포장완료표",
                문서명 = "포장 완료표",
                파일명 = $"포장완료표-{notification.입고상품Id}.txt",
                ContentType = "text/plain; charset=utf-8",
                암호화여부 = true,
                다운로드허용여부 = true,
                생성자 = notification.사용자Id,
                문서분류코드 = 문서분류코드.업무작업지,
                생명주기상태코드 = 문서생명주기상태코드.확인완료,
                원천원장Id = notification.입고상품Id.ToString(),
                원천원장종류코드 = "WarehouseInventory",
                원천문서종류코드 = "PACKING_COMPLETION_SHEET",
                템플릿버전 = "1.0",
                생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
                발급주체코드 = 문서발급주체코드.플랫폼,
                구조화스냅샷Json = JsonSerializer.Serialize(new
                {
                    notification.입고요청Id,
                    notification.입고상품Id,
                    notification.창고Id,
                    notification.출고예정Id,
                    notification.주문참조번호,
                    notification.상품명,
                    notification.SKU,
                    notification.포장수량,
                    notification.포장유형,
                    notification.보관위치,
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
                "살뜰 포장 완료표",
                $"입고상품 ID: {notification.입고상품Id}",
                $"주문 참조번호: {notification.주문참조번호}",
                $"상품: {notification.상품명} / SKU {notification.SKU}",
                $"포장 수량: {notification.포장수량}",
                $"포장 유형: {notification.포장유형}",
                $"보관 위치: {notification.보관위치}",
                $"완료 시각: {notification.발생시각Utc:yyyy-MM-dd HH:mm:ss} UTC",
                $"담당자: {notification.사용자Id}",
                $"TraceId: {notification.TraceId}"
            ],
            $"packing-completion:{notification.입고상품Id}",
            cancellationToken);

    private static IReadOnlyList<string> StableIds(
        string 주문참조번호,
        long? 입고요청Id,
        long? 입고상품Id,
        long? 출고예정Id,
        string 커뮤니티원장Id)
    {
        var values = new List<string>();
        Add(values, 문서StableId종류코드.주문참조, 주문참조번호);
        Add(values, 문서StableId종류코드.입고요청, 입고요청Id);
        Add(values, 문서StableId종류코드.입고상품, 입고상품Id);
        Add(values, 문서StableId종류코드.출고예정, 출고예정Id);
        Add(values, 문서StableId종류코드.커뮤니티원장, 커뮤니티원장Id);
        return values;
    }

    private static void Add(List<string> values, string kind, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            values.Add(문서StableId.만들기(kind, value));
        }
    }

    private static void Add(List<string> values, string kind, long? value)
    {
        if (value is > 0)
        {
            values.Add(문서StableId.만들기(kind, value.Value));
        }
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
