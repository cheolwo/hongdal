using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Interior.Contracts;
using Ssalddel.Unity.Data.Interiors;

namespace Ssalddel.Unity.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Unity 상품 근거 Item 상세가 Synty VisualKey·특성·효과·출처를 읽기 전용으로 투영하는지 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3Unity소비자회귀,
    WorkOrderIds = new[] { "E9-WO-TOWN-HOUSE-INTERIOR-LAYOUT" },
    Boundary = "소비자 시험은 클릭 입력·WI 효과 적용·저장 Scene·Game View 증거가 아니다.")]
public sealed class 상품근거ItemDetailProjectionTests
{
    [Fact]
    public void 상세정보는범주형외형과효과근거를표시하지만World효과를적용하지않는다()
    {
        var catalog = Catalog();
        var definition = Definition(catalog);

        var snapshot = new 상품근거ItemDetailProjection().Project(
            "reference:lamp:01",
            definition,
            catalog);

        Assert.True(snapshot.IsAvailable);
        Assert.Equal("Residential.Light.Table.Small", snapshot.VisualKey);
        Assert.Single(snapshot.Traits);
        Assert.Single(snapshot.Effects);
        Assert.Equal("ReadingFocus", snapshot.Effects[0].EffectCode);
        Assert.False(snapshot.CanApplyWorldEffect);
        Assert.Contains("Simulation Core", snapshot.EffectBoundaryNotice, StringComparison.Ordinal);
        Assert.Contains("현실 성능 보증", snapshot.ReferenceBoundaryNotice, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogRevision이맞지않으면상세정보를닫는다()
    {
        var catalog = Catalog();
        var definition = Definition(catalog);
        definition.ReferenceCatalogRevision = "catalog.other";

        var snapshot = new 상품근거ItemDetailProjection().Project(
            "reference:lamp:01",
            definition,
            catalog);

        Assert.False(snapshot.IsAvailable);
        Assert.Equal("PinnedItemDefinitionMismatch", snapshot.UnavailableReasonCode);
        Assert.Empty(snapshot.Effects);
    }

    private static ApprovedInteriorReferenceCatalog Catalog()
        => new ApprovedInteriorReferenceCatalog
        {
            Revision = "catalog.r1",
            CatalogHashSha256 = "catalog-hash",
            Items = new[]
            {
                new ApprovedInteriorReference
                {
                    ReferenceStableId = "reference:lamp:01",
                    MarketplaceCode = "Amazon",
                    CategoryCode = "Lighting",
                    ApprovedOriginalTitle = "Compact table lamp",
                    SourceUrl = "https://www.amazon.com/example-lamp",
                    ObservedAtUtc = "2026-08-24T00:00:00Z",
                },
            },
        };

    private static 상품근거ItemDefinition Definition(ApprovedInteriorReferenceCatalog catalog)
        => new 상품근거ItemDefinition
        {
            StableId = "interior-item:lamp:01",
            ReferenceStableId = "reference:lamp:01",
            CategoryCode = "Lighting",
            VisualKey = "Residential.Light.Table.Small",
            ReferenceCatalogRevision = catalog.Revision,
            ReferenceCatalogHashSha256 = catalog.CatalogHashSha256,
            TraitProfileRevision = "lamp-traits.r1",
            TraitProfileHashSha256 = new string('a', 64),
            EffectRuleRevision = "interior-item-effect.r1",
            EffectRuleHashSha256 = new string('b', 64),
            ActivationStateCode = 상품근거ItemCodes.DefinitionOnly,
            EffectAuthorityCode = 상품근거ItemCodes.SimulationCoreRequired,
            Traits = new[]
            {
                new 상품근거특성Value
                {
                    TraitCode = "BrightnessClass",
                    ValueCode = "Bright",
                    UnitCode = 상품근거ItemCodes.NoUnit,
                    DisplayName = "밝은 조명",
                    EvidenceSummary = "승인된 상품 규격의 밝기 범주",
                },
            },
            Effects = new[]
            {
                new 실내ItemEffect
                {
                    EffectCode = "ReadingFocus",
                    Magnitude = 2,
                    UnitCode = "Point",
                    DisplayName = "독서 집중",
                    BasisTraitCodes = new[] { "BrightnessClass" },
                },
            },
        };
}
