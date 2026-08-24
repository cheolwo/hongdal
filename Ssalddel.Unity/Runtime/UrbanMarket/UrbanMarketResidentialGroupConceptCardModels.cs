using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.PresentationContracts.LearningCards;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class UrbanMarketResidentialGroupConceptCardCodes
    {
        public const string Role = "MarketManager";
        public const string Intent = "ReviewOrdererGroupDemand";
        public const string ReviewOrdererGroupDemand = "ReviewOrdererGroupDemand";
        public const string PreviewSupplyPlan = "PreviewSupplyPlan";
        public const string CompareSupplyOffers = "CompareSupplyOffers";
        public const string PreviewOnlyEffect = "PreviewOnly";
    }

    public sealed class UrbanMarketConceptCardSourceApiModel
    {
        public string SourceStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset? EvidenceAsOfUtc { get; set; }
        public string QualityCode { get; set; } = DataQualityCodes.Observed;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public static class UrbanMarketConceptCardSourceMapper
    {
        public static ConceptCardSourceLineageItem[] MapRequired(
            IEnumerable<UrbanMarketConceptCardSourceApiModel>? source,
            string emptyError)
        {
            var values = (source ?? Array.Empty<UrbanMarketConceptCardSourceApiModel>())
                .Select(value => value ?? throw new InvalidOperationException(emptyError))
                .Select(value =>
                {
                    StableDataId.EnsureValid(value.SourceStableId, nameof(value.SourceStableId));
                    if (string.IsNullOrWhiteSpace(value.Revision)
                        || string.IsNullOrWhiteSpace(value.QualityCode))
                        throw new InvalidOperationException("UrbanMarketConceptCardSourceInvalid");
                    return new ConceptCardSourceLineageItem
                    {
                        SourceStableId = value.SourceStableId.Trim(),
                        Revision = value.Revision.Trim(),
                        EvidenceAsOfUtc = value.EvidenceAsOfUtc,
                        QualityCode = value.QualityCode.Trim(),
                    };
                })
                .OrderBy(value => value.SourceStableId, StringComparer.Ordinal)
                .ToArray();
            if (values.Length == 0) throw new InvalidOperationException(emptyError);
            var duplicate = values.GroupBy(value => value.SourceStableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("DuplicateUrbanMarketConceptCardSource:" + duplicate.Key);
            return values;
        }
    }

    public sealed class UrbanMarketResidentialGroupDemandApiModel
    {
        public long Revision { get; set; }
        public string PerspectiveRevision { get; set; } = string.Empty;
        public string ModeCode { get; set; } = string.Empty;
        public bool IsRoleAuthorized { get; set; }
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string RepresentativeNpcStableId { get; set; } = string.Empty;
        public string InquiryStableId { get; set; } = string.Empty;
        public int IntentParticipantCount { get; set; }
        public decimal IntentQuantity { get; set; }
        public int ConfirmedParticipantCount { get; set; }
        public decimal ConfirmedQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string InquiryStateCode { get; set; } = string.Empty;
        public string PickupPointStableId { get; set; } = string.Empty;
        public string PickupPointStateCode { get; set; } = string.Empty;
        public string[] AvailableActionCodes { get; set; } = Array.Empty<string>();
        public UrbanMarketConceptCardSourceApiModel[] IntentSourceLineage { get; set; } =
            Array.Empty<UrbanMarketConceptCardSourceApiModel>();
        public UrbanMarketConceptCardSourceApiModel[] ConfirmedSourceLineage { get; set; } =
            Array.Empty<UrbanMarketConceptCardSourceApiModel>();
        public UrbanMarketConceptCardSourceApiModel[] PickupSourceLineage { get; set; } =
            Array.Empty<UrbanMarketConceptCardSourceApiModel>();
        public UrbanMarketConceptCardSourceApiModel[] InquirySourceLineage { get; set; } =
            Array.Empty<UrbanMarketConceptCardSourceApiModel>();
    }

    public sealed class UrbanMarketResidentialGroupDemandPresentationModel
    {
        public long Revision { get; set; }
        public string PerspectiveRevision { get; set; } = string.Empty;
        public string ModeCode { get; set; } = string.Empty;
        public bool IsRoleAuthorized { get; set; }
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string RepresentativeNpcStableId { get; set; } = string.Empty;
        public string InquiryStableId { get; set; } = string.Empty;
        public int IntentParticipantCount { get; set; }
        public decimal IntentQuantity { get; set; }
        public int ConfirmedParticipantCount { get; set; }
        public decimal ConfirmedQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string InquiryStateCode { get; set; } = string.Empty;
        public string PickupPointStableId { get; set; } = string.Empty;
        public string PickupPointStateCode { get; set; } = string.Empty;
        public string[] AvailableActionCodes { get; set; } = Array.Empty<string>();
        public ConceptCardSourceLineageItem[] IntentSourceLineage { get; set; } =
            Array.Empty<ConceptCardSourceLineageItem>();
        public ConceptCardSourceLineageItem[] ConfirmedSourceLineage { get; set; } =
            Array.Empty<ConceptCardSourceLineageItem>();
        public ConceptCardSourceLineageItem[] PickupSourceLineage { get; set; } =
            Array.Empty<ConceptCardSourceLineageItem>();
        public ConceptCardSourceLineageItem[] InquirySourceLineage { get; set; } =
            Array.Empty<ConceptCardSourceLineageItem>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class UrbanMarketResidentialGroupDemandMapper
    {
        public UrbanMarketResidentialGroupDemandPresentationModel Map(
            UrbanMarketResidentialGroupDemandApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Revision < 0 || string.IsNullOrWhiteSpace(source.PerspectiveRevision))
                throw new InvalidOperationException("ResidentialGroupDemandRevisionInvalid");
            if (source.ModeCode != "Simulation")
                throw new InvalidOperationException("ResidentialGroupOperationalFallbackForbidden");
            if (!source.IsRoleAuthorized)
            {
                return new UrbanMarketResidentialGroupDemandPresentationModel
                {
                    Revision = source.Revision,
                    PerspectiveRevision = source.PerspectiveRevision,
                    ModeCode = source.ModeCode,
                    IsRoleAuthorized = false,
                };
            }

            StableDataId.EnsureValid(source.OrdererGroupStableId, nameof(source.OrdererGroupStableId));
            StableDataId.EnsureValid(source.ProductStableId, nameof(source.ProductStableId));
            StableDataId.EnsureValid(source.RepresentativeNpcStableId, nameof(source.RepresentativeNpcStableId));
            StableDataId.EnsureValid(source.InquiryStableId, nameof(source.InquiryStableId));
            StableDataId.EnsureValid(source.PickupPointStableId, nameof(source.PickupPointStableId));
            if (source.IntentParticipantCount < 0 || source.ConfirmedParticipantCount < 0
                || source.IntentQuantity < 0m || source.ConfirmedQuantity < 0m
                || source.ConfirmedParticipantCount > source.IntentParticipantCount
                || source.ConfirmedQuantity > source.IntentQuantity
                || string.IsNullOrWhiteSpace(source.QuantityUnitCode)
                || string.IsNullOrWhiteSpace(source.InquiryStateCode)
                || string.IsNullOrWhiteSpace(source.PickupPointStateCode))
                throw new InvalidOperationException("ResidentialGroupDemandInvalid");

            return new UrbanMarketResidentialGroupDemandPresentationModel
            {
                Revision = source.Revision,
                PerspectiveRevision = source.PerspectiveRevision.Trim(),
                ModeCode = source.ModeCode,
                IsRoleAuthorized = true,
                OrdererGroupStableId = source.OrdererGroupStableId.Trim(),
                ProductStableId = source.ProductStableId.Trim(),
                RepresentativeNpcStableId = source.RepresentativeNpcStableId.Trim(),
                InquiryStableId = source.InquiryStableId.Trim(),
                IntentParticipantCount = source.IntentParticipantCount,
                IntentQuantity = source.IntentQuantity,
                ConfirmedParticipantCount = source.ConfirmedParticipantCount,
                ConfirmedQuantity = source.ConfirmedQuantity,
                QuantityUnitCode = source.QuantityUnitCode.Trim(),
                InquiryStateCode = source.InquiryStateCode.Trim(),
                PickupPointStableId = source.PickupPointStableId.Trim(),
                PickupPointStateCode = source.PickupPointStateCode.Trim(),
                AvailableActionCodes = NormalizeCodes(source.AvailableActionCodes),
                IntentSourceLineage = UrbanMarketConceptCardSourceMapper.MapRequired(
                    source.IntentSourceLineage, "ResidentialGroupIntentSourceMissing"),
                ConfirmedSourceLineage = UrbanMarketConceptCardSourceMapper.MapRequired(
                    source.ConfirmedSourceLineage, "ResidentialGroupConfirmedSourceMissing"),
                PickupSourceLineage = UrbanMarketConceptCardSourceMapper.MapRequired(
                    source.PickupSourceLineage, "ResidentialGroupPickupSourceMissing"),
                InquirySourceLineage = UrbanMarketConceptCardSourceMapper.MapRequired(
                    source.InquirySourceLineage, "ResidentialGroupInquirySourceMissing"),
            };
        }

        private static string[] NormalizeCodes(IEnumerable<string>? values)
            => (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
    }

    public sealed class UrbanMarketResidentialGroupConceptCardProjectionInput
    {
        public WorldContextId WorldId { get; set; }
        public long ProjectionRevision { get; set; }
        public string InterpretationRevision { get; set; } = string.Empty;
        public string SelectedCardStableId { get; set; } = string.Empty;
        public WorldStableId GroupWorldId { get; set; }
        public WorldStableId ProductWorldId { get; set; }
        public WorldStableId PickupWorldId { get; set; }
        public WorldStableId SupplyWorldId { get; set; }
        public WorldStableId InquiryWorldId { get; set; }
        public ResidentialGroupRepresentativeVisitSnapshot Visit { get; set; } = null!;
        public UrbanMarketResidentialGroupDemandPresentationModel GroupDemand { get; set; } = null!;
        public UrbanMarketSupplyManagementPresentationModel SupplyManagement { get; set; } = null!;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class UrbanMarketResidentialGroupConceptCardAdapter
    {
        private readonly ResidentialGroupRepresentativeVisitValidator visitValidator =
            new ResidentialGroupRepresentativeVisitValidator();
        private readonly ConceptCardDeckProjector projector = new ConceptCardDeckProjector();

        public ConceptCardDeckPresentationModel? Project(
            UrbanMarketResidentialGroupConceptCardProjectionInput source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.GroupDemand == null) throw new InvalidOperationException("ResidentialGroupCardDemandMissing");
            if (!source.GroupDemand.IsRoleAuthorized) return null;
            if (source.SupplyManagement == null) throw new InvalidOperationException("ResidentialGroupCardSupplyMissing");
            if (source.Visit == null) throw new InvalidOperationException("ResidentialGroupCardVisitMissing");
            visitValidator.Validate(source.Visit);
            ValidateBoundary(source);

            var group = source.GroupDemand;
            var supply = source.SupplyManagement;
            var unit = group.QuantityUnitCode;
            var groupWorlds = new[] { source.GroupWorldId, source.ProductWorldId };
            var supplyWorlds = new[] { source.SupplyWorldId, source.ProductWorldId };
            var supplySource = supply.SourceLineage.First();
            var inquiryAndSupply = Merge(group.InquirySourceLineage, supply.SourceLineage);
            var actions = ActionDrafts();

            return projector.Project(new ConceptCardDeckProjectionInput
            {
                DeckStableId = "concept-card-deck:" + group.OrdererGroupStableId,
                AnchorWorldObjectRef = new WorldObjectRef(
                    source.WorldId,
                    new WorldStableId(group.RepresentativeNpcStableId)),
                RoleCode = UrbanMarketResidentialGroupConceptCardCodes.Role,
                IntentCode = UrbanMarketResidentialGroupConceptCardCodes.Intent,
                Mode = DataRuntimeMode.Simulation,
                SourceRevision = source.ProjectionRevision,
                InterpretationRevision = source.InterpretationRevision,
                SelectedCardStableId = source.SelectedCardStableId,
                IsRoleAuthorized = true,
                AuthorizedIntentCodes = group.AvailableActionCodes,
                Cards = new[]
                {
                    Card(1, "group-order-status", group.OrdererGroupStableId, ConceptCardKindCodes.Status,
                        "concept:group-order-status", "공동주택 감자 주문",
                        "의향 " + Quantity(group.IntentQuantity, unit) + " · 확정 "
                        + Quantity(group.ConfirmedQuantity, unit),
                        "확정 " + Quantity(group.ConfirmedQuantity, unit),
                        groupWorlds.Append(source.InquiryWorldId),
                        Merge(group.IntentSourceLineage, group.ConfirmedSourceLineage,
                            group.InquirySourceLineage)),
                    Card(2, "confirmed-demand", group.OrdererGroupStableId, ConceptCardKindCodes.Concept,
                        "concept:confirmed-demand", "확정 수요",
                        "개별 주문자가 최종 확인해 공급 계획에 반영되는 수요입니다.",
                        Quantity(group.ConfirmedQuantity, unit) + " · "
                        + group.ConfirmedParticipantCount + "명",
                        groupWorlds, group.ConfirmedSourceLineage,
                        cautions: new[] { "의향 수요와 전체 hard demand를 같은 값으로 보지 않습니다." },
                        related: new[] { "concept:intent-demand", "concept:hard-demand" }),
                    Card(3, "intent-demand", group.OrdererGroupStableId, ConceptCardKindCodes.Concept,
                        "concept:intent-demand", "의향 수요",
                        "구매 의향을 집계한 공급 검토 참고값이며 확정 주문은 아닙니다.",
                        Quantity(group.IntentQuantity, unit) + " · "
                        + group.IntentParticipantCount + "명",
                        groupWorlds, group.IntentSourceLineage,
                        cautions: new[] { "확정 수요와 hard demand에 자동 합산하지 않습니다." },
                        related: new[] { "concept:confirmed-demand" }),
                    Card(4, "residential-pickup", group.OrdererGroupStableId, ConceptCardKindCodes.Concept,
                        "concept:residential-pickup", "공동수령",
                        "확정 fulfillment 이후 연결되는 공동주택 수령 후보입니다.",
                        group.PickupPointStateCode,
                        new[] { source.GroupWorldId, source.PickupWorldId },
                        group.PickupSourceLineage,
                        cautions: new[] { group.PickupPointStableId + " · 수령 완료를 의미하지 않습니다." }),
                    Card(5, "supply-status", group.OrdererGroupStableId, ConceptCardKindCodes.Status,
                        "concept:supply-status", "감자 공급 상태",
                        "전체 hard demand " + Quantity(supply.ManagementPreview.HardDemandQuantity, unit)
                        + " · 현재 재고 " + Quantity(supply.DemandAndOrders.CurrentAvailableInventory, unit)
                        + " · 처리 후 입고 "
                        + Quantity(supply.DemandAndOrders.InboundAfterProcessingPotentialQuantity, unit),
                        "공급 불가 " + Quantity(supply.DemandAndOrders.CannotCoverQuantity, unit),
                        supplyWorlds, supply.SourceLineage,
                        cautions: new[] { supply.DemandAndOrders.LimitationText }),
                    Card(6, "supply-gap-reason", group.OrdererGroupStableId, ConceptCardKindCodes.Reason,
                        "concept:supply-coverage-gap", "왜 공급이 부족한가요?",
                        "브리핑이 제공한 입력·조정·결과를 그대로 보여줍니다.",
                        Quantity(supply.DemandAndOrders.CannotCoverQuantity, unit) + " 부족",
                        supplyWorlds, supply.SourceLineage,
                        evidence: new[]
                        {
                            Evidence("미처리 주문", supply.DemandAndOrders.PendingOrderQuantity,
                                unit, ConceptCardCalculationRoleCodes.Input, supplySource,
                                supply.PresentationRevision),
                            Evidence("현재 즉시 충족", supply.DemandAndOrders.ImmediatelyFulfillableQuantity,
                                unit, ConceptCardCalculationRoleCodes.Adjustment, supplySource,
                                supply.PresentationRevision),
                            Evidence("입고 처리 후 가능", supply.DemandAndOrders.InboundAfterProcessingPotentialQuantity,
                                unit, ConceptCardCalculationRoleCodes.Adjustment, supplySource,
                                supply.PresentationRevision),
                            Evidence("기한 내 공급 불가", supply.DemandAndOrders.CannotCoverQuantity,
                                unit, ConceptCardCalculationRoleCodes.Result, supplySource,
                                supply.PresentationRevision),
                        }),
                    Card(7, "supply-review-action", group.OrdererGroupStableId, ConceptCardKindCodes.Action,
                        "concept:supply-review-action", "가능한 행동",
                        "현재 Perspective가 허용한 공급 검토만 표시합니다.",
                        string.Empty,
                        new[] { source.GroupWorldId, source.SupplyWorldId, source.InquiryWorldId },
                        inquiryAndSupply,
                        actions: actions),
                },
            });
        }

        private static void ValidateBoundary(UrbanMarketResidentialGroupConceptCardProjectionInput source)
        {
            var group = source.GroupDemand;
            var supply = source.SupplyManagement;
            if (source.ProjectionRevision < 0 || string.IsNullOrWhiteSpace(source.InterpretationRevision))
                throw new InvalidOperationException("ResidentialGroupCardRevisionInvalid");
            if (string.IsNullOrWhiteSpace(source.WorldId.Value)
                || new[] { source.GroupWorldId, source.ProductWorldId, source.PickupWorldId,
                    source.SupplyWorldId, source.InquiryWorldId }.Any(value => !value.IsDefined))
                throw new InvalidOperationException("ResidentialGroupCardWorldIdentityMissing");
            if (group.ModeCode != "Simulation" || supply.ModeCode != "Simulation")
                throw new InvalidOperationException("ResidentialGroupCardModeMismatch");
            if (group.ProductStableId != supply.ProductStableId
                || group.QuantityUnitCode != supply.QuantityUnitCode)
                throw new InvalidOperationException("ResidentialGroupCardProductUnitMismatch");
            if (group.OrdererGroupStableId != source.Visit.OrdererGroupStableId
                || group.RepresentativeNpcStableId != source.Visit.NpcStableId
                || group.InquiryStableId != source.Visit.InquiryStableId)
                throw new InvalidOperationException("ResidentialGroupCardVisitMismatch");
        }

        private static ConceptCardDraft Card(
            int sequence,
            string suffix,
            string scopeStableId,
            string kind,
            string conceptStableId,
            string title,
            string summary,
            string primary,
            IEnumerable<WorldStableId> worldIds,
            IEnumerable<ConceptCardSourceLineageItem> lineage,
            IEnumerable<string>? cautions = null,
            IEnumerable<string>? related = null,
            IEnumerable<ConceptCardEvidenceDraft>? evidence = null,
            IEnumerable<ConceptCardActionDraft>? actions = null)
            => new ConceptCardDraft
            {
                Sequence = sequence,
                StableId = "concept-card:" + suffix + ":" + scopeStableId,
                CardKindCode = kind,
                ConceptStableId = conceptStableId,
                TitleText = title,
                SummaryText = summary,
                PrimaryValueText = primary,
                SimulationLabel = "Simulation",
                SourceWorldIds = worldIds.Distinct().ToArray(),
                SourceLineage = Merge(lineage),
                Cautions = (cautions ?? Array.Empty<string>()).ToArray(),
                RelatedConceptStableIds = (related ?? Array.Empty<string>()).ToArray(),
                EvidenceRows = (evidence ?? Array.Empty<ConceptCardEvidenceDraft>()).ToArray(),
                ActionItems = (actions ?? Array.Empty<ConceptCardActionDraft>()).ToArray(),
            };

        private static ConceptCardEvidenceDraft Evidence(
            string label,
            decimal value,
            string unit,
            string role,
            ConceptCardSourceLineageItem source,
            string ruleRevision)
            => new ConceptCardEvidenceDraft
            {
                LabelText = label,
                ValueText = Quantity(value, unit),
                CalculationRoleCode = role,
                SourceStableId = source.SourceStableId,
                RuleRevision = ruleRevision,
            };

        private static ConceptCardActionDraft[] ActionDrafts()
            => new[]
            {
                Action(UrbanMarketResidentialGroupConceptCardCodes.ReviewOrdererGroupDemand,
                    "집단 수요 다시 보기"),
                Action(UrbanMarketResidentialGroupConceptCardCodes.PreviewSupplyPlan,
                    "공급 계획 Preview"),
                Action(UrbanMarketResidentialGroupConceptCardCodes.CompareSupplyOffers,
                    "공급처 제안 비교"),
            };

        private static ConceptCardActionDraft Action(string intent, string label)
            => new ConceptCardActionDraft
            {
                IntentCode = intent,
                LabelText = label,
                EffectCode = UrbanMarketResidentialGroupConceptCardCodes.PreviewOnlyEffect,
                IsAvailable = true,
            };

        private static ConceptCardSourceLineageItem[] Merge(
            params IEnumerable<ConceptCardSourceLineageItem>[] sources)
        {
            var groups = sources.SelectMany(value => value)
                .GroupBy(value => value.SourceStableId, StringComparer.Ordinal)
                .ToArray();
            var conflict = groups.FirstOrDefault(group => group
                .Select(value => value.Revision)
                .Distinct(StringComparer.Ordinal)
                .Count() > 1);
            if (conflict != null)
                throw new InvalidOperationException(
                    "UrbanMarketConceptCardSourceRevisionConflict:" + conflict.Key);
            return groups.Select(group => group.First())
                .OrderBy(value => value.SourceStableId, StringComparer.Ordinal)
                .ToArray();
        }

        private static string Quantity(decimal value, string unit)
            => value.ToString("0.##", CultureInfo.InvariantCulture) + " " + unit;
    }
}
