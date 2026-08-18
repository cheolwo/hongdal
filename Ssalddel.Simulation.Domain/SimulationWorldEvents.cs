using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly List<SimulationWorldEventSnapshot> worldEvents =
            new List<SimulationWorldEventSnapshot>();

        public SimulationWorldEventProjectionSnapshot GetWorldEvents(long afterWorldRevision)
        {
            lock (gate)
            {
                if (afterWorldRevision < -1)
                    throw new SimulationContractException(
                        "SimulationWorldEventAfterRevisionInvalid");
                if (afterWorldRevision > Revision)
                    throw new SimulationConflictException(
                        "SimulationWorldEventAfterRevisionAhead");

                return new SimulationWorldEventProjectionSnapshot
                {
                    SessionStableId = SessionStableId,
                    WorldTick = CurrentTick,
                    WorldRevision = Revision,
                    AfterWorldRevision = afterWorldRevision,
                    NextAfterWorldRevision = Revision,
                    HasMore = false,
                    Events = worldEvents
                        .Where(value => value.VisibleFromWorldTick <= CurrentTick
                            && value.LastChangedWorldRevision > afterWorldRevision)
                        .OrderBy(value => value.OccurredWorldTick)
                        .ThenBy(value => value.EventStableId, StringComparer.Ordinal)
                        .Select(CloneWorldEvent)
                        .ToArray(),
                    SimulationOnly = true,
                    IsOperationalState = false,
                    PresentationOnly = true,
                };
            }
        }

        private void RegisterSurvivalTarotWorldEvent(
            SimulationSurvivalTarotOpportunitySnapshot opportunity)
        {
            if (survivalTarotCreationState == null)
                return;
            var activeBuildingStableId = FindCommonSafeBuildingStableId();
            var anchors = survivalTarotCreationState.SafeBuildingStableIds
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            worldEvents.Add(new SimulationWorldEventSnapshot
            {
                EventStableId = "world-event:" + opportunity.OpportunityStableId,
                EventRevision = 1,
                LastChangedWorldRevision = Revision,
                EventTypeCode = SimulationWorldEventCodes.SurvivalTarotOpportunity,
                TriggerCode = opportunity.TriggerCode,
                StateCode = SimulationWorldEventCodes.AwaitingResponse,
                OccurredWorldTick = opportunity.TriggeredWorldTick,
                VisibleFromWorldTick = opportunity.TriggeredWorldTick,
                AudienceScopeCode = SimulationWorldEventCodes.SessionParticipants,
                PresentationKey = PresentationKey(opportunity.TriggerCode),
                ResponseKindCode = SimulationWorldEventCodes.SurvivalTarotConsensus,
                SourceOpportunityStableId = opportunity.OpportunityStableId,
                ChoiceSetStableId = opportunity.Draw.DrawStableId,
                Choices = opportunity.Draw.Offers
                    .OrderBy(value => value.OfferSlotNumber)
                    .Select(value => new SimulationWorldEventChoiceSnapshot
                    {
                        ChoiceStableId = value.OfferStableId,
                        DisplayOrder = value.OfferSlotNumber,
                        CardStableId = value.Card.CardStableId,
                        CardRevision = value.Card.CardRevision,
                        OrientationCode = value.OrientationCode,
                        KoreanTitle = value.Card.Title,
                        KoreanSummary = value.Card.Summary,
                    }).ToArray(),
                ActiveBuildingStableId = activeBuildingStableId,
                AnchorBuildingStableIds = anchors,
                TileKeys = anchors.Select(value => worldInventoryBuildings[value].TileKey)
                    .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                RegionStableIds = anchors
                    .Select(value => worldInventoryBuildings[value].RegionStableId)
                    .Distinct(StringComparer.Ordinal).OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
                ParticipantPlayerStableIds = opportunity.ParticipantPlayerStableIds.ToArray(),
                RespondedParticipantCount = opportunity.Responses.Length,
                RequiredParticipantCount = opportunity.ParticipantPlayerStableIds.Length,
                CanRespond = !string.IsNullOrWhiteSpace(activeBuildingStableId),
                RequiresUnanimousResponse = true,
                RequiresExpectedRevision = true,
                RuleRevision = survivalTarotCreationState.RuleRevision,
                SourceStableIds = new[]
                {
                    opportunity.OpportunityStableId,
                    opportunity.Draw.DrawStableId,
                    survivalTarotCreationState.RuleRevision,
                },
                SimulationOnly = true,
                IsOperationalState = false,
                PresentationOnly = true,
            });
        }

        private void UpdateSurvivalTarotWorldEvent(
            SimulationSurvivalTarotOpportunitySnapshot opportunity)
        {
            var value = worldEvents.Single(item => string.Equals(
                item.SourceOpportunityStableId,
                opportunity.OpportunityStableId,
                StringComparison.Ordinal));
            value.EventRevision++;
            value.LastChangedWorldRevision = Revision;
            value.StateCode = opportunity.StatusCode == SimulationSurvivalTarotCodes.Resolved
                ? SimulationWorldEventCodes.Resolved
                : SimulationWorldEventCodes.AwaitingResponse;
            value.RespondedParticipantCount = opportunity.Responses.Length;
            value.SelectedChoiceStableId = opportunity.SelectedOfferStableId;
            value.ActiveBuildingStableId = opportunity.SafeBuildingStableId;
            value.CanRespond = opportunity.StatusCode != SimulationSurvivalTarotCodes.Resolved
                && !string.IsNullOrWhiteSpace(opportunity.SafeBuildingStableId);
        }

        private void RegisterFarmThreatWorldEvent(
            SimulationThreatEncounterSnapshot encounter,
            SimulationWorldEventChoiceSnapshot[] choices)
        {
            if (farmSurvivalCreationState == null) return;
            var players = farmActors.Values
                .Where(value => value.ActorKindCode == SimulationFarmSurvivalCodes.Player)
                .Select(value => value.ActorStableId)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            worldEvents.Add(new SimulationWorldEventSnapshot
            {
                EventStableId = "world-event:" + encounter.EncounterStableId,
                EventRevision = 1,
                LastChangedWorldRevision = Revision,
                EventTypeCode = SimulationWorldEventCodes.FarmThreatEncounter,
                TriggerCode = encounter.ThreatTypeCode,
                StateCode = encounter.StateCode,
                OccurredWorldTick = encounter.OccurredWorldTick,
                VisibleFromWorldTick = encounter.OccurredWorldTick,
                ExpiresAfterWorldTick = encounter.DecisionDeadlineWorldTick,
                AudienceScopeCode = SimulationWorldEventCodes.SessionParticipants,
                PresentationKey = encounter.PresentationKey,
                ResponseKindCode = choices.Length > 0
                    ? SimulationWorldEventCodes.FarmThreatChoice : string.Empty,
                SourceOpportunityStableId = encounter.EncounterStableId,
                ChoiceSetStableId = choices.Length > 0
                    ? "choice-set:" + encounter.EncounterStableId : string.Empty,
                Choices = choices.Select(value => new SimulationWorldEventChoiceSnapshot
                {
                    ChoiceStableId = value.ChoiceStableId,
                    DisplayOrder = value.DisplayOrder,
                    KoreanTitle = value.KoreanTitle,
                    KoreanSummary = value.KoreanSummary,
                }).ToArray(),
                ActiveBuildingStableId = farmSurvivalCreationState.FarmBuildingStableId,
                AnchorBuildingStableIds = new[]
                {
                    farmSurvivalCreationState.FarmBuildingStableId,
                },
                TileKeys = new[] { farmSurvivalCreationState.TileKey },
                RegionStableIds = new[] { farmSurvivalCreationState.RegionStableId },
                ParticipantPlayerStableIds = players,
                RequiredParticipantCount = choices.Length > 0 ? 1 : 0,
                CanRespond = choices.Length > 0,
                RequiresUnanimousResponse = false,
                RequiresExpectedRevision = choices.Length > 0,
                RuleRevision = farmSurvivalCreationState.RuleRevision,
                SourceStableIds = new[]
                {
                    encounter.EncounterStableId,
                    farmSurvivalCreationState.AreaStableId,
                    farmSurvivalCreationState.TileKey,
                },
                SimulationOnly = true,
                IsOperationalState = false,
                PresentationOnly = true,
            });
        }

        private void UpdateFarmThreatWorldEvent(
            SimulationThreatEncounterSnapshot encounter,
            SimulationWorldEventChoiceSnapshot[]? choices = null)
        {
            var worldEvent = worldEvents.Single(value => string.Equals(
                value.SourceOpportunityStableId,
                encounter.EncounterStableId,
                StringComparison.Ordinal));
            worldEvent.EventRevision++;
            worldEvent.LastChangedWorldRevision = Revision;
            worldEvent.StateCode = encounter.StateCode;
            worldEvent.PresentationKey = encounter.PresentationKey;
            worldEvent.ExpiresAfterWorldTick = encounter.DecisionDeadlineWorldTick;
            worldEvent.SelectedChoiceStableId = encounter.SelectedChoiceStableId;
            if (choices != null)
            {
                worldEvent.ResponseKindCode =
                    SimulationWorldEventCodes.FarmThreatChoice;
                worldEvent.ChoiceSetStableId =
                    "choice-set:" + encounter.EncounterStableId;
                worldEvent.Choices = choices.Select(value =>
                    new SimulationWorldEventChoiceSnapshot
                    {
                        ChoiceStableId = value.ChoiceStableId,
                        DisplayOrder = value.DisplayOrder,
                        KoreanTitle = value.KoreanTitle,
                        KoreanSummary = value.KoreanSummary,
                    }).ToArray();
                worldEvent.RequiredParticipantCount = 1;
                worldEvent.RequiresExpectedRevision = true;
            }
            worldEvent.RespondedParticipantCount =
                string.IsNullOrWhiteSpace(encounter.SelectedChoiceStableId) ? 0 : 1;
            worldEvent.CanRespond =
                encounter.StateCode == SimulationFarmSurvivalCodes.AwaitingResponse
                || encounter.StateCode ==
                    SimulationFarmSurvivalCodes.AwaitingDefenseChoice;
        }

        private string FindCommonSafeBuildingStableId()
        {
            if (survivalTarotCreationState == null) return string.Empty;
            var buildings = survivalTarotCreationState.ParticipantPlayerStableIds
                .Where(worldInventoryPlayers.ContainsKey)
                .Select(value => worldInventoryPlayers[value].CurrentBuildingStableId)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return buildings.Length == 1
                && survivalTarotCreationState.SafeBuildingStableIds.Contains(
                    buildings[0], StringComparer.Ordinal)
                ? buildings[0]
                : string.Empty;
        }

        private static string PresentationKey(string triggerCode)
            => triggerCode == SimulationSurvivalTarotCodes.ExternalExpeditionRequired
                ? SimulationWorldEventCodes.ExternalExpeditionPresentation
                : triggerCode == SimulationSurvivalTarotCodes.FoodReserveCrisis
                    ? SimulationWorldEventCodes.FoodReserveCrisisPresentation
                    : SimulationWorldEventCodes.PeriodicTarotPresentation;

        private static SimulationWorldEventSnapshot CloneWorldEvent(
            SimulationWorldEventSnapshot source)
            => new SimulationWorldEventSnapshot
            {
                EventStableId = source.EventStableId,
                EventRevision = source.EventRevision,
                LastChangedWorldRevision = source.LastChangedWorldRevision,
                EventTypeCode = source.EventTypeCode,
                TriggerCode = source.TriggerCode,
                StateCode = source.StateCode,
                OccurredWorldTick = source.OccurredWorldTick,
                VisibleFromWorldTick = source.VisibleFromWorldTick,
                ExpiresAfterWorldTick = source.ExpiresAfterWorldTick,
                AudienceScopeCode = source.AudienceScopeCode,
                PresentationKey = source.PresentationKey,
                ResponseKindCode = source.ResponseKindCode,
                SourceOpportunityStableId = source.SourceOpportunityStableId,
                ChoiceSetStableId = source.ChoiceSetStableId,
                Choices = source.Choices.Select(value => new SimulationWorldEventChoiceSnapshot
                {
                    ChoiceStableId = value.ChoiceStableId,
                    DisplayOrder = value.DisplayOrder,
                    CardStableId = value.CardStableId,
                    CardRevision = value.CardRevision,
                    OrientationCode = value.OrientationCode,
                    KoreanTitle = value.KoreanTitle,
                    KoreanSummary = value.KoreanSummary,
                }).ToArray(),
                SelectedChoiceStableId = source.SelectedChoiceStableId,
                ActiveBuildingStableId = source.ActiveBuildingStableId,
                AnchorBuildingStableIds = source.AnchorBuildingStableIds.ToArray(),
                TileKeys = source.TileKeys.ToArray(),
                RegionStableIds = source.RegionStableIds.ToArray(),
                ParticipantPlayerStableIds = source.ParticipantPlayerStableIds.ToArray(),
                RespondedParticipantCount = source.RespondedParticipantCount,
                RequiredParticipantCount = source.RequiredParticipantCount,
                CanRespond = source.CanRespond,
                RequiresUnanimousResponse = source.RequiresUnanimousResponse,
                RequiresExpectedRevision = source.RequiresExpectedRevision,
                RuleRevision = source.RuleRevision,
                SourceStableIds = source.SourceStableIds.ToArray(),
                SourceInstanceStableId = source.SourceInstanceStableId,
                NatureRouteCode = source.NatureRouteCode,
                ProjectedThreatPressureDelta = source.ProjectedThreatPressureDelta,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
                PresentationOnly = source.PresentationOnly,
            };
    }
}
