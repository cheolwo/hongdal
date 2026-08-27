using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation행위기록SchemaCodes
    {
        public const string 발현기록 = "simulation-action-manifestation.v1";
        public const string 원장상태 = "simulation-action-manifestation-ledger.v1";
        public const string 체크포인트 = "simulation-action-manifestation-checkpoint.v1";
    }

    public static class Simulation행위결과분류Codes
    {
        public const string 성공 = "Success";
        public const string 의미있는실패 = "MeaningfulFailure";
        public const string 후퇴복구 = "RetreatOrRecovery";
        public const string 취소 = "Cancelled";
    }

    public static class Simulation행위변화의미Codes
    {
        public const string Actor상태변경 = "ActorStateChanged";
        public const string 세계객체생성 = "WorldObjectCreated";
        public const string 세계객체제거 = "WorldObjectRemoved";
        public const string 재고변경 = "InventoryChanged";
        public const string 지표변경 = "SurfaceChanged";
        public const string 통행변경 = "TraversalChanged";
        public const string 실외배치변경 = "ExteriorPlacementChanged";
        public const string 실내설비변경 = "InteriorFixtureChanged";
        public const string 대기변경 = "AtmosphereChanged";
        public const string 시간상태변경 = "TimeStateChanged";
        public const string 플레이어진척변경 = "PlayerProgressChanged";
    }

    public static class Simulation분야성장적용상태Codes
    {
        public const string Applied = "Applied";
        public const string Reused = "Reused";
        public const string NotApplicable = "NotApplicable";
    }

    public sealed class Simulation분야성장적용Snapshot
    {
        public string 상태Code { get; set; } = string.Empty;
        public string 사유Code { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long BeforeProfileRevision { get; set; }
        public long AfterProfileRevision { get; set; }
    }

    public sealed class Simulation세계상호작용권위증거Bundle
    {
        public SimulationWorldInteractionManifestationRecord 기존E5발현기록
            { get; set; } = new SimulationWorldInteractionManifestationRecord();
        public Simulation행위발현Record 행위발현기록 { get; set; }
            = new Simulation행위발현Record();
    }

    public sealed class Simulation세계상호작용실행Result<T>
    {
        public T AuthorityResult { get; set; } = default!;
        public Simulation행위발현Record 행위발현기록 { get; set; }
            = new Simulation행위발현Record();
        public Simulation분야성장적용Snapshot 분야성장적용 { get; set; }
            = new Simulation분야성장적용Snapshot();
        public bool Reused { get; set; }
    }

    public sealed class Simulation행위발현Record
    {
        public string SchemaCode { get; set; } = Simulation행위기록SchemaCodes.발현기록;
        public string 행위기록StableId { get; set; } = string.Empty;
        public string 이전기록HashSha256 { get; set; } = string.Empty;
        public string 기록HashSha256 { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayableLoopStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string CommandId { get; set; } = string.Empty;
        public string TriggerSourceCode { get; set; } = string.Empty;
        public string InitiatorStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ActorKindCode { get; set; } = string.Empty;
        public string[] TargetStableIds { get; set; } = Array.Empty<string>();
        public string OutcomeStableId { get; set; } = string.Empty;
        public string PrimaryOutcomeCode { get; set; } = string.Empty;
        public string 결과분류Code { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string BattleOutcomeStableId { get; set; } = string.Empty;
        public string EffectBatchStableId { get; set; } = string.Empty;
        public string[] EffectReceiptStableIds { get; set; } = Array.Empty<string>();
        public string[] 변화의미Codes { get; set; } = Array.Empty<string>();
        public string[] 영향공간StableIds { get; set; } = Array.Empty<string>();
        public string[] SourceReferenceIds { get; set; } = Array.Empty<string>();
        public long BeforeWorldRevision { get; set; }
        public long AfterWorldRevision { get; set; }
        public int AppliedWorldTick { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string SpatialRevision { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation행위기록Cursor
    {
        public long AfterWorldRevision { get; set; } = -1;
        public int AppliedWorldTick { get; set; } = -1;
        public string 행위기록StableId { get; set; } = string.Empty;
        public string 기록HashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation행위기록Query
    {
        public string WorldStableId { get; set; } = string.Empty;
        public Simulation행위기록Cursor Cursor { get; set; } = new Simulation행위기록Cursor();
        public long ThroughWorldRevision { get; set; } = long.MaxValue;
        public string[] WorldInteractionIds { get; set; } = Array.Empty<string>();
        public string[] 변화의미Codes { get; set; } = Array.Empty<string>();
        public string[] 공간StableIds { get; set; } = Array.Empty<string>();
        public int MaxCount { get; set; } = 256;
    }

    public sealed class Simulation행위기록Page
    {
        public Simulation행위발현Record[] Records { get; set; }
            = Array.Empty<Simulation행위발현Record>();
        public Simulation행위기록Cursor NextCursor { get; set; }
            = new Simulation행위기록Cursor();
        public bool RequiresCheckpointRebuild { get; set; }
        public long CheckpointWorldRevision { get; set; } = -1;
        public string CheckpointWorldStateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation행위기록CheckpointSnapshot
    {
        public string SchemaCode { get; set; } = Simulation행위기록SchemaCodes.체크포인트;
        public long ConsolidatedThroughWorldRevision { get; set; } = -1;
        public string WorldStateHashSha256 { get; set; } = string.Empty;
        public string LastConsolidatedRecordHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation행위기록LedgerSnapshot
    {
        public string SchemaCode { get; set; } = Simulation행위기록SchemaCodes.원장상태;
        public string WorldStableId { get; set; } = string.Empty;
        public Simulation행위기록CheckpointSnapshot Checkpoint { get; set; }
            = new Simulation행위기록CheckpointSnapshot();
        public Simulation행위발현Record[] TailRecords { get; set; }
            = Array.Empty<Simulation행위발현Record>();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public interface ISimulation행위기록Reader
    {
        Simulation행위기록Page Query(Simulation행위기록Query query);
    }
}
