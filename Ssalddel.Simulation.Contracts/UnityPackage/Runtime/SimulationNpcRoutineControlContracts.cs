using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldInteractionOriginCodes
    {
        public const string OperationsDerived = "OperationsDerived";
        public const string SimulationNative = "SimulationNative";
        public const string Hybrid = "Hybrid";

        public static string[] All { get; } =
        {
            OperationsDerived, SimulationNative, Hybrid,
        };
    }

    public static class SimulationWorldInteractionControlPolicyCodes
    {
        public const string NpcRoutine = "NpcRoutine";
        public const string PlayerOrNpc = "PlayerOrNpc";
        public const string PlayerDirect = "PlayerDirect";
        public const string WorldAutomatic = "WorldAutomatic";

        public static string[] All { get; } =
        {
            NpcRoutine, PlayerOrNpc, PlayerDirect, WorldAutomatic,
        };
    }

    public static class SimulationNpcRoutineControlRevisionCodes
    {
        /// <summary>기존 저장·클라이언트가 직접 Confirm하던 호환 경계다.</summary>
        public const string LegacyDirect = "";
        /// <summary>운영 파생 WI를 NPC 루틴으로 실행하고 플레이어는 정책·예외만 다룬다.</summary>
        public const string R1 = "npc-routine-control.r1";
        /// <summary>Hub 입고검수·적재·피킹·포장·출고 준비 전체를 NPC 루틴으로 닫는다.</summary>
        public const string R2 = "npc-routine-control.r2";
        /// <summary>Hub r2를 보존하고 Nature 현장 보급의 정책 위임을 추가한다.</summary>
        public const string R3 = "npc-routine-control.r3";

        public static bool IsKnown(string value)
            => string.IsNullOrWhiteSpace(value)
                || string.Equals(value, R1, StringComparison.Ordinal)
                || string.Equals(value, R2, StringComparison.Ordinal)
                || string.Equals(value, R3, StringComparison.Ordinal);
    }

    public static class SimulationNpcRoutinePlayerInterventionCodes
    {
        public const string ChangePolicy = "ChangePolicy";
        public const string CancelBeforeCompletion = "CancelBeforeCompletion";
        public const string ResolveFacilityCapacity = "ResolveFacilityCapacity";
    }

    /// <summary>
    /// NPC 루틴이 WI를 시작하거나 상위 상태 전이를 파생한 계보 기록이다.
    /// 클라이언트가 TriggerSource를 제출하는 Command 계약이 아니다.
    /// </summary>
    public sealed class SimulationNpcRoutineExecutionSnapshot
    {
        public string ExecutionStableId { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string ParentExecutionStableId { get; set; } = string.Empty;
        public string PolicyStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string InventoryStableId { get; set; } = string.Empty;
        public string OriginCode { get; set; } = string.Empty;
        public string ControlPolicyCode { get; set; } = string.Empty;
        public string TriggerSourceCode { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public SimulationWI음양주체분류Snapshot 음양주체분류 { get; set; }
            = new SimulationWI음양주체분류Snapshot();
        public int RecordedWorldTick { get; set; }
        public long Revision { get; set; }
    }

    /// <summary>
    /// 플레이어와 Unity가 NPC 루틴의 진행·차단 이유와 허용 개입만 읽는 관점별 조회 결과다.
    /// </summary>
    public sealed class SimulationNpcRoutineWorkProjection
    {
        public string ProjectionStableId { get; set; } = string.Empty;
        public string AreaCode { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string WorldInteractionName { get; set; } = string.Empty;
        public string WorldInteractionDisplayName { get; set; } = string.Empty;
        public string ResponsibilityKindCode { get; set; } = string.Empty;
        public string PrimaryOutcomeCode { get; set; } = string.Empty;
        public string SingleResponsibilityAssessmentCode { get; set; } = string.Empty;
        public string PolicyStableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string NpcActorStableId { get; set; } = string.Empty;
        public string TargetInventoryStableId { get; set; } = string.Empty;
        public string PhaseCode { get; set; } = string.Empty;
        public decimal ProgressRate { get; set; }
        public string OriginCode { get; set; } = string.Empty;
        public string ControlPolicyCode { get; set; } = string.Empty;
        public string TriggerSourceCode { get; set; } = string.Empty;
        public SimulationWI음양주체분류Snapshot 음양주체분류 { get; set; }
            = new SimulationWI음양주체분류Snapshot();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] AllowedPlayerInterventionCodes { get; set; } = Array.Empty<string>();
        public int WorldTick { get; set; }
        public long WorldRevision { get; set; }
    }
}
