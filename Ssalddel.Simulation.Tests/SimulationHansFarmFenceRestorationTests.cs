using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "한스 농장 첫 울타리 복구의 권위 전이와 Save/Replay 결정성을 검증한다.",
    Boundary = "자동 시험은 Logic E5 근거이며 Unity World 배치·Play Mode·Game View·Presentation E5를 대신하지 않는다.",
    SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E3결정성검증)]
public sealed class SimulationHansFarmFenceRestorationTests
{
    [Fact]
    public void 한스농장첫사건은_부러진도끼_벌목_통나무줍기_울타리일괄수리로닫힌다()
    {
        var aggregate = new 경영SimulationSessionAggregate(CreateRequest());
        var initial = aggregate.GetNatureSurvivalState();
        var hans = Assert.IsType<SimulationHansFarmFenceRestorationSnapshot>(
            initial.HansFarmFenceRestoration);
        Assert.Equal(3, hans.Segments.Length);
        Assert.All(hans.Segments, value => Assert.Equal(
            SimulationNatureSurvivalCodes.Damaged, value.StateCode));

        var blockedRepair = Preview(aggregate,
            SimulationNatureSurvivalCodes.RepairHansFarmFence,
            SimulationNatureSurvivalCodes.HansFarmFenceAggregateStableId);
        Assert.False(blockedRepair.CanConfirm);
        Assert.Contains(SimulationNatureSurvivalCodes.HansBrokenAxeUnavailable,
            blockedRepair.BlockReasonCodes);
        Assert.Contains(SimulationNatureSurvivalCodes.TimberInsufficient,
            blockedRepair.BlockReasonCodes);

        var acquired = Confirm(aggregate, "command:hans:broken-axe", 0,
            SimulationNatureSurvivalCodes.AcquireHansBrokenAxe,
            SimulationNatureSurvivalCodes.HansBrokenAxePickupStableId);
        Assert.True(acquired.NatureSurvival.HansFarmFenceRestoration!
            .PlayerCarriesBrokenAxe);
        Assert.Contains(aggregate.GetWorldInventory().Players.Single().Items,
            value => value.ItemCode ==
                SimulationNatureSurvivalCodes.HansBrokenAxeItemCode
                && value.Quantity == 1);
        Assert.DoesNotContain(aggregate.GetActorEquipmentState().ItemDefinitions,
            value => value.ItemDefinitionStableId ==
                SimulationNatureSurvivalCodes.HansBrokenAxeItemCode);
        Assert.Contains(SimulationActorEquipmentCodes.Woodcutting,
            aggregate.GetActorEquipmentState().CapabilityCodes);

        var harvesting = Confirm(aggregate, "command:hans:harvest",
            acquired.Revision, SimulationNatureSurvivalCodes.BeginHarvest,
            "resource:nature-tree:hans:01");
        var harvested = aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:hans:harvest:hold",
            ExpectedRevision = harvesting.Revision,
            ElapsedRealtimeSeconds = NatureSurvivalRules.HarvestWorkSeconds,
            WorkInputHeld = true,
        });
        var dropped = Assert.Single(harvested.NatureSurvival.DroppedTimber);
        var collected = Confirm(aggregate, "command:hans:collect",
            harvested.Revision,
            SimulationNatureSurvivalCodes.CollectDroppedTimber,
            dropped.DroppedTimberStableId);
        Assert.Equal(2, collected.NatureSurvival.TimberQuantity);

        var repairPreview = Preview(aggregate,
            SimulationNatureSurvivalCodes.RepairHansFarmFence,
            SimulationNatureSurvivalCodes.HansFarmFenceAggregateStableId);
        Assert.True(repairPreview.CanConfirm);
        Assert.Equal(2, repairPreview.RequiredTimberQuantity);
        var repaired = Confirm(aggregate, "command:hans:fence-repair",
            collected.Revision,
            SimulationNatureSurvivalCodes.RepairHansFarmFence,
            SimulationNatureSurvivalCodes.HansFarmFenceAggregateStableId);

        var repairedHans = repaired.NatureSurvival.HansFarmFenceRestoration!;
        Assert.Equal(0, repaired.NatureSurvival.TimberQuantity);
        Assert.Equal(SimulationNatureSurvivalCodes.Repaired,
            repairedHans.FenceStateCode);
        Assert.True(repairedHans.NextChoiceAvailable);
        Assert.All(repairedHans.Segments, value => Assert.Equal(
            SimulationNatureSurvivalCodes.Repaired, value.StateCode));

        var duplicate = Confirm(aggregate, "command:hans:fence-repair",
            collected.Revision,
            SimulationNatureSurvivalCodes.RepairHansFarmFence,
            SimulationNatureSurvivalCodes.HansFarmFenceAggregateStableId);
        Assert.Equal(repaired.Revision, duplicate.Revision);
        Assert.Equal(0, duplicate.NatureSurvival.TimberQuantity);
        var unavailable = Preview(aggregate,
            SimulationNatureSurvivalCodes.RepairHansFarmFence,
            SimulationNatureSurvivalCodes.HansFarmFenceAggregateStableId);
        Assert.False(unavailable.CanConfirm);
        Assert.Contains(
            SimulationNatureSurvivalCodes.HansFarmFenceAlreadyRepaired,
            unavailable.BlockReasonCodes);
    }

    [Fact]
    public void 한스농장울타리상태와사건물품은_SaveReplay에서동일하다()
    {
        var aggregate = new 경영SimulationSessionAggregate(CreateRequest());
        var acquired = Confirm(aggregate, "command:hans:save:axe", 0,
            SimulationNatureSurvivalCodes.AcquireHansBrokenAxe,
            SimulationNatureSurvivalCodes.HansBrokenAxePickupStableId);
        var harvesting = Confirm(aggregate, "command:hans:save:harvest",
            acquired.Revision, SimulationNatureSurvivalCodes.BeginHarvest,
            "resource:nature-tree:hans:01");
        var harvested = aggregate.AdvanceNatureSurvivalClock(new()
        {
            CommandId = "command:hans:save:hold",
            ExpectedRevision = harvesting.Revision,
            ElapsedRealtimeSeconds = NatureSurvivalRules.HarvestWorkSeconds,
            WorkInputHeld = true,
        });
        var collected = Confirm(aggregate, "command:hans:save:collect",
            harvested.Revision,
            SimulationNatureSurvivalCodes.CollectDroppedTimber,
            Assert.Single(harvested.NatureSurvival.DroppedTimber)
                .DroppedTimberStableId);
        Confirm(aggregate, "command:hans:save:repair", collected.Revision,
            SimulationNatureSurvivalCodes.RepairHansFarmFence,
            SimulationNatureSurvivalCodes.HansFarmFenceAggregateStableId);

        var package = aggregate.CreateSavePackage(new()
        {
            SaveStableId = "save:hans-farm-fence-restoration",
            ExpectedRevision = aggregate.Revision,
        });
        var restored = SimulationSessionReplay.Restore(package);
        var replayed = restored.CreateSavePackage(new()
        {
            SaveStableId = package.SaveStableId,
            ExpectedRevision = restored.Revision,
        });

        Assert.Equal(package.ReplayHash, replayed.ReplayHash);
        Assert.Equal(SimulationNatureSurvivalCodes.Repaired,
            replayed.Snapshot.NatureSurvival.HansFarmFenceRestoration!
                .FenceStateCode);
        Assert.True(replayed.Snapshot.NatureSurvival
            .HansFarmFenceRestoration!.PlayerCarriesBrokenAxe);
    }

    private static SimulationNatureSurvivalActionPreviewSnapshot Preview(
        경영SimulationSessionAggregate aggregate, string action, string target)
        => aggregate.PreviewNatureSurvivalAction(new()
        {
            ObservedWorldRevision = aggregate.Revision,
            PlayerStableId = "player:hans-farm",
            ActionCode = action,
            TargetStableId = target,
        });

    private static 경영SimulationSessionSnapshot Confirm(
        경영SimulationSessionAggregate aggregate, string commandId,
        long expectedRevision, string action, string target)
        => aggregate.ConfirmNatureSurvivalAction(new()
        {
            CommandId = commandId,
            ExpectedRevision = expectedRevision,
            PlayerStableId = "player:hans-farm",
            ActionCode = action,
            TargetStableId = target,
        });

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.Parse(
                "d72f4751-25a6-4e1f-918c-c5bcb507ff8a"),
            ScenarioStableId = "scenario:hans-farm-fence-restoration",
            ScenarioDataRevision = "hans-farm-fence-restoration.r1",
            ScenarioSeed = 426,
            RuleRevision = "simulation.rule.r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:hans-farm",
                TerritoryStableId = "territory:nature",
                SettlementStableId = "settlement:hans-farm",
                GameDateStartsOn = new DateTimeOffset(2026, 9, 4, 0, 0, 0,
                    TimeSpan.Zero),
            },
            NatureSurvival = new SimulationNatureSurvivalInitialStateRequest
            {
                ProfileRevision =
                    SimulationNatureSurvivalCodes.ProfileRevisionR6,
                PlayerStableId = "player:hans-farm",
                HansFarmFenceRestorationEnabled = true,
                BuildingProgressionCatalog =
                    Simulation영역건물발전Catalog.CreateDefault(),
                ResourceNodes = new[]
                {
                    new SimulationNatureResourceNodeInitialStateRequest
                    {
                        ResourceNodeStableId =
                            "resource:nature-tree:hans:01",
                        H2StableId =
                            SimulationNatureSurvivalCodes.HarvestH2StableId,
                        H1StableId =
                            "h1-stock:nature-exploration-buffer",
                        LocalX = -4d,
                        LocalZ = 5d,
                    },
                },
            },
        };
}
