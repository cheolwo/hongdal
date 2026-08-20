using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationSaveSchemaVersions
    {
        public const string V1 = "simulation-save.v1";
        public const string V2 = "simulation-save.v2";
        public const string V3 = "simulation-save.v3";
        public const string V4 = "simulation-save.v4";
        public const string V5 = "simulation-save.v5";
        public const string V6 = "simulation-save.v6";
    }

    public static class SimulationReplayHashAlgorithmCodes
    {
        public const string Sha256 = "SHA-256";
    }

    public static class SimulationCommandTypeCodes
    {
        public const string DecisionConfirm = "DecisionConfirm";
        public const string HarvestDispositionImpactConfirm = "HarvestDispositionImpactConfirm";
        public const string LogisticsMovementConfirm = "LogisticsMovementConfirm";
        public const string TurnClosingConfirm = "TurnClosingConfirm";
        public const string NpcPolicyChange = "NpcPolicyChange";
        public const string WorldItemAcquisitionConfirm = "WorldItemAcquisitionConfirm";
        public const string SurvivalTarotResponseConfirm = "SurvivalTarotResponseConfirm";
        public const string SurvivalTarotResolutionConfirm = "SurvivalTarotResolutionConfirm";
        public const string FarmWorkConfirm = "FarmWorkConfirm";
        public const string FarmWorkPlanConfirm = "FarmWorkPlanConfirm";
        public const string ThreatResponseConfirm = "ThreatResponseConfirm";
        public const string CombatPerspectiveConfirm = "CombatPerspectiveConfirm";
        public const string CombatBeatStart = "CombatBeatStart";
        public const string CombatReactionConfirm = "CombatReactionConfirm";
        public const string TacticalOrderConfirm = "TacticalOrderConfirm";
        public const string TeamRoleCardEquip = "TeamRoleCardEquip";
        public const string CombatCardLoadoutSet = "CombatCardLoadoutSet";
        public const string TeamActivityStart = "TeamActivityStart";
        public const string TeamActivityEnd = "TeamActivityEnd";
        public const string TileTraversalConfirm = "TileTraversalConfirm";
        public const string CollectibleCardDraw = "CollectibleCardDraw";
        public const string CollectibleCardTransfer = "CollectibleCardTransfer";
        public const string TickAdvance = "TickAdvance";
        public const string TaskCancel = "TaskCancel";
        public const string RegionalIncidentResponseConfirm = "RegionalIncidentResponseConfirm";
        public const string NatureEncounterVictory = "NatureEncounterVictory";
        public const string IntegratedWorldConfirm = "IntegratedWorldConfirm";
        public const string IntegratedWorldEffectEnqueued = "IntegratedWorldEffectEnqueued";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSaveReplay,
        SsalddelCodeLayer.Contract,
        "세션 저장 식별자와 기대 개정을 정의한다.",
        StepKey = "contract.save-request",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 10,
        Boundary = "저장 자료는 Simulation 상태만 포함하며 운영 원장과 공공데이터 원본을 복제하지 않는다.")]
    public sealed class SimulationSessionSaveRequest
    {
        public string SaveStableId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationLhWorldStateSnapshot? LhWorldState { get; set; }
    }

    public sealed class SimulationSessionRestoreRequest
    {
        public string SaveStableId { get; set; } = string.Empty;
    }

    public sealed class SimulationCommandLogEntrySnapshot
    {
        public long Sequence { get; set; }
        public string CommandTypeCode { get; set; } = string.Empty;
        public int AppliedWorldTick { get; set; }
        public long ResultingWorldRevision { get; set; }
        public 경영SimulationTick진행Request? TickRequest { get; set; }
        public SimulationDecisionConfirmRequest? DecisionConfirmRequest { get; set; }
        public SimulationHarvestDispositionImpactConfirmRequest? HarvestDispositionImpactConfirmRequest { get; set; }
        public SimulationLogisticsMovementConfirmRequest? LogisticsMovementConfirmRequest { get; set; }
        public SimulationTurnClosingConfirmRequest? TurnClosingConfirmRequest { get; set; }
        public SimulationNpcPolicyChangeRequest? NpcPolicyChangeRequest { get; set; }
        public SimulationWorldItemAcquisitionConfirmRequest? WorldItemAcquisitionConfirmRequest
            { get; set; }
        public SimulationSurvivalTarotResponseConfirmRequest? SurvivalTarotResponseConfirmRequest
            { get; set; }
        public SimulationSurvivalTarotResolutionConfirmRequest? SurvivalTarotResolutionConfirmRequest
            { get; set; }
        public SimulationFarmWorkConfirmRequest? FarmWorkConfirmRequest { get; set; }
        public SimulationFarmWorkPlanConfirmRequest? FarmWorkPlanConfirmRequest { get; set; }
        public SimulationThreatResponseConfirmRequest? ThreatResponseConfirmRequest { get; set; }
        public SimulationCombatPerspectiveConfirmRequest? CombatPerspectiveConfirmRequest
            { get; set; }
        public SimulationCombatBeatStartRequest? CombatBeatStartRequest { get; set; }
        public SimulationCombatReactionConfirmRequest? CombatReactionConfirmRequest
            { get; set; }
        public SimulationTacticalOrderConfirmRequest? TacticalOrderConfirmRequest
            { get; set; }
        public SimulationTeamRoleCardEquipRequest? TeamRoleCardEquipRequest { get; set; }
        public SimulationCombatCardLoadoutSetRequest? CombatCardLoadoutSetRequest
            { get; set; }
        public SimulationTeamActivityStartRequest? TeamActivityStartRequest { get; set; }
        public SimulationTeamActivityEndRequest? TeamActivityEndRequest { get; set; }
        public SimulationTileTraversalConfirmRequest? TileTraversalConfirmRequest { get; set; }
        public SimulationCollectibleCardDrawRequest? CollectibleCardDrawRequest { get; set; }
        public SimulationCollectibleCardTransferRequest? CollectibleCardTransferRequest { get; set; }
        public SimulationTaskCancelRequest? TaskCancelRequest { get; set; }
        public string? TaskStableId { get; set; }
        public SimulationRegionalIncidentResponseConfirmRequest?
            RegionalIncidentResponseConfirmRequest { get; set; }
        public string? WorldEventStableId { get; set; }
        public SimulationNatureEncounterVictoryRequest? NatureEncounterVictoryRequest
            { get; set; }
        public SimulationIntegratedWorldCommandRequest? IntegratedWorldConfirmRequest
            { get; set; }
        public SimulationFacilityDamageQueueRequest? FacilityDamageQueueRequest { get; set; }
    }

    public sealed class SimulationFacilityDamageQueueRequest
    {
        public string BattleStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string SeverityCode { get; set; } = string.Empty;
    }

    public sealed class SimulationSessionSavePackage
    {
        public string SchemaVersion { get; set; } = SimulationSaveSchemaVersions.V2;
        public string SaveStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public int SavedWorldTick { get; set; }
        public long SavedWorldRevision { get; set; }
        public string ReplayHashAlgorithmCode { get; set; }
            = SimulationReplayHashAlgorithmCodes.Sha256;
        public string ReplayHash { get; set; } = string.Empty;
        public 경영SimulationSession생성Request SessionCreateRequest { get; set; }
            = new 경영SimulationSession생성Request();
        public 경영SimulationSessionSnapshot Snapshot { get; set; }
            = new 경영SimulationSessionSnapshot();
        public SimulationWorldInventorySnapshot WorldInventory { get; set; }
            = new SimulationWorldInventorySnapshot();
        public SimulationSurvivalTarotStateSnapshot SurvivalTarot { get; set; }
            = new SimulationSurvivalTarotStateSnapshot();
        public SimulationCommandLogEntrySnapshot[] CommandLog { get; set; }
            = Array.Empty<SimulationCommandLogEntrySnapshot>();
        public SimulationBattleSaveRecordSnapshot[] Battles { get; set; }
            = Array.Empty<SimulationBattleSaveRecordSnapshot>();
        public SimulationLhWorldStateSnapshot? LhWorld { get; set; }
    }

    public sealed class SimulationSessionRestoreResult
    {
        public string SaveStableId { get; set; } = string.Empty;
        public string SchemaVersion { get; set; } = string.Empty;
        public string ReplayHash { get; set; } = string.Empty;
        public int ReplayedCommandCount { get; set; }
        public int RestoredBattleCount { get; set; }
        public 경영SimulationSessionSnapshot Session { get; set; }
            = new 경영SimulationSessionSnapshot();
    }
}
