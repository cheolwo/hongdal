using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 팀 정책을 다시 확인한 뒤 Session aggregate의 공동 카드 원장을 변경한다.
    /// HTTP가 팀 구성이나 초기 카드를 생성하지 않는다.
    /// </summary>
    public sealed class SimulationTeamRoleCardService
    {
        private readonly ISimulationTeamObservationPolicyStore policyStore;
        private readonly I경영SimulationSessionStore sessionStore;

        public SimulationTeamRoleCardService(
            ISimulationTeamObservationPolicyStore teamPolicyStore,
            I경영SimulationSessionStore simulationSessionStore)
        {
            policyStore = teamPolicyStore
                ?? throw new ArgumentNullException(nameof(teamPolicyStore));
            sessionStore = simulationSessionStore
                ?? throw new ArgumentNullException(nameof(simulationSessionStore));
        }

        public SimulationTeamRoleCardStateSnapshot Get(
            string sessionStableId,
            string actorStableId)
        {
            var session = FindCurrent(sessionStableId, actorStableId);
            return session.GetTeamRoleCards();
        }

        public SimulationTeamRoleCardStateSnapshot Equip(
            string sessionStableId,
            SimulationTeamRoleCardEquipRequest request)
        {
            SimulationTeamRoleCardState.ValidateEquip(request);
            return FindCurrent(sessionStableId, request.RequestingActorStableId)
                .EquipTeamRoleCard(request);
        }

        public SimulationTeamRoleCardStateSnapshot StartActivity(
            string sessionStableId,
            SimulationTeamActivityStartRequest request)
        {
            SimulationTeamRoleCardState.ValidateStart(request);
            return FindCurrent(sessionStableId, request.ActorStableId)
                .StartTeamActivity(request);
        }

        public SimulationTeamRoleCardStateSnapshot EndActivity(
            string sessionStableId,
            SimulationTeamActivityEndRequest request)
        {
            SimulationTeamRoleCardState.ValidateEnd(request);
            return FindCurrent(sessionStableId, request.ActorStableId)
                .EndTeamActivity(request);
        }

        private 경영SimulationSessionAggregate FindCurrent(
            string sessionStableId,
            string actorStableId)
        {
            var policy = policyStore.FindForObserver(sessionStableId, actorStableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationTeamObservationPolicyNotFound");
            var session = sessionStore.Find(sessionStableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationSessionNotFound");
            var state = session.GetTeamRoleCards();
            if (!policy.SimulationOnly || policy.IsOperationalState
                || !string.Equals(policy.SessionStableId, state.SessionStableId,
                    StringComparison.Ordinal)
                || !string.Equals(policy.TeamStableId, state.TeamStableId,
                    StringComparison.Ordinal)
                || policy.Revision != state.TeamPolicyRevision
                || !policy.MemberActorStableIds.OrderBy(value => value,
                        StringComparer.Ordinal)
                    .SequenceEqual(state.MemberActorStableIds.OrderBy(value => value,
                        StringComparer.Ordinal), StringComparer.Ordinal))
                throw new SimulationConflictException(
                    "SimulationTeamRoleCardPolicyMismatch");
            return session;
        }
    }
}
