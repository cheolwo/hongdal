using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

public interface ISimulationWorld업무규칙집결Store
{
    Task<SimulationWorld업무규칙집결저장결과> 저장Async(
        SimulationWorld업무규칙집결원장 catalog,
        CancellationToken cancellationToken);
}

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

public static class PyeongchangSimulationWorld업무규칙CatalogFactory
{
    public static SimulationWorld업무규칙집결원장 Create(
        string spatialBuildStableId,
        string spatialOutputHashSha256,
        string areaSetStableId)
    {
        var farm = Facility("facility:sim:pyeongchang:daegwallyeong-farm", "area:sim:pyeongchang:daegwallyeong-farm", SimulationWorld시설종류Codes.농장);
        var hub = Facility("facility:sim:pyeongchang:jinbu-hub", "area:sim:pyeongchang:jinbu-hub", SimulationWorld시설종류Codes.물류Hub);
        var mart = Facility("facility:sim:pyeongchang:pyeongchang-town-mart", "area:sim:pyeongchang:pyeongchang-town", SimulationWorld시설종류Codes.마트);
        var restaurant = Facility("facility:sim:pyeongchang:pyeongchang-town-restaurant", "area:sim:pyeongchang:pyeongchang-town", SimulationWorld시설종류Codes.음식점);
        var facilities = new[] { farm, hub, mart, restaurant };
        var capabilities = new List<SimulationWorld시설기능>();
        AddCapabilities(capabilities, farm, SimulationWorld시설기능Codes.생산, SimulationWorld시설기능Codes.수확, SimulationWorld시설기능Codes.포장, SimulationWorld시설기능Codes.출하);
        AddCapabilities(capabilities, hub, SimulationWorld시설기능Codes.입고, SimulationWorld시설기능Codes.검수, SimulationWorld시설기능Codes.보관, SimulationWorld시설기능Codes.상차, SimulationWorld시설기능Codes.하차);
        AddCapabilities(capabilities, mart, SimulationWorld시설기능Codes.주문, SimulationWorld시설기능Codes.입고, SimulationWorld시설기능Codes.보관, SimulationWorld시설기능Codes.진열, SimulationWorld시설기능Codes.판매);
        AddCapabilities(capabilities, restaurant, SimulationWorld시설기능Codes.주문, SimulationWorld시설기능Codes.입고, SimulationWorld시설기능Codes.보관, SimulationWorld시설기능Codes.소비);

        var rules = new[]
        {
            Rule("rule:simulation:farm:harvest-allocation", SimulationWorld업무규칙영역Codes.생산, "HarvestAllocation", "SimulationHarvestDispositionImpactEngine", "HarvestDispositionImpactPreviewRequest", "HarvestDispositionImpactPreview", "수확 Lot의 판로와 자원 예약 후보를 계산한다."),
            Rule("rule:simulation:farm:outbound-cargo", SimulationWorld업무규칙영역Codes.생산, "OutboundCargo", "SimulationLogisticsMovementEngine", "LogisticsMovementPreviewRequest", "LogisticsMovementPreview", "출하 가능한 원천 allocation에서 Simulation 화물을 구성한다."),
            Rule("rule:simulation:warehouse:capacity-reservation", SimulationWorld업무규칙영역Codes.창고, "CapacityReservation", "Simulation창고자원효과계산기", "WarehouseResourceEffectRequest", "WarehouseResourceEffectResult", "입고 전 창고 보관 용량을 예약한다."),
            Rule("rule:simulation:warehouse:inbound-inspection", SimulationWorld업무규칙영역Codes.창고, "InboundInspection", "SimulationFreightReceiptEngine", "FreightReceiptPreviewRequest", "FreightReceiptPreview", "Hub 도착 화물을 검수 전 재고 후보로 유지한다."),
            Rule("rule:simulation:logistics:movement", SimulationWorld업무규칙영역Codes.물류, "Movement", "SimulationLogisticsMovementEngine", "LogisticsMovementPreviewRequest", "LogisticsMovementSnapshot", "공통 WorldTick으로 출발·이동·도착 상태를 전이한다."),
            Rule("rule:simulation:freight:dispatch", SimulationWorld업무규칙영역Codes.화물, "Dispatch", "SimulationFreightDispatchEngine", "FreightDispatchPreviewRequest", "FreightDispatchPreview", "차량 용량과 후보 상태로 가상 배차 후보를 계산한다."),
            Rule("rule:simulation:freight:transport", SimulationWorld업무규칙영역Codes.화물, "Transport", "SimulationFreightTransportEngine", "FreightTransportPreviewRequest", "FreightTransportSnapshot", "상차·운송·하차와 별도 인수완료 상태를 관리한다."),
            Rule("rule:simulation:order:individual", SimulationWorld업무규칙영역Codes.주문, "IndividualOrder", "SimulationIndividualOrderEngine", "IndividualOrderPreviewRequest", "IndividualOrderPreview", "마트 재고에 대한 개별 주문 후보와 예약을 계산한다."),
            Rule("rule:simulation:mart:stock-display", SimulationWorld업무규칙영역Codes.마트, "StockDisplay", "도심마트공급경영SimulationEngine", "MartSupplySimulationRequest", "MartSupplySimulationSnapshot", "마트 입고 재고와 진열·판매 가능 상태를 투영한다."),
            Rule("rule:simulation:restaurant:ingredient-order", SimulationWorld업무규칙영역Codes.음식점, "IngredientOrder", "Simulation음식배달Engine", "FoodDeliveryPreviewRequest", "FoodDeliveryPreview", "음식점 식자재 주문·입고 후보를 Simulation으로 계산한다."),
        };
        var bindings = new[]
        {
            Bind(farm, SimulationWorld시설기능Codes.수확, rules[0], 100), Bind(farm, SimulationWorld시설기능Codes.출하, rules[1], 90),
            Bind(hub, SimulationWorld시설기능Codes.보관, rules[2], 100), Bind(hub, SimulationWorld시설기능Codes.검수, rules[3], 100),
            Bind(hub, SimulationWorld시설기능Codes.입고, rules[4], 90), Bind(hub, SimulationWorld시설기능Codes.상차, rules[5], 80),
            Bind(hub, SimulationWorld시설기능Codes.하차, rules[6], 80), Bind(mart, SimulationWorld시설기능Codes.주문, rules[7], 100),
            Bind(mart, SimulationWorld시설기능Codes.진열, rules[8], 90), Bind(restaurant, SimulationWorld시설기능Codes.주문, rules[9], 100),
        };
        return new SimulationWorld업무규칙집결원장
        {
            CatalogRevision = "pyeongchang-farm-hub-town-business-rules.v1",
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
                    Revision = "r1",
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
