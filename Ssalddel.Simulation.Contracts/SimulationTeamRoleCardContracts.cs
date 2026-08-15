using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationTeamRoleCardCodes
    {
        public const string RuleRevision = "team-role-card-loadout.r1";
        public const string FarmWork = "FarmWork";
        public const string Exploration = "Exploration";
        public const string Logistics = "Logistics";
        public const string Primary = "Primary";
        public const string Support = "Support";
        public const string Idle = "Idle";
        public const string Active = "Active";
    }

    public sealed class SimulationTeamRoleCardInitialState
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public long TeamPolicyRevision { get; set; }
        public string RuleRevision { get; set; } = SimulationTeamRoleCardCodes.RuleRevision;
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public SimulationTeamRoleCardInitialCard[] Cards { get; set; }
            = Array.Empty<SimulationTeamRoleCardInitialCard>();
    }

    public sealed class SimulationTeamRoleCardInitialCard
    {
        public string CardCopyStableId { get; set; } = string.Empty;
        public string CardDefinitionStableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string[] ActivityRoleCodes { get; set; } = Array.Empty<string>();
        public string EquippedActorStableId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = SimulationTeamRoleCardCodes.Primary;
    }

    public sealed class SimulationTeamRoleCardEquipRequest
    {
        public Guid ClientRequestId { get; set; }
        public long ExpectedRevision { get; set; }
        public long ExpectedTeamPolicyRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
    }

    public sealed class SimulationTeamActivityStartRequest
    {
        public Guid ClientRequestId { get; set; }
        public long ExpectedRevision { get; set; }
        public long ExpectedTeamPolicyRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string ActivityRoleCode { get; set; } = string.Empty;
        public string ActivityStableId { get; set; } = string.Empty;
        public string LocationStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationTeamActivityEndRequest
    {
        public Guid ClientRequestId { get; set; }
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ActivityStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationTeamRoleCardSnapshot
    {
        public string CardCopyStableId { get; set; } = string.Empty;
        public string CardDefinitionStableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string[] ActivityRoleCodes { get; set; } = Array.Empty<string>();
        public string EquippedActorStableId { get; set; } = string.Empty;
        public string SlotCode { get; set; } = string.Empty;
        public string LockedActivityStableId { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
        public bool IsPhysicalItem { get; set; }
        public bool RequiresPhysicalProximityForEquip { get; set; }
    }

    public sealed class SimulationTeamActivityAssignmentSnapshot
    {
        public string ActivityStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string ActivityRoleCode { get; set; } = string.Empty;
        public string LocationStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
    }

    public sealed class SimulationTeamMemberRoleProjection
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string CurrentRoleCode { get; set; } = SimulationTeamRoleCardCodes.Idle;
        public string ActivityStableId { get; set; } = string.Empty;
        public string[] EquippedCardCopyStableIds { get; set; } = Array.Empty<string>();
        public bool IsPermanentProfession { get; set; }
    }

    public sealed class SimulationTeamRoleCardStateSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string TeamStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public long TeamPolicyRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public SimulationTeamRoleCardSnapshot[] Cards { get; set; }
            = Array.Empty<SimulationTeamRoleCardSnapshot>();
        public SimulationTeamActivityAssignmentSnapshot[] ActiveActivities { get; set; }
            = Array.Empty<SimulationTeamActivityAssignmentSnapshot>();
        public SimulationTeamMemberRoleProjection[] MemberRoles { get; set; }
            = Array.Empty<SimulationTeamMemberRoleProjection>();
        public bool SupportsRemoteEquip { get; set; }
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }
}
