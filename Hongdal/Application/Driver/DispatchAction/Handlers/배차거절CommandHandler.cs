using Hongdal.Application.CommandProcessing;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.공통;

namespace Hongdal.Application.Driver.DispatchAction;

public sealed class 배차거절CommandHandler : IRequestHandler<배차거절Command, FluentResults.Result<배차거절결과>>
{
    private readonly HongdalContext _db;
    private readonly IDriverRejectedRequestStore _rejectedRequestStore;
    private readonly IPublisher _publisher;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly I참여자실행권한검사 _권한검사;
    private readonly ILogger<배차거절CommandHandler> _logger;

    public 배차거절CommandHandler(
        HongdalContext db,
        IDriverRejectedRequestStore rejectedRequestStore,
        IPublisher publisher,
        ICurrentUserAccessor currentUserAccessor,
        I참여자실행권한검사 권한검사,
        ILogger<배차거절CommandHandler> logger)
    {
        _db = db;
        _rejectedRequestStore = rejectedRequestStore;
        _publisher = publisher;
        _currentUserAccessor = currentUserAccessor;
        _권한검사 = 권한검사;
        _logger = logger;
    }

    public async Task<FluentResults.Result<배차거절결과>> Handle(배차거절Command request, CancellationToken cancellationToken)
    {
        if (!_권한검사.Try검증(_currentUserAccessor.UserId, _currentUserAccessor.Role, request.참여자Id, request.실행역할, out var 권한오류))
        {
            return FluentResults.Result.Fail<배차거절결과>(권한오류);
        }

        if (string.IsNullOrWhiteSpace(request.기사Id))
        {
            return FluentResults.Result.Fail<배차거절결과>("기사 인증 정보가 없습니다.");
        }

        if (string.IsNullOrWhiteSpace(request.RequestId))
        {
            return FluentResults.Result.Fail<배차거절결과>("의뢰Id는 필수입니다.");
        }

        var now = DateTime.UtcNow;
        var queue = await _db.배차대기
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.의뢰Id == request.RequestId, cancellationToken);
        if (queue is null)
        {
            return FluentResults.Result.Fail<배차거절결과>("배차대기 데이터를 찾을 수 없습니다.");
        }

        var canRejectRecommendation = queue.상태 == 상태값.배차대기상태.대기
                                      && queue.배차큐단계 == 상태값.배차큐단계.배차추천
                                      && queue.배차노출상태 == 상태값.배차노출상태.추천중
                                      && string.Equals(queue.현재추천대상기사Id, request.기사Id, StringComparison.Ordinal)
                                      && queue.추천만료시각.HasValue
                                      && queue.추천만료시각 > now;
        if (!canRejectRecommendation)
        {
            return FluentResults.Result.Fail<배차거절결과>("거절 가능한 추천 배차가 아닙니다.");
        }

        await _rejectedRequestStore.RejectAsync(request.기사Id, request.RequestId, cancellationToken);

        try
        {
            await _publisher.Publish(
                new 배차거절됨Event(
                    request.기사Id,
                    request.RequestId,
                    request.사유,
                    now,
                    System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "배차거절 사후처리 이벤트 발행 중 예외가 발생했습니다. RequestId={RequestId}", request.RequestId);
        }

        return FluentResults.Result.Ok(new 배차거절결과(request.RequestId, "거절되었습니다."));
    }
}
