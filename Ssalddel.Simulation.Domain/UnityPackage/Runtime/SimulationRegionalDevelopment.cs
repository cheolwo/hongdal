using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, SimulationRegionalDevelopmentOpportunitySnapshot>
            regionalDevelopmentOpportunities =
                new Dictionary<string, SimulationRegionalDevelopmentOpportunitySnapshot>(
                    StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationRegionalRouteSafetySnapshot>
            regionalRouteSafeties =
                new Dictionary<string, SimulationRegionalRouteSafetySnapshot>(
                    StringComparer.Ordinal);
        private readonly HashSet<string> operationalFarmDevelopmentH1StableIds =
            new HashSet<string>(StringComparer.Ordinal);
        private long regionalDevelopmentRevision = 0;
        private bool regionalDevelopmentSchemaEnabled = true;

        /// <summary>
        /// 지역 발전의 첫 계약 골격이다. 아직 전투·배치 상태 전이는 연결하지 않는다.
        /// </summary>
        private SimulationRegionalDevelopmentStateSnapshot CreateRegionalDevelopmentStateSnapshot()
            => new SimulationRegionalDevelopmentStateSnapshot
            {
                Revision = regionalDevelopmentRevision,
                RuleRevision = SimulationRegionalDevelopmentCodes.RuleRevision,
                Opportunities = regionalDevelopmentOpportunities.Values
                    .OrderBy(value => value.EarnedWorldTick)
                    .ThenBy(value => value.OpportunityStableId, StringComparer.Ordinal)
                    .Select(CloneRegionalDevelopmentOpportunity).ToArray(),
                RouteSafeties = regionalRouteSafeties.Values
                    .OrderBy(value => value.NatureRouteCode, StringComparer.Ordinal)
                    .Select(CloneRegionalRouteSafety).ToArray(),
                Areas = new[]
                {
                    new SimulationRegionalDevelopmentAreaSnapshot
                    {
                        AreaCode = SimulationRegionalIncidentCodes.Farm,
                        TargetH2StableId = SimulationRegionalDevelopmentCodes
                            .FarmIncidentContainmentH2,
                        StateCode = operationalFarmDevelopmentH1StableIds.Count == 0
                            ? SimulationRegionalDevelopmentCodes.NotStarted
                            : operationalFarmDevelopmentH1StableIds.Count == 3
                                ? SimulationRegionalDevelopmentCodes.IndependentReady
                                : SimulationRegionalDevelopmentCodes.Developing,
                        RequiredH1StableIds = new[]
                        {
                            SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1,
                            SimulationRegionalDevelopmentCodes.FarmIncidentQuarantineH1,
                            SimulationRegionalDevelopmentCodes.FarmWeatherProtectionH1,
                        },
                        OperationalH1StableIds = operationalFarmDevelopmentH1StableIds
                            .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                    },
                },
                Connectors = new[]
                {
                    new SimulationRegionalDevelopmentConnectorSnapshot
                    {
                        ConnectorStableId = SimulationRegionalDevelopmentCodes
                            .NatureFarmSafetyConnector,
                        FromAreaCode = SimulationRegionalIncidentCodes.NatureHome,
                        ToAreaCode = SimulationRegionalIncidentCodes.Farm,
                        StateCode = operationalFarmDevelopmentH1StableIds.Count == 3
                            ? SimulationRegionalDevelopmentCodes.Available
                            : SimulationRegionalDevelopmentCodes.Locked,
                        RequiredAreaCode = SimulationRegionalIncidentCodes.Farm,
                    },
                },
                SimulationOnly = true,
                IsOperationalState = false,
            };

        internal void UseLegacyRegionalDevelopmentRules()
            => regionalDevelopmentSchemaEnabled = false;

        private bool HasRegionalDevelopmentState()
            => regionalDevelopmentOpportunities.Count > 0
                || regionalRouteSafeties.Count > 0
                || operationalFarmDevelopmentH1StableIds.Count > 0;

        private bool IsNatureRouteSecured(string natureRouteCode, int worldTick)
            => regionalRouteSafeties.TryGetValue(natureRouteCode, out var safety)
                && worldTick >= safety.SecuredFromWorldTick
                && worldTick < safety.SecuredUntilWorldTick;

        private void RecordNatureEncounterVictoryForRegionalDevelopment(
            string battleStableId,
            SimulationNatureThreatEncounterSnapshot encounter)
        {
            regionalDevelopmentRevision++;
            regionalRouteSafeties[encounter.NatureRouteCode] =
                new SimulationRegionalRouteSafetySnapshot
                {
                    NatureRouteCode = encounter.NatureRouteCode,
                    SecuredFromWorldTick = CurrentTick,
                    SecuredUntilWorldTick = CurrentTick + 1,
                    SourceEncounterStableId = encounter.EncounterStableId,
                    SourceBattleStableId = battleStableId.Trim(),
                };

            if (!string.Equals(encounter.NatureRouteCode,
                    SimulationRegionalIncidentCodes.NatureToFarm,
                    StringComparison.Ordinal))
                return;

            var incident = regionalIncidents.Values
                .Where(value => value.NatureRouteCode == encounter.NatureRouteCode
                    && value.RemainingSeverity > 0
                    && !regionalDevelopmentOpportunities.Values.Any(opportunity =>
                        opportunity.SourceIncidentStableId == value.IncidentStableId))
                .OrderBy(value => value.OccurredWorldTick)
                .ThenBy(value => value.IncidentStableId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (incident == null) return;

            var opportunityId = "regional-development-opportunity:"
                + incident.IncidentStableId;
            regionalDevelopmentOpportunities.Add(opportunityId,
                new SimulationRegionalDevelopmentOpportunitySnapshot
                {
                    OpportunityStableId = opportunityId,
                    OpportunityRevision = 1,
                    SourceIncidentStableId = incident.IncidentStableId,
                    SourceEncounterStableId = encounter.EncounterStableId,
                    SourceBattleStableId = battleStableId.Trim(),
                    NatureRouteCode = encounter.NatureRouteCode,
                    TargetAreaCode = SimulationRegionalIncidentCodes.Farm,
                    StateCode = SimulationRegionalDevelopmentCodes.Available,
                    EarnedWorldTick = CurrentTick,
                });
        }

        private static bool IsFarmDevelopmentBlueprint(string blueprintStableId)
            => blueprintStableId == SimulationRegionalDevelopmentCodes.FarmExposureInspectionH1
                || blueprintStableId == SimulationRegionalDevelopmentCodes.FarmIncidentQuarantineH1
                || blueprintStableId == SimulationRegionalDevelopmentCodes.FarmWeatherProtectionH1;

        private void AppendRegionalDevelopmentPlacementBlocks(
            SimulationConstructionOrderPayload payload,
            ICollection<string> blocks)
        {
            var isDevelopment = IsFarmDevelopmentBlueprint(payload.BlueprintStableId);
            if (!isDevelopment)
            {
                if (!string.IsNullOrWhiteSpace(payload.DevelopmentOpportunityStableId))
                    blocks.Add("SimulationRegionalDevelopmentBlueprintMismatch");
                return;
            }
            if (!string.Equals(payload.TargetH2StableId,
                    SimulationRegionalDevelopmentCodes.FarmIncidentContainmentH2,
                    StringComparison.Ordinal))
                blocks.Add("SimulationRegionalDevelopmentTargetH2Mismatch");
            if (string.IsNullOrWhiteSpace(payload.DevelopmentOpportunityStableId))
            {
                blocks.Add("SimulationRegionalDevelopmentOpportunityRequired");
                return;
            }
            if (!regionalDevelopmentOpportunities.TryGetValue(
                    payload.DevelopmentOpportunityStableId, out var opportunity))
            {
                blocks.Add("SimulationRegionalDevelopmentOpportunityNotFound");
                return;
            }
            if (opportunity.StateCode != SimulationRegionalDevelopmentCodes.Available)
                blocks.Add("SimulationRegionalDevelopmentOpportunityUnavailable");
            if (opportunity.TargetAreaCode != SimulationRegionalIncidentCodes.Farm)
                blocks.Add("SimulationRegionalDevelopmentOpportunityAreaMismatch");
            if (operationalFarmDevelopmentH1StableIds.Contains(payload.BlueprintStableId))
                blocks.Add("SimulationRegionalDevelopmentH1AlreadyOperational");
        }

        private void ReserveRegionalDevelopmentOpportunityForConstruction(
            SimulationConstructionOrderPayload payload,
            string constructionProjectStableId)
        {
            if (!IsFarmDevelopmentBlueprint(payload.BlueprintStableId)) return;
            var opportunity = regionalDevelopmentOpportunities[
                payload.DevelopmentOpportunityStableId];
            opportunity.OpportunityRevision++;
            opportunity.StateCode = SimulationRegionalDevelopmentCodes.Reserved;
            opportunity.ReservedWorldTick = CurrentTick;
            opportunity.ReservedProjectStableId = constructionProjectStableId;
            regionalDevelopmentRevision++;
        }

        private void CompleteRegionalDevelopmentConstruction(
            SimulationConstructionProjectSnapshot project)
        {
            if (string.IsNullOrWhiteSpace(project.DevelopmentOpportunityStableId)) return;
            if (!regionalDevelopmentOpportunities.TryGetValue(
                    project.DevelopmentOpportunityStableId, out var opportunity)
                || opportunity.StateCode != SimulationRegionalDevelopmentCodes.Reserved
                || opportunity.ReservedProjectStableId != project.ConstructionProjectStableId)
                throw new SimulationConflictException(
                    "SimulationRegionalDevelopmentReservationInvalid");
            opportunity.OpportunityRevision++;
            opportunity.StateCode = SimulationRegionalDevelopmentCodes.Consumed;
            opportunity.ConsumedWorldTick = CurrentTick;
            opportunity.OperationalFacilityStableId = project.TargetFacilityStableId;
            operationalFarmDevelopmentH1StableIds.Add(project.BlueprintStableId);
            regionalDevelopmentRevision++;
        }

        private static SimulationRegionalDevelopmentOpportunitySnapshot
            CloneRegionalDevelopmentOpportunity(
                SimulationRegionalDevelopmentOpportunitySnapshot value)
            => new SimulationRegionalDevelopmentOpportunitySnapshot
            {
                OpportunityStableId = value.OpportunityStableId,
                OpportunityRevision = value.OpportunityRevision,
                SourceIncidentStableId = value.SourceIncidentStableId,
                SourceEncounterStableId = value.SourceEncounterStableId,
                SourceBattleStableId = value.SourceBattleStableId,
                NatureRouteCode = value.NatureRouteCode,
                TargetAreaCode = value.TargetAreaCode,
                StateCode = value.StateCode,
                EarnedWorldTick = value.EarnedWorldTick,
                ReservedWorldTick = value.ReservedWorldTick,
                ConsumedWorldTick = value.ConsumedWorldTick,
                ReservedProjectStableId = value.ReservedProjectStableId,
                OperationalFacilityStableId = value.OperationalFacilityStableId,
            };

        private static SimulationRegionalRouteSafetySnapshot CloneRegionalRouteSafety(
            SimulationRegionalRouteSafetySnapshot value)
            => new SimulationRegionalRouteSafetySnapshot
            {
                NatureRouteCode = value.NatureRouteCode,
                SecuredFromWorldTick = value.SecuredFromWorldTick,
                SecuredUntilWorldTick = value.SecuredUntilWorldTick,
                SourceEncounterStableId = value.SourceEncounterStableId,
                SourceBattleStableId = value.SourceBattleStableId,
            };

        internal static SimulationRegionalDevelopmentStateSnapshot
            CloneRegionalDevelopmentState(SimulationRegionalDevelopmentStateSnapshot source)
            => new SimulationRegionalDevelopmentStateSnapshot
            {
                Revision = source.Revision,
                RuleRevision = source.RuleRevision,
                Opportunities = source.Opportunities
                    .Select(CloneRegionalDevelopmentOpportunity).ToArray(),
                RouteSafeties = source.RouteSafeties
                    .Select(CloneRegionalRouteSafety).ToArray(),
                Areas = source.Areas.Select(value =>
                    new SimulationRegionalDevelopmentAreaSnapshot
                    {
                        AreaCode = value.AreaCode,
                        TargetH2StableId = value.TargetH2StableId,
                        StateCode = value.StateCode,
                        RequiredH1StableIds = value.RequiredH1StableIds.ToArray(),
                        OperationalH1StableIds = value.OperationalH1StableIds.ToArray(),
                    }).ToArray(),
                Connectors = source.Connectors.Select(value =>
                    new SimulationRegionalDevelopmentConnectorSnapshot
                    {
                        ConnectorStableId = value.ConnectorStableId,
                        FromAreaCode = value.FromAreaCode,
                        ToAreaCode = value.ToAreaCode,
                        StateCode = value.StateCode,
                        RequiredAreaCode = value.RequiredAreaCode,
                    }).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };
    }
}
