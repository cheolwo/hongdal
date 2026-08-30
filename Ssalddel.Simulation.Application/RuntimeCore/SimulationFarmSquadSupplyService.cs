using System;
using System.Collections.Concurrent;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 분대 보급 Aggregate를 찾고 등록하는 Application 저장소 경계를 정의한다.",
        Boundary = "영속 저장이나 Save 판본이 아닌 E1~E3 실행 수명 저장소 계약이다.",
        WorldInteractionIds = new[] { SimulationFarm분대보급Codes.WorldInteractionId })]
    public interface ISimulationFarm분대보급Store
    {
        SimulationFarm분대보급Aggregate? Find(string ledgerStableId);
        void Add(string ledgerStableId, SimulationFarm분대보급Aggregate aggregate);
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 분대 보급 E1~E3 집중시험용 메모리 저장소를 제공한다.",
        Boundary = "Save·RemoteHost·운영 영속성 증거가 아니다.",
        WorldInteractionIds = new[] { SimulationFarm분대보급Codes.WorldInteractionId })]
    public sealed class InMemorySimulationFarm분대보급Store : ISimulationFarm분대보급Store
    {
        private readonly ConcurrentDictionary<string, SimulationFarm분대보급Aggregate> values =
            new ConcurrentDictionary<string, SimulationFarm분대보급Aggregate>(StringComparer.Ordinal);
        public SimulationFarm분대보급Aggregate? Find(string ledgerStableId)
            => values.TryGetValue(ledgerStableId, out var value) ? value : null;
        public void Add(string ledgerStableId, SimulationFarm분대보급Aggregate aggregate)
        {
            if (!values.TryAdd(ledgerStableId, aggregate)) throw new SimulationConflictException("FarmSquadSupplyLedgerAlreadyExists");
        }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 분대 보급 Query·Preview·Confirm과 읽기 전용 준비 카드를 제공한다.",
        Boundary = "HTTP·Save·Unity Scene 없이 같은 Domain 규칙만 호출한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { SimulationFarm분대보급Codes.WorldInteractionId })]
    public sealed class SimulationFarm분대보급Service
    {
        private readonly ISimulationFarm분대보급Store store;
        public SimulationFarm분대보급Service(ISimulationFarm분대보급Store store)
            => this.store = store ?? throw new ArgumentNullException(nameof(store));
        public void Create(string ledgerStableId, SimulationFarm분대보급InitialStateRequest request)
            => store.Add(Require(ledgerStableId), new SimulationFarm분대보급Aggregate(request));
        public SimulationFarm분대보급LedgerSnapshot Get(string ledgerStableId) => Find(ledgerStableId).Snapshot();
        public SimulationFarm분대보급PreviewSnapshot Preview(string ledgerStableId, SimulationFarm분대보급PreviewRequest request)
            => Find(ledgerStableId).Preview(request);
        public SimulationFarm분대보급ConfirmResult Confirm(string ledgerStableId, SimulationFarm분대보급ConfirmRequest request)
            => Find(ledgerStableId).Confirm(request);
        public SimulationFarm분대보급CardSnapshot[] ProjectCards(string ledgerStableId)
        {
            var state = Get(ledgerStableId);
            return state.Squads.Select(x => new SimulationFarm분대보급CardSnapshot
                {
                    CardStableId = "card:farm-defense-supply:" + x.SquadStableId,
                    SourceWorldRevision = state.WorldRevision,
                    SquadStableId = x.SquadStableId,
                    RequiredFoodUnits = x.RequiredFoodUnits,
                    RequiredDurabilityRestoreUnits = x.RequiredDurabilityRestoreUnits,
                    IsSupplied = x.IsSupplied
                }).OrderBy(x => x.SquadStableId, StringComparer.Ordinal).ToArray();
        }
        private SimulationFarm분대보급Aggregate Find(string ledgerStableId)
            => store.Find(Require(ledgerStableId)) ?? throw new SimulationNotFoundException("FarmSquadSupplyLedgerNotFound");
        private static string Require(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException("FarmSquadSupplyLedgerStableIdInvalid");
            return value.Trim();
        }
    }
}
