using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const int CollectibleCoverageRadius = 5;
        private readonly Dictionary<string, string> collectibleActorL2Tiles =
            new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly HashSet<string> collectibleRevealedL2Tiles =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> collectibleRevealedL1Areas =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly List<SimulationWorldDiscoveryEventSnapshot> collectibleDiscoveryEvents =
            new List<SimulationWorldDiscoveryEventSnapshot>();
        private readonly Dictionary<string, CollectiblePityState> collectiblePityStates =
            new Dictionary<string, CollectiblePityState>(StringComparer.Ordinal);
        private readonly List<SimulationCardDrawOpportunitySnapshot> collectibleDrawOpportunities =
            new List<SimulationCardDrawOpportunitySnapshot>();
        private readonly List<SimulationCollectibleCardCopySnapshot> collectibleCards =
            new List<SimulationCollectibleCardCopySnapshot>();
        private readonly List<SimulationCollectibleRewardEvaluationSnapshot> collectibleEvaluations =
            new List<SimulationCollectibleRewardEvaluationSnapshot>();
        private readonly List<SimulationCollectibleCardTransferSnapshot> collectibleTransfers =
            new List<SimulationCollectibleCardTransferSnapshot>();
        private readonly HashSet<string> processedCollectibleRewardEvents =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedTraversalCommand> appliedTraversalCommands =
            new Dictionary<string, AppliedTraversalCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedDrawCommand> appliedCollectibleDrawCommands =
            new Dictionary<string, AppliedDrawCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedTransferCommand> appliedCollectibleTransferCommands =
            new Dictionary<string, AppliedTransferCommand>(StringComparer.Ordinal);
        private string collectibleTeamStableId = string.Empty;
        private string[] collectibleMemberActorStableIds = Array.Empty<string>();
        private int collectibleCoverageCenterX;
        private int collectibleCoverageCenterY;
        private bool collectibleRewardsConfigured;

        private static readonly SimulationCollectibleCardDefinitionSnapshot[] CollectibleCatalog =
        {
            Definition("collectible-card:farm:soil-reading", SimulationCollectibleCardRewardCodes.Farm,
                "고랭지 흙 읽기", "collectible.farm.soil-reading"),
            Definition("collectible-card:farm:water-path", SimulationCollectibleCardRewardCodes.Farm,
                "밭 물길 돌보기", "collectible.farm.water-path"),
            Definition("collectible-card:farm:harvest-notes", SimulationCollectibleCardRewardCodes.Farm,
                "수확 준비 기록", "collectible.farm.harvest-notes"),
            Definition("collectible-card:exploration:terrain-reading", SimulationCollectibleCardRewardCodes.Exploration,
                "산길 지형 판독", "collectible.exploration.terrain-reading"),
            Definition("collectible-card:exploration:road-markers", SimulationCollectibleCardRewardCodes.Exploration,
                "길 위의 표식", "collectible.exploration.road-markers"),
            Definition("collectible-card:exploration:observation-notes", SimulationCollectibleCardRewardCodes.Exploration,
                "탐험 관찰 기록", "collectible.exploration.observation-notes"),
        };

        private static readonly string CollectibleCatalogHash = CalculateSha256(string.Join("|",
            CollectibleCatalog.OrderBy(value => value.CardDefinitionStableId, StringComparer.Ordinal)
                .Select(value => string.Join("~", value.CardDefinitionStableId, value.FamilyCode,
                    value.KoreanTitle, value.PresentationKey, value.EvidenceKindCode))));

        public SimulationWorldExplorationStateSnapshot GetWorldExplorationState()
        {
            lock (gate)
            {
                EnsureCollectibleRewardsConfigured();
                return CreateWorldExplorationStateSnapshot();
            }
        }

        public SimulationCollectibleCardRewardStateSnapshot GetCollectibleCardRewards(
            string requestingActorStableId)
        {
            lock (gate)
            {
                EnsureCollectibleMember(requestingActorStableId);
                return CreateCollectibleCardRewardStateSnapshot(requestingActorStableId);
            }
        }

        public SimulationTileTraversalConfirmResponse ConfirmTileTraversal(
            SimulationTileTraversalConfirmRequest request)
        {
            ValidateTileTraversalRequest(request);
            lock (gate)
            {
                EnsureCollectibleMember(request.ActorStableId);
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildTileTraversalPayloadKey(request);
                if (appliedTraversalCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return CloneTraversalResponse(applied.Response);
                }
                if (HasDifferentKindCommand(commandId)
                    || appliedCollectibleDrawCommands.ContainsKey(commandId)
                    || appliedCollectibleTransferCommands.ContainsKey(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");

                var actor = request.ActorStableId.Trim();
                var from = ParseL2Tile(request.FromL2TileKey);
                var to = ParseL2Tile(request.ToL2TileKey);
                if (!string.Equals(collectibleActorL2Tiles[actor], from.Key,
                        StringComparison.Ordinal))
                    throw new SimulationConflictException("SimulationTraversalFromTileMismatch");
                if ((Math.Abs(to.X - from.X) > 1 || Math.Abs(to.Y - from.Y) > 1)
                    || (to.X == from.X && to.Y == from.Y))
                    throw new SimulationContractException("SimulationTraversalNotAdjacent");
                if (Math.Abs(to.X - collectibleCoverageCenterX) > CollectibleCoverageRadius
                    || Math.Abs(to.Y - collectibleCoverageCenterY) > CollectibleCoverageRadius)
                    throw new SimulationNotFoundException("SimulationTraversalOutsideCoverage");

                collectibleActorL2Tiles[actor] = to.Key;
                var created = new List<string>();
                var wasNewL2 = collectibleRevealedL2Tiles.Add(to.Key);
                if (wasNewL2)
                {
                    var eventId = "discovery:l2:" + to.Key;
                    AddDiscoveryEvent(eventId, actor,
                        SimulationCollectibleCardRewardCodes.NewL2Tile, to.Key);
                    var opportunity = EvaluateCollectibleReward(eventId, actor,
                        SimulationCollectibleCardRewardCodes.Exploration,
                        SimulationCollectibleCardRewardCodes.NewL2Tile, 15m);
                    if (!string.IsNullOrEmpty(opportunity)) created.Add(opportunity);
                }

                var l1Key = ParentL1Key(to.X, to.Y);
                var wasNewL1 = collectibleRevealedL1Areas.Add(l1Key);
                if (wasNewL1)
                {
                    var eventId = "discovery:l1:" + l1Key;
                    AddDiscoveryEvent(eventId, actor,
                        SimulationCollectibleCardRewardCodes.NewL1Area, l1Key);
                    var opportunity = EvaluateCollectibleReward(eventId, actor,
                        SimulationCollectibleCardRewardCodes.Exploration,
                        SimulationCollectibleCardRewardCodes.NewL1Area, 40m);
                    if (!string.IsNullOrEmpty(opportunity)) created.Add(opportunity);
                }

                Revision++;
                AppendTileTraversalCommand(request);
                var response = new SimulationTileTraversalConfirmResponse
                {
                    Exploration = CreateWorldExplorationStateSnapshot(),
                    Rewards = CreateCollectibleCardRewardStateSnapshot(actor),
                    WasNewL2Tile = wasNewL2,
                    WasNewL1Area = wasNewL1,
                    CreatedOpportunityStableIds = created.ToArray(),
                };
                appliedTraversalCommands.Add(commandId,
                    new AppliedTraversalCommand(payloadKey, CloneTraversalResponse(response)));
                return response;
            }
        }

        public SimulationCollectibleCardDrawResponse DrawCollectibleCard(
            SimulationCollectibleCardDrawRequest request)
        {
            ValidateCollectibleCardDrawRequest(request);
            lock (gate)
            {
                EnsureCollectibleMember(request.ActorStableId);
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildCollectibleCardDrawPayloadKey(request);
                if (appliedCollectibleDrawCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return CloneDrawResponse(applied.Response);
                }
                if (HasDifferentKindCommand(commandId)
                    || appliedTraversalCommands.ContainsKey(commandId)
                    || appliedCollectibleTransferCommands.ContainsKey(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");

                var actor = request.ActorStableId.Trim();
                var opportunity = collectibleDrawOpportunities.SingleOrDefault(value =>
                    value.OpportunityStableId == request.OpportunityStableId.Trim())
                    ?? throw new SimulationNotFoundException(
                        "SimulationCollectibleCardOpportunityNotFound");
                if (!string.Equals(opportunity.OwnerActorStableId, actor,
                        StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "SimulationCollectibleCardOpportunityOwnerMismatch");
                if (opportunity.StateCode != SimulationCollectibleCardRewardCodes.Pending)
                    throw new SimulationConflictException(
                        "SimulationCollectibleCardOpportunityAlreadyDrawn");

                var ownedDefinitions = new HashSet<string>(collectibleCards.Select(value =>
                    value.CardDefinitionStableId), StringComparer.Ordinal);
                var available = CollectibleCatalog.Where(value =>
                        value.FamilyCode == opportunity.FamilyCode
                        && !ownedDefinitions.Contains(value.CardDefinitionStableId))
                    .OrderBy(value => value.CardDefinitionStableId, StringComparer.Ordinal)
                    .ToArray();
                if (available.Length == 0)
                    throw new SimulationConflictException(
                        "SimulationCollectibleCardCatalogExhausted");
                var definition = available[DeterministicIndex(
                    ScenarioSeed + "|" + SimulationCollectibleCardRewardCodes.CatalogRevision
                    + "|" + opportunity.OpportunityStableId, available.Length)];
                var cardCopyId = "collectible-card-copy:" + ShortHash(
                    opportunity.OpportunityStableId + "|" + definition.CardDefinitionStableId);
                var card = new SimulationCollectibleCardCopySnapshot
                {
                    CardCopyStableId = cardCopyId,
                    CardDefinitionStableId = definition.CardDefinitionStableId,
                    OwnerActorStableId = actor,
                    FamilyCode = definition.FamilyCode,
                    KoreanTitle = definition.KoreanTitle,
                    PresentationKey = definition.PresentationKey,
                    AcquiredFromOpportunityStableId = opportunity.OpportunityStableId,
                    AcquiredWorldTick = CurrentTick,
                    PresentationOnly = true,
                };
                collectibleCards.Add(card);
                opportunity.StateCode = SimulationCollectibleCardRewardCodes.Drawn;
                opportunity.DrawnCardCopyStableId = cardCopyId;
                Revision++;
                AppendCollectibleCardDrawCommand(request);
                var response = new SimulationCollectibleCardDrawResponse
                {
                    DrawnCard = CloneCollectibleCard(card),
                    Rewards = CreateCollectibleCardRewardStateSnapshot(actor),
                };
                appliedCollectibleDrawCommands.Add(commandId,
                    new AppliedDrawCommand(payloadKey, CloneDrawResponse(response)));
                return response;
            }
        }

        public SimulationCollectibleCardTransferResponse TransferCollectibleCard(
            SimulationCollectibleCardTransferRequest request)
        {
            ValidateCollectibleCardTransferRequest(request);
            lock (gate)
            {
                EnsureCollectibleMember(request.OwnerActorStableId);
                EnsureCollectibleMember(request.TargetActorStableId);
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildCollectibleCardTransferPayloadKey(request);
                if (appliedCollectibleTransferCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return CloneTransferResponse(applied.Response);
                }
                if (HasDifferentKindCommand(commandId)
                    || appliedTraversalCommands.ContainsKey(commandId)
                    || appliedCollectibleDrawCommands.ContainsKey(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                if (string.Equals(request.OwnerActorStableId.Trim(),
                        request.TargetActorStableId.Trim(), StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationCollectibleCardTransferSameOwner");

                var card = collectibleCards.SingleOrDefault(value =>
                    value.CardCopyStableId == request.CardCopyStableId.Trim())
                    ?? throw new SimulationNotFoundException(
                        "SimulationCollectibleCardNotFound");
                if (!string.Equals(card.OwnerActorStableId,
                        request.OwnerActorStableId.Trim(), StringComparison.Ordinal))
                    throw new SimulationConflictException(
                        "SimulationCollectibleCardOwnerMismatch");
                var from = card.OwnerActorStableId;
                card.OwnerActorStableId = request.TargetActorStableId.Trim();
                collectibleTransfers.Add(new SimulationCollectibleCardTransferSnapshot
                {
                    TransferStableId = "collectible-card-transfer:" + ShortHash(commandId),
                    CommandId = commandId,
                    CardCopyStableId = card.CardCopyStableId,
                    FromActorStableId = from,
                    ToActorStableId = card.OwnerActorStableId,
                    WorldTick = CurrentTick,
                });
                Revision++;
                AppendCollectibleCardTransferCommand(request);
                var response = new SimulationCollectibleCardTransferResponse
                {
                    TransferredCard = CloneCollectibleCard(card),
                    Rewards = CreateCollectibleCardRewardStateSnapshot(from),
                };
                appliedCollectibleTransferCommands.Add(commandId,
                    new AppliedTransferCommand(payloadKey, CloneTransferResponse(response)));
                return response;
            }
        }

        private void InitializeCollectibleCardRewards()
        {
            if (teamRoleCardCreationState == null) return;
            collectibleTeamStableId = teamRoleCardCreationState.TeamStableId;
            collectibleMemberActorStableIds = teamRoleCardCreationState.MemberActorStableIds
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var initialTileKey = farmSurvivalCreationState?.TileKey;
            if (string.IsNullOrWhiteSpace(initialTileKey))
                initialTileKey = PyeongchangWorldExplorationFixtureIds.DaegwallyeongFarmCenterTile;
            var initial = ParseL2Tile(initialTileKey);
            collectibleCoverageCenterX = initial.X;
            collectibleCoverageCenterY = initial.Y;
            foreach (var actor in collectibleMemberActorStableIds)
                collectibleActorL2Tiles.Add(actor, initial.Key);
            collectibleRevealedL2Tiles.Add(initial.Key);
            collectibleRevealedL1Areas.Add(ParentL1Key(initial.X, initial.Y));
            collectibleRewardsConfigured = true;
        }

        private void EvaluateCollectibleRewardForFarmCompletion(FarmWorkOrderState workOrder)
        {
            if (!collectibleRewardsConfigured
                || workOrder.AssignmentKindCode != SimulationFarmSurvivalCodes.PlayerDirect
                || workOrder.ActionCode != SimulationFarmSurvivalCodes.Tilling
                || !collectibleMemberActorStableIds.Contains(workOrder.ActorStableId,
                    StringComparer.Ordinal)) return;
            EvaluateCollectibleReward("farm-completion:" + workOrder.TargetStableId
                    + ":" + workOrder.ActionCode,
                workOrder.ActorStableId, SimulationCollectibleCardRewardCodes.Farm,
                SimulationCollectibleCardRewardCodes.FarmTillingCompleted, 20m);
        }

        private string EvaluateCollectibleReward(string eventStableId, string actorStableId,
            string familyCode, string triggerCode, decimal baseProbability)
        {
            if (!processedCollectibleRewardEvents.Add(eventStableId)) return string.Empty;
            var familyCapacity = CollectibleCatalog.Count(value => value.FamilyCode == familyCode);
            var usedCapacity = collectibleCards.Count(value => value.FamilyCode == familyCode)
                + collectibleDrawOpportunities.Count(value => value.FamilyCode == familyCode
                    && value.StateCode == SimulationCollectibleCardRewardCodes.Pending);
            var pity = GetPity(actorStableId, familyCode);
            if (usedCapacity >= familyCapacity)
            {
                collectibleEvaluations.Add(CreateEvaluation(eventStableId, actorStableId,
                    familyCode, triggerCode, pity, 0m, 0m, false, 0m,
                    SimulationCollectibleCardRewardCodes.CatalogCapacitySuppressed,
                    string.Empty));
                return string.Empty;
            }

            var roleBonus = HasMatchingActiveRole(actorStableId, familyCode) ? 10m : 0m;
            var probability = Math.Min(100m, baseProbability + roleBonus);
            var guaranteed = pity.ConsecutiveFailures >= 5;
            var attemptOrdinal = pity.EligibleAttemptCount + 1;
            var sample = DeterministicPercent(string.Join("|", ScenarioSeed,
                SimulationCollectibleCardRewardCodes.RuleRevision, eventStableId,
                actorStableId, triggerCode, attemptOrdinal));
            var success = guaranteed || sample < probability;
            var opportunityId = success
                ? "card-opportunity:" + ShortHash(eventStableId + "|" + actorStableId
                    + "|" + familyCode) : string.Empty;
            collectibleEvaluations.Add(CreateEvaluation(eventStableId, actorStableId,
                familyCode, triggerCode, pity, probability, roleBonus, guaranteed, sample,
                success ? SimulationCollectibleCardRewardCodes.Success
                    : SimulationCollectibleCardRewardCodes.Failure, opportunityId));
            pity.EligibleAttemptCount = attemptOrdinal;
            if (!success)
            {
                pity.ConsecutiveFailures++;
                return string.Empty;
            }
            var failureCountBefore = pity.ConsecutiveFailures;
            pity.ConsecutiveFailures = 0;
            collectibleDrawOpportunities.Add(new SimulationCardDrawOpportunitySnapshot
            {
                OpportunityStableId = opportunityId,
                OwnerActorStableId = actorStableId,
                FamilyCode = familyCode,
                TriggerCode = triggerCode,
                SourceEventStableId = eventStableId,
                StateCode = SimulationCollectibleCardRewardCodes.Pending,
                AwardedWorldTick = CurrentTick,
                AppliedProbabilityPercent = probability,
                ActiveRoleBonusPercentagePoints = roleBonus,
                PityFailureCountBefore = failureCountBefore,
                WasGuaranteed = guaranteed,
                HasExpiry = false,
            });
            return opportunityId;
        }

        private bool HasMatchingActiveRole(string actorStableId, string familyCode)
        {
            if (teamRoleCardState == null) return false;
            var member = teamRoleCardState.Snapshot().MemberRoles.SingleOrDefault(value =>
                value.ActorStableId == actorStableId);
            return member != null && string.Equals(member.CurrentRoleCode,
                familyCode == SimulationCollectibleCardRewardCodes.Farm
                    ? SimulationTeamRoleCardCodes.FarmWork
                    : SimulationTeamRoleCardCodes.Exploration,
                StringComparison.Ordinal);
        }

        private void AddDiscoveryEvent(string eventId, string actor, string trigger, string key)
            => collectibleDiscoveryEvents.Add(new SimulationWorldDiscoveryEventSnapshot
            {
                EventStableId = eventId,
                ActorStableId = actor,
                TriggerCode = trigger,
                SpatialUnitKey = key,
                WorldTick = CurrentTick,
            });

        private SimulationWorldExplorationStateSnapshot CreateWorldExplorationStateSnapshot()
            => new SimulationWorldExplorationStateSnapshot
            {
                SessionStableId = SessionStableId,
                TeamStableId = collectibleTeamStableId,
                Revision = Revision,
                RuleRevision = SimulationCollectibleCardRewardCodes.RuleRevision,
                ActorTilePositions = collectibleActorL2Tiles.OrderBy(value => value.Key,
                        StringComparer.Ordinal)
                    .Select(value => new SimulationActorTilePositionSnapshot
                    {
                        ActorStableId = value.Key,
                        CurrentL2TileKey = value.Value,
                    }).ToArray(),
                RevealedL2TileKeys = collectibleRevealedL2Tiles.OrderBy(value => value,
                    StringComparer.Ordinal).ToArray(),
                RevealedL1AreaKeys = collectibleRevealedL1Areas.OrderBy(value => value,
                    StringComparer.Ordinal).ToArray(),
                DiscoveryEvents = collectibleDiscoveryEvents.Select(CloneDiscoveryEvent).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };

        private SimulationCollectibleCardRewardStateSnapshot
            CreateCollectibleCardRewardStateSnapshot(string? requestingActorStableId = null)
            => new SimulationCollectibleCardRewardStateSnapshot
            {
                SessionStableId = SessionStableId,
                TeamStableId = collectibleTeamStableId,
                Revision = Revision,
                RuleRevision = SimulationCollectibleCardRewardCodes.RuleRevision,
                CatalogRevision = SimulationCollectibleCardRewardCodes.CatalogRevision,
                CatalogHashSha256 = CollectibleCatalogHash,
                ProbabilityProfile = new SimulationCollectibleCardProbabilityProfileSnapshot
                {
                    FarmBasePercent = 20m,
                    NewL2BasePercent = 15m,
                    NewL1BasePercent = 40m,
                    MatchingActiveRoleBonusPercentagePoints = 10m,
                    GuaranteedAfterConsecutiveFailures = 5,
                },
                Definitions = CollectibleCatalog.Select(CloneCollectibleDefinition).ToArray(),
                DrawOpportunities = collectibleDrawOpportunities.Where(value =>
                        requestingActorStableId == null
                        || value.OwnerActorStableId == requestingActorStableId)
                    .Select(CloneDrawOpportunity).ToArray(),
                Cards = collectibleCards.Select(CloneCollectibleCard).ToArray(),
                PityStates = collectiblePityStates.Values.OrderBy(value => value.ActorStableId,
                        StringComparer.Ordinal).ThenBy(value => value.FamilyCode,
                        StringComparer.Ordinal)
                    .Select(value => new SimulationCollectibleCardPitySnapshot
                    {
                        ActorStableId = value.ActorStableId,
                        FamilyCode = value.FamilyCode,
                        EligibleAttemptCount = value.EligibleAttemptCount,
                        ConsecutiveFailureCount = value.ConsecutiveFailures,
                    }).ToArray(),
                Evaluations = collectibleEvaluations.Select(CloneEvaluation).ToArray(),
                Transfers = collectibleTransfers.Select(CloneTransfer).ToArray(),
                SupportsRemoteTransfer = true,
                HasExpiry = false,
                PresentationOnly = true,
                SimulationOnly = true,
                IsOperationalState = false,
            };

        private SimulationWorldExplorationStateSnapshot?
            CreateWorldExplorationStateSnapshotOrNull()
            => collectibleRewardsConfigured ? CreateWorldExplorationStateSnapshot() : null;

        private SimulationCollectibleCardRewardStateSnapshot?
            CreateCollectibleCardRewardStateSnapshotOrNull()
            => collectibleRewardsConfigured ? CreateCollectibleCardRewardStateSnapshot() : null;

        private void EnsureCollectibleRewardsConfigured()
        {
            if (!collectibleRewardsConfigured)
                throw new SimulationNotFoundException(
                    "SimulationCollectibleCardRewardStateNotFound");
        }

        private void EnsureCollectibleMember(string actorStableId)
        {
            EnsureCollectibleRewardsConfigured();
            RequireStableId(actorStableId, "SimulationCollectibleCardActorInvalid");
            if (!collectibleMemberActorStableIds.Contains(actorStableId.Trim(),
                    StringComparer.Ordinal))
                throw new SimulationNotFoundException(
                    "SimulationCollectibleCardTeamMemberNotFound");
        }

        private CollectiblePityState GetPity(string actor, string family)
        {
            var key = actor + "|" + family;
            if (!collectiblePityStates.TryGetValue(key, out var state))
            {
                state = new CollectiblePityState(actor, family);
                collectiblePityStates.Add(key, state);
            }
            return state;
        }

        private bool HasAppliedCollectibleCardCommand(string commandId)
            => appliedTraversalCommands.ContainsKey(commandId)
                || appliedCollectibleDrawCommands.ContainsKey(commandId)
                || appliedCollectibleTransferCommands.ContainsKey(commandId);

        private void AppendTileTraversalCommand(SimulationTileTraversalConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TileTraversalConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TileTraversalConfirmRequest = CloneTileTraversalRequest(request),
            });

        private void AppendCollectibleCardDrawCommand(SimulationCollectibleCardDrawRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.CollectibleCardDraw,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                CollectibleCardDrawRequest = CloneCollectibleCardDrawRequest(request),
            });

        private void AppendCollectibleCardTransferCommand(
            SimulationCollectibleCardTransferRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.CollectibleCardTransfer,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                CollectibleCardTransferRequest = CloneCollectibleCardTransferRequest(request),
            });

        internal static SimulationTileTraversalConfirmRequest CloneTileTraversalRequest(
            SimulationTileTraversalConfirmRequest source) => new SimulationTileTraversalConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                ActorStableId = source.ActorStableId,
                FromL2TileKey = source.FromL2TileKey,
                ToL2TileKey = source.ToL2TileKey,
            };

        internal static SimulationCollectibleCardDrawRequest CloneCollectibleCardDrawRequest(
            SimulationCollectibleCardDrawRequest source) => new SimulationCollectibleCardDrawRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                ActorStableId = source.ActorStableId,
                OpportunityStableId = source.OpportunityStableId,
            };

        internal static SimulationCollectibleCardTransferRequest
            CloneCollectibleCardTransferRequest(SimulationCollectibleCardTransferRequest source)
            => new SimulationCollectibleCardTransferRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                OwnerActorStableId = source.OwnerActorStableId,
                TargetActorStableId = source.TargetActorStableId,
                CardCopyStableId = source.CardCopyStableId,
            };

        internal static void ValidateTileTraversalRequest(
            SimulationTileTraversalConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.ActorStableId, "SimulationCollectibleCardActorInvalid");
            ParseL2Tile(request.FromL2TileKey);
            ParseL2Tile(request.ToL2TileKey);
        }

        internal static void ValidateCollectibleCardDrawRequest(
            SimulationCollectibleCardDrawRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.ActorStableId, "SimulationCollectibleCardActorInvalid");
            RequireStableId(request.OpportunityStableId,
                "SimulationCollectibleCardOpportunityInvalid");
        }

        internal static void ValidateCollectibleCardTransferRequest(
            SimulationCollectibleCardTransferRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(request.OwnerActorStableId,
                "SimulationCollectibleCardOwnerInvalid");
            RequireStableId(request.TargetActorStableId,
                "SimulationCollectibleCardTargetOwnerInvalid");
            RequireStableId(request.CardCopyStableId,
                "SimulationCollectibleCardCopyInvalid");
        }

        internal static string BuildTileTraversalPayloadKey(
            SimulationTileTraversalConfirmRequest request)
            => string.Join("|", request.ExpectedRevision, request.ActorStableId.Trim(),
                request.FromL2TileKey.Trim(), request.ToL2TileKey.Trim());

        internal static string BuildCollectibleCardDrawPayloadKey(
            SimulationCollectibleCardDrawRequest request)
            => string.Join("|", request.ExpectedRevision, request.ActorStableId.Trim(),
                request.OpportunityStableId.Trim());

        internal static string BuildCollectibleCardTransferPayloadKey(
            SimulationCollectibleCardTransferRequest request)
            => string.Join("|", request.ExpectedRevision, request.OwnerActorStableId.Trim(),
                request.TargetActorStableId.Trim(), request.CardCopyStableId.Trim());

        internal static string BuildWorldExplorationStatePayloadKey(
            SimulationWorldExplorationStateSnapshot state)
            => string.Join("~", state.SessionStableId, state.TeamStableId, state.Revision,
                state.RuleRevision,
                string.Join(";", state.ActorTilePositions.OrderBy(value => value.ActorStableId,
                    StringComparer.Ordinal).Select(value => value.ActorStableId + "|"
                        + value.CurrentL2TileKey)),
                string.Join(",", state.RevealedL2TileKeys.OrderBy(value => value,
                    StringComparer.Ordinal)),
                string.Join(",", state.RevealedL1AreaKeys.OrderBy(value => value,
                    StringComparer.Ordinal)),
                string.Join(";", state.DiscoveryEvents.Select(value => string.Join("|",
                    value.EventStableId, value.ActorStableId, value.TriggerCode,
                    value.SpatialUnitKey, value.WorldTick))), state.SimulationOnly,
                state.IsOperationalState);

        internal static string BuildCollectibleCardRewardStatePayloadKey(
            SimulationCollectibleCardRewardStateSnapshot state)
            => string.Join("~", state.SessionStableId, state.TeamStableId, state.Revision,
                state.RuleRevision, state.CatalogRevision, state.CatalogHashSha256,
                state.ProbabilityProfile.FarmBasePercent,
                state.ProbabilityProfile.NewL2BasePercent,
                state.ProbabilityProfile.NewL1BasePercent,
                state.ProbabilityProfile.MatchingActiveRoleBonusPercentagePoints,
                state.ProbabilityProfile.GuaranteedAfterConsecutiveFailures,
                string.Join(";", state.Definitions.Select(value => string.Join("|",
                    value.CardDefinitionStableId, value.FamilyCode, value.KoreanTitle,
                    value.PresentationKey, value.EvidenceKindCode,
                    value.PresentationOnly))),
                string.Join(";", state.DrawOpportunities.Select(value => string.Join("|",
                    value.OpportunityStableId, value.OwnerActorStableId, value.FamilyCode,
                    value.TriggerCode, value.SourceEventStableId, value.StateCode,
                    value.AwardedWorldTick, value.AppliedProbabilityPercent,
                    value.ActiveRoleBonusPercentagePoints, value.PityFailureCountBefore,
                    value.WasGuaranteed, value.DrawnCardCopyStableId, value.HasExpiry))),
                string.Join(";", state.Cards.Select(value => string.Join("|",
                    value.CardCopyStableId, value.CardDefinitionStableId,
                    value.OwnerActorStableId, value.FamilyCode, value.KoreanTitle,
                    value.PresentationKey, value.AcquiredFromOpportunityStableId,
                    value.AcquiredWorldTick, value.PresentationOnly))),
                string.Join(";", state.PityStates.Select(value => string.Join("|",
                    value.ActorStableId, value.FamilyCode, value.EligibleAttemptCount,
                    value.ConsecutiveFailureCount))),
                string.Join(";", state.Evaluations.Select(value => string.Join("|",
                    value.EvaluationStableId, value.ActorStableId, value.FamilyCode,
                    value.TriggerCode, value.SourceEventStableId, value.WorldTick,
                    value.AttemptOrdinal, value.AppliedProbabilityPercent,
                    value.ActiveRoleBonusPercentagePoints, value.PityFailureCountBefore,
                    value.WasGuaranteed, value.DeterministicSamplePercent,
                    value.ResultCode, value.OpportunityStableId))),
                string.Join(";", state.Transfers.Select(value => string.Join("|",
                    value.TransferStableId, value.CommandId, value.CardCopyStableId,
                    value.FromActorStableId, value.ToActorStableId, value.WorldTick))),
                state.SupportsRemoteTransfer, state.HasExpiry, state.PresentationOnly,
                state.SimulationOnly, state.IsOperationalState);

        internal static SimulationWorldExplorationStateSnapshot?
            CloneWorldExplorationStateOrNull(SimulationWorldExplorationStateSnapshot? source)
            => source == null ? null : new SimulationWorldExplorationStateSnapshot
            {
                SessionStableId = source.SessionStableId,
                TeamStableId = source.TeamStableId,
                Revision = source.Revision,
                RuleRevision = source.RuleRevision,
                ActorTilePositions = source.ActorTilePositions.Select(value =>
                    new SimulationActorTilePositionSnapshot
                    {
                        ActorStableId = value.ActorStableId,
                        CurrentL2TileKey = value.CurrentL2TileKey,
                    }).ToArray(),
                RevealedL2TileKeys = source.RevealedL2TileKeys.ToArray(),
                RevealedL1AreaKeys = source.RevealedL1AreaKeys.ToArray(),
                DiscoveryEvents = source.DiscoveryEvents.Select(CloneDiscoveryEvent).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        internal static SimulationCollectibleCardRewardStateSnapshot?
            CloneCollectibleCardRewardStateOrNull(
                SimulationCollectibleCardRewardStateSnapshot? source)
            => source == null ? null : CloneRewardState(source);

        private static SimulationCollectibleCardRewardStateSnapshot CloneRewardState(
            SimulationCollectibleCardRewardStateSnapshot source)
            => new SimulationCollectibleCardRewardStateSnapshot
            {
                SessionStableId = source.SessionStableId,
                TeamStableId = source.TeamStableId,
                Revision = source.Revision,
                RuleRevision = source.RuleRevision,
                CatalogRevision = source.CatalogRevision,
                CatalogHashSha256 = source.CatalogHashSha256,
                ProbabilityProfile = new SimulationCollectibleCardProbabilityProfileSnapshot
                {
                    FarmBasePercent = source.ProbabilityProfile.FarmBasePercent,
                    NewL2BasePercent = source.ProbabilityProfile.NewL2BasePercent,
                    NewL1BasePercent = source.ProbabilityProfile.NewL1BasePercent,
                    MatchingActiveRoleBonusPercentagePoints = source.ProbabilityProfile
                        .MatchingActiveRoleBonusPercentagePoints,
                    GuaranteedAfterConsecutiveFailures = source.ProbabilityProfile
                        .GuaranteedAfterConsecutiveFailures,
                },
                Definitions = source.Definitions.Select(CloneCollectibleDefinition).ToArray(),
                DrawOpportunities = source.DrawOpportunities.Select(CloneDrawOpportunity).ToArray(),
                Cards = source.Cards.Select(CloneCollectibleCard).ToArray(),
                PityStates = source.PityStates.Select(value => new SimulationCollectibleCardPitySnapshot
                {
                    ActorStableId = value.ActorStableId,
                    FamilyCode = value.FamilyCode,
                    EligibleAttemptCount = value.EligibleAttemptCount,
                    ConsecutiveFailureCount = value.ConsecutiveFailureCount,
                }).ToArray(),
                Evaluations = source.Evaluations.Select(CloneEvaluation).ToArray(),
                Transfers = source.Transfers.Select(CloneTransfer).ToArray(),
                SupportsRemoteTransfer = source.SupportsRemoteTransfer,
                HasExpiry = source.HasExpiry,
                PresentationOnly = source.PresentationOnly,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private SimulationCollectibleRewardEvaluationSnapshot CreateEvaluation(
            string eventId, string actor, string family, string trigger,
            CollectiblePityState pity, decimal probability, decimal roleBonus,
            bool guaranteed, decimal sample, string result, string opportunityId)
            => new SimulationCollectibleRewardEvaluationSnapshot
            {
                EvaluationStableId = "card-reward-evaluation:" + ShortHash(eventId),
                ActorStableId = actor,
                FamilyCode = family,
                TriggerCode = trigger,
                SourceEventStableId = eventId,
                WorldTick = CurrentTick,
                AttemptOrdinal = result == SimulationCollectibleCardRewardCodes.CatalogCapacitySuppressed
                    ? pity.EligibleAttemptCount : pity.EligibleAttemptCount + 1,
                AppliedProbabilityPercent = probability,
                ActiveRoleBonusPercentagePoints = roleBonus,
                PityFailureCountBefore = pity.ConsecutiveFailures,
                WasGuaranteed = guaranteed,
                DeterministicSamplePercent = sample,
                ResultCode = result,
                OpportunityStableId = opportunityId,
            };

        private static SimulationCollectibleCardDefinitionSnapshot Definition(
            string id, string family, string title, string presentation)
            => new SimulationCollectibleCardDefinitionSnapshot
            {
                CardDefinitionStableId = id,
                FamilyCode = family,
                KoreanTitle = title,
                PresentationKey = presentation,
                EvidenceKindCode = SimulationCollectibleCardRewardCodes.ScenarioEvidence,
                PresentationOnly = true,
            };

        private static SimulationCollectibleCardDefinitionSnapshot CloneCollectibleDefinition(
            SimulationCollectibleCardDefinitionSnapshot source) => new SimulationCollectibleCardDefinitionSnapshot
            {
                CardDefinitionStableId = source.CardDefinitionStableId,
                FamilyCode = source.FamilyCode,
                KoreanTitle = source.KoreanTitle,
                PresentationKey = source.PresentationKey,
                EvidenceKindCode = source.EvidenceKindCode,
                PresentationOnly = source.PresentationOnly,
            };

        private static SimulationCardDrawOpportunitySnapshot CloneDrawOpportunity(
            SimulationCardDrawOpportunitySnapshot source) => new SimulationCardDrawOpportunitySnapshot
            {
                OpportunityStableId = source.OpportunityStableId,
                OwnerActorStableId = source.OwnerActorStableId,
                FamilyCode = source.FamilyCode,
                TriggerCode = source.TriggerCode,
                SourceEventStableId = source.SourceEventStableId,
                StateCode = source.StateCode,
                AwardedWorldTick = source.AwardedWorldTick,
                AppliedProbabilityPercent = source.AppliedProbabilityPercent,
                ActiveRoleBonusPercentagePoints = source.ActiveRoleBonusPercentagePoints,
                PityFailureCountBefore = source.PityFailureCountBefore,
                WasGuaranteed = source.WasGuaranteed,
                DrawnCardCopyStableId = source.DrawnCardCopyStableId,
                HasExpiry = source.HasExpiry,
            };

        private static SimulationCollectibleCardCopySnapshot CloneCollectibleCard(
            SimulationCollectibleCardCopySnapshot source) => new SimulationCollectibleCardCopySnapshot
            {
                CardCopyStableId = source.CardCopyStableId,
                CardDefinitionStableId = source.CardDefinitionStableId,
                OwnerActorStableId = source.OwnerActorStableId,
                FamilyCode = source.FamilyCode,
                KoreanTitle = source.KoreanTitle,
                PresentationKey = source.PresentationKey,
                AcquiredFromOpportunityStableId = source.AcquiredFromOpportunityStableId,
                AcquiredWorldTick = source.AcquiredWorldTick,
                PresentationOnly = source.PresentationOnly,
            };

        private static SimulationCollectibleRewardEvaluationSnapshot CloneEvaluation(
            SimulationCollectibleRewardEvaluationSnapshot source)
            => new SimulationCollectibleRewardEvaluationSnapshot
            {
                EvaluationStableId = source.EvaluationStableId,
                ActorStableId = source.ActorStableId,
                FamilyCode = source.FamilyCode,
                TriggerCode = source.TriggerCode,
                SourceEventStableId = source.SourceEventStableId,
                WorldTick = source.WorldTick,
                AttemptOrdinal = source.AttemptOrdinal,
                AppliedProbabilityPercent = source.AppliedProbabilityPercent,
                ActiveRoleBonusPercentagePoints = source.ActiveRoleBonusPercentagePoints,
                PityFailureCountBefore = source.PityFailureCountBefore,
                WasGuaranteed = source.WasGuaranteed,
                DeterministicSamplePercent = source.DeterministicSamplePercent,
                ResultCode = source.ResultCode,
                OpportunityStableId = source.OpportunityStableId,
            };

        private static SimulationCollectibleCardTransferSnapshot CloneTransfer(
            SimulationCollectibleCardTransferSnapshot source)
            => new SimulationCollectibleCardTransferSnapshot
            {
                TransferStableId = source.TransferStableId,
                CommandId = source.CommandId,
                CardCopyStableId = source.CardCopyStableId,
                FromActorStableId = source.FromActorStableId,
                ToActorStableId = source.ToActorStableId,
                WorldTick = source.WorldTick,
            };

        private static SimulationWorldDiscoveryEventSnapshot CloneDiscoveryEvent(
            SimulationWorldDiscoveryEventSnapshot source)
            => new SimulationWorldDiscoveryEventSnapshot
            {
                EventStableId = source.EventStableId,
                ActorStableId = source.ActorStableId,
                TriggerCode = source.TriggerCode,
                SpatialUnitKey = source.SpatialUnitKey,
                WorldTick = source.WorldTick,
            };

        private static SimulationTileTraversalConfirmResponse CloneTraversalResponse(
            SimulationTileTraversalConfirmResponse source) => new SimulationTileTraversalConfirmResponse
            {
                Exploration = CloneWorldExplorationStateOrNull(source.Exploration)!,
                Rewards = CloneRewardState(source.Rewards),
                WasNewL2Tile = source.WasNewL2Tile,
                WasNewL1Area = source.WasNewL1Area,
                CreatedOpportunityStableIds = source.CreatedOpportunityStableIds.ToArray(),
            };

        private static SimulationCollectibleCardDrawResponse CloneDrawResponse(
            SimulationCollectibleCardDrawResponse source) => new SimulationCollectibleCardDrawResponse
            {
                DrawnCard = CloneCollectibleCard(source.DrawnCard),
                Rewards = CloneRewardState(source.Rewards),
            };

        private static SimulationCollectibleCardTransferResponse CloneTransferResponse(
            SimulationCollectibleCardTransferResponse source) => new SimulationCollectibleCardTransferResponse
            {
                TransferredCard = CloneCollectibleCard(source.TransferredCard),
                Rewards = CloneRewardState(source.Rewards),
            };

        private static L2Tile ParseL2Tile(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException("SimulationL2TileKeyInvalid");
            var parts = value.Trim().Split(':');
            if (parts.Length != 4 || parts[0] != "kr5186" || parts[1] != "l2"
                || !int.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var x)
                || !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var y))
                throw new SimulationContractException("SimulationL2TileKeyInvalid");
            return new L2Tile("kr5186:l2:" + x.ToString(CultureInfo.InvariantCulture)
                + ":" + y.ToString(CultureInfo.InvariantCulture), x, y);
        }

        private static string ParentL1Key(int l2X, int l2Y)
            => "kr5186:l1:" + FloorDivide(l2X, 4).ToString(CultureInfo.InvariantCulture)
                + ":" + FloorDivide(l2Y, 4).ToString(CultureInfo.InvariantCulture);

        private static int FloorDivide(int value, int divisor)
        {
            var quotient = value / divisor;
            var remainder = value % divisor;
            return remainder < 0 ? quotient - 1 : quotient;
        }

        private static decimal DeterministicPercent(string value)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                var integer = ((ulong)hash[0] << 56) | ((ulong)hash[1] << 48)
                    | ((ulong)hash[2] << 40) | ((ulong)hash[3] << 32)
                    | ((ulong)hash[4] << 24) | ((ulong)hash[5] << 16)
                    | ((ulong)hash[6] << 8) | hash[7];
                return Math.Round((decimal)integer / ulong.MaxValue * 100m, 6,
                    MidpointRounding.AwayFromZero);
            }
        }

        private static int DeterministicIndex(string value, int count)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
                return (int)(((uint)hash[0] << 24 | (uint)hash[1] << 16
                    | (uint)hash[2] << 8 | hash[3]) % (uint)count);
            }
        }

        private static string ShortHash(string value) => CalculateSha256(value).Substring(0, 24);

        private static string CalculateSha256(string value)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                    .Replace("-", string.Empty).ToLowerInvariant();
        }

        private sealed class CollectiblePityState
        {
            public CollectiblePityState(string actor, string family)
            {
                ActorStableId = actor;
                FamilyCode = family;
            }
            public string ActorStableId { get; }
            public string FamilyCode { get; }
            public int EligibleAttemptCount { get; set; }
            public int ConsecutiveFailures { get; set; }
        }

        private readonly struct L2Tile
        {
            public L2Tile(string key, int x, int y) { Key = key; X = x; Y = y; }
            public string Key { get; }
            public int X { get; }
            public int Y { get; }
        }

        private sealed class AppliedTraversalCommand
        {
            public AppliedTraversalCommand(string payloadKey,
                SimulationTileTraversalConfirmResponse response)
            { PayloadKey = payloadKey; Response = response; }
            public string PayloadKey { get; }
            public SimulationTileTraversalConfirmResponse Response { get; }
        }

        private sealed class AppliedDrawCommand
        {
            public AppliedDrawCommand(string payloadKey,
                SimulationCollectibleCardDrawResponse response)
            { PayloadKey = payloadKey; Response = response; }
            public string PayloadKey { get; }
            public SimulationCollectibleCardDrawResponse Response { get; }
        }

        private sealed class AppliedTransferCommand
        {
            public AppliedTransferCommand(string payloadKey,
                SimulationCollectibleCardTransferResponse response)
            { PayloadKey = payloadKey; Response = response; }
            public string PayloadKey { get; }
            public SimulationCollectibleCardTransferResponse Response { get; }
        }
    }
}
