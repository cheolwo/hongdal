using System;
using System.Collections.Concurrent;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "플레이어 지식 Aggregate를 찾고 등록하는 Application 저장소 경계를 정의한다.",
        Boundary = "영속 저장 계약이 아니라 이번 Logic E1~E3의 실행 수명 저장소다.")]
    public interface ISimulation플레이어지식Store
    {
        Simulation플레이어지식Aggregate? Find(string ledgerStableId);
        void Add(string ledgerStableId, Simulation플레이어지식Aggregate aggregate);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "지식 습득 단위시험과 LocalProcess 실행을 위한 메모리 저장소를 제공한다.",
        Boundary = "Save 판본 또는 운영 영속성을 의미하지 않는다.")]
    public sealed class InMemorySimulation플레이어지식Store :
        ISimulation플레이어지식Store
    {
        private readonly ConcurrentDictionary<string,
            Simulation플레이어지식Aggregate> aggregates =
            new ConcurrentDictionary<string, Simulation플레이어지식Aggregate>(
                StringComparer.Ordinal);

        public Simulation플레이어지식Aggregate? Find(string ledgerStableId)
            => aggregates.TryGetValue(ledgerStableId, out var value)
                ? value
                : null;

        public void Add(string ledgerStableId,
            Simulation플레이어지식Aggregate aggregate)
        {
            if (!aggregates.TryAdd(ledgerStableId, aggregate))
                throw new SimulationConflictException(
                    "PlayerKnowledgeLedgerAlreadyExists");
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "지식 습득 Preview와 Confirm을 플레이어 지식 Aggregate에 전달한다.",
        Boundary = "HTTP·저장·Unity Adapter를 열지 않는 순수 Application 경계다.",
        SubmoduleKey = Ssalddel.Contracts.Common.Metadata
            .SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행)]
    public sealed class Simulation플레이어지식Service
    {
        private readonly ISimulation플레이어지식Store store;

        public Simulation플레이어지식Service(
            ISimulation플레이어지식Store store)
            => this.store = store ?? throw new ArgumentNullException(nameof(store));

        public void Create(string ledgerStableId,
            Simulation플레이어지식InitialStateRequest request)
            => store.Add(RequireLedgerStableId(ledgerStableId),
                new Simulation플레이어지식Aggregate(request));

        public Simulation플레이어지식LedgerSnapshot Get(string ledgerStableId)
            => Find(ledgerStableId).Snapshot();

        public Simulation지식습득PreviewSnapshot Preview(string ledgerStableId,
            Simulation지식습득PreviewRequest request)
            => Find(ledgerStableId).Preview(request);

        public Simulation지식습득ConfirmResult Confirm(string ledgerStableId,
            Simulation지식습득ConfirmRequest request)
            => Find(ledgerStableId).Confirm(request);

        private Simulation플레이어지식Aggregate Find(string ledgerStableId)
            => store.Find(RequireLedgerStableId(ledgerStableId))
                ?? throw new SimulationNotFoundException(
                    "PlayerKnowledgeLedgerNotFound");

        private static string RequireLedgerStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(
                    "PlayerKnowledgeLedgerStableIdInvalid");
            return value.Trim();
        }
    }
}
