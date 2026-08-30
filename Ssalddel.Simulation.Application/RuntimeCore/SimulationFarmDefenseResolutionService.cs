using System;
using System.Collections.Concurrent;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 방어 결과 Aggregate의 E1~E3 실행 수명 저장소 경계를 정의한다.",
        Boundary = "Save·RemoteHost·운영 영속 저장을 증명하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm방위결과Codes.WorldInteractionId })]
    public interface ISimulationFarm방위결과Store
    {
        SimulationFarm방위결과Aggregate? Find(string ledgerStableId);
        void Add(string ledgerStableId, SimulationFarm방위결과Aggregate aggregate);
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 방어 결과 E1~E3 집중시험용 메모리 저장소를 제공한다.",
        Boundary = "Save·RemoteHost·운영 영속성 증거가 아니다.",
        WorldInteractionIds = new[] { SimulationFarm방위결과Codes.WorldInteractionId })]
    public sealed class InMemorySimulationFarm방위결과Store : ISimulationFarm방위결과Store
    {
        private readonly ConcurrentDictionary<string, SimulationFarm방위결과Aggregate> values =
            new ConcurrentDictionary<string, SimulationFarm방위결과Aggregate>(StringComparer.Ordinal);
        public SimulationFarm방위결과Aggregate? Find(string ledgerStableId)
            => values.TryGetValue(ledgerStableId, out var value) ? value : null;
        public void Add(string ledgerStableId, SimulationFarm방위결과Aggregate aggregate)
        {
            if (!values.TryAdd(ledgerStableId, aggregate))
                throw new SimulationConflictException("FarmDefenseResolutionLedgerAlreadyExists");
        }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "Farm 방어 결과 Query·Preview·Confirm과 읽기 전용 결과 카드를 같은 Domain 규칙에서 제공한다.",
        Boundary = "결과를 재계산하지 않고 HTTP·Save·Unity Scene을 포함하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { SimulationFarm방위결과Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위결과Service
    {
        private readonly ISimulationFarm방위결과Store store;
        public SimulationFarm방위결과Service(ISimulationFarm방위결과Store store)
            => this.store = store ?? throw new ArgumentNullException(nameof(store));
        public void Create(string ledgerStableId, SimulationFarm방위결과InitialStateRequest request)
            => store.Add(Require(ledgerStableId), new SimulationFarm방위결과Aggregate(request));
        public SimulationFarm방위결과LedgerSnapshot Get(string ledgerStableId) => Find(ledgerStableId).Snapshot();
        public SimulationFarm방위결과PreviewSnapshot Preview(string ledgerStableId, SimulationFarm방위결과PreviewRequest request)
            => Find(ledgerStableId).Preview(request);
        public SimulationFarm방위결과ConfirmResult Confirm(string ledgerStableId, SimulationFarm방위결과ConfirmRequest request)
            => Find(ledgerStableId).Confirm(request);
        public SimulationFarm방위결과CardSnapshot[] ProjectCards(string ledgerStableId)
        {
            var state = Get(ledgerStableId);
            return state.Results.Select(x => new SimulationFarm방위결과CardSnapshot
                {
                    CardStableId = "card:farm-defense-result:" + x.EncounterStableId,
                    SourceWorldRevision = state.WorldRevision,
                    EncounterStableId = x.EncounterStableId,
                    SquadStableId = x.SquadStableId,
                    ThreatReductionUnits = x.ThreatReductionUnits,
                    SafeUntilWorldTick = x.SafeUntilWorldTick,
                    ProductionModifierMilli = x.ProductionModifierMilli,
                    RecoveryModifierMilli = x.RecoveryModifierMilli,
                    LootLineCount = x.Loot.Length,
                    IsResolved = x.IsResolved
                }).OrderBy(x => x.EncounterStableId, StringComparer.Ordinal).ToArray();
        }
        private SimulationFarm방위결과Aggregate Find(string ledgerStableId)
            => store.Find(Require(ledgerStableId)) ?? throw new SimulationNotFoundException("FarmDefenseResolutionLedgerNotFound");
        private static string Require(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException("FarmDefenseResolutionLedgerStableIdInvalid");
            return value.Trim();
        }
    }
}
