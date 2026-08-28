using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "온라인 세계 상태 사본을 저장하고 복원하는 RemoteHost 포트를 제공한다.",
        Boundary = "포트 존재는 실제 DB migration·재기동 또는 운영 배포 증거가 아니다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2원격HostAdapter)]
    public interface ISimulationOnlineWorldCheckpointStore
    {
        SimulationOnlineWorldCheckpointSnapshot? Find();
        void Save(SimulationOnlineWorldCheckpointSnapshot checkpoint);
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Application,
        "온라인 세계 조회·합류·파티·신호와 상태 사본 저장을 조율한다.",
        StepKey = "application.online-world",
        DependsOnStepKeys = new[] { "api.online-world" },
        ExecutionStage = SsalddelCodeExecutionStage.Confirm,
        Effects = SsalddelCodeEffect.PersistentRead |
            SsalddelCodeEffect.PersistentWrite |
            SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "검증된 온라인 Simulation 상태만 변경하며 운영 원장을 호출하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "공식 지속 세계와 비공개 협동방의 서버 권위 실행 경계를 제공한다.",
        Boundary = "Application 실행은 Unity 실제 입력·화면과 운영 배포 증거를 대신하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2원격HostAdapter)]
    public sealed class SimulationOnlineWorldService
    {
        private readonly object gate = new object();
        private readonly ISimulationOnlineWorldCheckpointStore checkpointStore;
        private readonly SimulationOnlineWorldCoordinator coordinator;

        public SimulationOnlineWorldService(
            ISimulationOnlineWorldCheckpointStore checkpointStore)
        {
            this.checkpointStore = checkpointStore
                ?? throw new ArgumentNullException(nameof(checkpointStore));
            var checkpoint = checkpointStore.Find();
            coordinator = checkpoint == null
                ? new SimulationOnlineWorldCoordinator()
                : new SimulationOnlineWorldCoordinator(checkpoint);
            if (checkpoint == null)
                checkpointStore.Save(coordinator.CaptureCheckpoint());
        }

        public SimulationOnlineWorldDirectorySnapshot Directory()
            => coordinator.Directory();

        public SimulationOnlineWorldStateSnapshot GetWorld(string worldStableId)
            => coordinator.RequireWorld(worldStableId);

        public SimulationAccountMeditationSnapshot GetAccountMeditation(
            string authenticatedPlayerStableId)
            => coordinator.AccountMeditation(authenticatedPlayerStableId);

        public bool IsConnectedParticipant(string worldStableId,
            string authenticatedPlayerStableId)
            => coordinator.IsConnectedParticipant(worldStableId,
                authenticatedPlayerStableId);

        public SimulationOnlineWorldMutationResult CreatePrivateRoom(
            string authenticatedPlayerStableId,
            SimulationPrivateRoomCreateRequest request)
            => Mutate(() => coordinator.CreatePrivateRoom(
                authenticatedPlayerStableId, request));

        public SimulationOnlineWorldMutationResult Join(
            string authenticatedPlayerStableId,
            SimulationOnlineWorldJoinRequest request)
            => Mutate(() => coordinator.Join(authenticatedPlayerStableId,
                request));

        public SimulationOnlineWorldMutationResult Leave(
            string authenticatedPlayerStableId,
            SimulationOnlineWorldLeaveRequest request)
            => Mutate(() => coordinator.Leave(authenticatedPlayerStableId,
                request));

        public SimulationOnlineWorldMutationResult CreateParty(
            string authenticatedPlayerStableId,
            SimulationOnlinePartyCreateRequest request)
            => Mutate(() => coordinator.CreateParty(
                authenticatedPlayerStableId, request));

        public SimulationOnlineWorldMutationResult SendFixedSignal(
            string authenticatedPlayerStableId,
            SimulationFixedSignalSendRequest request)
            => Mutate(() => coordinator.SendFixedSignal(
                authenticatedPlayerStableId, request));

        public SimulationOnlineWorldMutationResult TransferAreaSet(
            string authenticatedPlayerStableId,
            SimulationOnlineAreaSetTransferRequest request)
            => Mutate(() => coordinator.TransferAreaSet(
                authenticatedPlayerStableId, request));

        public SimulationOnlineWorldMutationResult ApplyVerifiedMeditation(
            SimulationVerifiedMeditationContributionRequest request)
            => Mutate(() => coordinator.ApplyVerifiedMeditation(request));

        public SimulationOnlineWorldMutationResult
            ApplyVerifiedObjectiveContribution(
                SimulationVerifiedObjectiveContributionRequest request)
            => Mutate(() => coordinator.ApplyVerifiedObjectiveContribution(
                request));

        public SimulationOnlineWorldCheckpointSnapshot CaptureCheckpoint()
            => coordinator.CaptureCheckpoint();

        private SimulationOnlineWorldMutationResult Mutate(
            Func<SimulationOnlineWorldMutationResult> action)
        {
            lock (gate)
            {
                var result = action();
                if (result.Applied)
                    checkpointStore.Save(coordinator.CaptureCheckpoint());
                return result;
            }
        }
    }
}
