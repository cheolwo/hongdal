using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Interior.Contracts;
using Ssalddel.Interior.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "상품 근거 특성에서 파생되는 Item 효과 정의의 결정성·revision·권위 차단을 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3결정성검증,
    WorkOrderIds = new[] { "E9-WO-TOWN-HOUSE-INTERIOR-LAYOUT" },
    Boundary = "효과 정의 시험은 실제 WI 실행·WorldTick 적용·Scene·Game View 증거가 아니다.")]
public sealed class 상품특성효과DerivationEngineTests
{
    [Fact]
    public void 같은승인특성과규칙은입력순서와무관하게같은효과와Hash를만든다()
    {
        var engine = new 상품특성효과DerivationEngine();

        var first = engine.Derive(Request(reverseOrder: false));
        var second = engine.Derive(Request(reverseOrder: true));

        Assert.Equal(first.DefinitionHashSha256, second.DefinitionHashSha256);
        Assert.Equal(2, first.Effects.Length);
        Assert.Contains(first.Effects, value => value.EffectCode == "ReadingFocus" && value.Magnitude == 2);
        Assert.Contains(first.Effects, value => value.EffectCode == "EnergyUse" && value.Magnitude == 1);
        Assert.Equal(상품근거ItemCodes.DefinitionOnly, first.ActivationStateCode);
        Assert.Equal(상품근거ItemCodes.SimulationCoreRequired, first.EffectAuthorityCode);
    }

    [Fact]
    public void 효과규칙Revision이바뀌면정의Hash도바뀐다()
    {
        var engine = new 상품특성효과DerivationEngine();
        var firstRequest = Request();
        var secondRequest = Request();
        secondRequest.EffectRuleSet.Revision = "interior-item-effect.r2";
        secondRequest.EffectRuleSet.RuleSetHashSha256 = string.Empty;

        var first = engine.Derive(firstRequest);
        var second = engine.Derive(secondRequest);

        Assert.NotEqual(first.EffectRuleHashSha256, second.EffectRuleHashSha256);
        Assert.NotEqual(first.DefinitionHashSha256, second.DefinitionHashSha256);
    }

    [Fact]
    public void 승인Catalog에없는상품Reference는효과정의를만들수없다()
    {
        var request = Request();
        request.TraitProfile.ReferenceStableId = "reference:missing";
        request.TraitProfile.ProfileHashSha256 = string.Empty;

        var exception = Assert.Throws<ArgumentException>(() =>
            new 상품특성효과DerivationEngine().Derive(request));

        Assert.Contains("승인 Catalog", exception.Message, StringComparison.Ordinal);
    }

    internal static 상품근거ItemDerivationRequest Request(bool reverseOrder = false)
    {
        var reference = new ApprovedInteriorReference
        {
            ReferenceStableId = "reference:lamp:01",
            MarketplaceCode = "Amazon",
            CategoryCode = "Lighting",
            RoomRoleCodes = new[] { InteriorLayoutCodes.Bedroom },
            PlacementRoleCodes = new[] { "BedsideLighting" },
            ApprovedOriginalTitle = "Compact table lamp",
            SourceUrl = "https://www.amazon.com/example-lamp",
            ObservedAtUtc = "2026-08-24T00:00:00Z",
            RawObservationHashSha256 = new string('a', 64),
            SourceRevision = "amazon-fixture.r1",
        };
        var catalog = new ApprovedInteriorReferenceCatalog
        {
            StableId = "catalog:interior-reference",
            Revision = "catalog.r1",
            Items = new[] { reference },
        };
        catalog.CatalogHashSha256 = InteriorLayoutHash.ComputeCatalogHash(catalog);

        var traits = new[]
        {
            new 상품근거특성Value
            {
                TraitCode = "BrightnessClass",
                ValueCode = "Bright",
                UnitCode = 상품근거ItemCodes.NoUnit,
                DisplayName = "밝은 조명",
                EvidenceSummary = "승인된 상품 규격의 밝기 범주",
            },
            new 상품근거특성Value
            {
                TraitCode = "EnergyUseClass",
                ValueCode = "Low",
                UnitCode = 상품근거ItemCodes.NoUnit,
                DisplayName = "낮은 전력 사용",
                EvidenceSummary = "승인된 상품 규격의 전력 범주",
            },
        };
        var profile = new 상품근거특성Profile
        {
            StableId = "trait-profile:lamp:01",
            ReferenceStableId = reference.ReferenceStableId,
            CategoryCode = reference.CategoryCode,
            ProfileRevision = "lamp-traits.r1",
            ApprovalRevision = "trait-approval.r1",
            Traits = reverseOrder ? traits.Reverse().ToArray() : traits,
        };
        profile.ProfileHashSha256 = 상품근거ItemHash.ComputeTraitProfileHash(profile);

        var rules = new[]
        {
            new 상품특성효과Rule
            {
                StableId = "rule:lighting:bright:reading",
                CategoryCode = "Lighting",
                TraitCode = "BrightnessClass",
                TraitValueCode = "Bright",
                EffectCode = "ReadingFocus",
                Magnitude = 2,
                UnitCode = "Point",
                DisplayName = "독서 집중",
            },
            new 상품특성효과Rule
            {
                StableId = "rule:lighting:low-energy",
                CategoryCode = "Lighting",
                TraitCode = "EnergyUseClass",
                TraitValueCode = "Low",
                EffectCode = "EnergyUse",
                Magnitude = 1,
                UnitCode = "Point",
                DisplayName = "전력 소비",
            },
        };
        var ruleSet = new 상품특성효과RuleSet
        {
            StableId = "rule-set:interior-item-effect",
            Revision = "interior-item-effect.r1",
            Rules = reverseOrder ? rules.Reverse().ToArray() : rules,
        };
        ruleSet.RuleSetHashSha256 = 상품근거ItemHash.ComputeRuleSetHash(ruleSet);

        return new 상품근거ItemDerivationRequest
        {
            ItemDefinitionStableId = "interior-item:lamp:01",
            VisualKey = "Residential.Light.Table.Small",
            ReferenceCatalog = catalog,
            TraitProfile = profile,
            EffectRuleSet = ruleSet,
        };
    }
}
