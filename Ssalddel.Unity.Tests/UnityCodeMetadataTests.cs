using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.Battles;
using Ssalddel.Unity.Community;
using Ssalddel.Unity.PublicData;
using Ssalddel.Unity.Survival;
using Ssalddel.Unity.Warehouse;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class UnityCodeMetadataTests
{
    [Fact]
    public void 마지막성공로딩흐름은_공통Runtime과세Coordinator를_연결한다()
    {
        var metadata = SsalddelCodeMetadataReader.ReadFeature(
            SsalddelCodeFeatureKeys.UnityResilientWorldLoad,
            typeof(LastSuccessfulLoadRuntime<,>).Assembly);
        var diagnostics = SsalddelCodeMetadataValidator.Validate(
            SsalddelCodeMetadataGraphBuilder.Build(metadata),
            requireNavigationFields: true);

        Assert.Contains(metadata, item => item.ComponentType == typeof(LastSuccessfulLoadRuntime<,>));
        Assert.Contains(metadata, item => item.ComponentType == typeof(CommunityMarketSquareLoadCoordinator));
        Assert.Contains(metadata, item => item.ComponentType == typeof(PublicDataHallLoadCoordinator));
        Assert.Contains(metadata, item => item.ComponentType == typeof(WarehouseWorldLoadCoordinator));
        Assert.DoesNotContain(diagnostics, item =>
            item.Severity == SsalddelCodeMetadataDiagnosticSeverity.Error);
        Assert.All(metadata, item =>
            Assert.Equal(SsalddelCodeDataScope.ClientPresentation, item.WritesTo));
    }

    [Fact]
    public void 전투MapperMetadata는_Simulation을읽고_표현만만든다()
    {
        var metadata = Assert.Single(
            SsalddelCodeMetadataReader.Read(typeof(BattlePresentationMapper)),
            item => item.FeatureKey == SsalddelCodeFeatureKeys.SimulationParallelBattle);

        Assert.Equal(SsalddelCodeDataScope.SimulationState, metadata.ReadsFrom);
        Assert.Equal(SsalddelCodeDataScope.None, metadata.WritesTo);
        Assert.Equal(SsalddelCodeEffect.None, metadata.Effects);
        Assert.Equal(SsalddelCodeExecutionStage.Presentation, metadata.ExecutionStage);
    }

    [Fact]
    public void 농장전투입력Metadata는_Simulation을읽고_클라이언트표현만바꾼다()
    {
        var metadata = Assert.Single(
            SsalddelCodeMetadataReader.Read(typeof(FarmCombatInputCommandFactory)),
            item => item.FeatureKey
                == SsalddelCodeFeatureKeys.SimulationFarmCombatInput);

        Assert.Equal(SsalddelCodeDataScope.SimulationState, metadata.ReadsFrom);
        Assert.Equal(SsalddelCodeDataScope.ClientPresentation, metadata.WritesTo);
        Assert.Equal(SsalddelCodeExecutionStage.Presentation,
            metadata.ExecutionStage);
    }
}
