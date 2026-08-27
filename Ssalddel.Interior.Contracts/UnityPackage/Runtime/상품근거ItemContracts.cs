using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Interior.Contracts
{
    public static class 상품근거ItemCodes
    {
        public const string SchemaVersion = "marketplace-grounded-interior-item.v1";
        public const string DefinitionOnly = "DefinitionOnly";
        public const string SimulationCoreRequired = "SimulationCoreRequired";
        public const string NoUnit = "None";
    }

    public sealed class 상품근거특성Value
    {
        public string TraitCode { get; set; } = string.Empty;
        public string ValueCode { get; set; } = string.Empty;
        public double? NumericValue { get; set; }
        public string UnitCode { get; set; } = 상품근거ItemCodes.NoUnit;
        public string DisplayName { get; set; } = string.Empty;
        public string EvidenceSummary { get; set; } = string.Empty;
    }

    public sealed class 상품근거특성Profile
    {
        public string StableId { get; set; } = string.Empty;
        public string ReferenceStableId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string ProfileRevision { get; set; } = string.Empty;
        public string ApprovalRevision { get; set; } = string.Empty;
        public 상품근거특성Value[] Traits { get; set; } = Array.Empty<상품근거특성Value>();
        public string ProfileHashSha256 { get; set; } = string.Empty;
    }

    public sealed class 상품특성효과Rule
    {
        public string StableId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string TraitCode { get; set; } = string.Empty;
        public string TraitValueCode { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public double Magnitude { get; set; }
        public string UnitCode { get; set; } = 상품근거ItemCodes.NoUnit;
        public string DisplayName { get; set; } = string.Empty;
    }

    public sealed class 상품특성효과RuleSet
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public 상품특성효과Rule[] Rules { get; set; } = Array.Empty<상품특성효과Rule>();
        public string RuleSetHashSha256 { get; set; } = string.Empty;
    }

    public sealed class 실내ItemEffect
    {
        public string EffectCode { get; set; } = string.Empty;
        public double Magnitude { get; set; }
        public string UnitCode { get; set; } = 상품근거ItemCodes.NoUnit;
        public string DisplayName { get; set; } = string.Empty;
        public string[] BasisTraitCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class 상품근거ItemDefinition
    {
        public string SchemaVersion { get; set; } = 상품근거ItemCodes.SchemaVersion;
        public string StableId { get; set; } = string.Empty;
        public string ReferenceStableId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string ReferenceCatalogRevision { get; set; } = string.Empty;
        public string ReferenceCatalogHashSha256 { get; set; } = string.Empty;
        public string TraitProfileRevision { get; set; } = string.Empty;
        public string TraitProfileHashSha256 { get; set; } = string.Empty;
        public string EffectRuleRevision { get; set; } = string.Empty;
        public string EffectRuleHashSha256 { get; set; } = string.Empty;
        public 상품근거특성Value[] Traits { get; set; } = Array.Empty<상품근거특성Value>();
        public 실내ItemEffect[] Effects { get; set; } = Array.Empty<실내ItemEffect>();
        public string ActivationStateCode { get; set; } = 상품근거ItemCodes.DefinitionOnly;
        public string EffectAuthorityCode { get; set; } = 상품근거ItemCodes.SimulationCoreRequired;
        public string DefinitionHashSha256 { get; set; } = string.Empty;
    }

    public sealed class 상품근거ItemDerivationRequest
    {
        public string ItemDefinitionStableId { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public ApprovedInteriorReferenceCatalog ReferenceCatalog { get; set; } = new();
        public 상품근거특성Profile TraitProfile { get; set; } = new();
        public 상품특성효과RuleSet EffectRuleSet { get; set; } = new();
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "승인 상품 Reference를 게임용 특성·효과 정의와 범주형 VisualKey로 결속하는 계약이다.",
        StepKey = "contract.marketplace-grounded-interior-item",
        DependsOnStepKeys = new[] { "contract.interior-layout-plan" },
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 19,
        Boundary = "효과 정의만 만들며 WI Confirm·WorldTick·재고·소유권·운영 상품 성능을 확정하지 않는다.")]
    public interface I상품특성효과DerivationEngine
    {
        상품근거ItemDefinition Derive(상품근거ItemDerivationRequest request);
    }
}
