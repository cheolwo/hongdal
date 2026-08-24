using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// 업무 영역 사건과 지역 인과 원장에서 Nature 경로별 위협 압력을 계산한다.
    /// 상태를 변경하지 않는 순수 정책이며, 조우 생성과 전투 결과 처리는 소유하지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
    public static class SimulationNatureThreatPressurePolicy
    {
        public static SimulationNatureThreatRouteSnapshot[] Evaluate(
            IEnumerable<SimulationRegionalIncidentSnapshot> incidents,
            SimulationRegionalCausalityStateSnapshot? causality = null)
        {
            var active = incidents.Where(value => value.RemainingSeverity > 0).ToArray();
            var total = active.Sum(value => value.RemainingSeverity);
            return new[]
            {
                SimulationRegionalIncidentCodes.NatureToFarm,
                SimulationRegionalIncidentCodes.NatureToTown,
                SimulationRegionalIncidentCodes.NatureToCityHub,
            }.Select(route =>
            {
                var routeIncidents = active.Where(value => value.NatureRouteCode == route)
                    .OrderBy(value => value.OccurredWorldTick)
                    .ThenBy(value => value.IncidentStableId, StringComparer.Ordinal).ToArray();
                var root = routeIncidents.Sum(value => value.RemainingSeverity);
                var spillover = total / 3;
                var incidentPressure = Math.Min(12, root * 2 + spillover);
                var relevantChanges = causality?.Changes.Where(value =>
                    string.IsNullOrWhiteSpace(value.NatureRouteCode)
                    || value.NatureRouteCode == route).ToArray()
                    ?? Array.Empty<SimulationRegionalCausalityChangeSnapshot>();
                var hasChangeLedger = causality?.Changes.Length > 0;
                var threatModifier = hasChangeLedger
                    ? Math.Max(0, relevantChanges.Sum(value => value.ThreatDelta))
                    : causality?.ThreatScore ?? 0;
                var recoveryModifier = hasChangeLedger
                    ? Math.Max(0, relevantChanges.Sum(value => value.RecoveryDelta))
                    : causality?.RecoveryScore ?? 0;
                var pressure = Math.Max(0, Math.Min(12,
                    incidentPressure + threatModifier - recoveryModifier));
                return new SimulationNatureThreatRouteSnapshot
                {
                    NatureRouteCode = route,
                    RootRemainingSeverity = root,
                    GlobalSpilloverPressure = spillover,
                    IncidentPressure = incidentPressure,
                    ThreatScoreModifier = threatModifier,
                    RecoveryScoreModifier = recoveryModifier,
                    EffectivePressure = pressure,
                    PressureLevelCode = pressure <= 1
                        ? SimulationNatureThreatCodes.Stable
                        : pressure <= 3
                            ? SimulationNatureThreatCodes.Warning
                            : pressure <= 7
                                ? SimulationNatureThreatCodes.Threatened
                                : SimulationNatureThreatCodes.Infested,
                    SourceIncidentStableIds = active
                        .OrderBy(value => value.OccurredWorldTick)
                        .ThenBy(value => value.IncidentStableId, StringComparer.Ordinal)
                        .Select(value => value.IncidentStableId).ToArray(),
                };
            }).ToArray();
        }

        public static int ThreatUnitCount(int effectivePressure)
            => effectivePressure < 4 ? 0 : Math.Min(5, (effectivePressure - 1) / 2);
    }
}
