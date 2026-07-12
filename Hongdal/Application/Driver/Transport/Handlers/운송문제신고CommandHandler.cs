using System.Diagnostics;
using Hongdal.Contracts.Driver.Transport;
using FluentResults;
using Hongdal.Application.CommandProcessing;
using MediatR;

namespace Hongdal.Application.Driver.Transport;

public sealed class 운송문제신고CommandHandler : IRequestHandler<운송문제신고Command, Result<기사운송요약응답>>
{
    private readonly HongdalContext _db;
    private readonly IPublisher _publisher;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;
    private readonly I운송증빙첨부JsonWriter _attachmentWriter;
    private readonly ILogger<운송문제신고CommandHandler> _logger;

    public 운송문제신고CommandHandler(
        HongdalContext db,
        IPublisher publisher,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사,
        I운송증빙첨부JsonWriter attachmentWriter,
        ILogger<운송문제신고CommandHandler> logger)
    {
        _db = db;
        _publisher = publisher;
        _currentUserAccessor = currentUserAccessor;
        _권한검사 = 권한검사;
        _attachmentWriter = attachmentWriter;
        _logger = logger;
    }

    public async Task<Result<기사운송요약응답>> Handle(운송문제신고Command request, CancellationToken cancellationToken)
    {
        if (!_권한검사.Try검증(_currentUserAccessor.UserId, _currentUserAccessor.Role, request.참여자Id, request.실행역할, out var 권한오류))
        {
            return Result.Fail<기사운송요약응답>(권한오류);
        }

        var entity = await _db.운송원장
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.기사_운송자 == request.기사Id, cancellationToken);

        if (entity is null)
        {
            return Result.Fail<기사운송요약응답>("운송을 찾을 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        var 예외 = 운송현장예외정책.정리(
            request.단계,
            request.예외코드,
            request.사유,
            request.관리자확인요청);
        var memo = BuildMemoLine(예외, request.메모);
        entity.메모 = string.IsNullOrWhiteSpace(entity.메모) ? memo : $"{entity.메모}\n{memo}";
        _attachmentWriter.추가(
            entity,
            new 운송증빙첨부(
                "transport-field-exception",
                request.증빙ObjectName,
                request.증빙Url,
                request.기사Id,
                now,
                new Dictionary<string, object?>
                {
                    ["stage"] = 예외.단계,
                    ["exceptionCode"] = 예외.예외코드,
                    ["reason"] = 예외.사유,
                    ["memo"] = request.메모?.Trim(),
                    ["nextAction"] = 예외.다음행동안내,
                    ["adminReviewRequired"] = 예외.관리자확인필요,
                    ["adminReviewRequested"] = request.관리자확인요청,
                    ["transportStatus"] = entity.상태,
                    ["traceId"] = Activity.Current?.TraceId.ToString()
                }));
        entity.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);
        await PublishAfterCommitAsync(entity, 예외, request, now, cancellationToken);

        return Result.Ok(new 기사운송요약응답
        {
            Id = entity.Id,
            운송번호 = entity.운송번호,
            상태 = entity.상태,
            출발지 = entity.출발지,
            도착지 = entity.도착지,
            기사_운송자 = entity.기사_운송자,
            출발_픽업 = entity.출발_픽업,
            도착 = entity.도착,
            운임 = entity.운임,
            예외신고됨 = true,
            최근예외단계 = 예외.단계,
            최근예외코드 = 예외.예외코드,
            최근예외메시지 = 예외.사유,
            다음행동안내 = 예외.다음행동안내,
            관리자확인필요 = 예외.관리자확인필요,
            UpdatedAt = entity.UpdatedAt
        });
    }

    private static string BuildMemoLine(운송현장예외정리결과 예외, string? requestMemo)
    {
        var memo = string.IsNullOrWhiteSpace(requestMemo) ? string.Empty : $" 메모={requestMemo.Trim()}";
        var admin = 예외.관리자확인필요 ? " 관리자확인필요" : string.Empty;
        return $"[운송예외][{예외.단계}][{예외.예외코드}] {예외.사유}{memo}{admin}";
    }

    private async Task PublishAfterCommitAsync(
        운송원장 entity,
        운송현장예외정리결과 예외,
        운송문제신고Command request,
        DateTime now,
        CancellationToken cancellationToken)
    {
        try
        {
            await _publisher.Publish(
                new 운송문제신고됨Event(
                    request.기사Id,
                    entity.Id,
                    entity.운송번호,
                    예외.단계,
                    예외.예외코드,
                    예외.사유,
                    request.메모,
                    request.증빙ObjectName,
                    request.증빙Url,
                    예외.관리자확인필요,
                    now,
                    Activity.Current?.TraceId.ToString() ?? string.Empty),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "운송 문제 신고 사후처리 이벤트 발행 중 예외가 발생했습니다. TransportId={TransportId}",
                entity.Id);
        }
    }
}
