using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 팀 관찰 정책을 확인한 뒤 Session의 발견 원장과 개인 수집 카드 원장을 조회·변경한다.
    /// 확률, seed, 카드 정의 선택은 클라이언트 입력으로 받지 않는다.
    /// </summary>
    public sealed class SimulationCollectibleCardRewardService
    {
        private readonly ISimulationTeamObservationPolicyStore policyStore;
        private readonly I경영SimulationSessionStore sessionStore;

        public SimulationCollectibleCardRewardService(
            ISimulationTeamObservationPolicyStore teamPolicyStore,
            I경영SimulationSessionStore simulationSessionStore)
        {
            policyStore = teamPolicyStore
                ?? throw new ArgumentNullException(nameof(teamPolicyStore));
            sessionStore = simulationSessionStore
                ?? throw new ArgumentNullException(nameof(simulationSessionStore));
        }

        public SimulationWorldExplorationStateSnapshot GetExploration(
            string sessionStableId, string actorStableId)
            => FindCurrent(sessionStableId, actorStableId).GetWorldExplorationState();

        public SimulationCollectibleCardRewardStateSnapshot GetRewards(
            string sessionStableId, string actorStableId)
            => FindCurrent(sessionStableId, actorStableId)
                .GetCollectibleCardRewards(actorStableId);

        public SimulationTileTraversalConfirmResponse ConfirmTraversal(
            string sessionStableId, SimulationTileTraversalConfirmRequest request)
            => FindCurrent(sessionStableId, request.ActorStableId)
                .ConfirmTileTraversal(request);

        public SimulationCollectibleCardDrawResponse Draw(string sessionStableId,
            string opportunityStableId, SimulationCollectibleCardDrawRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.OpportunityStableId)
                && !string.Equals(request.OpportunityStableId, opportunityStableId,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationCollectibleCardOpportunityPathMismatch");
            var normalized = new SimulationCollectibleCardDrawRequest
            {
                CommandId = request.CommandId,
                ExpectedRevision = request.ExpectedRevision,
                ActorStableId = request.ActorStableId,
                OpportunityStableId = opportunityStableId,
            };
            return FindCurrent(sessionStableId, normalized.ActorStableId)
                .DrawCollectibleCard(normalized);
        }

        public SimulationCollectibleCardTransferResponse Transfer(string sessionStableId,
            string cardCopyStableId, SimulationCollectibleCardTransferRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.CardCopyStableId)
                && !string.Equals(request.CardCopyStableId, cardCopyStableId,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationCollectibleCardCopyPathMismatch");
            var normalized = new SimulationCollectibleCardTransferRequest
            {
                CommandId = request.CommandId,
                ExpectedRevision = request.ExpectedRevision,
                OwnerActorStableId = request.OwnerActorStableId,
                TargetActorStableId = request.TargetActorStableId,
                CardCopyStableId = cardCopyStableId,
            };
            return FindCurrent(sessionStableId, normalized.OwnerActorStableId)
                .TransferCollectibleCard(normalized);
        }

        private 경영SimulationSessionAggregate FindCurrent(
            string sessionStableId, string actorStableId)
        {
            var policy = policyStore.FindForObserver(sessionStableId, actorStableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationTeamObservationPolicyNotFound");
            var session = sessionStore.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
            var state = session.GetCollectibleCardRewards(actorStableId);
            var teamRoles = session.GetTeamRoleCards();
            if (!policy.SimulationOnly || policy.IsOperationalState
                || !string.Equals(policy.SessionStableId, state.SessionStableId,
                    StringComparison.Ordinal)
                || !string.Equals(policy.TeamStableId, state.TeamStableId,
                    StringComparison.Ordinal)
                || policy.Revision != teamRoles.TeamPolicyRevision
                || !policy.MemberActorStableIds.OrderBy(value => value,
                        StringComparer.Ordinal)
                    .SequenceEqual(teamRoles.MemberActorStableIds.OrderBy(value => value,
                        StringComparer.Ordinal), StringComparer.Ordinal))
                throw new SimulationConflictException(
                    "SimulationCollectibleCardTeamPolicyMismatch");
            return session;
        }
    }
}
