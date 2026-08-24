using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.PresentationContracts.LearningCards;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class UrbanMarketResidentialGroupConceptCardTests
{
    [Fact]
    public void 대표NpcDeck은_정해진일곱Card를순서대로투영한다()
    {
        var deck = Project(ValidInput())!;

        Assert.Equal(7, deck.Cards.Length);
        Assert.Equal(
            new[]
            {
                "공동주택 감자 주문", "확정 수요", "의향 수요", "공동수령",
                "감자 공급 상태", "왜 공급이 부족한가요?", "가능한 행동",
            },
            deck.Cards.Select(value => value.TitleText));
        Assert.Equal("npc:sim:residential-group-representative:1",
            deck.AnchorWorldObjectRef.ObjectId.Value);
        Assert.Equal("Simulation", deck.ModeCode);
    }

    [Fact]
    public void 집단확정수요와_전체HardDemand를서로덮어쓰지않는다()
    {
        var deck = Project(ValidInput())!;
        var confirmed = Card(deck, "확정 수요");
        var supply = Card(deck, "감자 공급 상태");

        Assert.Equal("385 kg · 61명", confirmed.PrimaryValueText);
        Assert.Contains("전체 hard demand 2105 kg", supply.SummaryText);
        Assert.DoesNotContain("2105", confirmed.PrimaryValueText);
        Assert.Equal("group-order:sim:potato:1", Assert.Single(confirmed.SourceLineage).SourceStableId);
        Assert.Equal("simulation-result:urban-market-potato:1", Assert.Single(supply.SourceLineage).SourceStableId);
    }

    [Fact]
    public void 공급부족Reason은_브리핑값을재계산하지않고근거행으로보존한다()
    {
        var reason = Card(Project(ValidInput())!, "왜 공급이 부족한가요?");

        Assert.Equal("75 kg 부족", reason.PrimaryValueText);
        Assert.Equal(
            new[] { "385 kg", "80 kg", "230 kg", "75 kg" },
            reason.EvidenceRows.Select(value => value.ValueText));
        Assert.Equal(
            new[]
            {
                ConceptCardCalculationRoleCodes.Input,
                ConceptCardCalculationRoleCodes.Adjustment,
                ConceptCardCalculationRoleCodes.Adjustment,
                ConceptCardCalculationRoleCodes.Result,
            },
            reason.EvidenceRows.Select(value => value.CalculationRoleCode));
    }

    [Fact]
    public void ActionCard는_기존Perspective가허용한행동만표시한다()
    {
        var input = ValidInput();
        input.GroupDemand.AvailableActionCodes = new[]
        {
            UrbanMarketResidentialGroupConceptCardCodes.ReviewOrdererGroupDemand,
            UrbanMarketResidentialGroupConceptCardCodes.PreviewSupplyPlan,
            UrbanMarketResidentialGroupConceptCardCodes.CompareSupplyOffers,
            "ConfirmAllMembers",
        };

        var actions = Card(Project(input)!, "가능한 행동").ActionItems;

        Assert.Equal(3, actions.Length);
        Assert.DoesNotContain(actions, value => value.IntentCode == "ConfirmAllMembers");
        Assert.All(actions, value =>
        {
            Assert.True(value.IsAvailable);
            Assert.Equal(UrbanMarketResidentialGroupConceptCardCodes.PreviewOnlyEffect, value.EffectCode);
        });
    }

    [Fact]
    public void 대표역할권한이없으면_Deck을만들지않는다()
    {
        var input = ValidInput();
        input.GroupDemand.IsRoleAuthorized = false;

        Assert.Null(Project(input));
    }

    [Fact]
    public void 집단수요와공급의상품또는단위가다르면거부한다()
    {
        var input = ValidInput();
        input.SupplyManagement.QuantityUnitCode = "box";

        AssertError("ResidentialGroupCardProductUnitMismatch", () => Project(input));
    }

    [Fact]
    public void 대표방문의집단_Npc_문의가다르면거부한다()
    {
        var input = ValidInput();
        input.GroupDemand.InquiryStableId = "market-inquiry:sim:potato:other";

        AssertError("ResidentialGroupCardVisitMismatch", () => Project(input));
    }

    [Fact]
    public void 집단수요SourceLineage가없으면Mapper에서거부한다()
    {
        var source = GroupApi();
        source.ConfirmedSourceLineage = Array.Empty<UrbanMarketConceptCardSourceApiModel>();

        AssertError("ResidentialGroupConfirmedSourceMissing", () =>
            new UrbanMarketResidentialGroupDemandMapper().Map(source));
    }

    [Fact]
    public void OperationalPayload를Simulation대표Deck으로사용할수없다()
    {
        var source = GroupApi();
        source.ModeCode = "Operational";

        AssertError("ResidentialGroupOperationalFallbackForbidden", () =>
            new UrbanMarketResidentialGroupDemandMapper().Map(source));
    }

    [Fact]
    public void 대표집단계약에는_주민개인정보필드가없다()
    {
        var properties = new[]
        {
            typeof(UrbanMarketResidentialGroupDemandApiModel),
            typeof(UrbanMarketResidentialGroupDemandPresentationModel),
        }.SelectMany(value => value.GetProperties()).Select(value => value.Name).ToArray();

        Assert.DoesNotContain("ResidentUserId", properties);
        Assert.DoesNotContain("ResidentName", properties);
        Assert.DoesNotContain("Contact", properties);
        Assert.DoesNotContain("Address", properties);
        Assert.DoesNotContain("BuildingUnit", properties);
        Assert.DoesNotContain("IndividualQuantity", properties);
        Assert.DoesNotContain("PaymentDetail", properties);
    }

    private static ConceptCardDeckPresentationModel? Project(
        UrbanMarketResidentialGroupConceptCardProjectionInput source)
        => new UrbanMarketResidentialGroupConceptCardAdapter().Project(source);

    private static ConceptCardPresentationModel Card(
        ConceptCardDeckPresentationModel deck,
        string title)
        => deck.Cards.Single(value => value.TitleText == title);

    private static UrbanMarketResidentialGroupConceptCardProjectionInput ValidInput()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();
        return new UrbanMarketResidentialGroupConceptCardProjectionInput
        {
            WorldId = new WorldContextId("world:urban-market:sim:1"),
            ProjectionRevision = 7,
            InterpretationRevision = "interpretation:residential-group-supply:7",
            SelectedCardStableId = "concept-card:confirmed-demand:orderer-group:residential:potato:1",
            GroupWorldId = new WorldStableId("world:orderer-group:residential-potato:1"),
            ProductWorldId = new WorldStableId("world:product:potato"),
            PickupWorldId = new WorldStableId("world:pickup-point:residential-sample-1"),
            SupplyWorldId = new WorldStableId("world:supply-management:potato:1"),
            InquiryWorldId = new WorldStableId("world:market-inquiry:potato:1"),
            Visit = visit,
            GroupDemand = new UrbanMarketResidentialGroupDemandMapper().Map(GroupApi()),
            SupplyManagement = new UrbanMarketSupplyManagementPresentationMapper().Map(SupplyApi()),
        };
    }

    private static UrbanMarketResidentialGroupDemandApiModel GroupApi()
        => new UrbanMarketResidentialGroupDemandApiModel
        {
            Revision = 7,
            PerspectiveRevision = "market-manager-group-perspective:7",
            ModeCode = "Simulation",
            IsRoleAuthorized = true,
            OrdererGroupStableId = "orderer-group:residential:potato:1",
            ProductStableId = "product:potato",
            RepresentativeNpcStableId = "npc:sim:residential-group-representative:1",
            InquiryStableId = "market-inquiry:sim:potato:1",
            IntentParticipantCount = 67,
            IntentQuantity = 410m,
            ConfirmedParticipantCount = 61,
            ConfirmedQuantity = 385m,
            QuantityUnitCode = "kg",
            InquiryStateCode = "Submitted",
            PickupPointStableId = "pickup-point:residential:sample-1",
            PickupPointStateCode = "Candidate",
            AvailableActionCodes = new[]
            {
                UrbanMarketResidentialGroupConceptCardCodes.ReviewOrdererGroupDemand,
                UrbanMarketResidentialGroupConceptCardCodes.PreviewSupplyPlan,
                UrbanMarketResidentialGroupConceptCardCodes.CompareSupplyOffers,
            },
            IntentSourceLineage = new[] { Source("group-purchase:sim:potato:1", "group-purchase-revision:7") },
            ConfirmedSourceLineage = new[] { Source("group-order:sim:potato:1", "group-order-revision:7") },
            PickupSourceLineage = new[] { Source("pickup-point:residential:sample-1", "pickup-point-revision:1") },
            InquirySourceLineage = new[] { Source("market-inquiry:sim:potato:1", "market-inquiry-revision:4") },
        };

    private static UrbanMarketSupplyManagementApiModel SupplyApi()
        => new UrbanMarketSupplyManagementApiModel
        {
            Revision = 7,
            PresentationRevision = "supply-presentation:7",
            ModeCode = "Simulation",
            ProductStableId = "product:potato",
            QuantityUnitCode = "kg",
            DemandAndOrders = new UrbanMarketDemandBriefingApiModel
            {
                AsOfTick = 7,
                TodayOrderCount = 37,
                TodayRequestedQuantity = 385m,
                PendingOrderQuantity = 385m,
                CurrentAvailableInventory = 80m,
                TodayScheduledInbound = 230m,
                ImmediatelyFulfillableQuantity = 80m,
                InboundAfterProcessingPotentialQuantity = 230m,
                CannotCoverQuantity = 75m,
                ReasonCodes = new[] { "SupplyCoverageGap" },
                LimitationText = "Simulation · 입고는 검수와 작업 이후에만 사용할 수 있습니다.",
            },
            ManagementPreview = new UrbanMarketManagementPreviewApiModel
            {
                HardDemandQuantity = 2105m,
                FulfilledQuantity = 1730m,
                UnfulfilledQuantity = 375m,
                PurchaseCost = 900000m,
                EndingCash = 4100000m,
                OutstandingPaymentAmount = 0m,
                WasteQuantity = 0m,
                ReceivingWorkload = 500m,
            },
            SupplyPortfolio = new[]
            {
                Supplier("supplier:local-coop", .4m),
                Supplier("supplier:national-wholesaler", .5m),
                Supplier("supplier:spot-market", .1m),
            },
            SourceLineage = new[]
            {
                Source("simulation-result:urban-market-potato:1", "simulation-result-revision:7"),
            },
        };

    private static UrbanMarketSupplierPortfolioApiModel Supplier(string id, decimal share)
        => new UrbanMarketSupplierPortfolioApiModel
        {
            SupplierStableId = id,
            AcceptedQuantity = 100m,
            AcceptedSupplyShareRate = share,
            PurchaseCost = 100000m,
        };

    private static UrbanMarketConceptCardSourceApiModel Source(string id, string revision)
        => new UrbanMarketConceptCardSourceApiModel
        {
            SourceStableId = id,
            Revision = revision,
            EvidenceAsOfUtc = new DateTimeOffset(2026, 8, 9, 0, 0, 0, TimeSpan.Zero),
            QualityCode = DataQualityCodes.Observed,
        };

    private static void AssertError(string expected, Action action)
    {
        var error = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(expected, error.Message);
    }
}
