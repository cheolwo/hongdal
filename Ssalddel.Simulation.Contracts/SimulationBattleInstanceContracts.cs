using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationBattleInstanceCodes
    {
        public const string RuleRevision = "battle-instance.parallel-management.r1";
        public const string Preparing = "Preparing";
        public const string Deploying = "Deploying";
        public const string Active = "Active";
        public const string Completed = "Completed";
        public const string Reconciled = "Reconciled";
        public const string Commander = "Commander";
        public const string DelegatedSquad = "DelegatedSquad";
        public const string Spectator = "Spectator";
        public const string SupplyCrate = "SupplyCrate";
        public const string ReinforcementSquad = "ReinforcementSquad";
        public const string Balanced = "Balanced";
        public const string Defensive = "Defensive";
        public const string Forward = "Forward";
        public const string Reserved = "Reserved";
        public const string Released = "Released";
        public const string InTransit = "InTransit";
        public const string Arrived = "Arrived";
        public const string Returned = "Returned";
        public const string Victory = "Victory";
        public const string Defeat = "Defeat";
        public const string Pending = "Pending";
        public const string Applied = "Applied";
        public const int CombatStepMilliseconds = 100;
        public const int MaximumCombatTick = 9000;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationParallelBattle,
        SsalddelCodeLayer.Contract,
        "병렬 전투 생성 미리보기의 서버 입력을 정의한다.",
        StepKey = "contract.battle-preview",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 10,
        Boundary = "클라이언트는 전투 결과나 수치 보정을 확정하지 않고 예상 World 개정과 안정 ID만 보낸다.")]
    public sealed class SimulationBattleCreatePreviewRequest
    {
        public long ExpectedWorldRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
        public string RequestingActorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleCreateConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string ExpectedBattleWorldContextHashSha256 { get; set; } = string.Empty;
        public string ExpectedBattlefieldDerivationInputHashSha256 { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string RequestingActorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleCreatePreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public int AlliedStrength { get; set; }
        public int HostileStrength { get; set; }
        public string[] InitialResourceStableIds { get; set; } = Array.Empty<string>();
        public string[] ReinforcementCandidateStableIds { get; set; } = Array.Empty<string>();
        public SimulationCombatScaleDecisionSnapshot ScaleDecision { get; set; } = new();
        public SimulationLocalCombatWorldContextSnapshot LocalWorldContext { get; set; } = new();
        public SimulationBattlefieldDerivationSnapshot BattlefieldDerivation { get; set; } = new();
        public SimulationBattleUnitRosterSnapshot UnitRoster { get; set; } = new();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationBattleParticipationConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public long ExpectedTeamPolicyRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string ParticipationRoleCode { get; set; } = string.Empty;
        public string DelegatedSquadStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleDeploymentPreviewRequest
    {
        public long ExpectedBattleRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string DeploymentCode { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleDeploymentConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string DeploymentCode { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleDeploymentPreviewSnapshot
    {
        public string BattleStableId { get; set; } = string.Empty;
        public long BattleRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string DeploymentCode { get; set; } = string.Empty;
        public int ProjectedPreparednessBonus { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
    }

    public sealed class SimulationBattleSupportPreviewRequest
    {
        public long ExpectedWorldRevision { get; set; }
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string SupportCode { get; set; } = string.Empty;
        public string SourceResourceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleSupportConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string SupportCode { get; set; } = string.Empty;
        public string SourceResourceStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleSupportPreviewSnapshot
    {
        public string BattleStableId { get; set; } = string.Empty;
        public long BattleRevision { get; set; }
        public long WorldRevision { get; set; }
        public string SupportCode { get; set; } = string.Empty;
        public string SourceResourceStableId { get; set; } = string.Empty;
        public int ArrivesCombatTick { get; set; }
        public int ProjectedStrengthBonus { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
        public bool SimulationOnly { get; set; } = true;
    }

    public sealed class SimulationBattleAdvanceRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public int CombatTickCount { get; set; } = 1;
    }

    public sealed class SimulationBattleInstanceSnapshot
    {
        public string BattleStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = SimulationBattleInstanceCodes.RuleRevision;
        public string CombatSpaceCode { get; set; } = SimulationLocalCombatCodes.DerivedBattlefield;
        public string EncounterScaleCode { get; set; } = SimulationLocalCombatCodes.Battlefield;
        public string ScalePolicyRevision { get; set; } = SimulationLocalCombatCodes.ScalePolicyRevision;
        public string[] ScaleReasonCodes { get; set; } = Array.Empty<string>();
        public string PhaseCode { get; set; } = string.Empty;
        public long BattleRevision { get; set; }
        public int CombatTick { get; set; }
        public int StartedWorldTick { get; set; }
        public long StartedWorldRevision { get; set; }
        public int AlliedStrength { get; set; }
        public int HostileStrength { get; set; }
        public int ScenarioSeed { get; set; }
        public string DeploymentCode { get; set; } = string.Empty;
        public SimulationBattleParticipantSnapshot[] Participants { get; set; }
            = Array.Empty<SimulationBattleParticipantSnapshot>();
        public SimulationBattleResourceReservationSnapshot[] ResourceReservations { get; set; }
            = Array.Empty<SimulationBattleResourceReservationSnapshot>();
        public SimulationBattleSupportSnapshot[] Supports { get; set; }
            = Array.Empty<SimulationBattleSupportSnapshot>();
        public SimulationBattlefieldDerivationSnapshot BattlefieldDerivation { get; set; } = new();
        public SimulationBattleUnitRosterSnapshot UnitRoster { get; set; } = new();
        public SimulationLocalCombatStateSnapshot LocalCombat { get; set; } = new();
        public SimulationBattleParticipationReservationSnapshot[] ParticipationReservations
            { get; set; } = Array.Empty<SimulationBattleParticipationReservationSnapshot>();
        public SimulationBattleWorldTargetReservationSnapshot[] WorldTargetReservations
            { get; set; } = Array.Empty<SimulationBattleWorldTargetReservationSnapshot>();
        public SimulationBattleSemanticEffectSnapshot[] SemanticEffects { get; set; }
            = Array.Empty<SimulationBattleSemanticEffectSnapshot>();
        public SimulationBattleOutcomeSnapshot? Outcome { get; set; }
        public string ReplayHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationBattleParticipantSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string ParticipationRoleCode { get; set; } = string.Empty;
        public string DelegatedSquadStableId { get; set; } = string.Empty;
        public bool CanControlWorldState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class SimulationBattleResourceReservationSnapshot
    {
        public string ResourceStableId { get; set; } = string.Empty;
        public string ReservationKindCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationBattleInstanceCodes.Reserved;
        public string SourceCommandId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleSupportSnapshot
    {
        public string SupportStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string SupportCode { get; set; } = string.Empty;
        public string SourceResourceStableId { get; set; } = string.Empty;
        public int ConfirmedWorldTick { get; set; }
        public long ConfirmedWorldRevision { get; set; }
        public int ArrivesCombatTick { get; set; }
        public int StrengthBonus { get; set; }
        public string StateCode { get; set; } = SimulationBattleInstanceCodes.InTransit;
    }

    public sealed class SimulationBattleOutcomeSnapshot
    {
        public string OutcomeStableId { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public int CompletedWorldTick { get; set; }
        public int AlliedStrengthDelta { get; set; }
        public int RecoverableInjuryCount { get; set; }
        public decimal FacilityDamageUnits { get; set; }
        public decimal SupplyLossUnits { get; set; }
        public int SecurityDelta { get; set; }
        public int MoraleDelta { get; set; }
        public bool UsedDeterministicAutoCommand { get; set; }
        public string ReconciliationStateCode { get; set; } = SimulationBattleInstanceCodes.Pending;
        public int? AppliedWorldTick { get; set; }
        public long? AppliedWorldRevision { get; set; }
    }

    public sealed class SimulationBattleCreationSnapshot
    {
        public string BattleStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string CommanderActorStableId { get; set; } = string.Empty;
        public int StartedWorldTick { get; set; }
        public long StartedWorldRevision { get; set; }
        public int ScenarioSeed { get; set; }
        public int AlliedStrength { get; set; }
        public int HostileStrength { get; set; }
        public string CombatSpaceCode { get; set; } = SimulationLocalCombatCodes.DerivedBattlefield;
        public string EncounterScaleCode { get; set; } = SimulationLocalCombatCodes.Battlefield;
        public string ScalePolicyRevision { get; set; } = SimulationLocalCombatCodes.ScalePolicyRevision;
        public string[] ScaleReasonCodes { get; set; } = Array.Empty<string>();
        public SimulationLocalCombatWorldContextSnapshot LocalWorldContext { get; set; } = new();
        public string[] InitialResourceStableIds { get; set; } = Array.Empty<string>();
        public string[] ReinforcementCandidateStableIds { get; set; } = Array.Empty<string>();
        public SimulationBattlefieldDerivationSnapshot BattlefieldDerivation { get; set; } = new();
        public SimulationBattleUnitRosterSnapshot UnitRoster { get; set; } = new();
        public string CreateCommandId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleAppliedCommandSnapshot
    {
        public string CommandId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public SimulationBattleInstanceSnapshot Result { get; set; }
            = new SimulationBattleInstanceSnapshot();
    }

    public sealed class SimulationBattleSaveRecordSnapshot
    {
        public SimulationBattleCreationSnapshot Creation { get; set; }
            = new SimulationBattleCreationSnapshot();
        public SimulationBattleInstanceSnapshot State { get; set; }
            = new SimulationBattleInstanceSnapshot();
        public string[] ReplayEvents { get; set; } = Array.Empty<string>();
        public SimulationBattleAppliedCommandSnapshot[] AppliedCommands { get; set; }
            = Array.Empty<SimulationBattleAppliedCommandSnapshot>();
        public string IntegrityHashSha256 { get; set; } = string.Empty;
    }
}
