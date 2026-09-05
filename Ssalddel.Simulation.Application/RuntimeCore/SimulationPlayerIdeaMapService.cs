using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "LocalProcess와 RemoteHost가 공유하는 플레이어 이데아 맵 읽기 경계다.",
        Boundary = "Application은 이데아 관계를 저장하거나 다시 계산하지 않고 Session Core의 파생 조회를 호출한다.")]
    public sealed class SimulationPlayerIdeaMapService
    {
        private readonly I경영SimulationSessionStore sessionStore;

        public SimulationPlayerIdeaMapService(
            I경영SimulationSessionStore simulationSessionStore)
        {
            sessionStore = simulationSessionStore
                ?? throw new ArgumentNullException(nameof(simulationSessionStore));
        }

        public Simulation플레이어이데아맵ProjectionSnapshot Get(
            string sessionStableId, string playerStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException(
                    "SimulationSessionStableIdInvalid");
            var session = sessionStore.Find(sessionStableId.Trim())
                ?? throw new SimulationNotFoundException(
                    "SimulationSessionNotFound");
            return session.GetPlayerIdeaMapProjection(playerStableId);
        }
    }
}
