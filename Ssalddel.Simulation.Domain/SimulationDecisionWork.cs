using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly Dictionary<string, SimulationDecisionSnapshot> decisions =
            new Dictionary<string, SimulationDecisionSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationTaskSnapshot> tasks =
            new Dictionary<string, SimulationTaskSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationEffectRecord> effects =
            new Dictionary<string, SimulationEffectRecord>(StringComparer.Ordinal);
        private readonly Dictionary<string, 적용된DecisionConfirmCommand> appliedDecisionCommands =
            new Dictionary<string, 적용된DecisionConfirmCommand>(StringComparer.Ordinal);

        public SimulationDecisionPreviewSnapshot PreviewDecision(
            SimulationDecisionPreviewRequest request)
        {
            ValidateDecisionPreview(request);
            lock (gate)
            {
                return CreateDecisionPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmDecision(
            SimulationDecisionConfirmRequest request)
        {
            ValidateDecisionConfirm(request);
            lock (gate)
            {
                if (HasAppliedHarvestDispositionImpactCommand(request.CommandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                return ConfirmDecisionCore(request, false, AppendDecisionConfirmCommand);
            }
        }

        private 경영SimulationSessionSnapshot ConfirmDecisionCore(
            SimulationDecisionConfirmRequest request,
            bool includeExpectedCostsAsEffects,
            Action<SimulationDecisionConfirmRequest> appendCommand)
        {
                if (appliedCommands.ContainsKey(request.CommandId)
                    || appliedTurnClosingCommands.ContainsKey(request.CommandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                var payloadKey = BuildDecisionPayloadKey(request.Preview);
                if (appliedDecisionCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }

                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");

                var preview = CreateDecisionPreview(request.Preview);
                if (preview.Decision.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException("SimulationDecisionPreviewBlocked");
                if (CurrentTick + preview.TaskPlan.DurationTicks > DurationTicks)
                    throw new SimulationConflictException("SimulationTaskDurationExceeded");
                if (decisions.ContainsKey(preview.Decision.DecisionStableId))
                    throw new SimulationConflictException("SimulationDecisionStableIdConflict");
                if (tasks.ContainsKey(preview.TaskPlan.TaskStableId))
                    throw new SimulationConflictException("SimulationTaskStableIdConflict");

                var decision = CloneDecision(preview.Decision);
                decision.StateCode = SimulationDecisionStateCodes.Confirmed;
                decision.Revision = 1;
                decision.ConfirmedTick = CurrentTick;

                var task = new SimulationTaskSnapshot
                {
                    TaskStableId = preview.TaskPlan.TaskStableId,
                    TaskTypeCode = preview.TaskPlan.TaskTypeCode,
                    StateCode = SimulationTaskStateCodes.Scheduled,
                    Revision = 1,
                    CausedByDecisionStableId = decision.DecisionStableId,
                    FacilityStableId = preview.TaskPlan.FacilityStableId,
                    AssignedCapacity = preview.TaskPlan.AssignedCapacity,
                    AssignedCapacityUnitCode = preview.TaskPlan.AssignedCapacityUnitCode,
                    ScheduledStartTick = CurrentTick + 1,
                    ExpectedEndTick = CurrentTick + preview.TaskPlan.DurationTicks,
                    InputLotStableIds = Copy(preview.TaskPlan.InputLotStableIds),
                    OutputCandidateCodes = Copy(preview.TaskPlan.OutputCandidateCodes),
                    SourceStableIds = MergeSources(
                        decision.SourceStableIds,
                        preview.TaskPlan.SourceStableIds),
                };
                var individualOrder = PrepareIndividualOrderReservation(request.Preview, preview);
                var cancelledIndividualOrder = PrepareIndividualOrderCancellation(request.Preview);
                var freightReceipt = PrepareFreightReceipt(request.Preview);
                var groupOrder = PrepareGroupOrder(request.Preview);
                var foodDelivery = PrepareFoodDelivery(request.Preview);
                var foodDeliveryReceipt = PrepareFoodDeliveryReceipt(request.Preview);
                var marketConsumption = PrepareMarketConsumption(request.Preview);
                var exportPreparation = Prepare수출준비(request.Preview, preview);
                var exportCargoPreparation = Prepare수출Cargo준비(request.Preview, preview);
                var exportCargoHandoff = Prepare수출Cargo인계(request.Preview, preview);
                var exportPortReceipt = Prepare수출항만인수(request.Preview, preview);
                var exportReadinessReview = Prepare수출준비성검토(request.Preview, preview);
                var exportShipmentPlan = Prepare수출선적계획(request.Preview, preview);
                var exportShipmentExecution = Prepare수출선적실행(request.Preview, preview);

                var effectValues = includeExpectedCostsAsEffects
                    ? decision.ExpectedCosts.Concat(decision.ExpectedEffects)
                    : decision.ExpectedEffects.AsEnumerable();
                var pendingEffects = effectValues
                    .Select((value, index) => new SimulationEffectRecord
                    {
                        EffectStableId = task.TaskStableId + ":effect:" + (index + 1).ToString(CultureInfo.InvariantCulture),
                        EffectTypeCode = value.ValueTypeCode,
                        StateCode = SimulationEffectStateCodes.Pending,
                        Revision = 1,
                        CausedByDecisionStableId = decision.DecisionStableId,
                        CausedByTaskStableId = task.TaskStableId,
                        TargetLedgerStableId = value.TargetLedgerStableId,
                        BeforeValue = value.BeforeValue,
                        Delta = value.Delta,
                        AfterValue = value.AfterValue,
                        UnitCode = value.UnitCode,
                        SourceStableIds = MergeSources(value.SourceStableIds, task.SourceStableIds),
                    })
                    .ToArray();

                if (pendingEffects.Any(value => effects.ContainsKey(value.EffectStableId)))
                    throw new SimulationConflictException("SimulationEffectStableIdConflict");

                decisions.Add(decision.DecisionStableId, decision);
                tasks.Add(task.TaskStableId, task);
                foreach (var effect in pendingEffects)
                    effects.Add(effect.EffectStableId, effect);
                ReserveIndividualOrder(individualOrder);
                ScheduleIndividualOrderCancellation(cancelledIndividualOrder, task);
                ScheduleFreightReceipt(freightReceipt, decision, task);
                ScheduleGroupOrder(groupOrder, decision, task);
                ScheduleFoodDelivery(foodDelivery, decision, task);
                ScheduleFoodDeliveryReceipt(foodDeliveryReceipt, decision, task);
                ScheduleMarketConsumption(marketConsumption, decision, task);
                Schedule수출준비(exportPreparation);
                Schedule수출Cargo준비(exportCargoPreparation);
                Schedule수출Cargo인계(exportCargoHandoff);
                Schedule수출항만인수(exportPortReceipt);
                Schedule수출준비성검토(exportReadinessReview);
                Schedule수출선적계획(exportShipmentPlan);
                Schedule수출선적실행(exportShipmentExecution);

                Revision++;
                appendCommand(request);
                var snapshot = CreateSnapshot();
                appliedDecisionCommands.Add(
                    request.CommandId,
                    new 적용된DecisionConfirmCommand(payloadKey, Clone(snapshot)));
                return snapshot;
        }

        private void AdvanceDecisionWork(int currentTick)
        {
            foreach (var task in tasks.Values
                .OrderBy(value => value.ExpectedEndTick)
                .ThenBy(value => value.TaskStableId, StringComparer.Ordinal))
            {
                if (task.StateCode == SimulationTaskStateCodes.Completed
                    || task.StateCode == SimulationTaskStateCodes.Cancelled)
                    continue;

                if (currentTick >= task.ExpectedEndTick)
                {
                    AdvanceLogisticsMovementForTask(task, currentTick);
                    ApplySettlementEconomyForTask(task, task.ExpectedEndTick);
                    ApplyIndividualOrderCancellationForTask(task, task.ExpectedEndTick);
                    ApplyIndividualOrderForTask(task, task.ExpectedEndTick);
                    ApplyFreightReceiptForTask(task, task.ExpectedEndTick);
                    ApplyGroupOrderForTask(task, task.ExpectedEndTick);
                    AdvanceFoodDeliveryForTask(task, task.ExpectedEndTick);
                    ApplyMarketConsumptionForTask(task, task.ExpectedEndTick);
                    Advance수출준비ForTask(task, task.ExpectedEndTick);
                    Advance수출Cargo준비ForTask(task, task.ExpectedEndTick);
                    Advance수출Cargo인계ForTask(task, task.ExpectedEndTick);
                    Advance수출항만인수ForTask(task, task.ExpectedEndTick);
                    Advance수출준비성검토ForTask(task, task.ExpectedEndTick);
                    Advance수출선적계획ForTask(task, task.ExpectedEndTick);
                    Advance수출선적실행ForTask(task, task.ExpectedEndTick);
                    task.StateCode = SimulationTaskStateCodes.Completed;
                    task.Revision++;
                    task.ActualEndTick = task.ExpectedEndTick;
                    foreach (var effect in effects.Values.Where(
                        value => value.CausedByTaskStableId == task.TaskStableId
                            && value.StateCode == SimulationEffectStateCodes.Pending))
                    {
                        effect.StateCode = SimulationEffectStateCodes.Applied;
                        effect.Revision++;
                        effect.AppliedTick = task.ExpectedEndTick;
                    }
                }
                else if (currentTick >= task.ScheduledStartTick
                    && task.StateCode == SimulationTaskStateCodes.Scheduled)
                {
                    AdvanceLogisticsMovementForTask(task, currentTick);
                    AdvanceFoodDeliveryForTask(task, currentTick);
                    Advance수출준비ForTask(task, currentTick);
                    Advance수출선적실행ForTask(task, currentTick);
                    task.StateCode = SimulationTaskStateCodes.InProgress;
                    task.Revision++;
                }
                else if (currentTick >= task.ScheduledStartTick)
                {
                    AdvanceLogisticsMovementForTask(task, currentTick);
                    AdvanceFoodDeliveryForTask(task, currentTick);
                    Advance수출준비ForTask(task, currentTick);
                    Advance수출선적실행ForTask(task, currentTick);
                }
            }
        }

        private bool HasAppliedDecisionCommand(string commandId)
            => appliedDecisionCommands.ContainsKey(commandId)
                || HasAppliedHarvestDispositionImpactCommand(commandId)
                || appliedLogisticsMovementCommands.ContainsKey(commandId)
                || appliedTurnClosingCommands.ContainsKey(commandId);

        private SimulationDecisionPreviewSnapshot CreateDecisionPreview(
            SimulationDecisionPreviewRequest request)
            => new SimulationDecisionPreviewSnapshot
            {
                Decision = new SimulationDecisionSnapshot
                {
                    DecisionStableId = request.DecisionStableId.Trim(),
                    DecisionTypeCode = request.DecisionTypeCode.Trim(),
                    StateCode = SimulationDecisionStateCodes.Previewed,
                    Revision = 0,
                    SessionStableId = SessionStableId,
                    FactionStableId = FactionStableId,
                    TerritoryStableId = TerritoryStableId,
                    SettlementStableId = SettlementStableId,
                    ActorStableId = request.ActorStableId.Trim(),
                    TargetStableIds = NormalizeIds(request.TargetStableIds),
                    CreatedTick = CurrentTick,
                    ExpectedCosts = NormalizeValues(request.ExpectedCosts),
                    ExpectedEffects = NormalizeValues(request.ExpectedEffects),
                    Uncertainties = NormalizeText(request.Uncertainties),
                    BlockReasonCodes = NormalizeIds(request.BlockReasonCodes),
                    SourceStableIds = NormalizeIds(request.SourceStableIds),
                },
                TaskPlan = new SimulationTaskPlanSnapshot
                {
                    TaskStableId = request.Task.TaskStableId.Trim(),
                    TaskTypeCode = request.Task.TaskTypeCode.Trim(),
                    FacilityStableId = request.Task.FacilityStableId.Trim(),
                    AssignedCapacity = request.Task.AssignedCapacity,
                    AssignedCapacityUnitCode = request.Task.AssignedCapacityUnitCode.Trim(),
                    DurationTicks = request.Task.DurationTicks,
                    InputLotStableIds = NormalizeIds(request.Task.InputLotStableIds),
                    OutputCandidateCodes = NormalizeIds(request.Task.OutputCandidateCodes),
                    SourceStableIds = NormalizeIds(request.Task.SourceStableIds),
                },
            };

        private SimulationDecisionSnapshot[] CreateDecisionSnapshots()
            => decisions.Values
                .OrderBy(value => value.CreatedTick)
                .ThenBy(value => value.DecisionStableId, StringComparer.Ordinal)
                .Select(CloneDecision)
                .ToArray();

        private SimulationTaskSnapshot[] CreateTaskSnapshots()
            => tasks.Values
                .OrderBy(value => value.ScheduledStartTick)
                .ThenBy(value => value.TaskStableId, StringComparer.Ordinal)
                .Select(CloneTask)
                .ToArray();

        private SimulationEffectRecord[] CreateEffectSnapshots()
            => effects.Values
                .OrderBy(value => value.EffectStableId, StringComparer.Ordinal)
                .Select(CloneEffect)
                .ToArray();

        internal static void ValidateDecisionConfirm(SimulationDecisionConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.Preview == null)
                throw new SimulationContractException("SimulationDecisionPreviewMissing");
            ValidateDecisionPreview(request.Preview);
        }

        private static void ValidateDecisionPreview(SimulationDecisionPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.DecisionStableId, "SimulationDecisionStableIdInvalid");
            RequireStableId(request.DecisionTypeCode, "SimulationDecisionTypeCodeInvalid");
            RequireStableId(request.ActorStableId, "SimulationDecisionActorStableIdInvalid");
            ValidateIds(request.TargetStableIds, true, "SimulationDecisionTargetStableIdsInvalid");
            ValidateValues(request.ExpectedCosts, false, "SimulationDecisionExpectedCostsInvalid");
            ValidateValues(request.ExpectedEffects, true, "SimulationDecisionExpectedEffectsInvalid");
            ValidateText(request.Uncertainties, "SimulationDecisionUncertaintiesInvalid");
            ValidateIds(request.BlockReasonCodes, false, "SimulationDecisionBlockReasonCodesInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationDecisionSourceStableIdsInvalid");
            if (request.Task == null)
                throw new SimulationContractException("SimulationTaskPlanMissing");
            RequireStableId(request.Task.TaskStableId, "SimulationTaskStableIdInvalid");
            RequireStableId(request.Task.TaskTypeCode, "SimulationTaskTypeCodeInvalid");
            RequireStableId(request.Task.FacilityStableId, "SimulationTaskFacilityStableIdInvalid");
            if (request.Task.AssignedCapacity <= 0m)
                throw new SimulationContractException("SimulationTaskAssignedCapacityInvalid");
            RequireStableId(
                request.Task.AssignedCapacityUnitCode,
                "SimulationTaskAssignedCapacityUnitCodeInvalid");
            if (request.Task.DurationTicks <= 0 || request.Task.DurationTicks > 28)
                throw new SimulationContractException("SimulationTaskDurationTicksInvalid");
            ValidateIds(request.Task.InputLotStableIds, true, "SimulationTaskInputLotStableIdsInvalid");
            ValidateIds(request.Task.OutputCandidateCodes, false, "SimulationTaskOutputCandidateCodesInvalid");
            ValidateIds(request.Task.SourceStableIds, true, "SimulationTaskSourceStableIdsInvalid");
        }

        private static void ValidateValues(
            SimulationValueProjection[] values,
            bool requireAny,
            string errorCode)
        {
            if (values == null || (requireAny && values.Length == 0))
                throw new SimulationContractException(errorCode);
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (value == null)
                    throw new SimulationContractException(errorCode);
                RequireStableId(value.ValueTypeCode, errorCode);
                RequireStableId(value.TargetLedgerStableId, errorCode);
                RequireStableId(value.UnitCode, errorCode);
                ValidateIds(value.SourceStableIds, true, errorCode);
                if (value.BeforeValue + value.Delta != value.AfterValue)
                    throw new SimulationContractException("SimulationValueConservationInvalid");
                var key = value.ValueTypeCode.Trim() + "\u001f"
                    + value.TargetLedgerStableId.Trim() + "\u001f"
                    + value.UnitCode.Trim();
                if (!keys.Add(key))
                    throw new SimulationContractException("SimulationValueProjectionDuplicate");
            }
        }

        private static void ValidateIds(string[] values, bool requireAny, string errorCode)
        {
            if (values == null || (requireAny && values.Length == 0))
                throw new SimulationContractException(errorCode);
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                RequireStableId(value, errorCode);
                if (!unique.Add(value.Trim()))
                    throw new SimulationContractException(errorCode);
            }
        }

        private static void ValidateText(string[] values, string errorCode)
        {
            if (values == null)
                throw new SimulationContractException(errorCode);
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new SimulationContractException(errorCode);
            }
        }

        internal static string BuildDecisionPayloadKey(SimulationDecisionPreviewRequest request)
        {
            var preview = NormalizePreviewForPayload(request);
            return string.Join("\u001e", new[]
            {
                preview.DecisionStableId,
                preview.DecisionTypeCode,
                preview.ActorStableId,
                string.Join("\u001f", preview.TargetStableIds),
                ValuesKey(preview.ExpectedCosts),
                ValuesKey(preview.ExpectedEffects),
                string.Join("\u001f", preview.Uncertainties),
                string.Join("\u001f", preview.BlockReasonCodes),
                string.Join("\u001f", preview.SourceStableIds),
                preview.Task.TaskStableId,
                preview.Task.TaskTypeCode,
                preview.Task.FacilityStableId,
                preview.Task.AssignedCapacity.ToString(CultureInfo.InvariantCulture),
                preview.Task.AssignedCapacityUnitCode,
                preview.Task.DurationTicks.ToString(CultureInfo.InvariantCulture),
                string.Join("\u001f", preview.Task.InputLotStableIds),
                string.Join("\u001f", preview.Task.OutputCandidateCodes),
                string.Join("\u001f", preview.Task.SourceStableIds),
            });
        }

        private static SimulationDecisionPreviewRequest NormalizePreviewForPayload(
            SimulationDecisionPreviewRequest request)
            => new SimulationDecisionPreviewRequest
            {
                DecisionStableId = request.DecisionStableId.Trim(),
                DecisionTypeCode = request.DecisionTypeCode.Trim(),
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = NormalizeIds(request.TargetStableIds),
                ExpectedCosts = NormalizeValues(request.ExpectedCosts),
                ExpectedEffects = NormalizeValues(request.ExpectedEffects),
                Uncertainties = NormalizeText(request.Uncertainties),
                BlockReasonCodes = NormalizeIds(request.BlockReasonCodes),
                SourceStableIds = NormalizeIds(request.SourceStableIds),
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = request.Task.TaskStableId.Trim(),
                    TaskTypeCode = request.Task.TaskTypeCode.Trim(),
                    FacilityStableId = request.Task.FacilityStableId.Trim(),
                    AssignedCapacity = request.Task.AssignedCapacity,
                    AssignedCapacityUnitCode = request.Task.AssignedCapacityUnitCode.Trim(),
                    DurationTicks = request.Task.DurationTicks,
                    InputLotStableIds = NormalizeIds(request.Task.InputLotStableIds),
                    OutputCandidateCodes = NormalizeIds(request.Task.OutputCandidateCodes),
                    SourceStableIds = NormalizeIds(request.Task.SourceStableIds),
                },
            };

        private static string ValuesKey(SimulationValueProjection[] values)
            => string.Join("\u001f", values.Select(value => string.Join("|", new[]
            {
                value.ValueTypeCode,
                value.TargetLedgerStableId,
                value.BeforeValue.ToString(CultureInfo.InvariantCulture),
                value.Delta.ToString(CultureInfo.InvariantCulture),
                value.AfterValue.ToString(CultureInfo.InvariantCulture),
                value.UnitCode,
                string.Join(",", value.SourceStableIds),
            })));

        private static string[] NormalizeIds(string[] values)
            => values.Select(value => value.Trim())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private static string[] NormalizeText(string[] values)
            => values.Select(value => value.Trim())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private static SimulationValueProjection[] NormalizeValues(
            SimulationValueProjection[] values)
            => values.Select(CloneValue)
                .OrderBy(value => value.ValueTypeCode, StringComparer.Ordinal)
                .ThenBy(value => value.TargetLedgerStableId, StringComparer.Ordinal)
                .ThenBy(value => value.UnitCode, StringComparer.Ordinal)
                .ToArray();

        private static SimulationValueProjection CloneValue(SimulationValueProjection source)
            => new SimulationValueProjection
            {
                ValueTypeCode = source.ValueTypeCode.Trim(),
                TargetLedgerStableId = source.TargetLedgerStableId.Trim(),
                BeforeValue = source.BeforeValue,
                Delta = source.Delta,
                AfterValue = source.AfterValue,
                UnitCode = source.UnitCode.Trim(),
                SourceStableIds = NormalizeIds(source.SourceStableIds),
            };

        private static SimulationDecisionSnapshot CloneDecision(SimulationDecisionSnapshot source)
            => new SimulationDecisionSnapshot
            {
                DecisionStableId = source.DecisionStableId,
                DecisionTypeCode = source.DecisionTypeCode,
                StateCode = source.StateCode,
                Revision = source.Revision,
                SessionStableId = source.SessionStableId,
                FactionStableId = source.FactionStableId,
                TerritoryStableId = source.TerritoryStableId,
                SettlementStableId = source.SettlementStableId,
                ActorStableId = source.ActorStableId,
                TargetStableIds = Copy(source.TargetStableIds),
                CreatedTick = source.CreatedTick,
                ConfirmedTick = source.ConfirmedTick,
                ExpectedCosts = source.ExpectedCosts.Select(CloneValue).ToArray(),
                ExpectedEffects = source.ExpectedEffects.Select(CloneValue).ToArray(),
                Uncertainties = Copy(source.Uncertainties),
                BlockReasonCodes = Copy(source.BlockReasonCodes),
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static SimulationTaskSnapshot CloneTask(SimulationTaskSnapshot source)
            => new SimulationTaskSnapshot
            {
                TaskStableId = source.TaskStableId,
                TaskTypeCode = source.TaskTypeCode,
                StateCode = source.StateCode,
                Revision = source.Revision,
                CausedByDecisionStableId = source.CausedByDecisionStableId,
                FacilityStableId = source.FacilityStableId,
                AssignedCapacity = source.AssignedCapacity,
                AssignedCapacityUnitCode = source.AssignedCapacityUnitCode,
                ScheduledStartTick = source.ScheduledStartTick,
                ExpectedEndTick = source.ExpectedEndTick,
                ActualEndTick = source.ActualEndTick,
                InputLotStableIds = Copy(source.InputLotStableIds),
                OutputCandidateCodes = Copy(source.OutputCandidateCodes),
                BlockReasonCodes = Copy(source.BlockReasonCodes),
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static SimulationEffectRecord CloneEffect(SimulationEffectRecord source)
            => new SimulationEffectRecord
            {
                EffectStableId = source.EffectStableId,
                EffectTypeCode = source.EffectTypeCode,
                StateCode = source.StateCode,
                Revision = source.Revision,
                AppliedTick = source.AppliedTick,
                CausedByDecisionStableId = source.CausedByDecisionStableId,
                CausedByTaskStableId = source.CausedByTaskStableId,
                TargetLedgerStableId = source.TargetLedgerStableId,
                BeforeValue = source.BeforeValue,
                Delta = source.Delta,
                AfterValue = source.AfterValue,
                UnitCode = source.UnitCode,
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static SimulationDecisionSnapshot[] CloneDecisions(
            SimulationDecisionSnapshot[] source)
            => source.Select(CloneDecision).ToArray();

        private static SimulationTaskSnapshot[] CloneTasks(SimulationTaskSnapshot[] source)
            => source.Select(CloneTask).ToArray();

        private static SimulationEffectRecord[] CloneEffects(SimulationEffectRecord[] source)
            => source.Select(CloneEffect).ToArray();

        private static string[] Copy(string[] source)
            => source.ToArray();

        private static string[] MergeSources(string[] first, string[] second)
            => first.Concat(second)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();

        private sealed class 적용된DecisionConfirmCommand
        {
            public 적용된DecisionConfirmCommand(
                string payloadKey,
                경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
