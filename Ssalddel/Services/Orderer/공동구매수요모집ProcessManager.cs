using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using 살뜰.Services.Options;
using 살뜰.Services.Versioning;

namespace Ssalddel.Services.Orderer;

public interface I공동구매수요모집ProcessManager
{
    Task<공동구매자동집단응답> 수요등록조율Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default);

    Task<공동구매자동수요철회응답> 수요철회조율Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken = default);

    Task<공동구매수요모집Os조율응답> 집단조율Async(
        string 자동집단Id,
        string 트리거코드,
        DateTime? 기준시각Utc = null,
        CancellationToken cancellationToken = default);

    Task<공동구매수요모집마감스캔응답> 모집마감스캔Async(
        DateTime? 기준시각Utc = null,
        int? 최대건수 = null,
        CancellationToken cancellationToken = default);

    Task<공동구매수요모집Os상태응답?> 운영상태조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동구매수요모집인계승인응답> 인계승인Async(
        string 자동집단Id,
        공동구매수요모집인계승인요청 요청,
        string 승인자키,
        CancellationToken cancellationToken = default);

    Task<공동구매수요모집Os상태응답> 후속원장연결Async(
        string 자동집단Id,
        string 인계요청Id,
        string 대상원장Id,
        CancellationToken cancellationToken = default);
}

public interface I공동구매수요모집ProcessStore
{
    Task<공동구매수요모집Os조율응답> 운영조율Async(
        string 자동집단Id,
        string 트리거코드,
        string 조율멱등키,
        IReadOnlyList<string> 정책코드목록,
        DateTime 기준시각Utc,
        TimeSpan 장기모집점검주기,
        string 실행모드,
        bool 후속워크플로우활성여부,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> 운영점검대상조회Async(
        DateTime 기준시각Utc,
        int 최대건수,
        CancellationToken cancellationToken);

    Task<공동구매수요모집Os상태응답?> 운영상태조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken);

    Task<공동구매수요모집인계승인응답> 인계승인Async(
        string 자동집단Id,
        공동구매수요모집인계승인요청 요청,
        string 승인자키,
        DateTime 승인시각Utc,
        string 실행모드,
        bool 후속워크플로우활성여부,
        CancellationToken cancellationToken);

    Task<공동구매수요모집Os상태응답> 후속원장연결Async(
        string 자동집단Id,
        string 인계요청Id,
        string 대상원장Id,
        DateTime 연결시각Utc,
        CancellationToken cancellationToken);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Application,
    "수요 변경, 모집 마감, 검토 큐와 사람 승인 인계를 하나의 공동구매 모집 원장 순서로 조율합니다.",
    ContractType = typeof(I공동구매수요모집ProcessManager),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "엔진 판단을 직접 확정하지 않고 상태전이 Port에 저장을 위임하며, 승인 뒤에도 1.5 원장이나 외부 실행을 자동 생성하지 않습니다.")]
internal sealed class 공동구매수요모집ProcessManager : I공동구매수요모집ProcessManager
{
    private readonly I공동구매자동집단화저장소 _수요명령Port;
    private readonly I공동구매수요모집ProcessStore _상태전이Port;
    private readonly IVersionFeatureFlagService _기능플래그;
    private readonly ISsalddelExecutionModePolicy _실행모드;
    private readonly TimeProvider _시각;
    private readonly IOptionsMonitor<GroupPurchaseDemandProcessManagerOptions> _options;
    private readonly ILogger<공동구매수요모집ProcessManager> _logger;

    public 공동구매수요모집ProcessManager(
        I공동구매자동집단화저장소 수요명령Port,
        I공동구매수요모집ProcessStore 상태전이Port,
        IVersionFeatureFlagService 기능플래그,
        ISsalddelExecutionModePolicy 실행모드,
        TimeProvider 시각,
        IOptionsMonitor<GroupPurchaseDemandProcessManagerOptions> options,
        ILogger<공동구매수요모집ProcessManager>? logger = null)
    {
        _수요명령Port = 수요명령Port;
        _상태전이Port = 상태전이Port;
        _기능플래그 = 기능플래그;
        _실행모드 = 실행모드;
        _시각 = 시각;
        _options = options;
        _logger = logger ?? NullLogger<공동구매수요모집ProcessManager>.Instance;
    }

    public async Task<공동구매자동집단응답> 수요등록조율Async(
        공동구매자동수요등록Command command,
        CancellationToken cancellationToken = default)
    {
        var 집단 = await _수요명령Port.수요등록Async(command, cancellationToken);
        var 조율 = await 집단조율내부Async(
            집단.자동집단Id,
            공동구매수요모집Os트리거코드.수요변경,
            조율멱등키(command.요청멱등키, 공동구매수요모집Os트리거코드.수요변경),
            null,
            cancellationToken: cancellationToken);
        return 조율.집단;
    }

    public async Task<공동구매자동수요철회응답> 수요철회조율Async(
        공동구매자동수요철회Command command,
        CancellationToken cancellationToken = default)
    {
        var 철회 = await _수요명령Port.수요철회Async(command, cancellationToken);
        await 집단조율내부Async(
            철회.자동집단Id,
            공동구매수요모집Os트리거코드.수요철회,
            조율멱등키(command.요청멱등키, 공동구매수요모집Os트리거코드.수요철회),
            null,
            cancellationToken: cancellationToken);
        return 철회;
    }

    public Task<공동구매수요모집Os조율응답> 집단조율Async(
        string 자동집단Id,
        string 트리거코드,
        DateTime? 기준시각Utc = null,
        CancellationToken cancellationToken = default)
        => 집단조율내부Async(
            자동집단Id,
            트리거코드,
            string.Empty,
            기준시각Utc,
            cancellationToken);

    private Task<공동구매수요모집Os조율응답> 집단조율내부Async(
        string 자동집단Id,
        string 트리거코드,
        string 조율멱등키,
        DateTime? 기준시각Utc,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(트리거코드);

        var now = Utc시각(기준시각Utc ?? _시각.GetUtcNow().UtcDateTime);
        return _상태전이Port.운영조율Async(
            자동집단Id.Trim(),
            트리거코드.Trim(),
            조율멱등키,
            적용정책(트리거코드),
            now,
            장기모집점검주기(),
            _실행모드.Mode.ToString(),
            후속워크플로우활성여부(),
            cancellationToken);
    }

    public async Task<공동구매수요모집마감스캔응답> 모집마감스캔Async(
        DateTime? 기준시각Utc = null,
        int? 최대건수 = null,
        CancellationToken cancellationToken = default)
    {
        var now = Utc시각(기준시각Utc ?? _시각.GetUtcNow().UtcDateTime);
        var limit = Math.Clamp(최대건수 ?? _options.CurrentValue.BatchSize, 1, 1000);
        var 대상목록 = await _상태전이Port.운영점검대상조회Async(now, limit, cancellationToken);
        var 결과 = new 공동구매수요모집마감스캔응답
        {
            기준시각Utc = now,
            조회건수 = 대상목록.Count
        };

        foreach (var 자동집단Id in 대상목록)
        {
            try
            {
                var 조율 = await 집단조율Async(
                    자동집단Id,
                    공동구매수요모집Os트리거코드.모집마감점검,
                    now,
                    cancellationToken);
                결과.조율건수++;
                if (조율.운영상태.현재큐 == 공동구매수요모집Os큐코드.확정검토)
                {
                    결과.확정검토건수++;
                }
                else if (조율.운영상태.현재큐 == 공동구매수요모집Os큐코드.모집종료)
                {
                    결과.모집종료건수++;
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                결과.실패건수++;
                _logger.LogWarning(
                    ex,
                    "공동구매 수요·모집 프로세스 점검에 실패했습니다. AutoGroupId={AutoGroupId}",
                    자동집단Id);
            }
        }

        return 결과;
    }

    public async Task<공동구매수요모집Os상태응답?> 운영상태조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        var 상태 = await _상태전이Port.운영상태조회Async(자동집단Id.Trim(), cancellationToken);
        if (상태 is not null)
        {
            상태.실행모드 = _실행모드.Mode.ToString();
            상태.시뮬레이션여부 = _실행모드.IsSimulation;
            상태.후속워크플로우활성여부 = 후속워크플로우활성여부();
        }

        return 상태;
    }

    public Task<공동구매수요모집인계승인응답> 인계승인Async(
        string 자동집단Id,
        공동구매수요모집인계승인요청 요청,
        string 승인자키,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentNullException.ThrowIfNull(요청);
        ArgumentException.ThrowIfNullOrWhiteSpace(승인자키);
        if (string.IsNullOrWhiteSpace(요청.요청멱등키))
        {
            throw new InvalidOperationException("공동구매 모집 결과 인계 승인에는 요청 멱등 키가 필요합니다.");
        }

        if (요청.요청멱등키.Trim().Length > 160)
        {
            throw new InvalidOperationException("요청 멱등 키는 160자 이하여야 합니다.");
        }

        return _상태전이Port.인계승인Async(
            자동집단Id.Trim(),
            요청,
            승인자키.Trim(),
            _시각.GetUtcNow().UtcDateTime,
            _실행모드.Mode.ToString(),
            후속워크플로우활성여부(),
            cancellationToken);
    }

    public Task<공동구매수요모집Os상태응답> 후속원장연결Async(
        string 자동집단Id,
        string 인계요청Id,
        string 대상원장Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(인계요청Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(대상원장Id);
        if (!후속워크플로우활성여부())
        {
            throw new InvalidOperationException("1.5 공급·가격·무역 준비 기능이 비활성 상태입니다.");
        }

        return _상태전이Port.후속원장연결Async(
            자동집단Id.Trim(),
            인계요청Id.Trim(),
            대상원장Id.Trim(),
            _시각.GetUtcNow().UtcDateTime,
            cancellationToken);
    }

    private static IReadOnlyList<string> 적용정책(string 트리거코드)
        => 트리거코드 switch
        {
            공동구매수요모집Os트리거코드.수요변경 or
            공동구매수요모집Os트리거코드.수요철회 =>
            [공동구매수요모집Os정책코드.수요집단화묶음],
            공동구매수요모집Os트리거코드.모집마감점검 =>
            [
                공동구매수요모집Os정책코드.모집마감우선,
                공동구매수요모집Os정책코드.장기모집정체보정
            ],
            _ => [공동구매수요모집Os정책코드.수요집단화묶음]
        };

    private TimeSpan 장기모집점검주기()
        => TimeSpan.FromHours(Math.Clamp(_options.CurrentValue.AgingReviewHours, 1, 24 * 30));

    private bool 후속워크플로우활성여부()
        => _기능플래그.IsEnabled(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow);

    private static string 조율멱등키(string? 요청멱등키, string 트리거코드)
        => string.IsNullOrWhiteSpace(요청멱등키)
            ? string.Empty
            : $"{트리거코드}:{요청멱등키.Trim()}";

    private static DateTime Utc시각(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandProcessManager,
    SsalddelCodeLayer.Infrastructure,
    "기능 플래그가 켜진 동안 모집 마감과 장기 정체 집단을 주기적으로 프로세스 점검 대상으로 조회합니다.",
    FlowOrder = 50,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "1.0 모집 원장만 재계산하며 주문, 결제, 공급자 선정 또는 1.5 원장을 자동 생성하지 않습니다.")]
public sealed class 공동구매수요모집DeadlineScanBackgroundService : BackgroundService
{
    private readonly I공동구매수요모집ProcessManager _processManager;
    private readonly IVersionFeatureFlagService _기능플래그;
    private readonly IOptionsMonitor<GroupPurchaseDemandProcessManagerOptions> _options;
    private readonly ILogger<공동구매수요모집DeadlineScanBackgroundService> _logger;

    public 공동구매수요모집DeadlineScanBackgroundService(
        I공동구매수요모집ProcessManager processManager,
        IVersionFeatureFlagService 기능플래그,
        IOptionsMonitor<GroupPurchaseDemandProcessManagerOptions> options,
        ILogger<공동구매수요모집DeadlineScanBackgroundService> logger)
    {
        _processManager = processManager;
        _기능플래그 = 기능플래그;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;
            try
            {
                if (options.Enabled
                    && _기능플래그.IsEnabled(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow))
                {
                    var result = await _processManager.모집마감스캔Async(
                        최대건수: options.BatchSize,
                        cancellationToken: stoppingToken);
                    if (result.조율건수 > 0 || result.실패건수 > 0)
                    {
                        _logger.LogInformation(
                            "공동구매 수요·모집 프로세스 점검 완료. Scanned={Scanned}, Coordinated={Coordinated}, Review={Review}, Closed={Closed}, Failed={Failed}",
                            result.조회건수,
                            result.조율건수,
                            result.확정검토건수,
                            result.모집종료건수,
                            result.실패건수);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "공동구매 수요·모집 background 점검 중 예외가 발생했습니다.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Clamp(options.ScanIntervalSeconds, 10, 3600)),
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
