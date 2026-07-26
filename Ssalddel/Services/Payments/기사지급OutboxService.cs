using Microsoft.EntityFrameworkCore;
using 살뜰.Data;
using 살뜰.Services.Options;
using 살뜰.도메인.기사;
using 살뜰.도메인.설정;

namespace 살뜰.Services.Payments;

public sealed record 기사지급Gateway결과(
    bool Success,
    bool Retryable,
    string ResultCode,
    string Message)
{
    public static 기사지급Gateway결과 성공(string code, string message)
        => new(true, false, code, message);

    public static 기사지급Gateway결과 재시도(string code, string message)
        => new(false, true, code, message);

    public static 기사지급Gateway결과 차단(string code, string message)
        => new(false, false, code, message);
}

public interface I기사지급Gateway
{
    Task<기사지급Gateway결과> 처리Async(
        기사운송대금지급요청 request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 실제 지급 Provider가 연결되기 전의 안전 경계입니다.
/// Simulation에서는 송금 없이 처리 가능성만 검증하고, Operational에서는 명시적으로 차단합니다.
/// </summary>
public sealed class 준비전용기사지급Gateway(
    ISsalddelExecutionModePolicy executionMode) : I기사지급Gateway
{
    public Task<기사지급Gateway결과> 처리Async(
        기사운송대금지급요청 request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(executionMode.IsSimulation
            ? 기사지급Gateway결과.성공(
                "SimulationNoTransfer",
                "Simulation에서 승인·계좌·금액 경계를 검증했습니다. 실제 송금은 실행하지 않았습니다.")
            : 기사지급Gateway결과.차단(
                "OperationalProviderNotConfigured",
                "Operational 지급 Provider가 구성되지 않아 실제 송금을 실행하지 않았습니다."));
    }
}

public interface I기사지급OutboxService
{
    Task<int> 대기항목처리Async(
        int take = 100,
        CancellationToken cancellationToken = default);
}

public sealed class 기사지급OutboxService : I기사지급OutboxService
{
    private readonly SsalddelContext _db;
    private readonly I기사지급Gateway _gateway;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<기사지급OutboxService> _logger;

    public 기사지급OutboxService(
        SsalddelContext db,
        I기사지급Gateway gateway,
        TimeProvider timeProvider,
        ILogger<기사지급OutboxService> logger)
    {
        _db = db;
        _gateway = gateway;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<int> 대기항목처리Async(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var eligibleStatuses = new List<string>
        {
            기사지급Outbox상태코드.대기,
            기사지급Outbox상태코드.재시도대기
        };
        var items = await _db.기사지급Outbox
            .Where(x =>
                eligibleStatuses.Contains(x.처리상태)
                && (!x.다음시도시각Utc.HasValue || x.다음시도시각Utc <= now))
            .OrderBy(x => x.CreatedAtUtc)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var outbox in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = await _db.기사운송대금지급요청
                .SingleOrDefaultAsync(x => x.Id == outbox.기사지급요청Id, cancellationToken);
            if (request is null)
            {
                처리차단(
                    outbox,
                    now,
                    "PayoutRequestMissing",
                    "기사 지급 승인 원본을 찾을 수 없습니다.");
                processed++;
                continue;
            }

            outbox.시도횟수 += 1;
            outbox.마지막시도시각Utc = now;
            outbox.UpdatedAtUtc = now;
            processed++;

            try
            {
                var result = await _gateway.처리Async(request, cancellationToken);
                request.마지막처리코드 = result.ResultCode;
                request.마지막처리메시지 = result.Message;
                request.UpdatedAtUtc = now;
                outbox.마지막결과코드 = result.ResultCode;
                outbox.마지막오류메시지 = result.Success ? string.Empty : result.Message;

                if (result.Success)
                {
                    // 현재 기본 Gateway의 성공은 송금이 아니라 Simulation 검증 완료입니다.
                    request.상태코드 = 기사지급요청상태코드.Simulation검증완료;
                    request.Simulation검증일시Utc = now;
                    outbox.처리상태 = 기사지급Outbox상태코드.Simulation검증완료;
                    outbox.다음시도시각Utc = null;
                }
                else if (result.Retryable)
                {
                    재시도예약(outbox, request, now, result.ResultCode, result.Message);
                }
                else
                {
                    처리차단(outbox, now, result.ResultCode, result.Message);
                    request.상태코드 = 기사지급요청상태코드.운영Provider미구성;
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                재시도예약(
                    outbox,
                    request,
                    now,
                    "UnexpectedGatewayFailure",
                    "지급 경계 처리 중 일시적 오류가 발생했습니다.");
                _logger.LogWarning(
                    exception,
                    "기사 지급 Outbox 처리 실패. OutboxId={OutboxId} PayoutRequestId={PayoutRequestId} Attempt={Attempt}",
                    outbox.Id,
                    outbox.기사지급요청Id,
                    outbox.시도횟수);
            }
        }

        if (processed > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return processed;
    }

    private static void 재시도예약(
        기사지급Outbox outbox,
        기사운송대금지급요청 request,
        DateTime now,
        string code,
        string message)
    {
        var delayMinutes = Math.Min(60, Math.Pow(2, Math.Min(outbox.시도횟수, 6)));
        outbox.처리상태 = 기사지급Outbox상태코드.재시도대기;
        outbox.다음시도시각Utc = now.AddMinutes(delayMinutes);
        outbox.마지막결과코드 = code;
        outbox.마지막오류메시지 = message;
        request.상태코드 = 기사지급요청상태코드.재시도대기;
        request.마지막처리코드 = code;
        request.마지막처리메시지 = message;
        request.UpdatedAtUtc = now;
    }

    private static void 처리차단(
        기사지급Outbox outbox,
        DateTime now,
        string code,
        string message)
    {
        outbox.처리상태 = 기사지급Outbox상태코드.운영Provider미구성;
        outbox.다음시도시각Utc = null;
        outbox.마지막결과코드 = code;
        outbox.마지막오류메시지 = message;
        outbox.UpdatedAtUtc = now;
    }
}
