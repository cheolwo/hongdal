using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSaveReplay,
        SsalddelCodeLayer.Domain,
        "세션 Snapshot과 Command log를 봉인한 저장 자료로 만든다.",
        StepKey = "domain.save-package",
        DependsOnStepKeys = new string[] { "application.save-replay" },
        ExecutionStage = SsalddelCodeExecutionStage.Persistence,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 40,
        Boundary = "재생 hash와 schema가 일치하는 Simulation 자료만 만들며 운영 원장을 직렬화하지 않는다.")]
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly List<SimulationCommandLogEntrySnapshot> commandLog =
            new List<SimulationCommandLogEntrySnapshot>();

        public SimulationSessionSavePackage CreateSavePackage(SimulationSessionSaveRequest request)
        {
            ValidateSaveRequest(request);
            lock (gate)
            {
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");

                var lhWorld = request.LhWorldState ?? lhWorldState;
                var package = new SimulationSessionSavePackage
                {
                    SchemaVersion = regionalIncidents.Count > 0
                        || appliedRegionalIncidentResponseCommands.Count > 0
                        ? SimulationSaveSchemaVersions.V4
                        : lhWorld != null
                        ? SimulationSaveSchemaVersions.V3
                        : spatialWorldCreationState == null
                            ? SimulationSaveSchemaVersions.V1
                            : SimulationSaveSchemaVersions.V2,
                    SaveStableId = request.SaveStableId.Trim(),
                    SessionStableId = SessionStableId,
                    SavedWorldTick = CurrentTick,
                    SavedWorldRevision = Revision,
                    ReplayHashAlgorithmCode = SimulationReplayHashAlgorithmCodes.Sha256,
                    SessionCreateRequest = CreateSessionRequest(),
                    Snapshot = CreateSnapshot(),
                    WorldInventory = CreateWorldInventorySnapshot(),
                    SurvivalTarot = CreateSurvivalTarotStateSnapshot(),
                    CommandLog = commandLog.Select(SimulationSaveReplayCloner.CloneCommand).ToArray(),
                    LhWorld = SimulationSaveReplayCloner.CloneLhWorld(lhWorld),
                };
                package.ReplayHash = SimulationReplayHasher.Calculate(package);
                return SimulationSaveReplayCloner.ClonePackage(package);
            }
        }

        private void AppendTickCommand(경영SimulationTick진행Request request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TickAdvance,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TickRequest = SimulationSaveReplayCloner.CloneTickRequest(request),
            });

        private void AppendDecisionConfirmCommand(SimulationDecisionConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.DecisionConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                DecisionConfirmRequest = SimulationSaveReplayCloner.CloneConfirmRequest(request),
            });

        private void AppendTaskCancelCommand(
            string taskStableId,
            SimulationTaskCancelRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TaskCancel,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TaskCancelRequest = SimulationSaveReplayCloner.CloneTaskCancelRequest(request),
                TaskStableId = taskStableId.Trim(),
            });

        private void AppendRegionalIncidentResponseConfirmCommand(
            string eventStableId,
            SimulationRegionalIncidentResponseConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.RegionalIncidentResponseConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                WorldEventStableId = eventStableId.Trim(),
                RegionalIncidentResponseConfirmRequest =
                    SimulationSaveReplayCloner.CloneRegionalIncidentResponseConfirmRequest(request),
            });

        private void AppendNatureEncounterVictoryCommand(string battleStableId,
            string encounterStableId)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.NatureEncounterVictory,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                NatureEncounterVictoryRequest = new SimulationNatureEncounterVictoryRequest
                {
                    BattleStableId = battleStableId.Trim(),
                    EncounterStableId = encounterStableId.Trim(),
                },
            });

        private void AppendHarvestDispositionImpactConfirmCommand(
            SimulationHarvestDispositionImpactConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.HarvestDispositionImpactConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                HarvestDispositionImpactConfirmRequest =
                    SimulationSaveReplayCloner.CloneHarvestDispositionImpactConfirmRequest(request),
            });

        private void AppendLogisticsMovementConfirmCommand(
            SimulationLogisticsMovementConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.LogisticsMovementConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                LogisticsMovementConfirmRequest =
                    SimulationSaveReplayCloner.CloneLogisticsMovementConfirmRequest(request),
            });

        private void AppendTurnClosingCommand(SimulationTurnClosingConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TurnClosingConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TurnClosingConfirmRequest =
                    SimulationSaveReplayCloner.CloneTurnClosingConfirmRequest(request),
            });

        private void AppendNpcPolicyCommand(SimulationNpcPolicyChangeRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.NpcPolicyChange,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                NpcPolicyChangeRequest = SimulationSaveReplayCloner.CloneNpcPolicyChangeRequest(request),
            });

        private void AppendWorldItemAcquisitionCommand(
            SimulationWorldItemAcquisitionConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.WorldItemAcquisitionConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                WorldItemAcquisitionConfirmRequest =
                    SimulationSaveReplayCloner.CloneWorldItemAcquisitionConfirmRequest(request),
            });

        private void AppendSurvivalTarotResponseCommand(
            SimulationSurvivalTarotResponseConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.SurvivalTarotResponseConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                SurvivalTarotResponseConfirmRequest =
                    SimulationSaveReplayCloner.CloneSurvivalTarotResponseConfirmRequest(request),
            });

        private void AppendSurvivalTarotResolutionCommand(
            SimulationSurvivalTarotResolutionConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.SurvivalTarotResolutionConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                SurvivalTarotResolutionConfirmRequest =
                    SimulationSaveReplayCloner.CloneSurvivalTarotResolutionConfirmRequest(request),
            });

        private void AppendFarmWorkConfirmCommand(
            SimulationFarmWorkConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.FarmWorkConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                FarmWorkConfirmRequest =
                    SimulationSaveReplayCloner.CloneFarmWorkConfirmRequest(request),
            });

        private void AppendFarmWorkPlanConfirmCommand(
            SimulationFarmWorkPlanConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.FarmWorkPlanConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                FarmWorkPlanConfirmRequest =
                    SimulationSaveReplayCloner.CloneFarmWorkPlanConfirmRequest(request),
            });

        private void AppendThreatResponseConfirmCommand(
            SimulationThreatResponseConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.ThreatResponseConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                ThreatResponseConfirmRequest =
                    SimulationSaveReplayCloner.CloneThreatResponseConfirmRequest(request),
            });

        private void AppendTeamRoleCardEquipCommand(
            SimulationTeamRoleCardEquipRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TeamRoleCardEquip,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TeamRoleCardEquipRequest =
                    SimulationSaveReplayCloner.CloneTeamRoleCardEquipRequest(request),
            });

        private void AppendTeamActivityStartCommand(
            SimulationTeamActivityStartRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TeamActivityStart,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TeamActivityStartRequest =
                    SimulationSaveReplayCloner.CloneTeamActivityStartRequest(request),
            });

        private void AppendTeamActivityEndCommand(
            SimulationTeamActivityEndRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TeamActivityEnd,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TeamActivityEndRequest =
                    SimulationSaveReplayCloner.CloneTeamActivityEndRequest(request),
            });

        private 경영SimulationSession생성Request CreateSessionRequest()
            => new 경영SimulationSession생성Request
            {
                ClientRequestId = ClientRequestId,
                ScenarioStableId = ScenarioStableId,
                ScenarioDataRevision = ScenarioDataRevision,
                ScenarioSeed = ScenarioSeed,
                RuleRevision = RuleRevision,
                DurationTicks = DurationTicks,
                WorldContext = new SimulationWorldContext생성Request
                {
                    FactionStableId = FactionStableId,
                    TerritoryStableId = TerritoryStableId,
                    SettlementStableId = SettlementStableId,
                    GameDateStartsOn = GameDateStartsOn,
                },
                Settlement = CloneSettlementRequest(settlementCreationState),
                NpcWorkforce = CloneNpcWorkforceInitialState(npcWorkforceCreationState),
                SpatialWorld = CloneSimulationSpatialInitialState(spatialWorldCreationState),
                WorldInventory = CloneWorldInventoryInitialState(worldInventoryCreationState),
                SurvivalTarot = CloneSurvivalTarotInitialState(survivalTarotCreationState),
                FarmSurvival = CloneFarmSurvivalInitialState(farmSurvivalCreationState),
                TeamRoleCards = CloneTeamRoleCardInitialStateOrNull(
                    teamRoleCardCreationState),
            };

        private static void ValidateSaveRequest(SimulationSessionSaveRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.SaveStableId, "SimulationSaveStableIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateLhWorldState(request.LhWorldState, request.ExpectedRevision);
        }
    }
}
