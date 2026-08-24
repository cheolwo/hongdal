using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// H5/LH 생활세계 안의 소규모 교전과 독립 전장을 같은 전투 원장에서
    /// 구분하기 위한 안정 코드다.
    /// </summary>
    public static class SimulationLocalCombatCodes
    {
        public const string RuleRevision = "combat-encounter.world-local.r2";
        public const string ScalePolicyRevision = "combat-scale.nature-farm.r1";
        public const string WorldLocal = "WorldLocal";
        public const string DerivedBattlefield = "DerivedBattlefield";
        public const string Instant = "Instant";
        public const string Field = "Field";
        public const string Battlefield = "Battlefield";
        public const string Contact = "Contact";
        public const string Near = "Near";
        public const string Far = "Far";
        public const string RetreatBoundary = "RetreatBoundary";
        public const string BasicAttack = "BasicAttack";
        public const string Guard = "Guard";
        public const string Counter = "Counter";
        public const string Dodge = "Dodge";
        public const string Approach = "Approach";
        public const string Retreat = "Retreat";
        public const string RoleCardSkill = "RoleCardSkill";
        public const string HoldPosition = "HoldPosition";
        public const string DirectAction = "DirectAction";
        public const string TacticalCommand = "TacticalCommand";
        public const string Player = "Player";
        public const string Companion = "Companion";
        public const string Hostile = "Hostile";
        public const string Active = "Active";
        public const string Defeated = "Defeated";
        public const string Retreated = "Retreated";
        public const string EscalationWarning = "EscalationWarning";
        public const string Transitioning = "Transitioning";
        public const string LargeRaid = "LargeRaid";
        public const string StrongholdDefense = "StrongholdDefense";
        public const string WithdrawalOperation = "WithdrawalOperation";
        public const string BossOperation = "BossOperation";
        public const int InstantMaximumThreatUnits = 3;
        public const int LocalMaximumThreatUnits = 5;
        public const int LocalMaximumCompanionUnits = 3;
        public const int DefaultActionCooldownTicks = 5;
        public const int GuardWindowTicks = 4;
        public const int CounterWindowTicks = 2;
    }

    public sealed class SimulationCombatScaleDecisionSnapshot
    {
        public string RuleRevision { get; set; } = SimulationLocalCombatCodes.ScalePolicyRevision;
        public string EncounterScaleCode { get; set; } = string.Empty;
        public string CombatSpaceCode { get; set; } = string.Empty;
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationLocalCombatWorldContextSnapshot
    {
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string WorldLayoutHashSha256 { get; set; } = string.Empty;
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

    public sealed class SimulationLocalCombatActorSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public string ThreatTypeCode { get; set; } = string.Empty;
        public int HealthPermille { get; set; } = 1000;
        public int StaminaPermille { get; set; } = 1000;
        public string RangeBandCode { get; set; } = SimulationLocalCombatCodes.Near;
        public string FocusedTargetStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = SimulationLocalCombatCodes.Active;
        public int NextActionCombatTick { get; set; }
        public string[] RoleCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationLocalCombatActionSnapshot
    {
        public string ActionStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public int CombatTick { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public int HealthDeltaPermille { get; set; }
        public int StaminaDeltaPermille { get; set; }
        public int ReactionOffsetMs { get; set; }
        public string AppliedCardModifierHashSha256 { get; set; } = string.Empty;
        public string ControlModeCode { get; set; } = string.Empty;
        public string[] AppliedCardModifierCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationLocalCombatStateSnapshot
    {
        public string RuleRevision { get; set; } = SimulationLocalCombatCodes.RuleRevision;
        public int FrozenWorldTick { get; set; }
        public long FrozenWorldRevision { get; set; }
        public string StateCode { get; set; } = SimulationLocalCombatCodes.Active;
        public string FocusedTargetStableId { get; set; } = string.Empty;
        public string ControlModeCode { get; set; } = SimulationLocalCombatCodes.DirectAction;
        public string[] ActiveCardModifierCodes { get; set; } = Array.Empty<string>();
        public SimulationLocalCombatWorldContextSnapshot WorldContext { get; set; } = new();
        public SimulationLocalCombatActorSnapshot[] Actors { get; set; }
            = Array.Empty<SimulationLocalCombatActorSnapshot>();
        public SimulationLocalCombatActionSnapshot[] Actions { get; set; }
            = Array.Empty<SimulationLocalCombatActionSnapshot>();
        public bool EscalationRequired { get; set; }
        public string[] EscalationReasonCodes { get; set; } = Array.Empty<string>();
        public bool HostileTelegraphActive { get; set; }
        public int HostileTelegraphOpenedCombatTick { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationParallelBattle,
        SsalddelCodeLayer.Contract,
        "현재 H5/LH 공간에서 수행하는 전투 행동의 서버 입력을 정의한다.",
        StepKey = "contract.local-combat-action",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 11,
        Boundary = "클라이언트는 피해·명중·미터 좌표를 제출하지 않고 대상과 행동 의도만 보낸다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약 정의는 실행 효과나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class SimulationLocalCombatActionConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public int ReactionOffsetMs { get; set; }
    }

    public sealed class SimulationLocalCombatFocusConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string TargetActorStableId { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationParallelBattle,
        SsalddelCodeLayer.Contract,
        "현장 전투의 1인칭 직접 행동과 3인칭 전술 지휘 중 하나를 서버에 확정한다.",
        StepKey = "contract.local-combat-control-mode",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 10,
        Boundary = "카메라 자체가 아니라 서버가 확정한 전투 조작 방식만 행동 허용 범위를 바꾼다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약 정의는 실행 효과나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class SimulationLocalCombatControlModeConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string ControlModeCode { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleEscalationConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedBattleRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
        public string ExpectedBattleWorldContextHashSha256 { get; set; } = string.Empty;
        public string ExpectedBattlefieldDerivationInputHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleEscalationPreviewRequest
    {
        public long ExpectedBattleRevision { get; set; }
        public long ExpectedWorldRevision { get; set; }
        public string RequestingActorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationBattleEscalationPreviewSnapshot
    {
        public string BattleStableId { get; set; } = string.Empty;
        public long BattleRevision { get; set; }
        public long WorldRevision { get; set; }
        public SimulationCombatScaleDecisionSnapshot ScaleDecision { get; set; } = new();
        public SimulationBattlefieldDerivationSnapshot BattlefieldDerivation { get; set; } = new();
        public bool CanConfirm { get; set; }
        public string[] BlockingReasonCodes { get; set; } = Array.Empty<string>();
    }
}
