using System.Reflection;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 도심마트공급경영SimulationContractTests
{
    [Fact]
    public void 공급_수요_주문Snapshot은_각자Revision과Lineage로_검증된다()
    {
        var supply = ValidSupply();
        var demand = ValidDemand();
        var orders = ValidOrders();

        도심마트공급경영SimulationDataValidator.Validate(supply);
        도심마트공급경영SimulationDataValidator.Validate(demand);
        도심마트공급경영SimulationDataValidator.Validate(orders);

        Assert.Equal("supply-data:1", supply.DataRevision);
        Assert.Equal("demand-data:1", demand.DataRevision);
        Assert.Equal("order-data:1", orders.DataRevision);
        Assert.All(new 도심마트SimulationDataSnapshot[] { supply, demand, orders }, snapshot =>
        {
            Assert.Equal(SimulationModeCodes.Simulation, snapshot.ModeCode);
            Assert.False(snapshot.IsOperationalState);
            Assert.NotEmpty(snapshot.SourceLineage);
        });
    }

    [Fact]
    public void SimulationSnapshot을_Operational상태로표시하면_거부한다()
    {
        var snapshot = ValidSupply();
        snapshot.IsOperationalState = true;

        AssertError("SimulationOperationalBoundaryViolation",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void 공급제안이_존재하지않는공급처를참조하면_거부한다()
    {
        var snapshot = ValidSupply();
        snapshot.Offers[0].SupplierStableId = "supplier:missing";

        AssertError("SupplyOfferSupplierMissing",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void 계약안이_최소수량보다작은기본물량이면_거부한다()
    {
        var snapshot = ValidSupply();
        snapshot.ContractDrafts[0].BaseWeeklyQuantity = 4m;

        AssertError("SupplyContractDraftMinimumQuantityNotMet",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void 계약안은_원천Offer의_공급처상품통화단위를바꿀수없다()
    {
        var snapshot = ValidSupply();
        snapshot.ContractDrafts[0].QuantityUnitCode = "box";

        AssertError("SupplyContractDraftOfferBoundaryMismatch",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void DemandScenario는_인구기준과명시적선택률이필수다()
    {
        var snapshot = ValidDemand();
        snapshot.ProductSelectionRate = null;

        AssertError("DemandProductSelectionRateMissingOrInvalid",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));

        snapshot = ValidDemand();
        snapshot.PopulationBasisRevision = string.Empty;
        AssertError("DemandPopulationBasisRevisionMissing",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void Demand수량범위가_역전되면_거부한다()
    {
        var snapshot = ValidDemand();
        snapshot.DemandSegments[0].MaximumQuantity = 90m;

        AssertError("DemandSegmentQuantityRangeInvalid",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void 주문과Allocation의_중복StableId를_거부한다()
    {
        var snapshot = ValidOrders();
        snapshot.Orders = new[] { snapshot.Orders[0], snapshot.Orders[0] };

        AssertError("SimulationOrderStableIdDuplicate",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));

        snapshot = ValidOrders();
        snapshot.Allocations = new[] { snapshot.Allocations[0], snapshot.Allocations[0] };
        AssertError("SimulationOrderAllocationStableIdDuplicate",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void 주문Allocation은_주문참조와단위가_일치해야한다()
    {
        var snapshot = ValidOrders();
        snapshot.Allocations[0].OrderStableId = "simulation-order:missing";
        AssertError("SimulationOrderAllocationOrderMissing",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));

        snapshot = ValidOrders();
        snapshot.Allocations[0].QuantityUnitCode = "box";
        AssertError("SimulationOrderAllocationUnitMismatch",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void 활성Allocation합계가_주문할당수량과다르면_거부한다()
    {
        var snapshot = ValidOrders();
        snapshot.Allocations[0].Quantity = 5m;

        AssertError("SimulationOrderAllocationTotalMismatch",
            () => 도심마트공급경영SimulationDataValidator.Validate(snapshot));
    }

    [Fact]
    public void Simulation주문계약에는_개인정보와운영원장Id필드가없다()
    {
        var forbidden = new[]
        {
            "UserId", "AccountId", "CustomerId", "Address", "Phone", "Contact",
            "OperationalOrderId", "OperationalContractId", "OperationalPurchaseOrderId",
        };
        var contractTypes = typeof(도심마트공급경영SimulationDataSnapshot).Assembly
            .GetTypes()
            .Where(type => type.Namespace == "Ssalddel.Simulation.Contracts"
                && (type.Name.Contains("도심마트", StringComparison.Ordinal)
                    || type == typeof(SimulationDataLineage)))
            .ToArray();

        var propertyNames = contractTypes.SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Select(property => property.Name)
            .ToArray();

        Assert.All(forbidden, name => Assert.DoesNotContain(name, propertyNames));
        var references = typeof(도심마트공급경영SimulationDataSnapshot).Assembly
            .GetReferencedAssemblies().Select(value => value.Name).ToArray();
        Assert.DoesNotContain("Ssalddel.Contracts", references);
        Assert.DoesNotContain("Ssalddel.Domain", references);
        Assert.DoesNotContain("Ssalddel.Unity", references);
    }

    private static 도심마트공급경영SimulationDataSnapshot ValidSupply()
        => new()
        {
            SnapshotStableId = "supply-snapshot:potato:1",
            SessionStableId = "simulation-session:potato",
            ScenarioStableId = "scenario:urban-market-potato-4w",
            DataRevision = "supply-data:1",
            SourceLineage = Lineage("fixture:supply:potato", "fixture:1", "supply-contract-rule:1"),
            Suppliers = new[]
            {
                new 도심마트공급처SimulationData
                {
                    SupplierStableId = "supplier:local-coop",
                    DisplayName = "지역 생산자 협동조합",
                    SupplierTypeCode = "LocalCooperative",
                    MaximumWeeklyQuantity = 100m,
                    QuantityUnitCode = "kg",
                },
            },
            Offers = new[]
            {
                new 도심마트공급제안SimulationData
                {
                    OfferStableId = "offer:local-coop:potato:1",
                    SupplierStableId = "supplier:local-coop",
                    ProductStableId = "product:potato",
                    UnitPrice = 1850m,
                    CurrencyCode = "KRW",
                    MinimumOrderQuantity = 5m,
                    MaximumWeeklyQuantity = 100m,
                    QuantityUnitCode = "kg",
                    DeliveriesPerWeek = 2,
                    LeadTimeTicks = 2,
                    PaymentDueTicks = 14,
                    QualityStandardCode = "Fresh-A",
                    OfferRevision = "offer-rule:1",
                },
            },
            ContractDrafts = new[]
            {
                new 도심마트공급계약안SimulationData
                {
                    ContractDraftStableId = "contract-draft:local-coop:potato:1",
                    SourceOfferStableId = "offer:local-coop:potato:1",
                    SupplierStableId = "supplier:local-coop",
                    ProductStableId = "product:potato",
                    UnitPrice = 1850m,
                    CurrencyCode = "KRW",
                    MinimumOrderQuantity = 5m,
                    BaseWeeklyQuantity = 20m,
                    MaximumAdditionalWeeklyQuantity = 10m,
                    QuantityUnitCode = "kg",
                    DeliveriesPerWeek = 2,
                    LeadTimeTicks = 2,
                    PaymentDueTicks = 14,
                    QualityStandardCode = "Fresh-A",
                    StartsAtTick = 0,
                    EndsAtTick = 27,
                    DraftRevision = "contract-draft:1",
                },
            },
        };

    private static 도심마트수요시나리오DataSnapshot ValidDemand()
        => new()
        {
            SnapshotStableId = "demand-snapshot:potato:1",
            SessionStableId = "simulation-session:potato",
            ScenarioStableId = "scenario:urban-market-potato-4w",
            DataRevision = "demand-data:1",
            RegionStableId = "region:kr:seoul:sample",
            PopulationBasisRevision = "population-basis:2026-07",
            DemandRuleRevision = "demand-rule:1",
            ScenarioSeed = 240809,
            ProductSelectionRate = 0.20m,
            SimulationMarketShareRate = 0.05m,
            SeasonAssumptionCode = "NormalSeason",
            EventAssumptionCode = "NoEvent",
            LimitationText = "교육용 Simulation fixture이며 실제 주문 예측이 아닙니다.",
            SourceLineage = Lineage("potential-demand:sample-region", "potential-demand:1", "demand-rule:1"),
            DemandSegments = new[]
            {
                new 도심마트기간별수요SimulationData
                {
                    DemandSegmentStableId = "demand-segment:potato:week-1",
                    ProductStableId = "product:potato",
                    StartsAtTick = 0,
                    EndsAtTick = 6,
                    MinimumQuantity = 80m,
                    ExpectedQuantity = 100m,
                    MaximumQuantity = 120m,
                    QuantityUnitCode = "kg",
                },
            },
        };

    private static 도심마트주문SimulationDataSnapshot ValidOrders()
        => new()
        {
            SnapshotStableId = "order-snapshot:potato:tick-0",
            SessionStableId = "simulation-session:potato",
            ScenarioStableId = "scenario:urban-market-potato-4w",
            DataRevision = "order-data:1",
            DemandScenarioDataRevision = "demand-data:1",
            GenerationRuleRevision = "order-generation-rule:1",
            ScenarioSeed = 240809,
            QuantityBasisCode = 도심마트주문생성수량기준코드.ExpectedQuantity,
            SplitStrategyCode = 도심마트주문분할전략코드.EvenSplitWithSeededRemainder,
            GenerationLimitationText = "Simulation fixture orders only.",
            SourceLineage = Lineage("demand-snapshot:potato:1", "demand-data:1", "order-generation-rule:1"),
            Orders = new[]
            {
                new 도심마트주문SimulationData
                {
                    OrderStableId = "simulation-order:potato:0:1",
                    SourceDemandSegmentStableId = "demand-segment:potato:week-1",
                    DemandSourceTypeCode = SimulationDemandSourceTypeCodes.BaseScenarioDemand,
                    ProductStableId = "product:potato",
                    RegionStableId = "region:kr:seoul:sample",
                    CreatedTick = 0,
                    FulfillmentDueTick = 1,
                    RequestedQuantity = 10m,
                    AllocatedQuantity = 6m,
                    FulfilledQuantity = 4m,
                    UnfulfilledQuantity = 0m,
                    QuantityUnitCode = "kg",
                    StateCode = SimulationOrderStateCodes.PartiallyFulfilled,
                },
            },
            Allocations = new[]
            {
                new 도심마트주문재고할당SimulationData
                {
                    AllocationStableId = "simulation-order-allocation:potato:0:1",
                    OrderStableId = "simulation-order:potato:0:1",
                    InventoryStableId = "simulation-inventory:sales-floor:potato",
                    Quantity = 6m,
                    QuantityUnitCode = "kg",
                    StateCode = SimulationOrderAllocationStateCodes.Reserved,
                    AllocationRevision = 1,
                },
            },
        };

    private static SimulationDataLineage[] Lineage(string sourceStableId, string sourceRevision, string ruleRevision)
        => new[]
        {
            new SimulationDataLineage
            {
                SourceStableId = sourceStableId,
                SourceDataRevision = sourceRevision,
                RuleRevision = ruleRevision,
            },
        };

    private static void AssertError(string expected, Action action)
    {
        var exception = Assert.Throws<SimulationContractException>(action);
        Assert.Equal(expected, exception.ErrorCode);
    }
}
