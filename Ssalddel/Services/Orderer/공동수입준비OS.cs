using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using 살뜰.Services.Options;
using 살뜰.Services.Versioning;

namespace Ssalddel.Services.Orderer;

public interface I공동수입준비OS
{
    Task<공동수입준비Os상태응답?> 운영상태조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동수입준비Os상태응답> 작업실행Async(
        string 자동집단Id,
        공동수입준비Os작업실행요청 요청,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);

    Task<공동수입준비Os상태응답> 전문검토인계Async(
        string 자동집단Id,
        공동수입준비Os전문검토인계요청 요청,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);

    Task<공동수입준비Os정기점검응답> 정기점검Async(
        int? 최대건수 = null,
        CancellationToken cancellationToken = default);
}

internal static class 공동수입준비Os원장상태저장정책
{
    public const string BlockId = "trade-readiness-os-state";
    public const string 준비자료BlockId = "trade-readiness-request";
    public const string JsonKey = "Json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static 공동수입준비Os영속상태 읽기(커뮤니티원장Dto ledger)
    {
        var json = ledger.블록목록
            .FirstOrDefault(block => string.Equals(block.BlockId, BlockId, StringComparison.OrdinalIgnoreCase))?
            .Data.GetValueOrDefault(JsonKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new 공동수입준비Os영속상태();
        }

        try
        {
            return JsonSerializer.Deserialize<공동수입준비Os영속상태>(json, JsonOptions)
                   ?? new 공동수입준비Os영속상태();
        }
        catch (JsonException)
        {
            return new 공동수입준비Os영속상태
            {
                마지막오류 = "저장된 1.5 OS 상태를 읽지 못해 원장 근거로 상태를 다시 구성했습니다."
            };
        }
    }

    public static 공동수입준비원장저장요청 준비자료읽기(커뮤니티원장Dto ledger)
    {
        var json = ledger.블록목록
            .FirstOrDefault(block => string.Equals(block.BlockId, 준비자료BlockId, StringComparison.OrdinalIgnoreCase))?
            .Data.GetValueOrDefault(JsonKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("공동수입 원장에 1.5 준비 자료 원본 블록이 없습니다.");
        }

        return JsonSerializer.Deserialize<공동수입준비원장저장요청>(json, JsonOptions)
               ?? throw new InvalidOperationException("공동수입 원장의 1.5 준비 자료를 읽을 수 없습니다.");
    }

    public static 커뮤니티원장저장요청 저장요청(
        커뮤니티원장Dto ledger,
        공동수입준비Os영속상태 state,
        string 상태코드)
    {
        var blocks = ledger.블록목록
            .Where(block => !string.Equals(block.BlockId, BlockId, StringComparison.OrdinalIgnoreCase))
            .Append(new 커뮤니티원장블록Dto
            {
                BlockId = BlockId,
                BlockType = CommunityLedgerBlockTypes.Generic,
                Title = "1.5 공동수입 준비 OS 상태",
                State = 상태코드,
                Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [JsonKey] = JsonSerializer.Serialize(state, JsonOptions),
                    ["ExecutionBoundary"] = "NoContractNoPaymentNoFilingNoTransport"
                }
            })
            .ToArray();

        return new 커뮤니티원장저장요청
        {
            원장Id = ledger.원장Id,
            기대Revision = ledger.Revision,
            커뮤니티Id = ledger.커뮤니티Id,
            원장템플릿Key = ledger.원장템플릿Key,
            제목 = ledger.제목,
            원함 = ledger.원함,
            상태 = ledger.상태,
            현재단계Key = ledger.현재단계Key,
            대상OsCode = ledger.대상OsCode,
            대상OsName = ledger.대상OsName,
            생성자UserId = ledger.생성자UserId,
            생성자표시명 = ledger.생성자표시명,
            블록목록 = blocks,
            블록담당자명시적갱신여부 = true,
            참여자목록 = ledger.참여자목록,
            포함원장목록 = ledger.포함원장목록,
            다이어그램스냅샷 = ledger.다이어그램스냅샷,
            외부참조 = ledger.외부참조,
            확장속성 = ledger.확장속성
        };
    }
}

internal sealed class 공동수입준비Os영속상태
{
    public string 마지막트리거코드 { get; set; } = 공동수입준비Os트리거코드.원장조회;
    public string 마지막조율자표시명 { get; set; } = string.Empty;
    public DateTimeOffset? 마지막조율시각Utc { get; set; }
    public DateTimeOffset? 다음점검시각Utc { get; set; }
    public string 마지막오류 { get; set; } = string.Empty;
    public 공동수입준비Os전문검토인계기록? 전문검토인계기록 { get; set; }
    public Dictionary<string, 공동수입준비Os작업실행기록> 작업실행기록 { get; set; } = new(StringComparer.Ordinal);
    public List<공동수입준비Os명령기록> 최근명령목록 { get; set; } = [];
}

internal sealed class 공동수입준비Os작업실행기록
{
    public int 시도횟수 { get; set; }
    public DateTimeOffset? 마지막실행시각Utc { get; set; }
    public string 마지막오류 { get; set; } = string.Empty;
}

internal sealed class 공동수입준비Os명령기록
{
    public string 요청멱등키 { get; set; } = string.Empty;
    public string 요청지문 { get; set; } = string.Empty;
    public string 명령유형 { get; set; } = string.Empty;
    public DateTimeOffset 처리시각Utc { get; set; }
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "기존 공동수입 원장의 1.5 준비 블록에서 완성도·최신성·포워더 인계·회신·전문검토 상태를 작업별로 조율합니다.",
    ContractType = typeof(I공동수입준비OS),
    FlowOrder = 32,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "공유 공공데이터 배치는 등록 상태만 참조하며 포워더 자동 선정, 외부 자동 전송, 계약·결제·신고·운송·창고 실행 또는 별도 공동수입 원장 생성을 수행하지 않습니다.")]
public sealed class 공동수입준비OS : I공동수입준비OS
{
    private const string 수동명령유형 = "WorkloadRun";
    private const string 전문검토인계명령유형 = "QualifiedReviewHandoff";

    private readonly I공동구매자동집단화저장소 _groupStore;
    private readonly I커뮤니티원장저장소 _ledgerStore;
    private readonly I공동구매수요모집Os배치Catalog _sharedBatchCatalog;
    private readonly IVersionFeatureFlagService _featureFlags;
    private readonly ISsalddelExecutionModePolicy _executionMode;
    private readonly IOptionsMonitor<GroupImportReadinessOsOptions> _options;
    private readonly TimeProvider _timeProvider;

    public 공동수입준비OS(
        I공동구매자동집단화저장소 groupStore,
        I커뮤니티원장저장소 ledgerStore,
        I공동구매수요모집Os배치Catalog sharedBatchCatalog,
        IVersionFeatureFlagService featureFlags,
        ISsalddelExecutionModePolicy executionMode,
        IOptionsMonitor<GroupImportReadinessOsOptions> options,
        TimeProvider timeProvider)
    {
        _groupStore = groupStore;
        _ledgerStore = ledgerStore;
        _sharedBatchCatalog = sharedBatchCatalog;
        _featureFlags = featureFlags;
        _executionMode = executionMode;
        _options = options;
        _timeProvider = timeProvider;
    }

    public async Task<공동수입준비Os상태응답?> 운영상태조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        var context = await Context조회Async(자동집단Id.Trim(), cancellationToken);
        return context is null
            ? null
            : 상태구성(context, 이미처리됨: false);
    }

    public async Task<공동수입준비Os상태응답> 작업실행Async(
        string 자동집단Id,
        공동수입준비Os작업실행요청 요청,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentNullException.ThrowIfNull(요청);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorDisplayName);
        기능활성검증();
        멱등키검증(요청.요청멱등키);

        var workloadCode = string.IsNullOrWhiteSpace(요청.작업코드)
            ? 공동수입준비Os작업코드.전체준비점검
            : 요청.작업코드.Trim();
        if (!공동수입준비Os작업코드.수동실행지원목록.Contains(workloadCode))
        {
            throw new ArgumentException($"수동 실행을 지원하지 않는 1.5 OS 작업입니다. WorkloadCode={workloadCode}");
        }

        var context = await Context조회Async(자동집단Id.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException("점검할 1.5 준비 블록이 있는 공동수입 원장을 찾을 수 없습니다.");
        var fingerprint = 지문($"{수동명령유형}|{workloadCode}|{요청.재시도여부}");
        var previousCommand = context.State.최근명령목록.FirstOrDefault(item =>
            string.Equals(item.요청멱등키, 요청.요청멱등키.Trim(), StringComparison.Ordinal));
        if (previousCommand is not null)
        {
            동일멱등요청검증(previousCommand, fingerprint);
            return 상태구성(context, 이미처리됨: true);
        }

        기대Revision검증(요청.기대Revision, context.Ledger.Revision);
        var now = _timeProvider.GetUtcNow();
        var targetCodes = workloadCode == 공동수입준비Os작업코드.전체준비점검
            ? 공동수입준비Os작업코드.수동실행지원목록
                .Where(code => code != 공동수입준비Os작업코드.전체준비점검)
                .ToArray()
            : [workloadCode];
        foreach (var targetCode in targetCodes)
        {
            var execution = context.State.작업실행기록.GetValueOrDefault(targetCode)
                            ?? new 공동수입준비Os작업실행기록();
            execution.시도횟수++;
            execution.마지막실행시각Utc = now;
            execution.마지막오류 = string.Empty;
            context.State.작업실행기록[targetCode] = execution;
        }

        context.State.마지막트리거코드 = 요청.재시도여부
            ? 공동수입준비Os트리거코드.수동재시도
            : 공동수입준비Os트리거코드.수동점검;
        context.State.마지막조율자표시명 = actorDisplayName.Trim();
        context.State.마지막조율시각Utc = now;
        context.State.다음점검시각Utc = 다음점검시각(now);
        명령기록(context.State, 요청.요청멱등키, fingerprint, 수동명령유형, now);

        return await 상태저장Async(context, actorUserId.Trim(), cancellationToken);
    }

    public async Task<공동수입준비Os상태응답> 전문검토인계Async(
        string 자동집단Id,
        공동수입준비Os전문검토인계요청 요청,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentNullException.ThrowIfNull(요청);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorDisplayName);
        기능활성검증();
        멱등키검증(요청.요청멱등키);
        if (string.IsNullOrWhiteSpace(요청.검토수신자표시명)
            || string.IsNullOrWhiteSpace(요청.검토범위)
            || string.IsNullOrWhiteSpace(요청.인계메모))
        {
            throw new ArgumentException("전문 검토 인계에는 검토 수신자, 검토 범위와 인계 메모가 필요합니다.");
        }

        var context = await Context조회Async(자동집단Id.Trim(), cancellationToken)
            ?? throw new KeyNotFoundException("전문 검토로 인계할 1.5 준비 블록이 있는 공동수입 원장을 찾을 수 없습니다.");
        var fingerprint = 지문(string.Join('|',
            전문검토인계명령유형,
            요청.검토수신자표시명.Trim(),
            요청.검토범위.Trim(),
            요청.인계메모.Trim()));
        var previousCommand = context.State.최근명령목록.FirstOrDefault(item =>
            string.Equals(item.요청멱등키, 요청.요청멱등키.Trim(), StringComparison.Ordinal));
        if (previousCommand is not null)
        {
            동일멱등요청검증(previousCommand, fingerprint);
            return 상태구성(context, 이미처리됨: true);
        }

        기대Revision검증(요청.기대Revision, context.Ledger.Revision);
        var before = 상태구성(context, 이미처리됨: false);
        if (!before.전문검토인계가능)
        {
            throw new InvalidOperationException("구조와 최신성 차단 사유를 모두 해소해야 전문 검토로 인계할 수 있습니다.");
        }

        var now = _timeProvider.GetUtcNow();
        context.State.전문검토인계기록 = new 공동수입준비Os전문검토인계기록
        {
            검토수신자표시명 = 요청.검토수신자표시명.Trim(),
            검토범위 = 요청.검토범위.Trim(),
            인계메모 = 요청.인계메모.Trim(),
            인계자UserId = actorUserId.Trim(),
            인계자표시명 = actorDisplayName.Trim(),
            인계시각Utc = now
        };
        var execution = context.State.작업실행기록.GetValueOrDefault(공동수입준비Os작업코드.전문검토인계)
                        ?? new 공동수입준비Os작업실행기록();
        execution.시도횟수++;
        execution.마지막실행시각Utc = now;
        execution.마지막오류 = string.Empty;
        context.State.작업실행기록[공동수입준비Os작업코드.전문검토인계] = execution;
        context.State.마지막트리거코드 = 공동수입준비Os트리거코드.전문검토인계;
        context.State.마지막조율자표시명 = actorDisplayName.Trim();
        context.State.마지막조율시각Utc = now;
        context.State.다음점검시각Utc = 다음점검시각(now);
        명령기록(context.State, 요청.요청멱등키, fingerprint, 전문검토인계명령유형, now);

        return await 상태저장Async(context, actorUserId.Trim(), cancellationToken);
    }

    public async Task<공동수입준비Os정기점검응답> 정기점검Async(
        int? 최대건수 = null,
        CancellationToken cancellationToken = default)
    {
        var now = _timeProvider.GetUtcNow();
        var result = new 공동수입준비Os정기점검응답 { 기준시각Utc = now };
        if (!기능활성여부() || !_options.CurrentValue.Enabled)
        {
            return result;
        }

        var limit = Math.Clamp(최대건수 ?? _options.CurrentValue.BatchSize, 1, 200);
        var ledgers = await _ledgerStore.원장목록조회Async(
            new 커뮤니티원장조회조건
            {
                원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
                Limit = limit
            },
            cancellationToken);
        result.조회건수 = ledgers.Count;

        foreach (var ledger in ledgers)
        {
            var autoGroupId = ledger.외부참조.GetValueOrDefault("AutoGroupId");
            var hasReadinessSource = ledger.블록목록.Any(block => string.Equals(
                block.BlockId,
                공동수입준비Os원장상태저장정책.준비자료BlockId,
                StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(autoGroupId) || !hasReadinessSource)
            {
                result.건너뜀건수++;
                continue;
            }

            var state = 공동수입준비Os원장상태저장정책.읽기(ledger);
            if (state.다음점검시각Utc.HasValue && state.다음점검시각Utc.Value > now)
            {
                result.건너뜀건수++;
                continue;
            }

            try
            {
                await 작업실행Async(
                    autoGroupId,
                    new 공동수입준비Os작업실행요청
                    {
                        요청멱등키 = $"scheduled:{ledger.원장Id}:{ledger.Revision}",
                        기대Revision = ledger.Revision,
                        작업코드 = 공동수입준비Os작업코드.전체준비점검
                    },
                    "system:group-import-readiness-os",
                    "1.5 공동수입 준비 OS",
                    cancellationToken);
                result.조율건수++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch
            {
                result.실패건수++;
            }
        }

        return result;
    }

    private async Task<공동수입준비OsContext?> Context조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken)
    {
        var group = await _groupStore.집단조회Async(자동집단Id, cancellationToken)
            ?? throw new InvalidOperationException("1.5 준비 OS의 원천 자동집단을 찾을 수 없습니다.");
        var ledger = await _ledgerStore.원장조회Async(
            공동수입준비원장Service.원장Id생성(자동집단Id),
            cancellationToken);
        if (ledger is null && !string.IsNullOrWhiteSpace(group.공동구매주문집계원장Id))
        {
            var candidates = await _ledgerStore.원장목록조회Async(
                new 커뮤니티원장조회조건
                {
                    원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
                    포함원장Id = group.공동구매주문집계원장Id,
                    Limit = 2
                },
                cancellationToken);
            if (candidates.Count > 1)
            {
                throw new InvalidOperationException("원천 공동구매 원장에 연결된 공동수입 원장이 둘 이상입니다.");
            }
            ledger = candidates.SingleOrDefault();
        }
        if (ledger is null)
        {
            return null;
        }

        return new 공동수입준비OsContext(
            ledger,
            group,
            공동수입준비Os원장상태저장정책.준비자료읽기(ledger),
            공동수입준비Os원장상태저장정책.읽기(ledger));
    }

    private 공동수입준비Os상태응답 상태구성(
        공동수입준비OsContext context,
        bool 이미처리됨)
    {
        var now = _timeProvider.GetUtcNow();
        var options = _options.CurrentValue;
        var freshness = TimeSpan.FromDays(Math.Clamp(options.EvidenceFreshnessDays, 1, 365));
        var evaluation = 공동수입준비원장정책.평가(context.Request, context.Group, now);
        var sharedCatalog = _sharedBatchCatalog.조회();
        var sharedJobs = sharedCatalog.작업목록
            .Where(item => !string.Equals(
                item.작업코드,
                공동구매수요모집Os배치작업코드.모집마감장기정체점검,
                StringComparison.Ordinal))
            .ToArray();

        var materialTransport = 재료묶음운송작업(context.Request, evaluation, context.State);
        var supplier = 공급자작업(context.Request, evaluation, context.State, now, freshness);
        var quoteCost = 견적원가작업(context.Request, evaluation, context.State, now, freshness);
        var classification = 품목규제작업(context.Request, evaluation, context.State, now, freshness);
        var responsibility = 책임작업(context.Request, evaluation, context.State);
        var freshnessBlockers = materialTransport.차단사유목록
            .Concat(supplier.차단사유목록)
            .Concat(quoteCost.차단사유목록)
            .Concat(classification.차단사유목록)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var handoffReady = evaluation.전문검토인계가능 && freshnessBlockers.Length == 0;
        var professionalReviewCompleted = 전문검토완료(context.Request);
        var handoff = 전문검토인계작업(context.State, handoffReady, professionalReviewCompleted);
        var shared = 공유공공데이터작업(sharedJobs, context.State);
        var tasks = new[] { shared, materialTransport, supplier, quoteCost, classification, responsibility, handoff };
        var blockers = evaluation.차단사유목록
            .Concat(freshnessBlockers)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var warnings = evaluation.경고목록
            .Concat(tasks.SelectMany(item => item.경고목록))
            .Concat(string.IsNullOrWhiteSpace(context.State.마지막오류) ? [] : [context.State.마지막오류])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var nextStageCandidate = handoffReady
                                 && professionalReviewCompleted
                                 && context.State.전문검토인계기록 is not null;
        var stateCode = nextStageCandidate
            ? 공동수입준비Os상태코드.다음단계인계후보
            : context.State.전문검토인계기록 is not null && handoffReady
                ? 공동수입준비Os상태코드.전문검토진행중
                : handoffReady
                    ? 공동수입준비Os상태코드.전문검토인계준비
                    : freshnessBlockers.Length > 0 && evaluation.차단사유목록.Count == 0
                        ? 공동수입준비Os상태코드.근거재확인필요
                        : 공동수입준비Os상태코드.자료수집중;

        return new 공동수입준비Os상태응답
        {
            자동집단Id = context.Group.자동집단Id,
            원장Id = context.Ledger.원장Id,
            원장Revision = context.Ledger.Revision,
            기능활성여부 = 기능활성여부(),
            OsWorker활성여부 = 기능활성여부() && options.Enabled,
            실행모드 = _executionMode.Mode.ToString(),
            시뮬레이션여부 = _executionMode.IsSimulation,
            상태코드 = stateCode,
            마지막트리거코드 = context.State.마지막트리거코드,
            마지막조율자표시명 = context.State.마지막조율자표시명,
            마지막조율시각Utc = context.State.마지막조율시각Utc,
            다음점검시각Utc = context.State.다음점검시각Utc ?? 다음점검시각(now),
            포워더인계준비가능 = evaluation.포워더인계준비가능,
            포워더인계기록완료 = evaluation.포워더인계기록완료,
            포워더회신기록완료 = evaluation.포워더회신기록완료,
            전문검토인계가능 = handoffReady,
            전문검토완료여부 = professionalReviewCompleted,
            다음단계인계후보여부 = nextStageCandidate,
            이미처리됨 = 이미처리됨,
            전문검토인계기록 = context.State.전문검토인계기록,
            작업목록 = tasks,
            공유배치목록 = sharedJobs,
            차단사유목록 = blockers,
            경고목록 = warnings,
            계약서명가능 = false,
            결제가능 = false,
            신고실행가능 = false,
            운송지시가능 = false,
            포워더자동선정가능 = false,
            외부자동전송가능 = false
        };
    }

    private 공동수입준비Os작업응답 재료묶음운송작업(
        공동수입준비원장저장요청 request,
        공동수입준비원장평가응답 evaluation,
        공동수입준비Os영속상태 state)
    {
        var blockers = new List<string>();
        if (!evaluation.재료품목구조완료)
        {
            blockers.Add("각 재료를 승인된 1.0 수요 집단·수량·단위와 연결해야 합니다.");
        }
        if (!evaluation.국제운송검토구조완료)
        {
            blockers.Add("LCL·FCL 비교 후보와 포워더 회신 상태 구조를 먼저 완성해야 합니다.");
        }
        if (!evaluation.포워더인계구조완료)
        {
            blockers.Add("최소화된 전달 범위와 개인정보·동의 경계를 포함한 포워더 인계 패키지를 먼저 완성해야 합니다.");
        }

        var handoffRecorded = evaluation.포워더인계기록완료;
        var responseRecorded = 국제운송검토완료(request);
        var warnings = responseRecorded
            ? Array.Empty<string>()
            : handoffRecorded
                ? ["포워더·물류대행업체에 인계한 뒤 LCL/FCL·견적·일정 회신을 기다리고 있습니다."]
                : ["OS는 외부로 전송하지 않습니다. 사람이 업체를 정해 최소 정보 패키지를 전달하고 인계 사실을 기록해야 합니다."];
        var status = blockers.Count > 0
            ? 공동수입준비Os작업상태코드.차단
            : responseRecorded
                ? 공동수입준비Os작업상태코드.완료
                : handoffRecorded
                    ? 공동수입준비Os작업상태코드.진행중
                    : 공동수입준비Os작업상태코드.사람검토대기;

        return 작업(
            공동수입준비Os작업코드.재료묶음운송검토,
            "다중 재료·포워더 인계·회신 점검",
            "인계조율",
            status,
            responseRecorded
                ? $"{request.재료품목목록.Count}개 재료의 집계 인계와 포워더 국제 운송 제안이 기록되어 있습니다."
                : handoffRecorded
                    ? "사람이 정한 업체에 집계 자료를 전달한 기록이 있으며 회신을 기다립니다."
                    : "재료별 집계 수요를 최소화한 인계 패키지로 준비하고 사람이 전달할 때까지 대기합니다.",
            "공동수입 원장 재료 품목·포워더 인계·회신 블록",
            state,
            blockers,
            warnings,
            "FCA 등 Incoterms와 LCL/FCL을 분리하며 OS는 업체 선정·외부 전송·운송 방식 확정·운송 지시를 수행하지 않습니다.");
    }

    private async Task<공동수입준비Os상태응답> 상태저장Async(
        공동수입준비OsContext context,
        string actorUserId,
        CancellationToken cancellationToken)
    {
        var response = 상태구성(context, 이미처리됨: false);
        var saved = await _ledgerStore.원장저장Async(
            공동수입준비Os원장상태저장정책.저장요청(context.Ledger, context.State, response.상태코드),
            actorUserId,
            cancellationToken);
        var savedContext = context with { Ledger = saved };
        return 상태구성(savedContext, 이미처리됨: false);
    }

    private 공동수입준비Os작업응답 공급자작업(
        공동수입준비원장저장요청 request,
        공동수입준비원장평가응답 evaluation,
        공동수입준비Os영속상태 state,
        DateTimeOffset now,
        TimeSpan freshness)
    {
        var blockers = new List<string>();
        if (!evaluation.공급자근거구조완료)
        {
            blockers.Add("공급자 후보의 원출처·식별자·검토자 구조를 먼저 완성해야 합니다.");
        }
        foreach (var supplier in request.공급자근거목록)
        {
            if (!최신인가(supplier.확인시각Utc, now, freshness)
                || !supplier.검토시각Utc.HasValue
                || !최신인가(supplier.검토시각Utc.Value, now, freshness))
            {
                blockers.Add($"공급자 '{표시(supplier.조직명, supplier.공급자후보키)}'의 확인·검토 근거가 최신성 기준을 벗어났습니다.");
            }
            if (supplier.최신상태재확인필요)
            {
                blockers.Add($"공급자 '{표시(supplier.조직명, supplier.공급자후보키)}'의 현재 영업·등록 상태 재확인이 필요합니다.");
            }
        }

        return 작업(
            공동수입준비Os작업코드.공급자근거점검,
            "공급자 근거·최신성 점검",
            "원장근거점검",
            blockers.Count == 0 ? 공동수입준비Os작업상태코드.준비 : 공동수입준비Os작업상태코드.차단,
            blockers.Count == 0 ? "공급자 원출처와 사람 검토 기록이 최신성 기준을 충족합니다." : "공급자 식별·검토 근거를 보완하거나 다시 확인해야 합니다.",
            "1.5 공급자 근거 블록",
            state,
            blockers,
            [],
            "공급자 후보를 자동 선정하거나 연락·계약하지 않습니다.");
    }

    private 공동수입준비Os작업응답 견적원가작업(
        공동수입준비원장저장요청 request,
        공동수입준비원장평가응답 evaluation,
        공동수입준비Os영속상태 state,
        DateTimeOffset now,
        TimeSpan freshness)
    {
        var blockers = new List<string>();
        if (!evaluation.견적구조완료 || !evaluation.예상비용구조완료)
        {
            blockers.Add("유효 견적과 다섯 범주의 예상 총원가 근거를 완성해야 합니다.");
        }
        foreach (var quote in request.견적목록.Where(item => !최신인가(item.확인시각Utc, now, freshness)))
        {
            blockers.Add($"견적 '{표시(quote.견적키, "미지정")}'의 확인 근거가 최신성 기준을 벗어났습니다.");
        }
        foreach (var cost in request.예상비용목록.Where(item =>
                     !최신인가(item.확인시각Utc, now, freshness)
                     || item.유효기한Utc.HasValue && item.유효기한Utc.Value <= now))
        {
            blockers.Add($"예상 비용 '{표시(cost.표시명, cost.비용키)}'의 계산 근거가 오래되었거나 유효기간이 지났습니다.");
        }

        return 작업(
            공동수입준비Os작업코드.견적원가점검,
            "견적·예상 총원가 최신성 점검",
            "원장근거점검",
            blockers.Count == 0 ? 공동수입준비Os작업상태코드.준비 : 공동수입준비Os작업상태코드.차단,
            blockers.Count == 0 ? "견적과 비용 근거가 구조·유효기간·최신성 기준을 충족합니다." : "견적 또는 비용 계산 근거를 갱신해야 합니다.",
            "1.5 견적·예상 총원가 블록",
            state,
            blockers,
            [],
            "관측 가격을 확정 판매가로 바꾸거나 결제를 만들지 않습니다.");
    }

    private 공동수입준비Os작업응답 품목규제작업(
        공동수입준비원장저장요청 request,
        공동수입준비원장평가응답 evaluation,
        공동수입준비Os영속상태 state,
        DateTimeOffset now,
        TimeSpan freshness)
    {
        var blockers = new List<string>();
        var warnings = new List<string>();
        var destinationCountry = 공동수입준비국가코드.정규화(request.도착국가코드);
        if (!evaluation.품목분류후보구조완료 || !evaluation.국가별검토구조완료)
        {
            blockers.Add("도착국가의 HS·HTS 후보와 국가별 공식 검토 근거 구조를 완성해야 합니다.");
        }
        foreach (var item in request.품목분류후보목록.Where(item =>
                     string.Equals(
                         공동수입준비국가코드.정규화(item.관할국가코드),
                         destinationCountry,
                         StringComparison.OrdinalIgnoreCase)
                     && !최신인가(item.확인시각Utc, now, freshness)))
        {
            blockers.Add($"품목분류 후보 '{표시(item.품목코드, item.후보키)}'의 공식 근거를 다시 확인해야 합니다.");
        }
        foreach (var item in request.국가별검토항목목록.Where(item =>
                     string.Equals(
                         공동수입준비국가코드.정규화(item.관할국가코드),
                         destinationCountry,
                         StringComparison.OrdinalIgnoreCase)
                     && !최신인가(item.확인시각Utc, now, freshness)))
        {
            blockers.Add($"국가별 검토 '{표시(item.표시명, item.항목코드)}'의 공식 근거를 다시 확인해야 합니다.");
        }
        if (!전문분류규제검토완료(request))
        {
            warnings.Add("품목분류와 국가별 규제 판단은 자격 있는 전문가의 확인이 남아 있습니다.");
        }
        var status = blockers.Count > 0
            ? 공동수입준비Os작업상태코드.차단
            : warnings.Count > 0
                ? 공동수입준비Os작업상태코드.사람검토대기
                : 공동수입준비Os작업상태코드.완료;

        return 작업(
            공동수입준비Os작업코드.품목규제점검,
            "HS·HTS·국가별 규제 검토",
            "전문검토준비",
            status,
            status == 공동수입준비Os작업상태코드.완료
                ? "자격 있는 검토자의 확인 상태가 원장에 기록되어 있습니다."
                : blockers.Count > 0
                    ? "공식 근거 구조와 최신성을 먼저 보완해야 합니다."
                    : "자료는 인계 가능하며 최종 판단은 자격 있는 사람에게 남겨 둡니다.",
            "1.5 품목분류·국가별 검토 블록",
            state,
            blockers,
            warnings,
            "OS는 HS·HTS를 확정하거나 수입 신고 판단을 대신하지 않습니다.");
    }

    private 공동수입준비Os작업응답 책임작업(
        공동수입준비원장저장요청 request,
        공동수입준비원장평가응답 evaluation,
        공동수입준비Os영속상태 state)
    {
        var blockers = evaluation.책임초안구조완료
            ? Array.Empty<string>()
            : ["판매자·수출자, 수입자, 관세사와 플랫폼의 책임 초안을 완성해야 합니다."];
        var unconfirmed = request.책임초안목록
            .Where(item => !item.당사자확인여부)
            .Select(item => $"'{표시(item.당사자표시명, item.역할코드)}' 당사자의 책임 범위 확인이 남아 있습니다.")
            .ToArray();
        var status = blockers.Length > 0
            ? 공동수입준비Os작업상태코드.차단
            : unconfirmed.Length > 0
                ? 공동수입준비Os작업상태코드.사람검토대기
                : 공동수입준비Os작업상태코드.완료;

        return 작업(
            공동수입준비Os작업코드.책임초안점검,
            "당사자·전문가 책임 초안 점검",
            "사람확인",
            status,
            status == 공동수입준비Os작업상태코드.완료
                ? "필수 역할의 책임 범위와 당사자 확인이 기록되어 있습니다."
                : "필수 역할과 각 당사자의 명시적 확인을 분리해 기록합니다.",
            "1.5 책임 초안 블록",
            state,
            blockers,
            unconfirmed,
            "OS가 수입자·관세사·운송 수행자를 지정하거나 계약 당사자로 만들지 않습니다.");
    }

    private 공동수입준비Os작업응답 공유공공데이터작업(
        IReadOnlyList<공동구매수요모집Os배치작업응답> sharedJobs,
        공동수입준비Os영속상태 state)
    {
        var activeCount = sharedJobs.Count(item => item.Os사용활성여부);
        var warnings = activeCount > 0
            ? Array.Empty<string>()
            : ["KAMIS·USDA·공식 기업 근거 공유 배치는 기본 비활성입니다. 자격 증명과 운영 설정을 검토한 뒤 서버 재시작으로 등록해야 합니다."];
        return 작업(
            공동수입준비Os작업코드.공유공공데이터점검,
            "KAMIS·USDA·기업 근거 공유 배치 점검",
            "공유배치카탈로그",
            activeCount > 0 ? 공동수입준비Os작업상태코드.준비 : 공동수입준비Os작업상태코드.설정비활성,
            activeCount > 0
                ? $"공유 배치 {activeCount}개가 OS 사용 상태입니다."
                : "공유 배치의 등록 설정을 표시하며 이 점검이 외부 API를 즉시 호출하지는 않습니다.",
            "1.0 OS 공유 공공데이터 배치 카탈로그",
            state,
            [],
            warnings,
            "공유 배치의 스케줄을 재사용하며 수동 점검은 외부 수집·게시 작업을 직접 실행하지 않습니다.",
            차단작업여부: false);
    }

    private 공동수입준비Os작업응답 전문검토인계작업(
        공동수입준비Os영속상태 state,
        bool handoffReady,
        bool reviewCompleted)
    {
        var status = !handoffReady
            ? 공동수입준비Os작업상태코드.차단
            : reviewCompleted && state.전문검토인계기록 is not null
                ? 공동수입준비Os작업상태코드.완료
                : state.전문검토인계기록 is not null
                    ? 공동수입준비Os작업상태코드.진행중
                    : 공동수입준비Os작업상태코드.사람검토대기;
        return 작업(
            공동수입준비Os작업코드.전문검토인계,
            "자격 있는 전문가 검토 인계",
            "사람인계",
            status,
            status switch
            {
                공동수입준비Os작업상태코드.완료 => "전문 검토 인계와 원장상 검토 완료 근거가 모두 기록되었습니다.",
                공동수입준비Os작업상태코드.진행중 => "검토 수신자에게 자료가 인계되었으며 결과 기록을 기다립니다.",
                공동수입준비Os작업상태코드.사람검토대기 => "관리자가 검토 수신자·범위·메모를 확인해 인계해야 합니다.",
                _ => "앞선 구조·최신성 차단 사유를 해소해야 합니다."
            },
            "공동수입 원장의 1.5 준비 블록과 사람 인계 기록",
            state,
            handoffReady ? [] : ["앞선 준비 작업의 차단 사유가 남아 있습니다."],
            [],
            "인계 기록은 전문 자격·판단 결과 또는 다음 단계 실행 승인이 아닙니다.",
            수동실행가능여부: false,
            선행작업코드목록:
            [
                공동수입준비Os작업코드.재료묶음운송검토,
                공동수입준비Os작업코드.공급자근거점검,
                공동수입준비Os작업코드.견적원가점검,
                공동수입준비Os작업코드.품목규제점검,
                공동수입준비Os작업코드.책임초안점검
            ]);
    }

    private 공동수입준비Os작업응답 작업(
        string code,
        string name,
        string type,
        string status,
        string guidance,
        string source,
        공동수입준비Os영속상태 state,
        IReadOnlyList<string> blockers,
        IReadOnlyList<string> warnings,
        string boundary,
        bool 차단작업여부 = true,
        bool 수동실행가능여부 = true,
        IReadOnlyList<string>? 선행작업코드목록 = null)
    {
        var execution = state.작업실행기록.GetValueOrDefault(code);
        return new 공동수입준비Os작업응답
        {
            작업코드 = code,
            작업명 = name,
            작업유형 = type,
            상태코드 = status,
            상태안내 = guidance,
            데이터출처 = source,
            실행방식 = "LedgerInspection",
            스케줄 = $"{Math.Clamp(_options.CurrentValue.ScanIntervalSeconds, 30, 86400)}초 간격 정기 점검 + 관리자 수동 점검",
            차단작업여부 = 차단작업여부,
            수동실행가능여부 = 수동실행가능여부,
            재시도가능여부 = 수동실행가능여부 && status is
                공동수입준비Os작업상태코드.차단 or
                공동수입준비Os작업상태코드.실패 or
                공동수입준비Os작업상태코드.설정비활성,
            시도횟수 = execution?.시도횟수 ?? 0,
            마지막실행시각Utc = execution?.마지막실행시각Utc,
            마지막오류 = execution?.마지막오류 ?? string.Empty,
            선행작업코드목록 = 선행작업코드목록 ?? [],
            차단사유목록 = blockers,
            경고목록 = warnings,
            실행경계 = boundary
        };
    }

    private bool 전문검토완료(공동수입준비원장저장요청 request)
        => 전문분류규제검토완료(request)
           && 국제운송검토완료(request)
           && request.책임초안목록.Count > 0
           && request.책임초안목록.All(item => item.당사자확인여부)
           && request.미확인항목목록.All(string.IsNullOrWhiteSpace);

    private static bool 국제운송검토완료(공동수입준비원장저장요청 request)
        => 공동수입준비원장정책.포워더회신완료여부(request)
           && request.포워더인계 is not null
           && request.포워더인계.인계상태코드 is
               공동수입준비포워더인계상태코드.인계기록됨 or
               공동수입준비포워더인계상태코드.회신기록됨
           && request.포워더인계.인계시각Utc.HasValue;

    private static bool 전문분류규제검토완료(공동수입준비원장저장요청 request)
    {
        var destinationCountry = 공동수입준비국가코드.정규화(request.도착국가코드);
        var classifications = request.품목분류후보목록
            .Where(item => string.Equals(
                공동수입준비국가코드.정규화(item.관할국가코드),
                destinationCountry,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var jurisdictionItems = request.국가별검토항목목록
            .Where(item => string.Equals(
                공동수입준비국가코드.정규화(item.관할국가코드),
                destinationCountry,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return classifications.Length > 0
           && classifications.All(item =>
               !item.전문가검토필요
               && string.Equals(
                   item.검토상태코드,
                   공동수입준비검토상태코드.전문가검토완료,
                   StringComparison.OrdinalIgnoreCase)
               && !string.IsNullOrWhiteSpace(item.검토자표시명))
           && jurisdictionItems.Length > 0
           && jurisdictionItems.All(item =>
               string.Equals(
                   item.검토상태코드,
                   공동수입준비검토상태코드.전문가검토완료,
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   item.검토상태코드,
                   공동수입준비검토상태코드.해당없음,
                   StringComparison.OrdinalIgnoreCase));
    }

    private static bool 최신인가(DateTimeOffset observedAt, DateTimeOffset now, TimeSpan freshness)
        => observedAt != default
           && observedAt <= now.AddMinutes(5)
           && observedAt >= now.Subtract(freshness);

    private DateTimeOffset 다음점검시각(DateTimeOffset now)
        => now.AddSeconds(Math.Clamp(_options.CurrentValue.ScanIntervalSeconds, 30, 86400));

    private void 명령기록(
        공동수입준비Os영속상태 state,
        string idempotencyKey,
        string fingerprint,
        string commandType,
        DateTimeOffset now)
    {
        state.최근명령목록.Add(new 공동수입준비Os명령기록
        {
            요청멱등키 = idempotencyKey.Trim(),
            요청지문 = fingerprint,
            명령유형 = commandType,
            처리시각Utc = now
        });
        var maxHistory = Math.Clamp(_options.CurrentValue.MaxCommandHistory, 10, 200);
        if (state.최근명령목록.Count > maxHistory)
        {
            state.최근명령목록 = state.최근명령목록
                .OrderByDescending(item => item.처리시각Utc)
                .Take(maxHistory)
                .OrderBy(item => item.처리시각Utc)
                .ToList();
        }
    }

    private void 기능활성검증()
    {
        if (!기능활성여부())
        {
            throw new InvalidOperationException("1.5 공급·가격·무역 준비 기능이 비활성 상태입니다.");
        }
    }

    private bool 기능활성여부()
        => _featureFlags.IsEnabled(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow);

    private static void 멱등키검증(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("1.5 OS 명령에는 요청 멱등 키가 필요합니다.");
        }
        if (value.Trim().Length > 160)
        {
            throw new InvalidOperationException("요청 멱등 키는 160자 이하여야 합니다.");
        }
    }

    private static void 동일멱등요청검증(공동수입준비Os명령기록 previous, string fingerprint)
    {
        if (!string.Equals(previous.요청지문, fingerprint, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("같은 멱등 키를 서로 다른 1.5 OS 명령에 사용할 수 없습니다.");
        }
    }

    private static void 기대Revision검증(long? expected, long actual)
    {
        if (expected.HasValue && expected.Value != actual)
        {
            throw new InvalidOperationException($"공동수입 원장 Revision이 변경되었습니다. Expected={expected.Value}, Actual={actual}");
        }
    }

    private static string 지문(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string 표시(string? preferred, string? fallback)
        => !string.IsNullOrWhiteSpace(preferred)
            ? preferred.Trim()
            : !string.IsNullOrWhiteSpace(fallback)
                ? fallback.Trim()
                : "미지정";

    private sealed record 공동수입준비OsContext(
        커뮤니티원장Dto Ledger,
        공동구매자동집단응답 Group,
        공동수입준비원장저장요청 Request,
        공동수입준비Os영속상태 State);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Infrastructure,
    "기존 공동수입 원장의 1.5 준비 블록을 주기적으로 읽어 최신성·포워더·전문검토 인계 상태를 재조율합니다.",
    FlowOrder = 36,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "원장 근거 점검만 실행하며 외부 공공데이터 호출, 게시, 계약, 결제, 신고, 운송 또는 창고 지시를 수행하지 않습니다.")]
public sealed class 공동수입준비OsWorker : BackgroundService
{
    private readonly I공동수입준비OS _os;
    private readonly IOptionsMonitor<GroupImportReadinessOsOptions> _options;
    private readonly ILogger<공동수입준비OsWorker> _logger;

    public 공동수입준비OsWorker(
        I공동수입준비OS os,
        IOptionsMonitor<GroupImportReadinessOsOptions> options,
        ILogger<공동수입준비OsWorker> logger)
    {
        _os = os;
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
                if (options.Enabled)
                {
                    var result = await _os.정기점검Async(options.BatchSize, stoppingToken);
                    if (result.조율건수 > 0 || result.실패건수 > 0)
                    {
                        _logger.LogInformation(
                            "1.5 공동수입 준비 OS 점검 완료. Scanned={Scanned}, Coordinated={Coordinated}, Skipped={Skipped}, Failed={Failed}",
                            result.조회건수,
                            result.조율건수,
                            result.건너뜀건수,
                            result.실패건수);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "1.5 공동수입 준비 OS background 점검 중 예외가 발생했습니다.");
            }

            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(Math.Clamp(options.ScanIntervalSeconds, 30, 86400)),
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
