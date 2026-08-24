using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.Perspectives;
using Ssalddel.Unity.Transport;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class RoleNpcTransportDataFlowTests
{
    [Fact]
    public async Task AuthorizedRoleQuery는_View를모르고_PresentationCoordinator가_별도로적용한다()
    {
        var snapshot = RoleSnapshot();
        var query = new AuthorizedRoleProjectionQuery(
            new 역할관점조회UseCase(new RoleRepository(snapshot)));
        var target = new RoleTarget("logistics-dock:inbound-a");
        var sink = new RoleSink();

        var authorized = await query.ExecuteAsync(new 역할관점조회Request
        {
            RequestedRoleCode = RolePerspectiveCodes.Transporter,
            WorldZoneCode = WorldZoneCodes.UrbanLogisticsCenter,
        });

        Assert.Null(target.Last);
        var model = new RolePresentationPerspectiveCoordinator(
            new RolePresentationPresenter(),
            new RolePresentationApplicator())
            .Apply(authorized, new[] { target }, sink);

        Assert.Equal(snapshot.StableId, model.AuthorizedSnapshotStableId);
        Assert.StartsWith("interpretation:", model.InterpretationRevision);
        Assert.StartsWith("presentation:", model.PresentationRevision);
        Assert.Equal("내 하차 Dock", target.Last!.LabelText);
        Assert.Single(sink.Interactions);
    }

    [Fact]
    public void Npc는_DataSnapshot에서_WorldState와PresentationModel을거쳐_ViewTarget에적용된다()
    {
        var snapshot = NpcSnapshot();
        var state = new NpcMovementInterpreter().Interpret(snapshot);
        var model = new NpcMovementPresenter().Present(state);
        var target = new NpcTarget(snapshot.NpcStableId);

        var unresolved = new NpcMovementApplicator().Apply(
            new[] { model },
            new INpcMovementPresentationTarget[] { target });

        Assert.Empty(unresolved);
        Assert.StartsWith("interpretation:", model.InterpretationRevision);
        Assert.StartsWith("presentation:", model.PresentationRevision);
        Assert.Equal("open-cargo-door", target.Last!.ArrivalAnimationCode);
        Assert.Equal("transport-task:71", target.Last.CanonicalTaskStableId);
    }

    [Fact]
    public void TransportApplicator는_CorridorPresenter출력만소비하고_낮은Revision을거부한다()
    {
        var corridor = new TransportCorridorProjector().Project(Handoff())!;
        var presenter = new TransportCorridorPresenter();
        var model = presenter.Present(corridor)!;
        var target = new TruckTarget(model.TruckStableId);
        var applicator = new TruckMovementApplicator();

        Assert.True(applicator.Apply(model, target));
        Assert.StartsWith("interpretation:", model.InterpretationRevision);
        Assert.StartsWith("presentation:", model.PresentationRevision);
        Assert.Contains("network.logistics-center", model.StatusLabelText);

        model.DataRevision--;
        Assert.False(applicator.Apply(model, target));
        Assert.Throws<InvalidOperationException>(() =>
            applicator.Apply(model, new TruckTarget("truck-projection:other")));
    }

    private static 역할관점Snapshot RoleSnapshot()
        => new RolePerspectiveMapper().Map(new RolePerspectiveApiModel
        {
            StableId = "role-perspective:transport-71",
            Revision = 7,
            AuthorizedRoleCode = RolePerspectiveCodes.Transporter,
            WorldZoneCode = WorldZoneCodes.UrbanLogisticsCenter,
            ViewerScopeCode = WorldViewerScopeCodes.AuthorizedParty,
            SourceTypeCode = RolePerspectiveSourceTypeCodes.OperationalProjection,
            AuthorizationDecisionId = "authorization:transport-71",
            GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
            ObjectEmphases = new[]
            {
                new RoleObjectEmphasisApiModel
                {
                    TargetStableId = "logistics-dock:inbound-a",
                    EmphasisCode = RoleObjectEmphasisCodes.Destination,
                    Label = "내 하차 Dock",
                    DetailPanelCode = "transport-dropoff-detail",
                },
            },
            AllowedInteractions = new[]
            {
                new RoleAllowedInteractionApiModel
                {
                    InteractionCode = "inspect-dropoff",
                    TargetStableId = "logistics-dock:inbound-a",
                    EffectCode = WorldInteractionEffectCodes.ReadOnly,
                },
            },
        });

    private static NpcMovementSnapshot NpcSnapshot()
        => new NpcMovementMapper().Map(new NpcMovementApiModel
        {
            StableId = "npc-movement:transport-driver-71",
            Revision = 5,
            NpcStableId = "npc:transport-driver.71",
            ActorRoleCode = "Transporter",
            WorldZoneCode = "transport-network",
            RouteCode = "transport-network-hub-delivery",
            CurrentWaypointKey = "network.logistics-center",
            DestinationWaypointKey = "network.warehouse",
            MovementStateCode = NpcMovementStateCodes.Moving,
            ArrivalActionCode = "open-cargo-door",
            SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection,
            CanonicalTaskStableId = "transport-task:71",
            GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
        });

    private static CargoWarehouseHandoffSnapshot Handoff()
        => new CargoWarehouseHandoffMapper(new NpcMovementMapper()).Map(
            new CargoWarehouseHandoffApiModel
            {
                StableId = "cargo-handoff:transport-71.inbound-91",
                Revision = 5,
                HandoffStateCode = CargoHandoffStateCodes.InTransit,
                CargoStableId = "cargo:transport-71",
                TransportTaskStableId = "transport-task:71",
                InboundTaskStableId = "inbound-task:91",
                GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                Movements = new[]
                {
                    new NpcMovementApiModel
                    {
                        StableId = "npc-movement:transport-driver-71",
                        Revision = 5,
                        NpcStableId = "npc:transport-driver.71",
                        ActorRoleCode = "Transporter",
                        WorldZoneCode = "transport-network",
                        RouteCode = "transport-network-hub-delivery",
                        CurrentWaypointKey = "network.logistics-center",
                        DestinationWaypointKey = "network.warehouse",
                        MovementStateCode = NpcMovementStateCodes.Moving,
                        ArrivalActionCode = "arrive",
                        SourceTypeCode = NpcMovementSourceTypeCodes.OperationalProjection,
                        CanonicalTaskStableId = "transport-task:71",
                        GeneratedAt = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                    },
                },
            });

    private sealed class RoleRepository(역할관점Snapshot snapshot) : I역할관점Repository
    {
        public Task<역할관점Snapshot> 조회Async(
            역할관점조회Request request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(snapshot);
    }

    private sealed class RoleTarget(string stableId) : IRolePresentationTarget
    {
        public string StableId { get; } = stableId;
        public RoleObjectPresentationModel? Last { get; private set; }
        public void ClearRolePresentation() => Last = null;
        public void ApplyRolePresentation(RoleObjectPresentationModel model) => Last = model;
    }

    private sealed class RoleSink : IRolePresentationInteractionSink
    {
        public IReadOnlyList<RoleInteractionPresentationModel> Interactions { get; private set; } =
            Array.Empty<RoleInteractionPresentationModel>();
        public void ReplaceAllowedInteractions(IReadOnlyList<RoleInteractionPresentationModel> interactions)
            => Interactions = interactions;
    }

    private sealed class NpcTarget(string npcStableId) : INpcMovementPresentationTarget
    {
        public string NpcStableId { get; } = npcStableId;
        public NpcMovementPresentationModel? Last { get; private set; }
        public void ApplyMovementPresentation(NpcMovementPresentationModel model) => Last = model;
    }

    private sealed class TruckTarget(string truckStableId) : ITruckMovementPresentationTarget
    {
        public string TruckStableId { get; } = truckStableId;
        public TruckMovementPresentationModel? Last { get; private set; }
        public void ApplyTruckMovementPresentation(TruckMovementPresentationModel model) => Last = model;
    }
}
