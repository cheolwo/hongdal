using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation플레이어분야SchemaCodes
    {
        public const string 분야Catalog = "simulation-player-domain-catalog.v1";
        public const string 분야ProfileV1 = "simulation-player-domain-profile.v1";
        public const string 분야Profile = "simulation-player-domain-profile.v2";
        public const string 분야학습효과선 = "simulation-domain-learning-effect-line.v1";
        public const string 분야진척기여 = "simulation-domain-progress-contribution.v1";
    }

    public static class Simulation플레이어분야Codes
    {
        public const string 자연생존 = "NatureSurvival";
        public const string 탐사공간 = "ExplorationAndSpace";
        public const string 채집자원 = "GatheringAndResources";
        public const string 전투사냥 = "CombatAndHunting";
        public const string 건설배치 = "ConstructionAndPlacement";
        public const string 설비에너지 = "FacilitiesAndEnergy";
        public const string 제작장비 = "CraftingAndEquipment";
        public const string 농업생산 = "AgricultureAndProduction";
        public const string 창고재고 = "WarehouseAndInventory";
        public const string 운송배송 = "TransportAndDelivery";
        public const string 시장생활서비스 = "MarketAndLifeService";
        public const string 운영조직 = "OperationsAndOrganization";
        public const string 지역발전복구 = "RegionalDevelopmentAndRecovery";
        public const string 교역수출 = "TradeAndExport";
        public const string 성찰근거해석 = "ReflectionAndEvidenceInterpretation";
        public const string 기존성찰 = "LegacyReflection";
    }

    public static class Simulation분야준비상태Codes
    {
        public const string Playable = "Playable";
        public const string RegisteredWI = "RegisteredWI";
        public const string ContractSeed = "ContractSeed";
        public const string AssetSeed = "AssetSeed";
        public const string Unavailable = "Unavailable";
    }

    public static class Simulation분야진척종류Codes
    {
        public const string 이해도 = "Understanding";
        public const string 현장숙련도 = "FieldProficiency";
        public const string 운영숙련도 = "OperationalProficiency";
    }

    public static class Simulation분야단계Codes
    {
        public const string 미접촉 = "NotEncountered";
        public const string 입문 = "Introduced";
        public const string 이해 = "Understood";
        public const string 연결이해 = "ConnectedUnderstanding";
        public const string 미경험 = "NoExperience";
        public const string 기초 = "Basic";
        public const string 익숙함 = "Familiar";
        public const string 숙련 = "Proficient";
    }

    public static class Simulation분야기여SourceCodes
    {
        public const string 승인자료성찰 = "ApprovedMaterialReflection";
        public const string 플레이어현장행동 = "PlayerFieldAction";
        public const string 플레이어운영위임 = "PlayerOperationalDelegation";
    }

    public static class Simulation분야행동결과Codes
    {
        public const string 성공 = "Success";
        public const string 의미있는실패 = "MeaningfulFailure";
        public const string 후퇴복구 = "RetreatOrRecovery";
        public const string 취소 = "Cancelled";
        public const string 차단 = "Blocked";
    }

    public static class Simulation분야기여방식Codes
    {
        public const string PlayerDirect = "PlayerDirect";
        public const string PlayerOrOperation = "PlayerOrOperation";
        public const string OperationOnly = "OperationOnly";
        public const string LearningOnly = "LearningOnly";
        public const string None = "None";
    }

    public sealed class Simulation분야단계기준Definition
    {
        public string 단계Code { get; set; } = string.Empty;
        public string 한국어명 { get; set; } = string.Empty;
        public int 최소진척 { get; set; }
    }

    public sealed class Simulation세부숙련Definition
    {
        public string StableId { get; set; } = string.Empty;
        public string 한국어명 { get; set; } = string.Empty;
    }

    public sealed class Simulation플레이어분야Definition
    {
        public string 분야StableId { get; set; } = string.Empty;
        public string 한국어명 { get; set; } = string.Empty;
        public string 준비상태Code { get; set; } = string.Empty;
        public Simulation세부숙련Definition[] 세부숙련들 { get; set; }
            = Array.Empty<Simulation세부숙련Definition>();
    }

    public sealed class Simulation분야숙련결속선Definition
    {
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public int 기여가중치Permille { get; set; } = 1_000;
    }

    public sealed class SimulationWI분야결속Definition
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string 기여방식Code { get; set; } = string.Empty;
        public Simulation분야숙련결속선Definition[] 결속선들 { get; set; }
            = Array.Empty<Simulation분야숙련결속선Definition>();
        public string NoPlayerProgressReason { get; set; } = string.Empty;
    }

    public sealed class Simulation플레이어분야CatalogSnapshot
    {
        public string SchemaCode { get; set; }
            = Simulation플레이어분야SchemaCodes.분야Catalog;
        public string CatalogRevision { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public Simulation플레이어분야Definition[] 분야들 { get; set; }
            = Array.Empty<Simulation플레이어분야Definition>();
        public SimulationWI분야결속Definition[] Wi결속들 { get; set; }
            = Array.Empty<SimulationWI분야결속Definition>();
        public Simulation분야단계기준Definition[] 이해도단계기준들 { get; set; }
            = Array.Empty<Simulation분야단계기준Definition>();
        public Simulation분야단계기준Definition[] 숙련도단계기준들 { get; set; }
            = Array.Empty<Simulation분야단계기준Definition>();
    }

    public sealed class Simulation분야이해효과선Snapshot
    {
        public string SchemaCode { get; set; }
            = Simulation플레이어분야SchemaCodes.분야학습효과선;
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public int 이해도증가량 { get; set; } = 1;
        public string[] 해금후보Codes { get; set; } = Array.Empty<string>();
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation분야진척Snapshot
    {
        public string 분야StableId { get; set; } = string.Empty;
        public int 이해도 { get; set; }
        public int 현장숙련도 { get; set; }
        public int 운영숙련도 { get; set; }
        public string 이해도단계Code { get; set; }
            = Simulation분야단계Codes.미접촉;
        public string 현장숙련도단계Code { get; set; }
            = Simulation분야단계Codes.미경험;
        public string 운영숙련도단계Code { get; set; }
            = Simulation분야단계Codes.미경험;
        public Simulation세부숙련진척Snapshot[] 세부숙련진척들 { get; set; }
            = Array.Empty<Simulation세부숙련진척Snapshot>();
        public string[] 활성해금Codes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation세부숙련진척Snapshot
    {
        public string 세부숙련StableId { get; set; } = string.Empty;
        public int 이해도 { get; set; }
        public int 현장숙련도 { get; set; }
        public int 운영숙련도 { get; set; }
    }

    public sealed class Simulation분야진척기여Snapshot
    {
        public string SchemaCode { get; set; }
            = Simulation플레이어분야SchemaCodes.분야진척기여;
        public string ContributionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string SourceCode { get; set; } = string.Empty;
        public string 분야StableId { get; set; } = string.Empty;
        public string 세부숙련StableId { get; set; } = string.Empty;
        public string PublicationStableId { get; set; } = string.Empty;
        public string PublicationRevision { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string OriginCommandId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public string EffectBatchStableId { get; set; } = string.Empty;
        public string EffectReceiptStableId { get; set; } = string.Empty;
        public string 결과Code { get; set; } = string.Empty;
        public int 이해도증가량 { get; set; }
        public int 현장숙련도증가량 { get; set; }
        public int 운영숙련도증가량 { get; set; }
        public long AppliedWorldRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation플레이어분야ProfileSnapshot
    {
        public string SchemaCode { get; set; }
            = Simulation플레이어분야SchemaCodes.분야Profile;
        public string PlayerStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CatalogRevision { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public Simulation분야진척Snapshot[] 분야진척들 { get; set; }
            = Array.Empty<Simulation분야진척Snapshot>();
        public Simulation분야진척기여Snapshot[] 기여기록들 { get; set; }
            = Array.Empty<Simulation분야진척기여Snapshot>();
        public long 명상경험Milli { get; set; }
        public int 명상숙련도 { get; set; }
        public string 명상숙련도단계Code { get; set; }
            = Simulation분야단계Codes.미경험;
        public Simulation명상분야기여요약Snapshot[] 명상분야기여요약들
            { get; set; } = Array.Empty<Simulation명상분야기여요약Snapshot>();
        public Simulation명상숙련기여Snapshot[] 명상기여기록들 { get; set; }
            = Array.Empty<Simulation명상숙련기여Snapshot>();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation분야학습기여Request
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string PublicationStableId { get; set; } = string.Empty;
        public string PublicationRevision { get; set; } = string.Empty;
        public string PublicationHashSha256 { get; set; } = string.Empty;
        public long AppliedWorldRevision { get; set; }
        public Simulation행위발현Record 적용행위기록 { get; set; }
            = new Simulation행위발현Record();
        public Simulation분야이해효과선Snapshot[] 효과선들 { get; set; }
            = Array.Empty<Simulation분야이해효과선Snapshot>();
    }

    public sealed class Simulation현장숙련기여Request
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public Simulation행위발현Record 행위기록 { get; set; }
            = new Simulation행위발현Record();
    }

    public sealed class Simulation운영숙련기여Request
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public Simulation행위발현Record 위임행위기록 { get; set; }
            = new Simulation행위발현Record();
        public Simulation행위발현Record Npc완료행위기록 { get; set; }
            = new Simulation행위발현Record();
        public Simulation행위발현Record 검토행위기록 { get; set; }
            = new Simulation행위발현Record();
    }

    public sealed class Simulation플레이어분야PerspectiveWorldState
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public string InterpretationRevision { get; set; } = string.Empty;
        public string ProfileRevision { get; set; } = string.Empty;
        public Simulation분야진척Snapshot[] 강조분야들 { get; set; }
            = Array.Empty<Simulation분야진척Snapshot>();
        public string[] 전체자료접근Codes { get; set; } = Array.Empty<string>();
        public string[] 선택형기회후보Codes { get; set; } = Array.Empty<string>();
    }
}
