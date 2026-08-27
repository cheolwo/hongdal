using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Unity.Data.Interiors
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.ViewModel,
        "상품 근거 Item 정의를 범주형 Synty 표현과 읽기 전용 특성·효과 상세로 투영한다.",
        StepKey = "unity.marketplace-grounded-item-detail",
        DependsOnStepKeys = new[] { "domain.marketplace-grounded-item-effect-derive" },
        ExecutionStage = SsalddelCodeExecutionStage.Presentation,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        Effects = SsalddelCodeEffect.UiStateMutation,
        FlowOrder = 41,
        Boundary = "Unity는 특성·효과를 재계산하거나 World 상태에 적용하지 않고 승인 근거와 정의를 표시만 한다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E7,
        "플레이어가 실내 Item의 승인 특성·효과 정의와 현실 관측 근거를 읽는 상세 투영 책임이다.",
        WorkOrderIds = new[] { "E9-WO-TOWN-HOUSE-INTERIOR-LAYOUT" },
        Boundary = "상세 Projection은 Item 사용 WI·Simulation 효과 적용·저장 Scene·Game View 증거가 아니다.")]
    public sealed class 상품근거ItemDetailProjection
    {
        public const string ReferenceBoundaryNotice =
            "현실 상품 관측을 기반으로 한 참고 근거이며 게임 재고·소유 물품 또는 현실 성능 보증이 아닙니다.";
        public const string EffectBoundaryNotice =
            "표시 효과는 승인 특성을 고정 규칙으로 변환한 정의이며 실제 World 적용에는 WI Confirm과 Simulation Core 판정이 필요합니다.";

        public 상품근거ItemDetailSnapshot Project(
            string referenceStableId,
            상품근거ItemDefinition definition,
            ApprovedInteriorReferenceCatalog catalog)
        {
            if (string.IsNullOrWhiteSpace(referenceStableId))
                throw new ArgumentException("ReferenceStableId가 필요합니다.", nameof(referenceStableId));
            if (definition is null) throw new ArgumentNullException(nameof(definition));
            if (catalog is null) throw new ArgumentNullException(nameof(catalog));

            if (!string.Equals(referenceStableId.Trim(), definition.ReferenceStableId, StringComparison.Ordinal)
                || !string.Equals(definition.ReferenceCatalogRevision, catalog.Revision, StringComparison.Ordinal)
                || !string.Equals(definition.ReferenceCatalogHashSha256, catalog.CatalogHashSha256,
                    StringComparison.Ordinal)
                || !string.Equals(definition.ActivationStateCode, 상품근거ItemCodes.DefinitionOnly,
                    StringComparison.Ordinal)
                || !string.Equals(definition.EffectAuthorityCode, 상품근거ItemCodes.SimulationCoreRequired,
                    StringComparison.Ordinal))
            {
                return 상품근거ItemDetailSnapshot.Unavailable(referenceStableId, "PinnedItemDefinitionMismatch");
            }

            var reference = catalog.Items.SingleOrDefault(value => string.Equals(
                value.ReferenceStableId,
                referenceStableId.Trim(),
                StringComparison.Ordinal));
            if (reference is null
                || !string.Equals(reference.CategoryCode, definition.CategoryCode, StringComparison.Ordinal))
                return 상품근거ItemDetailSnapshot.Unavailable(referenceStableId, "ApprovedReferenceNotFound");

            return new 상품근거ItemDetailSnapshot
            {
                IsAvailable = true,
                ItemDefinitionStableId = definition.StableId,
                ReferenceStableId = definition.ReferenceStableId,
                CategoryCode = definition.CategoryCode,
                VisualKey = definition.VisualKey,
                MarketplaceCode = reference.MarketplaceCode,
                ApprovedOriginalTitle = reference.ApprovedOriginalTitle,
                SourceUrl = reference.SourceUrl,
                ObservedAtUtc = reference.ObservedAtUtc,
                TraitProfileRevision = definition.TraitProfileRevision,
                EffectRuleRevision = definition.EffectRuleRevision,
                Traits = definition.Traits
                    .OrderBy(value => value.TraitCode, StringComparer.Ordinal)
                    .Select(value => new 상품근거ItemTraitDetail
                    {
                        TraitCode = value.TraitCode,
                        ValueCode = value.ValueCode,
                        NumericValue = value.NumericValue,
                        UnitCode = value.UnitCode,
                        DisplayName = value.DisplayName,
                        EvidenceSummary = value.EvidenceSummary,
                    })
                    .ToArray(),
                Effects = definition.Effects
                    .OrderBy(value => value.EffectCode, StringComparer.Ordinal)
                    .Select(value => new 상품근거ItemEffectDetail
                    {
                        EffectCode = value.EffectCode,
                        Magnitude = value.Magnitude,
                        UnitCode = value.UnitCode,
                        DisplayName = value.DisplayName,
                        BasisTraitCodes = value.BasisTraitCodes.ToArray(),
                    })
                    .ToArray(),
                CanApplyWorldEffect = false,
                ReferenceBoundaryNotice = ReferenceBoundaryNotice,
                EffectBoundaryNotice = EffectBoundaryNotice,
            };
        }
    }

    public sealed class 상품근거ItemDetailSnapshot
    {
        public bool IsAvailable { get; set; }
        public string UnavailableReasonCode { get; set; } = string.Empty;
        public string ItemDefinitionStableId { get; set; } = string.Empty;
        public string ReferenceStableId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string MarketplaceCode { get; set; } = string.Empty;
        public string ApprovedOriginalTitle { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string ObservedAtUtc { get; set; } = string.Empty;
        public string TraitProfileRevision { get; set; } = string.Empty;
        public string EffectRuleRevision { get; set; } = string.Empty;
        public 상품근거ItemTraitDetail[] Traits { get; set; } = Array.Empty<상품근거ItemTraitDetail>();
        public 상품근거ItemEffectDetail[] Effects { get; set; } = Array.Empty<상품근거ItemEffectDetail>();
        public bool CanApplyWorldEffect { get; set; }
        public string ReferenceBoundaryNotice { get; set; } = string.Empty;
        public string EffectBoundaryNotice { get; set; } = string.Empty;

        public static 상품근거ItemDetailSnapshot Unavailable(string referenceStableId, string reasonCode)
            => new 상품근거ItemDetailSnapshot
            {
                ReferenceStableId = referenceStableId,
                UnavailableReasonCode = reasonCode,
            };
    }

    public sealed class 상품근거ItemTraitDetail
    {
        public string TraitCode { get; set; } = string.Empty;
        public string ValueCode { get; set; } = string.Empty;
        public double? NumericValue { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string EvidenceSummary { get; set; } = string.Empty;
    }

    public sealed class 상품근거ItemEffectDetail
    {
        public string EffectCode { get; set; } = string.Empty;
        public double Magnitude { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string[] BasisTraitCodes { get; set; } = Array.Empty<string>();
    }
}
