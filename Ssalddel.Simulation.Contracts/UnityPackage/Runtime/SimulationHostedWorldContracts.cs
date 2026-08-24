using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationHostedWorldCodes
    {
        public const string RuleRevision = "hosted-world-permissions.v2";
        public const string Solo = "Solo";
        public const string HostedMultiplayer = "HostedMultiplayer";
        public const string InviteOnly = "InviteOnly";
        public const string Active = "Active";
        public const string Invited = "Invited";
        public const string Allow = "Allow";
        public const string Deny = "Deny";
        public const string Direct = "Direct";
        public const string Confirm = "Confirm";
        public const string HostApproval = "HostApproval";
        public const string Observe = "Observe";
        public const string Interact = "Interact";
        public const string PerformWork = "PerformWork";
        public const string Build = "Build";
        public const string Demolish = "Demolish";
        public const string OpenHostedWorld = "OpenHostedWorld";
        public const string JoinHostedWorld = "JoinHostedWorld";
        public const string HostedGuestAction = "HostedGuestAction";
        public const string HostedSessionOpened = "HostedSessionOpened";
        public const string HostedGuestJoined = "HostedGuestJoined";
        public const string HostedGuestWorkCompleted = "HostedGuestWorkCompleted";
        public const string PolicySource = "source:hosted-world-permissions.v2";
    }

    public sealed class SimulationHostedWorldParticipantSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string ParticipantStateCode { get; set; } = string.Empty;
        public string CurrentAreaSetStableId { get; set; } = string.Empty;
        public long JoinedAtWorldRevision { get; set; }
    }

    public sealed class SimulationHostedWorldPermissionGrantSnapshot
    {
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string ScopeStableId { get; set; } = string.Empty;
        public string CapabilityCode { get; set; } = string.Empty;
        public string GrantStateCode { get; set; } = string.Empty;
        public string ActionRiskPolicyCode { get; set; } = string.Empty;
        public string GrantedByPlayerStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string GrantHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationHostedWorldAuditSnapshot
    {
        public string EffectStableId { get; set; } = string.Empty;
        public string EffectTypeCode { get; set; } = string.Empty;
        public string ChangedByPlayerStableId { get; set; } = string.Empty;
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string ScopeStableId { get; set; } = string.Empty;
        public string CapabilityCode { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
    }

    public sealed class SimulationHostedWorldStateSnapshot
    {
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public string HostedSessionStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string OwnerPlayerStableId { get; set; } = string.Empty;
        public string SessionModeCode { get; set; } = SimulationHostedWorldCodes.Solo;
        public string JoinPolicyCode { get; set; } = SimulationHostedWorldCodes.InviteOnly;
        public string DefaultGuestPermissionProfileCode { get; set; } = "FarmHelper";
        public SimulationHostedWorldParticipantSnapshot[] Participants { get; set; } = Array.Empty<SimulationHostedWorldParticipantSnapshot>();
        public SimulationHostedWorldPermissionGrantSnapshot[] PermissionGrants { get; set; } = Array.Empty<SimulationHostedWorldPermissionGrantSnapshot>();
        public SimulationHostedWorldAuditSnapshot[] AuditTrail { get; set; } = Array.Empty<SimulationHostedWorldAuditSnapshot>();
        public long PermissionRevision { get; set; }
        public long CreatedAtWorldRevision { get; set; }
        public bool HostLossBlocksMutation { get; set; } = true;
        public bool EscPausesWorld { get; set; }
        public string SessionHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public class SimulationHostedWorldOpenPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string OwnerPlayerStableId { get; set; } = string.Empty;
        public string InvitedGuestPlayerStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationHostedWorldOpenConfirmRequest : SimulationHostedWorldOpenPreviewRequest
    {
        public string CommandId { get; set; } = string.Empty;
    }

    public class SimulationHostedWorldJoinPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string GuestPlayerStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationHostedWorldJoinConfirmRequest : SimulationHostedWorldJoinPreviewRequest
    {
        public string CommandId { get; set; } = string.Empty;
    }

    public class SimulationHostedGuestActionPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string GuestPlayerStableId { get; set; } = string.Empty;
        public string ScopeStableId { get; set; } = string.Empty;
        public string CapabilityCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationHostedGuestActionConfirmRequest : SimulationHostedGuestActionPreviewRequest
    {
        public string CommandId { get; set; } = string.Empty;
    }

    public sealed class SimulationHostedWorldPreviewSnapshot
    {
        public long BaseRevision { get; set; }
        public string ActionCode { get; set; } = string.Empty;
        public string ActorPlayerStableId { get; set; } = string.Empty;
        public string TargetPlayerStableId { get; set; } = string.Empty;
        public string ScopeStableId { get; set; } = string.Empty;
        public string CapabilityCode { get; set; } = string.Empty;
        public string GrantStateCode { get; set; } = string.Empty;
        public string ActionRiskPolicyCode { get; set; } = string.Empty;
        public int DurationTicks { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public string PreviewHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }
}
