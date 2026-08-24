using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
public sealed class SimulationWorld업무규칙집결저장결과
{
    public bool Inserted { get; set; }
    public string CatalogRevision { get; set; } = string.Empty;
    public string CatalogHashSha256 { get; set; } = string.Empty;
    public int FacilityCount { get; set; }
    public int CapabilityCount { get; set; }
    public int RuleCount { get; set; }
    public int BindingCount { get; set; }
    public int ScenarioRuleSetCount { get; set; }
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
    "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
    Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
public interface ISimulationWorld업무규칙집결Store
{
    Task<SimulationWorld업무규칙집결저장결과> 저장Async(
        SimulationWorld업무규칙집결원장 catalog,
        CancellationToken cancellationToken);
}

public interface ISimulationWorld업무규칙집결Reader
{
    Task<SimulationWorld업무규칙집결원장?> 조회Async(
        string catalogRevision,
        CancellationToken cancellationToken);
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
    "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
    Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
public sealed class SimulationWorld업무규칙집결JobShell
{
    public const string SpatialBuildNotFoundCode = "SimulationWorldBusinessRuleSpatialBuildNotFound";
    public const string SpatialNodeNotFoundCode = "SimulationWorldBusinessRuleSpatialNodeNotFound";
    private readonly ISimulationWorld공간실행Reader _spatialReader;
    private readonly ISimulationWorld업무규칙집결Store _store;

    public SimulationWorld업무규칙집결JobShell(
        ISimulationWorld공간실행Reader spatialReader,
        ISimulationWorld업무규칙집결Store store)
    {
        _spatialReader = spatialReader;
        _store = store;
    }

    public async Task<SimulationWorld업무규칙집결저장결과> 실행Async(
        string spatialBuildStableId,
        CancellationToken cancellationToken)
    {
        var spatial = await _spatialReader.조회Async(spatialBuildStableId, cancellationToken)
            ?? throw new InvalidOperationException(SpatialBuildNotFoundCode);
        var catalog = PyeongchangSimulationWorld업무규칙CatalogFactory.Create(
            spatial.BuildStableId,
            spatial.OutputHashSha256,
            spatial.AreaSetStableId);
        var nodeIds = new HashSet<string>(spatial.Nodes.Select(x => x.StableId), StringComparer.Ordinal);
        if (catalog.Facilities.Any(x => !nodeIds.Contains(x.SpatialNodeStableId)))
            throw new InvalidOperationException(SpatialNodeNotFoundCode);
        return await _store.저장Async(catalog, cancellationToken);
    }
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
    "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
    Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
public static class PyeongchangSimulationWorld업무규칙CatalogFactory
{
    public static SimulationWorld업무규칙집결원장 Create(
        string spatialBuildStableId,
        string spatialOutputHashSha256,
        string areaSetStableId)
    {
        var farm = Facility(PyeongchangSimulationWorldStableIds.대관령Farm시설, PyeongchangSimulationWorldStableIds.대관령Farm영역, SimulationWorld시설종류Codes.농장);
        var hub = Facility(PyeongchangSimulationWorldStableIds.진부Hub시설, PyeongchangSimulationWorldStableIds.진부Hub영역, SimulationWorld시설종류Codes.물류Hub);
        var mart = Facility(PyeongchangSimulationWorldStableIds.평창읍Mart시설, PyeongchangSimulationWorldStableIds.평창읍Town영역, SimulationWorld시설종류Codes.마트);
        var restaurant = Facility(PyeongchangSimulationWorldStableIds.평창읍음식점시설, PyeongchangSimulationWorldStableIds.평창읍Town영역, SimulationWorld시설종류Codes.음식점);
        var facilities = new[] { farm, hub, mart, restaurant };
        var capabilities = new List<SimulationWorld시설기능>();
        AddCapabilities(capabilities, farm, SimulationWorld시설기능Codes.생산, SimulationWorld시설기능Codes.수확, SimulationWorld시설기능Codes.포장, SimulationWorld시설기능Codes.출하, SimulationWorld시설기능Codes.방어);
        AddCapabilities(capabilities, hub, SimulationWorld시설기능Codes.입고, SimulationWorld시설기능Codes.검수, SimulationWorld시설기능Codes.보관, SimulationWorld시설기능Codes.상차, SimulationWorld시설기능Codes.하차);
        AddCapabilities(capabilities, mart, SimulationWorld시설기능Codes.주문, SimulationWorld시설기능Codes.입고, SimulationWorld시설기능Codes.보관, SimulationWorld시설기능Codes.진열, SimulationWorld시설기능Codes.판매);
        AddCapabilities(capabilities, restaurant, SimulationWorld시설기능Codes.주문, SimulationWorld시설기능Codes.입고, SimulationWorld시설기능Codes.보관, SimulationWorld시설기능Codes.소비);

        var rules = new[]
        {
            Rule(PyeongchangSimulationWorldStableIds.수확판로배분규칙, SimulationWorld업무규칙영역Codes.생산, "HarvestAllocation", "SimulationHarvestDispositionImpactEngine", "HarvestDispositionImpactPreviewRequest", "HarvestDispositionImpactPreview", "수확 Lot의 판로와 자원 예약 후보를 계산한다."),
            Rule(PyeongchangSimulationWorldStableIds.Farm출하화물규칙, SimulationWorld업무규칙영역Codes.생산, "OutboundCargo", "SimulationLogisticsMovementEngine", "LogisticsMovementPreviewRequest", "LogisticsMovementPreview", "출하 가능한 원천 allocation에서 Simulation 화물을 구성한다."),
            Rule(PyeongchangSimulationWorldStableIds.창고용량예약규칙, SimulationWorld업무규칙영역Codes.창고, "CapacityReservation", "Simulation창고자원효과계산기", "WarehouseResourceEffectRequest", "WarehouseResourceEffectResult", "입고 전 창고 보관 용량을 예약한다."),
            Rule(PyeongchangSimulationWorldStableIds.창고입고검수규칙, SimulationWorld업무규칙영역Codes.창고, "InboundInspection", "SimulationFreightReceiptEngine", "FreightReceiptPreviewRequest", "FreightReceiptPreview", "물류 거점 도착 화물을 검수 전 재고 후보로 유지한다."),
            Rule(PyeongchangSimulationWorldStableIds.창고적재규칙, SimulationWorld업무규칙영역Codes.창고, "PutAway", "SimulationWarehousePutAwayEngine", nameof(SimulationWarehousePutAwayPreviewRequest), nameof(SimulationDecisionPreviewSnapshot), "검수를 통과한 같은 입고 재고를 적재 담당 NPC의 작업으로 보관 위치에 적재한다."),
            Rule(PyeongchangSimulationWorldStableIds.물류이동규칙, SimulationWorld업무규칙영역Codes.물류, "Movement", "SimulationLogisticsMovementEngine", "LogisticsMovementPreviewRequest", "LogisticsMovementSnapshot", "공통 WorldTick으로 출발·이동·도착 상태를 전이한다."),
            Rule(PyeongchangSimulationWorldStableIds.화물배차규칙, SimulationWorld업무규칙영역Codes.화물, "Dispatch", "SimulationFreightDispatchEngine", "FreightDispatchPreviewRequest", "FreightDispatchPreview", "차량 용량과 후보 상태로 가상 배차 후보를 계산한다."),
            Rule(PyeongchangSimulationWorldStableIds.화물운송규칙, SimulationWorld업무규칙영역Codes.화물, "Transport", "SimulationFreightTransportEngine", "FreightTransportPreviewRequest", "FreightTransportSnapshot", "상차·운송·하차와 별도 인수완료 상태를 관리한다."),
            Rule(PyeongchangSimulationWorldStableIds.개별주문규칙, SimulationWorld업무규칙영역Codes.주문, "IndividualOrder", "SimulationIndividualOrderEngine", "IndividualOrderPreviewRequest", "IndividualOrderPreview", "마트 재고에 대한 개별 주문 후보와 예약을 계산한다."),
            Rule(PyeongchangSimulationWorldStableIds.Mart재고진열규칙, SimulationWorld업무규칙영역Codes.마트, "StockDisplay", "도심마트공급경영SimulationEngine", "MartSupplySimulationRequest", "MartSupplySimulationSnapshot", "마트 입고 재고와 진열·판매 가능 상태를 투영한다."),
            Rule(PyeongchangSimulationWorldStableIds.음식점식자재주문규칙, SimulationWorld업무규칙영역Codes.음식점, "IngredientOrder", "Simulation음식배달Engine", "FoodDeliveryPreviewRequest", "FoodDeliveryPreview", "음식점 식자재 주문·입고 후보를 Simulation으로 계산한다."),
            Rule(PyeongchangSimulationWorldStableIds.팀역할Card장착규칙, SimulationWorld업무규칙영역Codes.팀역할, "RoleCardEquip", "SimulationTeamRoleCardState", nameof(SimulationTeamRoleCardEquipRequest), nameof(SimulationTeamRoleCardStateSnapshot), "팀 공동 카드 사본을 구성원의 장착 칸으로 옮기고 현재 역할 투영을 갱신한다."),
            Rule(PyeongchangSimulationWorldStableIds.팀활동시작규칙, SimulationWorld업무규칙영역Codes.팀역할, "TeamActivityStart", "SimulationTeamRoleCardState", nameof(SimulationTeamActivityStartRequest), nameof(SimulationTeamRoleCardStateSnapshot), "장착 카드의 역할과 일치하는 팀 활동을 시작하고 카드와 행위자를 잠근다."),
            Rule(PyeongchangSimulationWorldStableIds.팀활동종료규칙, SimulationWorld업무규칙영역Codes.팀역할, "TeamActivityEnd", "SimulationTeamRoleCardState", nameof(SimulationTeamActivityEndRequest), nameof(SimulationTeamRoleCardStateSnapshot), "팀 활동을 종료하고 카드와 행위자의 잠금을 해제한다."),
            Rule(PyeongchangSimulationWorldStableIds.L2타일발견보상규칙, SimulationWorld업무규칙영역Codes.수집보상, "TileDiscoveryReward", "경영SimulationSessionAggregate", nameof(SimulationTileTraversalConfirmRequest), nameof(SimulationTileTraversalConfirmResponse), "서버가 현재 L2와 인접 이동을 확인한 뒤 팀 최초 발견과 결정적 수집 카드 기회를 판정한다."),
            Rule(PyeongchangSimulationWorldStableIds.농사완료보상규칙, SimulationWorld업무규칙영역Codes.수집보상, "FarmCompletionReward", "경영SimulationSessionAggregate", nameof(SimulationFarmWorkConfirmRequest), nameof(SimulationCollectibleCardRewardStateSnapshot), "플레이어 직접 밭갈기가 WorldTick에서 실제 완료될 때만 결정적 수집 카드 기회를 판정한다."),
            Rule(PyeongchangSimulationWorldStableIds.수집Card뽑기규칙, SimulationWorld업무규칙영역Codes.수집보상, "CollectibleCardDraw", "경영SimulationSessionAggregate", nameof(SimulationCollectibleCardDrawRequest), nameof(SimulationCollectibleCardDrawResponse), "개인 미개봉 기회의 소유자를 확인하고 팀이 아직 보유하지 않은 정의 중 하나를 서버가 결정한다."),
            Rule(PyeongchangSimulationWorldStableIds.수집Card양도규칙, SimulationWorld업무규칙영역Codes.수집보상, "CollectibleCardTransfer", "경영SimulationSessionAggregate", nameof(SimulationCollectibleCardTransferRequest), nameof(SimulationCollectibleCardTransferResponse), "같은 팀 구성원 사이에서 카드 사본의 소유권만 원격으로 변경한다."),
            Rule(PyeongchangSimulationWorldStableIds.전투시점확정규칙, SimulationWorld업무규칙영역Codes.전투, "CombatPerspectiveConfirm", "경영SimulationSessionAggregate", nameof(SimulationCombatPerspectiveConfirmRequest), nameof(SimulationFarmCombatStateSnapshot), "플레이어가 선택한 전투 시점을 고정하되, 활성 전투 박자 중에는 변경하지 못하게 한다."),
            Rule(PyeongchangSimulationWorldStableIds.전투박자시작규칙, SimulationWorld업무규칙영역Codes.전투, "CombatBeatStart", "경영SimulationSessionAggregate", nameof(SimulationCombatBeatStartRequest), nameof(SimulationCombatBeatSnapshot), "서버가 공격 유형과 충돌 시각, 시점별 방어·카운터 허용 구간을 결정한다."),
            Rule(PyeongchangSimulationWorldStableIds.전투반응판정규칙, SimulationWorld업무규칙영역Codes.전투, "CombatReactionConfirm", "경영SimulationSessionAggregate", nameof(SimulationCombatReactionConfirmRequest), nameof(SimulationCombatReactionSnapshot), "Unity가 제출한 행동과 반응 시각을 서버가 판정해 피해·방어 점수·위협 경직을 확정한다."),
            Rule(PyeongchangSimulationWorldStableIds.전술기회생성규칙, SimulationWorld업무규칙영역Codes.전투, "TacticalOpportunityDerivation", "경영SimulationSessionAggregate", nameof(SimulationCombatReactionSnapshot), nameof(SimulationTacticalOpportunitySnapshot), "성공한 영웅의 방어·카운터 판정에서 해당 전선과 다음 명령창에만 유효한 전술 기회를 결정적으로 생성한다."),
            Rule(PyeongchangSimulationWorldStableIds.전술명령확정규칙, SimulationWorld업무규칙영역Codes.전투, "TacticalOrderConfirm", "경영SimulationSessionAggregate", nameof(SimulationTacticalOrderConfirmRequest), nameof(SimulationFarmTacticalCombatStateSnapshot), "기회를 만든 영웅의 전진 공격·대형 사수·전술 후퇴 명령을 확정하고 다음 WorldTick에서 분대·전선·시설 결과를 판정한다."),
        };
        var bindings = new[]
        {
            Bind(farm, SimulationWorld시설기능Codes.수확, rules[0], 100), Bind(farm, SimulationWorld시설기능Codes.출하, rules[1], 90),
            Bind(hub, SimulationWorld시설기능Codes.보관, rules[2], 100), Bind(hub, SimulationWorld시설기능Codes.검수, rules[3], 100),
            Bind(hub, SimulationWorld시설기능Codes.보관, rules[4], 95), Bind(hub, SimulationWorld시설기능Codes.입고, rules[5], 90),
            Bind(hub, SimulationWorld시설기능Codes.상차, rules[6], 80), Bind(hub, SimulationWorld시설기능Codes.하차, rules[7], 80),
            Bind(mart, SimulationWorld시설기능Codes.주문, rules[8], 100), Bind(mart, SimulationWorld시설기능Codes.진열, rules[9], 90),
            Bind(restaurant, SimulationWorld시설기능Codes.주문, rules[10], 100),
            Bind(farm, SimulationWorld시설기능Codes.방어, rules[18], 100),
            Bind(farm, SimulationWorld시설기능Codes.방어, rules[19], 95),
            Bind(farm, SimulationWorld시설기능Codes.방어, rules[20], 90),
            Bind(farm, SimulationWorld시설기능Codes.방어, rules[21], 85),
            Bind(farm, SimulationWorld시설기능Codes.방어, rules[22], 80),
        };
        return new SimulationWorld업무규칙집결원장
        {
            CatalogRevision = "pyeongchang-farm-hub-town-business-rules.v6",
            SpatialBuildStableId = spatialBuildStableId,
            SpatialOutputHashSha256 = spatialOutputHashSha256,
            CreatedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
            Facilities = facilities,
            Capabilities = capabilities,
            Rules = rules,
            Parameters = Array.Empty<SimulationWorld업무Simulation규칙Parameter>(),
            Bindings = bindings,
            ScenarioRuleSets = new[]
            {
                new SimulationWorldScenario규칙묶음
                {
                    StableId = "scenario-rule-set:pyeongchang-farm-hub-town",
                    Revision = "r6",
                    AreaSetStableId = areaSetStableId,
                    Items = rules.Select((rule, index) => new SimulationWorldScenario규칙항목
                    {
                        RuleStableId = rule.StableId, RuleRevision = rule.Revision,
                        ApplyOrder = index + 1, Required = true,
                    }).ToArray(),
                },
            },
        };
    }

    private static SimulationWorld시설의미 Facility(string id, string nodeId, string type) => new()
    {
        StableId = id, SpatialNodeStableId = nodeId, FacilityTypeCode = type,
        EvidenceKindCode = SimulationWorld근거종류Codes.시나리오,
        EvidenceSourceStableId = "scenario:pyeongchang-farm-hub-town.v1",
        ConfidenceCode = "ScenarioDeclared", ScenarioAssigned = true,
    };

    private static void AddCapabilities(List<SimulationWorld시설기능> target, SimulationWorld시설의미 facility, params string[] codes)
    {
        foreach (var code in codes) target.Add(new SimulationWorld시설기능
        {
            StableId = "facility-capability:" + facility.StableId + ":" + code.ToLowerInvariant(),
            FacilityStableId = facility.StableId, CapabilityCode = code,
            EvidenceKindCode = SimulationWorld근거종류Codes.시나리오,
        });
    }

    private static SimulationWorld업무Simulation규칙 Rule(string id, string domain, string type, string engine, string input, string output, string description) => new()
    {
        StableId = id, Revision = "r1", DomainCode = domain, RuleTypeCode = type,
        StatusCode = SimulationWorld규칙상태Codes.활성, EngineKey = engine,
        InputContractKey = input, OutputContractKey = output, Deterministic = true,
        SimulationOnly = true, Description = description,
    };

    private static SimulationWorld객체업무규칙연결 Bind(SimulationWorld시설의미 facility, string capability, SimulationWorld업무Simulation규칙 rule, int priority) => new()
    {
        StableId = "business-rule-binding:" + facility.StableId + ":" + rule.StableId,
        FacilityStableId = facility.StableId, CapabilityCode = capability,
        RuleStableId = rule.StableId, RuleRevision = rule.Revision,
        ScopeCode = "Facility", Priority = priority,
        EvidenceKindCode = SimulationWorld근거종류Codes.시나리오, Active = true,
    };
}
}
