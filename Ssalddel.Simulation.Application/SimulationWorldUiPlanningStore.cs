using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
public sealed class SimulationWorldUI기획저장결과
{
    public bool Inserted { get; set; }
    public string CatalogRevision { get; set; } = string.Empty;
    public string CatalogHashSha256 { get; set; } = string.Empty;
    public int SurfaceCount { get; set; }
    public int InformationItemCount { get; set; }
    public int StatePresentationCount { get; set; }
    public int ActionCandidateCount { get; set; }
    public int RuleBindingCount { get; set; }
}

public interface ISimulationWorldUI기획Store
{
    Task<SimulationWorldUI기획저장결과> 저장Async(
        SimulationWorldUI기획원장 plan,
        SimulationWorld업무규칙집결원장 businessRules,
        CancellationToken cancellationToken);
}

public interface ISimulationWorldUI기획Assembler
{
    SimulationWorldUI기획원장 조립(SimulationWorld업무규칙집결원장 businessRules);
}

public sealed class PyeongchangSimulationWorldUI기획Assembler : ISimulationWorldUI기획Assembler
{
    public SimulationWorldUI기획원장 조립(SimulationWorld업무규칙집결원장 businessRules) =>
        PyeongchangSimulationWorldUI기획Factory.Create(businessRules);
}

public sealed class SimulationWorldUI기획JobShell
{
    public const string BusinessRuleCatalogNotFoundCode = "SimulationWorldUiPlanningBusinessRuleCatalogNotFound";
    private readonly ISimulationWorld업무규칙집결Reader _businessRuleReader;
    private readonly ISimulationWorldUI기획Assembler _assembler;
    private readonly ISimulationWorldUI기획Store _store;

    public SimulationWorldUI기획JobShell(
        ISimulationWorld업무규칙집결Reader businessRuleReader,
        ISimulationWorldUI기획Assembler assembler,
        ISimulationWorldUI기획Store store)
    {
        _businessRuleReader = businessRuleReader;
        _assembler = assembler;
        _store = store;
    }

    public async Task<SimulationWorldUI기획저장결과> 실행Async(
        string businessRuleCatalogRevision,
        CancellationToken cancellationToken)
    {
        var rules = await _businessRuleReader.조회Async(businessRuleCatalogRevision, cancellationToken)
            ?? throw new InvalidOperationException(BusinessRuleCatalogNotFoundCode);
        var plan = _assembler.조립(rules);
        return await _store.저장Async(plan, rules, cancellationToken);
    }
}

public static class PyeongchangSimulationWorldUI기획Factory
{
    public static SimulationWorldUI기획원장 Create(SimulationWorld업무규칙집결원장 rules)
    {
        const string roleMapEvidence = "ui-evidence:figma:platform-role-map";
        const string ordererFlowEvidence = "ui-evidence:figma:orderer-flow";
        const string commonHomeEvidence = "ui-evidence:figma:common-home";
        var evidence = new[]
        {
            Evidence(roleMapEvidence, "2427:243", "역할 앱 01~09 서비스 계층", "RoleLedgerProjectionAndCanonicalRequery"),
            Evidence(ordererFlowEvidence, "2308:990", "주문자 화면 계층·이동 지도", "DiscoverCompareParticipateReadiness"),
            Evidence(commonHomeEvidence, "2177:64", "살뜰 공통 홈", "DiscoveryParticipationBoundaryRoleSelection"),
        };
        var farm = Surface("farm-work", PyeongchangSimulationWorldStableIds.대관령Farm시설, "농장 출하 준비", "FarmWorkyard", 10, SimulationWorldUI역할Codes.화주, "Request", commonHomeEvidence);
        var farmDefense = Surface("farm-defense", PyeongchangSimulationWorldStableIds.대관령Farm시설, "농장 전투 방어", "FarmDefense", 15, SimulationWorldUI역할Codes.화주, "PerspectiveTelegraphReact", commonHomeEvidence, SimulationWorldUI화면종류Codes.WorldHud);
        var farmTactical = Surface("farm-tactical-order", PyeongchangSimulationWorldStableIds.대관령Farm시설, "농장 영웅 전술 명령", "FarmTacticalOrder", 16, SimulationWorldUI역할Codes.화주, "OpportunityPreviewConfirmResolve", commonHomeEvidence, SimulationWorldUI화면종류Codes.WorldHud);
        var hub = Surface("hub-operations", PyeongchangSimulationWorldStableIds.진부Hub시설, "진부면 물류 거점 입출고", "LogisticsOperations", 20, SimulationWorldUI역할Codes.창고관리자, "ReceiveInspectStore", roleMapEvidence);
        var freightRequest = Surface("freight-request", PyeongchangSimulationWorldStableIds.진부Hub시설, "화물 의뢰·배차", "FreightDesk", 30, SimulationWorldUI역할Codes.화주, "RequestQuoteDispatch", roleMapEvidence);
        var freightExecution = Surface("freight-execution", PyeongchangSimulationWorldStableIds.진부Hub시설, "상차·운송·하차", "FreightRoute", 40, SimulationWorldUI역할Codes.기사, "AcceptLoadTransportUnload", roleMapEvidence, SimulationWorldUI화면종류Codes.WorldHud);
        var mart = Surface("mart-order", PyeongchangSimulationWorldStableIds.평창읍Mart시설, "마트 상품 발견·비교·주문", "MartOrderDesk", 50, SimulationWorldUI역할Codes.주문자, "DiscoverCompareParticipate", ordererFlowEvidence);
        var restaurant = Surface("restaurant-supply", PyeongchangSimulationWorldStableIds.평창읍음식점시설, "음식점 주문 수신·준비", "RestaurantReceiving", 60, SimulationWorldUI역할Codes.음식점, "ReceivePreparePickup", roleMapEvidence);
        var surfaces = new[] { farm, farmDefense, farmTactical, hub, freightRequest, freightExecution, mart, restaurant };
        var information = surfaces.SelectMany(Information).ToArray();
        var states = surfaces.SelectMany(States).ToArray();
        var actions = new List<SimulationWorldUI행동후보기획>();
        AddActions(actions, farm, "HarvestDispositionImpact", "HarvestDispositionImpact.Confirm", "수확 판로");
        AddActions(actions, farmDefense, "FarmCombatBeat", nameof(SimulationCombatReactionConfirmRequest), "전투 박자 대응");
        AddActions(actions, farmTactical, "FarmTacticalOrder", nameof(SimulationTacticalOrderConfirmRequest), "영웅 전술 명령");
        AddActions(actions, hub, "FreightReceipt", "FreightReceipt.Confirm", "입고 검수");
        AddActions(actions, freightRequest, "FreightDispatch", "FreightDispatch.Confirm", "화물 배차");
        AddActions(actions, freightExecution, "FreightTransport", "FreightTransport.Confirm", "운송 단계");
        AddActions(actions, mart, "IndividualOrder", "IndividualOrder.Confirm", "개별 주문");
        AddActions(actions, restaurant, "FoodDelivery", "FoodDelivery.Confirm", "식재료 주문");

        var surfaceByRule = new Dictionary<string, SimulationWorldUI화면영역기획>(StringComparer.Ordinal)
        {
            [PyeongchangSimulationWorldStableIds.수확판로배분규칙] = farm,
            [PyeongchangSimulationWorldStableIds.Farm출하화물규칙] = farm,
            [PyeongchangSimulationWorldStableIds.창고용량예약규칙] = hub,
            [PyeongchangSimulationWorldStableIds.창고입고검수규칙] = hub,
            [PyeongchangSimulationWorldStableIds.창고적재규칙] = hub,
            [PyeongchangSimulationWorldStableIds.물류이동규칙] = hub,
            [PyeongchangSimulationWorldStableIds.화물배차규칙] = freightRequest,
            [PyeongchangSimulationWorldStableIds.화물운송규칙] = freightExecution,
            [PyeongchangSimulationWorldStableIds.개별주문규칙] = mart,
            [PyeongchangSimulationWorldStableIds.Mart재고진열규칙] = mart,
            [PyeongchangSimulationWorldStableIds.음식점식자재주문규칙] = restaurant,
            [PyeongchangSimulationWorldStableIds.전투시점확정규칙] = farmDefense,
            [PyeongchangSimulationWorldStableIds.전투박자시작규칙] = farmDefense,
            [PyeongchangSimulationWorldStableIds.전투반응판정규칙] = farmDefense,
            [PyeongchangSimulationWorldStableIds.전술기회생성규칙] = farmTactical,
            [PyeongchangSimulationWorldStableIds.전술명령확정규칙] = farmTactical,
        };
        var ruleByKey = rules.Rules.ToDictionary(x => x.StableId + "@" + x.Revision, StringComparer.Ordinal);
        var bindings = rules.Bindings.Where(x => x.Active).Select(sourceBinding =>
        {
            if (!surfaceByRule.TryGetValue(sourceBinding.RuleStableId, out var surface))
                throw new InvalidOperationException(SimulationWorldUI기획Validator.InvalidCode + ":활성 업무 규칙 연결에 대응하는 UI 영역 정의가 없습니다.");
            if (!ruleByKey.ContainsKey(sourceBinding.RuleStableId + "@" + sourceBinding.RuleRevision))
                throw new InvalidOperationException(SimulationWorldUI기획Validator.InvalidCode + ":UI에 연결할 업무 규칙을 찾을 수 없습니다.");
            return new SimulationWorldUI업무규칙연결
            {
                StableId = "ui-rule-binding:" + sourceBinding.StableId,
                BusinessRuleBindingStableId = sourceBinding.StableId,
                FacilityCapabilityCode = sourceBinding.CapabilityCode,
                RuleStableId = sourceBinding.RuleStableId,
                RuleRevision = sourceBinding.RuleRevision,
                SurfaceStableId = surface.StableId,
                PurposeCode = SimulationWorldUI규칙연결목적Codes.상태설명과행동제안,
                Priority = sourceBinding.Priority,
            };
        }).ToArray();
        var plan = new SimulationWorldUI기획원장
        {
            SchemaVersion = SimulationWorldUI기획Validator.CurrentSchemaVersion,
            CatalogRevision = "pyeongchang-farm-hub-town-ui-plan.v5",
            BusinessRuleCatalogRevision = rules.CatalogRevision,
            BusinessRuleCatalogHashSha256 = SimulationWorld업무규칙집결Validator.ComputeHash(rules),
            CreatedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
            DesignEvidence = evidence,
            Surfaces = surfaces,
            InformationItems = information,
            StatePresentations = states,
            ActionCandidates = actions,
            RuleBindings = bindings,
        };
        SimulationWorldUI기획Validator.Validate(plan, rules);
        return plan;
    }

    private static SimulationWorldUI설계근거 Evidence(string id, string nodeId, string title, string structure) => new SimulationWorldUI설계근거
    {
        StableId = id, ProviderCode = "Figma", FileKey = "0KhuQLc1MleUBIQnARC21Z",
        NodeId = nodeId, KoreanTitle = title, ObservedStructureCode = structure,
        ObservedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
    };

    private static SimulationWorldUI화면영역기획 Surface(string suffix, string facility, string title, string anchor, int order, string role, string workflowStage, string evidenceId, string kind = SimulationWorldUI화면종류Codes.업무상세판) => new SimulationWorldUI화면영역기획
    {
        StableId = "ui-surface:sim:pyeongchang:" + suffix,
        FacilityStableId = facility,
        SurfaceKindCode = kind,
        PerspectiveCode = SimulationWorldUI관점Codes.Simulation참여자,
        RoleCode = role,
        WorkflowStageCode = workflowStage,
        KoreanTitle = title,
        AnchorSemanticCode = anchor,
        DisplayOrder = order,
        DefaultVisible = false,
        DesignEvidenceStableId = evidenceId,
    };

    private static IEnumerable<SimulationWorldUI정보항목기획> Information(SimulationWorldUI화면영역기획 surface)
    {
        return new[]
        {
            Info(surface, "summary", "Summary", "업무 요약", "BusinessSummary", "Text", 100, false),
            Info(surface, "status", "Status", "현재 상태", "CanonicalSimulationState", "Status", 90, true),
            Info(surface, "next-step", "NextStep", "다음 단계", "AllowedNextAction", "Text", 80, false),
            Info(surface, "evidence", "Evidence", "판정 근거", "RuleAndSourceEvidence", "EvidenceCard", 70, true),
            Info(surface, "limitation", "Limitation", "표현 한계", "SimulationBoundary", "Warning", 60, true),
            Info(surface, "canonical-refresh", "Refresh", "확정 뒤 원장 재조회", "CanonicalRequeryStatus", "Status", 50, true),
        };
    }

    private static SimulationWorldUI정보항목기획 Info(SimulationWorldUI화면영역기획 surface, string suffix, string kind, string label, string semantic, string format, int priority, bool provenance) => new SimulationWorldUI정보항목기획
    {
        StableId = surface.StableId + ":info:" + suffix,
        SurfaceStableId = surface.StableId,
        InformationKindCode = kind,
        KoreanLabel = label,
        ValueSemanticCode = semantic,
        SourceContractKey = nameof(SimulationWorldUIProjectionItem),
        FormatCode = format,
        Priority = priority,
        ProvenanceRequired = provenance,
    };

    private static IEnumerable<SimulationWorldUI상태표현기획> States(SimulationWorldUI화면영역기획 surface)
    {
        var values = new[]
        {
            new[] { SimulationWorldUI상태Codes.대기, "대기", "Neutral", "Quiet" },
            new[] { SimulationWorldUI상태Codes.불러오는중, "불러오는 중", "Info", "Loading" },
            new[] { SimulationWorldUI상태Codes.준비, "검토 가능", "Info", "Ready" },
            new[] { SimulationWorldUI상태Codes.미리보기준비, "미리보기 확인", "Attention", "Preview" },
            new[] { SimulationWorldUI상태Codes.진행중, "진행 중", "Progress", "Active" },
            new[] { SimulationWorldUI상태Codes.완료, "완료", "Success", "Completed" },
            new[] { SimulationWorldUI상태Codes.차단, "진행할 수 없음", "Warning", "Blocked" },
            new[] { SimulationWorldUI상태Codes.오류, "불러오기 실패", "Error", "Error" },
        };
        return values.Select((value, index) => new SimulationWorldUI상태표현기획
        {
            StableId = surface.StableId + ":state:" + value[0].ToLowerInvariant(),
            SurfaceStableId = surface.StableId,
            StateCode = value[0], KoreanLabel = value[1], SeverityCode = value[2],
            PresentationIntentCode = value[3],
            BlocksMutationActions = value[0] == SimulationWorldUI상태Codes.불러오는중 || value[0] == SimulationWorldUI상태Codes.차단 || value[0] == SimulationWorldUI상태Codes.오류,
            DisplayOrder = index + 1,
        });
    }

    private static void AddActions(List<SimulationWorldUI행동후보기획> target, SimulationWorldUI화면영역기획 surface, string capability, string command, string subject, string stableIdPrefix = "")
    {
        target.Add(Action(surface, stableIdPrefix + "inspect", SimulationWorldUI행동종류Codes.조회, subject + " 상세 보기", capability + ".Inspect", null, 10));
        target.Add(Action(surface, stableIdPrefix + "preview", SimulationWorldUI행동종류Codes.미리보기, subject + " 미리보기", capability + ".Preview", null, 20));
        target.Add(Action(surface, stableIdPrefix + "confirm", SimulationWorldUI행동종류Codes.확정, subject + " 확정", capability + ".Confirm", command, 30));
    }

    private static SimulationWorldUI행동후보기획 Action(SimulationWorldUI화면영역기획 surface, string suffix, string kind, string label, string capability, string? command, int order) => new SimulationWorldUI행동후보기획
    {
        StableId = surface.StableId + ":action:" + suffix,
        SurfaceStableId = surface.StableId,
        ActionKindCode = kind,
        KoreanLabel = label,
        CapabilityKey = capability,
        ServerCommandKey = command,
        RequiresPreview = kind == SimulationWorldUI행동종류Codes.확정,
        RequiresExplicitConfirmation = kind == SimulationWorldUI행동종류Codes.확정,
        RequiresExpectedRevision = kind == SimulationWorldUI행동종류Codes.확정,
        SimulationOnly = true,
        DisplayOrder = order,
    };
}
}
