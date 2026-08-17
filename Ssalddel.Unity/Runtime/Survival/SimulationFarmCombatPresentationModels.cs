using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Unity.Survival
{
    public static class FarmCombatPresentationCodes
    {
        public const string FirstPersonPrecision = "FirstPersonPrecision";
        public const string ThirdPersonAwareness = "ThirdPersonAwareness";
        public const string AwaitingCombat = "AwaitingCombat";
        public const string Active = "Active";
        public const string Guard = "Guard";
        public const string Counter = "Counter";
        public const string Open = "Open";
        public const string Available = "Available";
        public const string AdvanceAndAttack = "AdvanceAndAttack";
        public const string HoldFormation = "HoldFormation";
        public const string TacticalRetreat = "TacticalRetreat";
        public const string Resolved = "Resolved";
        public const string Allied = "Allied";
        public const string Hostile = "Hostile";
        public const string Perimeter = "Perimeter";
        public const string Forward = "Forward";
        public const string InnerFarm = "InnerFarm";

        public const string LineFormation = "Line";
        public const string WedgeFormation = "Wedge";
        public const string ColumnFormation = "Column";
        public const string IdleMovement = "Idle";
        public const string RunMovement = "Run";
        public const string GuardMovement = "Guard";
        public const string StaggerMovement = "Stagger";

        public const string Ready = "Ready";
        public const string Entering = "Entering";
        public const string Telegraph = "Telegraph";
        public const string Submitting = "Submitting";
        public const string Failed = "Failed";
    }

    public sealed class FarmCombatPerspectiveApiModel
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string PerspectiveCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmCombatBeatApiModel
    {
        public string BeatStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string AppliedPerspectiveCode { get; set; } = string.Empty;
        public string AttackPatternCode { get; set; } = string.Empty;
        public int ImpactOffsetMs { get; set; }
        public int GuardWindowMs { get; set; }
        public int CounterWindowMs { get; set; }
        public int PerfectGuardWindowMs { get; set; }
        public int PerfectCounterWindowMs { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmCombatStateApiModel
    {
        public long WorldRevision { get; set; }
        public FarmCombatEngagementApiModel[] Engagements { get; set; }
            = Array.Empty<FarmCombatEngagementApiModel>();
        public FarmCombatPerspectiveApiModel[] Perspectives { get; set; }
            = Array.Empty<FarmCombatPerspectiveApiModel>();
        public FarmCombatBeatApiModel[] Beats { get; set; }
            = Array.Empty<FarmCombatBeatApiModel>();
        public FarmCombatReactionApiModel[] Reactions { get; set; }
            = Array.Empty<FarmCombatReactionApiModel>();
        public FarmTacticalCombatStateApiModel Tactical { get; set; }
            = new FarmTacticalCombatStateApiModel();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class FarmCombatEngagementApiModel
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmCombatReactionApiModel
    {
        public string ReactionStableId { get; set; } = string.Empty;
        public string BeatStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ReactionActionCode { get; set; } = string.Empty;
        public int ReactionOffsetMs { get; set; }
        public int TimingDeltaMs { get; set; }
        public string GradeCode { get; set; } = string.Empty;
        public decimal ActorDamageUnits { get; set; }
        public int DefenseResponseScore { get; set; }
        public bool ThreatStaggered { get; set; }
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmTacticalCombatStateApiModel
    {
        public FarmTacticalFrontApiModel[] Fronts { get; set; }
            = Array.Empty<FarmTacticalFrontApiModel>();
        public FarmTacticalSquadApiModel[] Squads { get; set; }
            = Array.Empty<FarmTacticalSquadApiModel>();
        public FarmTacticalOpportunityApiModel[] Opportunities { get; set; }
            = Array.Empty<FarmTacticalOpportunityApiModel>();
        public FarmTacticalOrderWindowApiModel[] OrderWindows { get; set; }
            = Array.Empty<FarmTacticalOrderWindowApiModel>();
        public FarmTacticalOrderApiModel[] Orders { get; set; }
            = Array.Empty<FarmTacticalOrderApiModel>();
        public FarmTacticalResolutionApiModel[] Resolutions { get; set; }
            = Array.Empty<FarmTacticalResolutionApiModel>();
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class FarmTacticalFrontApiModel
    {
        public string FrontStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmTacticalSquadApiModel
    {
        public string SquadStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public string PositionCode { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int CombatStrength { get; set; }
        public int RecoverableInjuryCount { get; set; }
        public string[] MemberActorStableIds { get; set; } = Array.Empty<string>();
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmTacticalOrderApiModel
    {
        public string OrderStableId { get; set; } = string.Empty;
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public int ResolvesWorldTick { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmTacticalResolutionApiModel
    {
        public string ResolutionStableId { get; set; } = string.Empty;
        public string OrderStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public int ResolvedWorldTick { get; set; }
        public string FrontPositionCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmTacticalOpportunityApiModel
    {
        public string OpportunityStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string EarningActorStableId { get; set; } = string.Empty;
        public string OpportunityKindCode { get; set; } = string.Empty;
        public int Quality { get; set; }
        public int ExpiresWorldTick { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmTacticalOrderWindowApiModel
    {
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string AuthorizedActorStableId { get; set; } = string.Empty;
        public int ClosesWorldTick { get; set; }
        public string StateCode { get; set; } = string.Empty;
        public string[] AllowedOrderCodes { get; set; } = Array.Empty<string>();
        public string PresentationKey { get; set; } = string.Empty;
    }

    public sealed class FarmCombatPresentationFrame
    {
        public string BeatStableId { get; set; } = string.Empty;
        public string EncounterStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string PerspectiveCode { get; set; } = string.Empty;
        public string AttackPatternCode { get; set; } = string.Empty;
        public int ImpactOffsetMs { get; set; }
        public int GuardWindowMs { get; set; }
        public int CounterWindowMs { get; set; }
        public int PerfectGuardWindowMs { get; set; }
        public int PerfectCounterWindowMs { get; set; }
        public bool OwnsCombatInput { get; set; }
        public bool ShowFocusedThreatTelegraph { get; set; }
        public bool ShowAllThreats { get; set; }
        public bool ShowAllies { get; set; }
        public bool ShowFacilities { get; set; }
        public bool PresentationOnly { get; set; } = true;
        public bool ChangesWorldState { get; set; }
    }

    /// <summary>
    /// 서버가 확정한 전투 박자를 카메라·HUD 표현으로만 투영한다.
    /// 허용 구간과 판정 수치는 재계산하지 않고 서버 상태 사본을 그대로 사용한다.
    /// </summary>
    public sealed class FarmCombatPresentationMapper
    {
        public FarmCombatPresentationFrame Map(
            FarmCombatStateApiModel source,
            string actorStableId)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.WorldRevision < 0 || !source.SimulationOnly
                || source.IsOperationalState)
                throw new InvalidOperationException("FarmCombatBoundaryInvalid");
            if (string.IsNullOrWhiteSpace(actorStableId))
                throw new ArgumentException("FarmCombatActorMissing",
                    nameof(actorStableId));

            var actor = actorStableId.Trim();
            var beat = (source.Beats ?? Array.Empty<FarmCombatBeatApiModel>())
                .SingleOrDefault(value => value.ActorStableId == actor
                    && value.StateCode == FarmCombatPresentationCodes.Active)
                ?? throw new InvalidOperationException("FarmCombatActiveBeatNotFound");
            var perspective = (source.Perspectives
                ?? Array.Empty<FarmCombatPerspectiveApiModel>())
                .SingleOrDefault(value => value.ActorStableId == actor)
                ?.PerspectiveCode;
            if (perspective != FarmCombatPresentationCodes.FirstPersonPrecision
                && perspective != FarmCombatPresentationCodes.ThirdPersonAwareness)
                throw new InvalidOperationException("FarmCombatPerspectiveInvalid");
            if (!string.Equals(perspective, beat.AppliedPerspectiveCode,
                StringComparison.Ordinal))
                throw new InvalidOperationException("FarmCombatPerspectiveDrift");

            var firstPerson = perspective ==
                FarmCombatPresentationCodes.FirstPersonPrecision;
            return new FarmCombatPresentationFrame
            {
                BeatStableId = beat.BeatStableId,
                EncounterStableId = beat.EncounterStableId,
                ActorStableId = actor,
                PerspectiveCode = perspective,
                AttackPatternCode = beat.AttackPatternCode,
                ImpactOffsetMs = beat.ImpactOffsetMs,
                GuardWindowMs = beat.GuardWindowMs,
                CounterWindowMs = beat.CounterWindowMs,
                PerfectGuardWindowMs = beat.PerfectGuardWindowMs,
                PerfectCounterWindowMs = beat.PerfectCounterWindowMs,
                OwnsCombatInput = true,
                ShowFocusedThreatTelegraph = firstPerson,
                ShowAllThreats = !firstPerson,
                ShowAllies = !firstPerson,
                ShowFacilities = !firstPerson,
                PresentationOnly = true,
                ChangesWorldState = false,
            };
        }
    }

    /// <summary>
    /// Unity 입력을 서버 명령 초안으로 좁힌다. 결과 등급·피해·점수 필드는 의도적으로 없다.
    /// </summary>
    public sealed class FarmCombatReactionCommandDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string BeatStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string ReactionActionCode { get; set; } = string.Empty;
        public int ReactionOffsetMs { get; set; }
    }

    public sealed class FarmCombatPerspectiveCommandDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string ActorStableId { get; set; } = string.Empty;
        public string PerspectiveCode { get; set; } = string.Empty;
    }

    public sealed class FarmCombatBeatStartCommandDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string EncounterStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationFarmCombatInput,
        SsalddelCodeLayer.ClientAdapter,
        "서버 전투 상태를 공격 진입·방어·반격 명령 초안으로 변환한다.",
        StepKey = "unity.farm-combat-input",
        DependsOnStepKeys = new[] { "domain.farm-combat" },
        FlowOrder = 50,
        ExecutionStage = SsalddelCodeExecutionStage.Presentation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        Effects = SsalddelCodeEffect.UiStateMutation,
        Boundary = "피해·판정 등급·전술 효과를 계산하지 않고 안정 식별자와 입력 시각만 전달한다.")]
    public static class FarmCombatInputCommandFactory
    {
        public static FarmCombatPerspectiveCommandDraft CreatePerspective(
            FarmCombatStateApiModel state,
            string actorStableId,
            string perspectiveCode,
            string commandId)
        {
            ValidateStateAndIdentity(state, actorStableId, commandId);
            if (perspectiveCode != FarmCombatPresentationCodes.FirstPersonPrecision
                && perspectiveCode != FarmCombatPresentationCodes.ThirdPersonAwareness)
                throw new ArgumentException("FarmCombatPerspectiveInvalid",
                    nameof(perspectiveCode));
            if ((state.Beats ?? Array.Empty<FarmCombatBeatApiModel>())
                .Any(value => value.StateCode == FarmCombatPresentationCodes.Active))
                throw new InvalidOperationException("FarmCombatPerspectiveLocked");
            return new FarmCombatPerspectiveCommandDraft
            {
                CommandId = commandId.Trim(),
                ExpectedRevision = state.WorldRevision,
                ActorStableId = actorStableId.Trim(),
                PerspectiveCode = perspectiveCode,
            };
        }

        public static FarmCombatBeatStartCommandDraft CreateBeatStart(
            FarmCombatStateApiModel state,
            string actorStableId,
            string encounterStableId,
            string commandId)
        {
            ValidateStateAndIdentity(state, actorStableId, commandId);
            if (string.IsNullOrWhiteSpace(encounterStableId))
                throw new ArgumentException("FarmCombatEncounterRequired",
                    nameof(encounterStableId));
            if (!(state.Perspectives ?? Array.Empty<FarmCombatPerspectiveApiModel>())
                .Any(value => value.ActorStableId == actorStableId))
                throw new InvalidOperationException("FarmCombatPerspectiveRequired");
            if ((state.Beats ?? Array.Empty<FarmCombatBeatApiModel>())
                .Any(value => value.StateCode == FarmCombatPresentationCodes.Active))
                throw new InvalidOperationException("FarmCombatBeatAlreadyActive");
            if (!(state.Engagements ?? Array.Empty<FarmCombatEngagementApiModel>())
                .Any(value => value.EncounterStableId == encounterStableId
                    && value.StateCode == FarmCombatPresentationCodes.AwaitingCombat))
                throw new InvalidOperationException("FarmCombatEncounterNotReady");
            return new FarmCombatBeatStartCommandDraft
            {
                CommandId = commandId.Trim(),
                ExpectedRevision = state.WorldRevision,
                EncounterStableId = encounterStableId.Trim(),
                ActorStableId = actorStableId.Trim(),
            };
        }

        private static void ValidateStateAndIdentity(
            FarmCombatStateApiModel state,
            string actorStableId,
            string commandId)
        {
            if (state == null || !state.SimulationOnly || state.IsOperationalState
                || state.WorldRevision < 0)
                throw new InvalidOperationException("FarmCombatStateInvalid");
            if (string.IsNullOrWhiteSpace(actorStableId))
                throw new ArgumentException("FarmCombatActorRequired",
                    nameof(actorStableId));
            if (string.IsNullOrWhiteSpace(commandId))
                throw new ArgumentException("FarmCombatCommandIdRequired",
                    nameof(commandId));
        }
    }

    public sealed class FarmCombatBeatClock
    {
        private string _beatStableId = string.Empty;
        private double _observedAtMilliseconds;

        public string BeatStableId => _beatStableId;

        public bool Observe(string beatStableId, double nowMilliseconds)
        {
            if (string.IsNullOrWhiteSpace(beatStableId)
                || nowMilliseconds < 0d)
                throw new ArgumentException("FarmCombatBeatClockInputInvalid");
            if (string.Equals(_beatStableId, beatStableId,
                StringComparison.Ordinal)) return false;
            _beatStableId = beatStableId.Trim();
            _observedAtMilliseconds = nowMilliseconds;
            return true;
        }

        public int ElapsedMilliseconds(double nowMilliseconds, int maximum)
        {
            if (string.IsNullOrWhiteSpace(_beatStableId)
                || nowMilliseconds < _observedAtMilliseconds || maximum < 0)
                throw new InvalidOperationException("FarmCombatBeatClockUnavailable");
            return Math.Min(maximum,
                Math.Max(0, (int)Math.Round(nowMilliseconds
                    - _observedAtMilliseconds, MidpointRounding.AwayFromZero)));
        }

        public void Clear()
        {
            _beatStableId = string.Empty;
            _observedAtMilliseconds = 0d;
        }
    }

    public interface ISimulationFarmCombatAuthorityClient
    {
        Task<FarmCombatStateApiModel> LoadAsync(
            string sessionStableId,
            CancellationToken cancellationToken);

        Task<FarmCombatStateApiModel> ConfirmPerspectiveAsync(
            string sessionStableId,
            FarmCombatPerspectiveCommandDraft request,
            CancellationToken cancellationToken);

        Task<FarmCombatStateApiModel> StartBeatAsync(
            string sessionStableId,
            FarmCombatBeatStartCommandDraft request,
            CancellationToken cancellationToken);

        Task<FarmCombatStateApiModel> ConfirmReactionAsync(
            string sessionStableId,
            FarmCombatReactionCommandDraft request,
            CancellationToken cancellationToken);
    }

    public static class SimulationFarmCombatApiRoutes
    {
        public static string State(string sessionStableId)
            => Base(sessionStableId);

        public static string Perspective(string sessionStableId)
            => Base(sessionStableId) + "/combat/perspective/confirm";

        public static string StartBeat(string sessionStableId)
            => Base(sessionStableId) + "/combat/beats/start";

        public static string Reaction(string sessionStableId, string beatStableId)
            => Base(sessionStableId) + "/combat/beats/"
                + Required(beatStableId, nameof(beatStableId)) + "/react";

        private static string Base(string sessionStableId)
            => "api/simulation/v1/sessions/"
                + Required(sessionStableId, nameof(sessionStableId))
                + "/farm-survival";

        private static string Required(string value, string name)
            => !string.IsNullOrWhiteSpace(value)
                ? Uri.EscapeDataString(value.Trim())
                : throw new ArgumentException("FarmCombatRouteValueRequired", name);
    }

    public static class FarmCombatReactionCommandFactory
    {
        public static FarmCombatReactionCommandDraft Create(
            FarmCombatPresentationFrame frame,
            long expectedRevision,
            string commandId,
            string actionCode,
            int reactionOffsetMs)
        {
            if (frame == null || !frame.PresentationOnly || frame.ChangesWorldState
                || !frame.OwnsCombatInput)
                throw new InvalidOperationException("FarmCombatFrameInvalid");
            if (expectedRevision < 0 || string.IsNullOrWhiteSpace(commandId))
                throw new ArgumentException("FarmCombatCommandInvalid");
            if (actionCode != FarmCombatPresentationCodes.Guard
                && actionCode != FarmCombatPresentationCodes.Counter)
                throw new ArgumentException("FarmCombatActionInvalid",
                    nameof(actionCode));
            if (reactionOffsetMs < 0 || reactionOffsetMs > 1600)
                throw new ArgumentOutOfRangeException(nameof(reactionOffsetMs));
            return new FarmCombatReactionCommandDraft
            {
                CommandId = commandId.Trim(),
                ExpectedRevision = expectedRevision,
                BeatStableId = frame.BeatStableId,
                ActorStableId = frame.ActorStableId,
                ReactionActionCode = actionCode,
                ReactionOffsetMs = reactionOffsetMs,
            };
        }
    }

    public sealed class FarmTacticalOrderPresentationFrame
    {
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public int ClosesWorldTick { get; set; }
        public string[] AllowedOrderCodes { get; set; } = Array.Empty<string>();
        public string[] AvailableOpportunityStableIds { get; set; }
            = Array.Empty<string>();
        public string[] HighlightSquadStableIds { get; set; }
            = Array.Empty<string>();
        public bool SuggestThirdPersonTransition { get; set; }
        public bool ForceThirdPersonTransition { get; set; }
        public string SuggestedPerspectiveCode { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
        public bool ChangesWorldState { get; set; }
    }

    public sealed class FarmTacticalSquadMovementPresentationFrame
    {
        public string SquadStableId { get; set; } = string.Empty;
        public string SideCode { get; set; } = string.Empty;
        public string TargetPositionCode { get; set; } = string.Empty;
        public string FormationCode { get; set; } = string.Empty;
        public string MovementIntentCode { get; set; } = string.Empty;
        public int CanonicalMemberCount { get; set; }
        public int DisplayedMemberCount { get; set; }
        public string[] DisplayMemberStableIds { get; set; } = Array.Empty<string>();
        public bool PresentationOnly { get; set; } = true;
    }

    /// <summary>
    /// 서버가 다음 WorldTick에 확정한 전술 결과를 분대 이동 표현으로만 좁힌다.
    /// 피해·점수·승패를 다시 계산하지 않고 대형과 이동 의도만 선택한다.
    /// </summary>
    public sealed class FarmTacticalMovementPresentationFrame
    {
        public long WorldRevision { get; set; }
        public string ResolutionStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public FarmTacticalSquadMovementPresentationFrame[] Squads { get; set; }
            = Array.Empty<FarmTacticalSquadMovementPresentationFrame>();
        public bool PresentationOnly { get; set; } = true;
        public bool ChangesWorldState { get; set; }
    }

    public sealed class FarmTacticalMovementPresentationMapper
    {
        private const int MaximumDisplayedMembersPerSquad = 6;

        public FarmTacticalMovementPresentationFrame MapLatest(
            FarmCombatStateApiModel source)
        {
            ValidateBoundary(source);
            var resolution = (source.Tactical.Resolutions
                    ?? Array.Empty<FarmTacticalResolutionApiModel>())
                .OrderByDescending(value => value.ResolvedWorldTick)
                .ThenByDescending(value => value.ResolutionStableId,
                    StringComparer.Ordinal)
                .FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "FarmTacticalResolvedResultNotFound");
            if (string.IsNullOrWhiteSpace(resolution.ResolutionStableId)
                || string.IsNullOrWhiteSpace(resolution.FrontStableId)
                || !IsKnownOrder(resolution.OrderCode)
                || !IsKnownPosition(resolution.FrontPositionCode))
                throw new InvalidOperationException(
                    "FarmTacticalResolvedResultInvalid");

            var matchingOrder = (source.Tactical.Orders
                    ?? Array.Empty<FarmTacticalOrderApiModel>())
                .SingleOrDefault(value => value.OrderStableId ==
                    resolution.OrderStableId);
            if (matchingOrder == null
                || matchingOrder.FrontStableId != resolution.FrontStableId
                || matchingOrder.OrderCode != resolution.OrderCode
                || matchingOrder.StateCode != FarmCombatPresentationCodes.Resolved)
                throw new InvalidOperationException(
                    "FarmTacticalResolvedOrderDrift");

            var squads = (source.Tactical.Squads
                    ?? Array.Empty<FarmTacticalSquadApiModel>())
                .Where(value => value.FrontStableId ==
                    resolution.FrontStableId)
                .OrderBy(value => value.SideCode, StringComparer.Ordinal)
                .ThenBy(value => value.SquadStableId, StringComparer.Ordinal)
                .Select(value => MapSquad(value, resolution.OrderCode))
                .ToArray();
            if (squads.Length == 0
                || squads.Select(value => value.SquadStableId)
                    .Distinct(StringComparer.Ordinal).Count() != squads.Length)
                throw new InvalidOperationException(
                    "FarmTacticalResolvedSquadsInvalid");

            return new FarmTacticalMovementPresentationFrame
            {
                WorldRevision = source.WorldRevision,
                ResolutionStableId = resolution.ResolutionStableId,
                FrontStableId = resolution.FrontStableId,
                OrderCode = resolution.OrderCode,
                Squads = squads,
                PresentationOnly = true,
                ChangesWorldState = false,
            };
        }

        private static FarmTacticalSquadMovementPresentationFrame MapSquad(
            FarmTacticalSquadApiModel source,
            string orderCode)
        {
            if (string.IsNullOrWhiteSpace(source.SquadStableId)
                || (source.SideCode != FarmCombatPresentationCodes.Allied
                    && source.SideCode != FarmCombatPresentationCodes.Hostile)
                || !IsKnownPosition(source.PositionCode)
                || source.MemberCount < 0 || source.CombatStrength < 0)
                throw new InvalidOperationException(
                    "FarmTacticalSquadMovementInvalid");

            var displayed = Math.Min(source.MemberCount,
                MaximumDisplayedMembersPerSquad);
            var canonicalIds = source.MemberActorStableIds
                ?? Array.Empty<string>();
            var displayIds = Enumerable.Range(0, displayed)
                .Select(index => index < canonicalIds.Length
                    && !string.IsNullOrWhiteSpace(canonicalIds[index])
                        ? canonicalIds[index].Trim()
                        : source.SquadStableId + ":visual-member:"
                            + (index + 1).ToString("D2"))
                .ToArray();

            var allied = source.SideCode ==
                FarmCombatPresentationCodes.Allied;
            return new FarmTacticalSquadMovementPresentationFrame
            {
                SquadStableId = source.SquadStableId,
                SideCode = source.SideCode,
                TargetPositionCode = source.PositionCode,
                FormationCode = allied
                    ? FormationFor(orderCode)
                    : FarmCombatPresentationCodes.LineFormation,
                MovementIntentCode = allied
                    ? MovementFor(orderCode)
                    : source.CombatStrength == 0
                        ? FarmCombatPresentationCodes.StaggerMovement
                        : FarmCombatPresentationCodes.GuardMovement,
                CanonicalMemberCount = source.MemberCount,
                DisplayedMemberCount = displayed,
                DisplayMemberStableIds = displayIds,
                PresentationOnly = true,
            };
        }

        private static string FormationFor(string orderCode)
            => orderCode switch
            {
                FarmCombatPresentationCodes.AdvanceAndAttack =>
                    FarmCombatPresentationCodes.WedgeFormation,
                FarmCombatPresentationCodes.TacticalRetreat =>
                    FarmCombatPresentationCodes.ColumnFormation,
                _ => FarmCombatPresentationCodes.LineFormation,
            };

        private static string MovementFor(string orderCode)
            => orderCode == FarmCombatPresentationCodes.HoldFormation
                ? FarmCombatPresentationCodes.GuardMovement
                : FarmCombatPresentationCodes.RunMovement;

        private static bool IsKnownOrder(string value)
            => value == FarmCombatPresentationCodes.AdvanceAndAttack
                || value == FarmCombatPresentationCodes.HoldFormation
                || value == FarmCombatPresentationCodes.TacticalRetreat;

        private static bool IsKnownPosition(string value)
            => value == FarmCombatPresentationCodes.Perimeter
                || value == FarmCombatPresentationCodes.Forward
                || value == FarmCombatPresentationCodes.InnerFarm;

        private static void ValidateBoundary(FarmCombatStateApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.WorldRevision < 0 || !source.SimulationOnly
                || source.IsOperationalState || source.Tactical == null
                || !source.Tactical.SimulationOnly
                || source.Tactical.IsOperationalState)
                throw new InvalidOperationException(
                    "FarmTacticalMovementBoundaryInvalid");
        }
    }

    /// <summary>
    /// 서버가 연 명령창과 주변 전선만 3인칭 전술 제안으로 투영한다.
    /// 카메라는 강제로 바꾸지 않고 사용자가 제안을 수락했을 때만 기존 곡선 전환을 사용한다.
    /// </summary>
    public sealed class FarmTacticalOrderPresentationMapper
    {
        public FarmTacticalOrderPresentationFrame Map(
            FarmCombatStateApiModel source,
            string actorStableId)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.WorldRevision < 0 || !source.SimulationOnly
                || source.IsOperationalState || source.Tactical == null
                || !source.Tactical.SimulationOnly
                || source.Tactical.IsOperationalState)
                throw new InvalidOperationException("FarmTacticalBoundaryInvalid");
            if (string.IsNullOrWhiteSpace(actorStableId))
                throw new ArgumentException("FarmTacticalActorMissing",
                    nameof(actorStableId));

            var actor = actorStableId.Trim();
            var window = (source.Tactical.OrderWindows
                ?? Array.Empty<FarmTacticalOrderWindowApiModel>())
                .SingleOrDefault(value => value.StateCode ==
                    FarmCombatPresentationCodes.Open
                    && value.AuthorizedActorStableId == actor)
                ?? throw new InvalidOperationException(
                    "FarmTacticalOpenOrderWindowNotFound");
            var front = (source.Tactical.Fronts
                ?? Array.Empty<FarmTacticalFrontApiModel>())
                .SingleOrDefault(value => value.FrontStableId ==
                    window.FrontStableId)
                ?? throw new InvalidOperationException(
                    "FarmTacticalFrontNotFound");
            if (front.EncounterStableId != window.EncounterStableId)
                throw new InvalidOperationException("FarmTacticalFrontDrift");

            var allowed = window.AllowedOrderCodes
                ?? Array.Empty<string>();
            if (allowed.Length == 0 || allowed.Any(value =>
                value != FarmCombatPresentationCodes.AdvanceAndAttack
                && value != FarmCombatPresentationCodes.HoldFormation
                && value != FarmCombatPresentationCodes.TacticalRetreat))
                throw new InvalidOperationException(
                    "FarmTacticalAllowedOrderInvalid");

            return new FarmTacticalOrderPresentationFrame
            {
                OrderWindowStableId = window.OrderWindowStableId,
                FrontStableId = front.FrontStableId,
                ActorStableId = actor,
                ClosesWorldTick = window.ClosesWorldTick,
                AllowedOrderCodes = allowed.ToArray(),
                AvailableOpportunityStableIds = (source.Tactical.Opportunities
                    ?? Array.Empty<FarmTacticalOpportunityApiModel>())
                    .Where(value => value.FrontStableId == front.FrontStableId
                        && value.EarningActorStableId == actor
                        && value.StateCode ==
                            FarmCombatPresentationCodes.Available)
                    .Select(value => value.OpportunityStableId).ToArray(),
                HighlightSquadStableIds = (source.Tactical.Squads
                    ?? Array.Empty<FarmTacticalSquadApiModel>())
                    .Where(value => value.FrontStableId == front.FrontStableId)
                    .Select(value => value.SquadStableId).ToArray(),
                SuggestThirdPersonTransition = true,
                ForceThirdPersonTransition = false,
                SuggestedPerspectiveCode =
                    FarmCombatPresentationCodes.ThirdPersonAwareness,
                PresentationOnly = true,
                ChangesWorldState = false,
            };
        }
    }

    public sealed class FarmTacticalOrderPreviewDraft
    {
        public long ExpectedRevision { get; set; }
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string OpportunityStableId { get; set; } = string.Empty;
    }

    public sealed class FarmTacticalOrderConfirmDraft
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string OrderWindowStableId { get; set; } = string.Empty;
        public string FrontStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string OrderCode { get; set; } = string.Empty;
        public string OpportunityStableId { get; set; } = string.Empty;
    }

    public static class FarmTacticalOrderCommandFactory
    {
        public static FarmTacticalOrderPreviewDraft CreatePreview(
            FarmTacticalOrderPresentationFrame frame,
            long expectedRevision,
            string orderCode,
            string opportunityStableId)
        {
            Validate(frame, expectedRevision, orderCode, opportunityStableId);
            return new FarmTacticalOrderPreviewDraft
            {
                ExpectedRevision = expectedRevision,
                OrderWindowStableId = frame.OrderWindowStableId,
                FrontStableId = frame.FrontStableId,
                ActorStableId = frame.ActorStableId,
                OrderCode = orderCode,
                OpportunityStableId = opportunityStableId ?? string.Empty,
            };
        }

        public static FarmTacticalOrderConfirmDraft CreateConfirm(
            FarmTacticalOrderPresentationFrame frame,
            long expectedRevision,
            string commandId,
            string orderCode,
            string opportunityStableId)
        {
            Validate(frame, expectedRevision, orderCode, opportunityStableId);
            if (string.IsNullOrWhiteSpace(commandId))
                throw new ArgumentException("FarmTacticalCommandMissing",
                    nameof(commandId));
            return new FarmTacticalOrderConfirmDraft
            {
                CommandId = commandId.Trim(),
                ExpectedRevision = expectedRevision,
                OrderWindowStableId = frame.OrderWindowStableId,
                FrontStableId = frame.FrontStableId,
                ActorStableId = frame.ActorStableId,
                OrderCode = orderCode,
                OpportunityStableId = opportunityStableId ?? string.Empty,
            };
        }

        private static void Validate(
            FarmTacticalOrderPresentationFrame frame,
            long expectedRevision,
            string orderCode,
            string opportunityStableId)
        {
            if (frame == null || !frame.PresentationOnly
                || frame.ChangesWorldState || expectedRevision < 0)
                throw new InvalidOperationException("FarmTacticalFrameInvalid");
            if (!frame.AllowedOrderCodes.Contains(orderCode))
                throw new ArgumentException("FarmTacticalOrderInvalid",
                    nameof(orderCode));
            if (!string.IsNullOrEmpty(opportunityStableId)
                && !frame.AvailableOpportunityStableIds.Contains(
                    opportunityStableId))
                throw new ArgumentException("FarmTacticalOpportunityInvalid",
                    nameof(opportunityStableId));
        }
    }
}
