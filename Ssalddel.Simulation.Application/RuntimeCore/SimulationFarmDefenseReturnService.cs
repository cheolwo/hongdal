using System;
using System.Collections.Concurrent;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2, "Farm 방위 귀환 Aggregate 저장소 경계를 정의한다.", Boundary = "E1~E3 실행 수명 경계이며 Save·RemoteHost 영속 저장이 아니다.", WorldInteractionIds = new[] { SimulationFarm방위귀환Codes.WorldInteractionId })]
    public interface ISimulationFarm방위귀환Store { SimulationFarm방위귀환Aggregate? Find(string ledgerStableId); void Add(string ledgerStableId, SimulationFarm방위귀환Aggregate aggregate); }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2, "Farm 방위 귀환 집중시험용 메모리 저장소를 제공한다.", Boundary = "운영 영속성 증거가 아니다.", WorldInteractionIds = new[] { SimulationFarm방위귀환Codes.WorldInteractionId })]
    public sealed class InMemorySimulationFarm방위귀환Store : ISimulationFarm방위귀환Store
    {
        private readonly ConcurrentDictionary<string, SimulationFarm방위귀환Aggregate> values = new(StringComparer.Ordinal);
        public SimulationFarm방위귀환Aggregate? Find(string id) => values.TryGetValue(id, out var value) ? value : null;
        public void Add(string id, SimulationFarm방위귀환Aggregate aggregate) { if (!values.TryAdd(id, aggregate)) throw new SimulationConflictException("FarmDefenseReturnLedgerAlreadyExists"); }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2, "Farm 방위 귀환 Query·Preview·Confirm과 읽기 전용 인계 카드를 제공한다.", Boundary = "치료·생산 재합류를 실행하지 않고 HTTP·Save·Unity Scene을 포함하지 않는다.", SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행, WorldInteractionIds = new[] { SimulationFarm방위귀환Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위귀환Service
    {
        private readonly ISimulationFarm방위귀환Store store;
        public SimulationFarm방위귀환Service(ISimulationFarm방위귀환Store store) => this.store = store ?? throw new ArgumentNullException(nameof(store));
        public void Create(string id, SimulationFarm방위귀환InitialStateRequest request) => store.Add(Require(id), new SimulationFarm방위귀환Aggregate(request));
        public SimulationFarm방위귀환LedgerSnapshot Get(string id) => Find(id).Snapshot();
        public SimulationFarm방위귀환PreviewSnapshot Preview(string id, SimulationFarm방위귀환PreviewRequest request) => Find(id).Preview(request);
        public SimulationFarm방위귀환ConfirmResult Confirm(string id, SimulationFarm방위귀환ConfirmRequest request) => Find(id).Confirm(request);
        public SimulationFarm방위귀환CardSnapshot[] ProjectCards(string id)
        {
            var state = Get(id);
            return state.Returns.Select(x => new SimulationFarm방위귀환CardSnapshot
            {
                CardStableId = "card:farm-defense-return:" + x.ReturnStableId, SourceWorldRevision = state.WorldRevision,
                ReturnStableId = x.ReturnStableId, SquadStableId = x.SquadStableId, OutpostStableId = x.OutpostStableId,
                TreatmentRequiredCount = x.TreatmentRequiredActorStableIds.Length,
                ProductionRejoinCandidateCount = x.ProductionRejoinCandidateActorStableIds.Length, IsReturned = x.IsReturned
            }).OrderBy(x => x.ReturnStableId, StringComparer.Ordinal).ToArray();
        }
        private SimulationFarm방위귀환Aggregate Find(string id) => store.Find(Require(id)) ?? throw new SimulationNotFoundException("FarmDefenseReturnLedgerNotFound");
        private static string Require(string id) { if (string.IsNullOrWhiteSpace(id)) throw new SimulationContractException("FarmDefenseReturnLedgerStableIdInvalid"); return id.Trim(); }
    }
}
