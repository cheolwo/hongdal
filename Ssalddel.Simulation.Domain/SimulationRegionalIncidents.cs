using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
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
                        ? SimulationRegionalIncidentCodes.Stable
                        : pressure <= 3
                            ? SimulationRegionalIncidentCodes.Warning
                            : pressure <= 7
                                ? SimulationRegionalIncidentCodes.Threatened
                                : SimulationRegionalIncidentCodes.Infested,
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

    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, SimulationRegionalIncidentSnapshot>
            regionalIncidents = new Dictionary<string, SimulationRegionalIncidentSnapshot>(
                StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedRegionalIncidentResponseCommand>
            appliedRegionalIncidentResponseCommands =
                new Dictionary<string, AppliedRegionalIncidentResponseCommand>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationNatureThreatEncounterSnapshot>
            natureThreatEncounters =
                new Dictionary<string, SimulationNatureThreatEncounterSnapshot>(StringComparer.Ordinal);
        private readonly HashSet<string> appliedNatureBattleOutcomeIds =
            new HashSet<string>(StringComparer.Ordinal);

        public SimulationRegionalIncidentResponsePreviewSnapshot
            PreviewRegionalIncidentResponse(string eventStableId,
                SimulationRegionalIncidentResponsePreviewRequest request)
        {
            ValidateRegionalIncidentPreviewRequest(eventStableId, request);
            lock (gate)
            {
                var blocks = new List<string>();
                if (request.ExpectedRevision != Revision)
                    blocks.Add("SimulationExpectedRevisionMismatch");
                var incident = FindRegionalIncidentByEvent(eventStableId, blocks);
                IncidentChoiceRule? choice = null;
                if (incident != null)
                {
                    if (incident.StateCode != SimulationRegionalIncidentCodes.AwaitingResponse)
                        blocks.Add("SimulationRegionalIncidentResponseClosed");
                    choice = ChoiceRules(incident).FirstOrDefault(value =>
                        value.ChoiceStableId == request.ChoiceStableId.Trim());
                    if (choice == null)
                        blocks.Add("SimulationRegionalIncidentChoiceInvalid");
                    if (CurrentTick > incident.DeadlineWorldTick)
                        blocks.Add("SimulationRegionalIncidentDeadlinePassed");
                }

                return new SimulationRegionalIncidentResponsePreviewSnapshot
                {
                    SessionStableId = SessionStableId,
                    EventStableId = eventStableId.Trim(),
                    IncidentStableId = incident?.IncidentStableId ?? string.Empty,
                    ChoiceStableId = request.ChoiceStableId.Trim(),
                    DeadlineWorldTick = incident?.DeadlineWorldTick ?? 0,
                    ProjectedThreatSeverityDelta = incident == null || choice == null
                        ? 0 : choice.Unsafe ? incident.Severity : 0,
                    RequiredWorldInteractionIds = incident?.RequiredWorldInteractionIds.ToArray()
                        ?? Array.Empty<string>(),
                    RequiredActionCodes = incident?.RequiredActionCodes.ToArray()
                        ?? Array.Empty<string>(),
                    CanConfirm = blocks.Count == 0,
                    BlockingReasonCodes = blocks.ToArray(),
                    SimulationOnly = true,
                    IsOperationalState = false,
                };
            }
        }

        public 경영SimulationSessionSnapshot ConfirmRegionalIncidentResponse(
            string eventStableId,
            SimulationRegionalIncidentResponseConfirmRequest request)
        {
            ValidateRegionalIncidentConfirmRequest(eventStableId, request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payload = string.Join("~", eventStableId.Trim(),
                    request.ActorStableId.Trim(), request.ChoiceStableId.Trim());
                if (appliedRegionalIncidentResponseCommands.TryGetValue(commandId,
                    out var applied))
                {
                    if (applied.Payload != payload)
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                if (HasAppliedDecisionCommand(commandId)
                    || HasAppliedNpcPolicyCommand(commandId)
                    || HasAppliedFarmSurvivalCommand(commandId)
                    || HasAppliedCollectibleCardCommand(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");

                var preview = PreviewRegionalIncidentResponse(eventStableId,
                    new SimulationRegionalIncidentResponsePreviewRequest
                    {
                        ExpectedRevision = request.ExpectedRevision,
                        ActorStableId = request.ActorStableId,
                        ChoiceStableId = request.ChoiceStableId,
                    });
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(preview.BlockingReasonCodes[0]);

                var incident = regionalIncidents[preview.IncidentStableId];
                var choice = ChoiceRules(incident).Single(value =>
                    value.ChoiceStableId == request.ChoiceStableId.Trim());
                Revision++;
                incident.IncidentRevision++;
                incident.SelectedChoiceStableId = choice.ChoiceStableId;
                if (choice.Unsafe)
                {
                    incident.StateCode = SimulationRegionalIncidentCodes.AdverseOutcome;
                    incident.OutcomeCode = SimulationRegionalIncidentCodes.UnsafeResponse;
                    incident.RemainingSeverity = incident.Severity;
                    ObserveUnsafeRegionalIncidentOutcome(incident,
                        SimulationRegionalIncidentCodes.UnsafeIncidentResponse,
                        CurrentTick);
                }
                else
                {
                    incident.StateCode = SimulationRegionalIncidentCodes.RecoveryInProgress;
                    incident.OutcomeCode = string.Empty;
                    ResolveIncidentWhenRequirementsCompleted(incident, CurrentTick);
                }
                UpdateRegionalIncidentWorldEvent(incident);
                RebuildNatureThreat(CurrentTick);
                AppendRegionalIncidentResponseConfirmCommand(eventStableId, request);
                var snapshot = CreateSnapshot();
                appliedRegionalIncidentResponseCommands.Add(commandId,
                    new AppliedRegionalIncidentResponseCommand(payload, Clone(snapshot)));
                return snapshot;
            }
        }

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

        internal void RegisterFarmHarvestExposureIncident(string harvestLotStableId,
            string facilityStableId, int occurredWorldTick)
            => RegisterRegionalIncident(SimulationRegionalIncidentCodes.FarmHarvestExposure,
                SimulationRegionalIncidentCodes.Farm,
                SimulationRegionalIncidentCodes.NatureToFarm,
                harvestLotStableId, facilityStableId, occurredWorldTick, 2,
                new[] { "WI-FARM-05", "WI-FARM-06" },
                new[]
                {
                    SimulationFarmSurvivalCodes.HarvestCollection,
                    SimulationFarmSurvivalCodes.OutboundPacking,
                });

        internal void RegisterTownMarketContaminationIncident(string inventoryStableId,
            string facilityStableId, int occurredWorldTick)
            => RegisterRegionalIncident(SimulationRegionalIncidentCodes.TownMarketContamination,
                SimulationRegionalIncidentCodes.Town,
                SimulationRegionalIncidentCodes.NatureToTown,
                inventoryStableId, facilityStableId, occurredWorldTick, 2,
                new[] { "WI-MARKET-03", "WI-MARKET-04", "WI-MARKET-05" },
                new[]
                {
                    SimulationSupplyChainActionCodes.MarketInspection,
                    SimulationSupplyChainActionCodes.MarketBackroomPutAway,
                    SimulationSupplyChainActionCodes.MarketDisplayReplenishment,
                });

        internal void RegisterHubCargoBacklogIncident(string cargoStableId,
            string facilityStableId, int occurredWorldTick)
            => RegisterRegionalIncident(SimulationRegionalIncidentCodes.CityHubCargoBacklog,
                SimulationRegionalIncidentCodes.CityHub,
                SimulationRegionalIncidentCodes.NatureToCityHub,
                cargoStableId, facilityStableId, occurredWorldTick, 3,
                new[] { "WI-001", "WI-002" },
                new[]
                {
                    SimulationNpcActionCodes.WarehouseInboundInspection,
                    SimulationNpcActionCodes.WarehouseStorageMove,
                });

        internal void ObserveRegionalIncidentAction(string actionCode,
            string sourceTargetStableId, int completedWorldTick)
        {
            foreach (var incident in regionalIncidents.Values.Where(value =>
                value.SourceTargetStableId == sourceTargetStableId
                && value.RequiredActionCodes.Contains(actionCode, StringComparer.Ordinal)
                && value.StateCode != SimulationRegionalIncidentCodes.Resolved))
            {
                incident.CompletedActionCodes = incident.CompletedActionCodes
                    .Append(actionCode).Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                incident.IncidentRevision++;
                ResolveIncidentWhenRequirementsCompleted(incident, completedWorldTick);
                UpdateRegionalIncidentWorldEvent(incident);
            }
            RebuildNatureThreat(completedWorldTick);
        }

        internal void ObserveRegionalIncidentTaskCompletion(SimulationTaskSnapshot task,
            int completedWorldTick)
        {
            var targets = task.InputLotStableIds.Concat(task.SourceStableIds).ToHashSet(
                StringComparer.Ordinal);
            if (!string.IsNullOrWhiteSpace(task.CausedByDecisionStableId)
                && decisions.TryGetValue(task.CausedByDecisionStableId, out var decision))
                targets.UnionWith(decision.TargetStableIds);
            foreach (var incident in regionalIncidents.Values.Where(value =>
                targets.Contains(value.SourceTargetStableId)
                && value.RequiredActionCodes.Contains(task.ActionCode, StringComparer.Ordinal)))
                ObserveRegionalIncidentAction(task.ActionCode,
                    incident.SourceTargetStableId, completedWorldTick);
        }

        private void AdvanceRegionalIncidents(int currentWorldTick)
        {
            foreach (var incident in regionalIncidents.Values.Where(value =>
                value.StateCode != SimulationRegionalIncidentCodes.Resolved
                && value.StateCode != SimulationRegionalIncidentCodes.AdverseOutcome
                && currentWorldTick > value.DeadlineWorldTick))
            {
                incident.StateCode = SimulationRegionalIncidentCodes.AdverseOutcome;
                incident.OutcomeCode = SimulationRegionalIncidentCodes.DeadlineMissed;
                incident.RemainingSeverity = incident.Severity;
                incident.IncidentRevision++;
                ObserveUnsafeRegionalIncidentOutcome(incident,
                    SimulationRegionalIncidentCodes.IncidentDeadlineMissed,
                    currentWorldTick);
                UpdateRegionalIncidentWorldEvent(incident);
            }
            RebuildNatureThreat(currentWorldTick);
        }

        private void RegisterRegionalIncident(string typeCode, string sourceInstanceStableId,
            string routeCode, string sourceTargetStableId, string facilityStableId,
            int occurredWorldTick, int severity, string[] wiIds, string[] actionCodes)
        {
            var incidentId = "regional-incident:" + typeCode + ":" + sourceTargetStableId;
            if (regionalIncidents.ContainsKey(incidentId)) return;
            var eventId = "world-event:" + incidentId;
            var incident = new SimulationRegionalIncidentSnapshot
            {
                IncidentStableId = incidentId,
                EventStableId = eventId,
                IncidentRevision = 1,
                SourceInstanceStableId = sourceInstanceStableId,
                NatureRouteCode = routeCode,
                IncidentTypeCode = typeCode,
                StateCode = SimulationRegionalIncidentCodes.AwaitingResponse,
                Severity = severity,
                RemainingSeverity = 0,
                OccurredWorldTick = occurredWorldTick,
                DeadlineWorldTick = occurredWorldTick + 2,
                SourceTargetStableId = sourceTargetStableId,
                FacilityStableId = facilityStableId,
                RequiredWorldInteractionIds = wiIds.ToArray(),
                RequiredActionCodes = actionCodes.ToArray(),
                SourceStableIds = new[] { sourceTargetStableId, facilityStableId, typeCode },
                SimulationOnly = true,
                IsOperationalState = false,
            };
            regionalIncidents.Add(incidentId, incident);
            RegisterRegionalIncidentWorldEvent(incident);
        }

        private void ResolveIncidentWhenRequirementsCompleted(
            SimulationRegionalIncidentSnapshot incident, int completedWorldTick)
        {
            if (incident.StateCode == SimulationRegionalIncidentCodes.AwaitingResponse)
                return;
            if (!incident.RequiredActionCodes.All(value =>
                incident.CompletedActionCodes.Contains(value, StringComparer.Ordinal))) return;
            incident.StateCode = SimulationRegionalIncidentCodes.Resolved;
            incident.OutcomeCode = incident.RemainingSeverity > 0
                || completedWorldTick > incident.DeadlineWorldTick
                    ? SimulationRegionalIncidentCodes.Corrected
                    : SimulationRegionalIncidentCodes.Contained;
            incident.RemainingSeverity = 0;
            ObserveSafeRegionalIncidentOutcome(incident, completedWorldTick);
        }

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
                        || route.ThreatScoreModifier > 0);
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
                        StateCode = SimulationRegionalIncidentCodes.Active,
                        OccurredWorldTick = currentWorldTick,
                    };
                    natureThreatEncounters.Add(encounterId, encounter);
                }
                else
                {
                    encounter.EncounterRevision++;
                    encounter.StateCode = shouldBeActive
                        ? SimulationRegionalIncidentCodes.Active
                        : SimulationRegionalIncidentCodes.Resolved;
                    encounter.ResolvedWorldTick = shouldBeActive ? null : currentWorldTick;
                }
                encounter.RiskBandCode = SimulationRegionalIncidentCodes.EncounterBand;
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

        private SimulationRegionalIncidentSnapshot[] CreateRegionalIncidentSnapshots()
            => regionalIncidents.Values.OrderBy(value => value.OccurredWorldTick)
                .ThenBy(value => value.IncidentStableId, StringComparer.Ordinal)
                .Select(CloneRegionalIncident).ToArray();

        private SimulationRegionalIncidentSnapshot? FindRegionalIncidentByEvent(
            string eventStableId, ICollection<string> blocks)
        {
            var incident = regionalIncidents.Values.FirstOrDefault(value =>
                value.EventStableId == eventStableId.Trim());
            if (incident == null) blocks.Add("SimulationRegionalIncidentNotFound");
            return incident;
        }

        private static IncidentChoiceRule[] ChoiceRules(
            SimulationRegionalIncidentSnapshot incident)
        {
            if (incident.IncidentTypeCode == SimulationRegionalIncidentCodes.FarmHarvestExposure)
                return new[]
                {
                    new IncidentChoiceRule(SimulationRegionalIncidentCodes.FarmCollectAndPack, false,
                        "수확물 집하·포장", "집하와 포장 WI를 완료해 노출 원인을 없앱니다."),
                    new IncidentChoiceRule(SimulationRegionalIncidentCodes.FarmLeaveExposed, true,
                        "노출 상태로 방치", "빠르지만 자연권 위협을 높입니다."),
                };
            if (incident.IncidentTypeCode == SimulationRegionalIncidentCodes.TownMarketContamination)
                return new[]
                {
                    new IncidentChoiceRule(SimulationRegionalIncidentCodes.TownQuarantineAndRestock, false,
                        "격리·재입고", "검수·후방 적재·진열 보충 WI로 원인을 해결합니다."),
                    new IncidentChoiceRule(SimulationRegionalIncidentCodes.TownDiscardOutside, true,
                        "타운 외곽 폐기", "타운 문제를 자연권으로 떠넘겨 위협을 높입니다."),
                };
            return new[]
            {
                new IncidentChoiceRule(SimulationRegionalIncidentCodes.HubInspectAndPutAway, false,
                    "검수·창고 적재", "입고 검수와 적재 WI로 적체를 해소합니다."),
                new IncidentChoiceRule(SimulationRegionalIncidentCodes.HubOverflowOpenYard, true,
                    "야외 적치", "적체를 외부에 노출해 자연권 위협을 높입니다."),
            };
        }

        private void RegisterRegionalIncidentWorldEvent(
            SimulationRegionalIncidentSnapshot incident)
        {
            var choices = ChoiceRules(incident);
            worldEvents.Add(new SimulationWorldEventSnapshot
            {
                EventStableId = incident.EventStableId,
                EventRevision = 1,
                LastChangedWorldRevision = Revision,
                EventTypeCode = SimulationWorldEventCodes.RegionalIncident,
                TriggerCode = incident.IncidentTypeCode,
                StateCode = SimulationWorldEventCodes.AwaitingResponse,
                OccurredWorldTick = incident.OccurredWorldTick,
                VisibleFromWorldTick = incident.OccurredWorldTick,
                ExpiresAfterWorldTick = incident.DeadlineWorldTick,
                AudienceScopeCode = SimulationWorldEventCodes.SessionParticipants,
                PresentationKey = "regional-incident." + incident.IncidentTypeCode,
                ResponseKindCode = SimulationWorldEventCodes.RegionalIncident,
                SourceOpportunityStableId = incident.IncidentStableId,
                ChoiceSetStableId = "choice-set:" + incident.IncidentStableId,
                Choices = choices.Select((value, index) =>
                    new SimulationWorldEventChoiceSnapshot
                    {
                        ChoiceStableId = value.ChoiceStableId,
                        DisplayOrder = index + 1,
                        KoreanTitle = value.KoreanTitle,
                        KoreanSummary = value.KoreanSummary,
                    }).ToArray(),
                RequiredParticipantCount = 1,
                CanRespond = true,
                RequiresExpectedRevision = true,
                RuleRevision = RuleRevision,
                SourceStableIds = incident.SourceStableIds.ToArray(),
                SourceInstanceStableId = incident.SourceInstanceStableId,
                NatureRouteCode = incident.NatureRouteCode,
                SimulationOnly = true,
                IsOperationalState = false,
                PresentationOnly = true,
            });
        }

        private void UpdateRegionalIncidentWorldEvent(
            SimulationRegionalIncidentSnapshot incident)
        {
            var value = worldEvents.Single(item => item.EventStableId == incident.EventStableId);
            value.EventRevision++;
            value.LastChangedWorldRevision = Revision;
            value.StateCode = incident.StateCode == SimulationRegionalIncidentCodes.Resolved
                ? SimulationWorldEventCodes.Resolved
                : incident.StateCode == SimulationRegionalIncidentCodes.AdverseOutcome
                    ? SimulationWorldEventCodes.Warning
                    : SimulationWorldEventCodes.AwaitingResponse;
            value.SelectedChoiceStableId = incident.SelectedChoiceStableId;
            value.CanRespond = incident.StateCode == SimulationRegionalIncidentCodes.AwaitingResponse;
            value.RespondedParticipantCount = string.IsNullOrWhiteSpace(
                incident.SelectedChoiceStableId) ? 0 : 1;
            value.ProjectedThreatPressureDelta = incident.RemainingSeverity;
        }

        private void UpsertNaturePressureWorldEvent(SimulationNatureThreatRouteSnapshot route,
            SimulationNatureThreatEncounterSnapshot? encounter, int currentWorldTick)
        {
            var eventId = "world-event:nature-pressure:" + route.NatureRouteCode;
            var existing = worldEvents.FirstOrDefault(value => value.EventStableId == eventId);
            var isEncounter = encounter != null
                && encounter.StateCode == SimulationRegionalIncidentCodes.Active;
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
                    StateCode = isEncounter ? SimulationRegionalIncidentCodes.Active
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
                ? isEncounter ? SimulationRegionalIncidentCodes.Active : SimulationWorldEventCodes.Warning
                : SimulationWorldEventCodes.Resolved;
            existing.PresentationKey = encounter?.PresentationKey
                ?? "survival.nature-pressure.warning";
            existing.SourceOpportunityStableId = encounter?.EncounterStableId ?? string.Empty;
            existing.ProjectedThreatPressureDelta = route.EffectivePressure;
            existing.SourceStableIds = route.SourceIncidentStableIds.ToArray();
        }

        private static void ValidateRegionalIncidentPreviewRequest(string eventStableId,
            SimulationRegionalIncidentResponsePreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireRegionalText(eventStableId, "SimulationWorldEventStableIdInvalid");
            RequireRegionalText(request.ActorStableId, "SimulationActorStableIdInvalid");
            RequireRegionalText(request.ChoiceStableId, "SimulationRegionalIncidentChoiceInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
        }

        internal static void ValidateRegionalIncidentConfirmRequest(string eventStableId,
            SimulationRegionalIncidentResponseConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireRegionalText(request.CommandId, "SimulationCommandIdInvalid");
            ValidateRegionalIncidentPreviewRequest(eventStableId,
                new SimulationRegionalIncidentResponsePreviewRequest
                {
                    ExpectedRevision = request.ExpectedRevision,
                    ActorStableId = request.ActorStableId,
                    ChoiceStableId = request.ChoiceStableId,
                });
        }

        private static void RequireRegionalText(string value, string code)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new SimulationContractException(code);
        }

        internal static SimulationRegionalIncidentSnapshot CloneRegionalIncident(
            SimulationRegionalIncidentSnapshot source)
            => new SimulationRegionalIncidentSnapshot
            {
                IncidentStableId = source.IncidentStableId,
                EventStableId = source.EventStableId,
                IncidentRevision = source.IncidentRevision,
                SourceInstanceStableId = source.SourceInstanceStableId,
                NatureRouteCode = source.NatureRouteCode,
                IncidentTypeCode = source.IncidentTypeCode,
                StateCode = source.StateCode,
                OutcomeCode = source.OutcomeCode,
                Severity = source.Severity,
                RemainingSeverity = source.RemainingSeverity,
                OccurredWorldTick = source.OccurredWorldTick,
                DeadlineWorldTick = source.DeadlineWorldTick,
                SourceTargetStableId = source.SourceTargetStableId,
                FacilityStableId = source.FacilityStableId,
                SelectedChoiceStableId = source.SelectedChoiceStableId,
                RequiredWorldInteractionIds = source.RequiredWorldInteractionIds.ToArray(),
                RequiredActionCodes = source.RequiredActionCodes.ToArray(),
                CompletedActionCodes = source.CompletedActionCodes.ToArray(),
                SourceStableIds = source.SourceStableIds.ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        internal static SimulationNatureThreatEncounterSnapshot CloneNatureEncounter(
            SimulationNatureThreatEncounterSnapshot source)
            => new SimulationNatureThreatEncounterSnapshot
            {
                EncounterStableId = source.EncounterStableId,
                EncounterRevision = source.EncounterRevision,
                NatureRouteCode = source.NatureRouteCode,
                StateCode = source.StateCode,
                RiskBandCode = source.RiskBandCode,
                ThreatUnitCount = source.ThreatUnitCount,
                OccurredWorldTick = source.OccurredWorldTick,
                ResolvedWorldTick = source.ResolvedWorldTick,
                SourceIncidentStableIds = source.SourceIncidentStableIds.ToArray(),
                PresentationKey = source.PresentationKey,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private sealed class IncidentChoiceRule
        {
            public IncidentChoiceRule(string choiceStableId, bool unsafeChoice,
                string koreanTitle, string koreanSummary)
            {
                ChoiceStableId = choiceStableId;
                Unsafe = unsafeChoice;
                KoreanTitle = koreanTitle;
                KoreanSummary = koreanSummary;
            }

            public string ChoiceStableId { get; }
            public bool Unsafe { get; }
            public string KoreanTitle { get; }
            public string KoreanSummary { get; }
        }

        private sealed class AppliedRegionalIncidentResponseCommand
        {
            public AppliedRegionalIncidentResponseCommand(string payload,
                경영SimulationSessionSnapshot snapshot)
            {
                Payload = payload;
                Snapshot = snapshot;
            }

            public string Payload { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
