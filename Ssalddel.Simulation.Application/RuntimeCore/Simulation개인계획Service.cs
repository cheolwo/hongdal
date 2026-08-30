using System;
using System.Collections.Concurrent;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2, "개인 계획 독립 메모리 원장과 Query·Preview·Confirm 경계를 제공한다.",
        Boundary = "영속성·Session·RemoteHost 미연결. 규칙은 Domain 하나만 호출한다.",
        WorldInteractionIds = new[] { Simulation개인계획Codes.WorldInteractionId })]
    public sealed class Simulation개인계획Service
    {
        private readonly ConcurrentDictionary<string, Simulation개인계획Aggregate> 원장들 = new(StringComparer.Ordinal);
        public void Create(string 원장StableId, Simulation개인계획InitialState 초기)
        {
            var 키 = 검사(원장StableId); var 원장 = new Simulation개인계획Aggregate(초기);
            if (!원장들.TryAdd(키, 원장)) throw new SimulationConflictException("PersonalPlanLedgerAlreadyExists");
        }
        public Simulation개인계획Snapshot Get(string 원장StableId) => 찾기(원장StableId).Snapshot();
        public Simulation개인계획PreviewSnapshot Preview(string 원장StableId, Simulation개인계획PreviewRequest 요청)
            => 찾기(원장StableId).Preview(요청);
        public Simulation개인계획ConfirmResult Confirm(string 원장StableId, Simulation개인계획ConfirmRequest 요청)
            => 찾기(원장StableId).Confirm(요청);
        private Simulation개인계획Aggregate 찾기(string 키) => 원장들.TryGetValue(검사(키), out var 원장)
            ? 원장 : throw new SimulationNotFoundException("PersonalPlanLedgerNotFound");
        private static string 검사(string 값) => string.IsNullOrWhiteSpace(값)
            ? throw new SimulationContractException("PersonalPlanLedgerIdInvalid") : 값.Trim();
    }
}
