using Ssalddel.Contracts.Common.Education;
using Ssalddel.Services.Community;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Education;

public sealed class 교육기관제출Worker : BackgroundService
{
    private readonly I교육기관제출대기열 _대기열;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<교육기관제출Options> _options;
    private readonly ILogger<교육기관제출Worker> _logger;

    public 교육기관제출Worker(
        I교육기관제출대기열 대기열,
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<교육기관제출Options> options,
        ILogger<교육기관제출Worker> logger)
    {
        _대기열 = 대기열;
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;
            if (options.자동전송활성화)
            {
                try
                {
                    await ProcessPendingAsync(options, stoppingToken);
                }
                catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogError(ex, "교육기관 제출 대기열 처리 중 예외가 발생했습니다.");
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, options.조회주기초)), stoppingToken);
        }
    }

    private async Task ProcessPendingAsync(교육기관제출Options options, CancellationToken cancellationToken)
    {
        for (var processed = 0; processed < 20 && !cancellationToken.IsCancellationRequested; processed++)
        {
            var work = await _대기열.다음작업확보Async(cancellationToken);
            if (work is null)
            {
                return;
            }

            await using var scope = _scopeFactory.CreateAsyncScope();
            var ledgerStore = scope.ServiceProvider.GetRequiredService<I커뮤니티원장저장소>();
            var sender = scope.ServiceProvider.GetRequiredService<I교육기관제출전송Service>();
            var ledger = await ledgerStore.원장조회Async(work.원장Id, cancellationToken);
            if (ledger is null)
            {
                await _대기열.실패Async(
                    work.제출Id,
                    "제출 대상 현장 체험 활동 원장을 찾을 수 없습니다.",
                    설정대기: false,
                    options.최대시도횟수,
                    cancellationToken);
                continue;
            }

            var result = await sender.전송Async(work, ledger, cancellationToken);
            if (result.성공)
            {
                await _대기열.완료Async(work.제출Id, 교육기관제출상태.전송완료, cancellationToken);
                await ledgerStore.원장상태변경Async(
                    new 커뮤니티원장상태변경요청
                    {
                        원장Id = ledger.원장Id,
                        이전상태 = ledger.상태,
                        상태 = 현장체험활동상태.학교심사중,
                        현재단계Key = "school-review",
                        메모 = $"교육기관 {work.전송방식} 제출 완료"
                    },
                    "education-submission-worker",
                    cancellationToken);
                continue;
            }

            await _대기열.실패Async(
                work.제출Id,
                result.오류 ?? "교육기관 제출 전송에 실패했습니다.",
                result.설정필요,
                options.최대시도횟수,
                cancellationToken);
            _logger.LogWarning(
                "교육기관 제출 전송 실패. 제출Id={SubmissionId}, 원장Id={LedgerId}, 설정필요={ConfigurationRequired}",
                work.제출Id,
                work.원장Id,
                result.설정필요);
        }
    }
}
