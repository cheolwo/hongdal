using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationOnlineWorldCodes
    {
        public const string RuleRevision = "simulation-online-world.v1";
        public const string CheckpointSchema = "simulation-online-world-checkpoint.v1";

        public const string SoloLocalWorld = "SoloLocalWorld";
        public const string PrivateHostedRoom = "PrivateHostedRoom";
        public const string OfficialPersistentWorld = "OfficialPersistentWorld";

        public const string PublicAuthenticated = "PublicAuthenticated";
        public const string InviteOnly = "InviteOnly";
        public const string Active = "Active";
        public const string Suspended = "Suspended";
        public const string Connected = "Connected";
        public const string Invited = "Invited";
        public const string Waiting = "Waiting";
        public const string Left = "Left";

        public const string Owner = "Owner";
        public const string TemporaryLeader = "TemporaryLeader";
        public const string Member = "Member";

        public const string NeedHelp = "NeedHelp";
        public const string GatherHere = "GatherHere";
        public const string ThreatFound = "ThreatFound";
        public const string ResourceFound = "ResourceFound";
        public const string ObjectiveReady = "ObjectiveReady";

        public const string NatureCooperationWorldStableId =
            "official-world:nature-cooperation.v1";
        public const string NatureCooperationObjectiveStableId =
            "objective:nature-common-survival-base.v1";
        public const string NaturePrivateAreaSetStableId =
            "area-set:sim:nature-private-room.v1";
        public static readonly string[] NatureOfficialAreaSetStableIds =
        {
            "area-set:sim:nature-official:north.v1",
            "area-set:sim:nature-official:east.v1",
            "area-set:sim:nature-official:south.v1",
            "area-set:sim:nature-official:west.v1",
        };

        public const int PrivateRoomMinimumPlayers = 2;
        public const int PrivateRoomMaximumPlayers = 4;
        public const int OfficialAreaSetCapacity = 32;
        public const int PartyMaximumPlayers = 4;

        public const string Joined = "Joined";
        public const string Queued = "Queued";
        public const string LeftWorld = "LeftWorld";
        public const string SignalRecorded = "SignalRecorded";
        public const string PartyCreated = "PartyCreated";
        public const string AreaSetTransferred = "AreaSetTransferred";
        public const string MeditationApplied = "MeditationApplied";
        public const string ObjectiveContributionApplied =
            "ObjectiveContributionApplied";
        public const string RemoteHost = "RemoteHost";
        public const string AuthoritySessionReserved =
            "AuthoritySessionReserved";
        public const string AuthoritySessionRuntimeReadySingleActor =
            "AuthoritySessionRuntimeReadySingleActor";
        public const string AuthoritySessionRuntimeReadyCooperativeLogging =
            "AuthoritySessionRuntimeReadyCooperativeLogging";
        public const string ParticipantActorRegistrationReserved =
            "ParticipantActorRegistrationReserved";
        public const string ParticipantActorRegistered =
            "ParticipantActorRegistered";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Contract,
        "공식 지속 세계와 비공개 협동방의 조회·변경 계약을 정의한다.",
        StepKey = "contract.online-world",
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 10,
        Boundary = "온라인 세계 계약은 Solo 저장이나 운영 상태를 가져오지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "온라인 세계 종류·revision·용량·공유 범위의 계약 경계를 정의한다.",
        Boundary = "계약 정의만으로 온라인 게임플레이 또는 E 단계가 완료되지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세션권위계약)]
    public sealed class SimulationOnlineWorldDirectorySnapshot
    {
        public string RuleRevision { get; set; }
            = SimulationOnlineWorldCodes.RuleRevision;
        public long DirectoryRevision { get; set; }
        public SimulationOnlineWorldStateSnapshot[] Worlds { get; set; }
            = Array.Empty<SimulationOnlineWorldStateSnapshot>();
        public string DirectoryHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationOnlineWorldStateSnapshot
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string WorldKindCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string JoinPolicyCode { get; set; } = string.Empty;
        public string OwnerPlayerStableId { get; set; } = string.Empty;
        public string TemporaryLeaderPlayerStableId { get; set; } = string.Empty;
        public bool AlwaysActive { get; set; }
        public int MaximumParticipants { get; set; }
        public long WorldRevision { get; set; }
        public SimulationOnlineAreaSetStateSnapshot[] AreaSets { get; set; }
            = Array.Empty<SimulationOnlineAreaSetStateSnapshot>();
        public SimulationOnlineParticipantSnapshot[] Participants { get; set; }
            = Array.Empty<SimulationOnlineParticipantSnapshot>();
        public SimulationOnlinePartySnapshot[] Parties { get; set; }
            = Array.Empty<SimulationOnlinePartySnapshot>();
        public SimulationCooperativeObjectiveSnapshot[] Objectives { get; set; }
            = Array.Empty<SimulationCooperativeObjectiveSnapshot>();
        public SimulationFixedSignalSnapshot[] RecentSignals { get; set; }
            = Array.Empty<SimulationFixedSignalSnapshot>();
        public string StateHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationOnlineAreaSetStateSnapshot
    {
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AuthoritySessionStableId { get; set; } = string.Empty;
        public string AuthorityLocationCode { get; set; } = string.Empty;
        public string SessionBindingStateCode { get; set; } = string.Empty;
        public long SessionBindingRevision { get; set; }
        public long PartitionRevision { get; set; }
        public int Capacity { get; set; }
        public int ConnectedParticipants { get; set; }
        public int WaitingParticipants { get; set; }
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationOnlineParticipantSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string ParticipantStateCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string PartyStableId { get; set; } = string.Empty;
        public string AuthorityRoleCode { get; set; } = string.Empty;
        public long JoinedAtWorldRevision { get; set; }
        public long LastChangedAtWorldRevision { get; set; }
    }

    public sealed class SimulationOnlinePartySnapshot
    {
        public string PartyStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string LeaderPlayerStableId { get; set; } = string.Empty;
        public string[] MemberPlayerStableIds { get; set; } = Array.Empty<string>();
        public long Revision { get; set; }
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFixedSignalSnapshot
    {
        public string SignalStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string PartyStableId { get; set; } = string.Empty;
        public string SenderPlayerStableId { get; set; } = string.Empty;
        public string SignalCode { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public long AppliedWorldRevision { get; set; }
    }

    public sealed class SimulationCooperativeObjectiveSnapshot
    {
        public string ObjectiveStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long RequiredContributionUnits { get; set; }
        public long CurrentContributionUnits { get; set; }
        public string[] ContributionStableIds { get; set; } = Array.Empty<string>();
        public long Revision { get; set; }
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationAccountMeditationSnapshot
    {
        public string AccountPlayerStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public long MeditationExperienceMilli { get; set; }
        public int MeditationProficiency { get; set; }
        public string MeditationStageCode { get; set; } = string.Empty;
        public SimulationAccountMeditationContributionSnapshot[] Contributions
            { get; set; } = Array.Empty<SimulationAccountMeditationContributionSnapshot>();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationAccountMeditationContributionSnapshot
    {
        public string ContributionStableId { get; set; } = string.Empty;
        public string AccountPlayerStableId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public long MeditationExperienceMilli { get; set; }
        public long SourceActionWorldRevision { get; set; }
        public long AppliedOnlineWorldRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationPrivateRoomCreateRequest
    {
        public Guid ClientRequestId { get; set; }
        public string CommandId { get; set; } = string.Empty;
        public string[] InvitedPlayerStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationOnlineWorldJoinRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public long ExpectedAreaSetRevision { get; set; }
    }

    public sealed class SimulationOnlineWorldLeaveRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
    }

    public sealed class SimulationOnlinePartyCreateRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string PartyStableId { get; set; } = string.Empty;
        public string[] MemberPlayerStableIds { get; set; } = Array.Empty<string>();
        public long ExpectedWorldRevision { get; set; }
    }

    public sealed class SimulationFixedSignalSendRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string PartyStableId { get; set; } = string.Empty;
        public string SignalCode { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
    }

    public sealed class SimulationOnlineAreaSetTransferRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string TargetAreaSetStableId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public long ExpectedSourceAreaSetRevision { get; set; }
        public long ExpectedTargetAreaSetRevision { get; set; }
    }

    public sealed class SimulationVerifiedMeditationContributionRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public long MeditationExperienceMilli { get; set; }
        public long SourceActionWorldRevision { get; set; }
        public long ExpectedOnlineWorldRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationOnlineMeditationSyncRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public long ExpectedOnlineWorldRevision { get; set; }
    }

    public sealed class SimulationOnlineLoggingBeginRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string TargetResourceStableId { get; set; } = string.Empty;
        public long ExpectedOnlineWorldRevision { get; set; }
        public long ExpectedSessionWorldRevision { get; set; }
    }

    public sealed class SimulationOnlineLoggingFocusRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string ChallengeStableId { get; set; } = string.Empty;
        public long ExpectedOnlineWorldRevision { get; set; }
        public long ExpectedSessionWorldRevision { get; set; }
        public long ExpectedChallengeRevision { get; set; }
        public int InputOffsetMillis { get; set; }
    }

    public sealed class SimulationOnlineLoggingCompleteRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedOnlineWorldRevision { get; set; }
        public long ExpectedSessionWorldRevision { get; set; }
    }

    public sealed class SimulationOnlineLoggingReconnectRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public long ExpectedOnlineWorldRevision { get; set; }
        public Simulation행위기록Cursor Cursor { get; set; }
            = new Simulation행위기록Cursor();
        public int MaxCount { get; set; } = 64;
    }

    public sealed class SimulationOnlineLoggingReconnectSnapshot
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AuthoritySessionStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public long SessionWorldRevision { get; set; }
        public SimulationNatureSurvivalStateSnapshot Nature { get; set; }
            = new SimulationNatureSurvivalStateSnapshot();
        public Simulation행위기록Page ActionRecords { get; set; }
            = new Simulation행위기록Page();
    }

    public sealed class SimulationOnlineLoggingResultSnapshot
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AuthoritySessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public long SessionWorldRevision { get; set; }
        public SimulationNatureSurvivalStateSnapshot Nature { get; set; }
            = new SimulationNatureSurvivalStateSnapshot();
        public Simulation행위발현Record? CompletedActionRecord { get; set; }
        public SimulationAccountMeditationSnapshot? AccountMeditation
            { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationOnlineAuthoritySessionProvisionRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public long ExpectedOnlineWorldRevision { get; set; }
    }

    public sealed class SimulationOnlineAuthoritySessionRuntimeSnapshot
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AuthoritySessionStableId { get; set; } = string.Empty;
        public string AuthorityLocationCode { get; set; } = string.Empty;
        public string RuntimeStateCode { get; set; } = string.Empty;
        public string PrimaryActorStableId { get; set; } = string.Empty;
        public SimulationOnlineParticipantActorBindingSnapshot[] ParticipantActors
            { get; set; } = Array.Empty<SimulationOnlineParticipantActorBindingSnapshot>();
        public bool SupportsMultipleActors { get; set; }
        public long SourceOnlineWorldRevision { get; set; }
        public long SourceAreaSetRevision { get; set; }
        public long SessionWorldRevision { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationOnlineParticipantActorBindingSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string AuthoritySessionStableId { get; set; } = string.Empty;
        public string RegistrationStateCode { get; set; } = string.Empty;
        public bool HasAuthorityInventory { get; set; }
        public bool CanExecuteNatureWorldInteraction { get; set; }
        public long SourceParticipantRevision { get; set; }
    }

    public sealed class SimulationVerifiedObjectiveContributionRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string ObjectiveStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public long ContributionUnits { get; set; }
        public long AppliedWorldRevision { get; set; }
    }

    public sealed class SimulationOnlineWorldMutationResult
    {
        public bool Applied { get; set; }
        public string ResultCode { get; set; } = string.Empty;
        public SimulationOnlineWorldStateSnapshot World { get; set; }
            = new SimulationOnlineWorldStateSnapshot();
        public SimulationAccountMeditationSnapshot? AccountMeditation { get; set; }
    }

    public sealed class SimulationOnlineCommandReceiptSnapshot
    {
        public string CommandId { get; set; } = string.Empty;
        public string ActorPlayerStableId { get; set; } = string.Empty;
        public string PayloadHashSha256 { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public long ResultingWorldRevision { get; set; }
    }

    public sealed class SimulationOnlineWorldCheckpointSnapshot
    {
        public string SchemaCode { get; set; }
            = SimulationOnlineWorldCodes.CheckpointSchema;
        public long DirectoryRevision { get; set; }
        public SimulationOnlineWorldStateSnapshot[] Worlds { get; set; }
            = Array.Empty<SimulationOnlineWorldStateSnapshot>();
        public SimulationAccountMeditationSnapshot[] AccountMeditations { get; set; }
            = Array.Empty<SimulationAccountMeditationSnapshot>();
        public SimulationOnlineCommandReceiptSnapshot[] CommandReceipts { get; set; }
            = Array.Empty<SimulationOnlineCommandReceiptSnapshot>();
        public string CheckpointHashSha256 { get; set; } = string.Empty;
    }
}
