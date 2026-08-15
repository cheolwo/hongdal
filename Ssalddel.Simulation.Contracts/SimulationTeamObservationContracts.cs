using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationTeamObservationViewModeCodes
    {
        public const string FirstPerson = "FirstPerson";
        public const string Follow = "Follow";
    }

    public static class SimulationTeamObservationAccessReasonCodes
    {
        public const string SameTeam = "SameTeamObservationAllowed";
        public const string PolicyDisabled = "TeamObservationPolicyDisabled";
        public const string ObserverNotInTeam = "TeamObservationObserverNotInTeam";
        public const string TargetNotInTeam = "TeamObservationDifferentTeam";
        public const string SameActor = "TeamObservationSameActor";
        public const string ViewModeNotAllowed = "TeamObservationViewModeNotAllowed";
        public const string RevisionMismatch = "TeamObservationRevisionMismatch";
    }

    public static class SimulationTeamObservationSessionStateCodes
    {
        public const string Active = "Active";
        public const string Ended = "Ended";
    }

    public sealed class SimulationTeamObservationPolicySnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public bool MembersCanObserve { get; set; }
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public string[] AllowedViewModeCodes { get; set; } = Array.Empty<string>();
        public bool ShowObserverIndicator { get; set; } = true;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationTeamObservationAccessRequest
    {
        public string ObserverActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string RequestedViewModeCode { get; set; } = string.Empty;
        public long ExpectedTeamRevision { get; set; }
        public string TargetTileKey { get; set; } = string.Empty;
    }

    public sealed class SimulationTeamObservationAccessResponse
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

    public sealed class SimulationTeamObservationSessionStartRequest
    {
        public Guid ClientRequestId { get; set; }
        public string ObserverActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string RequestedViewModeCode { get; set; } = string.Empty;
        public long ExpectedTeamRevision { get; set; }
        public string TargetTileKey { get; set; } = string.Empty;
    }

    public sealed class SimulationTeamObservationSessionEndRequest
    {
        public Guid ClientRequestId { get; set; }
        public string ObserverActorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationTeamObservationSessionResponse
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

    public sealed class SimulationTeamMemberPoseSnapshot
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

    public sealed class SimulationTeamObservationFrameResponse
    {
        public SimulationTeamObservationSessionResponse Observation { get; set; }
            = new SimulationTeamObservationSessionResponse();
        public SimulationTeamMemberPoseSnapshot TargetPose { get; set; }
            = new SimulationTeamMemberPoseSnapshot();
        public bool ContainsPrivateUi { get; set; }
        public bool ContainsInventory { get; set; }
        public bool ContainsChat { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class SimulationTeamObserverIndicatorResponse
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string[] ObserverActorStableIds { get; set; } = Array.Empty<string>();
        public int ActiveObserverCount { get; set; }
        public bool ShowIndicator { get; set; }
        public bool PresentationOnly { get; set; }
    }
}
