using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public interface ISimulationTeamObservationPolicyStore
    {
        SimulationTeamObservationPolicySnapshot? FindForObserver(
            string sessionStableId,
            string observerActorStableId);
    }

    /// <summary>
    /// 실제 팀 원장이 연결되기 전 사용하는 process-local 연결 지점이다.
    /// Controller에는 쓰기 API를 노출하지 않으며 서버 조립부나 test fixture만 정책을 교체한다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class InMemorySimulationTeamObservationPolicyStore
        : ISimulationTeamObservationPolicyStore
    {
        private readonly ConcurrentDictionary<string,
            SimulationTeamObservationPolicySnapshot> policies = new(
                StringComparer.Ordinal);

        public void Replace(SimulationTeamObservationPolicySnapshot policy)
        {
            if (policy == null
                || string.IsNullOrWhiteSpace(policy.SessionStableId)
                || string.IsNullOrWhiteSpace(policy.TeamStableId)
                || policy.MemberActorStableIds == null
                || policy.AllowedViewModeCodes == null)
                throw new ArgumentException("SimulationTeamObservationPolicyInvalid",
                    nameof(policy));
            policies[Key(policy.SessionStableId, policy.TeamStableId)] = Clone(policy);
        }

        public SimulationTeamObservationPolicySnapshot? FindForObserver(
            string sessionStableId,
            string observerActorStableId)
        {
            var session = sessionStableId?.Trim() ?? string.Empty;
            var observer = observerActorStableId?.Trim() ?? string.Empty;
            var matches = policies.Values
                .Where(value => string.Equals(value.SessionStableId, session,
                    StringComparison.Ordinal))
                .OrderBy(value => value.TeamStableId, StringComparer.Ordinal)
                .Where(value => value.MemberActorStableIds.Any(member =>
                    string.Equals(member, observer, StringComparison.Ordinal)))
                .Select(Clone)
                .Take(2)
                .ToArray();
            if (matches.Length > 1)
                throw new SimulationContractException(
                    "SimulationTeamObservationMembershipAmbiguous");
            return matches.FirstOrDefault();
        }

        private static string Key(string sessionStableId, string teamStableId)
            => sessionStableId.Trim() + "|" + teamStableId.Trim();

        private static SimulationTeamObservationPolicySnapshot Clone(
            SimulationTeamObservationPolicySnapshot source)
            => new()
            {
                SessionStableId = source.SessionStableId,
                TeamStableId = source.TeamStableId,
                Revision = source.Revision,
                MembersCanObserve = source.MembersCanObserve,
                MemberActorStableIds = source.MemberActorStableIds.ToArray(),
                AllowedViewModeCodes = source.AllowedViewModeCodes.ToArray(),
                ShowObserverIndicator = source.ShowObserverIndicator,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public interface ISimulationTeamMemberPoseStore
    {
        SimulationTeamMemberPoseSnapshot? Find(
            string sessionStableId,
            string actorStableId);
    }

    /// <summary>
    /// Netcode 또는 전용 위치 수집기가 갱신할 공개 관찰 Pose 연결 지점이다.
    /// HTTP Controller는 이 저장소에 쓰기 기능을 노출하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class InMemorySimulationTeamMemberPoseStore
        : ISimulationTeamMemberPoseStore
    {
        private readonly ConcurrentDictionary<string, SimulationTeamMemberPoseSnapshot>
            poses = new(StringComparer.Ordinal);

        public void Replace(SimulationTeamMemberPoseSnapshot pose)
        {
            ValidatePose(pose);
            var key = Key(pose.SessionStableId, pose.ActorStableId);
            poses.AddOrUpdate(key, _ => Clone(pose), (_, current) =>
            {
                if (pose.PoseRevision < current.PoseRevision)
                    throw new SimulationConflictException(
                        "SimulationTeamObservationPoseRevisionStale");
                return Clone(pose);
            });
        }

        public SimulationTeamMemberPoseSnapshot? Find(
            string sessionStableId,
            string actorStableId)
            => poses.TryGetValue(Key(sessionStableId, actorStableId), out var value)
                ? Clone(value) : null;

        private static void ValidatePose(SimulationTeamMemberPoseSnapshot pose)
        {
            if (pose == null
                || string.IsNullOrWhiteSpace(pose.SessionStableId)
                || string.IsNullOrWhiteSpace(pose.ActorStableId)
                || string.IsNullOrWhiteSpace(pose.TileKey)
                || pose.PoseRevision < 0
                || pose.CameraHeightMeters <= 0d
                || !pose.IsAvailable
                || !pose.SimulationOnly
                || pose.IsOperationalState
                || !pose.PresentationOnly)
                throw new SimulationContractException(
                    "SimulationTeamObservationPoseInvalid");
        }

        private static string Key(string sessionStableId, string actorStableId)
            => (sessionStableId?.Trim() ?? string.Empty) + "|"
                + (actorStableId?.Trim() ?? string.Empty);

        private static SimulationTeamMemberPoseSnapshot Clone(
            SimulationTeamMemberPoseSnapshot source)
            => new()
            {
                SessionStableId = source.SessionStableId,
                ActorStableId = source.ActorStableId,
                PoseRevision = source.PoseRevision,
                CapturedAtUtc = source.CapturedAtUtc,
                TileKey = source.TileKey,
                LocalOffsetXMeters = source.LocalOffsetXMeters,
                LocalOffsetYMeters = source.LocalOffsetYMeters,
                ElevationMeters = source.ElevationMeters,
                CameraHeightMeters = source.CameraHeightMeters,
                YawDegrees = source.YawDegrees,
                PitchDegrees = source.PitchDegrees,
                MovementIntentCode = source.MovementIntentCode,
                IsAvailable = source.IsAvailable,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
                PresentationOnly = source.PresentationOnly,
            };
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public interface ISimulationTeamObservationSessionStore
    {
        SimulationTeamObservationSessionResponse Start(
            SimulationTeamObservationSessionResponse session);
        SimulationTeamObservationSessionResponse? Find(string observationSessionStableId);
        SimulationTeamObservationSessionResponse End(
            string observationSessionStableId,
            string observerActorStableId,
            DateTimeOffset endedAtUtc);
        IReadOnlyList<SimulationTeamObservationSessionResponse> FindActiveByTarget(
            string sessionStableId,
            string targetActorStableId);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class InMemorySimulationTeamObservationSessionStore
        : ISimulationTeamObservationSessionStore
    {
        private readonly ConcurrentDictionary<string,
            SimulationTeamObservationSessionResponse> sessions = new(
                StringComparer.Ordinal);

        public SimulationTeamObservationSessionResponse Start(
            SimulationTeamObservationSessionResponse session)
        {
            var candidate = Clone(session);
            var stored = sessions.GetOrAdd(candidate.ObservationSessionStableId,
                candidate);
            if (!SameStart(stored, candidate))
                throw new SimulationConflictException(
                    "SimulationTeamObservationClientRequestConflict");
            return Clone(stored);
        }

        public SimulationTeamObservationSessionResponse? Find(
            string observationSessionStableId)
            => sessions.TryGetValue(observationSessionStableId?.Trim()
                    ?? string.Empty, out var value)
                ? Clone(value) : null;

        public SimulationTeamObservationSessionResponse End(
            string observationSessionStableId,
            string observerActorStableId,
            DateTimeOffset endedAtUtc)
        {
            var key = observationSessionStableId?.Trim() ?? string.Empty;
            if (!sessions.TryGetValue(key, out var existing))
                throw new SimulationNotFoundException(
                    "SimulationTeamObservationSessionNotFound");
            if (!string.Equals(existing.ObserverActorStableId,
                    observerActorStableId?.Trim(), StringComparison.Ordinal))
                throw new SimulationConflictException(
                    "SimulationTeamObservationObserverMismatch");
            if (existing.StateCode == SimulationTeamObservationSessionStateCodes.Ended)
                return Clone(existing);

            var ended = Clone(existing);
            ended.StateCode = SimulationTeamObservationSessionStateCodes.Ended;
            ended.EndedAtUtc = endedAtUtc;
            sessions[key] = ended;
            return Clone(ended);
        }

        public IReadOnlyList<SimulationTeamObservationSessionResponse> FindActiveByTarget(
            string sessionStableId,
            string targetActorStableId)
            => sessions.Values
                .Where(value => value.StateCode
                    == SimulationTeamObservationSessionStateCodes.Active)
                .Where(value => string.Equals(value.SessionStableId,
                    sessionStableId?.Trim(), StringComparison.Ordinal))
                .Where(value => string.Equals(value.TargetActorStableId,
                    targetActorStableId?.Trim(), StringComparison.Ordinal))
                .OrderBy(value => value.ObserverActorStableId, StringComparer.Ordinal)
                .Select(Clone)
                .ToArray();

        private static bool SameStart(
            SimulationTeamObservationSessionResponse first,
            SimulationTeamObservationSessionResponse second)
            => first.SessionStableId == second.SessionStableId
                && first.TeamStableId == second.TeamStableId
                && first.ObserverActorStableId == second.ObserverActorStableId
                && first.TargetActorStableId == second.TargetActorStableId
                && first.ViewModeCode == second.ViewModeCode
                && first.TeamRevision == second.TeamRevision;

        private static SimulationTeamObservationSessionResponse Clone(
            SimulationTeamObservationSessionResponse source)
            => new()
            {
                ObservationSessionStableId = source.ObservationSessionStableId,
                SessionStableId = source.SessionStableId,
                TeamStableId = source.TeamStableId,
                ObserverActorStableId = source.ObserverActorStableId,
                TargetActorStableId = source.TargetActorStableId,
                ViewModeCode = source.ViewModeCode,
                StateCode = source.StateCode,
                TeamRevision = source.TeamRevision,
                StartedAtUtc = source.StartedAtUtc,
                EndedAtUtc = source.EndedAtUtc,
                CanControlTarget = source.CanControlTarget,
                MoveObserverActor = source.MoveObserverActor,
                ChangesWorldState = source.ChangesWorldState,
                ShowObserverIndicator = source.ShowObserverIndicator,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
                PresentationOnly = source.PresentationOnly,
            };
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class SimulationTeamObservationService
    {
        private readonly ISimulationTeamObservationPolicyStore policyStore;
        private readonly ISimulationTeamObservationSessionStore sessionStore;
        private readonly ISimulationTeamMemberPoseStore poseStore;
        private readonly SimulationTeamObservationPolicy policy = new();

        public SimulationTeamObservationService(
            ISimulationTeamObservationPolicyStore policyStore,
            ISimulationTeamObservationSessionStore sessionStore,
            ISimulationTeamMemberPoseStore poseStore)
        {
            this.policyStore = policyStore
                ?? throw new ArgumentNullException(nameof(policyStore));
            this.sessionStore = sessionStore
                ?? throw new ArgumentNullException(nameof(sessionStore));
            this.poseStore = poseStore
                ?? throw new ArgumentNullException(nameof(poseStore));
        }

        public SimulationTeamObservationAccessResponse Evaluate(
            string sessionStableId,
            SimulationTeamObservationAccessRequest request)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId) || request == null)
                throw new SimulationContractException(
                    "SimulationTeamObservationRequestInvalid");
            var source = policyStore.FindForObserver(
                    sessionStableId.Trim(), request.ObserverActorStableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationTeamObservationTeamNotFound");
            return policy.Evaluate(source, request);
        }

        public SimulationTeamObservationSessionResponse Start(
            string sessionStableId,
            SimulationTeamObservationSessionStartRequest request)
        {
            if (request == null || request.ClientRequestId == Guid.Empty)
                throw new SimulationContractException(
                    "SimulationTeamObservationStartRequestInvalid");
            var access = Evaluate(sessionStableId,
                new SimulationTeamObservationAccessRequest
                {
                    ObserverActorStableId = request.ObserverActorStableId,
                    TargetActorStableId = request.TargetActorStableId,
                    RequestedViewModeCode = request.RequestedViewModeCode,
                    ExpectedTeamRevision = request.ExpectedTeamRevision,
                    TargetTileKey = request.TargetTileKey,
                });
            if (!access.Allowed)
                throw new SimulationConflictException(access.ReasonCode);

            return sessionStore.Start(new SimulationTeamObservationSessionResponse
            {
                ObservationSessionStableId = "team-observation:"
                    + request.ClientRequestId.ToString("N"),
                SessionStableId = access.SessionStableId,
                TeamStableId = access.TeamStableId,
                ObserverActorStableId = access.ObserverActorStableId,
                TargetActorStableId = access.TargetActorStableId,
                ViewModeCode = access.ViewModeCode,
                StateCode = SimulationTeamObservationSessionStateCodes.Active,
                TeamRevision = access.TeamRevision,
                StartedAtUtc = DateTimeOffset.UtcNow,
                CanControlTarget = false,
                MoveObserverActor = false,
                ChangesWorldState = false,
                ShowObserverIndicator = true,
                SimulationOnly = true,
                IsOperationalState = false,
                PresentationOnly = true,
            });
        }

        public SimulationTeamObservationFrameResponse GetFrame(
            string sessionStableId,
            string observationSessionStableId)
        {
            var observation = FindActive(sessionStableId,
                observationSessionStableId);
            var pose = poseStore.Find(observation.SessionStableId,
                    observation.TargetActorStableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationTeamObservationPoseNotFound");
            if (!pose.IsAvailable || !pose.SimulationOnly
                || pose.IsOperationalState || !pose.PresentationOnly)
                throw new SimulationConflictException(
                    "SimulationTeamObservationPoseUnavailable");

            return new SimulationTeamObservationFrameResponse
            {
                Observation = observation,
                TargetPose = pose,
                ContainsPrivateUi = false,
                ContainsInventory = false,
                ContainsChat = false,
                PresentationOnly = true,
            };
        }

        public SimulationTeamObservationSessionResponse End(
            string sessionStableId,
            string observationSessionStableId,
            SimulationTeamObservationSessionEndRequest request)
        {
            if (request == null || request.ClientRequestId == Guid.Empty
                || string.IsNullOrWhiteSpace(request.ObserverActorStableId))
                throw new SimulationContractException(
                    "SimulationTeamObservationEndRequestInvalid");
            var existing = sessionStore.Find(observationSessionStableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationTeamObservationSessionNotFound");
            RequireSameSession(sessionStableId, existing);
            return sessionStore.End(observationSessionStableId,
                request.ObserverActorStableId, DateTimeOffset.UtcNow);
        }

        public SimulationTeamObserverIndicatorResponse GetObservers(
            string sessionStableId,
            string targetActorStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId)
                || string.IsNullOrWhiteSpace(targetActorStableId))
                throw new SimulationContractException(
                    "SimulationTeamObservationTargetInvalid");
            var observers = sessionStore.FindActiveByTarget(
                    sessionStableId.Trim(), targetActorStableId.Trim())
                .Where(IsStillAuthorized)
                .Select(value => value.ObserverActorStableId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            return new SimulationTeamObserverIndicatorResponse
            {
                SessionStableId = sessionStableId.Trim(),
                TargetActorStableId = targetActorStableId.Trim(),
                ObserverActorStableIds = observers,
                ActiveObserverCount = observers.Length,
                ShowIndicator = observers.Length > 0,
                PresentationOnly = true,
            };
        }

        private SimulationTeamObservationSessionResponse FindActive(
            string sessionStableId,
            string observationSessionStableId)
        {
            var observation = sessionStore.Find(observationSessionStableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationTeamObservationSessionNotFound");
            RequireSameSession(sessionStableId, observation);
            if (observation.StateCode != SimulationTeamObservationSessionStateCodes.Active)
                throw new SimulationConflictException(
                    "SimulationTeamObservationSessionNotActive");
            if (!IsStillAuthorized(observation))
                throw new SimulationConflictException(
                    "SimulationTeamObservationAuthorizationChanged");
            return observation;
        }

        private bool IsStillAuthorized(
            SimulationTeamObservationSessionResponse observation)
        {
            try
            {
                return Evaluate(observation.SessionStableId,
                    new SimulationTeamObservationAccessRequest
                    {
                        ObserverActorStableId = observation.ObserverActorStableId,
                        TargetActorStableId = observation.TargetActorStableId,
                        RequestedViewModeCode = observation.ViewModeCode,
                        ExpectedTeamRevision = observation.TeamRevision,
                        TargetTileKey = "team-observation:active-target",
                    }).Allowed;
            }
            catch (SimulationNotFoundException)
            {
                return false;
            }
        }

        private static void RequireSameSession(
            string sessionStableId,
            SimulationTeamObservationSessionResponse observation)
        {
            if (!string.Equals(sessionStableId?.Trim(),
                    observation.SessionStableId, StringComparison.Ordinal))
                throw new SimulationNotFoundException(
                    "SimulationTeamObservationSessionNotFound");
        }
    }
}
