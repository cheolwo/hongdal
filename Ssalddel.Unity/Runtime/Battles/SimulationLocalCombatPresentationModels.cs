using System;
using System.Linq;

namespace Ssalddel.Unity.Battles
{
    public static class LocalCombatPresentationCodes
    {
        public const string FirstPerson = "FirstPerson";
        public const string TacticalThirdPerson = "TacticalThirdPerson";
        public const string LeftPointer = "LeftPointer";
        public const string RightPointer = "RightPointer";
        public const string BasicAttack = "BasicAttack";
        public const string Guard = "Guard";
        public const string Counter = "Counter";
        public const string Dodge = "Dodge";
        public const string Approach = "Approach";
        public const string RoleCardSkill = "RoleCardSkill";
        public const string HoldPosition = "HoldPosition";
        public const string Retreat = "Retreat";
        public const string DirectAction = "DirectAction";
        public const string TacticalCommand = "TacticalCommand";
        public const string ObserverOperation = "ObserverOperation";
        public const string ObserverPaused = "ObserverPaused";
        public const string PauseObserverIntervention = "PauseObserverIntervention";
        public const string ActivateObserverCard = "ActivateObserverCard";
        public const string SkipObserverIntervention = "SkipObserverIntervention";
        public const string Active = "Active";
    }

    public sealed class LocalCombatWorldContextApiModel
    {
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string AreaSetInstanceStableId { get; set; } = string.Empty;
        public string H3Ref { get; set; } = string.Empty;
        public string H2Ref { get; set; } = string.Empty;
        public string H1Ref { get; set; } = string.Empty;
        public string FocusL3CellKey { get; set; } = string.Empty;
        public string RetreatConnectorStableId { get; set; } = string.Empty;
        public string ContextHashSha256 { get; set; } = string.Empty;
        public bool PinsDetailWindow { get; set; } = true;
        public bool PinsActiveWindow { get; set; } = true;
    }

    public sealed class LocalCombatActorApiModel
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public int HealthPermille { get; set; }
        public int StaminaPermille { get; set; }
        public string RangeBandCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int NextActionCombatTick { get; set; }
    }

    public sealed class LocalCombatStateApiModel
    {
        public string RuleRevision { get; set; } = string.Empty;
        public int FrozenWorldTick { get; set; }
        public long FrozenWorldRevision { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string FocusedTargetStableId { get; set; } = string.Empty;
        public string ControlModeCode { get; set; }
            = LocalCombatPresentationCodes.DirectAction;
        public string[] ActiveCardModifierCodes { get; set; } = Array.Empty<string>();
        public LocalCombatWorldContextApiModel WorldContext { get; set; } = new();
        public LocalCombatActorApiModel[] Actors { get; set; }
            = Array.Empty<LocalCombatActorApiModel>();
        public bool EscalationRequired { get; set; }
        public string[] EscalationReasonCodes { get; set; } = Array.Empty<string>();
        public bool HostileTelegraphActive { get; set; }
        public int HostileTelegraphOpenedCombatTick { get; set; }
        public bool ParticipationModeLocked { get; set; }
        public string FrozenCardLoadoutHashSha256 { get; set; } = string.Empty;
        public LocalCombatObserverOperationApiModel ObserverOperation { get; set; }
            = new();
        public LocalCombatPerformanceApiModel Performance { get; set; } = new();
    }

    public sealed class LocalCombatObserverOperationApiModel
    {
        public string PolicyRevision { get; set; } = string.Empty;
        public bool TacticalPauseActive { get; set; }
        public bool InterventionOpportunityConsumed { get; set; }
        public string ActivatedCardCopyStableId { get; set; } = string.Empty;
        public string ActivatedModifierCode { get; set; } = string.Empty;
        public int AutomaticActionCount { get; set; }
        public string[] AvailableEmergencyCardCopyStableIds { get; set; }
            = Array.Empty<string>();
    }

    public sealed class LocalCombatPerformanceApiModel
    {
        public string RuleRevision { get; set; } = string.Empty;
        public bool IsFinal { get; set; }
        public int FinalHealthPermille { get; set; }
        public int SuccessfulDefenseCount { get; set; }
        public int ElapsedCombatTicks { get; set; }
        public int HostileCount { get; set; }
        public int HealthScore { get; set; }
        public int DefenseScore { get; set; }
        public int SpeedScore { get; set; }
        public int TotalScore { get; set; }
        public string GradeCode { get; set; } = string.Empty;
        public int RewardBonusQuantity { get; set; }
    }

    public sealed class LocalCombatActionCommandDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public int ReactionOffsetMs { get; set; }
    }

    public sealed class LocalCombatControlModeCommandDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string ControlModeCode { get; set; } = string.Empty;
        public string ExpectedCardLoadoutHashSha256 { get; set; } = string.Empty;
    }

    public sealed class LocalCombatObserverInterventionCommandDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
    }

    public sealed class CombatScaleDecisionApiModel
    {
        public string RuleRevision { get; set; } = string.Empty;
        public string EncounterScaleCode { get; set; } = string.Empty;
        public string CombatSpaceCode { get; set; } = string.Empty;
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class BattleCreatePreviewCommandDraft
    {
        public long ExpectedWorldRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
        public string RequestingActorStableId { get; set; } = string.Empty;
    }


    /// <summary>
    /// 같은 포인터 입력을 현재 시점에 맞는 전투 의도로 번역한다.
    /// 1인칭 우클릭은 평소 마우스 시점을 보존하고 피격 예고 중에만 회피가 되며,
    /// 3인칭 우클릭은 평소 접근 이동, 피격 예고 중 회피가 된다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public static class LocalCombatInputCommandFactory
    {
        public static LocalCombatActionCommandDraft? CreatePointerAction(
            BattleInstanceApiModel battle, string perspectiveCode,
            string pointerCode, bool hostileTelegraphActive,
            string actorStableId, string targetActorStableId,
            string commandId, int reactionOffsetMs)
        {
            Validate(battle, perspectiveCode, actorStableId, commandId,
                reactionOffsetMs);
            if (pointerCode == LocalCombatPresentationCodes.LeftPointer)
                return Draft(battle, actorStableId, targetActorStableId, commandId,
                    LocalCombatPresentationCodes.BasicAttack, reactionOffsetMs);
            if (pointerCode != LocalCombatPresentationCodes.RightPointer)
                throw new ArgumentException("LocalCombatPointerCodeInvalid");
            if (hostileTelegraphActive
                && perspectiveCode == LocalCombatPresentationCodes.FirstPerson)
                return Draft(battle, actorStableId, targetActorStableId, commandId,
                    LocalCombatPresentationCodes.Dodge, reactionOffsetMs);
            if (perspectiveCode == LocalCombatPresentationCodes.FirstPerson)
                return null;
            if (hostileTelegraphActive)
                return Draft(battle, actorStableId, targetActorStableId, commandId,
                    LocalCombatPresentationCodes.HoldPosition, reactionOffsetMs);
            return Draft(battle, actorStableId, targetActorStableId, commandId,
                LocalCombatPresentationCodes.Approach, reactionOffsetMs);
        }

        public static LocalCombatActionCommandDraft? CreateActionSlot(
            BattleInstanceApiModel battle, string perspectiveCode, int slot,
            string actorStableId, string targetActorStableId,
            string commandId, int reactionOffsetMs)
        {
            Validate(battle, perspectiveCode, actorStableId, commandId,
                reactionOffsetMs);
            var tactical = perspectiveCode ==
                LocalCombatPresentationCodes.TacticalThirdPerson;
            var action = tactical ? slot switch
            {
                1 => LocalCombatPresentationCodes.BasicAttack,
                2 => LocalCombatPresentationCodes.HoldPosition,
                3 => LocalCombatPresentationCodes.Retreat,
                4 => null,
                _ => throw new ArgumentOutOfRangeException(nameof(slot),
                    "LocalCombatActionSlotInvalid"),
            } : slot switch
            {
                1 => LocalCombatPresentationCodes.BasicAttack,
                2 => battle.LocalCombat.HostileTelegraphActive
                    ? LocalCombatPresentationCodes.Counter
                    : LocalCombatPresentationCodes.Guard,
                3 => LocalCombatPresentationCodes.Dodge,
                4 => LocalCombatPresentationCodes.RoleCardSkill,
                _ => throw new ArgumentOutOfRangeException(nameof(slot),
                    "LocalCombatActionSlotInvalid"),
            };
            if (action == null) return null;
            return Draft(battle, actorStableId, targetActorStableId, commandId,
                action, reactionOffsetMs);
        }

        private static void Validate(BattleInstanceApiModel battle,
            string perspectiveCode, string actorStableId, string commandId,
            int reactionOffsetMs)
        {
            if (battle == null || battle.CombatSpaceCode !=
                    BattlePresentationCodes.WorldLocal
                || battle.LocalCombat == null
                || battle.LocalCombat.StateCode != LocalCombatPresentationCodes.Active
                || !battle.SimulationOnly || battle.IsOperationalState)
                throw new InvalidOperationException("LocalCombatInputUnavailable");
            if (perspectiveCode != LocalCombatPresentationCodes.FirstPerson
                && perspectiveCode != LocalCombatPresentationCodes.TacticalThirdPerson)
                throw new ArgumentException("LocalCombatPerspectiveInvalid");
            var requiredMode = perspectiveCode == LocalCombatPresentationCodes.FirstPerson
                ? LocalCombatPresentationCodes.DirectAction
                : LocalCombatPresentationCodes.TacticalCommand;
            if (battle.LocalCombat.ControlModeCode != requiredMode)
                throw new InvalidOperationException("LocalCombatControlModeMismatch");
            if (string.IsNullOrWhiteSpace(actorStableId)
                || string.IsNullOrWhiteSpace(commandId) || reactionOffsetMs < 0)
                throw new ArgumentException("LocalCombatCommandInvalid");
            if (!(battle.LocalCombat.Actors ?? Array.Empty<LocalCombatActorApiModel>())
                .Any(value => value.ActorStableId == actorStableId
                    && value.StateCode == LocalCombatPresentationCodes.Active))
                throw new InvalidOperationException("LocalCombatActorUnavailable");
        }

        private static LocalCombatActionCommandDraft Draft(
            BattleInstanceApiModel battle, string actorStableId, string targetActorStableId,
            string commandId, string actionCode, int reactionOffsetMs) => new()
            {
                CommandId = commandId.Trim(),
                ExpectedBattleRevision = battle.BattleRevision,
                RequestingActorStableId = actorStableId.Trim(),
                TargetActorStableId = targetActorStableId?.Trim() ?? string.Empty,
                ActionCode = actionCode,
                ReactionOffsetMs = reactionOffsetMs,
            };
    }
}
