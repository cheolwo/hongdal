using System;
using System.Collections.Concurrent;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 분대 배정 Aggregate를 찾고 등록하는 Application 저장소 경계를 정의한다.",
        Boundary = "영속 저장이나 Save 판본이 아닌 E1~E3 실행 수명 저장소 계약이다.",
        WorldInteractionIds = new[] { SimulationFarm분대배정Codes.WorldInteractionId })]
    public interface ISimulationFarm분대배정Store { SimulationFarm분대배정Aggregate? Find(string id); void Add(string id, SimulationFarm분대배정Aggregate aggregate); }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 분대 배정 E1~E3 실행 수명을 위한 메모리 저장소를 제공한다.",
        Boundary = "Save·RemoteHost·운영 영속성 증거가 아니다.", WorldInteractionIds = new[] { SimulationFarm분대배정Codes.WorldInteractionId })]
    public sealed class InMemorySimulationFarm분대배정Store : ISimulationFarm분대배정Store
    {
        private readonly ConcurrentDictionary<string, SimulationFarm분대배정Aggregate> values = new ConcurrentDictionary<string, SimulationFarm분대배정Aggregate>(StringComparer.Ordinal);
        public SimulationFarm분대배정Aggregate? Find(string id) => values.TryGetValue(id, out var value) ? value : null;
        public void Add(string id, SimulationFarm분대배정Aggregate aggregate) { if (!values.TryAdd(id, aggregate)) throw new SimulationConflictException("FarmSquadAssignmentLedgerAlreadyExists"); }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 분대 배정 Query·Preview·Confirm과 읽기 전용 초소 슬롯 카드를 제공한다.",
        Boundary = "HTTP·Save·Unity Scene 없이 같은 Domain 규칙만 호출한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { SimulationFarm분대배정Codes.WorldInteractionId })]
    public sealed class SimulationFarm분대배정Service
    {
        private readonly ISimulationFarm분대배정Store store;
        public SimulationFarm분대배정Service(ISimulationFarm분대배정Store store) => this.store = store ?? throw new ArgumentNullException(nameof(store));
        public void Create(string id, SimulationFarm분대배정InitialStateRequest request) => store.Add(Require(id), new SimulationFarm분대배정Aggregate(request));
        public SimulationFarm분대배정LedgerSnapshot Get(string id) => Find(id).Snapshot();
        public SimulationFarm분대배정PreviewSnapshot Preview(string id, SimulationFarm분대배정PreviewRequest request) => Find(id).Preview(request);
        public SimulationFarm분대배정ConfirmResult Confirm(string id, SimulationFarm분대배정ConfirmRequest request) => Find(id).Confirm(request);
        public SimulationFarm분대배정CardSnapshot[] ProjectCards(string id)
        {
            var state = Get(id);
            var assigned = state.Assignments.ToDictionary(x => x.OutpostStableId + "|" + x.SlotStableId, StringComparer.Ordinal);
            return state.Slots.Select(x => {
                assigned.TryGetValue(x.OutpostStableId + "|" + x.SlotStableId, out var assignment);
                return new SimulationFarm분대배정CardSnapshot {
                    CardStableId = "card:farm-defense-slot:" + x.OutpostStableId + ":" + x.SlotStableId,
                    SourceWorldRevision = state.WorldRevision, OutpostStableId = x.OutpostStableId, SlotStableId = x.SlotStableId,
                    SquadStableId = assignment?.SquadStableId ?? string.Empty, IsOccupied = assignment != null };
            }).OrderBy(x => x.OutpostStableId, StringComparer.Ordinal).ThenBy(x => x.SlotStableId, StringComparer.Ordinal).ToArray();
        }
        private SimulationFarm분대배정Aggregate Find(string id) => store.Find(Require(id)) ?? throw new SimulationNotFoundException("FarmSquadAssignmentLedgerNotFound");
        private static string Require(string value) { if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException("FarmSquadAssignmentLedgerStableIdInvalid"); return value.Trim(); }
    }
}
