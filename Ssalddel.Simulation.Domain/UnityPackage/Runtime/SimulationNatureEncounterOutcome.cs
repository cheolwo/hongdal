using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        /// <summary>
        /// Nature 조우의 권위 있는 전투 결과를 Session에 반영한다.
        /// 현재 규칙의 사건 심각도 변경은 호환을 위해 유지하며, 발전 기회 발급은 별도 모듈에서 맡는다.
        /// </summary>
        public 경영SimulationSessionSnapshot ApplyNatureEncounterVictory(
            string battleStableId, string encounterStableId)
        {
            lock (gate)
            {
                var outcomeId = battleStableId.Trim() + "~" + encounterStableId.Trim();
                if (appliedNatureBattleOutcomeIds.Contains(outcomeId)) return CreateSnapshot();
                if (!natureThreatEncounters.TryGetValue(encounterStableId.Trim(), out var encounter))
                    throw new SimulationNotFoundException("SimulationNatureThreatEncounterNotFound");
                if (encounter.StateCode == SimulationRegionalIncidentCodes.Resolved)
                {
                    appliedNatureBattleOutcomeIds.Add(outcomeId);
                    return CreateSnapshot();
                }

                if (regionalDevelopmentSchemaEnabled)
                {
                    Revision++;
                    RecordNatureEncounterVictoryForRegionalDevelopment(
                        battleStableId, encounter);
                    RebuildNatureThreat(CurrentTick);
                    AppendNatureEncounterVictoryCommand(battleStableId, encounterStableId);
                    appliedNatureBattleOutcomeIds.Add(outcomeId);
                    return CreateSnapshot();
                }

                var incident = regionalIncidents.Values
                    .Where(value => value.NatureRouteCode == encounter.NatureRouteCode
                        && value.RemainingSeverity > 0)
                    .OrderBy(value => value.OccurredWorldTick)
                    .ThenBy(value => value.IncidentStableId, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (incident != null)
                {
                    Revision++;
                    incident.RemainingSeverity = Math.Max(0, incident.RemainingSeverity - 1);
                    incident.IncidentRevision++;
                    if (incident.RemainingSeverity == 0)
                    {
                        incident.StateCode = SimulationRegionalIncidentCodes.Resolved;
                        incident.OutcomeCode = SimulationRegionalIncidentCodes.Corrected;
                        ObserveSafeRegionalIncidentOutcome(incident, CurrentTick);
                        UpdateRegionalIncidentWorldEvent(incident);
                    }
                    RebuildNatureThreat(CurrentTick);
                    AppendNatureEncounterVictoryCommand(battleStableId, encounterStableId);
                }
                appliedNatureBattleOutcomeIds.Add(outcomeId);
                return CreateSnapshot();
            }
        }
    }
}
