using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 살뜰.Data;
using 살뜰.Services.Dispatch.Notification;
using 살뜰.Services.Dispatch.Queue;

namespace 살뜰.Services.Dispatch.Coordination;

public interface I국내화물배차조율적용Service
{
    Task<국내화물배차조율적용결과> 추천잠금적용Async(
        국내화물배차조율결과 조율결과,
        int? timeoutSeconds = null,
        int 기사최대수락운송건수 = 2,
        CancellationToken cancellationToken = default);
}

public sealed partial class 국내화물배차조율적용Service : I국내화물배차조율적용Service
{
    private readonly SsalddelContext _db;
    private readonly 배차큐정책Options _options;
    private readonly I배차추천알림Service _알림Service;
    private readonly I국내화물운송기사상태Service _기사상태Service;
    private readonly I배달권실행공간Store _배달권실행공간Store;
    private readonly ILogger<국내화물배차조율적용Service> _logger;

    public 국내화물배차조율적용Service(
        SsalddelContext db,
        IOptions<배차큐정책Options> options,
        I배차추천알림Service 알림Service,
        I국내화물운송기사상태Service 기사상태Service,
        I배달권실행공간Store 배달권실행공간Store,
        ILogger<국내화물배차조율적용Service> logger)
    {
        _db = db;
        _options = options.Value;
        _알림Service = 알림Service;
        _기사상태Service = 기사상태Service;
        _배달권실행공간Store = 배달권실행공간Store;
        _logger = logger;
    }

    public async Task<국내화물배차조율적용결과> 추천잠금적용Async(
        국내화물배차조율결과 조율결과,
        int? timeoutSeconds = null,
        int 기사최대수락운송건수 = 2,
        CancellationToken cancellationToken = default)
    {
        var 잠금목록 = new List<국내화물배차추천잠금>();
        var 실패목록 = new List<국내화물배차추천잠금실패>();

        foreach (var 배차제안 in 조율결과.추천배정목록)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var 적용된잠금 = await 추천잠금시도Async(
                    배차제안,
                    timeoutSeconds,
                    Math.Max(1, 기사최대수락운송건수),
                    cancellationToken);
                if (적용된잠금 is null)
                {
                    실패목록.Add(new 국내화물배차추천잠금실패(
                        배차제안.의뢰Id,
                        배차제안.기사Id,
                        "이미 추천중이거나 확정되어 잠금 대상에서 제외되었습니다."));
                }
                else
                {
                    잠금목록.Add(적용된잠금);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogInformation(ex, "다량 배차 추천 잠금 중 동시성 충돌이 발생했습니다. RequestId={RequestId} DriverId={DriverId}", 배차제안.의뢰Id, 배차제안.기사Id);
                실패목록.Add(new 국내화물배차추천잠금실패(
                    배차제안.의뢰Id,
                    배차제안.기사Id,
                    "다른 배차 루프가 먼저 추천 또는 확정했습니다."));
            }
        }

        return new 국내화물배차조율적용결과(DateTime.UtcNow, 잠금목록, 실패목록);
    }

}
