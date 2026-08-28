using System;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Infrastructure
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "DB가 비활성인 개발 환경의 온라인 세계 상태 사본 Adapter를 제공한다.",
        Boundary = "메모리 Adapter는 영속 재기동 또는 운영 배포 증거가 아니다.",
        SubmoduleKey = Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceSubmoduleKeys.E2원격HostAdapter)]
    public sealed class InMemorySimulationOnlineWorldCheckpointStore
        : ISimulationOnlineWorldCheckpointStore
    {
        private readonly object gate = new object();
        private SimulationOnlineWorldCheckpointSnapshot? checkpoint;

        public SimulationOnlineWorldCheckpointSnapshot? Find()
        {
            lock (gate)
                return checkpoint == null ? null
                    : Clone(checkpoint);
        }

        public void Save(SimulationOnlineWorldCheckpointSnapshot candidate)
        {
            if (candidate == null) throw new ArgumentNullException(nameof(candidate));
            if (!string.Equals(candidate.CheckpointHashSha256,
                    SimulationOnlineWorldCoordinator.CalculateCheckpointHash(candidate),
                    StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationOnlineWorldCheckpointInvalid");
            lock (gate)
            {
                if (checkpoint != null
                    && candidate.DirectoryRevision < checkpoint.DirectoryRevision)
                    throw new SimulationConflictException(
                        "SimulationOnlineWorldCheckpointRevisionRegressed");
                checkpoint = Clone(candidate);
            }
        }

        private static SimulationOnlineWorldCheckpointSnapshot Clone(
            SimulationOnlineWorldCheckpointSnapshot source)
            => new SimulationOnlineWorldCheckpointSnapshot
            {
                SchemaCode = source.SchemaCode,
                DirectoryRevision = source.DirectoryRevision,
                Worlds = Array.ConvertAll(source.Worlds,
                    SimulationOnlineWorldCoordinator.CloneWorld),
                AccountMeditations = Array.ConvertAll(source.AccountMeditations,
                    SimulationOnlineWorldCoordinator.CloneAccount),
                CommandReceipts = Array.ConvertAll(source.CommandReceipts,
                    value => new SimulationOnlineCommandReceiptSnapshot
                    {
                        CommandId = value.CommandId,
                        ActorPlayerStableId = value.ActorPlayerStableId,
                        PayloadHashSha256 = value.PayloadHashSha256,
                        ResultCode = value.ResultCode,
                        WorldStableId = value.WorldStableId,
                        ResultingWorldRevision = value.ResultingWorldRevision,
                    }),
                CheckpointHashSha256 = source.CheckpointHashSha256,
            };
    }
}
