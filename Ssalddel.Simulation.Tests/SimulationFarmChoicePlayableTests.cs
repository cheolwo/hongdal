using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationFarmChoicePlayableTests
{
    private const string Player = "actor:sim.farmer-1";
    private const string CultivationUnit = "cultivation-unit:sim.potato-ready-1";

    [Fact]
    public void 실제수확Lot이생기기전에는_선택상황을열지않는다()
    {
        var aggregate = CreateAggregate();

        var context = aggregate.GetFarmChoiceContext();

        Assert.Equal(SimulationFarmChoicePlayableCodes.AwaitingHarvest,
            context.SituationStateCode);
        Assert.Empty(context.HarvestLotStableId);
        Assert.Empty(context.Candidates);
        Assert.Equal("HarvestLotNotReady", Assert.Single(context.Facts).FactCode);
    }

    [Fact]
    public void 밭에서수확한Lot은_마당집하가끝나기전까지선택상황을열지않는다()
    {
        var aggregate = CreateHarvestedAtFieldAggregate(out var harvestLot);

        var context = aggregate.GetFarmChoiceContext();

        Assert.Equal(Simulation수확Lot상태Codes.HarvestedAtField,
            harvestLot.StateCode);
        Assert.Equal(SimulationFarmChoicePlayableCodes.AwaitingHarvest,
            context.SituationStateCode);
        Assert.Empty(context.Candidates);
    }

    [Fact]
    public void 감자수확상황은_서버가근거있는세선택을제공한다()
    {
        var aggregate = CreateChoiceReadyAggregate(out var harvestLot);

        var context = aggregate.GetFarmChoiceContext();

        Assert.Equal(SimulationFarmChoicePlayableCodes.SituationStableId,
            context.SituationStableId);
        Assert.Equal(SimulationFarmChoicePlayableCodes.AwaitingChoice,
            context.SituationStateCode);
        Assert.Equal(harvestLot.HarvestLotStableId, context.HarvestLotStableId);
        Assert.Equal(harvestLot.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + harvestLot.UnitCode, context.Facts[0].ValueCode);
        Assert.Equal(3, context.Candidates.Length);
        Assert.All(context.Candidates, candidate =>
        {
            Assert.NotEmpty(candidate.ChoiceStableId);
            Assert.NotEmpty(candidate.CandidateReasons);
            Assert.All(candidate.CandidateReasons,
                reason => Assert.All(reason.SourceFactStableIds,
                    sourceFactStableId => Assert.Contains(context.Facts,
                        fact => fact.FactStableId == sourceFactStableId)));
        });
        Assert.Equal(3, context.Candidates.Select(candidate => candidate.ChoiceStableId)
            .Distinct(StringComparer.Ordinal).Count());
        Assert.True(context.IsSimulationOnly);
        Assert.False(context.IsOperationalState);
    }

    [Fact]
    public void Preview는_안정선택식별자만받고_상태를바꾸지않는다()
    {
        var aggregate = CreateChoiceReadyAggregate(out var harvestLot);
        var before = aggregate.Snapshot();

        var preview = aggregate.PreviewFarmChoice(new SimulationFarmChoicePreviewRequest
        {
            ExpectedRevision = before.Revision,
            ChoiceStableId = SimulationFarmChoicePlayableCodes.ReserveStorageChoice,
        });
        var after = aggregate.Snapshot();

        Assert.Equal(before.Revision, after.Revision);
        Assert.Equal(before.Decisions.Length, after.Decisions.Length);
        Assert.Equal(before.Tasks.Length, after.Tasks.Length);
        Assert.Equal(before.Effects.Length, after.Effects.Length);
        Assert.True(preview.IsCandidateOnly);
        Assert.True(preview.RequiresExplicitConfirm);
        Assert.Equal(SimulationHarvestDispositionChoiceCodes.ReserveStorage,
            preview.Impact.ChoiceCode);
        Assert.Equal(harvestLot.Quantity, preview.Impact.Quantity);
        Assert.Equal(harvestLot.HarvestLotStableId,
            preview.Impact.HarvestLotStableId);
        Assert.Equal(harvestLot.UnitCode, preview.Impact.SourceUnitCode);
    }

    [Fact]
    public void Confirm계약은_클라이언트계산수치를받지않는다()
    {
        var properties = typeof(SimulationFarmChoiceConfirmRequest).GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "ChoiceStableId", "CommandId", "ExpectedRevision" },
            properties);
    }

    [Fact]
    public void 기존수확영향API도_Farm세션에서는실제Lot수량과계보를검증한다()
    {
        var aggregate = CreateChoiceReadyAggregate(out var harvestLot);

        var mismatch = Assert.Throws<SimulationConflictException>(() =>
            aggregate.PreviewHarvestDispositionImpact(
                new SimulationHarvestDispositionImpactPreviewRequest
                {
                    DispositionDecisionStableId =
                        "harvest-disposition:test.client-supplied",
                    DispositionDecisionRevision = 1,
                    HarvestLotStableId = harvestLot.HarvestLotStableId,
                    HarvestLotRevision = harvestLot.Revision,
                    ProductStableId = harvestLot.ProductStableId,
                    Quantity = harvestLot.Quantity + 1m,
                    UnitCode = harvestLot.UnitCode,
                    ChoiceCode = SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
                    NextWorkflowCode =
                        SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate,
                    ActorStableId = Player,
                    SourceStableIds = harvestLot.SourceStableIds
                        .Append(harvestLot.HarvestLotStableId)
                        .ToArray(),
                }));

        Assert.Equal("SimulationHarvestLotQuantityMismatch", mismatch.ErrorCode);
    }

    [Fact]
    public void 보관과직거래선택은_Tick뒤서로다른권위상태를만든다()
    {
        var storage = ExecuteChoice(
            SimulationFarmChoicePlayableCodes.ReserveStorageChoice,
            "storage");
        var direct = ExecuteChoice(
            SimulationFarmChoicePlayableCodes.TownDirectSaleChoice,
            "direct");

        Assert.True(storage.Settlement!.StorageOccupied
            > direct.Settlement!.StorageOccupied);
        Assert.True(direct.Settlement.MarketSupplyByProduct
            .Single(item => item.ProductStableId == "product:potato").Quantity
            > storage.Settlement.MarketSupplyByProduct
                .Single(item => item.ProductStableId == "product:potato").Quantity);
        Assert.NotEqual(
            storage.Settlement.HarvestLotAllocations.Single().ChoiceCode,
            direct.Settlement.HarvestLotAllocations.Single().ChoiceCode);
    }

    [Fact]
    public void 출고포장은_명시적인Hub선택적용전에는진행할수없다()
    {
        var aggregate = CreateChoiceReadyAggregate(out var harvestLot);

        var beforeChoice = aggregate.PreviewFarmWork(
            PackingPreview(aggregate.Revision, harvestLot.HarvestLotStableId));
        Assert.Contains("SimulationHarvestDispositionChoiceRequired",
            beforeChoice.BlockingReasonCodes);

        var choice = aggregate.PreviewFarmChoice(new SimulationFarmChoicePreviewRequest
        {
            ExpectedRevision = aggregate.Revision,
            ChoiceStableId = SimulationFarmChoicePlayableCodes.ReserveStorageChoice,
        });
        var confirmed = aggregate.ConfirmFarmChoice(new SimulationFarmChoiceConfirmRequest
        {
            CommandId = "command:farm-choice:test:storage-for-packing",
            ExpectedRevision = choice.BaseRevision,
            ChoiceStableId = choice.ChoiceStableId,
        });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:farm-choice:test:storage-for-packing:tick",
            ExpectedRevision = confirmed.Revision,
            TickCount = choice.Impact.DurationTicks,
        });

        var storageChoice = aggregate.PreviewFarmWork(
            PackingPreview(aggregate.Revision, harvestLot.HarvestLotStableId));
        Assert.Contains(
            "SimulationHarvestDispositionChoiceDoesNotUseOutboundPacking",
            storageChoice.BlockingReasonCodes);
    }

    [Fact]
    public void 낡은개정과이미확정된상황은_다시선택할수없다()
    {
        var aggregate = CreateChoiceReadyAggregate(out _);
        var before = aggregate.Snapshot();
        var stale = Assert.Throws<SimulationConflictException>(() =>
            aggregate.PreviewFarmChoice(new SimulationFarmChoicePreviewRequest
            {
                ExpectedRevision = before.Revision - 1,
                ChoiceStableId = SimulationFarmChoicePlayableCodes.HubShipmentChoice,
            }));
        Assert.Equal("SimulationExpectedRevisionMismatch", stale.ErrorCode);

        var preview = aggregate.PreviewFarmChoice(new SimulationFarmChoicePreviewRequest
        {
            ExpectedRevision = before.Revision,
            ChoiceStableId = SimulationFarmChoicePlayableCodes.HubShipmentChoice,
        });
        var confirmRequest = new SimulationFarmChoiceConfirmRequest
        {
            CommandId = "command:farm-choice:test:once",
            ExpectedRevision = preview.BaseRevision,
            ChoiceStableId = preview.ChoiceStableId,
        };
        var confirmed = aggregate.ConfirmFarmChoice(confirmRequest);
        var retried = aggregate.ConfirmFarmChoice(confirmRequest);
        Assert.Equal(confirmed.Revision, retried.Revision);

        var repeated = Assert.Throws<SimulationConflictException>(() =>
            aggregate.PreviewFarmChoice(new SimulationFarmChoicePreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                ChoiceStableId = SimulationFarmChoicePlayableCodes.ReserveStorageChoice,
            }));
        Assert.Equal("SimulationFarmChoiceAlreadyConfirmed", repeated.ErrorCode);
    }

    [Fact]
    public void Hub출하실적은_접근원장과Connector를거쳐_새AreaSetWI를연다()
    {
        var aggregate = CreateChoiceReadyAggregate(out _);
        var initial = aggregate.GetPlayerAreaAccess(
            SimulationAreaAccessCodes.PlayerOwner);
        var locked = initial.AccessEntries.Single(value =>
            value.AreaSetStableId == SimulationAreaAccessCodes.HubAreaSet);
        var sourceHHash = locked.SourceHDefinitionHashSha256;
        var blocked = aggregate.PreviewAreaTraversal(
            new SimulationAreaTraversalPreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                PlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                TargetAreaSetStableId = SimulationAreaAccessCodes.HubAreaSet,
                ConnectorStableId = SimulationAreaAccessCodes.FarmToHubConnector,
            });
        Assert.False(blocked.CanConfirm);
        Assert.Contains("SimulationAreaAccessEvidenceMissing",
            blocked.BlockingReasonCodes);

        var choice = aggregate.PreviewFarmChoice(
            new SimulationFarmChoicePreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                ChoiceStableId = SimulationFarmChoicePlayableCodes.HubShipmentChoice,
            });
        var reserved = aggregate.ConfirmFarmChoice(
            new SimulationFarmChoiceConfirmRequest
            {
                CommandId = "command:farm-choice:area-access-evidence",
                ExpectedRevision = choice.BaseRevision,
                ChoiceStableId = choice.ChoiceStableId,
            });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:farm-choice:area-access-evidence:tick",
            ExpectedRevision = reserved.Revision,
            TickCount = choice.Impact.DurationTicks,
        });

        var granted = aggregate.GetPlayerAreaAccess(
            SimulationAreaAccessCodes.PlayerOwner).AccessEntries.Single(value =>
            value.AreaSetStableId == SimulationAreaAccessCodes.HubAreaSet);
        Assert.Equal(SimulationAreaAccessCodes.Granted, granted.AccessStateCode);
        Assert.Contains(SimulationAreaAccessCodes.FarmHubShipmentEvidence,
            granted.GrantedByEvidenceIds);
        Assert.Equal(sourceHHash, granted.SourceHDefinitionHashSha256);

        var traversalInput = new SimulationAreaTraversalPreviewRequest
        {
            ExpectedRevision = aggregate.Revision,
            PlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
            TargetAreaSetStableId = SimulationAreaAccessCodes.HubAreaSet,
            ConnectorStableId = SimulationAreaAccessCodes.FarmToHubConnector,
        };
        var traversal = aggregate.PreviewAreaTraversal(traversalInput);
        Assert.True(traversal.CanConfirm);
        Assert.Contains(SimulationAreaAccessCodes.HubManufacturingWorldInteraction,
            traversal.NewWorldInteractionIds);
        var moving = aggregate.ConfirmAreaTraversal(
            new SimulationAreaTraversalConfirmRequest
            {
                CommandId = "command:area-access:farm-to-hub",
                ExpectedRevision = traversal.BaseRevision,
                PlayerStableId = traversal.PlayerStableId,
                TargetAreaSetStableId = traversal.TargetAreaSetStableId,
                ConnectorStableId = traversal.ConnectorStableId,
            });
        var arrived = aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:area-access:farm-to-hub:tick",
            ExpectedRevision = moving.Revision,
            TickCount = traversal.DurationTicks,
        });
        var access = arrived.AreaAccess;
        var entered = access.AccessEntries.Single(value =>
            value.AreaSetStableId == SimulationAreaAccessCodes.HubAreaSet);
        Assert.Equal(SimulationAreaAccessCodes.HubAreaSet,
            access.CurrentAreaSetStableId);
        Assert.Equal(SimulationAreaAccessCodes.Entered, entered.AccessStateCode);
        Assert.Contains(SimulationAreaAccessCodes.HubManufacturingWorldInteraction,
            entered.AvailableWorldInteractionIds);
        Assert.False(access.MutatesStaticHDefinitions);
        Assert.Equal(sourceHHash, entered.SourceHDefinitionHashSha256);

        var save = aggregate.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:area-access:farm-to-hub",
            ExpectedRevision = aggregate.Revision,
        });
        var restored = SimulationSessionReplay.Restore(save).Snapshot();
        Assert.Equal(SimulationSaveSchemaVersions.V10, save.SchemaVersion);
        Assert.Equal(access.CurrentAreaSetStableId,
            restored.AreaAccess.CurrentAreaSetStableId);
        Assert.Equal(entered.AccessHashSha256,
            restored.AreaAccess.AccessEntries.Single(value =>
                value.AreaSetStableId == SimulationAreaAccessCodes.HubAreaSet)
                .AccessHashSha256);
    }

    [Fact]
    public void 작은창고건설은_Tick뒤저장용량을늘리고_다음비축선택을연다()
    {
        var aggregate = CreateChoiceReadyAggregate(
            CreateRequest(storageOccupied: 2000m,
                integratedWorld: CreateSmallStorageIntegratedWorld()),
            out var harvestLot);
        var before = aggregate.GetFarmChoiceContext();
        var blockedStorage = before.Candidates.Single(value =>
            value.ChoiceStableId
                == SimulationFarmChoicePlayableCodes.ReserveStorageChoice);
        Assert.False(blockedStorage.IsAvailable);
        Assert.Contains("InsufficientStorageCapacity",
            blockedStorage.BlockReasonCodes);
        Assert.Equal(0m, aggregate.Snapshot().Settlement!.StorageAvailable);

        var construction = new SimulationIntegratedWorldCommandRequest
        {
            ActionCode = SimulationIntegratedWorldActionCodes.ConstructionOrder,
            CommandId = "command:farm-choice:build-small-storage",
            ExpectedRevision = aggregate.Revision,
            Construction = new SimulationConstructionOrderPayload
            {
                BlueprintStableId = "blueprint:farm-small-storage.v1",
                BuildSiteH1StableId = "h1:Farm:small-storage-build-site",
            },
        };
        var preview = aggregate.PreviewIntegratedWorldCommand(construction);
        Assert.True(preview.CanConfirm);
        var planned = aggregate.ConfirmIntegratedWorldCommand(construction);
        Assert.Equal(2000m, planned.Settlement!.StorageCapacity);
        Assert.DoesNotContain(planned.Settlement.Facilities, value =>
            value.FacilityStableId
                == "facility:player-built:command:farm-choice:build-small-storage");

        Advance(aggregate, "command:farm-choice:build-small-storage:start");
        var building = aggregate.Snapshot();
        Assert.Equal(2000m, building.Settlement!.StorageCapacity);

        Advance(aggregate, "command:farm-choice:build-small-storage:complete");
        var completed = aggregate.Snapshot();
        var builtFacilityId =
            "facility:player-built:command:farm-choice:build-small-storage";
        Assert.Equal(2500m, completed.Settlement!.StorageCapacity);
        Assert.Equal(500m, completed.Settlement.StorageAvailable);
        Assert.Contains(completed.Settlement.Facilities, value =>
            value.FacilityStableId == builtFacilityId
            && value.FacilityTypeCode
                == SimulationSettlementFacilityTypeCodes.Storage);
        var runtimeFacility = completed.IntegratedWorld.Facilities.Single(value =>
            value.FacilityStableId == builtFacilityId);
        Assert.Equal(SimulationFacilityLifecycleCodes.Operational,
            runtimeFacility.LifecycleCode);
        Assert.Equal(500m, runtimeFacility.DefinedCapacities.Single(value =>
            value.CapacityCode == Simulation공간용량Codes.StorageCapacity).Quantity);

        var opened = aggregate.GetFarmChoiceContext();
        Assert.True(opened.Candidates.Single(value =>
            value.ChoiceStableId
                == SimulationFarmChoicePlayableCodes.ReserveStorageChoice).IsAvailable);
        var storagePreview = aggregate.PreviewFarmChoice(
            new SimulationFarmChoicePreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                ChoiceStableId = SimulationFarmChoicePlayableCodes.ReserveStorageChoice,
            });
        var confirmed = aggregate.ConfirmFarmChoice(
            new SimulationFarmChoiceConfirmRequest
            {
                CommandId = "command:farm-choice:store-in-small-storage",
                ExpectedRevision = storagePreview.BaseRevision,
                ChoiceStableId = storagePreview.ChoiceStableId,
            });
        var applied = aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:farm-choice:store-in-small-storage:tick",
            ExpectedRevision = confirmed.Revision,
            TickCount = storagePreview.Impact.DurationTicks,
        });
        Assert.Equal(builtFacilityId, applied.Settlement!.HarvestLotAllocations
            .Single(value => value.HarvestLotStableId == harvestLot.HarvestLotStableId)
            .FacilityStableId);
        Assert.Equal(2294m, applied.Settlement.StorageOccupied);

        var save = aggregate.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:farm-small-storage-choice",
            ExpectedRevision = aggregate.Revision,
        });
        var replayed = SimulationSessionReplay.Restore(save).Snapshot();
        Assert.Equal(save.ReplayHash,
            SimulationReplayHasher.Calculate(save));
        Assert.Equal(applied.Settlement.StorageCapacity,
            replayed.Settlement!.StorageCapacity);
        Assert.Equal(applied.Settlement.StorageOccupied,
            replayed.Settlement.StorageOccupied);
        Assert.Equal(applied.Settlement.Facilities.Select(value =>
                (value.FacilityStableId, value.FacilityTypeCode)),
            replayed.Settlement.Facilities.Select(value =>
                (value.FacilityStableId, value.FacilityTypeCode)));
    }

    [Fact]
    public void Solo세계는_초대형Hosted로열리고_손님권한과감사를_저장재생한다()
    {
        const string guest = "player:sim.guest-1";
        var aggregate = CreateAggregate();
        var initial = aggregate.GetHostedWorldState();
        Assert.Equal(SimulationHostedWorldCodes.Solo, initial.SessionModeCode);
        Assert.True(initial.HostLossBlocksMutation);
        Assert.False(initial.EscPausesWorld);

        var open = aggregate.PreviewOpenHostedWorld(
            new SimulationHostedWorldOpenPreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                OwnerPlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                InvitedGuestPlayerStableId = guest,
            });
        Assert.True(open.CanConfirm);
        var opening = aggregate.ConfirmOpenHostedWorld(
            new SimulationHostedWorldOpenConfirmRequest
            {
                CommandId = "command:hosted:open",
                ExpectedRevision = open.BaseRevision,
                OwnerPlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                InvitedGuestPlayerStableId = guest,
            });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:hosted:open:tick",
            ExpectedRevision = opening.Revision,
            TickCount = open.DurationTicks,
        });

        var invited = aggregate.GetHostedWorldState();
        Assert.Equal(SimulationHostedWorldCodes.HostedMultiplayer,
            invited.SessionModeCode);
        Assert.Equal(SimulationHostedWorldCodes.Invited,
            invited.Participants.Single(value => value.PlayerStableId == guest)
                .ParticipantStateCode);
        Assert.Empty(invited.PermissionGrants);

        var join = aggregate.PreviewJoinHostedWorld(
            new SimulationHostedWorldJoinPreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                GuestPlayerStableId = guest,
            });
        Assert.True(join.CanConfirm);
        var joining = aggregate.ConfirmJoinHostedWorld(
            new SimulationHostedWorldJoinConfirmRequest
            {
                CommandId = "command:hosted:join",
                ExpectedRevision = join.BaseRevision,
                GuestPlayerStableId = guest,
            });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:hosted:join:tick",
            ExpectedRevision = joining.Revision,
            TickCount = join.DurationTicks,
        });

        var active = aggregate.GetHostedWorldState();
        Assert.Equal(SimulationHostedWorldCodes.Active,
            active.Participants.Single(value => value.PlayerStableId == guest)
                .ParticipantStateCode);
        Assert.Equal(SimulationHostedWorldCodes.Allow,
            active.PermissionGrants.Single(value =>
                value.TargetPlayerStableId == guest
                && value.CapabilityCode == SimulationHostedWorldCodes.PerformWork)
                .GrantStateCode);
        Assert.Equal(SimulationHostedWorldCodes.Deny,
            active.PermissionGrants.Single(value =>
                value.TargetPlayerStableId == guest
                && value.CapabilityCode == SimulationHostedWorldCodes.Build)
                .GrantStateCode);
        Assert.Equal(SimulationHostedWorldCodes.Deny,
            active.PermissionGrants.Single(value =>
                value.TargetPlayerStableId == guest
                && value.CapabilityCode == SimulationHostedWorldCodes.Demolish)
                .GrantStateCode);

        var denied = aggregate.PreviewHostedGuestAction(
            new SimulationHostedGuestActionPreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                GuestPlayerStableId = guest,
                ScopeStableId = SimulationAreaAccessCodes.FarmAreaSet,
                CapabilityCode = SimulationHostedWorldCodes.Build,
                TargetStableId = "facility:sim.farm",
            });
        Assert.False(denied.CanConfirm);
        Assert.Contains("SimulationHostedPermissionDenied",
            denied.BlockingReasonCodes);

        var work = aggregate.PreviewHostedGuestAction(
            new SimulationHostedGuestActionPreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                GuestPlayerStableId = guest,
                ScopeStableId = SimulationAreaAccessCodes.FarmAreaSet,
                CapabilityCode = SimulationHostedWorldCodes.PerformWork,
                TargetStableId = "facility:sim.farm",
            });
        Assert.True(work.CanConfirm);
        var working = aggregate.ConfirmHostedGuestAction(
            new SimulationHostedGuestActionConfirmRequest
            {
                CommandId = "command:hosted:guest-work",
                ExpectedRevision = work.BaseRevision,
                GuestPlayerStableId = guest,
                ScopeStableId = SimulationAreaAccessCodes.FarmAreaSet,
                CapabilityCode = SimulationHostedWorldCodes.PerformWork,
                TargetStableId = "facility:sim.farm",
            });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:hosted:guest-work:tick",
            ExpectedRevision = working.Revision,
            TickCount = work.DurationTicks,
        });
        var applied = aggregate.GetHostedWorldState();
        Assert.Contains(applied.AuditTrail, value =>
            value.EffectTypeCode
                == SimulationHostedWorldCodes.HostedGuestWorkCompleted
            && value.TargetPlayerStableId == guest);

        var save = aggregate.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:hosted:farm-helper",
            ExpectedRevision = aggregate.Revision,
        });
        var restored = SimulationSessionReplay.Restore(save).Snapshot();
        Assert.Equal(SimulationSaveSchemaVersions.V11, save.SchemaVersion);
        Assert.Equal(save.ReplayHash, SimulationReplayHasher.Calculate(save));
        Assert.Equal(applied.SessionHashSha256,
            restored.HostedWorld.SessionHashSha256);
        Assert.Equal(applied.PermissionGrants.Select(value => value.GrantHashSha256),
            restored.HostedWorld.PermissionGrants.Select(value => value.GrantHashSha256));
    }

    [Fact]
    public void 두플레이어의실제Lot기여는_공동창고를완공하고_보호점으로복원된다()
    {
        const string guest = "player:sim.guest-1";
        const string ownerLot = "lot:coop.owner.components";
        const string guestLot = "lot:coop.guest.components";
        var request = CreateRequest(
            storageOccupied: 2000m,
            integratedWorld: CreateCoopStorageIntegratedWorld());
        request.NatureMind = new SimulationNatureMindInitialStateRequest
        {
            Players = new[]
            {
                new SimulationNatureMindPlayerInitialStateRequest
                {
                    PlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                    RecoveryBaseOutput = 3m,
                    ThreatBaseOutput = 7m,
                },
                new SimulationNatureMindPlayerInitialStateRequest
                {
                    PlayerStableId = guest,
                    RecoveryBaseOutput = 20m,
                    ThreatBaseOutput = 1m,
                },
            },
        };
        var aggregate = CreateChoiceReadyAggregate(request, out _);
        var initialGuestMind = aggregate.GetNatureMindState();
        Assert.Equal(SimulationNaturePeriodCodes.GwangbokPeriod,
            initialGuestMind.Periods.Single(value =>
                value.PlayerStableId == guest).PeriodStateCode);

        var hubChoice = aggregate.PreviewFarmChoice(
            new SimulationFarmChoicePreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                ChoiceStableId = SimulationFarmChoicePlayableCodes.HubShipmentChoice,
            });
        var hubConfirmed = aggregate.ConfirmFarmChoice(
            new SimulationFarmChoiceConfirmRequest
            {
                CommandId = "command:final:hub-choice",
                ExpectedRevision = hubChoice.BaseRevision,
                ChoiceStableId = hubChoice.ChoiceStableId,
            });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:final:hub-choice:tick",
            ExpectedRevision = hubConfirmed.Revision,
            TickCount = hubChoice.Impact.DurationTicks,
        });
        var guestBalanceAfterDisposition = aggregate.GetNatureMindState().Balances
            .Single(value => value.PlayerStableId == guest);
        Assert.True(guestBalanceAfterDisposition.RecoveryShare >= .8m,
            $"recovery={guestBalanceAfterDisposition.RecoveryOutput}; threat={guestBalanceAfterDisposition.ThreatOutput}; share={guestBalanceAfterDisposition.RecoveryShare}");
        Assert.Equal(SimulationNaturePeriodCodes.GwangbokPeriod,
            aggregate.GetNatureMindState().Periods.Single(value =>
                value.PlayerStableId == guest).PeriodStateCode);
        var traversal = aggregate.PreviewAreaTraversal(
            new SimulationAreaTraversalPreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                PlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                TargetAreaSetStableId = SimulationAreaAccessCodes.HubAreaSet,
                ConnectorStableId = SimulationAreaAccessCodes.FarmToHubConnector,
            });
        var traversalConfirmed = aggregate.ConfirmAreaTraversal(
            new SimulationAreaTraversalConfirmRequest
            {
                CommandId = "command:final:farm-to-hub",
                ExpectedRevision = traversal.BaseRevision,
                PlayerStableId = traversal.PlayerStableId,
                TargetAreaSetStableId = traversal.TargetAreaSetStableId,
                ConnectorStableId = traversal.ConnectorStableId,
            });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:final:farm-to-hub:tick",
            ExpectedRevision = traversalConfirmed.Revision,
            TickCount = traversal.DurationTicks,
        });
        Assert.Equal(SimulationAreaAccessCodes.HubAreaSet,
            aggregate.GetPlayerAreaAccess(SimulationAreaAccessCodes.PlayerOwner)
                .CurrentAreaSetStableId);
        var ownerInterpretation = aggregate.GetNatureFarmInterpretation(
            SimulationAreaAccessCodes.PlayerOwner);
        var guestInterpretation = aggregate.GetNatureFarmInterpretation(guest);
        Assert.Equal(ownerInterpretation.FactValue, guestInterpretation.FactValue);
        Assert.NotEqual(ownerInterpretation.InferenceCode,
            guestInterpretation.InferenceCode);
        Assert.Equal(SimulationNaturePeriodCodes.GwangbokPeriod,
            aggregate.GetNatureMindState().Periods.Single(value =>
                value.PlayerStableId == guest).PeriodStateCode);

        var open = aggregate.PreviewOpenHostedWorld(
            new SimulationHostedWorldOpenPreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                OwnerPlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                InvitedGuestPlayerStableId = guest,
            });
        var opening = aggregate.ConfirmOpenHostedWorld(
            new SimulationHostedWorldOpenConfirmRequest
            {
                CommandId = "command:coop:hosted-open",
                ExpectedRevision = open.BaseRevision,
                OwnerPlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
                InvitedGuestPlayerStableId = guest,
            });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:coop:hosted-open:tick",
            ExpectedRevision = opening.Revision,
            TickCount = 1,
        });
        var join = aggregate.PreviewJoinHostedWorld(
            new SimulationHostedWorldJoinPreviewRequest
            {
                ExpectedRevision = aggregate.Revision,
                GuestPlayerStableId = guest,
            });
        var joining = aggregate.ConfirmJoinHostedWorld(
            new SimulationHostedWorldJoinConfirmRequest
            {
                CommandId = "command:coop:hosted-join",
                ExpectedRevision = join.BaseRevision,
                GuestPlayerStableId = guest,
            });
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:coop:hosted-join:tick",
            ExpectedRevision = joining.Revision,
            TickCount = 1,
        });
        Assert.Contains(aggregate.GetCoopConstructionState().ProtectionCheckpoints,
            value => value.CheckpointKindCode
                == SimulationCoopConstructionCodes.HostedSessionProtection);

        var ownerRequest = ContributionRequest(aggregate.Revision,
            SimulationAreaAccessCodes.PlayerOwner, ownerLot, 1, 2m);
        var guestRequest = ContributionRequest(aggregate.Revision,
            guest, guestLot, 1, 2m);
        var ownerPreview = aggregate.PreviewCoopContribution(ownerRequest);
        var concurrentGuestPreview = aggregate.PreviewCoopContribution(guestRequest);
        Assert.True(ownerPreview.CanConfirm);
        Assert.True(concurrentGuestPreview.CanConfirm);
        Assert.Equal(2m, ownerPreview.AcceptedQuantity);
        var ownerConfirm = ConfirmContribution(ownerRequest,
            "command:coop:owner-contribution");
        var ownerConfirmed = aggregate.ConfirmCoopContribution(ownerConfirm);
        var ownerDuplicate = aggregate.ConfirmCoopContribution(ownerConfirm);
        Assert.Equal(ownerConfirmed.Revision, ownerDuplicate.Revision);
        var payloadConflict = Assert.Throws<SimulationConflictException>(() =>
            aggregate.ConfirmCoopContribution(new SimulationCoopContributionConfirmRequest
            {
                CommandId = ownerConfirm.CommandId,
                ExpectedRevision = ownerConfirm.ExpectedRevision,
                PlayerStableId = ownerConfirm.PlayerStableId,
                ProjectStableId = ownerConfirm.ProjectStableId,
                BlueprintStableId = ownerConfirm.BlueprintStableId,
                BuildSiteH1StableId = ownerConfirm.BuildSiteH1StableId,
                SourceLotStableId = ownerConfirm.SourceLotStableId,
                ExpectedSourceLotRevision = ownerConfirm.ExpectedSourceLotRevision,
                RequestedQuantity = ownerConfirm.RequestedQuantity + 1m,
            }));
        Assert.Equal("SimulationCommandPayloadConflict", payloadConflict.ErrorCode);
        var stale = Assert.Throws<SimulationConflictException>(() =>
            aggregate.ConfirmCoopContribution(ConfirmContribution(guestRequest,
                "command:coop:guest-stale")));
        Assert.Equal("SimulationExpectedRevisionMismatch", stale.ErrorCode);
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:coop:owner-contribution:tick",
            ExpectedRevision = ownerConfirmed.Revision,
            TickCount = 1,
        });
        var halfway = aggregate.GetCoopConstructionState();
        Assert.Equal(SimulationCoopConstructionCodes.Frame,
            Assert.Single(halfway.Projects).StageCode);
        Assert.Equal(0m, halfway.SourceLots.Single(value =>
            value.LotStableId == ownerLot).RemainingQuantity);
        Assert.Equal(2000m, aggregate.Snapshot().Settlement!.StorageCapacity);

        var guestRetry = ContributionRequest(aggregate.Revision,
            guest, guestLot, 1, 20m);
        var guestPreview = aggregate.PreviewCoopContribution(guestRetry);
        Assert.Equal(2m, guestPreview.AcceptedQuantity);
        var guestConfirmed = aggregate.ConfirmCoopContribution(
            ConfirmContribution(guestRetry, "command:coop:guest-contribution"));
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:coop:guest-contribution:tick",
            ExpectedRevision = guestConfirmed.Revision,
            TickCount = 1,
        });
        var completed = aggregate.GetCoopConstructionState();
        var project = Assert.Single(completed.Projects);
        Assert.Equal(SimulationCoopConstructionCodes.Operational, project.StageCode);
        Assert.Equal(4m, project.ContributedMaterialQuantity);
        Assert.Equal(2, completed.Contributions.Length);
        Assert.All(completed.Contributions, value =>
            Assert.Equal(SimulationCoopConstructionCodes.Consumed, value.StateCode));
        Assert.Equal(2m, completed.Contributions.Single(value =>
            value.PlayerStableId == SimulationAreaAccessCodes.PlayerOwner).EffectiveWork);
        Assert.Equal(2.5m, completed.Contributions.Single(value =>
            value.PlayerStableId == guest).EffectiveWork);
        Assert.Equal(2500m, aggregate.Snapshot().Settlement!.StorageCapacity);
        var originalHHash = SimulationAreaAccessCodes.FarmToHubSourceHHashSha256;

        var demolish = aggregate.PreviewCoopDemolition(ProtectedRequest(
            aggregate.Revision));
        Assert.True(demolish.CanConfirm);
        var demolitionConfirmed = aggregate.ConfirmCoopDemolition(
            ProtectedConfirm(aggregate.Revision, "command:coop:demolish"));
        var checkpointBeforeTick = aggregate.GetCoopConstructionState()
            .ProtectionCheckpoints.Single(value => value.CheckpointKindCode
                == SimulationCoopConstructionCodes.DestructiveActionCheckpoint);
        Assert.False(checkpointBeforeTick.HistoricalEffectsDeleted);
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:coop:demolish:tick",
            ExpectedRevision = demolitionConfirmed.Revision,
            TickCount = 1,
        });
        Assert.Equal(2000m, aggregate.Snapshot().Settlement!.StorageCapacity);

        var restore = aggregate.PreviewCoopRestore(ProtectedRequest(
            aggregate.Revision));
        Assert.True(restore.CanConfirm);
        var restoreConfirmed = aggregate.ConfirmCoopRestore(
            ProtectedConfirm(aggregate.Revision, "command:coop:restore"));
        aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:coop:restore:tick",
            ExpectedRevision = restoreConfirmed.Revision,
            TickCount = 1,
        });
        var restoredState = aggregate.GetCoopConstructionState();
        Assert.Equal(2500m, aggregate.Snapshot().Settlement!.StorageCapacity);
        var restoreEffect = Assert.Single(restoredState.RestoreEffects);
        Assert.False(restoreEffect.DeletesHistoricalEffects);
        Assert.False(restoreEffect.DuplicatesResources);
        Assert.Equal(2, restoredState.Contributions.Length);
        Assert.Equal(0m, restoredState.SourceLots.Where(value =>
            value.LotStableId == ownerLot || value.LotStableId == guestLot)
            .Sum(value => value.RemainingQuantity));
        Assert.Equal(SimulationAreaAccessCodes.FarmToHubSourceHHashSha256,
            originalHHash);

        var observability = aggregate.GetGameplayObservability();
        Assert.Equal(2, observability.Traces.Length);
        Assert.True(observability.RawFactSeparatedFromInterpretation);
        Assert.False(observability.MoodProjectionChangesRules);
        Assert.All(observability.Traces, trace =>
        {
            Assert.NotEmpty(trace.HostedSessionStableId);
            Assert.NotEmpty(trace.WorldStableId);
            Assert.NotEmpty(trace.PlayerStableId);
            Assert.NotEmpty(trace.PermissionDecisionId);
            Assert.NotEmpty(trace.RequestIdempotencyId);
            Assert.NotEmpty(trace.DecisionStableId);
            Assert.NotEmpty(trace.TaskStableId);
            Assert.NotEmpty(trace.EffectStableId);
            Assert.NotEmpty(trace.ProjectStableId);
            Assert.NotEmpty(trace.InterpretationHash);
            Assert.Equal("SimulationWorldShell", trace.ProjectionCode);
        });

        var save = aggregate.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:coop:protected-farm-storage",
            ExpectedRevision = aggregate.Revision,
        });
        var replayed = SimulationSessionReplay.Restore(save).Snapshot();
        Assert.Equal(SimulationSaveSchemaVersions.V12, save.SchemaVersion);
        Assert.Equal(save.ReplayHash, SimulationReplayHasher.Calculate(save));
        Assert.Equal(restoredState.StateHashSha256,
            replayed.CoopConstruction.StateHashSha256);
        Assert.Equal(aggregate.Snapshot().Settlement!.StorageCapacity,
            replayed.Settlement!.StorageCapacity);
    }

    private static 경영SimulationSessionSnapshot ExecuteChoice(
        string choiceStableId, string commandSuffix)
    {
        var aggregate = CreateChoiceReadyAggregate(out _);
        var before = aggregate.Snapshot();
        var preview = aggregate.PreviewFarmChoice(new SimulationFarmChoicePreviewRequest
        {
            ExpectedRevision = before.Revision,
            ChoiceStableId = choiceStableId,
        });
        var confirmed = aggregate.ConfirmFarmChoice(new SimulationFarmChoiceConfirmRequest
        {
            CommandId = "command:farm-choice:test:" + commandSuffix,
            ExpectedRevision = preview.BaseRevision,
            ChoiceStableId = preview.ChoiceStableId,
        });
        var context = aggregate.GetFarmChoiceContext();
        Assert.Equal(SimulationFarmChoicePlayableCodes.ChoiceConfirmed,
            context.SituationStateCode);
        Assert.Equal(choiceStableId, context.AppliedChoiceStableId);

        return aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:farm-choice:tick:" + commandSuffix,
            ExpectedRevision = confirmed.Revision,
            TickCount = preview.Impact.DurationTicks,
        });
    }

    private static 경영SimulationSessionAggregate CreateAggregate()
        => new(CreateRequest());

    private static 경영SimulationSessionAggregate CreateHarvestedAtFieldAggregate(
        out Simulation수확LotSnapshot harvestLot)
    {
        var aggregate = CreateAggregate();
        var before = aggregate.Snapshot();
        var confirmed = aggregate.ConfirmFarmWork(new SimulationFarmWorkConfirmRequest
        {
            CommandId = "command:farm-choice:harvest",
            ExpectedRevision = before.Revision,
            ActorStableId = Player,
            TargetStableId = CultivationUnit,
            ActionCode = SimulationFarmSurvivalCodes.Harvesting,
            AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
            PreferredSpatialStableId =
                PyeongchangSimulation공간StableIds.대관령Farm수확공간,
        });
        var harvested = aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:farm-choice:harvest:tick",
            ExpectedRevision = confirmed.WorldRevision,
            TickCount = 1,
        });
        harvestLot = Assert.Single(harvested.FarmSurvival!.HarvestLots);
        return aggregate;
    }

    private static 경영SimulationSessionAggregate CreateChoiceReadyAggregate(
        out Simulation수확LotSnapshot harvestLot)
        => CreateChoiceReadyAggregate(CreateRequest(), out harvestLot);

    private static 경영SimulationSessionAggregate CreateChoiceReadyAggregate(
        경영SimulationSession생성Request request,
        out Simulation수확LotSnapshot harvestLot)
    {
        var aggregate = new 경영SimulationSessionAggregate(request);
        var before = aggregate.Snapshot();
        var harvestConfirmed = aggregate.ConfirmFarmWork(
            new SimulationFarmWorkConfirmRequest
            {
                CommandId = "command:farm-choice:harvest",
                ExpectedRevision = before.Revision,
                ActorStableId = Player,
                TargetStableId = CultivationUnit,
                ActionCode = SimulationFarmSurvivalCodes.Harvesting,
                AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
                PreferredSpatialStableId =
                    PyeongchangSimulation공간StableIds.대관령Farm수확공간,
            });
        var harvested = aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:farm-choice:harvest:tick",
            ExpectedRevision = harvestConfirmed.WorldRevision,
            TickCount = 1,
        });
        var atField = Assert.Single(harvested.FarmSurvival!.HarvestLots);
        var confirmed = aggregate.ConfirmFarmWork(new SimulationFarmWorkConfirmRequest
        {
            CommandId = "command:farm-choice:collect",
            ExpectedRevision = aggregate.Revision,
            ActorStableId = Player,
            TargetStableId = atField.HarvestLotStableId,
            ActionCode = SimulationFarmSurvivalCodes.HarvestCollection,
            AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
            PreferredSpatialStableId =
                PyeongchangSimulation공간StableIds.대관령Farm집하공간,
        });
        var collected = aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = "command:farm-choice:collect:tick",
            ExpectedRevision = confirmed.WorldRevision,
            TickCount = 1,
        });
        harvestLot = Assert.Single(collected.FarmSurvival!.HarvestLots);
        return aggregate;
    }

    private static SimulationFarmWorkPreviewRequest PackingPreview(
        long revision,
        string harvestLotStableId)
        => new()
        {
            ExpectedRevision = revision,
            ActorStableId = Player,
            TargetStableId = harvestLotStableId,
            ActionCode = SimulationFarmSurvivalCodes.OutboundPacking,
            AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
            PreferredSpatialStableId =
                PyeongchangSimulation공간StableIds.대관령Farm포장공간,
        };

    private static 경영SimulationSession생성Request CreateRequest(
        decimal storageOccupied = 1200m,
        SimulationIntegratedWorldInitialStateRequest? integratedWorld = null) => new()
    {
        ClientRequestId = Guid.NewGuid(),
        ScenarioStableId = "scenario:test.farm-choice-playable",
        ScenarioDataRevision = "scenario-data:farm-choice-playable.r1",
        ScenarioSeed = 20260821,
        RuleRevision = "rule:farm-choice-playable.r1",
        DurationTicks = 28,
        WorldContext = new SimulationWorldContext생성Request
        {
            FactionStableId = "faction:sim.farmers-1",
            TerritoryStableId = "territory:sim.farm-region-1",
            SettlementStableId = "settlement:sim.farm-town-1",
            GameDateStartsOn = new DateTimeOffset(
                2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
        },
        Settlement = new SimulationSettlementInitialStateRequest
        {
            TreasuryBalance = 1_000_000m,
            CurrencyCode = "KRW",
            LaborCapacityTotal = 100m,
            LaborReserved = 25m,
            StorageCapacity = 2000m,
            StorageOccupied = storageOccupied,
            StorageUnitCode = "KGM",
            PopulationCount = 100,
            PopulationFoodDemandPerTick = 100m,
            GarrisonCount = 20,
            GarrisonFoodDemandPerTick = 20m,
            FoodEquivalentUnitCode = "FoodEquivalentUnit",
            FoodEquivalentRuleRevision = "food-equivalent:fixture-r1",
            Districts = new[]
            {
                new SimulationSettlementDistrictRequest
                {
                    DistrictStableId = "district:sim.farm",
                    DistrictTypeCode = "FarmDistrict",
                    SourceStableIds = Sources(),
                },
                new SimulationSettlementDistrictRequest
                {
                    DistrictStableId = "district:sim.central",
                    DistrictTypeCode = "CentralDistrict",
                    SourceStableIds = Sources(),
                },
            },
            Facilities = new[]
            {
                new SimulationSettlementFacilityRequest
                {
                    FacilityStableId = "facility:sim.storage",
                    FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Storage,
                    DistrictStableId = "district:sim.farm",
                    SourceStableIds = Sources(),
                },
                new SimulationSettlementFacilityRequest
                {
                    FacilityStableId = "facility:sim.market",
                    FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Market,
                    DistrictStableId = "district:sim.central",
                    SourceStableIds = Sources(),
                },
            },
            MarketSupplyByProduct = new[]
            {
                new SimulationMarketSupplyRequest
                {
                    ProductStableId = "product:potato",
                    Quantity = 300m,
                    UnitCode = "KGM",
                    SourceStableIds = Sources(),
                },
            },
            ReserveStockLots = new[]
            {
                new SimulationReserveStockLotRequest
                {
                    StockLotStableId = "stock-lot:sim.potato-1",
                    ProductStableId = "product:potato",
                    StorageFacilityStableId = "facility:sim.storage",
                    Quantity = 1200m,
                    UnitCode = "KGM",
                    FoodEquivalentQuantity = 1200m,
                    SourceStableIds = Sources(),
                },
            },
            SourceStableIds = Sources(),
        },
        SpatialWorld = PyeongchangSimulation공간상호작용Fixture.CreateFarmHubSupply(
            "facility:sim.farm",
            "facility:sim.market"),
        FarmSurvival = new SimulationFarmSurvivalInitialStateRequest
        {
            RuleRevision = SimulationFarmSurvivalCodes.ScenicSeasonRuleRevision,
            RegionStableId = "region:legal-dong:5176031000",
            AreaStableId = "area:pyeongchang:daegwallyeong-farm",
            TileKey = "kr5186:l2:700:1145",
            FarmBuildingStableId = "facility:sim.farm",
            SupplyUnits = 8m,
            RepairMaterialUnits = 4m,
            SeedUnits = 2m,
            WaterUnits = 2m,
            Actors = new[]
            {
                new SimulationFarmActorInitialStateRequest
                {
                    ActorStableId = Player,
                    ActorKindCode = SimulationFarmSurvivalCodes.Player,
                    KoreanName = "감자 농장 작업자",
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
                    CultivationUnitStableId = CultivationUnit,
                    Revision = 1,
                    TileStableId = "soil-tile:sim.potato-ready-1",
                    CultivationStableId = "cultivation:sim.potato-ready-1",
                    ProductStableId = SimulationFarmChoicePlayableCodes.ProductStableId,
                    CropVariantStableId = "crop-variant:potato.fixture",
                    StateCode = Simulation재배단위상태Codes.HarvestReady,
                    PhysicalAreaSquareMeters = 100m,
                    EffectiveCultivationAreaRatio = 1m,
                    SourceStableIds = Sources(),
                },
            },
            PotatoProductionRule = new Simulation감자생산RuleSnapshot
            {
                RuleStableId = "rule:potato-production.farm-choice.v1",
                RuleRevision = 1,
                SourceTypeCode = Simulation생산규칙SourceTypeCodes.Fixture,
                ProductStableId = SimulationFarmChoicePlayableCodes.ProductStableId,
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
                SourceStableIds = Sources(),
                Limitations = new[] { "Simulation fixture only" },
            },
        },
        IntegratedWorld = integratedWorld,
    };

    private static SimulationIntegratedWorldInitialStateRequest
        CreateSmallStorageIntegratedWorld()
        => new()
        {
            ScenarioRevision = "farm-small-storage.r1",
            ScenarioHashSha256 = "sha256:farm-small-storage-r1",
            FacilityDefinitions = new[]
            {
                new SimulationFacilityDefinitionRequest
                {
                    FacilityDefinitionStableId =
                        "facility-definition:farm-small-storage.v1",
                    Revision = "r1",
                    HashSha256 =
                        "sha256:facility-definition-farm-small-storage-r1",
                    FacilityTypeCode = "FarmSmallStorage",
                    CapabilityCodes = new[]
                    {
                        SimulationIntegratedCapabilityCodes.Storage,
                        SimulationIntegratedCapabilityCodes.WorkerAccessible,
                    },
                    Capacities = new[]
                    {
                        new SimulationFacilityCapacityDefinitionRequest
                        {
                            CapacityCode = Simulation공간용량Codes.StorageCapacity,
                            Quantity = 500m,
                            UnitCode = "KGM",
                        },
                    },
                },
            },
            Actors = new[]
            {
                new SimulationIntegratedActorSeedRequest
                {
                    ActorStableId = "actor:sim.builder-1",
                    EligibilityRank = 1,
                    FarmLaborEligible = true,
                },
            },
            Lots = new[]
            {
                new SimulationIntegratedLotSeedRequest
                {
                    LotStableId = "lot:sim.small-storage-components",
                    ItemCode = SimulationIntegratedItemCodes.FacilityComponent,
                    Quantity = 2m,
                    UnitCode = "unit",
                    FacilityStableId = "facility:sim.farm",
                },
            },
            FacilityBlueprints = new[]
            {
                new SimulationFacilityBlueprintRequest
                {
                    BlueprintStableId = "blueprint:farm-small-storage.v1",
                    Revision = "r1",
                    HashSha256 = "sha256:blueprint-farm-small-storage-r1",
                    FacilityDefinitionStableId =
                        "facility-definition:farm-small-storage.v1",
                    SettlementFacilityTypeCode =
                        SimulationSettlementFacilityTypeCodes.Storage,
                    SettlementDistrictStableId = "district:sim.farm",
                    ConstructionTicks = 2,
                    Materials = new[]
                    {
                        new SimulationIntegratedItemRequirement
                        {
                            ItemCode = SimulationIntegratedItemCodes.FacilityComponent,
                            Quantity = 2m,
                            UnitCode = "unit",
                        },
                    },
                },
            },
        };

    private static SimulationIntegratedWorldInitialStateRequest
        CreateCoopStorageIntegratedWorld()
    {
        var value = CreateSmallStorageIntegratedWorld();
        value.ScenarioRevision = "farm-coop-small-storage.r1";
        value.ScenarioHashSha256 = "sha256:farm-coop-small-storage-r1";
        value.Lots = new[]
        {
            new SimulationIntegratedLotSeedRequest
            {
                LotStableId = "lot:coop.owner.components",
                ItemCode = SimulationIntegratedItemCodes.CoopFacilityComponent,
                Quantity = 2m,
                UnitCode = "unit",
                FacilityStableId = SimulationAreaAccessCodes.PlayerOwner,
            },
            new SimulationIntegratedLotSeedRequest
            {
                LotStableId = "lot:coop.guest.components",
                ItemCode = SimulationIntegratedItemCodes.CoopFacilityComponent,
                Quantity = 2m,
                UnitCode = "unit",
                FacilityStableId = "player:sim.guest-1",
            },
        };
        value.FacilityBlueprints[0].BlueprintStableId =
            SimulationCoopConstructionCodes.FarmSmallStorageBlueprint;
        value.FacilityBlueprints[0].Materials[0].ItemCode =
            SimulationIntegratedItemCodes.CoopFacilityComponent;
        value.FacilityBlueprints[0].Materials[0].Quantity = 4m;
        return value;
    }

    private static SimulationCoopContributionPreviewRequest ContributionRequest(
        long revision, string player, string lot, long lotRevision, decimal quantity)
        => new()
        {
            ExpectedRevision = revision,
            PlayerStableId = player,
            ProjectStableId = SimulationCoopConstructionCodes.FarmSmallStorageProject,
            BlueprintStableId = SimulationCoopConstructionCodes.FarmSmallStorageBlueprint,
            BuildSiteH1StableId = SimulationCoopConstructionCodes.FarmSmallStorageBuildSite,
            SourceLotStableId = lot,
            ExpectedSourceLotRevision = lotRevision,
            RequestedQuantity = quantity,
        };

    private static SimulationCoopContributionConfirmRequest ConfirmContribution(
        SimulationCoopContributionPreviewRequest request, string commandId)
        => new()
        {
            CommandId = commandId,
            ExpectedRevision = request.ExpectedRevision,
            PlayerStableId = request.PlayerStableId,
            ProjectStableId = request.ProjectStableId,
            BlueprintStableId = request.BlueprintStableId,
            BuildSiteH1StableId = request.BuildSiteH1StableId,
            SourceLotStableId = request.SourceLotStableId,
            ExpectedSourceLotRevision = request.ExpectedSourceLotRevision,
            RequestedQuantity = request.RequestedQuantity,
        };

    private static SimulationCoopProtectedActionPreviewRequest ProtectedRequest(
        long revision) => new()
        {
            ExpectedRevision = revision,
            OwnerPlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
            ProjectStableId = SimulationCoopConstructionCodes.FarmSmallStorageProject,
        };

    private static SimulationCoopProtectedActionConfirmRequest ProtectedConfirm(
        long revision, string commandId) => new()
        {
            CommandId = commandId,
            ExpectedRevision = revision,
            OwnerPlayerStableId = SimulationAreaAccessCodes.PlayerOwner,
            ProjectStableId = SimulationCoopConstructionCodes.FarmSmallStorageProject,
        };

    private static void Advance(경영SimulationSessionAggregate aggregate,
        string commandId)
        => aggregate.Advance(new 경영SimulationTick진행Request
        {
            CommandId = commandId,
            ExpectedRevision = aggregate.Revision,
            TickCount = 1,
        });

    private static string[] Sources()
        => new[] { "source:scenario-farm-choice-playable-r1" };
}
