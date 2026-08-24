using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// Nature 생존 세션 원장만 호출한다. 운영 DB나 외부 Provider를 호출하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.",
        SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행)]
    public sealed class SimulationNatureSurvivalService
    {
        private readonly I경영SimulationSessionStore store;
        private readonly I세계상호작용실행Pipeline worldInteractions;

        public SimulationNatureSurvivalService(I경영SimulationSessionStore store,
            I세계상호작용실행Pipeline? worldInteractionPipeline = null)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            worldInteractions = worldInteractionPipeline
                ?? new 세계상호작용실행Pipeline();
        }

        public SimulationNatureSurvivalStateSnapshot Get(string sessionStableId)
            => Find(sessionStableId).GetNatureSurvivalState();

        public SimulationNatureSurvivalActionPreviewSnapshot Preview(
            string sessionStableId,
            SimulationNatureSurvivalActionPreviewRequest request)
            => Find(sessionStableId).PreviewNatureSurvivalAction(request);

        public 경영SimulationSessionSnapshot Confirm(
            string sessionStableId,
            SimulationNatureSurvivalCommandRequest request)
        {
            var aggregate = Find(sessionStableId);
            var preview = aggregate.PreviewNatureSurvivalAction(
                new SimulationNatureSurvivalActionPreviewRequest
                {
                    ObservedWorldRevision = request.ExpectedRevision,
                    PlayerStableId = request.PlayerStableId,
                    ActionCode = request.ActionCode,
                    TargetStableId = request.TargetStableId,
                    ChoiceCode = request.ChoiceCode,
                    LocalX = request.LocalX,
                    LocalZ = request.LocalZ,
                    YawDegrees = request.YawDegrees,
                });
            var wiId = SimulationNatureSurvivalCodes.WorldInteractionIdForAction(
                request.ActionCode);
            var immediateResult = request.ActionCode is
                SimulationNatureSurvivalCodes.AcquireAxe
                or SimulationNatureSurvivalCodes.PlaceCabinBlueprint
                or SimulationNatureSurvivalCodes.EnterCabin
                or SimulationNatureSurvivalCodes.LeaveCabin
                or SimulationNatureSurvivalCodes.ResolveEncounter
                or SimulationNatureSurvivalCodes.CancelActiveWork;
            var successor = request.ActionCode switch
            {
                SimulationNatureSurvivalCodes.AcquireAxe =>
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginHarvest =>
                    SimulationNatureSurvivalCodes.PlaceCabinBlueprintWorldInteractionId,
                SimulationNatureSurvivalCodes.PlaceCabinBlueprint =>
                    SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginCabinBuild =>
                    SimulationNatureSurvivalCodes.EnterCabinWorldInteractionId,
                SimulationNatureSurvivalCodes.EnterCabin =>
                    SimulationNatureSurvivalCodes.LeaveCabinWorldInteractionId,
                SimulationNatureSurvivalCodes.LeaveCabin =>
                    SimulationNatureSurvivalCodes.EnterCabinWorldInteractionId,
                SimulationNatureSurvivalCodes.CancelActiveWork =>
                    "NatureSafeChoice",
                _ => "WI-NATURE-04",
            };
            return worldInteractions.ExecutePlayerDriven(aggregate,
                new 세계상호작용실행Context
                {
                    WorldInteractionId = wiId,
                    CommandId = request.CommandId,
                    InitiatorStableId = request.PlayerStableId,
                    ActorStableId = request.PlayerStableId,
                    TargetStableId = string.IsNullOrWhiteSpace(preview.TargetStableId)
                        ? request.ActionCode : preview.TargetStableId,
                    SourceReferenceIds = new[]
                    {
                        request.TargetStableId, request.ChoiceCode,
                    }.Concat(preview.SpatialEvidenceReferenceIds).ToArray(),
                    TimeReferenceId = "simulation-time:nature-realtime",
                    SpatialEvidenceStateCode =
                        preview.SpatialEvidenceStateCode,
                    SpatialEvidenceReferenceIds =
                        preview.SpatialEvidenceReferenceIds,
                    TaskOrEffectReferenceIds = new[]
                    {
                        immediateResult ? "effect:nature:" + request.CommandId
                            : "task:nature:" + request.CommandId,
                    },
                    ResultStateCodes = immediateResult
                        ? new[] { request.ActionCode + ":Confirmed" }
                        : Array.Empty<string>(),
                    SuccessorOrReturnCodes = new[] { successor },
                }, () => aggregate.ConfirmNatureSurvivalAction(request));
        }

        public 경영SimulationSessionSnapshot AdvanceClock(
            string sessionStableId,
            SimulationNatureSurvivalClockAdvanceRequest request)
            => Find(sessionStableId).AdvanceNatureSurvivalClock(request);

        private 경영SimulationSessionAggregate Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdInvalid");
            return store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
        }
    }
}
