using System;
using System.Collections.Concurrent;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2, "자원 재생 독립 원장과 신뢰된 자동 Tick 경계를 제공한다.",
        Boundary = "플레이어 Confirm API·기존 Session 시계·저장을 연결하지 않는다.",
        WorldInteractionIds = new[] { Simulation세계자원재생Codes.WorldInteractionId })]
    public sealed class Simulation세계자원재생Service
    {
        private readonly ConcurrentDictionary<string, Simulation세계자원재생Aggregate> 원장들 = new(StringComparer.Ordinal);
        public void Create(string 원장StableId, Simulation세계자원재생InitialState 초기)
        {
            var 키 = 검사(원장StableId); var 원장 = new Simulation세계자원재생Aggregate(초기);
            if (!원장들.TryAdd(키, 원장)) throw new SimulationConflictException("ResourceRegenerationLedgerAlreadyExists");
        }
        public Simulation세계자원재생Snapshot Get(string 원장StableId) => 찾기(원장StableId).Snapshot();
        public Simulation자원재생Preview PreviewTick(string 원장StableId, Simulation자원재생TickRequest 요청) => 찾기(원장StableId).PreviewTick(요청);
        public Simulation자원재생TickResult ApplyTick(string 원장StableId, Simulation자원재생TickRequest 요청) => 찾기(원장StableId).ApplyTick(요청);
        private Simulation세계자원재생Aggregate 찾기(string 키) => 원장들.TryGetValue(검사(키), out var 원장) ? 원장 : throw new SimulationNotFoundException("ResourceRegenerationLedgerNotFound");
        private static string 검사(string 값) => string.IsNullOrWhiteSpace(값) ? throw new SimulationContractException("ResourceRegenerationLedgerIdInvalid") : 값.Trim();
    }
}
