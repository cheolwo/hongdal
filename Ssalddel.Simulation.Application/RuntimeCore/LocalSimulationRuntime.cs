using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// ASP.NET이나 HTTP 없이 공통 Application과 Session Aggregate를 실행하는 Solo 권위다.
    /// 세션 명령을 직렬화하여 실시간 진행, WorldTick과 저장이 서로 겹치지 않게 한다.
    /// </summary>
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Solo LocalProcess에서 공통 Simulation Core의 WI와 Session 명령을 실행한다.",
        Boundary = "Unity GameObject가 아니라 Local Runtime이 권위 상태를 변경한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2로컬권위Adapter)]
    public sealed class LocalSimulationRuntime : ISimulationRuntimeModules,
        ISimulationSessionRuntime, ISimulationNatureSurvivalRuntime,
        ISimulationSessionGameplayRuntime, ISimulationWorldInteractionRuntime,
        IDisposable
    {
        private readonly SemaphoreSlim commandGate = new SemaphoreSlim(1, 1);
        private readonly ISimulationSessionSaveStore saveStore;
        private readonly ISimulationLocalSaveSlotStore slotStore;
        private readonly I경영SimulationSessionStore sessionStore;
        private readonly 경영SimulationSession생명주기Service lifecycle;
        private readonly SimulationNatureSurvivalService nature;
        private readonly SimulationFarmSurvivalService farm;
        private readonly SimulationRegionalIncidentService regionalIncidents;

        public LocalSimulationRuntime(
            I경영SimulationSessionStore sessionStore,
            ISimulationSessionSaveStore sessionSaveStore,
            ISimulationLocalSaveSlotStore localSaveSlotStore,
            ISimulationBattleWorldReconciler? battleReconciler = null)
        {
            this.sessionStore = sessionStore
                ?? throw new ArgumentNullException(nameof(sessionStore));
            saveStore = sessionSaveStore
                ?? throw new ArgumentNullException(nameof(sessionSaveStore));
            slotStore = localSaveSlotStore
                ?? throw new ArgumentNullException(nameof(localSaveSlotStore));

            var sessions = new 경영SimulationSessionAccessor(sessionStore);
            lifecycle = new 경영SimulationSession생명주기Service(
                sessions, saveStore, battleReconciler);
            nature = new SimulationNatureSurvivalService(sessionStore);
            farm = new SimulationFarmSurvivalService(sessionStore);
            regionalIncidents = new SimulationRegionalIncidentService(sessions);
        }

        public SimulationRuntimeDescriptor Descriptor { get; } = new()
        {
            AuthorityLocation = SimulationAuthorityLocation.LocalProcess,
            Purpose = SimulationRuntimePurpose.Playable,
            RuntimeStableId = "simulation-runtime:local-process",
            RequiresNetwork = false,
        };

        public ISimulationSessionRuntime Sessions => this;
        public ISimulationNatureSurvivalRuntime Nature => this;
        public ISimulationSessionGameplayRuntime Gameplay => this;
        public ISimulationWorldInteractionRuntime WorldInteractions => this;
        public ISimulationTurnRuntime Turns => this;
        public ISimulationFarmChoiceRuntime FarmChoices => this;
        public ISimulationLogisticsRuntime Logistics => this;
        public ISimulationFarmWorldInteractionRuntime FarmWorldInteractions => this;
        public ISimulationNatureWorldInteractionRuntime NatureWorldInteractions => this;

        public ValueTask<경영SimulationSessionSnapshot> CreateAsync(
            경영SimulationSession생성Request request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => lifecycle.Create(request), cancellationToken);

        ValueTask<경영SimulationSessionSnapshot> ISimulationSessionRuntime.GetAsync(
            string sessionStableId,
            CancellationToken cancellationToken)
            => ExecuteAsync(() => lifecycle.Get(sessionStableId), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot> AdvanceWorldTickAsync(
            string sessionStableId,
            경영SimulationTick진행Request request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => lifecycle.Advance(sessionStableId, request),
                cancellationToken);

        ValueTask<SimulationNatureSurvivalStateSnapshot>
            ISimulationNatureSurvivalRuntime.GetAsync(
            string sessionStableId,
            CancellationToken cancellationToken)
            => ExecuteAsync(() => nature.Get(sessionStableId), cancellationToken);

        public ValueTask<SimulationNatureSurvivalActionPreviewSnapshot> PreviewAsync(
            string sessionStableId,
            SimulationNatureSurvivalActionPreviewRequest request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => nature.Preview(sessionStableId, request),
                cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot> ConfirmAsync(
            string sessionStableId,
            SimulationNatureSurvivalCommandRequest request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => nature.Confirm(sessionStableId, request),
                cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot> AdvanceRealtimeAsync(
            string sessionStableId,
            SimulationNatureSurvivalClockAdvanceRequest request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => nature.AdvanceClock(sessionStableId, request),
                cancellationToken);

        public ValueTask<SimulationTurnClosingContextSnapshot>
            GetTurnClosingContextAsync(string sessionStableId,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .GetTurnClosingContext(), cancellationToken);

        public ValueTask<SimulationTurnClosingPreviewSnapshot>
            PreviewTurnClosingAsync(string sessionStableId,
                SimulationTurnClosingPreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .PreviewTurnClosing(request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot> ConfirmTurnClosingAsync(
            string sessionStableId, SimulationTurnClosingConfirmRequest request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .ConfirmTurnClosing(request), cancellationToken);

        public ValueTask<Simulation타로객체반응PreviewSnapshot>
            PreviewTarotObjectReactionAsync(string sessionStableId,
                Simulation타로객체반응PreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .Preview타로객체반응(request), cancellationToken);

        public ValueTask<SimulationFarmChoiceContextSnapshot>
            GetFarmChoiceContextAsync(string sessionStableId,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .GetFarmChoiceContext(), cancellationToken);

        public ValueTask<SimulationFarmChoicePreviewSnapshot>
            PreviewFarmChoiceAsync(string sessionStableId,
                SimulationFarmChoicePreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .PreviewFarmChoice(request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot> ConfirmFarmChoiceAsync(
            string sessionStableId, SimulationFarmChoiceConfirmRequest request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .ConfirmFarmChoice(request), cancellationToken);

        public ValueTask<SimulationLogisticsMovementPreviewSnapshot>
            PreviewLogisticsMovementAsync(string sessionStableId,
                SimulationLogisticsMovementPreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .PreviewLogisticsMovement(request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot>
            ConfirmLogisticsMovementAsync(string sessionStableId,
                SimulationLogisticsMovementConfirmRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .ConfirmLogisticsMovement(request), cancellationToken);

        public ValueTask<SimulationFreightDispatchPreviewSnapshot>
            PreviewFreightDispatchAsync(string sessionStableId,
                SimulationFreightDispatchPreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .PreviewFreightDispatch(request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot>
            ConfirmFreightDispatchAsync(string sessionStableId,
                SimulationFreightDispatchConfirmRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .ConfirmFreightDispatch(request), cancellationToken);

        public ValueTask<SimulationFarmWorkPreviewSnapshot> PreviewFarmWorkAsync(
            string sessionStableId, SimulationFarmWorkPreviewRequest request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => farm.PreviewWork(sessionStableId, request),
                cancellationToken);

        public ValueTask<SimulationFarmConstructionPlacementPreviewSnapshot>
            PreviewFarmConstructionPlacementAsync(string sessionStableId,
                SimulationFarmConstructionPlacementPreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .PreviewFarmConstructionPlacement(request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot>
            ConfirmFarmConstructionPlacementAsync(string sessionStableId,
                SimulationFarmConstructionPlacementConfirmRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => RequireSession(sessionStableId)
                .ConfirmFarmConstructionPlacement(request), cancellationToken);

        public ValueTask<SimulationFarmSurvivalStateSnapshot> ConfirmFarmWorkAsync(
            string sessionStableId, SimulationFarmWorkConfirmRequest request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() => farm.ConfirmWork(sessionStableId, request),
                cancellationToken);

        public ValueTask<SimulationNatureThreatObservationPreviewSnapshot>
            PreviewNatureThreatObservationAsync(string sessionStableId,
                SimulationNatureThreatObservationPreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => regionalIncidents.PreviewThreatObservation(
                sessionStableId, request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot>
            ConfirmNatureThreatObservationAsync(string sessionStableId,
                SimulationNatureThreatObservationConfirmRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => regionalIncidents.ConfirmThreatObservation(
                sessionStableId, request), cancellationToken);

        public ValueTask<SimulationNatureEmergencyRetreatPreviewSnapshot>
            PreviewNatureEmergencyRetreatAsync(string sessionStableId,
                SimulationNatureEmergencyRetreatPreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => regionalIncidents.PreviewEmergencyRetreat(
                sessionStableId, request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot>
            ConfirmNatureEmergencyRetreatAsync(string sessionStableId,
                SimulationNatureEmergencyRetreatConfirmRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => regionalIncidents.ConfirmEmergencyRetreat(
                sessionStableId, request), cancellationToken);

        public ValueTask<SimulationNatureRestorationPreviewSnapshot>
            PreviewNatureRestorationAsync(string sessionStableId,
                SimulationNatureRestorationPreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => regionalIncidents.PreviewRestoration(
                sessionStableId, request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot>
            ConfirmNatureRestorationAsync(string sessionStableId,
                SimulationNatureRestorationConfirmRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => regionalIncidents.ConfirmRestoration(
                sessionStableId, request), cancellationToken);

        public ValueTask<SimulationNaturePartyRecoveryPreviewSnapshot>
            PreviewNaturePartyRecoveryAsync(string sessionStableId,
                SimulationNaturePartyRecoveryPreviewRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => regionalIncidents.PreviewPartyRecovery(
                sessionStableId, request), cancellationToken);

        public ValueTask<경영SimulationSessionSnapshot>
            ConfirmNaturePartyRecoveryAsync(string sessionStableId,
                SimulationNaturePartyRecoveryConfirmRequest request,
                CancellationToken cancellationToken = default)
            => ExecuteAsync(() => regionalIncidents.ConfirmPartyRecovery(
                sessionStableId, request), cancellationToken);

        public ValueTask<SimulationLocalSaveSlotResult> SaveSlotAsync(
            string sessionStableId,
            SimulationLocalSaveSlotRequest request,
            CancellationToken cancellationToken = default)
            => ExecuteAsync(() =>
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                var slotStableId = RequireSlot(request.SlotStableId);
                var saveStableId = "simulation-save:local:"
                    + slotStableId + ":" + Guid.NewGuid().ToString("N");
                var package = lifecycle.Save(sessionStableId,
                    new SimulationSessionSaveRequest
                    {
                        SaveStableId = saveStableId,
                        ExpectedRevision = request.ExpectedRevision,
                        LhWorldState = request.LhWorldState,
                    });
                slotStore.Write(slotStableId, package);
                return new SimulationLocalSaveSlotResult
                {
                    SlotStableId = slotStableId,
                    SaveStableId = package.SaveStableId,
                    ReplayHash = package.ReplayHash,
                    SavedWorldTick = package.SavedWorldTick,
                    SavedWorldRevision = package.SavedWorldRevision,
                };
            }, cancellationToken);

        public ValueTask<SimulationLocalLoadSlotResult> LoadSlotAsync(
            string slotStableId,
            CancellationToken cancellationToken = default)
            => LoadOrVerifyAsync(slotStableId, true, cancellationToken);

        public ValueTask<SimulationLocalLoadSlotResult> VerifySlotAsync(
            string slotStableId,
            CancellationToken cancellationToken = default)
            => LoadOrVerifyAsync(slotStableId, false, cancellationToken);

        private ValueTask<SimulationLocalLoadSlotResult> LoadOrVerifyAsync(
            string slotStableId,
            bool activate,
            CancellationToken cancellationToken)
            => ExecuteAsync(() =>
            {
                var slot = slotStore.Read(RequireSlot(slotStableId));
                saveStore.SaveOrGet(slot.Package);
                var request = new SimulationSessionRestoreRequest
                {
                    SaveStableId = slot.Package.SaveStableId,
                };
                var restored = activate
                    ? lifecycle.Restore(request)
                    : lifecycle.VerifyReplay(request);
                return new SimulationLocalLoadSlotResult
                {
                    SlotStableId = slot.SlotStableId,
                    RecoveredFromBackup = slot.RecoveredFromBackup,
                    Restore = restored,
                };
            }, cancellationToken);

        private async ValueTask<T> ExecuteAsync<T>(Func<T> action,
            CancellationToken cancellationToken)
        {
            await commandGate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return action();
            }
            finally
            {
                commandGate.Release();
            }
        }

        private static string RequireSlot(string slotStableId)
        {
            if (string.IsNullOrWhiteSpace(slotStableId))
                throw new SimulationContractException("SimulationLocalSaveSlotInvalid");
            return slotStableId.Trim();
        }

        private 경영SimulationSessionAggregate RequireSession(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException("SimulationSessionStableIdMissing");
            return sessionStore.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
        }

        public void Dispose() => commandGate.Dispose();
    }
}
