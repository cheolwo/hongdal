using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ssalddel.Unity.TeamObservation
{
    public static class TeamObservationViewModeCodes
    {
        public const string FirstPerson = "FirstPerson";
        public const string Follow = "Follow";
    }

    public static class SimulationTeamObservationApiRoutes
    {
        private static string Base(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new ArgumentException("SessionStableIdRequired",
                    nameof(sessionStableId));
            return "/api/simulation/v1/sessions/"
                + Uri.EscapeDataString(sessionStableId.Trim())
                + "/team-observation";
        }

        public static string PreviewAccess(string sessionStableId)
            => Base(sessionStableId) + "/access/preview";

        public static string Start(string sessionStableId)
            => Base(sessionStableId) + "/sessions/start";

        public static string Frame(
            string sessionStableId,
            string observationSessionStableId)
            => Base(sessionStableId) + "/sessions/"
                + Uri.EscapeDataString(Require(observationSessionStableId,
                    nameof(observationSessionStableId))) + "/frame";

        public static string End(
            string sessionStableId,
            string observationSessionStableId)
            => Base(sessionStableId) + "/sessions/"
                + Uri.EscapeDataString(Require(observationSessionStableId,
                    nameof(observationSessionStableId))) + "/end";

        public static string Observers(
            string sessionStableId,
            string targetActorStableId)
            => Base(sessionStableId) + "/targets/"
                + Uri.EscapeDataString(Require(targetActorStableId,
                    nameof(targetActorStableId))) + "/observers";

        private static string Require(string value, string parameterName)
            => !string.IsNullOrWhiteSpace(value)
                ? value.Trim()
                : throw new ArgumentException("StableIdRequired", parameterName);
    }

    public sealed class TeamObservationAccessApiModel
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public string ObserverActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string ViewModeCode { get; set; } = string.Empty;
        public string TargetTileKey { get; set; } = string.Empty;
        public long TeamRevision { get; set; }
        public bool Allowed { get; set; }
        public string ReasonCode { get; set; } = string.Empty;
        public bool RequiresPerViewConsent { get; set; }
        public bool CanControlTarget { get; set; }
        public bool ShowObserverIndicator { get; set; }
        public bool MoveObserverActor { get; set; }
        public bool ChangesWorldState { get; set; }
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class TeamObservationSessionStartApiModel
    {
        public Guid ClientRequestId { get; set; }
        public string ObserverActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string RequestedViewModeCode { get; set; } = string.Empty;
        public long ExpectedTeamRevision { get; set; }
        public string TargetTileKey { get; set; } = string.Empty;
    }

    public sealed class TeamObservationSessionEndApiModel
    {
        public Guid ClientRequestId { get; set; }
        public string ObserverActorStableId { get; set; } = string.Empty;
    }

    public sealed class TeamObservationSessionApiModel
    {
        public string ObservationSessionStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public string ObserverActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string ViewModeCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long TeamRevision { get; set; }
        public DateTimeOffset StartedAtUtc { get; set; }
        public DateTimeOffset? EndedAtUtc { get; set; }
        public bool CanControlTarget { get; set; }
        public bool MoveObserverActor { get; set; }
        public bool ChangesWorldState { get; set; }
        public bool ShowObserverIndicator { get; set; }
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class TeamMemberPoseApiModel
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public long PoseRevision { get; set; }
        public DateTimeOffset CapturedAtUtc { get; set; }
        public string TileKey { get; set; } = string.Empty;
        public double LocalOffsetXMeters { get; set; }
        public double LocalOffsetYMeters { get; set; }
        public double ElevationMeters { get; set; }
        public double CameraHeightMeters { get; set; }
        public double YawDegrees { get; set; }
        public double PitchDegrees { get; set; }
        public string MovementIntentCode { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class TeamObservationFrameApiModel
    {
        public TeamObservationSessionApiModel Observation { get; set; }
            = new TeamObservationSessionApiModel();
        public TeamMemberPoseApiModel TargetPose { get; set; }
            = new TeamMemberPoseApiModel();
        public bool ContainsPrivateUi { get; set; }
        public bool ContainsInventory { get; set; }
        public bool ContainsChat { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class TeamObserverIndicatorApiModel
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string[] ObserverActorStableIds { get; set; } = Array.Empty<string>();
        public int ActiveObserverCount { get; set; }
        public bool ShowIndicator { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class TeamObservationPresentationState
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public string LocalControlActorStableId { get; set; } = string.Empty;
        public string CameraTargetActorStableId { get; set; } = string.Empty;
        public string ViewModeCode { get; set; } = string.Empty;
        public string TileFocusKey { get; set; } = string.Empty;
        public long TeamRevision { get; set; }
        public bool IsActive { get; set; }
        public bool AcceptsTargetCommands { get; set; }
        public bool MovesLocalActor { get; set; }
        public bool ShowObservedIndicator { get; set; }
        public bool ExitOnLocalDanger { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class TeamObservationFramePresentationState
    {
        public string ObservationSessionStableId { get; set; } = string.Empty;
        public TeamObservationPresentationState Camera { get; set; }
            = new TeamObservationPresentationState();
        public long PoseRevision { get; set; }
        public DateTimeOffset CapturedAtUtc { get; set; }
        public double LocalOffsetXMeters { get; set; }
        public double LocalOffsetYMeters { get; set; }
        public double ElevationMeters { get; set; }
        public double CameraHeightMeters { get; set; }
        public double YawDegrees { get; set; }
        public double PitchDegrees { get; set; }
        public string MovementIntentCode { get; set; } = string.Empty;
        public bool ContainsPrivateUi { get; set; }
        public bool ContainsInventory { get; set; }
        public bool ContainsChat { get; set; }
        public bool PresentationOnly { get; set; }
    }

    /// <summary>
    /// 서버가 허용한 같은 팀 관찰 결과만 카메라 상태로 바꾼다.
    /// 대상 캐릭터 Command를 만드는 기능은 의도적으로 제공하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class TeamObservationPresentationMapper
    {
        public TeamObservationPresentationState Map(
            TeamObservationAccessApiModel source,
            string localActorStableId)
        {
            if (source == null
                || string.IsNullOrWhiteSpace(localActorStableId)
                || !source.Allowed
                || source.RequiresPerViewConsent
                || source.CanControlTarget
                || source.MoveObserverActor
                || source.ChangesWorldState
                || !source.SimulationOnly
                || source.IsOperationalState
                || !source.PresentationOnly
                || !source.ShowObserverIndicator
                || source.TeamRevision < 0
                || !string.Equals(source.ObserverActorStableId,
                    localActorStableId.Trim(), StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(source.SessionStableId)
                || string.IsNullOrWhiteSpace(source.TeamStableId)
                || string.IsNullOrWhiteSpace(source.TargetActorStableId)
                || string.IsNullOrWhiteSpace(source.TargetTileKey)
                || !IsSupportedView(source.ViewModeCode))
                throw new InvalidOperationException(
                    "TeamObservationPresentationBoundaryInvalid");

            return new TeamObservationPresentationState
            {
                SessionStableId = source.SessionStableId.Trim(),
                TeamStableId = source.TeamStableId.Trim(),
                LocalControlActorStableId = source.ObserverActorStableId.Trim(),
                CameraTargetActorStableId = source.TargetActorStableId.Trim(),
                ViewModeCode = source.ViewModeCode.Trim(),
                TileFocusKey = source.TargetTileKey.Trim(),
                TeamRevision = source.TeamRevision,
                IsActive = true,
                AcceptsTargetCommands = false,
                MovesLocalActor = false,
                ShowObservedIndicator = true,
                ExitOnLocalDanger = true,
                PresentationOnly = true,
            };
        }

        public static bool IsSupportedView(string viewModeCode)
            => string.Equals(viewModeCode, TeamObservationViewModeCodes.FirstPerson,
                   StringComparison.Ordinal)
                || string.Equals(viewModeCode, TeamObservationViewModeCodes.Follow,
                    StringComparison.Ordinal);

        public TeamObservationFramePresentationState MapFrame(
            TeamObservationFrameApiModel source,
            string localActorStableId)
        {
            if (source == null || source.Observation == null
                || source.TargetPose == null
                || !source.PresentationOnly
                || source.ContainsPrivateUi
                || source.ContainsInventory
                || source.ContainsChat
                || source.Observation.StateCode != "Active"
                || source.Observation.CanControlTarget
                || source.Observation.MoveObserverActor
                || source.Observation.ChangesWorldState
                || !source.Observation.ShowObserverIndicator
                || !source.Observation.SimulationOnly
                || source.Observation.IsOperationalState
                || !source.Observation.PresentationOnly
                || !source.TargetPose.IsAvailable
                || !source.TargetPose.SimulationOnly
                || source.TargetPose.IsOperationalState
                || !source.TargetPose.PresentationOnly
                || source.TargetPose.PoseRevision < 0
                || source.TargetPose.CameraHeightMeters <= 0d
                || !string.Equals(source.Observation.SessionStableId,
                    source.TargetPose.SessionStableId, StringComparison.Ordinal)
                || !string.Equals(source.Observation.TargetActorStableId,
                    source.TargetPose.ActorStableId, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "TeamObservationFrameBoundaryInvalid");

            var camera = Map(new TeamObservationAccessApiModel
            {
                SessionStableId = source.Observation.SessionStableId,
                TeamStableId = source.Observation.TeamStableId,
                ObserverActorStableId = source.Observation.ObserverActorStableId,
                TargetActorStableId = source.Observation.TargetActorStableId,
                ViewModeCode = source.Observation.ViewModeCode,
                TargetTileKey = source.TargetPose.TileKey,
                TeamRevision = source.Observation.TeamRevision,
                Allowed = true,
                RequiresPerViewConsent = false,
                CanControlTarget = false,
                ShowObserverIndicator = true,
                MoveObserverActor = false,
                ChangesWorldState = false,
                SimulationOnly = true,
                IsOperationalState = false,
                PresentationOnly = true,
            }, localActorStableId);

            return new TeamObservationFramePresentationState
            {
                ObservationSessionStableId =
                    source.Observation.ObservationSessionStableId,
                Camera = camera,
                PoseRevision = source.TargetPose.PoseRevision,
                CapturedAtUtc = source.TargetPose.CapturedAtUtc,
                LocalOffsetXMeters = source.TargetPose.LocalOffsetXMeters,
                LocalOffsetYMeters = source.TargetPose.LocalOffsetYMeters,
                ElevationMeters = source.TargetPose.ElevationMeters,
                CameraHeightMeters = source.TargetPose.CameraHeightMeters,
                YawDegrees = source.TargetPose.YawDegrees,
                PitchDegrees = source.TargetPose.PitchDegrees,
                MovementIntentCode = source.TargetPose.MovementIntentCode,
                ContainsPrivateUi = false,
                ContainsInventory = false,
                ContainsChat = false,
                PresentationOnly = true,
            };
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface ITeamObservationAuthorityClient
    {
        Task<TeamObservationSessionApiModel> StartAsync(
            string sessionStableId,
            TeamObservationSessionStartApiModel request,
            CancellationToken cancellationToken);
        Task<TeamObservationFrameApiModel> LoadFrameAsync(
            string sessionStableId,
            string observationSessionStableId,
            CancellationToken cancellationToken);
        Task<TeamObservationSessionApiModel> EndAsync(
            string sessionStableId,
            string observationSessionStableId,
            TeamObservationSessionEndApiModel request,
            CancellationToken cancellationToken);
        Task<TeamObserverIndicatorApiModel> LoadObserversAsync(
            string sessionStableId,
            string targetActorStableId,
            CancellationToken cancellationToken);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class TeamObservationClientCoordinator
    {
        private readonly ITeamObservationAuthorityClient authority;
        private readonly TeamObservationPresentationMapper mapper;

        public TeamObservationClientCoordinator(
            ITeamObservationAuthorityClient authority,
            TeamObservationPresentationMapper mapper)
        {
            this.authority = authority
                ?? throw new ArgumentNullException(nameof(authority));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public TeamObservationFramePresentationState? Current { get; private set; }

        public async Task<TeamObservationFramePresentationState> StartAsync(
            string sessionStableId,
            TeamObservationSessionStartApiModel request,
            CancellationToken cancellationToken = default)
        {
            var started = await authority.StartAsync(sessionStableId,
                request, cancellationToken);
            if (started == null || started.StateCode != "Active"
                || started.CanControlTarget || started.MoveObserverActor
                || started.ChangesWorldState)
                throw new InvalidOperationException(
                    "TeamObservationSessionBoundaryInvalid");
            var frame = await authority.LoadFrameAsync(sessionStableId,
                started.ObservationSessionStableId, cancellationToken);
            Current = mapper.MapFrame(frame, request.ObserverActorStableId);
            return Current;
        }

        public async Task<TeamObservationFramePresentationState> RefreshAsync(
            CancellationToken cancellationToken = default)
        {
            if (Current == null)
                throw new InvalidOperationException("TeamObservationSessionMissing");
            var frame = await authority.LoadFrameAsync(
                Current.Camera.SessionStableId,
                Current.ObservationSessionStableId,
                cancellationToken);
            var next = mapper.MapFrame(frame,
                Current.Camera.LocalControlActorStableId);
            if (next.PoseRevision < Current.PoseRevision)
                throw new InvalidOperationException(
                    "TeamObservationPoseRevisionStale");
            Current = next;
            return next;
        }

        public async Task EndAsync(
            Guid clientRequestId,
            CancellationToken cancellationToken = default)
        {
            if (Current == null) return;
            await authority.EndAsync(Current.Camera.SessionStableId,
                Current.ObservationSessionStableId,
                new TeamObservationSessionEndApiModel
                {
                    ClientRequestId = clientRequestId,
                    ObserverActorStableId =
                        Current.Camera.LocalControlActorStableId,
                }, cancellationToken);
            Current = null;
        }

        public void ClearLocalPresentation()
            => Current = null;
    }
}
