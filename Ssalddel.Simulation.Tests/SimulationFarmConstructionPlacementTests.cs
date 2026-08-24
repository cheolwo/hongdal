using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationFarmConstructionPlacementTests
{
    [Fact]
    public void 창고_배치는_스냅_Preview_확정_Tick_재조회를_거친다()
    {
        var session = CreateSession();
        var before = session.Snapshot();

        var preview = session.PreviewFarmConstructionPlacement(StoragePreview(
            before.Revision, -112, -5013));

        Assert.True(preview.CanConfirm);
        Assert.Equal(-100, preview.LocalXCentimeters);
        Assert.Equal(-5025, preview.LocalZCentimeters);
        Assert.Equal(800, preview.FootprintWidthCentimeters);
        Assert.Equal(600, preview.FootprintDepthCentimeters);
        Assert.Equal(before.Revision, session.Revision);
        Assert.Single(preview.ReservedMaterialLotStableIds);

        var planned = session.ConfirmFarmConstructionPlacement(new()
        {
            CommandId = "place-storage-01",
            ExpectedRevision = session.Revision,
            PlacementProposalStableId = preview.PlacementProposalStableId,
            PreviewHashSha256 = preview.PreviewHashSha256,
        });
        var project = Assert.Single(planned.IntegratedWorld.ConstructionProjects);
        Assert.Equal(-100, project.LocalXCentimeters);
        Assert.Equal(-5025, project.LocalZCentimeters);
        Assert.Equal("h2:Farm:processing-south", project.TargetH2StableId);
        Assert.Equal(SimulationConstructionProjectStateCodes.Planned, project.StateCode);
        Assert.Equal(10m, planned.IntegratedWorld.Lots.Single(value =>
            value.LotStableId == "lot:farm:facility-components").Quantity);

        Advance(session, "placement-tick-01");
        Advance(session, "placement-tick-02");
        var completed = session.Snapshot().IntegratedWorld;
        Assert.Equal(SimulationConstructionProjectStateCodes.Completed,
            Assert.Single(completed.ConstructionProjects).StateCode);
        Assert.Equal(8m, completed.Lots.Single(value =>
            value.LotStableId == "lot:farm:facility-components").Quantity);
        var facility = completed.Facilities.Single(value =>
            value.FacilityStableId == "facility:player-built:place-storage-01");
        Assert.Equal(SimulationFacilityLifecycleCodes.Operational,
            facility.LifecycleCode);
        Assert.Equal("connector:FarmSouthRoad", Assert.Single(
            facility.AccessConnectorStableIds));
    }

    [Fact]
    public void 보호구역_밖과_급경사와_도로미연결은_Preview에서_차단된다()
    {
        var session = CreateSession();

        var outside = session.PreviewFarmConstructionPlacement(StoragePreview(
            session.Revision, 0, -3900));
        Assert.False(outside.CanConfirm);
        Assert.Contains("SimulationConstructionPlacementOutsideZone",
            outside.BlockingReasonCodes);

        var steep = StoragePreview(session.Revision, 4500, 0);
        steep.PlacementZoneStableId = "placement-zone:farm:support-east-steep";
        steep.TargetH2StableId = "h2:Farm:support-east";
        steep.AccessConnectorStableId = "connector:FarmEastRoad";
        var steepResult = session.PreviewFarmConstructionPlacement(steep);
        Assert.False(steepResult.CanConfirm);
        Assert.Contains("SimulationConstructionPlacementSlopeBlocked",
            steepResult.BlockingReasonCodes);

        var disconnected = StoragePreview(session.Revision, 0, -5000);
        disconnected.AccessConnectorStableId = string.Empty;
        var disconnectedResult = session.PreviewFarmConstructionPlacement(disconnected);
        Assert.False(disconnectedResult.CanConfirm);
        Assert.Contains("SimulationConstructionPlacementRoadAccessRequired",
            disconnectedResult.BlockingReasonCodes);
    }

    [Fact]
    public void 울타리는_시작점에서_연속되고_밭입구는_Gate로_남긴다()
    {
        var session = CreateSession();
        var first = session.PreviewFarmConstructionPlacement(new()
        {
            ExpectedRevision = session.Revision,
            BlueprintStableId = "blueprint:farm-fence-straight.v1",
            PlacementZoneStableId = "placement-zone:farm:north-field-edge",
            TargetH2StableId = "h2:Farm:potato-fields",
            LocalXCentimeters = -3600,
            LocalZCentimeters = 3200,
            FenceChainStableId = "fence-chain:farm:north-edge",
        });
        Assert.True(first.CanConfirm);
        session.ConfirmFarmConstructionPlacement(new()
        {
            CommandId = "place-fence-01",
            ExpectedRevision = session.Revision,
            PlacementProposalStableId = first.PlacementProposalStableId,
            PreviewHashSha256 = first.PreviewHashSha256,
        });

        var disconnected = session.PreviewFarmConstructionPlacement(new()
        {
            ExpectedRevision = session.Revision,
            BlueprintStableId = "blueprint:farm-fence-straight.v1",
            PlacementZoneStableId = "placement-zone:farm:north-field-edge",
            TargetH2StableId = "h2:Farm:potato-fields",
            LocalXCentimeters = 2000,
            LocalZCentimeters = 3200,
            FenceChainStableId = "fence-chain:farm:north-edge",
        });
        Assert.False(disconnected.CanConfirm);
        Assert.Contains("SimulationConstructionFenceNotContinuous",
            disconnected.BlockingReasonCodes);

        var wrongEntranceObject = session.PreviewFarmConstructionPlacement(new()
        {
            ExpectedRevision = session.Revision,
            BlueprintStableId = "blueprint:farm-fence-straight.v1",
            PlacementZoneStableId = "placement-zone:farm:north-entrance",
            TargetH2StableId = "h2:Farm:potato-fields",
            LocalXCentimeters = 0,
            LocalZCentimeters = 3000,
            RotationQuarterTurns = 1,
            FenceChainStableId = "fence-chain:farm:north-entrance",
        });
        Assert.False(wrongEntranceObject.CanConfirm);
        Assert.Contains("SimulationConstructionFenceEntranceMustRemainOpen",
            wrongEntranceObject.BlockingReasonCodes);

        var gate = session.PreviewFarmConstructionPlacement(new()
        {
            ExpectedRevision = session.Revision,
            BlueprintStableId = "blueprint:farm-fence-gate.v1",
            PlacementZoneStableId = "placement-zone:farm:north-entrance",
            TargetH2StableId = "h2:Farm:potato-fields",
            LocalXCentimeters = 0,
            LocalZCentimeters = 3000,
            RotationQuarterTurns = 1,
            FenceChainStableId = "fence-chain:farm:north-entrance",
        });
        Assert.True(gate.CanConfirm);
    }

    [Fact]
    public void 울타리와_나무_연속배치는_한_작업자의_건설대기열로_직렬예약된다()
    {
        var session = CreateSession();
        var firstFence = Confirm(session, "continuous-fence-01", new()
        {
            ExpectedRevision = session.Revision,
            BlueprintStableId = "blueprint:farm-fence-straight.v1",
            PlacementZoneStableId = "placement-zone:farm:north-field-edge",
            TargetH2StableId = "h2:Farm:potato-fields",
            LocalXCentimeters = -3600,
            LocalZCentimeters = 3200,
            FenceChainStableId = "fence-chain:farm:north-edge",
        });
        var secondFence = Confirm(session, "continuous-fence-02", new()
        {
            ExpectedRevision = session.Revision,
            BlueprintStableId = "blueprint:farm-fence-straight.v1",
            PlacementZoneStableId = "placement-zone:farm:north-field-edge",
            TargetH2StableId = "h2:Farm:potato-fields",
            LocalXCentimeters = -2800,
            LocalZCentimeters = 3200,
            FenceChainStableId = "fence-chain:farm:north-edge",
        });
        var firstTree = Confirm(session, "continuous-tree-01", new()
        {
            ExpectedRevision = session.Revision,
            BlueprintStableId = "blueprint:farm-tree.v1",
            PlacementZoneStableId = "placement-zone:farm:tree-east",
            TargetH2StableId = "h2:Farm:support-east",
            LocalXCentimeters = 4000,
            LocalZCentimeters = -1000,
        });
        var secondTree = Confirm(session, "continuous-tree-02", new()
        {
            ExpectedRevision = session.Revision,
            BlueprintStableId = "blueprint:farm-tree.v1",
            PlacementZoneStableId = "placement-zone:farm:tree-east",
            TargetH2StableId = "h2:Farm:support-east",
            LocalXCentimeters = 4400,
            LocalZCentimeters = -1000,
        });

        Assert.Equal(firstFence.SelectedActorStableIds,
            secondFence.SelectedActorStableIds);
        Assert.Equal(firstFence.SelectedActorStableIds,
            firstTree.SelectedActorStableIds);
        Assert.Equal(firstFence.SelectedActorStableIds,
            secondTree.SelectedActorStableIds);
        var queued = session.Snapshot().IntegratedWorld.ConstructionProjects
            .OrderBy(value => value.ConstructionStartsAtTick).ToArray();
        Assert.Equal(new[] { 1, 3, 5, 7 }, queued.Select(value =>
            value.ConstructionStartsAtTick));
        Assert.Equal(new[] { 2, 4, 6, 8 }, queued.Select(value =>
            value.ConstructionCompletesAtTick));

        for (var tick = 1; tick <= 8; tick++)
            Advance(session, "continuous-placement-tick-" + tick);
        Assert.All(session.Snapshot().IntegratedWorld.ConstructionProjects,
            value => Assert.Equal(SimulationConstructionProjectStateCodes.Completed,
                value.StateCode));
        Assert.Equal(2m, session.Snapshot().IntegratedWorld.Lots.Single(value =>
            value.LotStableId == "lot:farm:saplings").Quantity);
    }

    [Fact]
    public void Preview_hash_revision과_Command_멱등성을_서버가_통제한다()
    {
        var session = CreateSession();
        var preview = session.PreviewFarmConstructionPlacement(StoragePreview(
            session.Revision, 0, -5000));

        Assert.Throws<SimulationConflictException>(() =>
            session.ConfirmFarmConstructionPlacement(new()
            {
                CommandId = "place-storage-hash",
                ExpectedRevision = session.Revision,
                PlacementProposalStableId = preview.PlacementProposalStableId,
                PreviewHashSha256 = "tampered",
            }));

        var request = new SimulationFarmConstructionPlacementConfirmRequest
        {
            CommandId = "place-storage-idempotent",
            ExpectedRevision = session.Revision,
            PlacementProposalStableId = preview.PlacementProposalStableId,
            PreviewHashSha256 = preview.PreviewHashSha256,
        };
        var first = session.ConfirmFarmConstructionPlacement(request);
        var second = session.ConfirmFarmConstructionPlacement(request);
        Assert.Equal(first.Revision, second.Revision);
        Assert.Single(second.IntegratedWorld.ConstructionProjects);

        var stale = session.PreviewFarmConstructionPlacement(StoragePreview(
            first.Revision - 1, 1000, -5000));
        Assert.False(stale.CanConfirm);
        Assert.Contains("SimulationExpectedRevisionMismatch", stale.BlockingReasonCodes);
    }

    [Fact]
    public void 동적_배치_좌표와_재료소비는_SaveReplay에서_동일하다()
    {
        var session = CreateSession();
        var preview = session.PreviewFarmConstructionPlacement(StoragePreview(
            session.Revision, 725, -5125));
        session.ConfirmFarmConstructionPlacement(new()
        {
            CommandId = "place-storage-replay",
            ExpectedRevision = session.Revision,
            PlacementProposalStableId = preview.PlacementProposalStableId,
            PreviewHashSha256 = preview.PreviewHashSha256,
        });
        Advance(session, "placement-replay-tick-01");
        Advance(session, "placement-replay-tick-02");

        var save = session.CreateSavePackage(new()
        {
            SaveStableId = "save:farm-placement-replay",
            ExpectedRevision = session.Revision,
        });
        var replayed = SimulationSessionReplay.Restore(save).Snapshot();
        var expected = Assert.Single(session.Snapshot().IntegratedWorld.ConstructionProjects);
        var actual = Assert.Single(replayed.IntegratedWorld.ConstructionProjects);
        Assert.Equal((expected.LocalXCentimeters, expected.LocalZCentimeters,
                expected.RotationQuarterTurns, expected.StateCode),
            (actual.LocalXCentimeters, actual.LocalZCentimeters,
                actual.RotationQuarterTurns, actual.StateCode));
        Assert.Equal(session.Snapshot().IntegratedWorld.Lots.Select(value =>
                (value.LotStableId, value.Quantity)),
            replayed.IntegratedWorld.Lots.Select(value =>
                (value.LotStableId, value.Quantity)));
    }

    [Fact]
    public void 발전H1은_전투에서얻은기회를예약하고_완공때소비하며_저장재생된다()
    {
        var session = CreateSession();
        var blocked = DevelopmentPreview(session.Revision,
            SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1,
            string.Empty, 0, 1500);

        var blockedPreview = session.PreviewFarmConstructionPlacement(blocked);

        Assert.False(blockedPreview.CanConfirm);
        Assert.Contains("SimulationRegionalDevelopmentOpportunityRequired",
            blockedPreview.BlockingReasonCodes);

        var opportunity = EarnFarmDevelopmentOpportunity(session,
            "inspection", "cultivation:test:development-potato-01");
        var preview = session.PreviewFarmConstructionPlacement(DevelopmentPreview(
            session.Revision,
            SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1,
            opportunity.OpportunityStableId, 0, 1500));
        Assert.True(preview.CanConfirm);
        Assert.Equal(opportunity.OpportunityStableId,
            preview.DevelopmentOpportunityStableId);

        var planned = session.ConfirmFarmConstructionPlacement(new()
        {
            CommandId = "place-farm-exposure-inspection",
            ExpectedRevision = session.Revision,
            PlacementProposalStableId = preview.PlacementProposalStableId,
            PreviewHashSha256 = preview.PreviewHashSha256,
        });
        var reserved = Assert.Single(planned.RegionalDevelopment.Opportunities);
        Assert.Equal(SimulationRegionalDevelopmentCodes.Reserved, reserved.StateCode);
        Assert.Equal("construction-project:place-farm-exposure-inspection",
            reserved.ReservedProjectStableId);
        Assert.Equal(opportunity.OpportunityStableId,
            Assert.Single(planned.IntegratedWorld.ConstructionProjects)
                .DevelopmentOpportunityStableId);

        Advance(session, "development-placement-tick-01");
        Advance(session, "development-placement-tick-02");
        var completed = session.Snapshot();
        var consumed = Assert.Single(completed.RegionalDevelopment.Opportunities);
        Assert.Equal(SimulationRegionalDevelopmentCodes.Consumed, consumed.StateCode);
        Assert.Equal(SimulationRegionalDevelopmentCodes.Developing,
            Assert.Single(completed.RegionalDevelopment.Areas).StateCode);
        Assert.Equal(new[]
        {
            SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1,
        }, Assert.Single(completed.RegionalDevelopment.Areas).OperationalH1StableIds);

        var save = session.CreateSavePackage(new()
        {
            SaveStableId = "save:farm-development-placement",
            ExpectedRevision = session.Revision,
        });
        Assert.Equal(SimulationSaveSchemaVersions.V14, save.SchemaVersion);
        var replayed = SimulationSessionReplay.Restore(save);
        var replayedSave = replayed.CreateSavePackage(new()
        {
            SaveStableId = save.SaveStableId,
            ExpectedRevision = replayed.Revision,
        });
        Assert.Equal(save.ReplayHash, replayedSave.ReplayHash);
        Assert.Equal(SimulationRegionalDevelopmentCodes.Consumed,
            Assert.Single(replayed.Snapshot().RegionalDevelopment.Opportunities).StateCode);
    }

    [Fact]
    public void Farm세발전H1완공은_H2독립준비와_Nature연결후보를연다()
    {
        var session = CreateSession();
        var plans = new[]
        {
            ("inspection", "cultivation:test:development-potato-01",
                SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1, -800),
            ("quarantine", "cultivation:test:development-potato-02",
                SimulationRegionalDevelopmentCodes.FarmIncidentQuarantineH1, 0),
            ("weather", "cultivation:test:development-potato-03",
                SimulationRegionalDevelopmentCodes.FarmWeatherProtectionH1, 800),
        };

        foreach (var plan in plans)
        {
            var opportunity = EarnFarmDevelopmentOpportunity(
                session, plan.Item1, plan.Item2);
            var preview = session.PreviewFarmConstructionPlacement(DevelopmentPreview(
                session.Revision, plan.Item3, opportunity.OpportunityStableId,
                plan.Item4, 1500));
            Assert.True(preview.CanConfirm);
            session.ConfirmFarmConstructionPlacement(new()
            {
                CommandId = "place-farm-development-" + plan.Item1,
                ExpectedRevision = session.Revision,
                PlacementProposalStableId = preview.PlacementProposalStableId,
                PreviewHashSha256 = preview.PreviewHashSha256,
            });
            Advance(session, "development-" + plan.Item1 + "-tick-01");
            Advance(session, "development-" + plan.Item1 + "-tick-02");
        }

        var development = session.Snapshot().RegionalDevelopment;
        Assert.All(development.Opportunities, value => Assert.Equal(
            SimulationRegionalDevelopmentCodes.Consumed, value.StateCode));
        var farm = Assert.Single(development.Areas);
        Assert.Equal(SimulationRegionalDevelopmentCodes.IndependentReady,
            farm.StateCode);
        Assert.Equal(3, farm.OperationalH1StableIds.Length);
        Assert.Equal(SimulationRegionalDevelopmentCodes.Available,
            Assert.Single(development.Connectors).StateCode);
    }

    private static SimulationFarmConstructionPlacementPreviewRequest StoragePreview(
        long revision, int x, int z) => new()
    {
        ExpectedRevision = revision,
        BlueprintStableId = "blueprint:farm-small-storage.v1",
        PlacementZoneStableId = "placement-zone:farm:processing-south",
        TargetH2StableId = "h2:Farm:processing-south",
        LocalXCentimeters = x,
        LocalZCentimeters = z,
        AccessConnectorStableId = "connector:FarmSouthRoad",
    };

    private static SimulationFarmConstructionPlacementPreviewRequest DevelopmentPreview(
        long revision, string blueprintStableId, string opportunityStableId,
        int x, int z) => new()
    {
        ExpectedRevision = revision,
        BlueprintStableId = blueprintStableId,
        PlacementZoneStableId = "placement-zone:farm:incident-containment",
        TargetH2StableId = SimulationRegionalDevelopmentCodes.FarmIncidentContainmentH2,
        LocalXCentimeters = x,
        LocalZCentimeters = z,
        DevelopmentOpportunityStableId = opportunityStableId,
    };

    private static SimulationRegionalDevelopmentOpportunitySnapshot
        EarnFarmDevelopmentOpportunity(경영SimulationSessionAggregate session,
            string suffix, string cultivationStableId)
    {
        session.ConfirmFarmWork(new SimulationFarmWorkConfirmRequest
        {
            CommandId = "command:test:development-harvest:" + suffix,
            ExpectedRevision = session.Revision,
            ActorStableId = "actor:test:farmer",
            TargetStableId = cultivationStableId,
            ActionCode = SimulationFarmSurvivalCodes.Harvesting,
            AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
        });
        var harvested = session.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:test:development-harvest-tick:" + suffix,
            ExpectedRevision = session.Revision,
            TickCount = 1,
        });
        var incident = Assert.Single(harvested.RegionalIncidents, value =>
            value.StateCode == SimulationRegionalIncidentCodes.AwaitingResponse);
        var adverse = session.ConfirmRegionalIncidentResponse(incident.EventStableId,
            new SimulationRegionalIncidentResponseConfirmRequest
            {
                CommandId = "command:test:development-opportunity-source:" + suffix,
                ExpectedRevision = session.Revision,
                ActorStableId = "actor:farm:builder-01",
                ChoiceStableId = SimulationRegionalIncidentCodes.FarmLeaveExposed,
            });
        var encounter = Assert.Single(adverse.NatureThreat.Encounters);
        var victory = session.ApplyNatureEncounterVictory(
            "battle:test:development-opportunity:" + suffix,
            encounter.EncounterStableId);
        return victory.RegionalDevelopment.Opportunities.Single(value =>
            value.SourceBattleStableId ==
                "battle:test:development-opportunity:" + suffix);
    }

    private static 경영SimulationSessionAggregate CreateSession()
    {
        var integrated = H5IntegratedWorldScenarioFixture.Create();
        integrated.FacilityDefinitions = integrated.FacilityDefinitions.Concat(new[]
        {
            Definition("facility-definition:farm-small-storage.v1", "FarmStorage"),
            Definition("facility-definition:farm-fence-straight.v1", "FarmFence"),
            Definition("facility-definition:farm-fence-gate.v1", "FarmFenceGate"),
            Definition("facility-definition:farm-tree.v1", "FarmTree"),
            Definition("facility-definition:farm-exposure-inspection.v1",
                "FarmExposureInspection"),
            Definition("facility-definition:farm-incident-quarantine.v1",
                "FarmIncidentQuarantine"),
            Definition("facility-definition:farm-weather-protection.v1",
                "FarmWeatherProtection"),
        }).ToArray();
        integrated.Lots = integrated.Lots.Concat(new[]
        {
            new SimulationIntegratedLotSeedRequest
            {
                LotStableId = "lot:farm:facility-components",
                ItemCode = SimulationIntegratedItemCodes.FacilityComponent,
                Quantity = 10m,
                UnitCode = "unit",
                FacilityStableId = "facility:farm:warehouse",
            },
            new SimulationIntegratedLotSeedRequest
            {
                LotStableId = "lot:farm:saplings",
                ItemCode = SimulationIntegratedItemCodes.FarmSapling,
                Quantity = 4m,
                UnitCode = "tree",
                FacilityStableId = "facility:farm:warehouse",
            },
        }).ToArray();
        integrated.FacilityBlueprints = integrated.FacilityBlueprints.Concat(new[]
        {
            Blueprint("blueprint:farm-small-storage.v1",
                "facility-definition:farm-small-storage.v1",
                SimulationConstructionPlacementKindCodes.Building,
                800, 600, 100, 5000, true, 2m,
                SimulationConstructionPlacementZoneTypeCodes.FarmProcessing,
                SimulationConstructionPlacementZoneTypeCodes.FarmSupport),
            Blueprint("blueprint:farm-fence-straight.v1",
                "facility-definition:farm-fence-straight.v1",
                SimulationConstructionPlacementKindCodes.FenceSegment,
                800, 50, 0, 12000, false, 1m,
                SimulationConstructionPlacementZoneTypeCodes.FarmFenceEdge),
            Blueprint("blueprint:farm-fence-gate.v1",
                "facility-definition:farm-fence-gate.v1",
                SimulationConstructionPlacementKindCodes.FenceGate,
                400, 100, 0, 12000, false, 1m,
                SimulationConstructionPlacementZoneTypeCodes.FarmEntrance),
            TreeBlueprint(),
            Blueprint(SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1,
                "facility-definition:farm-exposure-inspection.v1",
                SimulationConstructionPlacementKindCodes.Building,
                300, 300, 50, 5000, false, 2m,
                SimulationConstructionPlacementZoneTypeCodes.FarmSupport),
            Blueprint(SimulationRegionalDevelopmentCodes.FarmIncidentQuarantineH1,
                "facility-definition:farm-incident-quarantine.v1",
                SimulationConstructionPlacementKindCodes.Building,
                400, 300, 50, 5000, false, 2m,
                SimulationConstructionPlacementZoneTypeCodes.FarmSupport),
            Blueprint(SimulationRegionalDevelopmentCodes.FarmWeatherProtectionH1,
                "facility-definition:farm-weather-protection.v1",
                SimulationConstructionPlacementKindCodes.Building,
                300, 400, 50, 5000, false, 2m,
                SimulationConstructionPlacementZoneTypeCodes.FarmSupport),
        }).ToArray();
        integrated.ConstructionPlacementZones = new[]
        {
            Zone("placement-zone:farm:processing-south",
                "h2:Farm:processing-south",
                SimulationConstructionPlacementZoneTypeCodes.FarmProcessing,
                -3000, 3000, -6000, -4000, 2000,
                new[] { "connector:FarmSouthRoad" }),
            Zone("placement-zone:farm:support-east-steep",
                "h2:Farm:support-east",
                SimulationConstructionPlacementZoneTypeCodes.FarmSupport,
                3500, 6000, -3000, 3000, 9000,
                new[] { "connector:FarmEastRoad" }),
            Zone("placement-zone:farm:north-field-edge",
                "h2:Farm:potato-fields",
                SimulationConstructionPlacementZoneTypeCodes.FarmFenceEdge,
                -4000, 4000, 3000, 3400, 3000,
                Array.Empty<string>(), "fence-chain:farm:north-edge", -4000, 3200),
            Zone("placement-zone:farm:north-entrance",
                "h2:Farm:potato-fields",
                SimulationConstructionPlacementZoneTypeCodes.FarmEntrance,
                -100, 100, 2750, 3250, 3000,
                Array.Empty<string>(), "fence-chain:farm:north-entrance", 0, 2800),
            Zone("placement-zone:farm:tree-east",
                "h2:Farm:support-east",
                SimulationConstructionPlacementZoneTypeCodes.FarmSupport,
                3500, 6000, -2500, 2500, 2500,
                Array.Empty<string>()),
            Zone("placement-zone:farm:incident-containment",
                SimulationRegionalDevelopmentCodes.FarmIncidentContainmentH2,
                SimulationConstructionPlacementZoneTypeCodes.FarmSupport,
                -1500, 1500, 1000, 2500, 2000,
                Array.Empty<string>()),
        };
        return new 경영SimulationSessionAggregate(new()
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:farm-player-placement",
            ScenarioDataRevision = "r1",
            ScenarioSeed = 20260823,
            RuleRevision = "farm-player-placement.v1",
            DurationTicks = 20,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:player",
                TerritoryStableId = "territory:pyeongchang",
                SettlementStableId = "settlement:farm",
                GameDateStartsOn = new DateTimeOffset(
                    2026, 8, 23, 0, 0, 0, TimeSpan.Zero),
            },
            IntegratedWorld = integrated,
            SpatialWorld = new Simulation공간세계InitialStateRequest
            {
                Definitions = PyeongchangSimulation공간상호작용Fixture
                    .CreateFarmHubSupply("facility:farm:warehouse", "facility:market")
                    .Definitions.Concat(PyeongchangSimulation공간상호작용Fixture
                        .CreateNatureThreatResponse().Definitions).ToArray(),
            },
            FarmSurvival = new SimulationFarmSurvivalInitialStateRequest
            {
                RuleRevision = SimulationFarmSurvivalCodes.RuleRevision,
                RegionStableId = "region:test:farm-development",
                AreaStableId = "area:test:farm-development",
                TileKey = "kr5186:l2:700:1145",
                FarmBuildingStableId = "facility:farm:warehouse",
                Actors = new[]
                {
                    new SimulationFarmActorInitialStateRequest
                    {
                        ActorStableId = "actor:test:farmer",
                        ActorKindCode = SimulationFarmSurvivalCodes.Player,
                        KoreanName = "발전 시험 농장 작업자",
                        CapabilityCodes = new[]
                        {
                            SimulationFarmActorCapabilityCodes.FarmHarvest,
                            SimulationFarmActorCapabilityCodes.FarmCollection,
                            SimulationFarmActorCapabilityCodes.FarmPacking,
                        },
                    },
                },
                CultivationUnits = new[]
                {
                    new Simulation재배단위Snapshot
                    {
                        CultivationUnitStableId = "cultivation:test:development-potato-01",
                        Revision = 1,
                        TileStableId = "soil:test:development-potato",
                        CultivationStableId = "crop:test:development-potato",
                        ProductStableId = "product:potato",
                        CropVariantStableId = "crop-variant:potato.fixture",
                        StateCode = Simulation재배단위상태Codes.HarvestReady,
                        PhysicalAreaSquareMeters = 100m,
                        EffectiveCultivationAreaRatio = 1m,
                        SourceStableIds = new[] { "source:test:development-cultivation" },
                    },
                    new Simulation재배단위Snapshot
                    {
                        CultivationUnitStableId = "cultivation:test:development-potato-02",
                        Revision = 1,
                        TileStableId = "soil:test:development-potato-02",
                        CultivationStableId = "crop:test:development-potato-02",
                        ProductStableId = "product:potato",
                        CropVariantStableId = "crop-variant:potato.fixture",
                        StateCode = Simulation재배단위상태Codes.HarvestReady,
                        PhysicalAreaSquareMeters = 100m,
                        EffectiveCultivationAreaRatio = 1m,
                        SourceStableIds = new[] { "source:test:development-cultivation" },
                    },
                    new Simulation재배단위Snapshot
                    {
                        CultivationUnitStableId = "cultivation:test:development-potato-03",
                        Revision = 1,
                        TileStableId = "soil:test:development-potato-03",
                        CultivationStableId = "crop:test:development-potato-03",
                        ProductStableId = "product:potato",
                        CropVariantStableId = "crop-variant:potato.fixture",
                        StateCode = Simulation재배단위상태Codes.HarvestReady,
                        PhysicalAreaSquareMeters = 100m,
                        EffectiveCultivationAreaRatio = 1m,
                        SourceStableIds = new[] { "source:test:development-cultivation" },
                    },
                },
                PotatoProductionRule = new Simulation감자생산RuleSnapshot
                {
                    RuleStableId = "rule:test:development-potato-production",
                    RuleRevision = 1,
                    SourceTypeCode = Simulation생산규칙SourceTypeCodes.Fixture,
                    ProductStableId = "product:potato",
                    CropVariantStableId = "crop-variant:potato.fixture",
                    BaseYieldKilogramsPerSquareMeter = 3m,
                    MinimumEnvironmentFactor = 0.5m,
                    MaximumEnvironmentFactor = 1m,
                    MinimumInputFactor = 0.8m,
                    MaximumInputFactor = 1.2m,
                    MinimumFacilityFactor = 0.8m,
                    MaximumFacilityFactor = 1.2m,
                    MinimumLossFactor = 0.1m,
                    MaximumLossFactor = 1m,
                    SourceStableIds = new[] { "source:test:development-potato-rule" },
                    Limitations = new[] { "시험 전용" },
                },
            },
        });
    }

    private static SimulationFacilityDefinitionRequest Definition(string id, string type)
        => new()
        {
            FacilityDefinitionStableId = id,
            Revision = "r1",
            HashSha256 = "sha256:" + id + ":r1",
            FacilityTypeCode = type,
            CapabilityCodes = Array.Empty<string>(),
        };

    private static SimulationFacilityBlueprintRequest Blueprint(string id,
        string definition, string kind, int width, int depth, int clearance,
        int maxSlope, bool requiresRoad, decimal material,
        params string[] zoneTypes) => new()
    {
        BlueprintStableId = id,
        Revision = "r1",
        HashSha256 = "sha256:" + id + ":r1",
        FacilityDefinitionStableId = definition,
        ConstructionTicks = 2,
        PlacementKindCode = kind,
        AllowedPlacementZoneTypeCodes = zoneTypes,
        FootprintWidthCentimeters = width,
        FootprintDepthCentimeters = depth,
        ClearanceCentimeters = clearance,
        MaxSlopeMilliDegrees = maxSlope,
        RequiresRoadAccess = requiresRoad,
        Materials = new[]
        {
            new SimulationIntegratedItemRequirement
            {
                ItemCode = SimulationIntegratedItemCodes.FacilityComponent,
                Quantity = material,
                UnitCode = "unit",
            },
        },
    };

    private static SimulationFacilityBlueprintRequest TreeBlueprint()
    {
        var result = Blueprint("blueprint:farm-tree.v1",
            "facility-definition:farm-tree.v1",
            SimulationConstructionPlacementKindCodes.Tree,
            200, 200, 50, 7000, false, 1m,
            SimulationConstructionPlacementZoneTypeCodes.FarmSupport);
        result.Materials = new[]
        {
            new SimulationIntegratedItemRequirement
            {
                ItemCode = SimulationIntegratedItemCodes.FarmSapling,
                Quantity = 1m,
                UnitCode = "tree",
            },
        };
        return result;
    }

    private static SimulationFarmConstructionPlacementPreviewSnapshot Confirm(
        경영SimulationSessionAggregate session, string commandId,
        SimulationFarmConstructionPlacementPreviewRequest request)
    {
        var preview = session.PreviewFarmConstructionPlacement(request);
        Assert.True(preview.CanConfirm, string.Join(",", preview.BlockingReasonCodes));
        session.ConfirmFarmConstructionPlacement(new()
        {
            CommandId = commandId,
            ExpectedRevision = session.Revision,
            PlacementProposalStableId = preview.PlacementProposalStableId,
            PreviewHashSha256 = preview.PreviewHashSha256,
        });
        return preview;
    }

    private static SimulationConstructionPlacementZoneRequest Zone(string id, string h2,
        string type, int minX, int maxX, int minZ, int maxZ, int slope,
        string[] connectors, string fenceChain = "", int? startX = null,
        int? startZ = null) => new()
    {
        PlacementZoneStableId = id,
        TargetH2StableId = h2,
        ZoneTypeCode = type,
        PlacementProfileRevision = "farm-placement-profile.r1",
        MinXCentimeters = minX,
        MaxXCentimeters = maxX,
        MinZCentimeters = minZ,
        MaxZCentimeters = maxZ,
        TerrainSlopeMilliDegrees = slope,
        RoadAccessConnectorStableIds = connectors,
        FenceChainStableId = fenceChain,
        FenceStartXCentimeters = startX,
        FenceStartZCentimeters = startZ,
    };

    private static void Advance(경영SimulationSessionAggregate session, string commandId)
        => session.Advance(new 경영SimulationTick진행Request
        {
            CommandId = commandId,
            ExpectedRevision = session.Revision,
            TickCount = 1,
        });
}
