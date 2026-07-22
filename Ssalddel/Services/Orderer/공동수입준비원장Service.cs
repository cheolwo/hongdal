using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.Orderer;

public interface I공동수입준비원장Service
{
    Task<공동수입준비원장응답?> 조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동수입준비원장응답> 미리보기Async(
        string 자동집단Id,
        공동수입준비원장저장요청 request,
        CancellationToken cancellationToken = default);

    Task<공동수입준비원장응답> 저장Async(
        string 자동집단Id,
        공동수입준비원장저장요청 request,
        string actorUserId,
        string actorDisplayName,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Application,
    "사람이 승인한 1.0 자동집단만 1.5 공급·가격·무역 준비 원장으로 저장하고 다시 조회합니다.",
    ContractType = typeof(I공동수입준비원장Service),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "하위 운송·창고 원장을 만들지 않고 계약, 결제, 신고, 공급자 선정과 운송 지시 가능 상태를 항상 false로 유지합니다.")]
public sealed class 공동수입준비원장Service : I공동수입준비원장Service
{
    private const string 준비자료BlockId = "trade-readiness-request";
    private const string 멱등키속성 = "LastIdempotencyKey";
    private const string 요청지문속성 = "LastRequestFingerprint";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly I공동구매자동집단화저장소 _groupStore;
    private readonly I공동구매수요모집OS _demandOperatingSystem;
    private readonly I커뮤니티원장저장소 _ledgerStore;
    private readonly TimeProvider _timeProvider;

    public 공동수입준비원장Service(
        I공동구매자동집단화저장소 groupStore,
        I공동구매수요모집OS demandOperatingSystem,
        I커뮤니티원장저장소 ledgerStore,
        TimeProvider timeProvider)
    {
        _groupStore = groupStore;
        _demandOperatingSystem = demandOperatingSystem;
        _ledgerStore = ledgerStore;
        _timeProvider = timeProvider;
    }

    public async Task<공동수입준비원장응답?> 조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        var normalizedGroupId = 자동집단Id.Trim();
        var ledger = await _ledgerStore.원장조회Async(원장Id생성(normalizedGroupId), cancellationToken);
        if (ledger is null)
        {
            return null;
        }

        var group = await _groupStore.집단조회Async(normalizedGroupId, cancellationToken)
            ?? throw new InvalidOperationException("1.5 준비 원장의 원천 자동집단을 찾을 수 없습니다.");
        var operatingState = await _demandOperatingSystem.운영상태조회Async(normalizedGroupId, cancellationToken)
            ?? throw new InvalidOperationException("1.5 준비 원장의 원천 수요 OS 상태를 찾을 수 없습니다.");

        return ToResponse(ledger, group, operatingState, created: false, alreadyProcessed: false);
    }

    public async Task<공동수입준비원장응답> 미리보기Async(
        string 자동집단Id,
        공동수입준비원장저장요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        ArgumentNullException.ThrowIfNull(request);
        var normalizedGroupId = 자동집단Id.Trim();
        var group = await _groupStore.집단조회Async(normalizedGroupId, cancellationToken)
            ?? throw new KeyNotFoundException("1.5 준비 대상으로 검토할 자동집단을 찾을 수 없습니다.");
        var operatingState = await _demandOperatingSystem.운영상태조회Async(normalizedGroupId, cancellationToken)
            ?? new 공동구매수요모집Os상태응답 { 자동집단Id = normalizedGroupId };
        var evaluation = 공동수입준비원장정책.평가(request, group, _timeProvider.GetUtcNow());

        return new 공동수입준비원장응답
        {
            원장Id = 원장Id생성(normalizedGroupId),
            상태코드 = ResolveStatus(evaluation),
            자동집단Id = normalizedGroupId,
            원천수요운영체제Id = operatingState.운영체제Id,
            원천인계요청Id = operatingState.인계요청Id,
            상품키 = group.상품키,
            상품명 = group.상품명,
            원천Hs코드 = group.HS코드,
            모인수요수량 = group.총희망수량,
            수량단위 = group.수량단위,
            준비자료 = request,
            평가 = evaluation,
            저장시각Utc = _timeProvider.GetUtcNow()
        };
    }

    public async Task<공동수입준비원장응답> 저장Async(
        string 자동집단Id,
        공동수입준비원장저장요청 request,
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
        var group = await _groupStore.집단조회Async(normalizedGroupId, cancellationToken)
            ?? throw new KeyNotFoundException("1.5 준비 원장으로 인계할 자동집단을 찾을 수 없습니다.");
        var operatingState = await _demandOperatingSystem.운영상태조회Async(normalizedGroupId, cancellationToken)
            ?? throw new InvalidOperationException("1.0 수요 모집 OS 상태를 찾을 수 없습니다.");
        ValidateApprovedHandoff(operatingState);

        var ledgerId = 원장Id생성(normalizedGroupId);
        var existing = await _ledgerStore.원장조회Async(ledgerId, cancellationToken);
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

            var linkedOperatingState = await _demandOperatingSystem.후속원장연결Async(
                normalizedGroupId,
                operatingState.인계요청Id,
                ledgerId,
                cancellationToken);
            return ToResponse(existing, group, linkedOperatingState, created: false, alreadyProcessed: true);
        }
        if (existing is not null && !request.기대Revision.HasValue)
        {
            throw new InvalidOperationException("기존 1.5 준비 원장을 갱신하려면 기대 Revision이 필요합니다.");
        }

        var now = _timeProvider.GetUtcNow();
        var evaluation = 공동수입준비원장정책.평가(request, group, now);
        var status = ResolveStatus(evaluation);
        var saved = await _ledgerStore.원장저장Async(
            BuildSaveRequest(
                ledgerId,
                request,
                group,
                operatingState,
                evaluation,
                status,
                fingerprint,
                actorUserId.Trim(),
                actorDisplayName.Trim()),
            actorUserId.Trim(),
            cancellationToken);

        var linkedState = await _demandOperatingSystem.후속원장연결Async(
            normalizedGroupId,
            operatingState.인계요청Id,
            ledgerId,
            cancellationToken);
        return ToResponse(saved, group, linkedState, created: existing is null, alreadyProcessed: false);
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
        공동수입준비원장저장요청 request,
        공동구매자동집단응답 group,
        공동구매수요모집Os상태응답 operatingState,
        공동수입준비원장평가응답 evaluation,
        string status,
        string fingerprint,
        string actorUserId,
        string actorDisplayName)
    {
        var sourceReferences = string.IsNullOrWhiteSpace(group.공동구매주문집계원장Id)
            ? Array.Empty<커뮤니티포함원장참조Dto>()
            :
            [
                new 커뮤니티포함원장참조Dto
                {
                    원장Id = group.공동구매주문집계원장Id,
                    원장템플릿Key = CommunityLedgerTemplateKeys.GroupPurchase,
                    역할 = "원천 공동구매 수요 원장",
                    관계유형 = CommunityLedgerRelationTypes.Reference,
                    필수여부 = true,
                    표시순서 = 0
                }
            ];

        return new 커뮤니티원장저장요청
        {
            원장Id = ledgerId,
            기대Revision = request.기대Revision,
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
            제목 = $"{group.상품명} 공급·가격·무역 준비 원장",
            원함 = $"{group.총희망수량:0.####}{group.수량단위}의 확인된 수요를 공급자·가격·품목분류·국가별 수입 준비 자료와 연결합니다.",
            상태 = 커뮤니티원장상태.진행중,
            현재단계Key = status,
            대상OsCode = CommunityLedgerOperatingSystemCodes.GroupPurchaseImport,
            대상OsName = "공동수입 준비 OS",
            생성자UserId = actorUserId,
            생성자표시명 = actorDisplayName,
            블록목록 = BuildBlocks(request, group, operatingState, evaluation),
            블록담당자명시적갱신여부 = true,
            참여자목록 =
            [
                new 커뮤니티원장참여자Dto
                {
                    UserId = actorUserId,
                    DisplayName = actorDisplayName,
                    RoleLabel = "1.5 준비 원장 관리자",
                    ParticipationState = "검토중"
                }
            ],
            포함원장목록 = sourceReferences,
            다이어그램스냅샷 = BuildDiagram(ledgerId, evaluation),
            외부참조 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["AutoGroupId"] = group.자동집단Id,
                ["DemandHandoffRequestId"] = operatingState.인계요청Id,
                ["SourceGroupPurchaseLedgerId"] = group.공동구매주문집계원장Id,
                ["ProductKey"] = group.상품키,
                ["HsCode"] = group.HS코드,
                ["DestinationCountryCode"] = 공동수입준비국가코드.정규화(request.도착국가코드)
            },
            확장속성 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["WorkflowVersion"] = "1.5",
                ["ReadinessStatus"] = status,
                ["ExecutionBoundary"] = "NoContractNoPaymentNoFilingNoTransport",
                [멱등키속성] = request.요청멱등키.Trim(),
                [요청지문속성] = fingerprint
            }
        };
    }

    private static IReadOnlyList<커뮤니티원장블록Dto> BuildBlocks(
        공동수입준비원장저장요청 request,
        공동구매자동집단응답 group,
        공동구매수요모집Os상태응답 operatingState,
        공동수입준비원장평가응답 evaluation)
        =>
        [
            Block(준비자료BlockId, "1.5 준비 자료 원본", "recorded", new()
            {
                ["Json"] = JsonSerializer.Serialize(request, JsonOptions)
            }),
            Block("source-demand", "승인된 1.0 수요 집단", "approved-handoff", new()
            {
                ["AutoGroupId"] = group.자동집단Id,
                ["DemandHandoffRequestId"] = operatingState.인계요청Id,
                ["ProductKey"] = group.상품키,
                ["ProductName"] = group.상품명,
                ["HsCode"] = group.HS코드,
                ["DemandQuantity"] = group.총희망수량.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["QuantityUnit"] = group.수량단위
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
                ["TransportInstruction"] = bool.FalseString,
                ["WarehouseMutation"] = bool.FalseString
            })
        ];

    private static 커뮤니티원장블록Dto Block(
        string blockId,
        string title,
        string state,
        Dictionary<string, string> data,
        IReadOnlyList<공동수입책임초안>? responsibilities = null)
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
        공동수입준비원장평가응답 evaluation)
    {
        var nodes = new[]
        {
            Node("source-demand", "1.0 승인 수요", 40, evaluation.전문검토인계가능 ? "approved" : "linked"),
            Node("supplier-evidence", "공급자 근거", 260, evaluation.공급자근거구조완료 ? "structured" : "incomplete"),
            Node("quotes", "견적·MOQ", 480, evaluation.견적구조완료 ? "structured" : "incomplete"),
            Node("landed-cost", "예상 총원가", 700, evaluation.예상비용구조완료 ? "structured" : "incomplete"),
            Node("classification", "HS·HTS 후보", 920, evaluation.품목분류후보구조완료 ? "structured" : "incomplete"),
            Node("jurisdiction-review", "국가별 규제 검토", 1140, evaluation.국가별검토구조완료 ? "structured" : "incomplete"),
            Node("qualified-review", "자격 있는 검토자 인계", 1360, evaluation.전문검토인계가능 ? "ready" : "blocked")
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
            DiagramName = "1.0 수요 → 1.5 공급·가격·무역 준비",
            LedgerTemplateKey = CommunityLedgerTemplateKeys.GroupImport,
            WorkflowModeKey = CommunityLedgerOperatingSystemCodes.GroupPurchaseImport,
            Nodes = nodes,
            Edges = edges,
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["WorkflowVersion"] = "1.5",
                ["OperationalExecution"] = bool.FalseString
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

    private 공동수입준비원장응답 ToResponse(
        커뮤니티원장Dto ledger,
        공동구매자동집단응답 group,
        공동구매수요모집Os상태응답 operatingState,
        bool created,
        bool alreadyProcessed)
    {
        var requestJson = ledger.블록목록
            .FirstOrDefault(block => string.Equals(block.BlockId, 준비자료BlockId, StringComparison.OrdinalIgnoreCase))?
            .Data.GetValueOrDefault("Json");
        var request = string.IsNullOrWhiteSpace(requestJson)
            ? new 공동수입준비원장저장요청()
            : JsonSerializer.Deserialize<공동수입준비원장저장요청>(requestJson, JsonOptions)
              ?? new 공동수입준비원장저장요청();
        var evaluatedAt = _timeProvider.GetUtcNow();
        var savedAt = ledger.수정시각Utc == default
            ? evaluatedAt
            : new DateTimeOffset(DateTime.SpecifyKind(ledger.수정시각Utc, DateTimeKind.Utc));
        var evaluation = 공동수입준비원장정책.평가(request, group, evaluatedAt);

        return new 공동수입준비원장응답
        {
            원장Id = ledger.원장Id,
            Revision = ledger.Revision,
            생성됨 = created,
            이미처리됨 = alreadyProcessed,
            상태코드 = ResolveStatus(evaluation),
            자동집단Id = group.자동집단Id,
            원천수요운영체제Id = operatingState.운영체제Id,
            원천인계요청Id = operatingState.인계요청Id,
            상품키 = group.상품키,
            상품명 = group.상품명,
            원천Hs코드 = group.HS코드,
            모인수요수량 = group.총희망수량,
            수량단위 = group.수량단위,
            준비자료 = request,
            평가 = evaluation,
            저장시각Utc = savedAt
        };
    }

    private static string ResolveStatus(공동수입준비원장평가응답 evaluation)
        => evaluation.전문검토인계가능
            ? 공동수입준비원장상태코드.전문검토자료준비
            : 공동수입준비원장상태코드.초안;

    private static void ValidateIdempotencyKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("1.5 준비 원장 저장에는 요청 멱등 키가 필요합니다.");
        }
        if (value.Trim().Length > 160)
        {
            throw new InvalidOperationException("요청 멱등 키는 160자 이하여야 합니다.");
        }
    }

    private static string 요청지문(공동수입준비원장저장요청 request)
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

    public static string 원장Id생성(string 자동집단Id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);
        var digest = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(자동집단Id.Trim())))
            .ToLowerInvariant();
        return $"group-import-readiness-{digest[..32]}";
    }
}
