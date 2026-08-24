using System.Text.Json;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "WI 발생원과 E4 실행 문맥·E5 세계 발현 판정의 회귀를 검증한다.",
    Boundary = "결정적 계약 시험이며 실제 E5·E7 실행 증거를 대신하지 않는다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀)]
public sealed class SimulationWorldInteractionMaturityTests
{
    private readonly SimulationWorldInteractionMaturityService service = new();

    [Fact]
    public void E1_E2_E3은_각각다섯개_사람용하위Module을가진다()
    {
        var definitions = SsalddelEvidenceSubmoduleDefinitionCatalog.All;

        Assert.Equal(15, definitions.Count);
        Assert.Equal(5, definitions.Count(value =>
            value.EvidenceStage == SsalddelEvidenceStage.E1));
        Assert.Equal(5, definitions.Count(value =>
            value.EvidenceStage == SsalddelEvidenceStage.E2));
        Assert.Equal(5, definitions.Count(value =>
            value.EvidenceStage == SsalddelEvidenceStage.E3));
        Assert.Equal(definitions.Count, definitions.Select(value =>
            value.SubmoduleKey).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(definitions, value => value.TechnicalName ==
            "E1세계상호작용계약Module");
        Assert.Contains(definitions, value => value.TechnicalName ==
            "E2로컬권위AdapterModule");
        Assert.Contains(definitions, value => value.TechnicalName ==
            "E3저장재생검증Module");

        var aggregate = Assert.Single(
            SsalddelEvidenceResponsibilityReader.Read(
                typeof(경영SimulationSessionAggregate)),
            value => value.Role ==
                SsalddelEvidenceResponsibilityRole.Primary &&
                value.ComponentMethod is null);
        Assert.Equal(SsalddelEvidenceSubmoduleKeys.E1세션권위계약,
            aggregate.SubmoduleKey);
        var localRuntime = Assert.Single(
            SsalddelEvidenceResponsibilityReader.Read(
                typeof(LocalSimulationRuntime)),
            value => value.Role ==
                SsalddelEvidenceResponsibilityRole.Primary &&
                value.ComponentMethod is null);
        Assert.Equal(SsalddelEvidenceSubmoduleKeys.E2로컬권위Adapter,
            localRuntime.SubmoduleKey);
    }

    [Fact]
    public void 공통E단계Module은_E1부터E9까지_한국어역할을노출한다()
    {
        var expected = new[]
        {
            (SsalddelEvidenceStage.E1, "E1핵심계약Module", typeof(IE1핵심계약Module)),
            (SsalddelEvidenceStage.E2, "E2실행경계Module", typeof(IE2실행경계Module)),
            (SsalddelEvidenceStage.E3, "E3회귀증거Module", typeof(IE3회귀증거Module)),
            (SsalddelEvidenceStage.E4, "E4실행문맥결속Module", typeof(IE4실행문맥결속Module)),
            (SsalddelEvidenceStage.E5, "E5세계발현Module", typeof(IE5세계발현Module)),
            (SsalddelEvidenceStage.E6, "E6세계정제Module", typeof(IE6세계정제Module)),
            (SsalddelEvidenceStage.E7, "E7플레이경험폐루프Module", typeof(IE7플레이경험폐루프Module)),
            (SsalddelEvidenceStage.E8, "E8생활연속성Module", typeof(IE8생활연속성Module)),
            (SsalddelEvidenceStage.E9, "E9변화봉투Module", typeof(IE9변화봉투Module)),
        };

        Assert.Equal(expected.Select(item => (item.Item1, item.Item2)),
            SsalddelEvidenceStageDefinitionCatalog.All.Select(item =>
                (item.EvidenceStage, item.TechnicalName)));
        Assert.All(expected, item =>
            Assert.Contains(typeof(IE단계Module), item.Item3.GetInterfaces()));
    }

    [Fact]
    public void E4는_발생원과실행문맥과필수공간이모두결속되면완료된다()
    {
        var result = service.ReviewE4(new()
        {
            Definition = SpatialHarvestDefinition(),
            BoundTriggerSourceCodes = new[]
            {
                SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                SimulationWorldInteractionTriggerSourceCodes.NpcDriven,
            },
            BoundContextCodes = new[]
            {
                SimulationWorldInteractionContextCodes.Initiator,
                SimulationWorldInteractionContextCodes.Actor,
                SimulationWorldInteractionContextCodes.Target,
                SimulationWorldInteractionContextCodes.DataResource,
                SimulationWorldInteractionContextCodes.Time,
                SimulationWorldInteractionContextCodes.Spatial,
            },
            SpatialEvidenceStateCode =
                SimulationWorldInteractionSpatialEvidenceCodes.Bound,
        });

        Assert.Equal(SimulationWorldInteractionMaturityStateCodes.ContextBound,
            result.StateCode);
        Assert.Empty(result.MissingContextCodes);
    }

    [Fact]
    public void E4는_비공간WI에_H결속을강제하지않는다()
    {
        var result = service.ReviewE4(new()
        {
            Definition = NonSpatialWorldDefinition(),
            BoundTriggerSourceCodes = new[]
            {
                SimulationWorldInteractionTriggerSourceCodes.WorldDerived,
            },
            BoundContextCodes = new[]
            {
                SimulationWorldInteractionContextCodes.Initiator,
                SimulationWorldInteractionContextCodes.DataResource,
                SimulationWorldInteractionContextCodes.Time,
            },
            SpatialEvidenceStateCode =
                SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable,
        });

        Assert.Equal(SimulationWorldInteractionMaturityStateCodes.ContextBound,
            result.StateCode);
        Assert.Equal(SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable,
            result.SpatialEvidenceStateCode);
    }

    [Fact]
    public void E4는_공간WI의공간증거누락을_부분결속으로남긴다()
    {
        var definition = SpatialHarvestDefinition();
        var result = service.ReviewE4(new()
        {
            Definition = definition,
            BoundTriggerSourceCodes = new[]
            {
                SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
            },
            BoundContextCodes = definition.RequiredContextCodes
                .Where(value => value != SimulationWorldInteractionContextCodes.Spatial)
                .ToArray(),
            SpatialEvidenceStateCode =
                SimulationWorldInteractionSpatialEvidenceCodes.RequiredMissing,
        });

        Assert.Equal(
            SimulationWorldInteractionMaturityStateCodes.ContextPartiallyBound,
            result.StateCode);
        Assert.Contains(SimulationWorldInteractionContextCodes.Spatial,
            result.MissingContextCodes);
    }

    [Fact]
    public void E5는_공간조립만으로완료되지않고_결과와후속경로를요구한다()
    {
        var result = service.ReviewE5(new()
        {
            Definition = SpatialHarvestDefinition(),
            E4StateCode = SimulationWorldInteractionMaturityStateCodes.ContextBound,
            Invocation = SimulationWorldInteractionMaturityService.FromPlayer(
                "WI-FARM-04", "player:1", "actor:player:1"),
            SpatialEvidenceStateCode =
                SimulationWorldInteractionSpatialEvidenceCodes.Bound,
        });

        Assert.Equal(
            SimulationWorldInteractionMaturityStateCodes.ManifestationPartial,
            result.StateCode);
        Assert.Contains("AuthorityTransition", result.MissingEvidenceCodes);
        Assert.Contains("SuccessorOrReturnPath", result.MissingEvidenceCodes);
    }

    [Fact]
    public void E5는_비공간WorldDerivedWI도_공간없이발현될수있다()
    {
        var result = service.ReviewE5(new()
        {
            Definition = NonSpatialWorldDefinition(),
            E4StateCode = SimulationWorldInteractionMaturityStateCodes.ContextBound,
            Invocation = SimulationWorldInteractionMaturityService.FromWorldDerived(
                "WI-WORLD-ERA-01", "rule:era-transition.r1",
                "world-state:threat:82"),
            AuthorityTransitionRecorded = true,
            TaskOrEffectRecorded = true,
            ResultStateRecorded = true,
            SuccessorOrReturnPathRecorded = true,
            SpatialEvidenceStateCode =
                SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable,
        });

        Assert.Equal(SimulationWorldInteractionMaturityStateCodes.Manifested,
            result.StateCode);
        Assert.Empty(result.MissingEvidenceCodes);
    }

    [Fact]
    public void 허용되지않은발생원은_실행인스턴스에서거부된다()
    {
        var invocation = SimulationWorldInteractionMaturityService.FromWorldDerived(
            "WI-FARM-04", "rule:forged-origin.r1", "world-state:1");

        var error = Assert.Throws<InvalidOperationException>(() => service.ReviewE5(new()
        {
            Definition = SpatialHarvestDefinition(),
            E4StateCode = SimulationWorldInteractionMaturityStateCodes.ContextBound,
            Invocation = invocation,
            SpatialEvidenceStateCode =
                SimulationWorldInteractionSpatialEvidenceCodes.Bound,
        }));

        Assert.Equal("WorldInteractionTriggerSourceNotAllowed", error.Message);
    }

    [Fact]
    public void 발생원기록은_JSON왕복에서보존된다()
    {
        var source = SimulationWorldInteractionMaturityService.FromNpc(
            "WI-FARM-04", "npc:worker:1", "actor:npc:worker:1");

        var restored = JsonSerializer.Deserialize<
            SimulationWorldInteractionInvocationRecord>(
                JsonSerializer.Serialize(source));

        Assert.NotNull(restored);
        Assert.Equal(source.TriggerSourceCode, restored!.TriggerSourceCode);
        Assert.Equal(source.InitiatorStableId, restored.InitiatorStableId);
        Assert.Equal(source.ActorStableId, restored.ActorStableId);
    }

    [Fact]
    public void 공개Confirm요청은_발생원선택필드를노출하지않는다()
    {
        Assert.Null(typeof(SimulationNatureSurvivalCommandRequest)
            .GetProperty("TriggerSourceCode"));
        Assert.Null(typeof(SimulationFarmWorkConfirmRequest)
            .GetProperty("TriggerSourceCode"));
        Assert.Null(typeof(SimulationRegionalIncidentResponseConfirmRequest)
            .GetProperty("TriggerSourceCode"));
    }

    private static SimulationWorldInteractionDefinitionContext SpatialHarvestDefinition()
        => new()
        {
            WorldInteractionId = "WI-FARM-04",
            AllowedTriggerSourceCodes = new[]
            {
                SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                SimulationWorldInteractionTriggerSourceCodes.NpcDriven,
            },
            RequiredContextCodes = new[]
            {
                SimulationWorldInteractionContextCodes.Initiator,
                SimulationWorldInteractionContextCodes.Actor,
                SimulationWorldInteractionContextCodes.Target,
                SimulationWorldInteractionContextCodes.DataResource,
                SimulationWorldInteractionContextCodes.Time,
                SimulationWorldInteractionContextCodes.Spatial,
            },
            SpatialApplicabilityCode =
                SimulationWorldInteractionSpatialEvidenceCodes.Required,
        };

    private static SimulationWorldInteractionDefinitionContext NonSpatialWorldDefinition()
        => new()
        {
            WorldInteractionId = "WI-WORLD-ERA-01",
            AllowedTriggerSourceCodes = new[]
            {
                SimulationWorldInteractionTriggerSourceCodes.WorldDerived,
            },
            RequiredContextCodes = new[]
            {
                SimulationWorldInteractionContextCodes.Initiator,
                SimulationWorldInteractionContextCodes.DataResource,
                SimulationWorldInteractionContextCodes.Time,
            },
            SpatialApplicabilityCode =
                SimulationWorldInteractionSpatialEvidenceCodes.NotApplicable,
        };
}
