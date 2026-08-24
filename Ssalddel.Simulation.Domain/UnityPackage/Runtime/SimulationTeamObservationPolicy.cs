using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// 같은 Simulation 팀의 관찰 가능 여부만 판정한다. 관찰 결과는 대상의 이동,
    /// 상호작용, 재고, World Tick 또는 업무 개정을 변경할 수 없다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
    public sealed class SimulationTeamObservationPolicy
    {
        public SimulationTeamObservationAccessResponse Evaluate(
            SimulationTeamObservationPolicySnapshot policy,
            SimulationTeamObservationAccessRequest request)
        {
            ValidatePolicy(policy);
            ValidateRequest(request);

            var members = new HashSet<string>(
                policy.MemberActorStableIds.Select(value => value.Trim()),
                StringComparer.Ordinal);
            var allowedViews = new HashSet<string>(
                policy.AllowedViewModeCodes.Select(value => value.Trim()),
                StringComparer.Ordinal);
            var observer = request.ObserverActorStableId.Trim();
            var target = request.TargetActorStableId.Trim();
            var viewMode = request.RequestedViewModeCode.Trim();

            var reason = SimulationTeamObservationAccessReasonCodes.SameTeam;
            var allowed = true;
            if (request.ExpectedTeamRevision != policy.Revision)
            {
                allowed = false;
                reason = SimulationTeamObservationAccessReasonCodes.RevisionMismatch;
            }
            else if (!policy.MembersCanObserve)
            {
                allowed = false;
                reason = SimulationTeamObservationAccessReasonCodes.PolicyDisabled;
            }
            else if (!members.Contains(observer))
            {
                allowed = false;
                reason = SimulationTeamObservationAccessReasonCodes.ObserverNotInTeam;
            }
            else if (string.Equals(observer, target, StringComparison.Ordinal))
            {
                allowed = false;
                reason = SimulationTeamObservationAccessReasonCodes.SameActor;
            }
            else if (!members.Contains(target))
            {
                allowed = false;
                reason = SimulationTeamObservationAccessReasonCodes.TargetNotInTeam;
            }
            else if (!allowedViews.Contains(viewMode))
            {
                allowed = false;
                reason = SimulationTeamObservationAccessReasonCodes.ViewModeNotAllowed;
            }

            return new SimulationTeamObservationAccessResponse
            {
                SessionStableId = policy.SessionStableId.Trim(),
                TeamStableId = policy.TeamStableId.Trim(),
                ObserverActorStableId = observer,
                TargetActorStableId = target,
                ViewModeCode = viewMode,
                TargetTileKey = request.TargetTileKey.Trim(),
                TeamRevision = policy.Revision,
                Allowed = allowed,
                ReasonCode = reason,
                RequiresPerViewConsent = false,
                CanControlTarget = false,
                ShowObserverIndicator = allowed && policy.ShowObserverIndicator,
                MoveObserverActor = false,
                ChangesWorldState = false,
                SimulationOnly = true,
                IsOperationalState = false,
                PresentationOnly = true,
            };
        }

        private static void ValidatePolicy(SimulationTeamObservationPolicySnapshot policy)
        {
            if (policy == null
                || string.IsNullOrWhiteSpace(policy.SessionStableId)
                || string.IsNullOrWhiteSpace(policy.TeamStableId)
                || policy.Revision < 0
                || policy.MemberActorStableIds == null
                || policy.AllowedViewModeCodes == null
                || !policy.ShowObserverIndicator
                || !policy.SimulationOnly
                || policy.IsOperationalState)
                throw new SimulationContractException("SimulationTeamObservationPolicyInvalid");

            RequireUniqueValues(policy.MemberActorStableIds,
                "SimulationTeamObservationMemberInvalid");
            RequireUniqueValues(policy.AllowedViewModeCodes,
                "SimulationTeamObservationViewModeInvalid");
            if (policy.AllowedViewModeCodes.Any(value =>
                    !string.Equals(value,
                        SimulationTeamObservationViewModeCodes.FirstPerson,
                        StringComparison.Ordinal)
                    && !string.Equals(value,
                        SimulationTeamObservationViewModeCodes.Follow,
                        StringComparison.Ordinal)))
                throw new SimulationContractException(
                    "SimulationTeamObservationViewModeInvalid");
        }

        private static void ValidateRequest(SimulationTeamObservationAccessRequest request)
        {
            if (request == null
                || string.IsNullOrWhiteSpace(request.ObserverActorStableId)
                || string.IsNullOrWhiteSpace(request.TargetActorStableId)
                || string.IsNullOrWhiteSpace(request.RequestedViewModeCode)
                || request.ExpectedTeamRevision < 0
                || string.IsNullOrWhiteSpace(request.TargetTileKey))
                throw new SimulationContractException("SimulationTeamObservationRequestInvalid");
        }

        private static void RequireUniqueValues(string[] values, string errorCode)
        {
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var raw in values)
            {
                var value = raw?.Trim() ?? string.Empty;
                if (value.Length == 0 || !unique.Add(value))
                    throw new SimulationContractException(errorCode);
            }
        }
    }
}
