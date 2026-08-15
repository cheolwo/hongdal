using Ssalddel.Unity.TeamObservation;

namespace Ssalddel.Unity.Tests;

public sealed class SimulationTeamObservationPresentationTests
{
    [Fact]
    public void 같은팀관찰결과는_카메라초점만옮기고_대상Command를허용하지않는다()
    {
        var state = new TeamObservationPresentationMapper().Map(
            Allowed(), "actor:sim:farmer-1");

        Assert.True(state.IsActive);
        Assert.Equal("actor:sim:explorer-1", state.CameraTargetActorStableId);
        Assert.Equal("actor:sim:farmer-1", state.LocalControlActorStableId);
        Assert.Equal("kr5186:l2:700:1145", state.TileFocusKey);
        Assert.False(state.AcceptsTargetCommands);
        Assert.False(state.MovesLocalActor);
        Assert.True(state.ExitOnLocalDanger);
        Assert.True(state.ShowObservedIndicator);
    }

    [Fact]
    public void 조작권한이나_개별동의가섞인응답은_Unity표현경계에서거부한다()
    {
        var control = Allowed();
        control.CanControlTarget = true;
        var consent = Allowed();
        consent.RequiresPerViewConsent = true;

        Assert.Throws<InvalidOperationException>(() =>
            new TeamObservationPresentationMapper().Map(
                control, "actor:sim:farmer-1"));
        Assert.Throws<InvalidOperationException>(() =>
            new TeamObservationPresentationMapper().Map(
                consent, "actor:sim:farmer-1"));
    }

    [Fact]
    public void 관찰권한Api경로는_Session식별자를인코딩한다()
    {
        Assert.Equal(
            "/api/simulation/v1/sessions/session%3Asim%3Ateam%201/team-observation/access/preview",
            SimulationTeamObservationApiRoutes.PreviewAccess(
                "session:sim:team 1"));
    }

    [Fact]
    public async Task Coordinator는_시작_Frame갱신_종료를거치며_낡은Pose를거부한다()
    {
        var authority = new FakeAuthority(Frame(12));
        var coordinator = new TeamObservationClientCoordinator(authority,
            new TeamObservationPresentationMapper());
        var request = StartRequest();

        var started = await coordinator.StartAsync(
            "session:sim:team-1", request);
        authority.Frame = Frame(13);
        var refreshed = await coordinator.RefreshAsync();

        Assert.Equal(12, started.PoseRevision);
        Assert.Equal(13, refreshed.PoseRevision);
        Assert.False(refreshed.Camera.AcceptsTargetCommands);
        Assert.False(refreshed.ContainsPrivateUi);

        authority.Frame = Frame(11);
        var stale = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            coordinator.RefreshAsync());
        Assert.Equal("TeamObservationPoseRevisionStale", stale.Message);

        await coordinator.EndAsync(Guid.Parse(
            "22222222-2222-2222-2222-222222222222"));
        Assert.Null(coordinator.Current);
        Assert.True(authority.EndCalled);
    }

    [Fact]
    public void Frame에_비공개Ui_재고_채팅이포함되면_표현을거부한다()
    {
        var frame = Frame(12);
        frame.ContainsInventory = true;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new TeamObservationPresentationMapper().MapFrame(
                frame, "actor:sim:farmer-1"));

        Assert.Equal("TeamObservationFrameBoundaryInvalid", error.Message);
    }

    private static TeamObservationAccessApiModel Allowed()
        => new()
        {
            SessionStableId = "session:sim:team-1",
            TeamStableId = "team:sim:survivors",
            ObserverActorStableId = "actor:sim:farmer-1",
            TargetActorStableId = "actor:sim:explorer-1",
            ViewModeCode = TeamObservationViewModeCodes.FirstPerson,
            TargetTileKey = "kr5186:l2:700:1145",
            TeamRevision = 3,
            Allowed = true,
            ReasonCode = "SameTeamObservationAllowed",
            RequiresPerViewConsent = false,
            CanControlTarget = false,
            ShowObserverIndicator = true,
            MoveObserverActor = false,
            ChangesWorldState = false,
            SimulationOnly = true,
            IsOperationalState = false,
            PresentationOnly = true,
        };

    private static TeamObservationSessionStartApiModel StartRequest()
        => new()
        {
            ClientRequestId = Guid.Parse(
                "11111111-1111-1111-1111-111111111111"),
            ObserverActorStableId = "actor:sim:farmer-1",
            TargetActorStableId = "actor:sim:explorer-1",
            RequestedViewModeCode = TeamObservationViewModeCodes.FirstPerson,
            ExpectedTeamRevision = 3,
            TargetTileKey = "kr5186:l2:700:1145",
        };

    private static TeamObservationFrameApiModel Frame(long poseRevision)
        => new()
        {
            Observation = new TeamObservationSessionApiModel
            {
                ObservationSessionStableId = "team-observation:1",
                SessionStableId = "session:sim:team-1",
                TeamStableId = "team:sim:survivors",
                ObserverActorStableId = "actor:sim:farmer-1",
                TargetActorStableId = "actor:sim:explorer-1",
                ViewModeCode = TeamObservationViewModeCodes.FirstPerson,
                StateCode = "Active",
                TeamRevision = 3,
                CanControlTarget = false,
                MoveObserverActor = false,
                ChangesWorldState = false,
                ShowObserverIndicator = true,
                SimulationOnly = true,
                IsOperationalState = false,
                PresentationOnly = true,
            },
            TargetPose = new TeamMemberPoseApiModel
            {
                SessionStableId = "session:sim:team-1",
                ActorStableId = "actor:sim:explorer-1",
                PoseRevision = poseRevision,
                CapturedAtUtc = DateTimeOffset.Parse("2026-08-15T01:00:00Z"),
                TileKey = "kr5186:l2:701:1145",
                LocalOffsetXMeters = 120d,
                LocalOffsetYMeters = -35d,
                ElevationMeters = 735d,
                CameraHeightMeters = 1.65d,
                YawDegrees = 42d,
                PitchDegrees = -6d,
                MovementIntentCode = "Walk",
                IsAvailable = true,
                SimulationOnly = true,
                IsOperationalState = false,
                PresentationOnly = true,
            },
            ContainsPrivateUi = false,
            ContainsInventory = false,
            ContainsChat = false,
            PresentationOnly = true,
        };

    private sealed class FakeAuthority : ITeamObservationAuthorityClient
    {
        public FakeAuthority(TeamObservationFrameApiModel frame)
            => Frame = frame;

        public TeamObservationFrameApiModel Frame { get; set; }
        public bool EndCalled { get; private set; }

        public Task<TeamObservationSessionApiModel> StartAsync(
            string sessionStableId,
            TeamObservationSessionStartApiModel request,
            CancellationToken cancellationToken)
            => Task.FromResult(Frame.Observation);

        public Task<TeamObservationFrameApiModel> LoadFrameAsync(
            string sessionStableId,
            string observationSessionStableId,
            CancellationToken cancellationToken)
            => Task.FromResult(Frame);

        public Task<TeamObservationSessionApiModel> EndAsync(
            string sessionStableId,
            string observationSessionStableId,
            TeamObservationSessionEndApiModel request,
            CancellationToken cancellationToken)
        {
            EndCalled = true;
            var ended = Frame.Observation;
            ended.StateCode = "Ended";
            return Task.FromResult(ended);
        }

        public Task<TeamObserverIndicatorApiModel> LoadObserversAsync(
            string sessionStableId,
            string targetActorStableId,
            CancellationToken cancellationToken)
            => Task.FromResult(new TeamObserverIndicatorApiModel
            {
                SessionStableId = sessionStableId,
                TargetActorStableId = targetActorStableId,
                ObserverActorStableIds = ["actor:sim:farmer-1"],
                ActiveObserverCount = 1,
                ShowIndicator = true,
                PresentationOnly = true,
            });
    }
}
