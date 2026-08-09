using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Unity.Tests;

public sealed class UrbanMarketSupplyManagementUnityBindingTests
{
    [Fact]
    public void Mapper는서버가계산한Surface를그대로검증해투영한다()
    {
        var mapped = new UrbanMarketSupplyManagementPresentationMapper().Map(Api());

        Assert.Equal(7, mapped.DemandAndOrders.AsOfTick);
        Assert.Equal(430m, mapped.DemandAndOrders.PendingOrderQuantity);
        Assert.Equal(2105m, mapped.ManagementPreview.HardDemandQuantity);
        Assert.Equal(3, mapped.SupplyPortfolio.Length);
    }

    [Fact]
    public void 현재재고와예정입고를즉시충족량으로합산하면거부한다()
    {
        var source = Api();
        source.DemandAndOrders.ImmediatelyFulfillableQuantity = 300m;

        AssertError("SupplyManagementDemandBriefingInvalid", () =>
            new UrbanMarketSupplyManagementPresentationMapper().Map(source));
    }

    [Fact]
    public void Operational실패를SimulationPayload로위장할수없다()
    {
        var source = Api();
        source.ModeCode = "Operational";

        AssertError("SupplyManagementOperationalFallbackForbidden", () =>
            new UrbanMarketSupplyManagementPresentationMapper().Map(source));
    }

    [Fact]
    public void Applicator는낮은Revision으로되돌리지않는다()
    {
        var mapper = new UrbanMarketSupplyManagementPresentationMapper();
        var latest = Api();
        latest.Revision = 2;
        var stale = Api();
        stale.Revision = 1;
        var target = new FakeSupplyTarget();
        var applicator = new UrbanMarketSupplyManagementPresentationApplicator();

        Assert.True(applicator.Apply(mapper.Map(latest), target));
        Assert.False(applicator.Apply(mapper.Map(stale), target));
        Assert.Equal(2, target.Last!.Revision);
    }

    [Fact]
    public void 대표Coordinator는활성Movement와대화를각Target에적용한다()
    {
        var visit = ResidentialGroupRepresentativeVisitFixture.Create();
        var npc = new FakeNpcTarget(visit.NpcStableId);
        var dialogue = new FakeDialogueTarget();

        new ResidentialGroupRepresentativeUnityCoordinator().Apply(
            visit, Dialogue(), npc, dialogue);

        Assert.Equal("market.manager-desk", npc.Last!.DestinationWaypointKey);
        Assert.Equal(visit.InquiryStableId, dialogue.Last!.InquiryStableId);
    }

    [Fact]
    public void 대표대화에는Command효과를넣을수없다()
    {
        var dialogue = Dialogue();
        dialogue.CommandEffectCode = "ServerCommand";

        AssertError("RepresentativeDialogueInvalid", () =>
            new ResidentialGroupRepresentativeUnityCoordinator().Apply(
                ResidentialGroupRepresentativeVisitFixture.Create(), dialogue,
                new FakeNpcTarget("npc:sim:residential-group-representative:1"),
                new FakeDialogueTarget()));
    }

    private static UrbanMarketSupplyManagementApiModel Api()
        => new UrbanMarketSupplyManagementApiModel
        {
            Revision = 1,
            PresentationRevision = "supply-presentation:1",
            ModeCode = "Simulation",
            ProductStableId = "product:potato",
            QuantityUnitCode = "kg",
            DemandAndOrders = new UrbanMarketDemandBriefingApiModel
            {
                AsOfTick = 7,
                TodayOrderCount = 3,
                TodayRequestedQuantity = 445m,
                PendingOrderQuantity = 430m,
                CurrentAvailableInventory = 100m,
                TodayScheduledInbound = 50m,
                ImmediatelyFulfillableQuantity = 100m,
                InboundAfterProcessingPotentialQuantity = 50m,
                CannotCoverQuantity = 280m,
                ReasonCodes = new[] { "SupplyCoverageGap" },
                LimitationText = "Simulation · 자동 발주 없음",
            },
            ManagementPreview = new UrbanMarketManagementPreviewApiModel
            {
                HardDemandQuantity = 2105m,
                FulfilledQuantity = 800m,
                UnfulfilledQuantity = 1305m,
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
                new UrbanMarketConceptCardSourceApiModel
                {
                    SourceStableId = "simulation-result:urban-market-potato:1",
                    Revision = "simulation-result-revision:1",
                    QualityCode = "Observed",
                },
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

    private static ResidentialGroupRepresentativeDialoguePresentationModel Dialogue()
        => new ResidentialGroupRepresentativeDialoguePresentationModel
        {
            InquiryStableId = "market-inquiry:sim:potato:1",
            TitleText = "공동주택 감자 공급 문의",
            DemandText = "의향 410kg · 확정 385kg",
            BoundaryText = "검토 대화는 주문이나 계약을 확정하지 않습니다.",
            CommandEffectCode = RepresentativeVisitCommandEffectCodes.None,
        };

    private static void AssertError(string expected, Action action)
    {
        var error = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal(expected, error.Message);
    }

    private sealed class FakeSupplyTarget : IUrbanMarketSupplyManagementPresentationTarget
    {
        public UrbanMarketSupplyManagementPresentationModel? Last { get; private set; }
        public void ApplySupplyManagement(UrbanMarketSupplyManagementPresentationModel model) => Last = model;
    }

    private sealed class FakeNpcTarget : INpcMovementPresentationTarget
    {
        public FakeNpcTarget(string stableId) => NpcStableId = stableId;
        public string NpcStableId { get; }
        public NpcMovementPresentationModel? Last { get; private set; }
        public void ApplyMovementPresentation(NpcMovementPresentationModel model) => Last = model;
    }

    private sealed class FakeDialogueTarget : IResidentialGroupRepresentativeDialogueTarget
    {
        public ResidentialGroupRepresentativeDialoguePresentationModel? Last { get; private set; }
        public void ApplyRepresentativeDialogue(ResidentialGroupRepresentativeDialoguePresentationModel model)
            => Last = model;
    }
}
