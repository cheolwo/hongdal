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
        "공동체 방문자 임시 체류 Aggregate 저장소 경계를 정의한다.",
        Boundary = "영속 저장이나 Save 판본이 아닌 E1~E3 실행 수명 저장소 계약이다.",
        WorldInteractionIds = new[] { Simulation공동체방문자체류Codes.WorldInteractionId })]
    public interface ISimulation공동체방문자체류Store
    {
        Simulation공동체방문자체류Aggregate? Find(string ledgerStableId);
        void Add(string ledgerStableId,
            Simulation공동체방문자체류Aggregate aggregate);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "공동체 방문자 임시 체류 집중시험용 메모리 저장소를 제공한다.",
        Boundary = "RemoteHost·운영 영속성·Save/Replay 증거가 아니다.",
        WorldInteractionIds = new[] { Simulation공동체방문자체류Codes.WorldInteractionId })]
    public sealed class InMemorySimulation공동체방문자체류Store :
        ISimulation공동체방문자체류Store
    {
        private readonly ConcurrentDictionary<string,
            Simulation공동체방문자체류Aggregate> aggregates = new(
                StringComparer.Ordinal);

        public Simulation공동체방문자체류Aggregate? Find(string ledgerStableId)
            => aggregates.TryGetValue(ledgerStableId, out var value)
                ? value : null;

        public void Add(string ledgerStableId,
            Simulation공동체방문자체류Aggregate aggregate)
        {
            if (!aggregates.TryAdd(ledgerStableId, aggregate))
                throw new SimulationConflictException(
                    "CommunityVisitorStayLedgerAlreadyExists");
        }
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "공동체 방문자 임시 체류 Query·Preview·Confirm과 읽기 전용 카드를 제공한다.",
        Boundary = "HTTP·Save·Unity Scene 없이 같은 Domain 규칙을 호출한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { Simulation공동체방문자체류Codes.WorldInteractionId })]
    public sealed class Simulation공동체방문자체류Service
    {
        private readonly ISimulation공동체방문자체류Store store;

        public Simulation공동체방문자체류Service(
            ISimulation공동체방문자체류Store store)
            => this.store = store ?? throw new ArgumentNullException(nameof(store));

        public void Create(string ledgerStableId,
            Simulation공동체방문자체류InitialStateRequest request)
            => store.Add(Require(ledgerStableId),
                new Simulation공동체방문자체류Aggregate(request));

        public Simulation공동체방문자체류LedgerSnapshot Get(
            string ledgerStableId) => Find(ledgerStableId).Snapshot();

        public Simulation공동체방문자체류PreviewSnapshot Preview(
            string ledgerStableId,
            Simulation공동체방문자체류PreviewRequest request)
            => Find(ledgerStableId).Preview(request);

        public Simulation공동체방문자체류ConfirmResult Confirm(
            string ledgerStableId,
            Simulation공동체방문자체류ConfirmRequest request)
            => Find(ledgerStableId).Confirm(request);

        public Simulation공동체방문자응대CardSnapshot[] ProjectCards(
            string ledgerStableId)
        {
            var state = Get(ledgerStableId);
            var remaining = Math.Max(0,
                state.GuestCapacity - state.OccupiedGuestCapacity);
            return state.Visitors.OrderBy(value => value.VisitorStableId,
                    StringComparer.Ordinal)
                .Select(value => new Simulation공동체방문자응대CardSnapshot
                {
                    CardStableId = "card:community-visitor:" +
                                   value.VisitorStableId,
                    SourceWorldRevision = state.WorldRevision,
                    VisitorStableId = value.VisitorStableId,
                    StatusCode = value.StatusCode,
                    MindTraceCode = value.MindTraceCode,
                    RemainingGuestCapacity = remaining,
                }).ToArray();
        }

        private Simulation공동체방문자체류Aggregate Find(string ledgerStableId)
            => store.Find(Require(ledgerStableId))
               ?? throw new SimulationNotFoundException(
                   "CommunityVisitorStayLedgerNotFound");

        private static string Require(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(
                    "CommunityVisitorStayLedgerStableIdInvalid");
            return value.Trim();
        }
    }
}
