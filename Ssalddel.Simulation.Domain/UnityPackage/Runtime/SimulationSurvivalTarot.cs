using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly List<SimulationSurvivalTarotOpportunitySnapshot>
            survivalTarotOpportunities = new List<SimulationSurvivalTarotOpportunitySnapshot>();
        private readonly Dictionary<string, AppliedSurvivalTarotCommand>
            appliedSurvivalTarotResponseCommands =
                new Dictionary<string, AppliedSurvivalTarotCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedSurvivalTarotCommand>
            appliedSurvivalTarotResolutionCommands =
                new Dictionary<string, AppliedSurvivalTarotCommand>(StringComparer.Ordinal);
        private SimulationSurvivalTarotInitialStateRequest? survivalTarotCreationState;
        private string survivalTarotInitialPayloadKey = "none";
        private bool foodCrisisOpportunityRaised;
        private bool externalExpeditionOpportunityRaised;

        public SimulationSurvivalTarotStateSnapshot GetSurvivalTarotState()
        {
            lock (gate)
            {
                return CreateSurvivalTarotStateSnapshot();
            }
        }

        public SimulationSurvivalTarotCommandResultSnapshot ConfirmSurvivalTarotResponse(
            SimulationSurvivalTarotResponseConfirmRequest request)
        {
            ValidateSurvivalTarotResponseRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildSurvivalTarotResponsePayloadKey(request);
                if (appliedSurvivalTarotResponseCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            SimulationWorldSurvivalInventoryCodes.CommandPayloadConflict);
                    return CloneSurvivalTarotCommandResult(applied.Result);
                }

                if (HasDifferentKindCommand(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                EnsureExpectedRevision(request.ExpectedRevision);
                var opportunity = FindPendingSurvivalTarotOpportunity(
                    request.OpportunityStableId);
                EnsureSurvivalTarotParticipant(opportunity, request.PlayerStableId);
                EnsureSurvivalTarotOffer(opportunity, request.OfferStableId);
                var safeBuildingStableId = EnsureParticipantsTogetherAtSafeBuilding(opportunity);

                var playerStableId = request.PlayerStableId.Trim();
                var response = opportunity.Responses.SingleOrDefault(value =>
                    string.Equals(value.PlayerStableId, playerStableId, StringComparison.Ordinal));
                if (response == null)
                {
                    response = new SimulationSurvivalTarotParticipantResponseSnapshot
                    {
                        PlayerStableId = playerStableId,
                    };
                    opportunity.Responses = opportunity.Responses.Concat(new[] { response })
                        .OrderBy(value => value.PlayerStableId, StringComparer.Ordinal)
                        .ToArray();
                }

                Revision++;
                opportunity.SafeBuildingStableId = safeBuildingStableId;
                response.OfferStableId = request.OfferStableId.Trim();
                response.RespondedWorldTick = CurrentTick;
                response.RespondedWorldRevision = Revision;
                UpdateSurvivalTarotWorldEvent(opportunity);
                var result = CreateSurvivalTarotCommandResult(commandId);
                appliedSurvivalTarotResponseCommands.Add(commandId,
                    new AppliedSurvivalTarotCommand(
                        payloadKey,
                        CloneSurvivalTarotCommandResult(result)));
                AppendSurvivalTarotResponseCommand(request);
                return result;
            }
        }

        public SimulationSurvivalTarotCommandResultSnapshot ConfirmSurvivalTarotResolution(
            SimulationSurvivalTarotResolutionConfirmRequest request)
        {
            ValidateSurvivalTarotResolutionRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildSurvivalTarotResolutionPayloadKey(request);
                if (appliedSurvivalTarotResolutionCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            SimulationWorldSurvivalInventoryCodes.CommandPayloadConflict);
                    return CloneSurvivalTarotCommandResult(applied.Result);
                }

                if (HasDifferentKindCommand(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                EnsureExpectedRevision(request.ExpectedRevision);
                var opportunity = FindPendingSurvivalTarotOpportunity(
                    request.OpportunityStableId);
                EnsureSurvivalTarotParticipant(opportunity, request.PlayerStableId);
                EnsureSurvivalTarotOffer(opportunity, request.OfferStableId);
                opportunity.SafeBuildingStableId =
                    EnsureParticipantsTogetherAtSafeBuilding(opportunity);

                var selectedOfferStableId = request.OfferStableId.Trim();
                if (opportunity.Responses.Length != opportunity.ParticipantPlayerStableIds.Length
                    || opportunity.Responses.Any(value => !string.Equals(
                        value.OfferStableId, selectedOfferStableId, StringComparison.Ordinal)))
                {
                    throw new SimulationConflictException(
                        SimulationSurvivalTarotCodes.UnanimousResponseRequired);
                }

                var offer = opportunity.Draw.Offers.Single(value => string.Equals(
                    value.OfferStableId, selectedOfferStableId, StringComparison.Ordinal));
                Revision++;
                opportunity.StatusCode = SimulationSurvivalTarotCodes.Resolved;
                opportunity.SelectedOfferStableId = selectedOfferStableId;
                opportunity.ResolvedWorldTick = CurrentTick;
                opportunity.ResolvedWorldRevision = Revision;
                opportunity.ModifierLines = CreateSurvivalTarotModifierLines(opportunity, offer);
                UpdateSurvivalTarotWorldEvent(opportunity);

                var result = CreateSurvivalTarotCommandResult(commandId);
                appliedSurvivalTarotResolutionCommands.Add(commandId,
                    new AppliedSurvivalTarotCommand(
                        payloadKey,
                        CloneSurvivalTarotCommandResult(result)));
                AppendSurvivalTarotResolutionCommand(request);
                return result;
            }
        }

        private void InitializeSurvivalTarot(SimulationSurvivalTarotInitialStateRequest? request)
        {
            ValidateSurvivalTarotInitialState(request);
            survivalTarotInitialPayloadKey = BuildSurvivalTarotPayloadKey(request);
            survivalTarotCreationState = CloneSurvivalTarotInitialState(request);
            ValidateSurvivalTarotReferences();
            EvaluateSurvivalTarotOpportunity();
        }

        private void EvaluateSurvivalTarotOpportunity()
        {
            if (survivalTarotCreationState == null
                || survivalTarotOpportunities.Any(value =>
                    value.StatusCode == SimulationSurvivalTarotCodes.Pending))
                return;

            var reservePersonDays = CalculateFoodReservePersonDays();
            var farmReservePersonDays = CalculateFarmFoodReservePersonDays();
            string? triggerCode = null;
            if (survivalTarotCreationState.FarmBuildingStableIds.Length > 0
                && !externalExpeditionOpportunityRaised
                && farmReservePersonDays
                    <= survivalTarotCreationState.FarmExitThresholdPersonDays)
            {
                triggerCode = SimulationSurvivalTarotCodes.ExternalExpeditionRequired;
                externalExpeditionOpportunityRaised = true;
                if (reservePersonDays
                    <= survivalTarotCreationState.FoodCrisisThresholdPersonDays)
                    foodCrisisOpportunityRaised = true;
            }
            else if (!foodCrisisOpportunityRaised
                && reservePersonDays
                    <= survivalTarotCreationState.FoodCrisisThresholdPersonDays)
            {
                triggerCode = SimulationSurvivalTarotCodes.FoodReserveCrisis;
                foodCrisisOpportunityRaised = true;
            }
            else if (CurrentTick > 0
                && CurrentTick % survivalTarotCreationState.PeriodicIntervalTicks == 0
                && !survivalTarotOpportunities.Any(value =>
                    value.TriggerCode == SimulationSurvivalTarotCodes.Periodic
                    && value.TriggeredWorldTick == CurrentTick))
            {
                triggerCode = SimulationSurvivalTarotCodes.Periodic;
            }

            if (triggerCode == null) return;
            var priorSelections = survivalTarotOpportunities
                .Where(value => value.StatusCode == SimulationSurvivalTarotCodes.Resolved)
                .Select(value => value.SelectedOfferStableId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
            var draw = new Simulation타로카드뽑기().Draw(
                ScenarioSeed,
                CurrentTick + 1,
                priorSelections);
            var opportunity = new SimulationSurvivalTarotOpportunitySnapshot
            {
                OpportunityStableId = "survival-tarot-opportunity:"
                    + CurrentTick.ToString(CultureInfo.InvariantCulture) + ":"
                    + triggerCode.ToLowerInvariant(),
                TriggerCode = triggerCode,
                StatusCode = SimulationSurvivalTarotCodes.Pending,
                TriggeredWorldTick = CurrentTick,
                FoodReservePersonDays = reservePersonDays,
                FarmFoodReservePersonDays = farmReservePersonDays,
                RequiresExternalExpedition = triggerCode
                    == SimulationSurvivalTarotCodes.ExternalExpeditionRequired,
                ParticipantPlayerStableIds = survivalTarotCreationState
                    .ParticipantPlayerStableIds.OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                Draw = CloneTarotDraw(draw),
            };
            survivalTarotOpportunities.Add(opportunity);
            RegisterSurvivalTarotWorldEvent(opportunity);
        }

        private decimal CalculateFoodReservePersonDays()
        {
            if (survivalTarotCreationState == null) return 0m;
            var foodCodes = new HashSet<string>(
                survivalTarotCreationState.FoodItemCodes,
                StringComparer.Ordinal);
            var foodUnits = worldInventoryItemStacks.Values
                    .Where(value => foodCodes.Contains(value.ItemCode))
                    .Sum(value => value.Quantity)
                + worldInventoryPlayerItems.Values
                    .Where(value => foodCodes.Contains(value.ItemCode))
                    .Sum(value => value.Quantity);
            var dailyNeed = survivalTarotCreationState.FoodUnitsPerPlayerDay
                * survivalTarotCreationState.ParticipantPlayerStableIds.Length;
            return dailyNeed <= 0m ? 0m : foodUnits / dailyNeed;
        }

        private decimal CalculateFarmFoodReservePersonDays()
        {
            if (survivalTarotCreationState == null
                || survivalTarotCreationState.FarmBuildingStableIds.Length == 0)
                return 0m;
            var farmBuildings = new HashSet<string>(
                survivalTarotCreationState.FarmBuildingStableIds,
                StringComparer.Ordinal);
            var foodCodes = new HashSet<string>(
                survivalTarotCreationState.FoodItemCodes,
                StringComparer.Ordinal);
            var inventory = CreateWorldInventorySnapshot();
            var farmContainerIds = new HashSet<string>(inventory.Containers
                .Where(value => farmBuildings.Contains(value.BuildingStableId))
                .Select(value => value.ContainerStableId), StringComparer.Ordinal);
            var containerFoodUnits = inventory.ContainerItemStacks
                .Where(value => farmContainerIds.Contains(value.ContainerStableId)
                    && foodCodes.Contains(value.ItemCode))
                .Sum(value => value.Quantity);
            var carriedFarmFoodUnits = inventory.Players
                .Where(value => farmBuildings.Contains(value.CurrentBuildingStableId))
                .SelectMany(value => value.Items)
                .Where(value => foodCodes.Contains(value.ItemCode))
                .Sum(value => value.Quantity);
            var dailyNeed = survivalTarotCreationState.FoodUnitsPerPlayerDay
                * survivalTarotCreationState.ParticipantPlayerStableIds.Length;
            return dailyNeed <= 0m
                ? 0m
                : (containerFoodUnits + carriedFarmFoodUnits) / dailyNeed;
        }

        private SimulationSurvivalTarotOpportunitySnapshot FindPendingSurvivalTarotOpportunity(
            string opportunityStableId)
        {
            var opportunity = survivalTarotOpportunities.SingleOrDefault(value => string.Equals(
                value.OpportunityStableId,
                opportunityStableId.Trim(),
                StringComparison.Ordinal));
            if (opportunity == null)
                throw new SimulationNotFoundException(
                    SimulationSurvivalTarotCodes.OpportunityNotFound);
            if (opportunity.StatusCode != SimulationSurvivalTarotCodes.Pending)
                throw new SimulationConflictException(
                    SimulationSurvivalTarotCodes.OpportunityAlreadyResolved);
            return opportunity;
        }

        private static void EnsureSurvivalTarotParticipant(
            SimulationSurvivalTarotOpportunitySnapshot opportunity,
            string playerStableId)
        {
            if (!opportunity.ParticipantPlayerStableIds.Contains(
                playerStableId.Trim(), StringComparer.Ordinal))
            {
                throw new SimulationConflictException(
                    SimulationSurvivalTarotCodes.ParticipantNotFound);
            }
        }

        private static void EnsureSurvivalTarotOffer(
            SimulationSurvivalTarotOpportunitySnapshot opportunity,
            string offerStableId)
        {
            if (!opportunity.Draw.Offers.Any(value => string.Equals(
                value.OfferStableId, offerStableId.Trim(), StringComparison.Ordinal)))
            {
                throw new SimulationConflictException(
                    SimulationSurvivalTarotCodes.OfferNotFound);
            }
        }

        private string EnsureParticipantsTogetherAtSafeBuilding(
            SimulationSurvivalTarotOpportunitySnapshot opportunity)
        {
            if (survivalTarotCreationState == null)
                throw new SimulationConflictException(
                    SimulationSurvivalTarotCodes.SafeBuildingRequired);
            var buildings = opportunity.ParticipantPlayerStableIds
                .Select(value => worldInventoryPlayers[value].CurrentBuildingStableId)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (buildings.Length != 1)
                throw new SimulationConflictException(
                    SimulationSurvivalTarotCodes.ParticipantsNotTogether);
            if (!survivalTarotCreationState.SafeBuildingStableIds.Contains(
                buildings[0], StringComparer.Ordinal))
            {
                throw new SimulationConflictException(
                    SimulationSurvivalTarotCodes.SafeBuildingRequired);
            }
            return buildings[0];
        }

        private Simulation타로규칙보정선Snapshot[] CreateSurvivalTarotModifierLines(
            SimulationSurvivalTarotOpportunitySnapshot opportunity,
            Simulation타로CardOfferSnapshot offer)
        {
            var upright = offer.OrientationCode == Simulation타로카드방향Codes.Upright;
            var activeTurn = CurrentTick + 1;
            var cardStableId = offer.Card.CardStableId;
            if (cardStableId == "tarot:major.empress")
            {
                return new[]
                {
                    Modifier(opportunity, offer, "food-discovery", Simulation업무규칙영역Codes.Production,
                        upright ? 1.25m : 1.10m, Simulation타로보정의미Codes.Opportunity, activeTurn),
                    Modifier(opportunity, offer, "carry-time-cost", Simulation업무규칙영역Codes.Time,
                        upright ? 1.15m : 1.25m, Simulation타로보정의미Codes.Burden, activeTurn),
                };
            }
            if (cardStableId == "tarot:major.chariot")
            {
                return new[]
                {
                    Modifier(opportunity, offer, "movement-speed", Simulation업무규칙영역Codes.Transport,
                        upright ? 1.25m : 1.10m, Simulation타로보정의미Codes.Opportunity, activeTurn),
                    Modifier(opportunity, offer, "stamina-consumption", Simulation업무규칙영역Codes.Consumption,
                        upright ? 1.20m : 1.30m, Simulation타로보정의미Codes.Burden, activeTurn),
                };
            }
            if (cardStableId == "tarot:major.justice")
            {
                return new[]
                {
                    Modifier(opportunity, offer, "fair-share", Simulation업무규칙영역Codes.Warehouse,
                        upright ? 1.25m : 1.10m, Simulation타로보정의미Codes.Opportunity, activeTurn),
                    Modifier(opportunity, offer, "immediate-take-limit", Simulation업무규칙영역Codes.Warehouse,
                        upright ? 0.75m : 0.60m, Simulation타로보정의미Codes.Burden, activeTurn),
                };
            }
            return new[]
            {
                Modifier(opportunity, offer, "resource-consumption", Simulation업무규칙영역Codes.Consumption,
                    upright ? 0.80m : 0.90m, Simulation타로보정의미Codes.Opportunity, activeTurn),
                Modifier(opportunity, offer, "movement-speed", Simulation업무규칙영역Codes.Transport,
                    upright ? 0.90m : 0.80m, Simulation타로보정의미Codes.Burden, activeTurn),
            };
        }

        private Simulation타로규칙보정선Snapshot Modifier(
            SimulationSurvivalTarotOpportunitySnapshot opportunity,
            Simulation타로CardOfferSnapshot offer,
            string ruleCode,
            string domainCode,
            decimal modifierValue,
            string meaningCode,
            int activeTurn)
            => new Simulation타로규칙보정선Snapshot
            {
                ModifierLineStableId = opportunity.OpportunityStableId + ":modifier:" + ruleCode,
                UpperRuleStableId = "simulation-rule:survival-tarot",
                UpperRuleRevision = 1,
                SourceCardStableId = offer.Card.CardStableId,
                SourceCardRevision = offer.Card.CardRevision,
                CardOrientationCode = offer.OrientationCode,
                ResponseStableId = offer.OfferStableId,
                TargetConnectionPointStableId = "connection:survival:" + ruleCode,
                TargetRuleDomainCode = domainCode,
                CompatibleLowerRuleStableId = "simulation-rule:survival:" + ruleCode,
                CompatibleLowerRuleRevision = 1,
                CalculationKindCode = Simulation타로보정계산방식Codes.Multiplier,
                ModifierValue = modifierValue,
                ModifierUnitCode = "ratio",
                MeaningCode = meaningCode,
                ActiveFromTurnNumber = activeTurn,
                ActiveThroughTurnNumber = activeTurn,
                SourceTurnClosingStableId = opportunity.OpportunityStableId,
                SourceStableIds = new[]
                {
                    opportunity.OpportunityStableId,
                    survivalTarotCreationState?.RuleRevision ?? SimulationSurvivalTarotCodes.RuleRevision,
                },
            };

        private SimulationSurvivalTarotStateSnapshot CreateSurvivalTarotStateSnapshot()
        {
            var pending = survivalTarotOpportunities.SingleOrDefault(value =>
                value.StatusCode == SimulationSurvivalTarotCodes.Pending);
            var farmReservePersonDays = CalculateFarmFoodReservePersonDays();
            return new SimulationSurvivalTarotStateSnapshot
            {
                SessionStableId = SessionStableId,
                RuleRevision = survivalTarotCreationState?.RuleRevision ?? string.Empty,
                WorldTick = CurrentTick,
                WorldRevision = Revision,
                PeriodicIntervalTicks = survivalTarotCreationState?.PeriodicIntervalTicks ?? 0,
                FoodCrisisThresholdPersonDays =
                    survivalTarotCreationState?.FoodCrisisThresholdPersonDays ?? 0m,
                FarmExitThresholdPersonDays =
                    survivalTarotCreationState?.FarmExitThresholdPersonDays ?? 0m,
                CurrentFoodReservePersonDays = CalculateFoodReservePersonDays(),
                CurrentFarmFoodReservePersonDays = farmReservePersonDays,
                FarmScopeConfigured = survivalTarotCreationState != null
                    && survivalTarotCreationState.FarmBuildingStableIds.Length > 0,
                RequiresExternalExpedition = survivalTarotCreationState != null
                    && survivalTarotCreationState.FarmBuildingStableIds.Length > 0
                    && farmReservePersonDays
                        <= survivalTarotCreationState.FarmExitThresholdPersonDays,
                PendingOpportunity = pending == null ? null : CloneSurvivalTarotOpportunity(pending),
                OpportunityHistory = survivalTarotOpportunities
                    .Select(CloneSurvivalTarotOpportunity).ToArray(),
                ActiveModifierLines = survivalTarotOpportunities
                    .Where(value => value.StatusCode == SimulationSurvivalTarotCodes.Resolved)
                    .SelectMany(value => value.ModifierLines)
                    .Where(value => CurrentTick >= value.ActiveFromTurnNumber
                        && CurrentTick <= value.ActiveThroughTurnNumber)
                    .Select(CloneTarotModifierLine)
                    .ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private SimulationSurvivalTarotCommandResultSnapshot CreateSurvivalTarotCommandResult(
            string commandId)
            => new SimulationSurvivalTarotCommandResultSnapshot
            {
                CommandId = commandId,
                AppliedWorldTick = CurrentTick,
                AppliedWorldRevision = Revision,
                State = CreateSurvivalTarotStateSnapshot(),
                SimulationOnly = true,
                IsOperationalState = false,
            };

        internal static void ValidateSurvivalTarotInitialState(
            SimulationSurvivalTarotInitialStateRequest? request)
        {
            if (request == null) return;
            RequireStableId(request.RuleRevision, "SimulationSurvivalTarotRuleRevisionInvalid");
            if (request.IsOperationalState)
                throw new SimulationContractException(
                    "SimulationSurvivalTarotOperationalStateForbidden");
            if (request.PeriodicIntervalTicks <= 0 || request.PeriodicIntervalTicks > 28)
                throw new SimulationContractException(
                    "SimulationSurvivalTarotPeriodicIntervalInvalid");
            if (request.FoodCrisisThresholdPersonDays < 0m
                || request.FarmExitThresholdPersonDays < 0m
                || request.FoodUnitsPerPlayerDay <= 0m)
                throw new SimulationContractException(
                    "SimulationSurvivalTarotFoodThresholdInvalid");
            ValidateUniqueStableIds(request.FoodItemCodes,
                "SimulationSurvivalTarotFoodItemCodesInvalid");
            if (request.FarmBuildingStableIds == null)
                throw new SimulationContractException(
                    "SimulationSurvivalTarotFarmBuildingsInvalid");
            if (request.FarmBuildingStableIds.Length > 0)
                ValidateUniqueStableIds(request.FarmBuildingStableIds,
                    "SimulationSurvivalTarotFarmBuildingsInvalid");
            ValidateUniqueStableIds(request.SafeBuildingStableIds,
                "SimulationSurvivalTarotSafeBuildingsInvalid");
            ValidateUniqueStableIds(request.ParticipantPlayerStableIds,
                "SimulationSurvivalTarotParticipantsInvalid");
        }

        private void ValidateSurvivalTarotReferences()
        {
            if (survivalTarotCreationState == null) return;
            if (survivalTarotCreationState.SafeBuildingStableIds.Any(value =>
                !worldInventoryBuildings.ContainsKey(value)))
                throw new SimulationContractException(
                    "SimulationSurvivalTarotSafeBuildingReferenceInvalid");
            if (survivalTarotCreationState.FarmBuildingStableIds.Any(value =>
                !worldInventoryBuildings.ContainsKey(value)))
                throw new SimulationContractException(
                    "SimulationSurvivalTarotFarmBuildingReferenceInvalid");
            if (survivalTarotCreationState.ParticipantPlayerStableIds.Any(value =>
                !worldInventoryPlayers.ContainsKey(value)))
                throw new SimulationContractException(
                    "SimulationSurvivalTarotParticipantReferenceInvalid");
        }

        internal static void ValidateSurvivalTarotResponseRequest(
            SimulationSurvivalTarotResponseConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            RequireStableId(request.OpportunityStableId,
                "SimulationSurvivalTarotOpportunityStableIdInvalid");
            RequireStableId(request.PlayerStableId,
                "SimulationSurvivalTarotPlayerStableIdInvalid");
            RequireStableId(request.OfferStableId,
                "SimulationSurvivalTarotOfferStableIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
        }

        internal static void ValidateSurvivalTarotResolutionRequest(
            SimulationSurvivalTarotResolutionConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            RequireStableId(request.OpportunityStableId,
                "SimulationSurvivalTarotOpportunityStableIdInvalid");
            RequireStableId(request.PlayerStableId,
                "SimulationSurvivalTarotPlayerStableIdInvalid");
            RequireStableId(request.OfferStableId,
                "SimulationSurvivalTarotOfferStableIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
        }

        internal static string BuildSurvivalTarotPayloadKey(
            SimulationSurvivalTarotInitialStateRequest? request)
        {
            if (request == null) return "none";
            var parts = new List<string>
            {
                request.RuleRevision.Trim(),
                request.PeriodicIntervalTicks.ToString(CultureInfo.InvariantCulture),
                request.FoodCrisisThresholdPersonDays.ToString(CultureInfo.InvariantCulture),
                request.FoodUnitsPerPlayerDay.ToString(CultureInfo.InvariantCulture),
                string.Join(",", request.FoodItemCodes.OrderBy(value => value, StringComparer.Ordinal)),
                string.Join(",", request.SafeBuildingStableIds.OrderBy(value => value, StringComparer.Ordinal)),
                string.Join(",", request.ParticipantPlayerStableIds.OrderBy(value => value, StringComparer.Ordinal)),
                request.IsOperationalState.ToString(CultureInfo.InvariantCulture),
            };
            if (request.FarmBuildingStableIds.Length > 0)
            {
                parts.Add("FarmExitExtensionV1");
                parts.Add(request.FarmExitThresholdPersonDays
                    .ToString(CultureInfo.InvariantCulture));
                parts.Add(string.Join(",", request.FarmBuildingStableIds
                    .OrderBy(value => value, StringComparer.Ordinal)));
            }
            return string.Join("|", parts);
        }

        internal static SimulationSurvivalTarotInitialStateRequest? CloneSurvivalTarotInitialState(
            SimulationSurvivalTarotInitialStateRequest? source)
            => source == null ? null : new SimulationSurvivalTarotInitialStateRequest
            {
                RuleRevision = source.RuleRevision,
                PeriodicIntervalTicks = source.PeriodicIntervalTicks,
                FoodCrisisThresholdPersonDays = source.FoodCrisisThresholdPersonDays,
                FarmExitThresholdPersonDays = source.FarmExitThresholdPersonDays,
                FoodUnitsPerPlayerDay = source.FoodUnitsPerPlayerDay,
                FoodItemCodes = source.FoodItemCodes.ToArray(),
                FarmBuildingStableIds = source.FarmBuildingStableIds.ToArray(),
                SafeBuildingStableIds = source.SafeBuildingStableIds.ToArray(),
                ParticipantPlayerStableIds = source.ParticipantPlayerStableIds.ToArray(),
                IsOperationalState = source.IsOperationalState,
            };

        internal static string BuildSurvivalTarotResponsePayloadKey(
            SimulationSurvivalTarotResponseConfirmRequest request)
            => string.Join("|", request.OpportunityStableId.Trim(),
                request.PlayerStableId.Trim(), request.OfferStableId.Trim());

        internal static string BuildSurvivalTarotResolutionPayloadKey(
            SimulationSurvivalTarotResolutionConfirmRequest request)
            => string.Join("|", request.OpportunityStableId.Trim(),
                request.PlayerStableId.Trim(), request.OfferStableId.Trim());

        private static void ValidateUniqueStableIds(string[] values, string errorCode)
        {
            if (values == null || values.Length == 0)
                throw new SimulationContractException(errorCode);
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                RequireStableId(value, errorCode);
                if (!unique.Add(value.Trim()))
                    throw new SimulationContractException(errorCode);
            }
        }

        private void EnsureExpectedRevision(long expectedRevision)
        {
            if (expectedRevision != Revision)
                throw new SimulationConflictException(
                    SimulationWorldSurvivalInventoryCodes.ExpectedRevisionMismatch);
        }

        private bool HasAppliedSurvivalTarotCommand(string commandId)
            => appliedSurvivalTarotResponseCommands.ContainsKey(commandId)
                || appliedSurvivalTarotResolutionCommands.ContainsKey(commandId);

        private static SimulationSurvivalTarotCommandResultSnapshot
            CloneSurvivalTarotCommandResult(SimulationSurvivalTarotCommandResultSnapshot source)
            => new SimulationSurvivalTarotCommandResultSnapshot
            {
                CommandId = source.CommandId,
                AppliedWorldTick = source.AppliedWorldTick,
                AppliedWorldRevision = source.AppliedWorldRevision,
                State = CloneSurvivalTarotState(source.State),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        internal static SimulationSurvivalTarotStateSnapshot CloneSurvivalTarotState(
            SimulationSurvivalTarotStateSnapshot source)
            => new SimulationSurvivalTarotStateSnapshot
            {
                SessionStableId = source.SessionStableId,
                RuleRevision = source.RuleRevision,
                WorldTick = source.WorldTick,
                WorldRevision = source.WorldRevision,
                PeriodicIntervalTicks = source.PeriodicIntervalTicks,
                FoodCrisisThresholdPersonDays = source.FoodCrisisThresholdPersonDays,
                FarmExitThresholdPersonDays = source.FarmExitThresholdPersonDays,
                CurrentFoodReservePersonDays = source.CurrentFoodReservePersonDays,
                CurrentFarmFoodReservePersonDays = source.CurrentFarmFoodReservePersonDays,
                FarmScopeConfigured = source.FarmScopeConfigured,
                RequiresExternalExpedition = source.RequiresExternalExpedition,
                CalendarRuleCode = source.CalendarRuleCode,
                PendingOpportunity = source.PendingOpportunity == null
                    ? null : CloneSurvivalTarotOpportunity(source.PendingOpportunity),
                OpportunityHistory = source.OpportunityHistory
                    .Select(CloneSurvivalTarotOpportunity).ToArray(),
                ActiveModifierLines = source.ActiveModifierLines
                    .Select(CloneTarotModifierLine).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private static SimulationSurvivalTarotOpportunitySnapshot CloneSurvivalTarotOpportunity(
            SimulationSurvivalTarotOpportunitySnapshot source)
            => new SimulationSurvivalTarotOpportunitySnapshot
            {
                OpportunityStableId = source.OpportunityStableId,
                TriggerCode = source.TriggerCode,
                StatusCode = source.StatusCode,
                TriggeredWorldTick = source.TriggeredWorldTick,
                FoodReservePersonDays = source.FoodReservePersonDays,
                FarmFoodReservePersonDays = source.FarmFoodReservePersonDays,
                RequiresExternalExpedition = source.RequiresExternalExpedition,
                SafeBuildingStableId = source.SafeBuildingStableId,
                ParticipantPlayerStableIds = source.ParticipantPlayerStableIds.ToArray(),
                Draw = CloneTarotDraw(source.Draw),
                Responses = source.Responses.Select(value =>
                    new SimulationSurvivalTarotParticipantResponseSnapshot
                    {
                        PlayerStableId = value.PlayerStableId,
                        OfferStableId = value.OfferStableId,
                        RespondedWorldTick = value.RespondedWorldTick,
                        RespondedWorldRevision = value.RespondedWorldRevision,
                    }).ToArray(),
                SelectedOfferStableId = source.SelectedOfferStableId,
                ResolvedWorldTick = source.ResolvedWorldTick,
                ResolvedWorldRevision = source.ResolvedWorldRevision,
                ModifierLines = source.ModifierLines.Select(CloneTarotModifierLine).ToArray(),
            };

        private static Simulation타로DrawSnapshot CloneTarotDraw(Simulation타로DrawSnapshot source)
            => new Simulation타로DrawSnapshot
            {
                DrawStableId = source.DrawStableId,
                DeckStableId = source.DeckStableId,
                DeckRevision = source.DeckRevision,
                DrawRuleRevision = source.DrawRuleRevision,
                TurnNumber = source.TurnNumber,
                TurnHistoryHash = source.TurnHistoryHash,
                Offers = source.Offers.Select(value => new Simulation타로CardOfferSnapshot
                {
                    OfferStableId = value.OfferStableId,
                    OfferSlotNumber = value.OfferSlotNumber,
                    CardCopyStableId = value.CardCopyStableId,
                    OrientationCode = value.OrientationCode,
                    Card = new SimulationTurnCardSnapshot
                    {
                        CardStableId = value.Card.CardStableId,
                        CardRevision = value.Card.CardRevision,
                        CardKindCode = value.Card.CardKindCode,
                        Title = value.Card.Title,
                        Summary = value.Card.Summary,
                        EffectTimingCode = value.Card.EffectTimingCode,
                        EffectCode = value.Card.EffectCode,
                        TargetStatCode = value.Card.TargetStatCode,
                        StatDelta = value.Card.StatDelta,
                        SourceStableId = value.Card.SourceStableId,
                        EffectRuleRevision = value.Card.EffectRuleRevision,
                    },
                }).ToArray(),
            };

        private static Simulation타로규칙보정선Snapshot CloneTarotModifierLine(
            Simulation타로규칙보정선Snapshot source)
            => new Simulation타로규칙보정선Snapshot
            {
                ModifierLineStableId = source.ModifierLineStableId,
                UpperRuleStableId = source.UpperRuleStableId,
                UpperRuleRevision = source.UpperRuleRevision,
                SourceCardStableId = source.SourceCardStableId,
                SourceCardRevision = source.SourceCardRevision,
                CardOrientationCode = source.CardOrientationCode,
                ResponseStableId = source.ResponseStableId,
                TargetConnectionPointStableId = source.TargetConnectionPointStableId,
                TargetRuleDomainCode = source.TargetRuleDomainCode,
                CompatibleLowerRuleStableId = source.CompatibleLowerRuleStableId,
                CompatibleLowerRuleRevision = source.CompatibleLowerRuleRevision,
                CalculationKindCode = source.CalculationKindCode,
                ModifierValue = source.ModifierValue,
                ModifierUnitCode = source.ModifierUnitCode,
                MeaningCode = source.MeaningCode,
                ActiveFromTurnNumber = source.ActiveFromTurnNumber,
                ActiveThroughTurnNumber = source.ActiveThroughTurnNumber,
                SourceTurnClosingStableId = source.SourceTurnClosingStableId,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };

        private sealed class AppliedSurvivalTarotCommand
        {
            public AppliedSurvivalTarotCommand(
                string payloadKey,
                SimulationSurvivalTarotCommandResultSnapshot result)
            {
                PayloadKey = payloadKey;
                Result = result;
            }

            public string PayloadKey { get; }
            public SimulationSurvivalTarotCommandResultSnapshot Result { get; }
        }
    }
}
