using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        /// <summary>
        /// 계산된 경로 압력을 조우와 World Event 상태 사본으로 투영한다.
        /// 업무 영역 사건의 생성·응답과 전투 결과 규칙은 이 모듈 밖에 둔다.
        /// </summary>
        private void RebuildNatureThreat(int currentWorldTick)
        {
            var routes = SimulationNatureThreatPressurePolicy.Evaluate(
                regionalIncidents.Values,
                regionalCausalitySchemaEnabled
                    ? CreateRegionalCausalitySnapshot() : null);
            foreach (var route in routes)
            {
                var encounterId = "nature-encounter:" + SessionStableId + ":"
                    + route.NatureRouteCode + ":pressure";
                var shouldBeActive = route.EffectivePressure >= 4
                    && (route.RootRemainingSeverity > 0
                        || route.ThreatScoreModifier > 0)
                    && !IsNatureRouteSecured(route.NatureRouteCode, currentWorldTick);
                if (!natureThreatEncounters.TryGetValue(encounterId, out var encounter))
                {
                    if (!shouldBeActive)
                    {
                        UpsertNaturePressureWorldEvent(route, null, currentWorldTick);
                        continue;
                    }
                    encounter = new SimulationNatureThreatEncounterSnapshot
                    {
                        EncounterStableId = encounterId,
                        EncounterRevision = 1,
                        NatureRouteCode = route.NatureRouteCode,
                        StateCode = SimulationNatureThreatCodes.Active,
                        OccurredWorldTick = currentWorldTick,
                    };
                    natureThreatEncounters.Add(encounterId, encounter);
                }
                else
                {
                    encounter.EncounterRevision++;
                    encounter.StateCode = shouldBeActive
                        ? SimulationNatureThreatCodes.Active
                        : SimulationRegionalIncidentCodes.Resolved;
                    encounter.ResolvedWorldTick = shouldBeActive ? null : currentWorldTick;
                }
                encounter.RiskBandCode = SimulationNatureThreatCodes.EncounterBand;
                encounter.ThreatUnitCount = SimulationNatureThreatPressurePolicy
                    .ThreatUnitCount(route.EffectivePressure);
                encounter.SourceIncidentStableIds = route.SourceIncidentStableIds.ToArray();
                UpsertNaturePressureWorldEvent(route, encounter, currentWorldTick);
            }
        }

        private SimulationNatureThreatStateSnapshot CreateNatureThreatStateSnapshot()
            => new SimulationNatureThreatStateSnapshot
            {
                Routes = SimulationNatureThreatPressurePolicy.Evaluate(
                    regionalIncidents.Values.Select(CloneRegionalIncident),
                    regionalCausalitySchemaEnabled
                        ? CreateRegionalCausalitySnapshot() : null),
                Encounters = natureThreatEncounters.Values
                    .OrderBy(value => value.NatureRouteCode, StringComparer.Ordinal)
                    .Select(CloneNatureEncounter).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };

        private void UpsertNaturePressureWorldEvent(SimulationNatureThreatRouteSnapshot route,
            SimulationNatureThreatEncounterSnapshot? encounter, int currentWorldTick)
        {
            var eventId = "world-event:nature-pressure:" + route.NatureRouteCode;
            var existing = worldEvents.FirstOrDefault(value => value.EventStableId == eventId);
            var isEncounter = encounter != null
                && encounter.StateCode == SimulationNatureThreatCodes.Active;
            var visible = route.EffectivePressure >= 2;
            if (existing == null)
            {
                if (!visible) return;
                worldEvents.Add(new SimulationWorldEventSnapshot
                {
                    EventStableId = eventId,
                    EventRevision = 1,
                    LastChangedWorldRevision = Revision,
                    EventTypeCode = isEncounter
                        ? SimulationWorldEventCodes.NatureThreatEncounter
                        : SimulationWorldEventCodes.NatureThreatWarning,
                    TriggerCode = route.PressureLevelCode,
                    StateCode = isEncounter ? SimulationNatureThreatCodes.Active
                        : SimulationWorldEventCodes.Warning,
                    OccurredWorldTick = currentWorldTick,
                    VisibleFromWorldTick = currentWorldTick,
                    AudienceScopeCode = SimulationWorldEventCodes.SessionParticipants,
                    PresentationKey = encounter?.PresentationKey
                        ?? "survival.nature-pressure.warning",
                    SourceOpportunityStableId = encounter?.EncounterStableId ?? string.Empty,
                    NatureRouteCode = route.NatureRouteCode,
                    ProjectedThreatPressureDelta = route.EffectivePressure,
                    SourceStableIds = route.SourceIncidentStableIds.ToArray(),
                    RuleRevision = RuleRevision,
                    SimulationOnly = true,
                    IsOperationalState = false,
                    PresentationOnly = true,
                });
                return;
            }
            existing.EventRevision++;
            existing.LastChangedWorldRevision = Revision;
            existing.EventTypeCode = isEncounter
                ? SimulationWorldEventCodes.NatureThreatEncounter
                : SimulationWorldEventCodes.NatureThreatWarning;
            existing.TriggerCode = route.PressureLevelCode;
            existing.StateCode = visible
                ? isEncounter ? SimulationNatureThreatCodes.Active : SimulationWorldEventCodes.Warning
                : SimulationWorldEventCodes.Resolved;
            existing.PresentationKey = encounter?.PresentationKey
                ?? "survival.nature-pressure.warning";
            existing.SourceOpportunityStableId = encounter?.EncounterStableId ?? string.Empty;
            existing.ProjectedThreatPressureDelta = route.EffectivePressure;
            existing.SourceStableIds = route.SourceIncidentStableIds.ToArray();
        }
    }
}
