using System;
using System.Collections.Concurrent;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Farm 방위 소집 Aggregate를 찾고 등록하는 Application 저장소 경계를 정의한다.",
        Boundary = "영속 저장이나 Save 판본이 아닌 E1~E3 실행 수명 저장소 계약이다.",
        WorldInteractionIds = new[] { SimulationFarm방위소집Codes.WorldInteractionId })]
    public interface ISimulationFarm방위소집Store
    {
        SimulationFarm방위소집Aggregate? Find(string ledgerStableId);
        void Add(string ledgerStableId, SimulationFarm방위소집Aggregate aggregate);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Farm 방위 소집 집중시험을 위한 메모리 저장소를 제공한다.",
        Boundary = "RemoteHost·운영 영속성·Save/Replay 증거가 아니다.",
        WorldInteractionIds = new[] { SimulationFarm방위소집Codes.WorldInteractionId })]
    public sealed class InMemorySimulationFarm방위소집Store :
        ISimulationFarm방위소집Store
    {
        private readonly ConcurrentDictionary<string,
            SimulationFarm방위소집Aggregate> aggregates = new(
                StringComparer.Ordinal);

        public SimulationFarm방위소집Aggregate? Find(string ledgerStableId)
            => aggregates.TryGetValue(ledgerStableId, out var value)
                ? value : null;

        public void Add(string ledgerStableId,
            SimulationFarm방위소집Aggregate aggregate)
        {
            if (!aggregates.TryAdd(ledgerStableId, aggregate))
                throw new SimulationConflictException(
                    "FarmDefenseMobilizationLedgerAlreadyExists");
        }
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Farm 방위 소집 Query·Preview·Confirm과 읽기 전용 카드 투영을 제공한다.",
        Boundary = "HTTP·Save·Unity Scene 없이 같은 Domain 규칙을 호출한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { SimulationFarm방위소집Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위소집Service
    {
        private readonly ISimulationFarm방위소집Store store;

        public SimulationFarm방위소집Service(
            ISimulationFarm방위소집Store store)
            => this.store = store ?? throw new ArgumentNullException(nameof(store));

        public void Create(string ledgerStableId,
            SimulationFarm방위소집InitialStateRequest request)
            => store.Add(Require(ledgerStableId),
                new SimulationFarm방위소집Aggregate(request));

        public SimulationFarm방위소집LedgerSnapshot Get(string ledgerStableId)
            => Find(ledgerStableId).Snapshot();

        public SimulationFarm방위소집PreviewSnapshot Preview(
            string ledgerStableId, SimulationFarm방위소집PreviewRequest request)
            => Find(ledgerStableId).Preview(request);

        public SimulationFarm방위소집ConfirmResult Confirm(
            string ledgerStableId, SimulationFarm방위소집ConfirmRequest request)
            => Find(ledgerStableId).Confirm(request);

        public SimulationFarm방위소집CardSnapshot[] ProjectCards(
            string ledgerStableId)
        {
            var state = Get(ledgerStableId);
            return state.Squads.OrderBy(value => value.SquadStableId,
                    StringComparer.Ordinal)
                .Select(value => new SimulationFarm방위소집CardSnapshot
                {
                    CardStableId = "card:farm-defense-squad:" +
                                   value.SquadStableId,
                    SourceWorldRevision = state.WorldRevision,
                    SquadStableId = value.SquadStableId,
                    StatusCode = value.StatusCode,
                    ThreatStableId = value.MobilizedThreatStableId,
                    AssignedWorkerCount = value.AssignedWorkerStableIds.Length,
                    ProductionContributionSuspended =
                        value.AssignedWorkerStableIds.Any(worker =>
                            state.SuspendedProductionWorkerStableIds.Contains(
                                worker, StringComparer.Ordinal)),
                }).ToArray();
        }

        private SimulationFarm방위소집Aggregate Find(string ledgerStableId)
            => store.Find(Require(ledgerStableId))
               ?? throw new SimulationNotFoundException(
                   "FarmDefenseMobilizationLedgerNotFound");

        private static string Require(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(
                    "FarmDefenseMobilizationLedgerStableIdInvalid");
            return value.Trim();
        }
    }
}
