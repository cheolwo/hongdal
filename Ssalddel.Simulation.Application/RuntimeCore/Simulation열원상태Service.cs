using System;
using System.Collections.Concurrent;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "열원 실행 수명 원장의 조회·등록 경계를 정의한다.",
        Boundary = "영속 저장과 Save 계약이 아니다.",
        WorldInteractionIds = new[] { Simulation열원상태Codes.WorldInteractionId })]
    public interface ISimulation열원상태Store
    {
        Simulation열원상태Aggregate? Find(string 원장StableId);
        void Add(string 원장StableId, Simulation열원상태Aggregate 원장);
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "열원 집중시험에 독립 메모리 원장을 제공한다.",
        Boundary = "운영 저장·RemoteHost·Save 증거가 아니다.",
        WorldInteractionIds = new[] { Simulation열원상태Codes.WorldInteractionId })]
    public sealed class InMemorySimulation열원상태Store : ISimulation열원상태Store
    {
        private readonly ConcurrentDictionary<string, Simulation열원상태Aggregate> 원장들 = new(StringComparer.Ordinal);
        public Simulation열원상태Aggregate? Find(string 원장StableId)
            => 원장들.TryGetValue(원장StableId, out var 원장) ? 원장 : null;
        public void Add(string 원장StableId, Simulation열원상태Aggregate 원장)
        {
            if (!원장들.TryAdd(원장StableId, 원장)) throw new SimulationConflictException("HeatLedgerAlreadyExists");
        }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "신뢰된 열원 원장의 Query·Preview·Confirm을 같은 Domain 규칙으로 연결한다.",
        Boundary = "독립 실행 수명 저장소. RemoteHost·Save·Unity는 연결하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { Simulation열원상태Codes.WorldInteractionId })]
    public sealed class Simulation열원상태Service
    {
        private readonly ISimulation열원상태Store 저장소;
        public Simulation열원상태Service(ISimulation열원상태Store 저장소)
            => this.저장소 = 저장소 ?? throw new ArgumentNullException(nameof(저장소));
        public void Create(string 원장StableId, Simulation열원InitialState 초기상태)
            => 저장소.Add(검사(원장StableId), new Simulation열원상태Aggregate(초기상태));
        public Simulation열원LedgerSnapshot Get(string 원장StableId) => 찾기(원장StableId).Snapshot();
        public Simulation열원PreviewSnapshot Preview(string 원장StableId, Simulation열원PreviewRequest 요청)
            => 찾기(원장StableId).Preview(요청);
        public Simulation열원ConfirmResult Confirm(string 원장StableId, Simulation열원ConfirmRequest 요청)
            => 찾기(원장StableId).Confirm(요청);
        private Simulation열원상태Aggregate 찾기(string 원장StableId)
            => 저장소.Find(검사(원장StableId)) ?? throw new SimulationNotFoundException("HeatLedgerNotFound");
        private static string 검사(string 값)
            => string.IsNullOrWhiteSpace(값) ? throw new SimulationContractException("HeatLedgerIdRequired") : 값.Trim();
    }
}
