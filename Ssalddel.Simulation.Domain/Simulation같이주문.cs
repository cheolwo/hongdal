using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string 같이주문의향수량EffectCode = "GroupIntentQuantity";
        private const string 같이주문결과수량EffectCode = "GroupOrderResultQuantity";
        private const string 같이주문목표참여자EffectCode = "GroupOrderTargetParticipantCount";
        private const string 같이주문목표수량EffectCode = "GroupOrderTargetQuantity";
        private const string 초기상태Prefix = "InitialState:";
        private const string 완료상태Prefix = "CompletionState:";
        private readonly Dictionary<string, Simulation같이주문Snapshot> groupOrders =
            new Dictionary<string, Simulation같이주문Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, 적용된같이주문Command> appliedGroupOrderCommands =
            new Dictionary<string, 적용된같이주문Command>(StringComparer.Ordinal);

        public Simulation같이주문PreviewSnapshot PreviewGroupOrder(
            Simulation같이주문PreviewRequest request)
        {
            ValidateGroupOrderPreviewRequest(request);
            lock (gate)
            {
                return CreateGroupOrderPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmGroupOrder(
            Simulation같이주문ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateGroupOrderPreviewRequest(request.GroupOrder);

            lock (gate)
            {
                var payloadKey = BuildGroupOrderPayloadKey(request.GroupOrder);
                if (appliedGroupOrderCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }

                var preview = CreateGroupOrderPreview(request.GroupOrder);
                var snapshot = ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateGroupOrderDecisionRequest(request.GroupOrder, preview),
                });
                appliedGroupOrderCommands.Add(
                    request.CommandId,
                    new 적용된같이주문Command(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private Simulation같이주문PreviewSnapshot CreateGroupOrderPreview(
            Simulation같이주문PreviewRequest request)
        {
            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.같이주문);
            var participantCount = request.Intents
                .Select(value => value.ParticipantStableId.Trim())
                .Distinct(StringComparer.Ordinal)
                .Count();
            var totalQuantity = request.Intents.Sum(value => value.Quantity);
            var result = 같이주문상태Policy.판정(new 같이주문상태판정요청
            {
                참여자수 = participantCount,
                총희망수량 = totalQuantity,
                목표참여자수 = request.TargetParticipantCount,
                목표수량 = request.TargetQuantity,
            });
            var blocks = new List<string>();
            if (request.Intents.Length == 0)
                blocks.Add("SimulationGroupOrderIntentMissing");
            if (request.Intents.Any(value => !value.ExplicitParticipationConsent))
                blocks.Add("SimulationGroupOrderExplicitConsentRequired");
            if (request.Intents.GroupBy(value => value.IntentStableId.Trim(), StringComparer.Ordinal)
                .Any(group => group.Count() > 1))
                blocks.Add("SimulationGroupOrderIntentDuplicate");
            if (request.Intents.GroupBy(value => value.ParticipantStableId.Trim(), StringComparer.Ordinal)
                .Any(group => group.Count() > 1))
                blocks.Add("SimulationGroupOrderParticipantDuplicate");
            if (groupOrders.ContainsKey(request.GroupOrderStableId.Trim()))
                blocks.Add("SimulationGroupOrderAlreadyFinalized");

            var sources = MergeSources(
                request.SourceStableIds,
                MergeSources(
                    request.Intents.SelectMany(value => value.SourceStableIds).ToArray(),
                    workflow.SourceStableIds));
            var snapshot = new Simulation같이주문PreviewSnapshot
            {
                GroupOrderStableId = request.GroupOrderStableId.Trim(),
                ProductStableId = request.ProductStableId.Trim(),
                DeliveryScopeStableId = request.DeliveryScopeStableId.Trim(),
                AggregationFacilityStableId = request.AggregationFacilityStableId.Trim(),
                SuggestedStateCode = result.제안상태코드,
                CompletionStateCode = result.모집종료결과상태코드,
                ParticipantCount = participantCount,
                TotalQuantity = totalQuantity,
                UnitCode = request.UnitCode.Trim(),
                TargetParticipantCount = request.TargetParticipantCount,
                TargetQuantity = request.TargetQuantity,
                MinimumParticipantCountMet = result.최소참여자충족여부,
                TargetParticipantCountMet = result.목표참여자충족여부,
                TargetQuantityMet = result.목표수량충족여부,
                ExplicitTargetMet = result.명시목표충족여부,
                RuleRevision = result.RuleRevision,
                ExcludedOperationalEffectCodes = Copy(workflow.Simulation제외운영효과코드목록),
                BlockReasonCodes = blocks.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
            };
            snapshot.CommonDecisionPreview = CreateDecisionPreview(
                CreateGroupOrderDecisionRequest(request, snapshot));
            return snapshot;
        }

        private static SimulationDecisionPreviewRequest CreateGroupOrderDecisionRequest(
            Simulation같이주문PreviewRequest request,
            Simulation같이주문PreviewSnapshot preview)
        {
            var groupId = request.GroupOrderStableId.Trim();
            var unitCode = request.UnitCode.Trim();
            var sources = Copy(preview.SourceStableIds);
            var intentEffects = request.Intents.Select(value => new SimulationValueProjection
            {
                ValueTypeCode = 같이주문의향수량EffectCode,
                TargetLedgerStableId = value.IntentStableId.Trim(),
                BeforeValue = 0m,
                Delta = value.Quantity,
                AfterValue = value.Quantity,
                UnitCode = unitCode,
                SourceStableIds = MergeSources(
                    value.SourceStableIds,
                    new[] { value.ParticipantStableId.Trim() }),
            });
            var aggregateEffects = new[]
            {
                new SimulationValueProjection
                {
                    ValueTypeCode = 같이주문결과수량EffectCode,
                    TargetLedgerStableId = groupId,
                    BeforeValue = 0m,
                    Delta = preview.TotalQuantity,
                    AfterValue = preview.TotalQuantity,
                    UnitCode = unitCode,
                    SourceStableIds = sources,
                },
                new SimulationValueProjection
                {
                    ValueTypeCode = 같이주문목표참여자EffectCode,
                    TargetLedgerStableId = groupId,
                    BeforeValue = request.TargetParticipantCount,
                    Delta = preview.ParticipantCount - request.TargetParticipantCount,
                    AfterValue = preview.ParticipantCount,
                    UnitCode = "participant",
                    SourceStableIds = sources,
                },
                new SimulationValueProjection
                {
                    ValueTypeCode = 같이주문목표수량EffectCode,
                    TargetLedgerStableId = groupId,
                    BeforeValue = request.TargetQuantity,
                    Delta = preview.TotalQuantity - request.TargetQuantity,
                    AfterValue = preview.TotalQuantity,
                    UnitCode = unitCode,
                    SourceStableIds = sources,
                },
            };
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:group-order-finalization:" + groupId,
                DecisionTypeCode = Simulation같이주문DecisionTypeCodes.모집결과확정,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                    {
                        groupId,
                        request.ProductStableId.Trim(),
                        request.DeliveryScopeStableId.Trim(),
                        request.AggregationFacilityStableId.Trim(),
                    }
                    .Concat(request.Intents.Select(value => value.IntentStableId.Trim()))
                    .ToArray(),
                ExpectedEffects = intentEffects.Concat(aggregateEffects).ToArray(),
                Uncertainties = new[]
                {
                    초기상태Prefix + preview.SuggestedStateCode,
                    완료상태Prefix + preview.CompletionStateCode,
                },
                BlockReasonCodes = Copy(preview.BlockReasonCodes),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:group-order-finalization:" + groupId,
                    TaskTypeCode = "GroupOrderRecruitmentFinalization",
                    FacilityStableId = request.AggregationFacilityStableId.Trim(),
                    AssignedCapacity = preview.TotalQuantity,
                    AssignedCapacityUnitCode = unitCode,
                    DurationTicks = request.FinalizationDurationTicks,
                    InputLotStableIds = request.Intents.Select(value => value.IntentStableId.Trim()).ToArray(),
                    OutputCandidateCodes = new[] { preview.CompletionStateCode },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation같이주문Snapshot? PrepareGroupOrder(
            SimulationDecisionPreviewRequest request)
        {
            if (!string.Equals(
                request.DecisionTypeCode,
                Simulation같이주문DecisionTypeCodes.모집결과확정,
                StringComparison.Ordinal)) return null;

            var groupId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("group-order:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationGroupOrderStableIdInvalid");
            var productId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("product:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationGroupOrderProductStableIdInvalid");
            var deliveryScopeId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("delivery-scope:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationGroupOrderDeliveryScopeStableIdInvalid");
            var aggregationFacilityId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("facility:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationGroupOrderAggregationFacilityStableIdInvalid");
            if (groupOrders.ContainsKey(groupId))
                throw new SimulationConflictException("SimulationGroupOrderAlreadyFinalized");

            var intentEffects = request.ExpectedEffects
                .Where(value => value.ValueTypeCode == 같이주문의향수량EffectCode)
                .ToArray();
            var unitCode = intentEffects.Select(value => value.UnitCode)
                .Distinct(StringComparer.Ordinal).Single();
            var intents = intentEffects.Select(value =>
            {
                var participantId = value.SourceStableIds.Single(source =>
                    source.StartsWith("participant:", StringComparison.Ordinal));
                return new Simulation같이주문의향Snapshot
                {
                    IntentStableId = value.TargetLedgerStableId,
                    ParticipantStableId = participantId,
                    Quantity = value.AfterValue,
                    UnitCode = value.UnitCode,
                    ExplicitParticipationConsent = true,
                    SourceStableIds = Copy(value.SourceStableIds),
                };
            }).OrderBy(value => value.IntentStableId, StringComparer.Ordinal).ToArray();
            var targetParticipant = request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == 같이주문목표참여자EffectCode);
            var targetQuantity = request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == 같이주문목표수량EffectCode);
            var initialState = ParseState(request.Uncertainties, 초기상태Prefix);
            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.같이주문);
            return new Simulation같이주문Snapshot
            {
                GroupOrderStableId = groupId,
                ProductStableId = productId,
                DeliveryScopeStableId = deliveryScopeId,
                AggregationFacilityStableId = aggregationFacilityId,
                StateCode = initialState,
                Revision = 1,
                ParticipantCount = intents.Select(value => value.ParticipantStableId)
                    .Distinct(StringComparer.Ordinal).Count(),
                TotalQuantity = intents.Sum(value => value.Quantity),
                UnitCode = unitCode,
                TargetParticipantCount = decimal.ToInt32(targetParticipant.BeforeValue),
                TargetQuantity = targetQuantity.BeforeValue,
                RuleRevision = workflow.RuleRevision,
                ExcludedOperationalEffectCodes = Copy(workflow.Simulation제외운영효과코드목록),
                SourceStableIds = MergeSources(request.SourceStableIds, workflow.SourceStableIds),
                Intents = intents,
            };
        }

        private void ScheduleGroupOrder(
            Simulation같이주문Snapshot? groupOrder,
            SimulationDecisionSnapshot decision,
            SimulationTaskSnapshot task)
        {
            if (groupOrder == null) return;
            groupOrder.DecisionStableId = decision.DecisionStableId;
            groupOrder.TaskStableId = task.TaskStableId;
            groupOrder.CreatedTick = CurrentTick;
            groupOrders.Add(groupOrder.GroupOrderStableId, groupOrder);
        }

        private void ApplyGroupOrderForTask(SimulationTaskSnapshot task, int completedTick)
        {
            var groupOrder = groupOrders.Values.FirstOrDefault(value =>
                string.Equals(value.TaskStableId, task.TaskStableId, StringComparison.Ordinal)
                && !value.FinalizedTick.HasValue);
            if (groupOrder == null) return;
            var decision = decisions[groupOrder.DecisionStableId];
            var completionState = ParseState(decision.Uncertainties, 완료상태Prefix);
            var transition = 업무상태전이Policy.판정(
                업무흐름코드.같이주문,
                groupOrder.StateCode,
                completionState);
            if (!transition.허용여부)
                throw new SimulationConflictException("SimulationGroupOrderTransitionNotAllowed");
            groupOrder.StateCode = completionState;
            groupOrder.Revision++;
            groupOrder.FinalizedTick = completedTick;
        }

        private static string ParseState(string[] values, string prefix)
        {
            var value = values.SingleOrDefault(item => item.StartsWith(prefix, StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationGroupOrderStateProjectionMissing");
            return value.Substring(prefix.Length);
        }

        private Simulation같이주문Snapshot[] CreateGroupOrderSnapshots()
            => groupOrders.Values
                .OrderBy(value => value.GroupOrderStableId, StringComparer.Ordinal)
                .Select(CloneGroupOrder)
                .ToArray();

        internal static Simulation같이주문Snapshot CloneGroupOrder(Simulation같이주문Snapshot source)
            => new Simulation같이주문Snapshot
            {
                GroupOrderStableId = source.GroupOrderStableId,
                ProductStableId = source.ProductStableId,
                DeliveryScopeStableId = source.DeliveryScopeStableId,
                AggregationFacilityStableId = source.AggregationFacilityStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                ParticipantCount = source.ParticipantCount,
                TotalQuantity = source.TotalQuantity,
                UnitCode = source.UnitCode,
                TargetParticipantCount = source.TargetParticipantCount,
                TargetQuantity = source.TargetQuantity,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                CreatedTick = source.CreatedTick,
                FinalizedTick = source.FinalizedTick,
                RuleRevision = source.RuleRevision,
                ExcludedOperationalEffectCodes = Copy(source.ExcludedOperationalEffectCodes),
                SourceStableIds = Copy(source.SourceStableIds),
                Intents = source.Intents.Select(value => new Simulation같이주문의향Snapshot
                {
                    IntentStableId = value.IntentStableId,
                    ParticipantStableId = value.ParticipantStableId,
                    Quantity = value.Quantity,
                    UnitCode = value.UnitCode,
                    ExplicitParticipationConsent = value.ExplicitParticipationConsent,
                    SourceStableIds = Copy(value.SourceStableIds),
                }).ToArray(),
            };

        private static void ValidateGroupOrderPreviewRequest(Simulation같이주문PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.GroupOrderStableId, "SimulationGroupOrderStableIdInvalid");
            if (!request.GroupOrderStableId.Trim().StartsWith("group-order:", StringComparison.Ordinal))
                throw new SimulationContractException("SimulationGroupOrderStableIdInvalid");
            RequireStableId(request.ProductStableId, "SimulationGroupOrderProductStableIdInvalid");
            RequireStableId(request.DeliveryScopeStableId, "SimulationGroupOrderDeliveryScopeStableIdInvalid");
            if (!request.DeliveryScopeStableId.Trim().StartsWith("delivery-scope:", StringComparison.Ordinal))
                throw new SimulationContractException("SimulationGroupOrderDeliveryScopeStableIdInvalid");
            RequireStableId(request.AggregationFacilityStableId, "SimulationGroupOrderAggregationFacilityStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationActorStableIdInvalid");
            RequireText(request.UnitCode, "SimulationGroupOrderUnitCodeMissing");
            if (request.TargetParticipantCount <= 0)
                throw new SimulationContractException("SimulationGroupOrderTargetParticipantCountInvalid");
            if (request.TargetQuantity <= 0m)
                throw new SimulationContractException("SimulationGroupOrderTargetQuantityInvalid");
            if (request.FinalizationDurationTicks <= 0 || request.FinalizationDurationTicks > 7)
                throw new SimulationContractException("SimulationGroupOrderDurationInvalid");
            if (request.Intents == null)
                throw new SimulationContractException("SimulationGroupOrderIntentsInvalid");
            foreach (var intent in request.Intents)
            {
                if (intent == null)
                    throw new SimulationContractException("SimulationGroupOrderIntentsInvalid");
                RequireStableId(intent.IntentStableId, "SimulationGroupOrderIntentStableIdInvalid");
                if (!intent.IntentStableId.Trim().StartsWith("group-intent:", StringComparison.Ordinal))
                    throw new SimulationContractException("SimulationGroupOrderIntentStableIdInvalid");
                RequireStableId(intent.ParticipantStableId, "SimulationGroupOrderParticipantStableIdInvalid");
                if (!intent.ParticipantStableId.Trim().StartsWith("participant:", StringComparison.Ordinal))
                    throw new SimulationContractException("SimulationGroupOrderParticipantStableIdInvalid");
                if (intent.Quantity <= 0m)
                    throw new SimulationContractException("SimulationGroupOrderIntentQuantityInvalid");
                ValidateIds(intent.SourceStableIds, true, "SimulationGroupOrderIntentSourceStableIdsInvalid");
            }
            ValidateIds(request.SourceStableIds, true, "SimulationGroupOrderSourceStableIdsInvalid");
        }

        private static string BuildGroupOrderPayloadKey(Simulation같이주문PreviewRequest request)
            => string.Join("\u001e", new[]
            {
                request.GroupOrderStableId.Trim(),
                request.ProductStableId.Trim(),
                request.DeliveryScopeStableId.Trim(),
                request.AggregationFacilityStableId.Trim(),
                request.ActorStableId.Trim(),
                request.UnitCode.Trim(),
                request.TargetParticipantCount.ToString(CultureInfo.InvariantCulture),
                request.TargetQuantity.ToString(CultureInfo.InvariantCulture),
                request.FinalizationDurationTicks.ToString(CultureInfo.InvariantCulture),
                string.Join("\u001f", request.Intents
                    .OrderBy(value => value.IntentStableId, StringComparer.Ordinal)
                    .Select(value => string.Join("\u001d", new[]
                    {
                        value.IntentStableId.Trim(),
                        value.ParticipantStableId.Trim(),
                        value.Quantity.ToString(CultureInfo.InvariantCulture),
                        value.ExplicitParticipationConsent.ToString(CultureInfo.InvariantCulture),
                        string.Join("\u001c", value.SourceStableIds.OrderBy(source => source, StringComparer.Ordinal)),
                    }))),
                string.Join("\u001f", request.SourceStableIds.OrderBy(value => value, StringComparer.Ordinal)),
            });

        private sealed class 적용된같이주문Command
        {
            public 적용된같이주문Command(string payloadKey, 경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
