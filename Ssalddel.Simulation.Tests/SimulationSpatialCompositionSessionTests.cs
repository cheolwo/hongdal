using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Hub 공간 조립 graph의 WorldTick·v22 저장 재생과 Local·Hosted 동등성을 검증한다.",
    Boundary = "자동 시험은 실제 Unity 배치·통행 또는 E5 증거가 아니다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3저장재생검증)]
public sealed class SimulationSpatialCompositionSessionTests
{
    [Fact]
    public void WorldTick_CommitsH2AndV22RoundTripsCompositionGraph()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var initial = session.GetSpatialComposition("Hub");
        Assert.Equal(SimulationSpatialCompositionCodes.Qualified,
            Assessment(initial,
                PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2)
                .StateCode);

        var before = session.Snapshot();
        var after = session.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:spatial-composition:commit",
            ExpectedRevision = before.Revision,
            TickCount = 1,
        });
        var formed = session.GetSpatialComposition("Hub");
        Assert.Equal(SimulationSpatialCompositionCodes.Formed,
            Assessment(formed,
                PyeongchangHubSpatialCompositionCodes.InternalWarehouseH2)
                .StateCode);

        var saved = session.CreateSavePackage(
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:hub-spatial-composition:v22",
                ExpectedRevision = after.Revision,
            });
        Assert.Equal(SimulationSaveSchemaVersions.V22, saved.SchemaVersion);
        Assert.NotNull(saved.SpatialComposition);
        Assert.NotNull(saved.SpatialCompositionHandle);
        Assert.Equal(formed.GraphHashSha256,
            saved.SpatialCompositionHandle!.GraphHashSha256);

        var restored = SimulationSessionReplay.Restore(saved);
        var restoredGraph = restored.GetSpatialComposition("Hub");
        Assert.Equal(formed.GraphHashSha256, restoredGraph.GraphHashSha256);
        var replayed = restored.CreateSavePackage(
            new SimulationSessionSaveRequest
            {
                SaveStableId = saved.SaveStableId,
                ExpectedRevision = restored.Revision,
            });
        Assert.Equal(saved.ReplayHash, replayed.ReplayHash);
    }

    [Fact]
    public async Task LocalProcessAndRemoteHostFacade_ReturnSameRevisionGraphHash()
    {
        var localRoot = Path.Combine(Path.GetTempPath(),
            "ssalddel-spatial-composition-" + Guid.NewGuid().ToString("N"));
        try
        {
            using var local = new LocalSimulationRuntime(
                new InMemory경영SimulationSessionStore(),
                new InMemorySimulationSessionSaveStore(),
                new FileSimulationLocalSaveSlotStore(localRoot));
            var localSession = await local.CreateAsync(CreateRequest());
            var localGraph = await local.GetSpatialCompositionAsync(
                localSession.SessionStableId, "Hub");

            var remoteStore = new InMemory경영SimulationSessionStore();
            var remote = new 경영SimulationSessionService(remoteStore,
                new InMemorySimulationSessionSaveStore());
            var remoteSession = remote.Create(CreateRequest());
            var remoteGraph = remote.GetSpatialComposition(
                remoteSession.SessionStableId, "Hub");

            Assert.Equal(localSession.Revision, remoteSession.Revision);
            Assert.Equal(localGraph.GraphHashSha256,
                remoteGraph.GraphHashSha256);
        }
        finally
        {
            if (Directory.Exists(localRoot)) Directory.Delete(localRoot, true);
        }
    }

    private static SpatialCompositionAssessment Assessment(
        SimulationSpatialCompositionStateSnapshot state, string definitionId)
        => state.Assessments.Single(value =>
            value.TargetDefinitionStableId == definitionId);

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.Parse(
                "73a109f3-6449-4e1d-a873-9290aee92b25"),
            ScenarioStableId = "scenario:hub-internal-warehouse",
            ScenarioDataRevision = "fixture.hub-warehouse.r1",
            ScenarioSeed = 300,
            RuleRevision = "world-interaction.hub-warehouse.r1",
            SpatialCompositionRuleRevision =
                SimulationSpatialCompositionCodes.RuleRevision,
            DurationTicks = 30,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:hub-independent",
                TerritoryStableId = "territory:pyeongchang",
                SettlementStableId = "settlement:pyeongchang",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 8, 25, 0, 0, 0, TimeSpan.Zero),
            },
            SpatialWorld =
                PyeongchangSimulation공간상호작용Fixture.Create(),
        };
}
