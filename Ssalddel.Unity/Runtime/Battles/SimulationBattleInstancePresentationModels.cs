using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Unity.Battles
{
    public static class BattlePresentationCodes
    {
        public const string Deploying = "Deploying";
        public const string Active = "Active";
        public const string Completed = "Completed";
        public const string Reconciled = "Reconciled";
        public const string Commander = "Commander";
        public const string DelegatedSquad = "DelegatedSquad";
        public const string Spectator = "Spectator";
        public const string Management = "Management";
        public const string BattleControl = "BattleControl";
        public const string BattleObservation = "BattleObservation";
        public const string TacticalThirdPerson = "TacticalThirdPerson";
        public const string FirstPerson = "FirstPerson";
        public const string Follow = "Follow";
    }

    public sealed class BattleInstanceApiModel
    {
        public string BattleStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string PhaseCode { get; set; } = string.Empty;
        public long BattleRevision { get; set; }
        public int CombatTick { get; set; }
        public BattleParticipantApiModel[] Participants { get; set; }
            = Array.Empty<BattleParticipantApiModel>();
        public BattleSupportApiModel[] Supports { get; set; }
            = Array.Empty<BattleSupportApiModel>();
        public BattleOutcomeApiModel? Outcome { get; set; }
        public string ReplayHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class BattleParticipantApiModel
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string ParticipationRoleCode { get; set; } = string.Empty;
        public string DelegatedSquadStableId { get; set; } = string.Empty;
        public bool CanControlWorldState { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class BattleSupportApiModel
    {
        public string SupportStableId { get; set; } = string.Empty;
        public string SupportCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int ArrivesCombatTick { get; set; }
    }

    public sealed class BattleOutcomeApiModel
    {
        public string ResultCode { get; set; } = string.Empty;
        public int AlliedStrengthDelta { get; set; }
        public int RecoverableInjuryCount { get; set; }
        public decimal FacilityDamageUnits { get; set; }
        public decimal SupplyLossUnits { get; set; }
        public int SecurityDelta { get; set; }
        public int MoraleDelta { get; set; }
        public string ReconciliationStateCode { get; set; } = string.Empty;
        public int? AppliedWorldTick { get; set; }
    }

    public sealed class BattlePresentationState
    {
        public string BattleStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
        public string PhaseCode { get; set; } = string.Empty;
        public string LocalModeCode { get; set; } = string.Empty;
        public string DefaultViewModeCode { get; set; } = string.Empty;
        public bool ShowBattleRoot { get; set; }
        public bool ShowManagementRoot { get; set; }
        public bool CanControlBattle { get; set; }
        public bool CanSendManagementSupport { get; set; }
        public bool ShowBattleStatusCard { get; set; }
        public int ConfirmedSupportCount { get; set; }
        public BattleOutcomePresentationState? Outcome { get; set; }
        public bool ChangesWorldState { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class BattleOutcomePresentationState
    {
        public string ResultCode { get; set; } = string.Empty;
        public int RecoverableInjuryCount { get; set; }
        public decimal FacilityDamageUnits { get; set; }
        public decimal SupplyLossUnits { get; set; }
        public string ReconciliationStateCode { get; set; } = string.Empty;
        public bool AppliedToManagementWorld { get; set; }
    }

    /// <summary>
    /// 서버 전투 상태를 같은 SimulationWorldShell 안의 전투/경영 표현 선택으로만 바꾼다.
    /// 결과 수치와 World 상태는 계산하거나 변경하지 않는다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationParallelBattle,
        SsalddelCodeLayer.ViewModel,
        "서버 전투 Snapshot을 경영·전투 카메라와 정보판 표현 상태로 투영한다.",
        StepKey = "unity.battle-presentation",
        DependsOnStepKeys = new string[] { "infrastructure.battle-store" },
        ExecutionStage = SsalddelCodeExecutionStage.Presentation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 60,
        Boundary = "전투 결과·World 상태를 계산하거나 변경하지 않는 순수 Unity 표현 변환이다.")]
    public sealed class BattlePresentationMapper
    {
        public BattlePresentationState Map(BattleInstanceApiModel source,
            string localActorStableId)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (string.IsNullOrWhiteSpace(localActorStableId)
                || string.IsNullOrWhiteSpace(source.BattleStableId)
                || string.IsNullOrWhiteSpace(source.AreaStableId)
                || source.BattleRevision < 0 || source.CombatTick < 0
                || source.Participants == null || source.Supports == null
                || !source.SimulationOnly || source.IsOperationalState
                || source.ReplayHashSha256 == null || source.ReplayHashSha256.Length != 64)
                throw new InvalidOperationException("BattlePresentationBoundaryInvalid");

            var participant = source.Participants.FirstOrDefault(value =>
                value.ActorStableId == localActorStableId.Trim());
            var resolved = source.PhaseCode == BattlePresentationCodes.Completed
                || source.PhaseCode == BattlePresentationCodes.Reconciled;
            var controlsBattle = !resolved && participant != null
                && (participant.ParticipationRoleCode == BattlePresentationCodes.Commander
                    || participant.ParticipationRoleCode == BattlePresentationCodes.DelegatedSquad);
            var observes = !resolved && participant?.ParticipationRoleCode ==
                BattlePresentationCodes.Spectator;
            var mode = controlsBattle ? BattlePresentationCodes.BattleControl
                : observes ? BattlePresentationCodes.BattleObservation
                : BattlePresentationCodes.Management;
            var view = mode == BattlePresentationCodes.BattleControl
                ? source.PhaseCode == BattlePresentationCodes.Deploying
                    ? BattlePresentationCodes.TacticalThirdPerson
                    : BattlePresentationCodes.FirstPerson
                : mode == BattlePresentationCodes.BattleObservation
                    ? BattlePresentationCodes.Follow
                    : BattlePresentationCodes.TacticalThirdPerson;
            return new BattlePresentationState
            {
                BattleStableId = source.BattleStableId,
                AreaStableId = source.AreaStableId,
                PhaseCode = source.PhaseCode,
                LocalModeCode = mode,
                DefaultViewModeCode = view,
                ShowBattleRoot = controlsBattle || observes,
                ShowManagementRoot = !controlsBattle && !observes,
                CanControlBattle = controlsBattle,
                CanSendManagementSupport = mode == BattlePresentationCodes.Management
                    && !resolved,
                ShowBattleStatusCard = mode == BattlePresentationCodes.Management,
                ConfirmedSupportCount = source.Supports.Length,
                Outcome = source.Outcome == null ? null : new BattleOutcomePresentationState
                {
                    ResultCode = source.Outcome.ResultCode,
                    RecoverableInjuryCount = source.Outcome.RecoverableInjuryCount,
                    FacilityDamageUnits = source.Outcome.FacilityDamageUnits,
                    SupplyLossUnits = source.Outcome.SupplyLossUnits,
                    ReconciliationStateCode = source.Outcome.ReconciliationStateCode,
                    AppliedToManagementWorld = source.Outcome.AppliedWorldTick.HasValue,
                },
                ChangesWorldState = false,
                PresentationOnly = true,
            };
        }
    }

    public sealed class BattleSupportCommandDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string SupportCode { get; set; } = string.Empty;
        public string SourceResourceStableId { get; set; } = string.Empty;
    }

    public static class BattleSupportCommandFactory
    {
        public static BattleSupportCommandDraft Create(BattlePresentationState frame,
            string commandId, long expectedWorldRevision, long expectedBattleRevision,
            string actorStableId, string supportCode, string sourceResourceStableId)
        {
            if (frame == null || !frame.CanSendManagementSupport
                || frame.ChangesWorldState || !frame.PresentationOnly)
                throw new InvalidOperationException("BattleManagementSupportUnavailable");
            if (string.IsNullOrWhiteSpace(commandId) || expectedWorldRevision < 0
                || expectedBattleRevision < 0 || string.IsNullOrWhiteSpace(actorStableId)
                || string.IsNullOrWhiteSpace(supportCode)
                || string.IsNullOrWhiteSpace(sourceResourceStableId))
                throw new ArgumentException("BattleSupportCommandInvalid");
            return new BattleSupportCommandDraft
            {
                CommandId = commandId.Trim(),
                ExpectedWorldRevision = expectedWorldRevision,
                ExpectedBattleRevision = expectedBattleRevision,
                RequestingActorStableId = actorStableId.Trim(),
                SupportCode = supportCode.Trim(),
                SourceResourceStableId = sourceResourceStableId.Trim(),
            };
        }
    }
}
