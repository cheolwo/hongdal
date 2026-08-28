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
        private readonly string authorityLocationCode;

        public SimulationNatureSurvivalService(I경영SimulationSessionStore store,
            I세계상호작용실행Pipeline? worldInteractionPipeline = null,
            string authorityLocationCode = "RemoteHost")
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            worldInteractions = worldInteractionPipeline
                ?? new 세계상호작용실행Pipeline();
            this.authorityLocationCode = string.IsNullOrWhiteSpace(
                authorityLocationCode) ? "Unknown" : authorityLocationCode.Trim();
        }

        public SimulationNatureSurvivalStateSnapshot Get(string sessionStableId)
            => Find(sessionStableId).GetNatureSurvivalState();

        public Simulation플레이어기회Snapshot[] GetPlayerOpportunities(
            string sessionStableId)
            => Find(sessionStableId).GetNaturePlayerOpportunities();

        public Simulation영역수요Snapshot[] GetAreaNeeds(string sessionStableId)
            => Find(sessionStableId).GetNatureAreaNeeds();

        public Simulation영역건물발전Snapshot GetBuildingProgression(
            string sessionStableId, string areaCode)
            => Find(sessionStableId).GetAreaBuildingProgression(areaCode);

        public SimulationNatureSurvivalActionPreviewSnapshot Preview(
            string sessionStableId,
            SimulationNatureSurvivalActionPreviewRequest request)
            => Find(sessionStableId).PreviewNatureSurvivalAction(request);

        public 경영SimulationSessionSnapshot Confirm(
            string sessionStableId,
            SimulationNatureSurvivalCommandRequest request)
        {
            if (request.AuthoritativeRewardBonusQuantity != 0)
                throw new SimulationContractException(
                    "SimulationNatureCombatRewardBonusServerOnly");
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
            var startsLinkedCombat = request.ActionCode ==
                    SimulationNatureSurvivalCodes.ResolveEncounter
                && request.ChoiceCode == SimulationNatureSurvivalCodes.Fight
                && SimulationNatureSurvivalCodes.IsR2(
                    aggregate.GetNatureSurvivalState().ProfileRevision);
            var immediateResult = !startsLinkedCombat && request.ActionCode is
                SimulationNatureSurvivalCodes.AcquireAxe
                or SimulationNatureSurvivalCodes.PlaceCabinBlueprint
                or SimulationNatureSurvivalCodes.EnterCabin
                or SimulationNatureSurvivalCodes.LeaveCabin
                or SimulationNatureSurvivalCodes.ResolveEncounter
                or SimulationNatureSurvivalCodes.CancelActiveWork
                or SimulationNatureSurvivalCodes.StoreAtCabin
                or SimulationNatureSurvivalCodes.SleepInCabin
                or SimulationNatureSurvivalCodes.SelectExpansionPlan
                or SimulationNatureSurvivalCodes.CollectDroppedTimber;
            var successor = request.ActionCode switch
            {
                SimulationNatureSurvivalCodes.AcquireAxe =>
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginHarvest when
                    SimulationNatureSurvivalCodes.IsR5(
                        aggregate.GetNatureSurvivalState().ProfileRevision) =>
                    SimulationNatureSurvivalCodes
                        .CollectDroppedTimberWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginHarvest =>
                    SimulationNatureSurvivalCodes
                        .PlaceCabinBlueprintWorldInteractionId,
                SimulationNatureSurvivalCodes.CollectDroppedTimber =>
                    SimulationNatureSurvivalCodes
                        .PlaceCabinBlueprintWorldInteractionId,
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
                SimulationNatureSurvivalCodes.StoreAtCabin =>
                    SimulationNatureSurvivalCodes.ResolveEncounterWorldInteractionId,
                SimulationNatureSurvivalCodes.SleepInCabin =>
                    SimulationNatureSurvivalCodes.SelectExpansionPlanWorldInteractionId,
                SimulationNatureSurvivalCodes.SelectExpansionPlan =>
                    "Day2Ready",
                SimulationNatureSurvivalCodes.BeginBuildingConstruction =>
                    Simulation영역건물발전Codes.ConstructionWorldInteractionId,
                SimulationNatureSurvivalCodes.PrepareFieldSupply =>
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                _ => "WI-NATURE-04",
            };
            var context = new 세계상호작용실행Context
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
                    PlayableLoopStableId = SimulationNatureSurvivalCodes
                        .PlayableLoopStableIdForAction(request.ActionCode),
                    AuthorityLocationCode = authorityLocationCode,
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
                    PrimaryOutcomeCode = immediateResult
                        ? request.ActionCode + ":Confirmed"
                        : request.ActionCode + ":TaskStarted",
                    결과분류Code = request.ActionCode ==
                                   SimulationNatureSurvivalCodes.CancelActiveWork
                        ? Simulation행위결과분류Codes.취소
                        : request.ActionCode ==
                          SimulationNatureSurvivalCodes.ResolveEncounter
                          && request.ChoiceCode ==
                          SimulationNatureSurvivalCodes.Retreat
                            ? Simulation행위결과분류Codes.후퇴복구
                            : Simulation행위결과분류Codes.성공,
                    변화의미Codes = ChangeSemantics(request.ActionCode),
                    SpatialRevision = aggregate.SpatialCompositionRuleRevision,
                };
            worldInteractions.RecordPreview(context, aggregate.Revision,
                preview.CanConfirm, preview.BlockReasonCodes);
            return worldInteractions.ExecutePlayerDriven(aggregate, context,
                () => aggregate.ConfirmNatureSurvivalAction(request));
        }

        public 경영SimulationSessionSnapshot AdvanceClock(
            string sessionStableId,
            SimulationNatureSurvivalClockAdvanceRequest request)
            => Find(sessionStableId).AdvanceNatureSurvivalClock(request);

        public Simulation집중판정ChallengeSnapshot SubmitFocusTiming(
            string sessionStableId,
            Simulation집중판정AttemptRequest request)
            => Find(sessionStableId).SubmitNatureFocusTiming(request);

        private 경영SimulationSessionAggregate Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdInvalid");
            return store.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
        }

        private static string[] ChangeSemantics(string actionCode)
            => actionCode switch
            {
                SimulationNatureSurvivalCodes.AcquireAxe => new[]
                {
                    Simulation행위변화의미Codes.Actor상태변경,
                    Simulation행위변화의미Codes.재고변경,
                },
                SimulationNatureSurvivalCodes.BeginHarvest or
                SimulationNatureSurvivalCodes.CollectDroppedTimber => new[]
                {
                    Simulation행위변화의미Codes.세계객체생성,
                    Simulation행위변화의미Codes.재고변경,
                    Simulation행위변화의미Codes.실외배치변경,
                },
                SimulationNatureSurvivalCodes.PlaceCabinBlueprint or
                SimulationNatureSurvivalCodes.BeginCabinBuild or
                SimulationNatureSurvivalCodes.BeginBuildingConstruction or
                SimulationNatureSurvivalCodes.PrepareFieldSupply => new[]
                {
                    Simulation행위변화의미Codes.세계객체생성,
                    Simulation행위변화의미Codes.실외배치변경,
                    Simulation행위변화의미Codes.통행변경,
                },
                SimulationNatureSurvivalCodes.StoreAtCabin => new[]
                {
                    Simulation행위변화의미Codes.재고변경,
                    Simulation행위변화의미Codes.실내설비변경,
                },
                SimulationNatureSurvivalCodes.SleepInCabin => new[]
                {
                    Simulation행위변화의미Codes.Actor상태변경,
                    Simulation행위변화의미Codes.시간상태변경,
                    Simulation행위변화의미Codes.대기변경,
                    Simulation행위변화의미Codes.실내설비변경,
                },
                SimulationNatureSurvivalCodes.EnterCabin or
                SimulationNatureSurvivalCodes.LeaveCabin => new[]
                {
                    Simulation행위변화의미Codes.Actor상태변경,
                    Simulation행위변화의미Codes.통행변경,
                    Simulation행위변화의미Codes.실내설비변경,
                },
                _ => new[]
                {
                    Simulation행위변화의미Codes.Actor상태변경,
                },
            };
    }
}
