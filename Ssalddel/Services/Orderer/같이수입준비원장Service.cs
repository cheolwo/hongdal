using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.Orderer;

public interface I같이수입준비원장Service
{
    Task<같이수입준비원장응답?> 조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<같이수입준비원장응답> 미리보기Async(
        string 자동집단Id,
        같이수입준비원장저장요청 request,
        CancellationToken cancellationToken = default);

    Task<같이수입준비원장응답> 저장Async(
        string 자동집단Id,
        같이수입준비원장저장요청 request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "사람이 승인한 1.0 자동집단의 공급·가격·무역 준비 자료를 기존 같이 수입 원장의 블록으로 저장하고 다시 조회합니다.",
    ContractType = typeof(I같이수입준비원장Service),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "별도 준비 원장이나 하위 운송·창고 원장을 만들지 않고 포워더 자동 선정·외부 자동 전송·계약·결제·신고와 운송 지시 가능 상태를 항상 false로 유지합니다.")]
public sealed class 같이수입준비원장Service : I같이수입준비원장Service
{
    private const string 준비자료BlockId = "trade-readiness-request";
    private const string 멱등키속성 = "LastIdempotencyKey";
    private const string 요청지문속성 = "LastRequestFingerprint";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly I공동구매자동집단화저장소 _groupStore;
    private readonly I공동구매수요모집ProcessManager _demandProcessManager;
    private readonly I커뮤니티원장저장소 _ledgerStore;
    private readonly TimeProvider _timeProvider;

    public 같이수입준비원장Service(
        I공동구매자동집단화저장소 groupStore,
        I공동구매수요모집ProcessManager demandProcessManager,
        I커뮤니티원장저장소 ledgerStore,
        TimeProvider timeProvider)
    {
        _groupStore = groupStore;
        _demandProcessManager = demandProcessManager;
        _ledgerStore = ledgerStore;
        _timeProvider = timeProvider;
    }

    public async Task<같이수입준비원장응답?> 조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        var normalizedGroupId = 자동집단Id.Trim();
        var group = await _groupStore.집단조회Async(normalizedGroupId, cancellationToken);
        if (group is null)
        {
            return null;
        }
        var operatingState = await _demandProcessManager.운영상태조회Async(normalizedGroupId, cancellationToken)
                             ?? new 공동구매수요모집Os상태응답 { 자동집단Id = normalizedGroupId };
        var ledger = await 기존같이수입원장조회Async(
            [new 원천수요Context(group, operatingState)],
            cancellationToken);
        if (ledger is null)
        {
            return null;
        }

        var request = ReadRequest(ledger);
        var sources = await 원천수요목록조회Async(
            normalizedGroupId,
            request,
            승인필수: false,
            대상원장Id: ledger.원장Id,
            cancellationToken);
        return ToResponse(ledger, sources, request, created: false, alreadyProcessed: false);
    }

    public async Task<같이수입준비원장응답> 미리보기Async(
        string 자동집단Id,
        같이수입준비원장저장요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentNullException.ThrowIfNull(request);
        var normalizedGroupId = 자동집단Id.Trim();
        var sources = await 원천수요목록조회Async(
            normalizedGroupId,
            request,
            승인필수: false,
            대상원장Id: null,
            cancellationToken);
        var existing = await 기존같이수입원장조회Async(sources, cancellationToken);
        var primary = PrimarySource(sources, normalizedGroupId);
        var evaluation = 같이수입준비원장정책.평가(request, primary.Group, _timeProvider.GetUtcNow());

        return new 같이수입준비원장응답
        {
            원장Id = existing?.원장Id ?? 원장Id생성(normalizedGroupId),
            Revision = existing?.Revision ?? 0,
            상태코드 = ResolveStatus(evaluation),
            자동집단Id = normalizedGroupId,
            원천수요운영체제Id = primary.OperatingState.운영체제Id,
            원천인계요청Id = primary.OperatingState.인계요청Id,
            상품키 = primary.Group.상품키,
            상품명 = primary.Group.상품명,
            원천Hs코드 = primary.Group.HS코드,
            모인수요수량 = primary.Group.총희망수량,
            수량단위 = primary.Group.수량단위,
            거래문맥 = ResolveTransactionContext(sources),
            원천수요목록 = SourceResponses(sources),
            준비자료 = request,
            평가 = evaluation,
            저장시각Utc = _timeProvider.GetUtcNow()
        };
    }

    public async Task<같이수입준비원장응답> 저장Async(
        string 자동집단Id,
        같이수입준비원장저장요청 request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorUserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actorDisplayName);
        ValidateIdempotencyKey(request.요청멱등키);

        var normalizedGroupId = 자동집단Id.Trim();
        var sources = await 원천수요목록조회Async(
            normalizedGroupId,
            request,
            승인필수: true,
            대상원장Id: null,
            cancellationToken);
        var existing = await 기존같이수입원장조회Async(sources, cancellationToken);
        var ledgerId = existing?.원장Id ?? 원장Id생성(normalizedGroupId);
        연결대상원장검증(sources, ledgerId);
        var primary = PrimarySource(sources, normalizedGroupId);
        var fingerprint = 요청지문(request);
        if (existing is not null
            && string.Equals(
                existing.확장속성.GetValueOrDefault(멱등키속성),
                request.요청멱등키.Trim(),
                StringComparison.Ordinal))
        {
            if (!string.Equals(
                    existing.확장속성.GetValueOrDefault(요청지문속성),
                    fingerprint,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("같은 멱등 키를 서로 다른 1.5 준비 자료에 사용할 수 없습니다.");
            }

            var retryLinkedSources = await 후속원장목록연결Async(sources, ledgerId, cancellationToken);
            return ToResponse(existing, retryLinkedSources, request, created: false, alreadyProcessed: true);
        }
        if (existing is not null && !request.기대Revision.HasValue)
        {
            throw new InvalidOperationException("기존 같이 수입 원장의 준비 블록을 갱신하려면 기대 Revision이 필요합니다.");
        }

        var now = _timeProvider.GetUtcNow();
        var evaluation = 같이수입준비원장정책.평가(request, primary.Group, now);
        var status = ResolveStatus(evaluation);
        var saved = await _ledgerStore.원장저장Async(
            BuildSaveRequest(
                ledgerId,
                request,
                sources,
                evaluation,
                status,
                fingerprint,
                actorUserId.Trim(),
                actorDisplayName.Trim(),
                existing),
            actorUserId.Trim(),
            cancellationToken);

        var linkedSources = await 후속원장목록연결Async(sources, ledgerId, cancellationToken);
        return ToResponse(saved, linkedSources, request, created: existing is null, alreadyProcessed: false);
    }

    private static void ValidateApprovedHandoff(공동구매수요모집Os상태응답 state)
    {
        if (!string.Equals(
                state.인계상태,
                공동구매수요모집인계상태코드.승인후속대기,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException("운영자가 1.0 모집 결과의 1.5 준비 인계를 먼저 승인해야 합니다.");
        }
        if (!state.후속워크플로우활성여부)
        {
            throw new InvalidOperationException("1.5 공급·가격·무역 준비 기능이 비활성 상태입니다.");
        }
        if (string.IsNullOrWhiteSpace(state.인계요청Id)
            || string.IsNullOrWhiteSpace(state.승인자키)
            || !state.승인시각Utc.HasValue)
        {
            throw new InvalidOperationException("1.0 모집 결과의 인계 승인 식별자, 승인자와 승인 시각이 필요합니다.");
        }
    }

    private static 커뮤니티원장저장요청 BuildSaveRequest(
        string ledgerId,
        같이수입준비원장저장요청 request,
        IReadOnlyList<원천수요Context> sources,
        같이수입준비원장평가응답 evaluation,
        string status,
        string fingerprint,
        string actorUserId,
        string actorDisplayName,
        커뮤니티원장Dto? existing)
    {
        var primary = sources[0];
        var sourceReferences = sources
            .Where(source => !string.IsNullOrWhiteSpace(source.Group.공동구매주문집계원장Id))
            .Select((source, index) => new 커뮤니티포함원장참조Dto
            {
                원장Id = source.Group.공동구매주문집계원장Id,
                원장템플릿Key = CommunityLedgerTemplateKeys.GroupPurchase,
                역할 = $"원천 공동구매 수요 원장 · {source.Group.상품명}",
                관계유형 = CommunityLedgerRelationTypes.Reference,
                필수여부 = true,
                표시순서 = index
            })
            .GroupBy(item => item.원장Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        var materialItems = request.재료품목목록;
        var title = materialItems.Count > 1
            ? $"{materialItems[0].재료명} 외 {materialItems.Count - 1}개 재료 같이 수입 원장"
            : $"{materialItems[0].재료명} 같이 수입 원장";
        var totalDemandText = string.Join(", ", materialItems.Select(item =>
            $"{item.재료명} {item.모인수요수량:0.####}{item.수량단위}"));
        var readinessBlocks = BuildBlocks(request, sources, evaluation);
        var references = (existing?.포함원장목록 ?? [])
            .Concat(sourceReferences)
            .Where(item => !string.IsNullOrWhiteSpace(item.원장Id))
            .GroupBy(item => item.원장Id, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.표시순서)
            .ToArray();
        var participants = (existing?.참여자목록 ?? [])
            .Concat(
            [
                new 커뮤니티원장참여자Dto
                {
                    UserId = actorUserId,
                    DisplayName = actorDisplayName,
                    RoleLabel = "같이 수입 준비 자료 관리자",
                    ParticipationState = "검토중"
                }
            ])
            .GroupBy(item => $"{item.UserId}|{item.RoleLabel}", StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();
        var externalReferences = MergeDictionary(existing?.외부참조, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AutoGroupId"] = primary.Group.자동집단Id,
            ["AutoGroupIds"] = JsonSerializer.Serialize(sources.Select(source => source.Group.자동집단Id), JsonOptions),
            ["DemandHandoffRequestId"] = primary.OperatingState.인계요청Id,
            ["DemandHandoffRequestIds"] = JsonSerializer.Serialize(sources.Select(source => source.OperatingState.인계요청Id), JsonOptions),
            ["SourceGroupPurchaseLedgerId"] = primary.Group.공동구매주문집계원장Id,
            ["SourceGroupPurchaseLedgerIds"] = JsonSerializer.Serialize(
                sources.Select(source => source.Group.공동구매주문집계원장Id)
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal),
                JsonOptions),
            ["ProductKey"] = primary.Group.상품키,
            ["ProductKeys"] = JsonSerializer.Serialize(materialItems.Select(item => item.재료키), JsonOptions),
            ["HsCode"] = primary.Group.HS코드,
            ["DestinationCountryCode"] = 같이수입준비국가코드.정규화(request.도착국가코드),
            [공동구매거래문맥원장키.거래유형] = ResolveTransactionContext(sources).거래유형,
            [공동구매거래문맥원장키.가격표시기준] = ResolveTransactionContext(sources).가격표시기준,
            [공동구매거래문맥원장키.원천거래문맥원장Id] = primary.Group.공동구매주문집계원장Id,
            [공동구매거래문맥원장키.구매조직수] = ResolveTransactionContext(sources).구매조직수
                .ToString(System.Globalization.CultureInfo.InvariantCulture),
            [공동구매거래문맥원장키.세금계산서요청수] = ResolveTransactionContext(sources).세금계산서요청수
                .ToString(System.Globalization.CultureInfo.InvariantCulture)
        });
        var extensions = MergeDictionary(existing?.확장속성, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ReadinessWorkflowVersion"] = "1.5",
            ["ReadinessStatus"] = status,
            ["ExecutionBoundary"] = "NoForwarderAutoSelectionNoExternalAutoSendNoContractNoPaymentNoFilingNoTransport",
            [멱등키속성] = request.요청멱등키.Trim(),
            [요청지문속성] = fingerprint
        });

        return new 커뮤니티원장저장요청
        {
            원장Id = ledgerId,
            기대Revision = request.기대Revision,
            커뮤니티Id = existing?.커뮤니티Id ?? "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
            제목 = string.IsNullOrWhiteSpace(existing?.제목) ? title : existing.제목,
            원함 = string.IsNullOrWhiteSpace(existing?.원함)
                ? $"여러 재료의 확인된 수요({totalDemandText})를 한 같이 수입 준비 묶음으로 두고 포워더 인계·공급자·가격·품목분류·국가별 수입 자료와 연결합니다."
                : existing.원함,
            상태 = existing?.상태 ?? 커뮤니티원장상태.진행중,
            현재단계Key = existing?.현재단계Key ?? CommunityGroupImportLedgerStageCodes.ImportDecision,
            대상OsCode = CommunityLedgerOperatingSystemCodes.GroupPurchaseImport,
            대상OsName = "같이 수입 OS",
            생성자UserId = existing?.생성자UserId ?? actorUserId,
            생성자표시명 = existing?.생성자표시명 ?? actorDisplayName,
            블록목록 = MergeReadinessBlocks(readinessBlocks, existing),
            블록담당자명시적갱신여부 = true,
            참여자목록 = participants,
            포함원장목록 = references,
            다이어그램스냅샷 = existing?.다이어그램스냅샷 ?? BuildDiagram(ledgerId, evaluation),
            외부참조 = externalReferences,
            확장속성 = extensions
        };
    }

    private static IReadOnlyList<커뮤니티원장블록Dto> MergeReadinessBlocks(
        IReadOnlyList<커뮤니티원장블록Dto> readinessBlocks,
        커뮤니티원장Dto? existing)
    {
        var readinessIds = readinessBlocks
            .Select(block => block.BlockId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return (existing?.블록목록 ?? [])
            .Where(block => !readinessIds.Contains(block.BlockId))
            .Concat(readinessBlocks)
            .ToArray();
    }

    private static Dictionary<string, string> MergeDictionary(
        IReadOnlyDictionary<string, string>? existing,
        IReadOnlyDictionary<string, string> updates)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in existing ?? new Dictionary<string, string>())
        {
            result[pair.Key] = pair.Value;
        }
        foreach (var pair in updates)
        {
            result[pair.Key] = pair.Value;
        }
        return result;
    }

    private static IReadOnlyList<커뮤니티원장블록Dto> BuildBlocks(
        같이수입준비원장저장요청 request,
        IReadOnlyList<원천수요Context> sources,
        같이수입준비원장평가응답 evaluation)
    {
        var transactionContext = ResolveTransactionContext(sources);
        return
        [
            Block(준비자료BlockId, "1.5 준비 자료 원본", "recorded", new()
            {
                ["Json"] = JsonSerializer.Serialize(request, JsonOptions)
            }),
            Block("source-demand", "승인된 1.0 수요 집단 묶음", "approved-handoff", new()
            {
                ["Json"] = JsonSerializer.Serialize(SourceResponses(sources), JsonOptions),
                ["SourceCount"] = sources.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }),
            Block("transaction-context", "B2B·B2C 거래 문맥", "segregated", new()
            {
                [공동구매거래문맥원장키.거래유형] = transactionContext.거래유형,
                [공동구매거래문맥원장키.가격표시기준] = transactionContext.가격표시기준,
                [공동구매거래문맥원장키.원천거래문맥원장Id] = transactionContext.원천거래문맥원장Id,
                [공동구매거래문맥원장키.구매조직수] = transactionContext.구매조직수
                    .ToString(System.Globalization.CultureInfo.InvariantCulture),
                [공동구매거래문맥원장키.세금계산서요청수] = transactionContext.세금계산서요청수
                    .ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["MixedTransactionContextsAllowed"] = bool.FalseString
            }),
            Block("material-items", "공동구매 재료 품목 묶음", evaluation.재료품목구조완료 ? "structured" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.재료품목목록, JsonOptions),
                ["MaterialCount"] = request.재료품목목록.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }),
            Block("supplier-evidence", "공급자·관련 기업 근거", evaluation.공급자근거구조완료 ? "structured" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.공급자근거목록, JsonOptions)
            }),
            Block("quotes", "견적·MOQ·납기·포장", evaluation.견적구조완료 ? "structured" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.견적목록, JsonOptions)
            }),
            Block("landed-cost", "예상 총원가 분해", evaluation.예상비용구조완료 ? "structured" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.예상비용목록, JsonOptions)
            }),
            Block("forwarder-handoff", "포워더·물류대행업체 인계", evaluation.포워더인계기록완료 ? "handoff-recorded" : evaluation.포워더인계준비가능 ? "ready-for-human-handoff" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.포워더인계, JsonOptions),
                ["ForwarderAutoSelection"] = bool.FalseString,
                ["ExternalAutoSend"] = bool.FalseString,
                ["AggregatedDemandDefault"] = bool.TrueString
            }),
            Block("international-transport-review", "포워더 LCL·FCL 회신", evaluation.포워더회신기록완료 ? "response-recorded" : evaluation.국제운송검토구조완료 ? "awaiting-response" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.국제운송검토, JsonOptions),
                ["IncotermsSeparated"] = bool.TrueString,
                ["TransportInstruction"] = bool.FalseString
            }),
            Block("classification", "HS·HTS 후보와 검토 상태", evaluation.품목분류후보구조완료 ? "structured" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.품목분류후보목록, JsonOptions)
            }),
            Block("jurisdiction-review", "한국·미국 국가별 검토", evaluation.국가별검토구조완료 ? "structured" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.국가별검토항목목록, JsonOptions)
            }),
            Block("responsibilities", "거래·전문·플랫폼 책임 초안", evaluation.책임초안구조완료 ? "structured" : "incomplete", new()
            {
                ["Json"] = JsonSerializer.Serialize(request.책임초안목록, JsonOptions)
            }, request.책임초안목록),
            Block("open-items", "미확인 규제·계약 항목", evaluation.명시된미확인항목목록.Count == 0 ? "none-recorded" : "open", new()
            {
                ["Json"] = JsonSerializer.Serialize(evaluation.명시된미확인항목목록, JsonOptions),
                ["Blockers"] = JsonSerializer.Serialize(evaluation.차단사유목록, JsonOptions),
                ["Warnings"] = JsonSerializer.Serialize(evaluation.경고목록, JsonOptions)
            }),
            Block("execution-boundary", "1.5 실행 차단", "blocked", new()
            {
                ["ContractSigning"] = bool.FalseString,
                ["Payment"] = bool.FalseString,
                ["ImportFiling"] = bool.FalseString,
                ["SupplierAutoSelection"] = bool.FalseString,
                ["ForwarderAutoSelection"] = bool.FalseString,
                ["ExternalAutoSend"] = bool.FalseString,
                ["TransportInstruction"] = bool.FalseString,
                ["WarehouseMutation"] = bool.FalseString
            })
        ];
    }

    private static 커뮤니티원장블록Dto Block(
        string blockId,
        string title,
        string state,
        Dictionary<string, string> data,
        IReadOnlyList<같이수입책임초안>? responsibilities = null)
        => new()
        {
            BlockId = blockId,
            BlockType = CommunityLedgerBlockTypes.Generic,
            Title = title,
            State = state,
            Data = data,
            담당자목록 = responsibilities?
                .Where(item => !string.IsNullOrWhiteSpace(item.당사자표시명))
                .Select(item => new 커뮤니티원장블록담당자Dto
                {
                    DisplayName = item.당사자표시명.Trim(),
                    RoleLabel = item.역할코드.Trim(),
                    ResponsibilityType = CommunityLedgerBlockResponsibilityTypes.Primary
                })
                .ToArray() ?? []
        };

    private static DiagramSnapshotDto BuildDiagram(
        string ledgerId,
        같이수입준비원장평가응답 evaluation)
    {
        var nodes = new[]
        {
            Node("source-demand", "1.0 승인 수요 묶음", 40, evaluation.전문검토인계가능 ? "approved" : "linked"),
            Node("material-items", "여러 재료 품목", 240, evaluation.재료품목구조완료 ? "structured" : "incomplete"),
            Node("supplier-evidence", "공급자 근거", 440, evaluation.공급자근거구조완료 ? "structured" : "incomplete"),
            Node("quotes", "품목별 견적·MOQ", 640, evaluation.견적구조완료 ? "structured" : "incomplete"),
            Node("landed-cost", "예상 총원가", 840, evaluation.예상비용구조완료 ? "structured" : "incomplete"),
            Node("forwarder-handoff", "집계 자료 포워더 인계", 1040, evaluation.포워더인계기록완료 ? "recorded" : evaluation.포워더인계준비가능 ? "ready" : "incomplete"),
            Node("international-transport-review", "포워더 LCL·FCL 회신", 1240, evaluation.포워더회신기록완료 ? "recorded" : "awaiting"),
            Node("classification", "품목별 HS·HTS 후보", 1440, evaluation.품목분류후보구조완료 ? "structured" : "incomplete"),
            Node("jurisdiction-review", "국가별 규제 검토", 1640, evaluation.국가별검토구조완료 ? "structured" : "incomplete"),
            Node("qualified-review", "자격 있는 검토자 인계", 1840, evaluation.전문검토인계가능 ? "ready" : "blocked")
        };
        var edges = nodes
            .Zip(nodes.Skip(1), (from, to) => new DiagramEdgeDto
            {
                EdgeId = $"{from.NodeId}--{to.NodeId}",
                FromNodeId = from.NodeId,
                ToNodeId = to.NodeId,
                Label = "근거 확인 후",
                MeaningCode = CommunityLedgerRelationTypes.Handoff
            })
            .ToArray();

        return new DiagramSnapshotDto
        {
            DiagramId = $"diagram-{ledgerId}",
            DiagramName = "1.0 수요 → 같이 수입 원장 준비·포워더 인계",
            LedgerTemplateKey = CommunityLedgerTemplateKeys.GroupImport,
            WorkflowModeKey = CommunityLedgerOperatingSystemCodes.GroupPurchaseImport,
            Nodes = nodes,
            Edges = edges,
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["WorkflowVersion"] = "1.5",
                ["OperationalExecution"] = bool.FalseString,
                ["ExternalAutoSend"] = bool.FalseString
            }
        };
    }

    private static DiagramNodeDto Node(string nodeId, string title, double x, string state)
        => new()
        {
            NodeId = nodeId,
            Kind = "TradeReadinessEvidence",
            Title = title,
            X = x,
            Y = 120,
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["State"] = state
            }
        };

    private 같이수입준비원장응답 ToResponse(
        커뮤니티원장Dto ledger,
        IReadOnlyList<원천수요Context> sources,
        같이수입준비원장저장요청 request,
        bool created,
        bool alreadyProcessed)
    {
        var primary = sources[0];
        var evaluatedAt = _timeProvider.GetUtcNow();
        var savedAt = ledger.수정시각Utc == default
            ? evaluatedAt
            : new DateTimeOffset(DateTime.SpecifyKind(ledger.수정시각Utc, DateTimeKind.Utc));
        var evaluation = 같이수입준비원장정책.평가(request, primary.Group, evaluatedAt);

        return new 같이수입준비원장응답
        {
            원장Id = ledger.원장Id,
            Revision = ledger.Revision,
            생성됨 = created,
            이미처리됨 = alreadyProcessed,
            상태코드 = ResolveStatus(evaluation),
            자동집단Id = primary.Group.자동집단Id,
            원천수요운영체제Id = primary.OperatingState.운영체제Id,
            원천인계요청Id = primary.OperatingState.인계요청Id,
            상품키 = primary.Group.상품키,
            상품명 = primary.Group.상품명,
            원천Hs코드 = primary.Group.HS코드,
            모인수요수량 = primary.Group.총희망수량,
            수량단위 = primary.Group.수량단위,
            거래문맥 = ResolveTransactionContext(sources),
            원천수요목록 = SourceResponses(sources),
            준비자료 = request,
            평가 = evaluation,
            저장시각Utc = savedAt
        };
    }

    private async Task<IReadOnlyList<원천수요Context>> 원천수요목록조회Async(
        string anchorGroupId,
        같이수입준비원장저장요청 request,
        bool 승인필수,
        string? 대상원장Id,
        CancellationToken cancellationToken)
    {
        var anchorGroup = await _groupStore.집단조회Async(anchorGroupId, cancellationToken)
            ?? throw new KeyNotFoundException("1.5 준비 대상으로 검토할 기준 자동집단을 찾을 수 없습니다.");
        같이수입준비원장정책.단일재료호환정규화(request, anchorGroup);

        var anchorItem = request.재료품목목록.FirstOrDefault(item => string.Equals(
            item.원천자동집단Id?.Trim(),
            anchorGroupId,
            StringComparison.Ordinal));
        if (anchorItem is null)
        {
            throw new InvalidOperationException("경로의 기준 1.0 수요 집단을 재료 품목 묶음에서 제거할 수 없습니다.");
        }

        var duplicateSource = request.재료품목목록
            .Where(item => !string.IsNullOrWhiteSpace(item.원천자동집단Id))
            .GroupBy(item => item.원천자동집단Id.Trim(), StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateSource is not null)
        {
            throw new InvalidOperationException($"같은 1.0 수요 집단 '{duplicateSource.Key}'을 준비 묶음에 두 번 넣을 수 없습니다.");
        }

        var orderedGroupIds = request.재료품목목록
            .Select(item => item.원천자동집단Id?.Trim() ?? string.Empty)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(id => string.Equals(id, anchorGroupId, StringComparison.Ordinal) ? 0 : 1)
            .ToArray();
        var contexts = new List<원천수요Context>(orderedGroupIds.Length);
        foreach (var groupId in orderedGroupIds)
        {
            var group = string.Equals(groupId, anchorGroupId, StringComparison.Ordinal)
                ? anchorGroup
                : await _groupStore.집단조회Async(groupId, cancellationToken)
                  ?? throw new KeyNotFoundException($"재료 품목에 연결한 1.0 수요 집단 '{groupId}'을 찾을 수 없습니다.");
            var state = await _demandProcessManager.운영상태조회Async(groupId, cancellationToken)
                        ?? new 공동구매수요모집Os상태응답 { 자동집단Id = groupId };
            if (승인필수)
            {
                ValidateApprovedHandoff(state);
            }
            if (!string.IsNullOrWhiteSpace(대상원장Id)
                && !string.IsNullOrWhiteSpace(state.대상원장Id)
                && !string.Equals(state.대상원장Id, 대상원장Id, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"재료 '{group.상품명}'의 수요 집단은 이미 다른 같이 수입 원장에 연결되어 있습니다.");
            }

            var material = request.재료품목목록.Single(item => string.Equals(
                item.원천자동집단Id?.Trim(),
                groupId,
                StringComparison.Ordinal));
            if (!string.IsNullOrWhiteSpace(material.재료키)
                && !string.Equals(material.재료키.Trim(), group.상품키, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"재료 품목 '{material.재료명}'의 키가 원천 수요 집단 상품 키와 일치하지 않습니다.");
            }
            material.재료키 = group.상품키;
            material.재료명 = group.상품명;
            material.원천자동집단Id = group.자동집단Id;
            material.원천Hs코드 = group.HS코드;
            material.모인수요수량 = group.총희망수량;
            material.수량단위 = group.수량단위;
            contexts.Add(new 원천수요Context(group, state));
        }

        ValidateTransactionContextCompatibility(contexts);
        request.재료키 = contexts[0].Group.상품키;
        request.재료명 = contexts[0].Group.상품명;
        return contexts;
    }

    private async Task<커뮤니티원장Dto?> 기존같이수입원장조회Async(
        IReadOnlyList<원천수요Context> sources,
        CancellationToken cancellationToken)
    {
        var linkedLedgerIds = sources
            .Select(source => source.OperatingState.대상원장Id?.Trim() ?? string.Empty)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (linkedLedgerIds.Length > 1)
        {
            throw new InvalidOperationException("선택한 수요 집단들이 서로 다른 같이 수입 원장에 이미 연결되어 있어 한 묶음으로 합칠 수 없습니다.");
        }
        if (linkedLedgerIds.Length == 1)
        {
            var linked = await _ledgerStore.원장조회Async(linkedLedgerIds[0], cancellationToken)
                ?? throw new InvalidOperationException("1.0 수요 OS에 연결된 같이 수입 원장을 찾을 수 없습니다.");
            ValidateGroupImportLedger(linked);
            return linked;
        }

        var sourceLedgerIds = sources
            .Select(source => source.Group.공동구매주문집계원장Id?.Trim() ?? string.Empty)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (sourceLedgerIds.Length > 0)
        {
            var candidates = await _ledgerStore.원장목록조회Async(
                new 커뮤니티원장조회조건
                {
                    원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
                    포함원장Ids = sourceLedgerIds,
                    Limit = 20
                },
                cancellationToken);
            var matching = candidates
                .Where(candidate => candidate.포함원장목록.Any(reference =>
                    sourceLedgerIds.Contains(reference.원장Id, StringComparer.Ordinal)))
                .OrderByDescending(candidate => candidate.수정시각Utc)
                .ToArray();
            if (matching.Length > 1)
            {
                throw new InvalidOperationException("원천 공동구매 원장을 포함한 같이 수입 원장이 둘 이상입니다. 중복 원장을 먼저 정리해야 합니다.");
            }
            if (matching.Length == 1)
            {
                return matching[0];
            }
        }

        var deterministic = await _ledgerStore.원장조회Async(
            원장Id생성(sources[0].Group.자동집단Id),
            cancellationToken);
        if (deterministic is not null)
        {
            ValidateGroupImportLedger(deterministic);
        }
        return deterministic;
    }

    private static void 연결대상원장검증(
        IReadOnlyList<원천수요Context> sources,
        string ledgerId)
    {
        var conflict = sources.FirstOrDefault(source =>
            !string.IsNullOrWhiteSpace(source.OperatingState.대상원장Id)
            && !string.Equals(source.OperatingState.대상원장Id, ledgerId, StringComparison.Ordinal));
        if (conflict is not null)
        {
            throw new InvalidOperationException($"재료 '{conflict.Group.상품명}'의 수요 집단은 이미 다른 같이 수입 원장에 연결되어 있습니다.");
        }
    }

    private static void ValidateGroupImportLedger(커뮤니티원장Dto ledger)
    {
        if (!string.Equals(
                ledger.원장템플릿Key,
                CommunityLedgerTemplateKeys.GroupImport,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("1.0 수요 OS의 후속 대상은 같이 수입 원장이어야 합니다.");
        }
    }

    private async Task<IReadOnlyList<원천수요Context>> 후속원장목록연결Async(
        IReadOnlyList<원천수요Context> sources,
        string ledgerId,
        CancellationToken cancellationToken)
    {
        var linked = new List<원천수요Context>(sources.Count);
        foreach (var source in sources)
        {
            var state = await _demandProcessManager.후속원장연결Async(
                source.Group.자동집단Id,
                source.OperatingState.인계요청Id,
                ledgerId,
                cancellationToken);
            linked.Add(source with { OperatingState = state });
        }
        return linked;
    }

    private static 원천수요Context PrimarySource(
        IReadOnlyList<원천수요Context> sources,
        string anchorGroupId)
        => sources.First(source => string.Equals(
            source.Group.자동집단Id,
            anchorGroupId,
            StringComparison.Ordinal));

    private static IReadOnlyList<같이수입준비원천수요응답> SourceResponses(
        IReadOnlyList<원천수요Context> sources)
        => sources.Select(source => new 같이수입준비원천수요응답
        {
            자동집단Id = source.Group.자동집단Id,
            인계요청Id = source.OperatingState.인계요청Id,
            공동구매주문집계원장Id = source.Group.공동구매주문집계원장Id,
            재료키 = source.Group.상품키,
            재료명 = source.Group.상품명,
            원천Hs코드 = source.Group.HS코드,
            모인수요수량 = source.Group.총희망수량,
            수량단위 = source.Group.수량단위,
            거래유형 = 공동구매거래유형코드.정규화(source.Group.거래유형),
            가격표시기준 = 공동구매가격표시기준코드.정규화(
                source.Group.가격표시기준,
                source.Group.거래유형)
        }).ToArray();

    private static 공동구매거래문맥응답 ResolveTransactionContext(
        IReadOnlyList<원천수요Context> sources)
    {
        var primary = sources[0];
        var contexts = sources
            .Select(source => 공동구매거래문맥정책.생성(
                source.Group,
                source.Group.공동구매주문집계원장Id))
            .ToArray();
        return new 공동구매거래문맥응답
        {
            거래유형 = contexts[0].거래유형,
            가격표시기준 = contexts[0].가격표시기준,
            원천거래문맥원장Id = primary.Group.공동구매주문집계원장Id,
            구매조직수 = contexts.Sum(context => context.구매조직수),
            세금계산서요청수 = contexts.Sum(context => context.세금계산서요청수)
        };
    }

    private static void ValidateTransactionContextCompatibility(IReadOnlyList<원천수요Context> sources)
    {
        if (sources.Count < 2)
        {
            return;
        }

        var primary = sources[0].Group;
        var incompatible = sources.Skip(1).FirstOrDefault(source =>
            !공동구매거래문맥정책.호환됨(primary, source.Group));
        if (incompatible is not null)
        {
            throw new InvalidOperationException(
                $"'{incompatible.Group.상품명}' 수요는 B2B/B2C 또는 부가세 표시 기준이 달라 같은 같이 수입 원장에 합칠 수 없습니다. 거래 문맥별로 원장을 나눠 주세요.");
        }
    }

    private static 같이수입준비원장저장요청 ReadRequest(커뮤니티원장Dto ledger)
    {
        var requestJson = ledger.블록목록
            .FirstOrDefault(block => string.Equals(block.BlockId, 준비자료BlockId, StringComparison.OrdinalIgnoreCase))?
            .Data.GetValueOrDefault("Json");
        return string.IsNullOrWhiteSpace(requestJson)
            ? new 같이수입준비원장저장요청()
            : JsonSerializer.Deserialize<같이수입준비원장저장요청>(requestJson, JsonOptions)
              ?? new 같이수입준비원장저장요청();
    }

    private static string ResolveStatus(같이수입준비원장평가응답 evaluation)
        => evaluation.전문검토인계가능
            ? 같이수입준비원장상태코드.전문검토자료준비
            : 같이수입준비원장상태코드.초안;

    private static void ValidateIdempotencyKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("같이 수입 원장의 1.5 준비 블록 저장에는 요청 멱등 키가 필요합니다.");
        }
        if (value.Trim().Length > 160)
        {
            throw new InvalidOperationException("요청 멱등 키는 160자 이하여야 합니다.");
        }
    }

    private static string 요청지문(같이수입준비원장저장요청 request)
    {
        var originalKey = request.요청멱등키;
        var originalRevision = request.기대Revision;
        try
        {
            request.요청멱등키 = string.Empty;
            request.기대Revision = null;
            var json = JsonSerializer.Serialize(request, JsonOptions);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
        }
        finally
        {
            request.요청멱등키 = originalKey;
            request.기대Revision = originalRevision;
        }
    }

    private sealed record 원천수요Context(
        공동구매자동집단응답 Group,
        공동구매수요모집Os상태응답 OperatingState);

    public static string 원장Id생성(string 자동집단Id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(자동집단Id.Trim())))
            .ToLowerInvariant();
        return $"group-import-{digest[..32]}";
    }
}
