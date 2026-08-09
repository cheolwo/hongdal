using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.PresentationContracts.LearningCards;

namespace Ssalddel.Unity.Tests;

public sealed class ConceptCardPresentationTests
{
    private readonly ConceptCardDeckProjector projector = new();

    [Fact]
    public void 네종류카드는_Identity_Mode_Lineage와결정적Revision을보존한다()
    {
        var source = ValidInput();

        var first = projector.Project(source)!;
        var second = projector.Project(source)!;

        Assert.Equal("concept-card-deck:residential-potato:1", first.DeckStableId.Value);
        Assert.Equal("world:urban-market:sim:1/npc:sim:residential-group-representative:1", first.AnchorWorldObjectRef.ToString());
        Assert.Equal("Simulation", first.ModeCode);
        Assert.Equal(4, first.Cards.Length);
        Assert.Equal(
            new[]
            {
                ConceptCardKindCodes.Concept,
                ConceptCardKindCodes.Status,
                ConceptCardKindCodes.Reason,
                ConceptCardKindCodes.Action,
            },
            first.Cards.Select(value => value.CardKindCode));
        Assert.Equal(first.PresentationRevision, second.PresentationRevision);
        Assert.Equal(first.Cards.Select(value => value.PresentationRevision), second.Cards.Select(value => value.PresentationRevision));
        Assert.All(first.Cards, value => Assert.NotEmpty(value.SourceLineage));
        Assert.Equal("group-order:sim:1", first.Cards[0].SourceLineage[0].SourceStableId);
        Assert.Equal("concept-card:confirmed-demand:1", first.SelectedCardStableId!.Value.Value);
    }

    [Fact]
    public void 역할권한이없으면_Deck과선택을반환하지않는다()
    {
        var source = ValidInput();
        source.IsRoleAuthorized = false;

        Assert.Null(projector.Project(source));
    }

    [Fact]
    public void 승인되지않은Intent는_ActionCard에서노출하지않는다()
    {
        var source = ValidInput();
        source.AuthorizedIntentCodes = new[] { "ReviewSupplyPortfolio" };
        source.Cards.Single(value => value.CardKindCode == ConceptCardKindCodes.Action).ActionItems = new[]
        {
            AvailableAction("ReviewSupplyPortfolio", "공급처 추가 검토"),
            AvailableAction("ConfirmAllMembers", "주민 전체 확정"),
        };

        var result = projector.Project(source)!;
        var action = result.Cards.Single(value => value.CardKindCode == ConceptCardKindCodes.Action);

        var item = Assert.Single(action.ActionItems);
        Assert.Equal("ReviewSupplyPortfolio", item.IntentCode);
        Assert.DoesNotContain(action.ActionItems, value => value.IntentCode == "ConfirmAllMembers");
    }

    [Fact]
    public void 정상업무차단은_BlockReason과함께보존한다()
    {
        var source = ValidInput();
        source.AuthorizedIntentCodes = new[] { "ReviewSupplyPortfolio", "AdjustDeliverySchedule" };
        source.Cards.Single(value => value.CardKindCode == ConceptCardKindCodes.Action).ActionItems = new[]
        {
            AvailableAction("ReviewSupplyPortfolio", "공급처 추가 검토"),
            new ConceptCardActionDraft
            {
                IntentCode = "AdjustDeliverySchedule",
                LabelText = "납품 일정 조정",
                EffectCode = "PreviewOnly",
                IsAvailable = false,
                BlockReasonCodes = new[] { "SupplierScheduleUnavailable" },
            },
        };

        var action = projector.Project(source)!.Cards.Single(value => value.CardKindCode == ConceptCardKindCodes.Action);
        var blocked = action.ActionItems.Single(value => value.IntentCode == "AdjustDeliverySchedule");

        Assert.False(blocked.IsAvailable);
        Assert.Equal(new[] { "SupplierScheduleUnavailable" }, blocked.BlockReasonCodes);
    }

    [Fact]
    public void 권한필터로선택Card가제거되면_선택도제거한다()
    {
        var source = ValidInput();
        source.SelectedCardStableId = "concept-card:supply-action:1";
        source.AuthorizedIntentCodes = Array.Empty<string>();

        var result = projector.Project(source)!;

        Assert.DoesNotContain(result.Cards, value => value.CardKindCode == ConceptCardKindCodes.Action);
        Assert.Null(result.SelectedCardStableId);
    }

    [Fact]
    public void 중복CardStableId를거부한다()
    {
        var source = ValidInput();
        source.Cards[1].StableId = source.Cards[0].StableId;

        var error = Assert.Throws<InvalidOperationException>(() => projector.Project(source));

        Assert.StartsWith("DuplicateConceptCardStableId", error.Message);
    }

    [Fact]
    public void 근거Source가Lineage에없으면거부한다()
    {
        var source = ValidInput();
        source.Cards.Single(value => value.CardKindCode == ConceptCardKindCodes.Reason)
            .EvidenceRows[0].SourceStableId = "inventory:sim:missing";

        var error = Assert.Throws<InvalidOperationException>(() => projector.Project(source));

        Assert.Equal("ConceptCardEvidenceSourceMissing:inventory:sim:missing", error.Message);
    }

    [Fact]
    public void Mode가바뀌면_PresentationRevision도바뀐다()
    {
        var simulation = ValidInput();
        var operational = ValidInput();
        operational.Mode = DataRuntimeMode.Operational;

        Assert.NotEqual(
            projector.Project(simulation)!.PresentationRevision,
            projector.Project(operational)!.PresentationRevision);
    }

    [Fact]
    public void 공통Card계약에는_주민개인정보필드가없다()
    {
        var contractTypes = new[]
        {
            typeof(ConceptCardDeckPresentationModel),
            typeof(ConceptCardPresentationModel),
            typeof(ConceptCardEvidenceRow),
            typeof(ConceptCardActionItem),
            typeof(ConceptCardSourceLineageItem),
        };
        var forbidden = new[]
        {
            "UserId", "UserName", "Phone", "Contact", "Address",
            "BuildingUnit", "HouseholdUnit", "PaymentDetail",
        };

        var propertyNames = contractTypes
            .SelectMany(value => value.GetProperties())
            .Select(value => value.Name)
            .ToArray();

        Assert.All(forbidden, value => Assert.DoesNotContain(value, propertyNames));
    }

    private static ConceptCardDeckProjectionInput ValidInput()
    {
        var groupWorldId = new WorldStableId("world:group-demand:1");
        var productWorldId = new WorldStableId("world:product:potato");
        var source = new ConceptCardSourceLineageItem
        {
            SourceStableId = "group-order:sim:1",
            Revision = "group-order-revision:7",
            EvidenceAsOfUtc = new DateTimeOffset(2026, 8, 9, 0, 0, 0, TimeSpan.Zero),
            QualityCode = DataQualityCodes.Observed,
        };

        return new ConceptCardDeckProjectionInput
        {
            DeckStableId = "concept-card-deck:residential-potato:1",
            AnchorWorldObjectRef = new WorldObjectRef(
                new WorldContextId("world:urban-market:sim:1"),
                new WorldStableId("npc:sim:residential-group-representative:1")),
            RoleCode = "MarketManager",
            IntentCode = "ReviewOrdererGroupDemand",
            Mode = DataRuntimeMode.Simulation,
            SourceRevision = 7,
            InterpretationRevision = "interpretation:group-demand:7",
            SelectedCardStableId = "concept-card:confirmed-demand:1",
            IsRoleAuthorized = true,
            AuthorizedIntentCodes = new[] { "ReviewSupplyPortfolio" },
            Cards = new[]
            {
                new ConceptCardDraft
                {
                    Sequence = 1,
                    StableId = "concept-card:confirmed-demand:1",
                    CardKindCode = ConceptCardKindCodes.Concept,
                    ConceptStableId = "concept:confirmed-demand",
                    TitleText = "확정 수요",
                    SummaryText = "개별 주문 확인을 합산해 공급 계획에 반영하는 수요입니다.",
                    PrimaryValueText = "385 kg · 61명",
                    SimulationLabel = "Simulation",
                    SourceWorldIds = new[] { groupWorldId, productWorldId },
                    Cautions = new[] { "의향 수요와 다릅니다." },
                    RelatedConceptStableIds = new[] { "concept:intent-demand" },
                    SourceLineage = new[] { source },
                },
                new ConceptCardDraft
                {
                    Sequence = 2,
                    StableId = "concept-card:group-status:1",
                    CardKindCode = ConceptCardKindCodes.Status,
                    ConceptStableId = "concept:group-order-status",
                    TitleText = "공동주택 감자 주문",
                    SummaryText = "의향 410kg · 확정 385kg",
                    PrimaryValueText = "확정 385 kg",
                    SimulationLabel = "Simulation",
                    SourceWorldIds = new[] { groupWorldId, productWorldId },
                    SourceLineage = new[] { source },
                },
                new ConceptCardDraft
                {
                    Sequence = 3,
                    StableId = "concept-card:supply-gap-reason:1",
                    CardKindCode = ConceptCardKindCodes.Reason,
                    ConceptStableId = "concept:supply-coverage-gap",
                    TitleText = "왜 공급이 부족한가요?",
                    SummaryText = "확정 수요에서 현재 공급 가능량을 뺀 결과입니다.",
                    PrimaryValueText = "75 kg 부족",
                    SimulationLabel = "Simulation",
                    SourceWorldIds = new[] { groupWorldId, productWorldId },
                    EvidenceRows = new[]
                    {
                        new ConceptCardEvidenceDraft
                        {
                            LabelText = "확정 주문",
                            ValueText = "385 kg",
                            CalculationRoleCode = ConceptCardCalculationRoleCodes.Input,
                            SourceStableId = "group-order:sim:1",
                            RuleRevision = "supply-gap-rule:1",
                        },
                    },
                    SourceLineage = new[] { source },
                },
                new ConceptCardDraft
                {
                    Sequence = 4,
                    StableId = "concept-card:supply-action:1",
                    CardKindCode = ConceptCardKindCodes.Action,
                    ConceptStableId = "concept:supply-review-action",
                    TitleText = "가능한 행동",
                    SummaryText = "현재 허용된 공급 검토를 시작할 수 있습니다.",
                    SimulationLabel = "Simulation",
                    SourceWorldIds = new[] { groupWorldId, productWorldId },
                    ActionItems = new[]
                    {
                        AvailableAction("ReviewSupplyPortfolio", "공급처 추가 검토"),
                    },
                    SourceLineage = new[] { source },
                },
            },
        };
    }

    private static ConceptCardActionDraft AvailableAction(string intentCode, string label)
        => new()
        {
            IntentCode = intentCode,
            LabelText = label,
            EffectCode = "PreviewOnly",
            IsAvailable = true,
        };
}
