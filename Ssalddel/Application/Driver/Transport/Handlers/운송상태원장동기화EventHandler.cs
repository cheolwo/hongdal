using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using MediatR;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;

namespace Ssalddel.Application.Driver.Transport;

public sealed class 운송상태원장동기화EventHandler :
    INotificationHandler<운송상차지도착됨Event>,
    INotificationHandler<운송상차완료됨Event>,
    INotificationHandler<운송하차지도착됨Event>,
    INotificationHandler<운송인수완료됨Event>,
    INotificationHandler<운송문제신고됨Event>
{
    private readonly SsalddelContext _db;
    private readonly I운송원장Mongo동기화Service _원장동기화Service;
    private readonly I음식마트원장동기화OutboxService _음식마트원장동기화OutboxService;
    private readonly I원장다이어그램실시간알림Service _실시간알림Service;
    private readonly ILogger<운송상태원장동기화EventHandler> _logger;

    public 운송상태원장동기화EventHandler(
        SsalddelContext db,
        I운송원장Mongo동기화Service 원장동기화Service,
        I음식마트원장동기화OutboxService 음식마트원장동기화OutboxService,
        I원장다이어그램실시간알림Service 실시간알림Service,
        ILogger<운송상태원장동기화EventHandler> logger)
    {
        _db = db;
        _원장동기화Service = 원장동기화Service;
        _음식마트원장동기화OutboxService = 음식마트원장동기화OutboxService;
        _실시간알림Service = 실시간알림Service;
        _logger = logger;
    }

    public Task Handle(운송상차지도착됨Event notification, CancellationToken cancellationToken)
        => 원장동기화Async(notification.운송Id, notification.기사Id, notification.TraceId, cancellationToken);

    public Task Handle(운송상차완료됨Event notification, CancellationToken cancellationToken)
        => 원장동기화Async(notification.운송Id, notification.기사Id, notification.TraceId, cancellationToken);

    public Task Handle(운송하차지도착됨Event notification, CancellationToken cancellationToken)
        => 원장동기화Async(notification.운송Id, notification.기사Id, notification.TraceId, cancellationToken);

    public Task Handle(운송인수완료됨Event notification, CancellationToken cancellationToken)
        => 원장동기화Async(notification.운송Id, notification.기사Id, notification.TraceId, cancellationToken);

    public Task Handle(운송문제신고됨Event notification, CancellationToken cancellationToken)
        => 원장동기화Async(notification.운송Id, notification.기사Id, notification.TraceId, cancellationToken);

    private async Task 원장동기화Async(
        long 운송Id,
        string 변경자,
        string traceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var 운송실행투영 = await _db.운송원장
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == 운송Id, cancellationToken);
            if (운송실행투영 is null)
            {
                _logger.LogWarning(
                    "운송 상태 변경 후 원장 동기화 대상을 찾지 못했습니다. 운송Id={운송Id}, TraceId={TraceId}",
                    운송Id,
                    traceId);
                return;
            }

            var 원장 = await _원장동기화Service.운송실행투영동기화Async(
                운송실행투영,
                string.IsNullOrWhiteSpace(변경자) ? "system" : 변경자,
                cancellationToken);

            if (원장 is not null)
            {
                await _실시간알림Service.변경알림Async(
                    원장,
                    Resolve변경블록Id(원장.현재단계Key),
                    cancellationToken);
            }

            await 관련출고원장동기화Async(
                운송실행투영,
                string.IsNullOrWhiteSpace(변경자) ? "system" : 변경자,
                traceId,
                cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                ex,
                "운송 상태 변경 후 원장 동기화에 실패했습니다. 운송Id={운송Id}, TraceId={TraceId}",
                운송Id,
                traceId);
        }
    }

    private async Task 관련출고원장동기화Async(
        살뜰.도메인.운송.운송원장 운송실행투영,
        string 변경자,
        string traceId,
        CancellationToken cancellationToken)
    {
        var 의뢰Id = Clean(운송실행투영.의뢰Id);
        var 원본의뢰Id = Clean(운송실행투영.원본의뢰Id);
        if (의뢰Id is null && 원본의뢰Id is null)
        {
            return;
        }

        var 출고목록 = await _db.출고예정
            .AsNoTracking()
            .Where(x =>
                (의뢰Id != null && x.운송의뢰Id == 의뢰Id)
                || (원본의뢰Id != null && x.운송의뢰Id == 원본의뢰Id)
                || (원본의뢰Id != null && x.주문참조번호 == 원본의뢰Id))
            .ToListAsync(cancellationToken);
        if (출고목록.Count == 0)
        {
            return;
        }

        var 입고요청Ids = 출고목록
            .Where(x => x.입고요청Id.HasValue)
            .Select(x => x.입고요청Id!.Value)
            .Distinct()
            .ToArray();
        List<살뜰.도메인.창고.입고요청> 입고목록 = 입고요청Ids.Length == 0
            ? []
            : await _db.입고요청
                .AsNoTracking()
                .Where(x => 입고요청Ids.Contains(x.Id))
                .ToListAsync(cancellationToken);
        var 원장템플릿Key = 출고목록
            .Select(x => Clean(x.커뮤니티원장템플릿Key))
            .FirstOrDefault(x => x is not null)
            ?? ResolveSourceLedgerTemplateKey(운송실행투영.원본의뢰유형);

        await _음식마트원장동기화OutboxService.출고원장예약후즉시처리Async(
            출고목록,
            입고목록,
            변경자,
            $"transport-state:{traceId}:{운송실행투영.Id}:{운송실행투영.상태}",
            currentStageKey: 운송실행투영.상태,
            ledgerTemplateKey: 원장템플릿Key,
            cancellationToken: cancellationToken);
    }

    private static string ResolveSourceLedgerTemplateKey(string? 원본의뢰유형)
        => Clean(원본의뢰유형)?.Contains("Mart", StringComparison.OrdinalIgnoreCase) == true
            ? CommunityLedgerTemplateKeys.SsalddelMart
            : CommunityLedgerTemplateKeys.WarehouseOutbound;

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? Resolve변경블록Id(string? 현재단계)
    {
        var state = Clean(현재단계);
        if (state?.Contains("상차", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "pickup";
        }

        if (state?.Contains("하차", StringComparison.OrdinalIgnoreCase) == true
            || state?.Contains("인수완료", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "dropoff";
        }

        return null;
    }
}
