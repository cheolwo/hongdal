using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// 역할을 직업으로 고정하지 않고 팀 카드 장착과 현재 활동에서 파생한다.
    /// 카드는 비물리적 팀 능력 배치권이며 활동 중에만 잠긴다.
    /// </summary>
    public sealed class SimulationTeamRoleCardState
    {
        private readonly object gate = new();
        private readonly HashSet<string> members;
        private readonly Dictionary<string, CardState> cards;
        private readonly Dictionary<string, ActivityState> activities =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, CombatLoadoutState> combatLoadouts =
            new(StringComparer.Ordinal);
        private readonly Dictionary<Guid, AppliedCommand> applied = new();

        public SimulationTeamRoleCardState(
            SimulationTeamRoleCardInitialState initial)
        {
            ValidateInitial(initial);
            SessionStableId = initial.SessionStableId.Trim();
            TeamStableId = initial.TeamStableId.Trim();
            TeamPolicyRevision = initial.TeamPolicyRevision;
            RuleRevision = SimulationTeamRoleCardCodes.RuleRevision;
            members = new HashSet<string>(initial.MemberActorStableIds
                .Select(value => value.Trim()), StringComparer.Ordinal);
            cards = initial.Cards.ToDictionary(value => value.CardCopyStableId.Trim(),
                value => new CardState(value), StringComparer.Ordinal);
            ValidateSlots();
            InitializeCombatLoadouts(initial.CombatLoadouts);
        }

        public string SessionStableId { get; }
        public string TeamStableId { get; }
        public long TeamPolicyRevision { get; }
        public string RuleRevision { get; }
        public long Revision { get; private set; }

        public SimulationTeamRoleCardStateSnapshot Snapshot()
        {
            lock (gate) return CreateSnapshot();
        }

        public SimulationTeamRoleCardStateSnapshot Equip(
            SimulationTeamRoleCardEquipRequest request)
        {
            ValidateEquip(request);
            lock (gate)
            {
                var key = EquipKey(request);
                if (TryReplay(request.ClientRequestId, key, out var replay))
                    return replay;
                EnsureRevision(request.ExpectedRevision);
                EnsurePolicyRevision(request.ExpectedTeamPolicyRevision);
                EnsureMember(request.RequestingActorStableId);
                EnsureMember(request.TargetActorStableId);
                var card = FindCard(request.CardCopyStableId);
                if (card.IsLocked)
                    throw new SimulationConflictException(
                        "SimulationTeamRoleCardActiveLock");

                var slot = request.SlotCode.Trim();
                var target = request.TargetActorStableId.Trim();
                var displaced = cards.Values.FirstOrDefault(value =>
                    !ReferenceEquals(value, card)
                    && value.EquippedActorStableId == target
                    && value.SlotCode == slot);
                if (displaced?.IsLocked == true)
                    throw new SimulationConflictException(
                        "SimulationTeamRoleCardTargetSlotActiveLock");
                if (displaced != null)
                {
                    displaced.EquippedActorStableId = string.Empty;
                    displaced.SlotCode = string.Empty;
                }
                card.EquippedActorStableId = target;
                card.SlotCode = slot;
                Revision++;
                return Remember(request.ClientRequestId, key);
            }
        }

        public SimulationTeamRoleCardStateSnapshot StartActivity(
            SimulationTeamActivityStartRequest request)
        {
            ValidateStart(request);
            lock (gate)
            {
                var key = StartKey(request);
                if (TryReplay(request.ClientRequestId, key, out var replay))
                    return replay;
                EnsureRevision(request.ExpectedRevision);
                EnsurePolicyRevision(request.ExpectedTeamPolicyRevision);
                EnsureMember(request.ActorStableId);
                var actor = request.ActorStableId.Trim();
                if (activities.Values.Any(value => value.ActorStableId == actor))
                    throw new SimulationConflictException(
                        "SimulationTeamActorActivityAlreadyActive");
                var card = FindCard(request.CardCopyStableId);
                if (card.IsLocked)
                    throw new SimulationConflictException(
                        "SimulationTeamRoleCardActiveLock");
                if (card.EquippedActorStableId != actor)
                    throw new SimulationConflictException(
                        "SimulationTeamRoleCardNotEquippedByActor");
                var role = request.ActivityRoleCode.Trim();
                if (!card.ActivityRoleCodes.Contains(role, StringComparer.Ordinal))
                    throw new SimulationContractException(
                        "SimulationTeamRoleCardActivityUnsupported");

                var activity = new ActivityState
                {
                    ActivityStableId = request.ActivityStableId.Trim(),
                    ActorStableId = actor,
                    CardCopyStableId = card.CardCopyStableId,
                    ActivityRoleCode = role,
                    LocationStableId = request.LocationStableId.Trim(),
                };
                activities.Add(activity.ActivityStableId, activity);
                card.LockedActivityStableId = activity.ActivityStableId;
                Revision++;
                return Remember(request.ClientRequestId, key);
            }
        }

        public SimulationTeamRoleCardStateSnapshot EndActivity(
            SimulationTeamActivityEndRequest request)
        {
            ValidateEnd(request);
            lock (gate)
            {
                var key = EndKey(request);
                if (TryReplay(request.ClientRequestId, key, out var replay))
                    return replay;
                EnsureRevision(request.ExpectedRevision);
                EnsureMember(request.ActorStableId);
                var activityId = request.ActivityStableId.Trim();
                if (!activities.TryGetValue(activityId, out var activity))
                    throw new SimulationNotFoundException(
                        "SimulationTeamActivityNotFound");
                if (activity.ActorStableId != request.ActorStableId.Trim())
                    throw new SimulationConflictException(
                        "SimulationTeamActivityActorMismatch");
                activities.Remove(activityId);
                FindCard(activity.CardCopyStableId).LockedActivityStableId = string.Empty;
                Revision++;
                return Remember(request.ClientRequestId, key);
            }
        }

        public SimulationTeamRoleCardStateSnapshot SetCombatLoadout(
            SimulationCombatCardLoadoutSetRequest request)
        {
            ValidateCombatLoadout(request);
            lock (gate)
            {
                var key = CombatLoadoutKey(request);
                if (TryReplay(request.ClientRequestId, key, out var replay))
                    return replay;
                EnsureRevision(request.ExpectedRevision);
                EnsurePolicyRevision(request.ExpectedTeamPolicyRevision);
                EnsureMember(request.RequestingActorStableId);
                EnsureMember(request.TargetActorStableId);
                var actor = request.TargetActorStableId.Trim();
                var slots = request.Slots.Select(value =>
                {
                    var card = FindCard(value.CardCopyStableId);
                    if (card.IsLocked)
                        throw new SimulationConflictException(
                            "SimulationCombatCardLoadoutActiveLock");
                    return new SimulationCombatCardLoadoutSlotSnapshot
                    {
                        SlotCode = value.SlotCode.Trim(),
                        CardCopyStableId = card.CardCopyStableId,
                    };
                }).ToArray();
                combatLoadouts[LoadoutKey(actor, request.CombatControlModeCode)] =
                    new CombatLoadoutState(actor,
                        request.CombatControlModeCode.Trim(), slots);
                Revision++;
                return Remember(request.ClientRequestId, key);
            }
        }

        private SimulationTeamRoleCardStateSnapshot CreateSnapshot()
        {
            var cardSnapshots = cards.Values
                .OrderBy(value => value.CardCopyStableId, StringComparer.Ordinal)
                .Select(value => value.Snapshot()).ToArray();
            var activitySnapshots = activities.Values
                .OrderBy(value => value.ActivityStableId, StringComparer.Ordinal)
                .Select(value => value.Snapshot()).ToArray();
            return new SimulationTeamRoleCardStateSnapshot
            {
                SessionStableId = SessionStableId,
                TeamStableId = TeamStableId,
                Revision = Revision,
                TeamPolicyRevision = TeamPolicyRevision,
                RuleRevision = RuleRevision,
                MemberActorStableIds = members.OrderBy(value => value,
                    StringComparer.Ordinal).ToArray(),
                Cards = cardSnapshots,
                ActiveActivities = activitySnapshots,
                MemberRoles = members.OrderBy(value => value, StringComparer.Ordinal)
                    .Select(actor => CreateRole(actor, cardSnapshots,
                        activitySnapshots)).ToArray(),
                CombatLoadouts = combatLoadouts.Values
                    .OrderBy(value => value.ActorStableId, StringComparer.Ordinal)
                    .ThenBy(value => value.CombatControlModeCode,
                        StringComparer.Ordinal)
                    .Select(value => value.Snapshot()).ToArray(),
                SupportsRemoteEquip = true,
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private static SimulationTeamMemberRoleProjection CreateRole(
            string actor,
            SimulationTeamRoleCardSnapshot[] cardSnapshots,
            SimulationTeamActivityAssignmentSnapshot[] activitySnapshots)
        {
            var activity = activitySnapshots.SingleOrDefault(value =>
                value.ActorStableId == actor);
            return new SimulationTeamMemberRoleProjection
            {
                ActorStableId = actor,
                CurrentRoleCode = activity?.ActivityRoleCode
                    ?? SimulationTeamRoleCardCodes.Idle,
                ActivityStableId = activity?.ActivityStableId ?? string.Empty,
                EquippedCardCopyStableIds = cardSnapshots
                    .Where(value => value.EquippedActorStableId == actor)
                    .Select(value => value.CardCopyStableId).ToArray(),
                IsPermanentProfession = false,
            };
        }

        private bool TryReplay(Guid requestId, string key,
            out SimulationTeamRoleCardStateSnapshot snapshot)
        {
            if (applied.TryGetValue(requestId, out var command))
            {
                if (command.PayloadKey != key)
                    throw new SimulationConflictException(
                        "SimulationTeamRoleCardClientRequestConflict");
                snapshot = Clone(command.Snapshot);
                return true;
            }
            snapshot = null!;
            return false;
        }

        private SimulationTeamRoleCardStateSnapshot Remember(Guid requestId,
            string key)
        {
            var snapshot = CreateSnapshot();
            applied.Add(requestId, new AppliedCommand(key, Clone(snapshot)));
            return snapshot;
        }

        private CardState FindCard(string cardCopyStableId)
            => cards.TryGetValue(cardCopyStableId.Trim(), out var card)
                ? card : throw new SimulationNotFoundException(
                    "SimulationTeamRoleCardNotFound");

        private void EnsureMember(string actorStableId)
        {
            if (!members.Contains(actorStableId.Trim()))
                throw new SimulationConflictException(
                    "SimulationTeamActorNotMember");
        }

        private void EnsureRevision(long expected)
        {
            if (expected != Revision)
                throw new SimulationConflictException(
                    "SimulationExpectedRevisionMismatch");
        }

        private void EnsurePolicyRevision(long expected)
        {
            if (expected != TeamPolicyRevision)
                throw new SimulationConflictException(
                    "SimulationTeamPolicyRevisionMismatch");
        }

        private void ValidateSlots()
        {
            var duplicates = cards.Values
                .Where(value => value.EquippedActorStableId.Length > 0)
                .GroupBy(value => value.EquippedActorStableId + "|" + value.SlotCode,
                    StringComparer.Ordinal)
                .Any(group => group.Count() > 1);
            if (duplicates)
                throw new SimulationContractException(
                    "SimulationTeamRoleCardSlotDuplicated");
        }

        private void InitializeCombatLoadouts(
            SimulationCombatCardLoadoutInitialState[]? initial)
        {
            var values = initial ?? Array.Empty<SimulationCombatCardLoadoutInitialState>();
            if (values.Length > 0)
            {
                foreach (var value in values)
                {
                    var slots = value.Slots ?? Array.Empty<SimulationCombatCardLoadoutSlotSnapshot>();
                    combatLoadouts.Add(LoadoutKey(value.ActorStableId,
                        value.CombatControlModeCode), new CombatLoadoutState(
                        value.ActorStableId.Trim(), value.CombatControlModeCode.Trim(),
                        slots.Select(CloneSlot).ToArray()));
                }
                return;
            }

            // r1 저장자료에는 전투 편성이 없었다. 기존 업무 장착을 두 대체
            // 조작 방식의 초기 편성으로 한 번 승격해 기존 전투 효과를 보존한다.
            foreach (var actor in members.OrderBy(value => value, StringComparer.Ordinal))
            {
                var slots = cards.Values
                    .Where(value => value.EquippedActorStableId == actor
                        && ValidSlot(value.SlotCode))
                    .OrderBy(value => value.SlotCode, StringComparer.Ordinal)
                    .Select(value => new SimulationCombatCardLoadoutSlotSnapshot
                    {
                        SlotCode = value.SlotCode,
                        CardCopyStableId = value.CardCopyStableId,
                    }).ToArray();
                foreach (var mode in new[]
                         {
                             SimulationTeamRoleCardCodes.DirectAction,
                             SimulationTeamRoleCardCodes.TacticalCommand,
                         })
                    combatLoadouts.Add(LoadoutKey(actor, mode),
                        new CombatLoadoutState(actor, mode,
                            slots.Select(CloneSlot).ToArray()));
            }
        }

        public static void ValidateInitial(SimulationTeamRoleCardInitialState initial)
        {
            if (initial == null || string.IsNullOrWhiteSpace(initial.SessionStableId)
                || string.IsNullOrWhiteSpace(initial.TeamStableId)
                || initial.TeamPolicyRevision < 0
                || (initial.RuleRevision != SimulationTeamRoleCardCodes.RuleRevision
                    && initial.RuleRevision != "team-role-card-loadout.r1")
                || initial.MemberActorStableIds == null
                || initial.Cards == null || initial.MemberActorStableIds.Length == 0
                || initial.MemberActorStableIds.Any(string.IsNullOrWhiteSpace)
                || initial.MemberActorStableIds.Distinct(StringComparer.Ordinal).Count()
                    != initial.MemberActorStableIds.Length
                || initial.Cards.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.CardCopyStableId)
                    || string.IsNullOrWhiteSpace(value.CardDefinitionStableId)
                    || string.IsNullOrWhiteSpace(value.Title)
                    || value.ActivityRoleCodes == null
                    || value.ActivityRoleCodes.Length == 0
                    || value.ActivityRoleCodes.Any(string.IsNullOrWhiteSpace)
                    || (!string.IsNullOrEmpty(value.EquippedActorStableId)
                        && !initial.MemberActorStableIds.Contains(
                            value.EquippedActorStableId, StringComparer.Ordinal))
                    || (!string.IsNullOrEmpty(value.EquippedActorStableId)
                        && !ValidSlot(value.SlotCode)))
                || initial.Cards.Select(value => value.CardCopyStableId)
                    .Distinct(StringComparer.Ordinal).Count() != initial.Cards.Length)
                throw new SimulationContractException(
                    "SimulationTeamRoleCardInitialStateInvalid");
            ValidateInitialCombatLoadouts(initial);
        }

        public static void ValidateEquip(SimulationTeamRoleCardEquipRequest request)
        {
            if (request == null || request.ClientRequestId == Guid.Empty
                || request.ExpectedRevision < 0
                || request.ExpectedTeamPolicyRevision < 0
                || string.IsNullOrWhiteSpace(request.RequestingActorStableId)
                || string.IsNullOrWhiteSpace(request.TargetActorStableId)
                || string.IsNullOrWhiteSpace(request.CardCopyStableId)
                || !ValidSlot(request.SlotCode))
                throw new SimulationContractException(
                    "SimulationTeamRoleCardEquipRequestInvalid");
        }

        public static void ValidateStart(SimulationTeamActivityStartRequest request)
        {
            if (request == null || request.ClientRequestId == Guid.Empty
                || request.ExpectedRevision < 0
                || request.ExpectedTeamPolicyRevision < 0
                || string.IsNullOrWhiteSpace(request.ActorStableId)
                || string.IsNullOrWhiteSpace(request.CardCopyStableId)
                || string.IsNullOrWhiteSpace(request.ActivityRoleCode)
                || string.IsNullOrWhiteSpace(request.ActivityStableId)
                || string.IsNullOrWhiteSpace(request.LocationStableId))
                throw new SimulationContractException(
                    "SimulationTeamActivityStartRequestInvalid");
        }

        public static void ValidateEnd(SimulationTeamActivityEndRequest request)
        {
            if (request == null || request.ClientRequestId == Guid.Empty
                || request.ExpectedRevision < 0
                || string.IsNullOrWhiteSpace(request.ActorStableId)
                || string.IsNullOrWhiteSpace(request.ActivityStableId))
                throw new SimulationContractException(
                    "SimulationTeamActivityEndRequestInvalid");
        }

        public static void ValidateCombatLoadout(
            SimulationCombatCardLoadoutSetRequest request)
        {
            if (request == null || request.ClientRequestId == Guid.Empty
                || request.ExpectedRevision < 0
                || request.ExpectedTeamPolicyRevision < 0
                || string.IsNullOrWhiteSpace(request.RequestingActorStableId)
                || string.IsNullOrWhiteSpace(request.TargetActorStableId)
                || !ValidControlMode(request.CombatControlModeCode)
                || request.Slots == null || request.Slots.Length >
                    (request.CombatControlModeCode ==
                        SimulationTeamRoleCardCodes.ObserverOperation ? 3 : 2)
                || request.Slots.Any(value => value == null
                    || !ValidSlot(value.SlotCode)
                    || string.IsNullOrWhiteSpace(value.CardCopyStableId))
                || request.Slots.Select(value => value.SlotCode)
                    .Distinct(StringComparer.Ordinal).Count() != request.Slots.Length
                || request.Slots.Select(value => value.CardCopyStableId)
                    .Distinct(StringComparer.Ordinal).Count() != request.Slots.Length)
                throw new SimulationContractException(
                    "SimulationCombatCardLoadoutRequestInvalid");
        }

        private static bool ValidSlot(string slot)
            => slot == SimulationTeamRoleCardCodes.Primary
               || slot == SimulationTeamRoleCardCodes.Support
               || slot == SimulationTeamRoleCardCodes.ObserverTactic
               || slot == SimulationTeamRoleCardCodes.ObserverSupport
               || slot == SimulationTeamRoleCardCodes.ObserverEmergency;

        private static bool ValidControlMode(string mode)
            => mode == SimulationTeamRoleCardCodes.DirectAction
               || mode == SimulationTeamRoleCardCodes.TacticalCommand
               || mode == SimulationTeamRoleCardCodes.ObserverOperation;

        private static void ValidateInitialCombatLoadouts(
            SimulationTeamRoleCardInitialState initial)
        {
            var values = initial.CombatLoadouts
                ?? Array.Empty<SimulationCombatCardLoadoutInitialState>();
            if (values.Any(value => value == null
                    || !initial.MemberActorStableIds.Contains(value.ActorStableId,
                        StringComparer.Ordinal)
                    || !ValidControlMode(value.CombatControlModeCode)
                    || value.Slots == null || value.Slots.Length >
                        (value.CombatControlModeCode ==
                            SimulationTeamRoleCardCodes.ObserverOperation ? 3 : 2)
                    || value.Slots.Any(slot => slot == null
                        || !ValidSlot(slot.SlotCode)
                        || !initial.Cards.Any(card => card.CardCopyStableId ==
                            slot.CardCopyStableId))
                    || value.Slots.Select(slot => slot.SlotCode)
                        .Distinct(StringComparer.Ordinal).Count() != value.Slots.Length
                    || value.Slots.Select(slot => slot.CardCopyStableId)
                        .Distinct(StringComparer.Ordinal).Count() != value.Slots.Length)
                || values.GroupBy(value => LoadoutKey(value.ActorStableId,
                        value.CombatControlModeCode), StringComparer.Ordinal)
                    .Any(group => group.Count() > 1))
                throw new SimulationContractException(
                    "SimulationCombatCardLoadoutInitialStateInvalid");
        }

        private static string EquipKey(SimulationTeamRoleCardEquipRequest value)
            => string.Join("|", value.ExpectedRevision,
                value.ExpectedTeamPolicyRevision, value.RequestingActorStableId.Trim(),
                value.TargetActorStableId.Trim(), value.CardCopyStableId.Trim(),
                value.SlotCode.Trim());

        private static string StartKey(SimulationTeamActivityStartRequest value)
            => string.Join("|", value.ExpectedRevision,
                value.ExpectedTeamPolicyRevision, value.ActorStableId.Trim(),
                value.CardCopyStableId.Trim(), value.ActivityRoleCode.Trim(),
                value.ActivityStableId.Trim(), value.LocationStableId.Trim());

        private static string EndKey(SimulationTeamActivityEndRequest value)
            => string.Join("|", value.ExpectedRevision, value.ActorStableId.Trim(),
                value.ActivityStableId.Trim());

        private static string CombatLoadoutKey(
            SimulationCombatCardLoadoutSetRequest value)
            => string.Join("|", value.ExpectedRevision,
                value.ExpectedTeamPolicyRevision,
                value.RequestingActorStableId.Trim(),
                value.TargetActorStableId.Trim(),
                value.CombatControlModeCode.Trim(),
                string.Join(";", value.Slots
                    .OrderBy(slot => slot.SlotCode, StringComparer.Ordinal)
                    .Select(slot => slot.SlotCode.Trim() + ":"
                        + slot.CardCopyStableId.Trim())));

        private static string LoadoutKey(string actor, string mode)
            => actor.Trim() + "|" + mode.Trim();

        private static SimulationCombatCardLoadoutSlotSnapshot CloneSlot(
            SimulationCombatCardLoadoutSlotSnapshot value) => new()
            {
                SlotCode = value.SlotCode,
                CardCopyStableId = value.CardCopyStableId,
            };

        private static SimulationTeamRoleCardStateSnapshot Clone(
            SimulationTeamRoleCardStateSnapshot source)
            => new()
            {
                SessionStableId = source.SessionStableId,
                TeamStableId = source.TeamStableId,
                Revision = source.Revision,
                TeamPolicyRevision = source.TeamPolicyRevision,
                RuleRevision = source.RuleRevision,
                MemberActorStableIds = source.MemberActorStableIds.ToArray(),
                Cards = source.Cards.Select(value => new SimulationTeamRoleCardSnapshot
                {
                    CardCopyStableId = value.CardCopyStableId,
                    CardDefinitionStableId = value.CardDefinitionStableId,
                    Title = value.Title,
                    ActivityRoleCodes = value.ActivityRoleCodes.ToArray(),
                    EquippedActorStableId = value.EquippedActorStableId,
                    SlotCode = value.SlotCode,
                    LockedActivityStableId = value.LockedActivityStableId,
                    IsLocked = value.IsLocked,
                    IsPhysicalItem = false,
                    RequiresPhysicalProximityForEquip = false,
                }).ToArray(),
                ActiveActivities = source.ActiveActivities.Select(value =>
                    new SimulationTeamActivityAssignmentSnapshot
                    {
                        ActivityStableId = value.ActivityStableId,
                        ActorStableId = value.ActorStableId,
                        CardCopyStableId = value.CardCopyStableId,
                        ActivityRoleCode = value.ActivityRoleCode,
                        LocationStableId = value.LocationStableId,
                        StateCode = value.StateCode,
                    }).ToArray(),
                MemberRoles = source.MemberRoles.Select(value =>
                    new SimulationTeamMemberRoleProjection
                    {
                        ActorStableId = value.ActorStableId,
                        CurrentRoleCode = value.CurrentRoleCode,
                        ActivityStableId = value.ActivityStableId,
                        EquippedCardCopyStableIds =
                            value.EquippedCardCopyStableIds.ToArray(),
                        IsPermanentProfession = false,
                    }).ToArray(),
                CombatLoadouts = source.CombatLoadouts.Select(value =>
                    new SimulationCombatCardLoadoutSnapshot
                    {
                        ActorStableId = value.ActorStableId,
                        CombatControlModeCode = value.CombatControlModeCode,
                        Slots = value.Slots.Select(CloneSlot).ToArray(),
                    }).ToArray(),
                SupportsRemoteEquip = true,
                SimulationOnly = true,
                IsOperationalState = false,
            };

        private sealed class CardState
        {
            public CardState(SimulationTeamRoleCardInitialCard source)
            {
                CardCopyStableId = source.CardCopyStableId.Trim();
                CardDefinitionStableId = source.CardDefinitionStableId.Trim();
                Title = source.Title.Trim();
                ActivityRoleCodes = source.ActivityRoleCodes
                    .Select(value => value.Trim()).Distinct(StringComparer.Ordinal).ToArray();
                EquippedActorStableId = source.EquippedActorStableId?.Trim()
                    ?? string.Empty;
                SlotCode = source.SlotCode?.Trim() ?? string.Empty;
            }

            public string CardCopyStableId { get; }
            public string CardDefinitionStableId { get; }
            public string Title { get; }
            public string[] ActivityRoleCodes { get; }
            public string EquippedActorStableId { get; set; }
            public string SlotCode { get; set; }
            public string LockedActivityStableId { get; set; } = string.Empty;
            public bool IsLocked => LockedActivityStableId.Length > 0;

            public SimulationTeamRoleCardSnapshot Snapshot() => new()
            {
                CardCopyStableId = CardCopyStableId,
                CardDefinitionStableId = CardDefinitionStableId,
                Title = Title,
                ActivityRoleCodes = ActivityRoleCodes.ToArray(),
                EquippedActorStableId = EquippedActorStableId,
                SlotCode = SlotCode,
                LockedActivityStableId = LockedActivityStableId,
                IsLocked = IsLocked,
                IsPhysicalItem = false,
                RequiresPhysicalProximityForEquip = false,
            };
        }

        private sealed class ActivityState
        {
            public string ActivityStableId { get; set; } = string.Empty;
            public string ActorStableId { get; set; } = string.Empty;
            public string CardCopyStableId { get; set; } = string.Empty;
            public string ActivityRoleCode { get; set; } = string.Empty;
            public string LocationStableId { get; set; } = string.Empty;

            public SimulationTeamActivityAssignmentSnapshot Snapshot() => new()
            {
                ActivityStableId = ActivityStableId,
                ActorStableId = ActorStableId,
                CardCopyStableId = CardCopyStableId,
                ActivityRoleCode = ActivityRoleCode,
                LocationStableId = LocationStableId,
                StateCode = SimulationTeamRoleCardCodes.Active,
            };
        }

        private sealed class CombatLoadoutState
        {
            public CombatLoadoutState(string actorStableId,
                string combatControlModeCode,
                SimulationCombatCardLoadoutSlotSnapshot[] slots)
            {
                ActorStableId = actorStableId;
                CombatControlModeCode = combatControlModeCode;
                Slots = slots;
            }

            public string ActorStableId { get; }
            public string CombatControlModeCode { get; }
            public SimulationCombatCardLoadoutSlotSnapshot[] Slots { get; }

            public SimulationCombatCardLoadoutSnapshot Snapshot() => new()
            {
                ActorStableId = ActorStableId,
                CombatControlModeCode = CombatControlModeCode,
                Slots = Slots.Select(CloneSlot).ToArray(),
            };
        }

        private sealed class AppliedCommand
        {
            public AppliedCommand(string payloadKey,
                SimulationTeamRoleCardStateSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public SimulationTeamRoleCardStateSnapshot Snapshot { get; }
        }
    }

    public sealed partial class 경영SimulationSessionAggregate
    {
        private SimulationTeamRoleCardInitialState? teamRoleCardCreationState;
        private SimulationTeamRoleCardState? teamRoleCardState;
        private string teamRoleCardInitialPayloadKey = string.Empty;

        public SimulationTeamRoleCardStateSnapshot GetTeamRoleCards()
        {
            lock (gate) return RequireTeamRoleCardState().Snapshot();
        }

        public SimulationTeamRoleCardStateSnapshot EquipTeamRoleCard(
            SimulationTeamRoleCardEquipRequest request)
        {
            lock (gate)
            {
                var state = RequireTeamRoleCardState();
                var before = state.Revision;
                var result = state.Equip(request);
                if (result.Revision > before)
                {
                    Revision++;
                    AppendTeamRoleCardEquipCommand(request);
                }
                return result;
            }
        }

        public SimulationTeamRoleCardStateSnapshot StartTeamActivity(
            SimulationTeamActivityStartRequest request)
        {
            lock (gate)
            {
                var state = RequireTeamRoleCardState();
                var before = state.Revision;
                var result = state.StartActivity(request);
                if (result.Revision > before)
                {
                    Revision++;
                    AppendTeamActivityStartCommand(request);
                }
                return result;
            }
        }

        public SimulationTeamRoleCardStateSnapshot EndTeamActivity(
            SimulationTeamActivityEndRequest request)
        {
            lock (gate)
            {
                var state = RequireTeamRoleCardState();
                var before = state.Revision;
                var result = state.EndActivity(request);
                if (result.Revision > before)
                {
                    Revision++;
                    AppendTeamActivityEndCommand(request);
                }
                return result;
            }
        }

        public SimulationTeamRoleCardStateSnapshot SetTeamCombatCardLoadout(
            SimulationCombatCardLoadoutSetRequest request)
        {
            lock (gate)
            {
                var state = RequireTeamRoleCardState();
                var before = state.Revision;
                var result = state.SetCombatLoadout(request);
                if (result.Revision > before)
                {
                    Revision++;
                    AppendCombatCardLoadoutSetCommand(request);
                }
                return result;
            }
        }

        private void InitializeTeamRoleCards(
            SimulationTeamRoleCardInitialState? initial)
        {
            teamRoleCardInitialPayloadKey = BuildTeamRoleCardPayloadKey(initial);
            if (initial == null) return;
            ValidateTeamRoleCardInitialState(initial);
            if (!string.Equals(initial.SessionStableId, SessionStableId,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationTeamRoleCardSessionMismatch");
            teamRoleCardCreationState = CloneTeamRoleCardInitialState(initial);
            teamRoleCardState = new SimulationTeamRoleCardState(
                teamRoleCardCreationState);
        }

        private SimulationTeamRoleCardState RequireTeamRoleCardState()
            => teamRoleCardState ?? throw new SimulationNotFoundException(
                "SimulationTeamRoleCardStateNotFound");

        private SimulationTeamRoleCardStateSnapshot?
            CreateTeamRoleCardStateSnapshotOrNull()
            => teamRoleCardState?.Snapshot();

        internal static void ValidateTeamRoleCardInitialState(
            SimulationTeamRoleCardInitialState? initial)
        {
            if (initial != null)
                SimulationTeamRoleCardState.ValidateInitial(initial);
        }

        internal static SimulationTeamRoleCardInitialState?
            CloneTeamRoleCardInitialStateOrNull(
                SimulationTeamRoleCardInitialState? source)
            => source == null ? null : CloneTeamRoleCardInitialState(source);

        private static SimulationTeamRoleCardInitialState
            CloneTeamRoleCardInitialState(SimulationTeamRoleCardInitialState source)
            => new()
            {
                SessionStableId = source.SessionStableId,
                TeamStableId = source.TeamStableId,
                TeamPolicyRevision = source.TeamPolicyRevision,
                RuleRevision = source.RuleRevision,
                MemberActorStableIds = source.MemberActorStableIds.ToArray(),
                Cards = source.Cards.Select(value =>
                    new SimulationTeamRoleCardInitialCard
                    {
                        CardCopyStableId = value.CardCopyStableId,
                        CardDefinitionStableId = value.CardDefinitionStableId,
                        Title = value.Title,
                        ActivityRoleCodes = value.ActivityRoleCodes.ToArray(),
                        EquippedActorStableId = value.EquippedActorStableId,
                        SlotCode = value.SlotCode,
                    }).ToArray(),
                CombatLoadouts = (source.CombatLoadouts
                        ?? Array.Empty<SimulationCombatCardLoadoutInitialState>())
                    .Select(value => new SimulationCombatCardLoadoutInitialState
                    {
                        ActorStableId = value.ActorStableId,
                        CombatControlModeCode = value.CombatControlModeCode,
                        Slots = (value.Slots
                                ?? Array.Empty<SimulationCombatCardLoadoutSlotSnapshot>())
                            .Select(slot => new SimulationCombatCardLoadoutSlotSnapshot
                            {
                                SlotCode = slot.SlotCode,
                                CardCopyStableId = slot.CardCopyStableId,
                            }).ToArray(),
                    }).ToArray(),
            };

        internal static SimulationTeamRoleCardStateSnapshot?
            CloneTeamRoleCardStateOrNull(SimulationTeamRoleCardStateSnapshot? source)
            => source == null ? null : CloneTeamRoleCardState(source);

        internal static SimulationTeamRoleCardStateSnapshot CloneTeamRoleCardState(
            SimulationTeamRoleCardStateSnapshot source)
            => new()
            {
                SessionStableId = source.SessionStableId,
                TeamStableId = source.TeamStableId,
                Revision = source.Revision,
                TeamPolicyRevision = source.TeamPolicyRevision,
                RuleRevision = source.RuleRevision,
                MemberActorStableIds = source.MemberActorStableIds.ToArray(),
                Cards = source.Cards.Select(value => new SimulationTeamRoleCardSnapshot
                {
                    CardCopyStableId = value.CardCopyStableId,
                    CardDefinitionStableId = value.CardDefinitionStableId,
                    Title = value.Title,
                    ActivityRoleCodes = value.ActivityRoleCodes.ToArray(),
                    EquippedActorStableId = value.EquippedActorStableId,
                    SlotCode = value.SlotCode,
                    LockedActivityStableId = value.LockedActivityStableId,
                    IsLocked = value.IsLocked,
                    IsPhysicalItem = value.IsPhysicalItem,
                    RequiresPhysicalProximityForEquip =
                        value.RequiresPhysicalProximityForEquip,
                }).ToArray(),
                ActiveActivities = source.ActiveActivities.Select(value =>
                    new SimulationTeamActivityAssignmentSnapshot
                    {
                        ActivityStableId = value.ActivityStableId,
                        ActorStableId = value.ActorStableId,
                        CardCopyStableId = value.CardCopyStableId,
                        ActivityRoleCode = value.ActivityRoleCode,
                        LocationStableId = value.LocationStableId,
                        StateCode = value.StateCode,
                    }).ToArray(),
                MemberRoles = source.MemberRoles.Select(value =>
                    new SimulationTeamMemberRoleProjection
                    {
                        ActorStableId = value.ActorStableId,
                        CurrentRoleCode = value.CurrentRoleCode,
                        ActivityStableId = value.ActivityStableId,
                        EquippedCardCopyStableIds =
                            value.EquippedCardCopyStableIds.ToArray(),
                        IsPermanentProfession = value.IsPermanentProfession,
                    }).ToArray(),
                CombatLoadouts = (source.CombatLoadouts
                        ?? Array.Empty<SimulationCombatCardLoadoutSnapshot>())
                    .Select(value => new SimulationCombatCardLoadoutSnapshot
                    {
                        ActorStableId = value.ActorStableId,
                        CombatControlModeCode = value.CombatControlModeCode,
                        Slots = (value.Slots
                                ?? Array.Empty<SimulationCombatCardLoadoutSlotSnapshot>())
                            .Select(slot => new SimulationCombatCardLoadoutSlotSnapshot
                            {
                                SlotCode = slot.SlotCode,
                                CardCopyStableId = slot.CardCopyStableId,
                            }).ToArray(),
                    }).ToArray(),
                SupportsRemoteEquip = source.SupportsRemoteEquip,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        internal static string BuildTeamRoleCardPayloadKey(
            SimulationTeamRoleCardInitialState? initial)
        {
            if (initial == null) return string.Empty;
            return string.Join("~", initial.SessionStableId, initial.TeamStableId,
                initial.TeamPolicyRevision, initial.RuleRevision,
                string.Join(",", initial.MemberActorStableIds
                    .OrderBy(value => value, StringComparer.Ordinal)),
                string.Join(";", initial.Cards
                    .OrderBy(value => value.CardCopyStableId, StringComparer.Ordinal)
                    .Select(value => string.Join("|", value.CardCopyStableId,
                        value.CardDefinitionStableId, value.Title,
                        string.Join(",", value.ActivityRoleCodes
                            .OrderBy(role => role, StringComparer.Ordinal)),
                        value.EquippedActorStableId, value.SlotCode))),
                string.Join(";", (initial.CombatLoadouts
                        ?? Array.Empty<SimulationCombatCardLoadoutInitialState>())
                    .OrderBy(value => value.ActorStableId, StringComparer.Ordinal)
                    .ThenBy(value => value.CombatControlModeCode,
                        StringComparer.Ordinal)
                    .Select(value => string.Join("|", value.ActorStableId,
                        value.CombatControlModeCode,
                        string.Join(",", (value.Slots
                                ?? Array.Empty<SimulationCombatCardLoadoutSlotSnapshot>())
                            .OrderBy(slot => slot.SlotCode, StringComparer.Ordinal)
                            .Select(slot => slot.SlotCode + ":"
                                + slot.CardCopyStableId))))));
        }

        internal static string BuildTeamRoleCardStatePayloadKey(
            SimulationTeamRoleCardStateSnapshot state)
            => string.Join("~", state.SessionStableId, state.TeamStableId,
                state.Revision, state.TeamPolicyRevision, state.RuleRevision,
                string.Join(",", state.MemberActorStableIds
                    .OrderBy(value => value, StringComparer.Ordinal)),
                string.Join(";", state.Cards
                    .OrderBy(value => value.CardCopyStableId, StringComparer.Ordinal)
                    .Select(value => string.Join("|", value.CardCopyStableId,
                        value.CardDefinitionStableId, value.Title,
                        string.Join(",", value.ActivityRoleCodes
                            .OrderBy(role => role, StringComparer.Ordinal)),
                        value.EquippedActorStableId, value.SlotCode,
                        value.LockedActivityStableId, value.IsLocked,
                        value.IsPhysicalItem,
                        value.RequiresPhysicalProximityForEquip))),
                string.Join(";", state.ActiveActivities
                    .OrderBy(value => value.ActivityStableId, StringComparer.Ordinal)
                    .Select(value => string.Join("|", value.ActivityStableId,
                        value.ActorStableId, value.CardCopyStableId,
                        value.ActivityRoleCode, value.LocationStableId,
                        value.StateCode))), state.SupportsRemoteEquip,
                string.Join(";", (state.CombatLoadouts
                        ?? Array.Empty<SimulationCombatCardLoadoutSnapshot>())
                    .OrderBy(value => value.ActorStableId, StringComparer.Ordinal)
                    .ThenBy(value => value.CombatControlModeCode,
                        StringComparer.Ordinal)
                    .Select(value => string.Join("|", value.ActorStableId,
                        value.CombatControlModeCode,
                        string.Join(",", (value.Slots
                                ?? Array.Empty<SimulationCombatCardLoadoutSlotSnapshot>())
                            .OrderBy(slot => slot.SlotCode, StringComparer.Ordinal)
                            .Select(slot => slot.SlotCode + ":"
                                + slot.CardCopyStableId))))),
                state.SupportsRemoteEquip, state.SimulationOnly,
                state.IsOperationalState);
    }
}
