using System;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// 전투 판정의 안정 식별자와 서버 조정값이다. Unity는 이 값을 표시하고
    /// 반응 행동과 시각만 제출하며 피해량과 판정 등급은 계산하지 않는다.
    /// </summary>
    public static class SimulationFarmCombatCodes
    {
        public const string RuleRevision = "farm-combat.single-beat.r1";

        public const string FirstPersonPrecision = "FirstPersonPrecision";
        public const string ThirdPersonAwareness = "ThirdPersonAwareness";

        public const string Guard = "Guard";
        public const string Counter = "Counter";
        public const string NoResponse = "NoResponse";

        public const string Active = "Active";
        public const string Resolved = "Resolved";

        public const string Perfect = "Perfect";
        public const string OnTime = "OnTime";
        public const string Early = "Early";
        public const string Late = "Late";
        public const string Expired = "Expired";

        public const string ZombieSwipe = "ZombieSwipe";
        public const string ZombieLunge = "ZombieLunge";

        public const int ImpactOffsetMs = 1000;
        public const int MaximumReactionOffsetMs = 1600;
        public const int FirstPersonGuardWindowMs = 320;
        public const int FirstPersonCounterWindowMs = 200;
        public const int ThirdPersonGuardWindowMs = 220;
        public const int ThirdPersonCounterWindowMs = 130;
        public const int PerfectGuardWindowMs = 70;
        public const int PerfectCounterWindowMs = 45;
    }

    public sealed class SimulationCombatPerspectiveConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string PerspectiveCode { get; set; } = string.Empty;
    }

    public sealed class SimulationCombatBeatStartRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationCombatReactionConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string BeatStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ReactionActionCode { get; set; } = string.Empty;
        public int ReactionOffsetMs { get; set; }
    }

    public sealed class SimulationFarmCombatStateSnapshot
    {
        public string RuleRevision { get; set; } = SimulationFarmCombatCodes.RuleRevision;
        public SimulationCombatPerspectiveSnapshot[] Perspectives { get; set; }
            = Array.Empty<SimulationCombatPerspectiveSnapshot>();
        public SimulationCombatBeatSnapshot[] Beats { get; set; }
            = Array.Empty<SimulationCombatBeatSnapshot>();
        public SimulationCombatReactionSnapshot[] Reactions { get; set; }
            = Array.Empty<SimulationCombatReactionSnapshot>();
        public SimulationFarmTacticalCombatStateSnapshot Tactical { get; set; }
            = new SimulationFarmTacticalCombatStateSnapshot();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationCombatPerspectiveSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string PerspectiveCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationCombatBeatSnapshot
    {
        public string BeatStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string AppliedPerspectiveCode { get; set; } = string.Empty;
        public string AttackPatternCode { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public int StartedWorldTick { get; set; }
        public int ImpactOffsetMs { get; set; }
        public int GuardWindowMs { get; set; }
        public int CounterWindowMs { get; set; }
        public int PerfectGuardWindowMs { get; set; }
        public int PerfectCounterWindowMs { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string ReactionStableId { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class SimulationCombatReactionSnapshot
    {
        public string ReactionStableId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string BeatStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ReactionActionCode { get; set; } = string.Empty;
        public int ReactionOffsetMs { get; set; }
        public int TimingDeltaMs { get; set; }
        public string GradeCode { get; set; } = string.Empty;
        public decimal ActorDamageUnits { get; set; }
        public int DefenseResponseScore { get; set; }
        public bool ThreatStaggered { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
    }
}
