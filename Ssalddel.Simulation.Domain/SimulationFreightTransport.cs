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
        private const string 화물인수수량EffectCode = "FreightReceiptQuantity";
        private const string Simulation배차RuleRevision = "simulation-freight-dispatch.v1";
        private readonly Dictionary<string, SimulationFreightTransportSnapshot> freightTransports =
            new Dictionary<string, SimulationFreightTransportSnapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, 적용된FreightReceiptCommand> appliedFreightReceiptCommands =
            new Dictionary<string, 적용된FreightReceiptCommand>(StringComparer.Ordinal);

        public SimulationFreightTransportPreviewSnapshot PreviewFreightTransport(
            SimulationFreightTransportPreviewRequest request)
        {
            ValidateFreightTransportPreviewRequest(request);
            lock (gate)
            {
                return CreateFreightTransportPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmFreightTransport(
            SimulationFreightTransportConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateFreightTransportPreviewRequest(request.Freight);

            return ConfirmLogisticsMovement(new SimulationLogisticsMovementConfirmRequest
            {
                CommandId = request.CommandId,
                ExpectedRevision = request.ExpectedRevision,
                Movement = BindFreightTransport(request.Freight),
            });
        }

        public SimulationDecisionPreviewSnapshot PreviewFreightReceipt(
            SimulationFreightReceiptPreviewRequest request)
        {
            ValidateFreightReceiptRequest(request);
            lock (gate)
            {
                return CreateDecisionPreview(CreateFreightReceiptDecisionRequest(request));
            }
        }

        public 경영SimulationSessionSnapshot ConfirmFreightReceipt(
            SimulationFreightReceiptConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateFreightReceiptRequest(request.Receipt);

            lock (gate)
            {
                var payloadKey = BuildFreightReceiptPayloadKey(request.Receipt);
                if (appliedFreightReceiptCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }

                var snapshot = ConfirmDecision(new SimulationDecisionConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Preview = CreateFreightReceiptDecisionRequest(request.Receipt),
                });
                appliedFreightReceiptCommands.Add(
                    request.CommandId,
                    new 적용된FreightReceiptCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        private SimulationFreightTransportPreviewSnapshot CreateFreightTransportPreview(
            SimulationFreightTransportPreviewRequest request)
        {
            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.화물운송);
            var movement = BindFreightTransport(request);
            var logistics = CreateLogisticsMovementPreview(movement);
            return new SimulationFreightTransportPreviewSnapshot
            {
                TransportRequestStableId = request.Transport.TransportRequestStableId.Trim(),
                DispatchOfferStableId = request.Transport.DispatchOfferStableId.Trim(),
                RequestStateCode = 화물운송상태코드.배차대기,
                DispatchStateCode = 화물운송상태코드.배차확정,
                RuleRevision = workflow.RuleRevision,
                ExcludedOperationalEffectCodes = Copy(workflow.Simulation제외운영효과코드목록),
                BlockReasonCodes = Copy(logistics.CommonDecisionPreview.Decision.BlockReasonCodes),
                SourceStableIds = MergeSources(logistics.CommonDecisionPreview.Decision.SourceStableIds, workflow.SourceStableIds),
                LogisticsMovement = logistics,
            };
        }

        private static SimulationLogisticsMovementPreviewRequest BindFreightTransport(
            SimulationFreightTransportPreviewRequest request)
        {
            var source = request.Movement;
            return new SimulationLogisticsMovementPreviewRequest
            {
                CargoStableId = source.CargoStableId,
                CargoRevision = source.CargoRevision,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                RouteStableId = source.RouteStableId,
                OriginFacilityStableId = source.OriginFacilityStableId,
                DestinationFacilityStableId = source.DestinationFacilityStableId,
                ActorStableId = source.ActorStableId,
                PreferredOriginSpatialStableId = source.PreferredOriginSpatialStableId,
                PreferredRouteSpatialStableId = source.PreferredRouteSpatialStableId,
                PreferredDestinationSpatialStableId = source.PreferredDestinationSpatialStableId,
                RequiredRouteTicks = source.RequiredRouteTicks,
                FreightTransport = CloneFreightTransportBinding(request.Transport),
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
        }

        private SimulationFreightTransportSnapshot CreateFreightTransportSnapshot(
            SimulationLogisticsMovementPreviewRequest request,
            SimulationLogisticsMovementSnapshot movement)
        {
            var binding = request.FreightTransport
                ?? throw new SimulationContractException("SimulationFreightTransportBindingMissing");
            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.화물운송);
            var sources = MergeSources(movement.SourceStableIds, workflow.SourceStableIds);
            var snapshot = new SimulationFreightTransportSnapshot
            {
                TransportRequestStableId = binding.TransportRequestStableId.Trim(),
                DispatchOfferStableId = binding.DispatchOfferStableId.Trim(),
                RequestStateCode = 화물운송상태코드.배차대기,
                DispatchStateCode = 화물운송상태코드.배차확정,
                StateCode = 화물운송상태코드.배차확정,
                Revision = 1,
                CargoStableId = movement.CargoStableId,
                CarrierCandidateStableId = binding.CarrierCandidateStableId.Trim(),
                VehicleStableId = binding.VehicleStableId.Trim(),
                VehicleCapacity = binding.VehicleCapacity,
                VehicleCapacityUnitCode = binding.VehicleCapacityUnitCode.Trim(),
                Quantity = movement.Quantity,
                UnitCode = movement.UnitCode,
                LogisticsTaskStableId = movement.TaskStableId,
                RequestedTick = CurrentTick,
                DispatchedTick = CurrentTick,
                RuleRevision = workflow.RuleRevision,
                DispatchDecision = CloneFreightDispatchDecision(binding.DispatchDecision),
                ExcludedOperationalEffectCodes = Copy(workflow.Simulation제외운영효과코드목록),
                SourceStableIds = sources,
                StateHistory = new[]
                {
                    Transition(string.Empty, 화물운송상태코드.배차대기, CurrentTick,
                        binding.TransportRequestStableId, Simulation배차RuleRevision),
                    Transition(화물운송상태코드.배차대기, 화물운송상태코드.매칭중, CurrentTick,
                        binding.DispatchOfferStableId, Simulation배차RuleRevision),
                    Transition(화물운송상태코드.매칭중, 화물운송상태코드.배차확정, CurrentTick,
                        binding.CarrierCandidateStableId, Simulation배차RuleRevision),
                },
            };
            return snapshot;
        }

        private void AdvanceFreightTransportForMovement(
            SimulationLogisticsMovementSnapshot movement,
            int currentTick)
        {
            var freight = freightTransports.Values.FirstOrDefault(value =>
                string.Equals(value.CargoStableId, movement.CargoStableId, StringComparison.Ordinal));
            if (freight == null || freight.StateCode == 화물운송상태코드.인수완료) return;

            if (movement.StateCode == SimulationLogisticsMovementStateCodes.InTransit
                || movement.StateCode == SimulationLogisticsMovementStateCodes.ArrivedAtDestination)
            {
                if (freight.StateCode == 화물운송상태코드.배차확정)
                {
                    ApplyWorkflowTransition(freight, 화물운송상태코드.상차지도착, currentTick, movement.TaskStableId);
                    ApplyWorkflowTransition(freight, 화물운송상태코드.상차완료, currentTick, movement.TaskStableId);
                    ApplySimulationTransition(freight, 화물운송상태코드.운송중, currentTick, movement.TaskStableId);
                    freight.PickedUpTick = currentTick;
                }
            }

            if (movement.StateCode == SimulationLogisticsMovementStateCodes.ArrivedAtDestination
                && freight.StateCode == 화물운송상태코드.운송중)
            {
                ApplyWorkflowTransition(freight, 화물운송상태코드.하차지도착, currentTick, movement.TaskStableId);
                freight.ArrivedAtDropoffTick = currentTick;
                RegisterHubCargoBacklogIncident(freight.CargoStableId,
                    movement.DestinationFacilityStableId, currentTick);
            }
        }

        private SimulationDecisionPreviewRequest CreateFreightReceiptDecisionRequest(
            SimulationFreightReceiptPreviewRequest request)
        {
            if (!freightTransports.TryGetValue(request.TransportRequestStableId.Trim(), out var freight))
                throw new SimulationNotFoundException("SimulationFreightTransportNotFound");
            var movement = logisticsMovements[freight.CargoStableId];
            var workflow = 업무흐름규칙Catalog.조회(업무흐름코드.화물운송);
            var blocks = new List<string>();
            if (freight.Revision != request.TransportRevision)
                blocks.Add("SimulationFreightTransportRevisionMismatch");
            if (freight.StateCode != 화물운송상태코드.하차지도착)
                blocks.Add("SimulationFreightTransportNotAtDropoff");
            if (!string.IsNullOrWhiteSpace(freight.ReceiptTaskStableId))
                blocks.Add("SimulationFreightReceiptAlreadyScheduled");
            var sources = MergeSources(
                MergeSources(request.SourceStableIds, freight.SourceStableIds),
                workflow.SourceStableIds);
            return new SimulationDecisionPreviewRequest
            {
                DecisionStableId = "decision:freight-receipt:" + freight.TransportRequestStableId,
                DecisionTypeCode = SimulationFreightTransportDecisionTypeCodes.FreightReceipt,
                ActorStableId = request.ActorStableId.Trim(),
                TargetStableIds = new[] { freight.TransportRequestStableId, freight.CargoStableId },
                ExpectedEffects = new[]
                {
                    new SimulationValueProjection
                    {
                        ValueTypeCode = 화물인수수량EffectCode,
                        TargetLedgerStableId = freight.TransportRequestStableId,
                        BeforeValue = 0m,
                        Delta = freight.Quantity,
                        AfterValue = freight.Quantity,
                        UnitCode = freight.UnitCode,
                        SourceStableIds = sources,
                    },
                },
                BlockReasonCodes = blocks.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = sources,
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = "task:freight-receipt:" + freight.TransportRequestStableId,
                    TaskTypeCode = "FreightReceiptConfirmation",
                    FacilityStableId = movement.DestinationFacilityStableId,
                    ActionCode = SimulationNpcActionCodes.WarehouseInboundInspection,
                    PreferredSpatialStableId = request.PreferredSpatialStableId.Trim(),
                    AssignedCapacity = freight.Quantity,
                    AssignedCapacityUnitCode = freight.UnitCode,
                    DurationTicks = request.ReceiptDurationTicks,
                    InputLotStableIds = new[] { freight.CargoStableId },
                    OutputCandidateCodes = new[] { 화물운송상태코드.인수완료 },
                    SourceStableIds = sources,
                },
            };
        }

        private SimulationFreightTransportSnapshot? PrepareFreightReceipt(
            SimulationDecisionPreviewRequest request)
        {
            if (!string.Equals(request.DecisionTypeCode,
                SimulationFreightTransportDecisionTypeCodes.FreightReceipt,
                StringComparison.Ordinal)) return null;
            var transportId = request.TargetStableIds.SingleOrDefault(value =>
                value.StartsWith("freight-transport:", StringComparison.Ordinal))
                ?? throw new SimulationContractException("SimulationFreightTransportStableIdInvalid");
            if (!freightTransports.TryGetValue(transportId, out var freight))
                throw new SimulationNotFoundException("SimulationFreightTransportNotFound");
            if (freight.StateCode != 화물운송상태코드.하차지도착)
                throw new SimulationConflictException("SimulationFreightTransportNotAtDropoff");
            if (!string.IsNullOrWhiteSpace(freight.ReceiptTaskStableId))
                throw new SimulationConflictException("SimulationFreightReceiptAlreadyScheduled");
            return freight;
        }

        private static void ScheduleFreightReceipt(
            SimulationFreightTransportSnapshot? freight,
            SimulationDecisionSnapshot decision,
            SimulationTaskSnapshot task)
        {
            if (freight == null) return;
            freight.ReceiptDecisionStableId = decision.DecisionStableId;
            freight.ReceiptTaskStableId = task.TaskStableId;
            freight.Revision++;
        }

        private void ApplyFreightReceiptForTask(SimulationTaskSnapshot task, int completedTick)
        {
            var freight = freightTransports.Values.FirstOrDefault(value =>
                string.Equals(value.ReceiptTaskStableId, task.TaskStableId, StringComparison.Ordinal));
            if (freight == null || freight.StateCode == 화물운송상태코드.인수완료) return;
            ApplyWorkflowTransition(freight, 화물운송상태코드.인수완료, completedTick, task.TaskStableId);
            freight.ReceivedTick = completedTick;
        }

        private static void ApplyWorkflowTransition(
            SimulationFreightTransportSnapshot freight,
            string targetState,
            int worldTick,
            string causeStableId)
        {
            var result = 업무상태전이Policy.판정(
                업무흐름코드.화물운송,
                freight.StateCode,
                targetState);
            if (!result.허용여부)
                throw new SimulationConflictException("SimulationFreightTransportTransitionNotAllowed");
            ApplyTransition(freight, targetState, worldTick, causeStableId, result.RuleRevision);
        }

        private static void ApplySimulationTransition(
            SimulationFreightTransportSnapshot freight,
            string targetState,
            int worldTick,
            string causeStableId)
            => ApplyTransition(freight, targetState, worldTick, causeStableId, "simulation-logistics-movement.v1");

        private static void ApplyTransition(
            SimulationFreightTransportSnapshot freight,
            string targetState,
            int worldTick,
            string causeStableId,
            string ruleRevision)
        {
            if (string.Equals(freight.StateCode, targetState, StringComparison.Ordinal)) return;
            var history = freight.StateHistory.ToList();
            history.Add(Transition(freight.StateCode, targetState, worldTick, causeStableId, ruleRevision));
            freight.StateCode = targetState;
            freight.StateHistory = history.ToArray();
            freight.Revision++;
        }

        private static SimulationFreightTransportTransitionSnapshot Transition(
            string from,
            string to,
            int worldTick,
            string causeStableId,
            string ruleRevision)
            => new SimulationFreightTransportTransitionSnapshot
            {
                FromStateCode = from,
                ToStateCode = to,
                WorldTick = worldTick,
                CauseStableId = causeStableId.Trim(),
                RuleRevision = ruleRevision,
            };

        private SimulationFreightTransportSnapshot[] CreateFreightTransportSnapshots()
            => freightTransports.Values
                .OrderBy(value => value.TransportRequestStableId, StringComparer.Ordinal)
                .Select(CloneFreightTransport)
                .ToArray();

        internal static SimulationFreightTransportSnapshot CloneFreightTransport(
            SimulationFreightTransportSnapshot source)
            => new SimulationFreightTransportSnapshot
            {
                TransportRequestStableId = source.TransportRequestStableId,
                DispatchOfferStableId = source.DispatchOfferStableId,
                RequestStateCode = source.RequestStateCode,
                DispatchStateCode = source.DispatchStateCode,
                StateCode = source.StateCode,
                Revision = source.Revision,
                CargoStableId = source.CargoStableId,
                CarrierCandidateStableId = source.CarrierCandidateStableId,
                VehicleStableId = source.VehicleStableId,
                VehicleCapacity = source.VehicleCapacity,
                VehicleCapacityUnitCode = source.VehicleCapacityUnitCode,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                LogisticsTaskStableId = source.LogisticsTaskStableId,
                ReceiptDecisionStableId = source.ReceiptDecisionStableId,
                ReceiptTaskStableId = source.ReceiptTaskStableId,
                RequestedTick = source.RequestedTick,
                DispatchedTick = source.DispatchedTick,
                PickedUpTick = source.PickedUpTick,
                ArrivedAtDropoffTick = source.ArrivedAtDropoffTick,
                ReceivedTick = source.ReceivedTick,
                RuleRevision = source.RuleRevision,
                DispatchDecision = CloneFreightDispatchDecision(source.DispatchDecision),
                ExcludedOperationalEffectCodes = Copy(source.ExcludedOperationalEffectCodes),
                SourceStableIds = Copy(source.SourceStableIds),
                StateHistory = source.StateHistory.Select(value => Transition(
                    value.FromStateCode,
                    value.ToStateCode,
                    value.WorldTick,
                    value.CauseStableId,
                    value.RuleRevision)).ToArray(),
            };

        internal static SimulationFreightTransportBindingRequest? CloneFreightTransportBinding(
            SimulationFreightTransportBindingRequest? source)
            => source == null ? null : new SimulationFreightTransportBindingRequest
            {
                TransportRequestStableId = source.TransportRequestStableId,
                DispatchOfferStableId = source.DispatchOfferStableId,
                CarrierCandidateStableId = source.CarrierCandidateStableId,
                VehicleStableId = source.VehicleStableId,
                VehicleCapacity = source.VehicleCapacity,
                VehicleCapacityUnitCode = source.VehicleCapacityUnitCode,
                DispatchDecision = CloneFreightDispatchDecision(source.DispatchDecision),
            };

        private static void ValidateFreightTransportPreviewRequest(
            SimulationFreightTransportPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Transport == null)
                throw new SimulationContractException("SimulationFreightTransportBindingMissing");
            if (request.Movement == null)
                throw new SimulationContractException("SimulationLogisticsMovementMissing");
            ValidateFreightTransportBinding(request.Transport, request.Movement.UnitCode);
            ValidateLogisticsMovementRequest(request.Movement);
        }

        private static void ValidateFreightTransportBinding(
            SimulationFreightTransportBindingRequest request,
            string unitCode)
        {
            RequireStableId(request.TransportRequestStableId, "SimulationFreightTransportStableIdInvalid");
            RequireStableId(request.DispatchOfferStableId, "SimulationFreightDispatchOfferStableIdInvalid");
            RequireStableId(request.CarrierCandidateStableId, "SimulationFreightCarrierCandidateStableIdInvalid");
            RequireStableId(request.VehicleStableId, "SimulationFreightVehicleStableIdInvalid");
            if (request.VehicleCapacity <= 0m)
                throw new SimulationContractException("SimulationFreightVehicleCapacityInvalid");
            RequireText(request.VehicleCapacityUnitCode, "SimulationFreightVehicleCapacityUnitCodeMissing");
            if (!string.Equals(request.VehicleCapacityUnitCode.Trim(), unitCode.Trim(), StringComparison.Ordinal))
                throw new SimulationContractException("SimulationFreightVehicleCapacityUnitMismatch");
            ValidateFreightDispatchDecision(request);
        }

        private static void ValidateFreightDispatchDecision(
            SimulationFreightTransportBindingRequest binding)
        {
            var decision = binding.DispatchDecision;
            if (decision == null) return;
            RequireStableId(decision.DispatchOfferStableId,
                "SimulationFreightDispatchOfferStableIdInvalid");
            RequireStableId(decision.TransportRequestStableId,
                "SimulationFreightTransportStableIdInvalid");
            RequireText(decision.RuleRevision, "SimulationFreightDispatchRuleRevisionMissing");
            if (!string.Equals(decision.DispatchOfferStableId, binding.DispatchOfferStableId,
                    StringComparison.Ordinal)
                || !string.Equals(decision.TransportRequestStableId,
                    binding.TransportRequestStableId, StringComparison.Ordinal)
                || !string.Equals(decision.SelectedCarrierCandidateStableId,
                    binding.CarrierCandidateStableId, StringComparison.Ordinal)
                || !string.Equals(decision.SelectedVehicleStableId,
                    binding.VehicleStableId, StringComparison.Ordinal))
                throw new SimulationContractException("SimulationFreightDispatchDecisionBindingMismatch");
            ValidateIds(decision.SourceStableIds, true,
                "SimulationFreightDispatchSourceStableIdsInvalid");
            if (decision.CandidateEvaluations == null || decision.CandidateEvaluations.Length == 0)
                throw new SimulationContractException("SimulationFreightDispatchCandidateEvaluationsMissing");
            var selected = decision.CandidateEvaluations.Where(value => value.IsSelected).ToArray();
            if (selected.Length != 1
                || !selected[0].IsEligible
                || !string.Equals(selected[0].CarrierCandidateStableId,
                    binding.CarrierCandidateStableId, StringComparison.Ordinal)
                || !string.Equals(selected[0].VehicleStableId,
                    binding.VehicleStableId, StringComparison.Ordinal))
                throw new SimulationContractException("SimulationFreightDispatchSelectionInvalid");
            foreach (var candidate in decision.CandidateEvaluations)
            {
                RequireStableId(candidate.CarrierCandidateStableId,
                    "SimulationFreightCarrierCandidateStableIdInvalid");
                RequireStableId(candidate.VehicleStableId,
                    "SimulationFreightVehicleStableIdInvalid");
                RequireText(candidate.VehicleCapacityUnitCode,
                    "SimulationFreightVehicleCapacityUnitCodeMissing");
                ValidateIds(candidate.BlockReasonCodes, false,
                    "SimulationFreightDispatchBlockReasonCodesInvalid");
                if (candidate.Score == null)
                    throw new SimulationContractException("SimulationFreightDispatchScoreMissing");
            }
        }

        private static void ValidateFreightReceiptRequest(SimulationFreightReceiptPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.TransportRequestStableId, "SimulationFreightTransportStableIdInvalid");
            if (request.TransportRevision <= 0)
                throw new SimulationContractException("SimulationFreightTransportRevisionInvalid");
            RequireStableId(request.ActorStableId, "SimulationActorStableIdInvalid");
            if (!string.IsNullOrWhiteSpace(request.PreferredSpatialStableId))
                RequireStableId(request.PreferredSpatialStableId,
                    "SimulationPreferredSpatialStableIdInvalid");
            if (request.ReceiptDurationTicks <= 0 || request.ReceiptDurationTicks > 7)
                throw new SimulationContractException("SimulationFreightReceiptDurationInvalid");
            ValidateIds(request.SourceStableIds, true, "SimulationFreightReceiptSourceStableIdsInvalid");
        }

        private static string BuildFreightReceiptPayloadKey(SimulationFreightReceiptPreviewRequest request)
            => string.Join("\u001e", new[]
            {
                request.TransportRequestStableId.Trim(),
                request.TransportRevision.ToString(CultureInfo.InvariantCulture),
                request.ActorStableId.Trim(),
                request.PreferredSpatialStableId.Trim(),
                request.ReceiptDurationTicks.ToString(CultureInfo.InvariantCulture),
                string.Join("\u001f", request.SourceStableIds.Select(value => value.Trim())
                    .OrderBy(value => value, StringComparer.Ordinal)),
            });

        private sealed class 적용된FreightReceiptCommand
        {
            public 적용된FreightReceiptCommand(string payloadKey, 경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
