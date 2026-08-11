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
        private const string 음식배달수량EffectCode = "FoodDeliveryQuantity";
        private const string 음식배달조리기간EffectCode = "FoodDeliveryPreparationDuration";
        private const string 음식배달배송기간EffectCode = "FoodDeliveryTravelDuration";
        private const string 음식배달목적시설EffectCode = "FoodDeliveryDestinationFacility";
        private const string 음식배달수령수량EffectCode = "FoodDeliveryReceiptQuantity";
        private readonly Dictionary<string, Simulation음식배달Snapshot> foodDeliveries =
            new Dictionary<string, Simulation음식배달Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, 적용된음식배달Command> appliedFoodDeliveryCommands =
            new Dictionary<string, 적용된음식배달Command>(StringComparer.Ordinal);
        private readonly Dictionary<string, 적용된음식배달Command> appliedFoodReceiptCommands =
            new Dictionary<string, 적용된음식배달Command>(StringComparer.Ordinal);

        public Simulation음식배달PreviewSnapshot PreviewFoodDelivery(
            Simulation음식배달PreviewRequest request)
        {
            ValidateFoodDeliveryRequest(request);
            lock (gate) return CreateFoodDeliveryPreview(request);
        }

        public 경영SimulationSessionSnapshot ConfirmFoodDelivery(
            Simulation음식배달ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateFoodDeliveryRequest(request.FoodDelivery);
            lock (gate)
            {
                var payloadKey = BuildFoodDeliveryPayloadKey(request.FoodDelivery);
                if (appliedFoodDeliveryCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                var preview = CreateFoodDeliveryPreview(request.FoodDelivery);
                var snapshot = ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateFoodDeliveryDecisionRequest(request.FoodDelivery, preview),
                });
                appliedFoodDeliveryCommands.Add(
                    request.CommandId,
                    new 적용된음식배달Command(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        public SimulationDecisionPreviewSnapshot PreviewFoodDeliveryReceipt(
            Simulation음식배달수령PreviewRequest request)
        {
            ValidateFoodDeliveryReceiptRequest(request);
            lock (gate) return CreateDecisionPreview(
                CreateFoodDeliveryReceiptDecisionRequest(request));
        }

        public 경영SimulationSessionSnapshot ConfirmFoodDeliveryReceipt(
            Simulation음식배달수령ConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateFoodDeliveryReceiptRequest(request.Receipt);
            lock (gate)
            {
                var payloadKey = BuildFoodDeliveryReceiptPayloadKey(request.Receipt);
                if (appliedFoodReceiptCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                var snapshot = ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateFoodDeliveryReceiptDecisionRequest(request.Receipt),
                });
                appliedFoodReceiptCommands.Add(
                    request.CommandId,
                    new 적용된음식배달Command(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private Simulation음식배달PreviewSnapshot CreateFoodDeliveryPreview(
            Simulation음식배달PreviewRequest request)
        {
            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.음식배달);
            var blocks = new List<string>();
            if (foodDeliveries.ContainsKey(request.FoodOrderStableId.Trim()))
                blocks.Add("SimulationFoodDeliveryAlreadyExists");
            var sources = MergeSources(request.SourceStableIds, workflow.SourceStableIds);
            var preview = new Simulation음식배달PreviewSnapshot
            {
                FoodOrderStableId = request.FoodOrderStableId.Trim(),
                SuggestedStateCode = 음식배달상태코드.주문대기,
                TotalDurationTicks = request.PreparationDurationTicks
                    + request.DeliveryDurationTicks + 2,
                RuleRevision = workflow.RuleRevision,
                ExcludedOperationalEffectCodes = Copy(workflow.Simulation제외운영효과코드목록),
                BoundaryCodes = new[]
                {
                    "SimulationCandidateOnly",
                    "NoOperationalOrderWrite",
                    "NoRealDriverDispatch",
                    "NoPersonalAddress",
                    "NoPayment",
                    "ReceiptRequiresSeparateConfirmation",
                },
                BlockReasonCodes = blocks.ToArray(),
                SourceStableIds = sources,
            };
            preview.CommonDecisionPreview = CreateDecisionPreview(
                CreateFoodDeliveryDecisionRequest(request, preview));
            return preview;
        }

        private static SimulationDecisionPreviewRequest CreateFoodDeliveryDecisionRequest(
            Simulation음식배달PreviewRequest request,
            Simulation음식배달PreviewSnapshot preview)
        {
            var orderId = request.FoodOrderStableId.Trim();
            var unitCode = request.UnitCode.Trim();
            var sources = Copy(preview.SourceStableIds);
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:food-delivery-acceptance:" + orderId,
                DecisionTypeCode = Simulation음식배달DecisionTypeCodes.주문접수,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[]
                {
                    orderId,
                    request.MenuItemStableId.Trim(),
                    request.DeliveryScopeStableId.Trim(),
                    request.OrdererStableId.Trim(),
                },
                ExpectedEffects = new[]
                {
                    Projection(음식배달수량EffectCode, orderId, request.Quantity, unitCode, sources),
                    Projection(음식배달조리기간EffectCode, orderId,
                        request.PreparationDurationTicks, "tick", sources),
                    Projection(음식배달배송기간EffectCode, orderId,
                        request.DeliveryDurationTicks, "tick", sources),
                    Projection(음식배달목적시설EffectCode,
                        request.DestinationFacilityStableId.Trim(), 0m, "reference", sources),
                },
                Uncertainties = Copy(preview.BoundaryCodes),
                BlockReasonCodes = Copy(preview.BlockReasonCodes),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:food-delivery:" + orderId,
                    TaskTypeCode = "FoodDeliveryLifecycle",
                    FacilityStableId = request.RestaurantFacilityStableId.Trim(),
                    AssignedCapacity = request.Quantity,
                    AssignedCapacityUnitCode = unitCode,
                    DurationTicks = preview.TotalDurationTicks,
                    InputLotStableIds = new[] { request.MenuItemStableId.Trim() },
                    OutputCandidateCodes = new[] { 음식배달상태코드.전달완료 },
                    SourceStableIds = sources,
                },
            };
        }

        private SimulationDecisionPreviewRequest CreateFoodDeliveryReceiptDecisionRequest(
            Simulation음식배달수령PreviewRequest request)
        {
            if (!foodDeliveries.TryGetValue(request.FoodOrderStableId.Trim(), out var order))
                throw new SimulationNotFoundException("SimulationFoodDeliveryNotFound");
            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.음식배달);
            var blocks = new List<string>();
            if (order.Revision != request.FoodOrderRevision)
                blocks.Add("SimulationFoodDeliveryRevisionMismatch");
            if (order.StateCode != 음식배달상태코드.전달완료)
                blocks.Add("SimulationFoodDeliveryNotDelivered");
            if (!string.IsNullOrWhiteSpace(order.ReceiptTaskStableId))
                blocks.Add("SimulationFoodDeliveryReceiptAlreadyScheduled");
            var sources = MergeSources(MergeSources(request.SourceStableIds, order.SourceStableIds),
                workflow.SourceStableIds);
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:food-delivery-receipt:" + order.FoodOrderStableId,
                DecisionTypeCode = Simulation음식배달DecisionTypeCodes.수령확인,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { order.FoodOrderStableId, order.OrdererStableId },
                ExpectedEffects = new[]
                {
                    Projection(음식배달수령수량EffectCode, order.FoodOrderStableId,
                        order.Quantity, order.UnitCode, sources),
                },
                BlockReasonCodes = blocks.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:food-delivery-receipt:" + order.FoodOrderStableId,
                    TaskTypeCode = "FoodDeliveryReceiptConfirmation",
                    FacilityStableId = order.DestinationFacilityStableId,
                    AssignedCapacity = order.Quantity,
                    AssignedCapacityUnitCode = order.UnitCode,
                    DurationTicks = request.ReceiptDurationTicks,
                    InputLotStableIds = new[] { order.FoodOrderStableId },
                    OutputCandidateCodes = new[] { 음식배달상태코드.수령확인 },
                    SourceStableIds = sources,
                },
            };
        }

        private Simulation음식배달Snapshot? PrepareFoodDelivery(
            SimulationDecisionPreviewRequest request)
        {
            if (!string.Equals(request.DecisionTypeCode,
                Simulation음식배달DecisionTypeCodes.주문접수, StringComparison.Ordinal)) return null;
            var orderId = FindTarget(request.TargetStableIds, "food-order:",
                "SimulationFoodDeliveryStableIdInvalid");
            if (foodDeliveries.ContainsKey(orderId))
                throw new SimulationConflictException("SimulationFoodDeliveryAlreadyExists");
            var quantity = request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == 음식배달수량EffectCode);
            var preparation = request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == 음식배달조리기간EffectCode);
            var delivery = request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == 음식배달배송기간EffectCode);
            var destination = request.ExpectedEffects.Single(value =>
                value.ValueTypeCode == 음식배달목적시설EffectCode).TargetLedgerStableId;
            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.음식배달);
            return new Simulation음식배달Snapshot
            {
                FoodOrderStableId = orderId,
                MenuItemStableId = FindTarget(request.TargetStableIds, "menu-item:",
                    "SimulationFoodDeliveryMenuItemStableIdInvalid"),
                RestaurantFacilityStableId = request.Task.FacilityStableId,
                DestinationFacilityStableId = destination,
                DeliveryScopeStableId = FindTarget(request.TargetStableIds, "delivery-scope:",
                    "SimulationFoodDeliveryScopeStableIdInvalid"),
                OrdererStableId = FindTarget(request.TargetStableIds, "participant:",
                    "SimulationFoodDeliveryOrdererStableIdInvalid"),
                StateCode = 음식배달상태코드.주문대기,
                Revision = 1,
                Quantity = quantity.AfterValue,
                UnitCode = quantity.UnitCode,
                PreparationDurationTicks = decimal.ToInt32(preparation.AfterValue),
                DeliveryDurationTicks = decimal.ToInt32(delivery.AfterValue),
                TotalDurationTicks = request.Task.DurationTicks,
                AcceptedTick = CurrentTick,
                RuleRevision = workflow.RuleRevision,
                ExcludedOperationalEffectCodes = Copy(workflow.Simulation제외운영효과코드목록),
                SourceStableIds = MergeSources(request.SourceStableIds, workflow.SourceStableIds),
            };
        }

        private Simulation음식배달Snapshot? PrepareFoodDeliveryReceipt(
            SimulationDecisionPreviewRequest request)
        {
            if (!string.Equals(request.DecisionTypeCode,
                Simulation음식배달DecisionTypeCodes.수령확인, StringComparison.Ordinal)) return null;
            var orderId = FindTarget(request.TargetStableIds, "food-order:",
                "SimulationFoodDeliveryStableIdInvalid");
            if (!foodDeliveries.TryGetValue(orderId, out var order))
                throw new SimulationNotFoundException("SimulationFoodDeliveryNotFound");
            if (order.StateCode != 음식배달상태코드.전달완료)
                throw new SimulationConflictException("SimulationFoodDeliveryNotDelivered");
            if (!string.IsNullOrWhiteSpace(order.ReceiptTaskStableId))
                throw new SimulationConflictException("SimulationFoodDeliveryReceiptAlreadyScheduled");
            return order;
        }

        private void ScheduleFoodDelivery(
            Simulation음식배달Snapshot? order,
            SimulationDecisionSnapshot decision,
            SimulationTaskSnapshot task)
        {
            if (order == null) return;
            order.DecisionStableId = decision.DecisionStableId;
            order.TaskStableId = task.TaskStableId;
            foodDeliveries.Add(order.FoodOrderStableId, order);
        }

        private static void ScheduleFoodDeliveryReceipt(
            Simulation음식배달Snapshot? order,
            SimulationDecisionSnapshot decision,
            SimulationTaskSnapshot task)
        {
            if (order == null) return;
            order.ReceiptDecisionStableId = decision.DecisionStableId;
            order.ReceiptTaskStableId = task.TaskStableId;
            order.Revision++;
        }

        private void AdvanceFoodDeliveryForTask(SimulationTaskSnapshot task, int currentTick)
        {
            var order = foodDeliveries.Values.FirstOrDefault(value =>
                string.Equals(value.TaskStableId, task.TaskStableId, StringComparison.Ordinal));
            if (order != null)
            {
                var elapsed = currentTick - task.ScheduledStartTick + 1;
                if (elapsed >= 1 && order.CookingStartedTick == null)
                {
                    TransitionFoodDelivery(order, 음식배달상태코드.조리중, currentTick, task.TaskStableId);
                    order.CookingStartedTick ??= currentTick;
                }
                if (elapsed >= order.PreparationDurationTicks && order.ReadyForPickupTick == null)
                {
                    TransitionFoodDelivery(order, 음식배달상태코드.픽업대기, currentTick, task.TaskStableId);
                    order.ReadyForPickupTick ??= currentTick;
                }
                if (elapsed >= order.PreparationDurationTicks + 1 && order.DispatchCandidateTick == null)
                {
                    TransitionFoodDelivery(order, 음식배달상태코드.기사배정, currentTick, task.TaskStableId);
                    order.DispatchCandidateTick ??= currentTick;
                }
                if (elapsed >= order.PreparationDurationTicks + 2 && order.PickedUpTick == null)
                {
                    TransitionFoodDelivery(order, 음식배달상태코드.픽업완료, currentTick, task.TaskStableId);
                    order.PickedUpTick ??= currentTick;
                }
                if (elapsed >= order.TotalDurationTicks && order.DeliveredTick == null)
                {
                    TransitionFoodDelivery(order, 음식배달상태코드.전달완료, currentTick, task.TaskStableId);
                    order.DeliveredTick ??= currentTick;
                }
            }

            var receipt = foodDeliveries.Values.FirstOrDefault(value =>
                string.Equals(value.ReceiptTaskStableId, task.TaskStableId, StringComparison.Ordinal));
            if (receipt != null && currentTick >= task.ExpectedEndTick
                && receipt.StateCode != 음식배달상태코드.수령확인)
            {
                TransitionFoodDelivery(receipt, 음식배달상태코드.수령확인,
                    task.ExpectedEndTick, task.TaskStableId);
                receipt.ReceivedTick = task.ExpectedEndTick;
            }
        }

        private static void TransitionFoodDelivery(
            Simulation음식배달Snapshot order,
            string targetState,
            int worldTick,
            string causeStableId)
        {
            if (order.StateCode == targetState) return;
            var result = 업무상태전이Policy.판정(
                업무흐름코드.음식배달, order.StateCode, targetState);
            if (!result.허용여부)
                throw new SimulationConflictException("SimulationFoodDeliveryTransitionNotAllowed");
            var history = order.StateHistory.ToList();
            history.Add(new Simulation음식배달상태전이Snapshot
            {
                FromStateCode = order.StateCode,
                ToStateCode = targetState,
                WorldTick = worldTick,
                CauseStableId = causeStableId,
                RuleRevision = result.RuleRevision,
            });
            order.StateCode = targetState;
            order.StateHistory = history.ToArray();
            order.Revision++;
        }

        private Simulation음식배달Snapshot[] CreateFoodDeliverySnapshots()
            => foodDeliveries.Values.OrderBy(value => value.FoodOrderStableId, StringComparer.Ordinal)
                .Select(CloneFoodDelivery).ToArray();

        internal static Simulation음식배달Snapshot CloneFoodDelivery(
            Simulation음식배달Snapshot source)
            => new Simulation음식배달Snapshot
            {
                FoodOrderStableId = source.FoodOrderStableId,
                MenuItemStableId = source.MenuItemStableId,
                RestaurantFacilityStableId = source.RestaurantFacilityStableId,
                DestinationFacilityStableId = source.DestinationFacilityStableId,
                DeliveryScopeStableId = source.DeliveryScopeStableId,
                OrdererStableId = source.OrdererStableId,
                StateCode = source.StateCode,
                Revision = source.Revision,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                PreparationDurationTicks = source.PreparationDurationTicks,
                DeliveryDurationTicks = source.DeliveryDurationTicks,
                TotalDurationTicks = source.TotalDurationTicks,
                DecisionStableId = source.DecisionStableId,
                TaskStableId = source.TaskStableId,
                ReceiptDecisionStableId = source.ReceiptDecisionStableId,
                ReceiptTaskStableId = source.ReceiptTaskStableId,
                AcceptedTick = source.AcceptedTick,
                CookingStartedTick = source.CookingStartedTick,
                ReadyForPickupTick = source.ReadyForPickupTick,
                DispatchCandidateTick = source.DispatchCandidateTick,
                PickedUpTick = source.PickedUpTick,
                DeliveredTick = source.DeliveredTick,
                ReceivedTick = source.ReceivedTick,
                RuleRevision = source.RuleRevision,
                ExcludedOperationalEffectCodes = Copy(source.ExcludedOperationalEffectCodes),
                SourceStableIds = Copy(source.SourceStableIds),
                StateHistory = source.StateHistory.Select(value => new Simulation음식배달상태전이Snapshot
                {
                    FromStateCode = value.FromStateCode,
                    ToStateCode = value.ToStateCode,
                    WorldTick = value.WorldTick,
                    CauseStableId = value.CauseStableId,
                    RuleRevision = value.RuleRevision,
                }).ToArray(),
            };

        private static SimulationValueProjection Projection(
            string type, string target, decimal value, string unit, string[] sources)
            => new SimulationValueProjection
            {
                ValueTypeCode = type,
                TargetLedgerStableId = target,
                BeforeValue = 0m,
                Delta = value,
                AfterValue = value,
                UnitCode = unit,
                SourceStableIds = Copy(sources),
            };

        private static string FindTarget(string[] values, string prefix, string errorCode)
            => values.SingleOrDefault(value => value.StartsWith(prefix, StringComparison.Ordinal))
                ?? throw new SimulationContractException(errorCode);

        private static void ValidateFoodDeliveryRequest(Simulation음식배달PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequirePrefixedId(request.FoodOrderStableId, "food-order:", "SimulationFoodDeliveryStableIdInvalid");
            RequirePrefixedId(request.MenuItemStableId, "menu-item:", "SimulationFoodDeliveryMenuItemStableIdInvalid");
            RequireStableId(request.RestaurantFacilityStableId, "SimulationFoodDeliveryRestaurantFacilityStableIdInvalid");
            RequireStableId(request.DestinationFacilityStableId, "SimulationFoodDeliveryDestinationFacilityStableIdInvalid");
            RequirePrefixedId(request.DeliveryScopeStableId, "delivery-scope:", "SimulationFoodDeliveryScopeStableIdInvalid");
            RequirePrefixedId(request.OrdererStableId, "participant:", "SimulationFoodDeliveryOrdererStableIdInvalid");
            RequireStableId(request.ActorStableId, "SimulationActorStableIdInvalid");
            if (request.Quantity <= 0m) throw new SimulationContractException("SimulationFoodDeliveryQuantityInvalid");
            RequireText(request.UnitCode, "SimulationFoodDeliveryUnitCodeMissing");
            if (request.PreparationDurationTicks <= 0 || request.PreparationDurationTicks > 14)
                throw new SimulationContractException("SimulationFoodDeliveryPreparationDurationInvalid");
            if (request.DeliveryDurationTicks <= 0 || request.DeliveryDurationTicks > 14)
                throw new SimulationContractException("SimulationFoodDeliveryDurationInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationFoodDeliverySourceStableIdsInvalid");
        }

        private static void ValidateFoodDeliveryReceiptRequest(
            Simulation음식배달수령PreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequirePrefixedId(request.FoodOrderStableId, "food-order:", "SimulationFoodDeliveryStableIdInvalid");
            if (request.FoodOrderRevision <= 0)
                throw new SimulationContractException("SimulationFoodDeliveryRevisionInvalid");
            RequireStableId(request.ActorStableId, "SimulationActorStableIdInvalid");
            if (request.ReceiptDurationTicks <= 0 || request.ReceiptDurationTicks > 7)
                throw new SimulationContractException("SimulationFoodDeliveryReceiptDurationInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationFoodDeliveryReceiptSourceStableIdsInvalid");
        }

        private static void RequirePrefixedId(string value, string prefix, string errorCode)
        {
            RequireStableId(value, errorCode);
            if (!value.Trim().StartsWith(prefix, StringComparison.Ordinal))
                throw new SimulationContractException(errorCode);
        }

        private static string BuildFoodDeliveryPayloadKey(Simulation음식배달PreviewRequest value)
            => string.Join("\u001e", value.FoodOrderStableId.Trim(), value.MenuItemStableId.Trim(),
                value.RestaurantFacilityStableId.Trim(), value.DestinationFacilityStableId.Trim(),
                value.DeliveryScopeStableId.Trim(), value.OrdererStableId.Trim(), value.ActorStableId.Trim(),
                value.Quantity.ToString(CultureInfo.InvariantCulture), value.UnitCode.Trim(),
                value.PreparationDurationTicks.ToString(CultureInfo.InvariantCulture),
                value.DeliveryDurationTicks.ToString(CultureInfo.InvariantCulture),
                string.Join("\u001f", value.SourceStableIds.OrderBy(source => source, StringComparer.Ordinal)));

        private static string BuildFoodDeliveryReceiptPayloadKey(
            Simulation음식배달수령PreviewRequest value)
            => string.Join("\u001e", value.FoodOrderStableId.Trim(),
                value.FoodOrderRevision.ToString(CultureInfo.InvariantCulture), value.ActorStableId.Trim(),
                value.ReceiptDurationTicks.ToString(CultureInfo.InvariantCulture),
                string.Join("\u001f", value.SourceStableIds.OrderBy(source => source, StringComparer.Ordinal)));

        private sealed class 적용된음식배달Command
        {
            public 적용된음식배달Command(string payloadKey, 경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }
            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
