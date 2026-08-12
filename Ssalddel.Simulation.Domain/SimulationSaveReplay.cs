using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly List<SimulationCommandLogEntrySnapshot> commandLog =
            new List<SimulationCommandLogEntrySnapshot>();

        public SimulationSessionSavePackage CreateSavePackage(SimulationSessionSaveRequest request)
        {
            ValidateSaveRequest(request);
            lock (gate)
            {
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");

                var package = new SimulationSessionSavePackage
                {
                    SchemaVersion = SimulationSaveSchemaVersions.V1,
                    SaveStableId = request.SaveStableId.Trim(),
                    SessionStableId = SessionStableId,
                    SavedWorldTick = CurrentTick,
                    SavedWorldRevision = Revision,
                    ReplayHashAlgorithmCode = SimulationReplayHashAlgorithmCodes.Sha256,
                    SessionCreateRequest = CreateSessionRequest(),
                    Snapshot = CreateSnapshot(),
                    CommandLog = commandLog.Select(SimulationSaveReplayCloner.CloneCommand).ToArray(),
                };
                package.ReplayHash = SimulationReplayHasher.Calculate(package);
                return SimulationSaveReplayCloner.ClonePackage(package);
            }
        }

        private void AppendTickCommand(경영SimulationTick진행Request request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TickAdvance,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TickRequest = SimulationSaveReplayCloner.CloneTickRequest(request),
            });

        private void AppendDecisionConfirmCommand(SimulationDecisionConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.DecisionConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                DecisionConfirmRequest = SimulationSaveReplayCloner.CloneConfirmRequest(request),
            });

        private void AppendHarvestDispositionImpactConfirmCommand(
            SimulationHarvestDispositionImpactConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.HarvestDispositionImpactConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                HarvestDispositionImpactConfirmRequest =
                    SimulationSaveReplayCloner.CloneHarvestDispositionImpactConfirmRequest(request),
            });

        private void AppendLogisticsMovementConfirmCommand(
            SimulationLogisticsMovementConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.LogisticsMovementConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                LogisticsMovementConfirmRequest =
                    SimulationSaveReplayCloner.CloneLogisticsMovementConfirmRequest(request),
            });

        private void AppendTurnClosingCommand(SimulationTurnClosingConfirmRequest request)
            => commandLog.Add(new SimulationCommandLogEntrySnapshot
            {
                Sequence = commandLog.Count + 1L,
                CommandTypeCode = SimulationCommandTypeCodes.TurnClosingConfirm,
                AppliedWorldTick = CurrentTick,
                ResultingWorldRevision = Revision,
                TurnClosingConfirmRequest =
                    SimulationSaveReplayCloner.CloneTurnClosingConfirmRequest(request),
            });

        private 경영SimulationSession생성Request CreateSessionRequest()
            => new 경영SimulationSession생성Request
            {
                ClientRequestId = ClientRequestId,
                ScenarioStableId = ScenarioStableId,
                ScenarioDataRevision = ScenarioDataRevision,
                ScenarioSeed = ScenarioSeed,
                RuleRevision = RuleRevision,
                DurationTicks = DurationTicks,
                WorldContext = new SimulationWorldContext생성Request
                {
                    FactionStableId = FactionStableId,
                    TerritoryStableId = TerritoryStableId,
                    SettlementStableId = SettlementStableId,
                    GameDateStartsOn = GameDateStartsOn,
                },
                Settlement = CloneSettlementRequest(settlementCreationState),
            };

        private static void ValidateSaveRequest(SimulationSessionSaveRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.SaveStableId, "SimulationSaveStableIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
        }
    }

    public static class SimulationSessionReplay
    {
        public static 경영SimulationSessionAggregate Restore(SimulationSessionSavePackage package)
        {
            ValidatePackage(package);
            var aggregate = new 경영SimulationSessionAggregate(
                SimulationSaveReplayCloner.CloneCreateRequest(package.SessionCreateRequest));
            if (!string.Equals(aggregate.SessionStableId, package.SessionStableId, StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationSaveSessionIdentityMismatch");

            for (var index = 0; index < package.CommandLog.Length; index++)
            {
                var entry = package.CommandLog[index];
                if (entry.Sequence != index + 1L)
                    throw new SimulationConflictException("SimulationCommandLogSequenceInvalid");

                if (entry.CommandTypeCode == SimulationCommandTypeCodes.DecisionConfirm)
                {
                    if (entry.DecisionConfirmRequest == null || entry.TickRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.ConfirmDecision(
                        SimulationSaveReplayCloner.CloneConfirmRequest(entry.DecisionConfirmRequest));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.HarvestDispositionImpactConfirm)
                {
                    if (entry.HarvestDispositionImpactConfirmRequest == null
                        || entry.TickRequest != null || entry.DecisionConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.ConfirmHarvestDispositionImpact(
                        SimulationSaveReplayCloner.CloneHarvestDispositionImpactConfirmRequest(
                            entry.HarvestDispositionImpactConfirmRequest));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.LogisticsMovementConfirm)
                {
                    if (entry.LogisticsMovementConfirmRequest == null
                        || entry.TickRequest != null || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.ConfirmLogisticsMovement(
                        SimulationSaveReplayCloner.CloneLogisticsMovementConfirmRequest(
                            entry.LogisticsMovementConfirmRequest));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TurnClosingConfirm)
                {
                    if (entry.TurnClosingConfirmRequest == null || entry.TickRequest != null
                        || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.ConfirmTurnClosing(
                        SimulationSaveReplayCloner.CloneTurnClosingConfirmRequest(
                            entry.TurnClosingConfirmRequest));
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TickAdvance)
                {
                    if (entry.TickRequest == null || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    aggregate.Advance(
                        SimulationSaveReplayCloner.CloneTickRequest(entry.TickRequest));
                }
                else
                {
                    throw new SimulationConflictException("SimulationCommandTypeUnsupported");
                }

                var current = aggregate.Snapshot();
                if (current.CurrentTick != entry.AppliedWorldTick
                    || current.Revision != entry.ResultingWorldRevision)
                {
                    throw new SimulationConflictException("SimulationCommandReplayResultMismatch");
                }
            }

            var replayed = aggregate.CreateSavePackage(new SimulationSessionSaveRequest
            {
                SaveStableId = package.SaveStableId,
                ExpectedRevision = aggregate.Revision,
            });
            if (!string.Equals(replayed.ReplayHash, package.ReplayHash, StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationReplayHashMismatch");
            if (replayed.SavedWorldTick != package.SavedWorldTick
                || replayed.SavedWorldRevision != package.SavedWorldRevision)
            {
                throw new SimulationConflictException("SimulationSavePositionMismatch");
            }

            return aggregate;
        }

        private static void ValidatePackage(SimulationSessionSavePackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (!string.Equals(package.SchemaVersion, SimulationSaveSchemaVersions.V1, StringComparison.Ordinal))
                throw new SimulationContractException("SimulationSaveSchemaUnsupported");
            if (!string.Equals(
                package.ReplayHashAlgorithmCode,
                SimulationReplayHashAlgorithmCodes.Sha256,
                StringComparison.Ordinal))
            {
                throw new SimulationContractException("SimulationReplayHashAlgorithmUnsupported");
            }
            if (string.IsNullOrWhiteSpace(package.SaveStableId)
                || string.IsNullOrWhiteSpace(package.SessionStableId)
                || string.IsNullOrWhiteSpace(package.ReplayHash)
                || package.SessionCreateRequest == null
                || package.SessionCreateRequest.WorldContext == null
                || package.Snapshot == null
                || package.Snapshot.WorldContext == null
                || package.Snapshot.Decisions == null
                || package.Snapshot.Tasks == null
                || package.Snapshot.Effects == null
                || package.Snapshot.LogisticsMovements == null
                || package.Snapshot.FreightTransports == null
                || package.Snapshot.GroupOrders == null
                || package.Snapshot.FoodDeliveries == null
                || package.Snapshot.MarketConsumptions == null
                || package.Snapshot.IndividualOrders == null
                || package.Snapshot.StockReservations == null
                || package.Snapshot.ExportPreparations == null
                || package.Snapshot.ExportCargoPreparations == null
                || package.Snapshot.ExportCargoHandoffs == null
                || package.Snapshot.ExportPortReceipts == null
                || package.Snapshot.ExportReadinessReviews == null
                || package.Snapshot.ExportShipmentPlans == null
                || package.Snapshot.ExportShipmentExecutions == null
                || package.CommandLog == null)
            {
                throw new SimulationContractException("SimulationSavePackageInvalid");
            }
            if (package.Snapshot.Settlement != null
                && (package.Snapshot.Settlement.Districts == null
                    || package.Snapshot.Settlement.Facilities == null
                    || package.Snapshot.Settlement.MarketSupplyByProduct == null
                    || package.Snapshot.Settlement.ResidentConsumptionByProduct == null
                    || package.Snapshot.Settlement.ReserveStockLots == null
                    || package.Snapshot.Settlement.HarvestLotAllocations == null
                    || package.Snapshot.Settlement.ActiveTaskStableIds == null
                    || package.Snapshot.Settlement.SourceStableIds == null))
            {
                throw new SimulationContractException("SimulationSavePackageInvalid");
            }

            for (var index = 0; index < package.CommandLog.Length; index++)
            {
                var entry = package.CommandLog[index];
                if (entry == null || entry.Sequence != index + 1L)
                    throw new SimulationConflictException("SimulationCommandLogSequenceInvalid");
                if (entry.CommandTypeCode == SimulationCommandTypeCodes.DecisionConfirm)
                {
                    if (entry.DecisionConfirmRequest == null || entry.TickRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateDecisionConfirm(
                        entry.DecisionConfirmRequest);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.HarvestDispositionImpactConfirm)
                {
                    if (entry.HarvestDispositionImpactConfirmRequest == null
                        || entry.TickRequest != null || entry.DecisionConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateHarvestDispositionImpactConfirmRequestForReplay(
                        entry.HarvestDispositionImpactConfirmRequest);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.LogisticsMovementConfirm)
                {
                    if (entry.LogisticsMovementConfirmRequest == null
                        || entry.TickRequest != null || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateLogisticsMovementConfirmRequestForReplay(
                        entry.LogisticsMovementConfirmRequest);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TurnClosingConfirm)
                {
                    if (entry.TurnClosingConfirmRequest == null || entry.TickRequest != null
                        || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateTurnClosingConfirmRequest(
                        entry.TurnClosingConfirmRequest);
                }
                else if (entry.CommandTypeCode == SimulationCommandTypeCodes.TickAdvance)
                {
                    if (entry.TickRequest == null || entry.DecisionConfirmRequest != null
                        || entry.HarvestDispositionImpactConfirmRequest != null
                        || entry.LogisticsMovementConfirmRequest != null
                        || entry.TurnClosingConfirmRequest != null)
                        throw new SimulationConflictException("SimulationCommandLogPayloadInvalid");
                    경영SimulationSessionAggregate.ValidateAdvance(entry.TickRequest);
                }
                else
                {
                    throw new SimulationConflictException("SimulationCommandTypeUnsupported");
                }
            }

            경영SimulationSessionAggregate.ValidateCreate(package.SessionCreateRequest);
            string packageHash;
            try
            {
                packageHash = SimulationReplayHasher.Calculate(package);
            }
            catch (Exception error) when (
                error is NullReferenceException
                || error is ArgumentException
                || error is InvalidOperationException)
            {
                throw new SimulationContractException("SimulationSavePackageInvalid");
            }
            if (!string.Equals(packageHash, package.ReplayHash, StringComparison.Ordinal))
                throw new SimulationConflictException("SimulationReplayHashMismatch");
        }
    }

    internal static class SimulationReplayHasher
    {
        public static string Calculate(SimulationSessionSavePackage package)
        {
            var canonical = new StringBuilder();
            Add(canonical, package.SchemaVersion);
            AddCreateRequest(canonical, package.SessionCreateRequest);
            AddSnapshot(canonical, package.Snapshot);
            Add(canonical, package.CommandLog.Length);
            foreach (var entry in package.CommandLog.OrderBy(value => value.Sequence))
            {
                Add(canonical, entry.Sequence);
                Add(canonical, entry.CommandTypeCode);
                Add(canonical, entry.AppliedWorldTick);
                Add(canonical, entry.ResultingWorldRevision);
                if (entry.TickRequest != null)
                {
                    Add(canonical, entry.TickRequest.CommandId);
                    Add(canonical, entry.TickRequest.ExpectedRevision);
                    Add(canonical, entry.TickRequest.TickCount);
                }
                if (entry.DecisionConfirmRequest != null)
                {
                    Add(canonical, entry.DecisionConfirmRequest.CommandId);
                    Add(canonical, entry.DecisionConfirmRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildDecisionPayloadKey(
                        entry.DecisionConfirmRequest.Preview));
                }
                if (entry.HarvestDispositionImpactConfirmRequest != null)
                {
                    Add(canonical, entry.HarvestDispositionImpactConfirmRequest.CommandId);
                    Add(canonical, entry.HarvestDispositionImpactConfirmRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildHarvestDispositionImpactPayloadKey(
                        entry.HarvestDispositionImpactConfirmRequest.Impact));
                }
                if (entry.LogisticsMovementConfirmRequest != null)
                {
                    Add(canonical, entry.LogisticsMovementConfirmRequest.CommandId);
                    Add(canonical, entry.LogisticsMovementConfirmRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildLogisticsMovementPayloadKey(
                        entry.LogisticsMovementConfirmRequest.Movement));
                }
                if (entry.TurnClosingConfirmRequest != null)
                {
                    Add(canonical, entry.TurnClosingConfirmRequest.CommandId);
                    Add(canonical, entry.TurnClosingConfirmRequest.ExpectedRevision);
                    Add(canonical, 경영SimulationSessionAggregate.BuildTurnClosingPayloadKey(
                        entry.TurnClosingConfirmRequest.Preview));
                }
            }

            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(canonical.ToString());
                return BitConverter.ToString(sha.ComputeHash(bytes))
                    .Replace("-", string.Empty)
                    .ToLowerInvariant();
            }
        }

        private static void AddCreateRequest(
            StringBuilder target,
            경영SimulationSession생성Request request)
        {
            Add(target, request.ClientRequestId.ToString("N"));
            Add(target, request.ScenarioStableId);
            Add(target, request.ScenarioDataRevision);
            Add(target, request.ScenarioSeed);
            Add(target, request.RuleRevision);
            Add(target, request.DurationTicks);
            Add(target, request.WorldContext.FactionStableId);
            Add(target, request.WorldContext.TerritoryStableId);
            Add(target, request.WorldContext.SettlementStableId);
            Add(target, request.WorldContext.GameDateStartsOn.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            Add(target, 경영SimulationSessionAggregate.BuildSettlementPayloadKey(request.Settlement));
        }

        private static void AddSnapshot(StringBuilder target, 경영SimulationSessionSnapshot value)
        {
            Add(target, value.SessionStableId);
            Add(target, value.ClientRequestId.ToString("N"));
            Add(target, value.ScenarioStableId);
            Add(target, value.ScenarioDataRevision);
            Add(target, value.ScenarioSeed);
            Add(target, value.RuleRevision);
            Add(target, value.CurrentTick);
            Add(target, value.DurationTicks);
            Add(target, value.Revision);
            Add(target, value.IsCompleted);
            Add(target, value.ModeCode);
            Add(target, value.IsOperationalState);
            Add(target, value.WorldContext.FactionStableId);
            Add(target, value.WorldContext.TerritoryStableId);
            Add(target, value.WorldContext.SettlementStableId);
            Add(target, value.WorldContext.WorldTick);
            Add(target, value.WorldContext.WorldRevision);
            Add(target, value.WorldContext.GameDateStartsOn.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            Add(target, value.WorldContext.GameDate.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            Add(target, value.WorldContext.CalendarRuleCode);
            Add(target, value.Decisions.Length);
            foreach (var decision in value.Decisions)
                AddDecision(target, decision);
            Add(target, value.Tasks.Length);
            foreach (var task in value.Tasks)
                AddTask(target, task);
            Add(target, value.Effects.Length);
            foreach (var effect in value.Effects)
                AddEffect(target, effect);
            Add(target, value.LogisticsMovements.Length);
            foreach (var movement in value.LogisticsMovements)
                AddLogisticsMovement(target, movement);
            Add(target, value.FreightTransports.Length);
            foreach (var freight in value.FreightTransports)
                AddFreightTransport(target, freight);
            Add(target, value.GroupOrders.Length);
            foreach (var groupOrder in value.GroupOrders)
                AddGroupOrder(target, groupOrder);
            Add(target, value.FoodDeliveries.Length);
            foreach (var foodDelivery in value.FoodDeliveries)
                AddFoodDelivery(target, foodDelivery);
            Add(target, value.MarketConsumptions.Length);
            foreach (var consumption in value.MarketConsumptions)
                AddMarketConsumption(target, consumption);
            Add(target, value.IndividualOrders.Length);
            foreach (var order in value.IndividualOrders)
                AddIndividualOrder(target, order);
            Add(target, value.StockReservations.Length);
            foreach (var reservation in value.StockReservations)
                AddStockReservation(target, reservation);
            Add(target, value.ExportPreparations.Length);
            foreach (var preparation in value.ExportPreparations)
                Add수출준비(target, preparation);
            Add(target, value.ExportCargoPreparations.Length);
            foreach (var preparation in value.ExportCargoPreparations)
                Add수출Cargo준비(target, preparation);
            Add(target, value.ExportCargoHandoffs.Length);
            foreach (var handoff in value.ExportCargoHandoffs)
                Add수출Cargo인계(target, handoff);
            Add(target, value.ExportPortReceipts.Length);
            foreach (var receipt in value.ExportPortReceipts)
                Add수출항만인수(target, receipt);
            Add(target, value.ExportReadinessReviews.Length);
            foreach (var review in value.ExportReadinessReviews)
                Add수출준비성검토(target, review);
            Add(target, value.ExportShipmentPlans.Length);
            foreach (var plan in value.ExportShipmentPlans)
                Add수출선적계획(target, plan);
            Add(target, value.ExportShipmentExecutions.Length);
            foreach (var execution in value.ExportShipmentExecutions)
                Add수출선적실행(target, execution);
            if (value.TurnClosings.Length > 0 || value.ActiveTurnCardEffects.Length > 0)
            {
                Add(target, "TurnClosingExtensionV1");
                Add(target, value.TurnClosings.Length);
                foreach (var closing in value.TurnClosings)
                {
                    Add(target, closing.TurnClosingStableId);
                    Add(target, closing.ClosedTurnNumber);
                    Add(target, closing.ClosedGameDate.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
                    Add(target, closing.ResultingWorldTick);
                    Add(target, closing.ResultingRevision);
                    Add(target, closing.SelectedCards.Length);
                    foreach (var card in closing.SelectedCards)
                        AddTurnCard(target, card);
                }
                Add(target, value.ActiveTurnCardEffects.Length);
                foreach (var effect in value.ActiveTurnCardEffects)
                {
                    Add(target, effect.CardStableId);
                    Add(target, effect.CardRevision);
                    Add(target, effect.CardKindCode);
                    Add(target, effect.CardCopyStableId);
                    Add(target, effect.OfferStableId);
                    Add(target, effect.OrientationCode);
                    Add(target, effect.EffectCode);
                    Add(target, effect.TargetStatCode);
                    Add(target, effect.StatDelta);
                    Add(target, effect.ActiveTurnNumber);
                    Add(target, effect.SourceTurnClosingStableId);
                    Add(target, effect.SourceStableId);
                    Add(target, effect.RegionKey);
                    Add(target, effect.CalendarRevision);
                    Add(target, effect.EffectRuleRevision);
                    Add(target, effect.SourceUrl);
                    Add(target, effect.EvidenceCheckedAtUtc?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
                }
            }
            AddSettlement(target, value.Settlement);
        }

        private static void AddTurnCard(StringBuilder target, SimulationTurnCardSnapshot card)
        {
            Add(target, card.CardStableId);
            Add(target, card.CardRevision);
            Add(target, card.CardKindCode);
            Add(target, card.CardCopyStableId);
            Add(target, card.OfferStableId);
            Add(target, card.OrientationCode);
            Add(target, card.Title);
            Add(target, card.Summary);
            Add(target, card.EffectTimingCode);
            Add(target, card.EffectCode);
            Add(target, card.TargetStatCode);
            Add(target, card.StatDelta);
            Add(target, card.SourceStableId);
            Add(target, card.RegionKey);
            Add(target, card.AvailableFromGameDate?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
            Add(target, card.AvailableThroughGameDate?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
            Add(target, card.CalendarRevision);
            Add(target, card.EffectRuleRevision);
            Add(target, card.SourceUrl);
            Add(target, card.EvidenceCheckedAtUtc?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
        }

        private static void Add수출준비(
            StringBuilder target,
            Simulation수출준비Snapshot value)
        {
            Add(target, value.PreparationStableId);
            Add(target, value.RootPreparationStableId);
            Add(target, value.PreviousPreparationStableId ?? string.Empty);
            Add(target, value.AttemptNumber);
            Add(target, value.IsReworkAttempt);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.PackingFacilityStableId);
            Add(target, value.HandoffFacilityStableId);
            Add(target, value.PackageLotCandidateStableId);
            Add(target, value.HandoffCandidateStableId);
            Add(target, value.InspectionOutcomeCode);
            Add(target, value.FailureReasonCode ?? string.Empty);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.PackagingTicks);
            Add(target, value.InspectionTicks);
            Add(target, value.ReservedTick);
            Add(target, value.PackagedTick ?? -1);
            Add(target, value.InspectedTick ?? -1);
            Add(target, value.HandoffCandidateReadyTick ?? -1);
            Add(target, value.CanRetry);
            Add(target, value.CargoPreparationStableId ?? string.Empty);
            Add(target, value.CargoStableId ?? string.Empty);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출Cargo준비(
            StringBuilder target,
            Simulation수출Cargo준비Snapshot value)
        {
            Add(target, value.CargoPreparationStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceExportPreparationStableId);
            Add(target, value.RootExportPreparationStableId);
            Add(target, value.ExportPreparationAttemptNumber);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.CargoStableId);
            Add(target, value.CargoRevision);
            Add(target, value.RouteStableId);
            Add(target, value.OriginFacilityStableId);
            Add(target, value.DestinationFacilityStableId);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredPreparationTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.ReadyForHandoffTick ?? -1);
            Add(target, value.HandoffStableId ?? string.Empty);
            Add(target, value.HandoffCompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출Cargo인계(
            StringBuilder target,
            Simulation수출Cargo인계Snapshot value)
        {
            Add(target, value.HandoffStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceCargoPreparationStableId);
            Add(target, value.SourceExportPreparationStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.CargoStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.ReceivingFacilityStableId);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredHandoffTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.CompletedTick ?? -1);
            Add(target, value.LogisticsMovementCargoStableId ?? string.Empty);
            Add(target, value.LogisticsMovementTaskStableId ?? string.Empty);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출항만인수(
            StringBuilder target,
            Simulation수출항만인수Snapshot value)
        {
            Add(target, value.ReceiptStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.CargoStableId);
            Add(target, value.SourceExportCargoHandoffStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.ReceivingFacilityStableId);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredReceivingTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.CompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출선적실행(
            StringBuilder target,
            Simulation수출선적실행Snapshot value)
        {
            Add(target, value.ExecutionStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.OutcomeCode);
            Add(target, value.OutcomeRoll ?? -1);
            Add(target, value.SourceShipmentPlanStableId);
            Add(target, value.SourceReadinessReviewStableId);
            Add(target, value.SourcePortReceiptStableId);
            Add(target, value.CargoStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.DeliveredQuantity);
            Add(target, value.LostQuantity);
            Add(target, value.UnitCode);
            Add(target, value.DestinationCountryCode);
            Add(target, value.DestinationMarketStableId);
            Add(target, value.TransportModeCode);
            Add(target, value.ExecutionFacilityStableId);
            Add(target, value.EstimatedTransitTicks);
            Add(target, value.RiskScore);
            Add(target, value.SuccessProbabilityPercent);
            Add(target, value.ExpectedGrossRevenue);
            Add(target, value.ExpectedTotalCost);
            Add(target, value.PreviouslyRecognizedProjectedRevenue);
            Add(target, value.SuccessTreasuryDeltaCandidate);
            Add(target, value.LossTreasuryDeltaCandidate);
            Add(target, value.RequiredLossCapacityReservation);
            Add(target, value.AppliedTreasuryDelta ?? 0m);
            Add(target, value.TreasuryBeforeApplication ?? 0m);
            Add(target, value.TreasuryAfterApplication ?? 0m);
            Add(target, value.CurrencyCode);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.ScheduledTick);
            Add(target, value.DepartedTick ?? -1);
            Add(target, value.CompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출준비성검토(
            StringBuilder target,
            Simulation수출준비성검토Snapshot value)
        {
            Add(target, value.ReviewStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourcePortReceiptStableId);
            Add(target, value.ParentReviewStableId ?? string.Empty);
            Add(target, value.AttemptNumber);
            Add(target, value.CargoStableId);
            Add(target, value.SourceExportCargoHandoffStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.ReviewingFacilityStableId);
            Add(target, value.DocumentsPrepared);
            Add(target, value.InspectionPreparationReady);
            Add(target, value.OutcomeCode);
            AddStrings(target, value.MissingRequirementCodes);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredReviewTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.CompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void Add수출선적계획(
            StringBuilder target,
            Simulation수출선적계획Snapshot value)
        {
            Add(target, value.PlanStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceReadinessReviewStableId);
            Add(target, value.SourcePortReceiptStableId);
            Add(target, value.CargoStableId);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.DestinationCountryCode);
            Add(target, value.DestinationMarketStableId);
            Add(target, value.TransportModeCode);
            Add(target, value.PlanningFacilityStableId);
            Add(target, value.ExpectedGrossRevenue);
            Add(target, value.ExpectedInternationalLogisticsCost);
            Add(target, value.ExpectedHandlingCost);
            Add(target, value.ExpectedOtherCost);
            Add(target, value.ExpectedTotalCost);
            Add(target, value.ExpectedNetRevenue);
            Add(target, value.CurrencyCode);
            Add(target, value.EstimatedTransitTicks);
            Add(target, value.RiskScore);
            Add(target, value.RiskLevelCode);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredPlanningTicks);
            Add(target, value.ScheduledTick);
            Add(target, value.CompletedTick ?? -1);
            Add(target, value.ExecutionStableId ?? string.Empty);
            Add(target, value.ExecutionCompletedTick ?? -1);
            AddStrings(target, value.BoundaryCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddIndividualOrder(
            StringBuilder target,
            SimulationIndividualOrderSnapshot value)
        {
            Add(target, value.OrderStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.ActorStableId);
            Add(target, value.ProductStableId);
            Add(target, value.MarketFacilityStableId);
            Add(target, value.OrderedQuantity);
            Add(target, value.FulfilledQuantity);
            Add(target, value.UnitCode);
            Add(target, value.TotalPrice);
            Add(target, value.CurrencyCode);
            Add(target, value.RequiredLabor);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.CancellationTaskStableId ?? string.Empty);
            Add(target, value.ReservedTick);
            Add(target, value.ReadyForPickupTick.HasValue
                ? value.ReadyForPickupTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            Add(target, value.ConsumptionDecisionStableId ?? string.Empty);
            Add(target, value.ConsumptionTaskStableId ?? string.Empty);
            Add(target, value.ConsumedTick.HasValue
                ? value.ConsumedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            Add(target, value.CancelledTick.HasValue
                ? value.CancelledTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddStockReservation(
            StringBuilder target,
            SimulationStockReservationSnapshot value)
        {
            Add(target, value.ReservationStableId);
            Add(target, value.OrderStableId);
            Add(target, value.MarketFacilityStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.StateCode);
            Add(target, value.ReservedTick);
            Add(target, value.ConsumedTick.HasValue
                ? value.ConsumedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            Add(target, value.ReleasedTick.HasValue
                ? value.ReleasedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddLogisticsMovement(
            StringBuilder target,
            SimulationLogisticsMovementSnapshot value)
        {
            Add(target, value.CargoStableId);
            Add(target, value.CargoRevision);
            Add(target, value.SourceExportCargoHandoffStableId ?? string.Empty);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SourceAllocationStableId);
            Add(target, value.HarvestLotStableId);
            Add(target, value.PackageLotStableId);
            Add(target, value.ProductStableId);
            Add(target, value.Quantity);
            Add(target, value.ReservedQuantity);
            Add(target, value.UnitCode);
            Add(target, value.RouteStableId);
            Add(target, value.OriginFacilityStableId);
            Add(target, value.DestinationFacilityStableId);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.RequiredRouteTicks);
            Add(target, value.CompletedRouteTicks);
            Add(target, value.ReservedTick);
            Add(target, value.DepartedTick ?? -1);
            Add(target, value.ArrivedTick ?? -1);
            Add(target, value.DestinationStockCandidateStableId);
            Add(target, value.DestinationReceiptStableId ?? string.Empty);
            Add(target, value.DestinationReceiptCompletedTick ?? -1);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddFreightTransport(
            StringBuilder target,
            SimulationFreightTransportSnapshot value)
        {
            Add(target, value.TransportRequestStableId);
            Add(target, value.DispatchOfferStableId);
            Add(target, value.RequestStateCode);
            Add(target, value.DispatchStateCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.CargoStableId);
            Add(target, value.CarrierCandidateStableId);
            Add(target, value.VehicleStableId);
            Add(target, value.VehicleCapacity);
            Add(target, value.VehicleCapacityUnitCode);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.LogisticsTaskStableId);
            Add(target, value.ReceiptDecisionStableId ?? string.Empty);
            Add(target, value.ReceiptTaskStableId ?? string.Empty);
            Add(target, value.RequestedTick);
            Add(target, value.DispatchedTick ?? -1);
            Add(target, value.PickedUpTick ?? -1);
            Add(target, value.ArrivedAtDropoffTick ?? -1);
            Add(target, value.ReceivedTick ?? -1);
            Add(target, value.RuleRevision);
            AddFreightDispatchDecision(target, value.DispatchDecision);
            AddStrings(target, value.ExcludedOperationalEffectCodes);
            AddStrings(target, value.SourceStableIds);
            Add(target, value.StateHistory.Length);
            foreach (var transition in value.StateHistory)
            {
                Add(target, transition.FromStateCode);
                Add(target, transition.ToStateCode);
                Add(target, transition.WorldTick);
                Add(target, transition.CauseStableId);
                Add(target, transition.RuleRevision);
            }
        }

        private static void AddFreightDispatchDecision(
            StringBuilder target,
            SimulationFreightDispatchDecisionSnapshot? value)
        {
            Add(target, value == null ? 0 : 1);
            if (value == null) return;
            Add(target, value.DispatchOfferStableId);
            Add(target, value.TransportRequestStableId);
            Add(target, value.RecommendedCarrierCandidateStableId ?? string.Empty);
            Add(target, value.SelectedCarrierCandidateStableId ?? string.Empty);
            Add(target, value.SelectedVehicleStableId ?? string.Empty);
            Add(target, value.RuleRevision);
            AddStrings(target, value.SourceStableIds);
            Add(target, value.CandidateEvaluations.Length);
            foreach (var candidate in value.CandidateEvaluations)
            {
                Add(target, candidate.CarrierCandidateStableId);
                Add(target, candidate.VehicleStableId);
                Add(target, candidate.IsEligible ? 1 : 0);
                Add(target, candidate.IsRecommended ? 1 : 0);
                Add(target, candidate.IsSelected ? 1 : 0);
                Add(target, candidate.Rank);
                Add(target, candidate.PickupDistanceKm ?? decimal.MinValue);
                Add(target, candidate.VehicleCapacity);
                Add(target, candidate.VehicleCapacityUnitCode);
                Add(target, candidate.Reason);
                AddStrings(target, candidate.BlockReasonCodes);
                Add(target, candidate.Score.ScheduleScore);
                Add(target, candidate.Score.ProfitScore);
                Add(target, candidate.Score.DelayScore);
                Add(target, candidate.Score.DistanceScore);
                Add(target, candidate.Score.RecommendationTypeScore);
                Add(target, candidate.Score.CargoSensitivityScore);
                Add(target, candidate.Score.ReturnBurdenScore);
                Add(target, candidate.Score.BaseScore);
                Add(target, candidate.Score.DriverWaitingScore);
                Add(target, candidate.Score.TotalScore);
            }
        }

        private static void AddGroupOrder(StringBuilder target, Simulation같이주문Snapshot value)
        {
            Add(target, value.GroupOrderStableId);
            Add(target, value.ProductStableId);
            Add(target, value.DeliveryScopeStableId);
            Add(target, value.AggregationFacilityStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.ParticipantCount);
            Add(target, value.TotalQuantity);
            Add(target, value.UnitCode);
            Add(target, value.TargetParticipantCount);
            Add(target, value.TargetQuantity);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.CreatedTick);
            Add(target, value.FinalizedTick ?? -1);
            Add(target, value.RuleRevision);
            AddStrings(target, value.ExcludedOperationalEffectCodes);
            AddStrings(target, value.SourceStableIds);
            Add(target, value.Intents.Length);
            foreach (var intent in value.Intents)
            {
                Add(target, intent.IntentStableId);
                Add(target, intent.ParticipantStableId);
                Add(target, intent.Quantity);
                Add(target, intent.UnitCode);
                Add(target, intent.ExplicitParticipationConsent);
                AddStrings(target, intent.SourceStableIds);
            }
        }

        private static void AddFoodDelivery(
            StringBuilder target,
            Simulation음식배달Snapshot value)
        {
            Add(target, value.FoodOrderStableId);
            Add(target, value.MenuItemStableId);
            Add(target, value.RestaurantFacilityStableId);
            Add(target, value.DestinationFacilityStableId);
            Add(target, value.DeliveryScopeStableId);
            Add(target, value.OrdererStableId);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.PreparationDurationTicks);
            Add(target, value.DeliveryDurationTicks);
            Add(target, value.TotalDurationTicks);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.ReceiptDecisionStableId ?? string.Empty);
            Add(target, value.ReceiptTaskStableId ?? string.Empty);
            Add(target, value.AcceptedTick);
            Add(target, value.CookingStartedTick ?? -1);
            Add(target, value.ReadyForPickupTick ?? -1);
            Add(target, value.DispatchCandidateTick ?? -1);
            Add(target, value.PickedUpTick ?? -1);
            Add(target, value.DeliveredTick ?? -1);
            Add(target, value.ReceivedTick ?? -1);
            Add(target, value.RuleRevision);
            AddStrings(target, value.ExcludedOperationalEffectCodes);
            AddStrings(target, value.SourceStableIds);
            Add(target, value.StateHistory.Length);
            foreach (var transition in value.StateHistory)
            {
                Add(target, transition.FromStateCode);
                Add(target, transition.ToStateCode);
                Add(target, transition.WorldTick);
                Add(target, transition.CauseStableId);
                Add(target, transition.RuleRevision);
            }
        }

        private static void AddMarketConsumption(
            StringBuilder target,
            Simulation시장소비Snapshot value)
        {
            Add(target, value.ConsumptionStableId);
            Add(target, value.OrderStableId);
            Add(target, value.ReservationStableId);
            Add(target, value.ActorStableId);
            Add(target, value.ProductStableId);
            Add(target, value.MarketFacilityStableId);
            Add(target, value.Quantity);
            Add(target, value.UnitCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.DecisionStableId);
            Add(target, value.TaskStableId);
            Add(target, value.ScheduledTick);
            Add(target, value.ConsumedTick ?? -1);
            Add(target, value.MarketSupplyAfterOrderFulfillment);
            Add(target, value.MarketSupplyObservedAtConsumption.HasValue
                ? value.MarketSupplyObservedAtConsumption.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty);
            Add(target, value.AdditionalMarketSupplyDeductionApplied);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddDecision(StringBuilder target, SimulationDecisionSnapshot value)
        {
            Add(target, value.DecisionStableId);
            Add(target, value.DecisionTypeCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.SessionStableId);
            Add(target, value.FactionStableId);
            Add(target, value.TerritoryStableId);
            Add(target, value.SettlementStableId);
            Add(target, value.ActorStableId);
            AddStrings(target, value.TargetStableIds);
            Add(target, value.CreatedTick);
            Add(target, value.ConfirmedTick.HasValue ? value.ConfirmedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            AddValues(target, value.ExpectedCosts);
            AddValues(target, value.ExpectedEffects);
            AddStrings(target, value.Uncertainties);
            AddStrings(target, value.BlockReasonCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddTask(StringBuilder target, SimulationTaskSnapshot value)
        {
            Add(target, value.TaskStableId);
            Add(target, value.TaskTypeCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.CausedByDecisionStableId);
            Add(target, value.FacilityStableId);
            Add(target, value.AssignedCapacity);
            Add(target, value.AssignedCapacityUnitCode);
            Add(target, value.ScheduledStartTick);
            Add(target, value.ExpectedEndTick);
            Add(target, value.ActualEndTick.HasValue ? value.ActualEndTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            AddStrings(target, value.InputLotStableIds);
            AddStrings(target, value.OutputCandidateCodes);
            AddStrings(target, value.BlockReasonCodes);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddEffect(StringBuilder target, SimulationEffectRecord value)
        {
            Add(target, value.EffectStableId);
            Add(target, value.EffectTypeCode);
            Add(target, value.StateCode);
            Add(target, value.Revision);
            Add(target, value.AppliedTick.HasValue ? value.AppliedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
            Add(target, value.CausedByDecisionStableId);
            Add(target, value.CausedByTaskStableId);
            Add(target, value.TargetLedgerStableId);
            Add(target, value.BeforeValue);
            Add(target, value.Delta);
            Add(target, value.AfterValue);
            Add(target, value.UnitCode);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddSettlement(
            StringBuilder target,
            SimulationSettlementEconomySnapshot? value)
        {
            Add(target, value == null ? "none" : "present");
            if (value == null) return;
            Add(target, value.SettlementStableId);
            Add(target, value.WorldTick);
            Add(target, value.Revision);
            Add(target, value.RuleRevision);
            Add(target, value.TreasuryBalance);
            Add(target, value.TreasuryReserved);
            Add(target, value.TreasuryAvailable);
            Add(target, value.CurrencyCode);
            Add(target, value.LaborCapacityTotal);
            Add(target, value.LaborReserved);
            Add(target, value.LaborAvailable);
            Add(target, value.StorageCapacity);
            Add(target, value.StorageOccupied);
            Add(target, value.StorageReserved);
            Add(target, value.StorageAvailable);
            Add(target, value.StorageUnitCode);
            Add(target, value.PopulationCount);
            Add(target, value.PopulationFoodDemandPerTick);
            Add(target, value.GarrisonCount);
            Add(target, value.GarrisonFoodDemandPerTick);
            Add(target, value.FoodReserveEquivalent);
            Add(target, value.FoodDemandPerTick);
            Add(target, value.FoodSecurityDays);
            Add(target, value.FoodEquivalentUnitCode);
            Add(target, value.FoodEquivalentRuleRevision);
            Add(target, value.FoodSecurityFormulaCode);
            Add(target, value.Districts.Length);
            foreach (var district in value.Districts)
            {
                Add(target, district.DistrictStableId);
                Add(target, district.DistrictTypeCode);
                AddStrings(target, district.SourceStableIds);
            }
            Add(target, value.Facilities.Length);
            foreach (var facility in value.Facilities)
            {
                Add(target, facility.FacilityStableId);
                Add(target, facility.FacilityTypeCode);
                Add(target, facility.DistrictStableId);
                AddStrings(target, facility.SourceStableIds);
            }
            Add(target, value.MarketSupplyByProduct.Length);
            foreach (var supply in value.MarketSupplyByProduct)
            {
                Add(target, supply.ProductStableId);
                Add(target, supply.Quantity);
                Add(target, supply.UnitCode);
                AddStrings(target, supply.SourceStableIds);
            }
            Add(target, value.ResidentConsumptionByProduct.Length);
            foreach (var consumption in value.ResidentConsumptionByProduct)
            {
                Add(target, consumption.ProductStableId);
                Add(target, consumption.Quantity);
                Add(target, consumption.UnitCode);
                Add(target, consumption.ConsumptionCount);
                AddStrings(target, consumption.SourceStableIds);
            }
            Add(target, value.ReserveStockLots.Length);
            foreach (var lot in value.ReserveStockLots)
            {
                Add(target, lot.StockLotStableId);
                Add(target, lot.ProductStableId);
                Add(target, lot.StorageFacilityStableId);
                Add(target, lot.Quantity);
                Add(target, lot.OutboundReservedQuantity);
                Add(target, lot.AvailableQuantity);
                Add(target, lot.UnitCode);
                Add(target, lot.FoodEquivalentQuantity);
                Add(target, lot.OutboundReservedFoodEquivalentQuantity);
                Add(target, lot.AvailableFoodEquivalentQuantity);
                AddStrings(target, lot.SourceStableIds);
            }
            Add(target, value.HarvestLotAllocations.Length);
            foreach (var allocation in value.HarvestLotAllocations)
            {
                Add(target, allocation.AllocationStableId);
                Add(target, allocation.HarvestLotStableId);
                Add(target, allocation.HarvestLotRevision);
                Add(target, allocation.ProductStableId);
                Add(target, allocation.Quantity);
                Add(target, allocation.UnitCode);
                Add(target, allocation.ChoiceCode);
                Add(target, allocation.NextWorkflowCode);
                Add(target, allocation.DecisionStableId);
                Add(target, allocation.TaskStableId);
                Add(target, allocation.FacilityStableId);
                Add(target, allocation.RequiredLabor);
                Add(target, allocation.TreasuryCost);
                Add(target, allocation.ProjectedRevenue.HasValue
                    ? allocation.ProjectedRevenue.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                Add(target, allocation.StateCode);
                Add(target, allocation.ReservedTick);
                Add(target, allocation.AppliedTick.HasValue
                    ? allocation.AppliedTick.Value.ToString(CultureInfo.InvariantCulture) : string.Empty);
                Add(target, allocation.ReserveStockLotStableId ?? string.Empty);
                Add(target, allocation.StoredQuantity);
                Add(target, allocation.FoodEquivalentQuantity);
                Add(target, allocation.OutboundReservedQuantity);
                Add(target, allocation.AvailableQuantity);
                AddStrings(target, allocation.SourceStableIds);
            }
            AddStrings(target, value.ActiveTaskStableIds);
            AddStrings(target, value.SourceStableIds);
        }

        private static void AddValues(StringBuilder target, SimulationValueProjection[] values)
        {
            Add(target, values.Length);
            foreach (var value in values)
            {
                Add(target, value.ValueTypeCode);
                Add(target, value.TargetLedgerStableId);
                Add(target, value.BeforeValue);
                Add(target, value.Delta);
                Add(target, value.AfterValue);
                Add(target, value.UnitCode);
                AddStrings(target, value.SourceStableIds);
            }
        }

        private static void AddStrings(StringBuilder target, string[] values)
        {
            Add(target, values.Length);
            foreach (var value in values)
                Add(target, value);
        }

        private static void Add(StringBuilder target, object value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture));
            target.Append(':');
            target.Append(text);
            target.Append('|');
        }
    }

    public static class SimulationSaveReplayCloner
    {
        public static SimulationSessionSavePackage ClonePackage(SimulationSessionSavePackage source)
            => new SimulationSessionSavePackage
            {
                SchemaVersion = source.SchemaVersion,
                SaveStableId = source.SaveStableId,
                SessionStableId = source.SessionStableId,
                SavedWorldTick = source.SavedWorldTick,
                SavedWorldRevision = source.SavedWorldRevision,
                ReplayHashAlgorithmCode = source.ReplayHashAlgorithmCode,
                ReplayHash = source.ReplayHash,
                SessionCreateRequest = CloneCreateRequest(source.SessionCreateRequest),
                Snapshot = 경영SimulationSessionAggregate.Clone(source.Snapshot),
                CommandLog = source.CommandLog.Select(CloneCommand).ToArray(),
            };

        public static 경영SimulationSession생성Request CloneCreateRequest(
            경영SimulationSession생성Request source)
            => new 경영SimulationSession생성Request
            {
                ClientRequestId = source.ClientRequestId,
                ScenarioStableId = source.ScenarioStableId,
                ScenarioDataRevision = source.ScenarioDataRevision,
                ScenarioSeed = source.ScenarioSeed,
                RuleRevision = source.RuleRevision,
                DurationTicks = source.DurationTicks,
                WorldContext = new SimulationWorldContext생성Request
                {
                    FactionStableId = source.WorldContext.FactionStableId,
                    TerritoryStableId = source.WorldContext.TerritoryStableId,
                    SettlementStableId = source.WorldContext.SettlementStableId,
                    GameDateStartsOn = source.WorldContext.GameDateStartsOn,
                },
                Settlement = 경영SimulationSessionAggregate.CloneSettlementRequest(source.Settlement),
            };

        public static SimulationCommandLogEntrySnapshot CloneCommand(
            SimulationCommandLogEntrySnapshot source)
            => new SimulationCommandLogEntrySnapshot
            {
                Sequence = source.Sequence,
                CommandTypeCode = source.CommandTypeCode,
                AppliedWorldTick = source.AppliedWorldTick,
                ResultingWorldRevision = source.ResultingWorldRevision,
                TickRequest = source.TickRequest == null ? null : CloneTickRequest(source.TickRequest),
                DecisionConfirmRequest = source.DecisionConfirmRequest == null
                    ? null
                    : CloneConfirmRequest(source.DecisionConfirmRequest),
                HarvestDispositionImpactConfirmRequest = source.HarvestDispositionImpactConfirmRequest == null
                    ? null
                    : CloneHarvestDispositionImpactConfirmRequest(
                        source.HarvestDispositionImpactConfirmRequest),
                LogisticsMovementConfirmRequest = source.LogisticsMovementConfirmRequest == null
                    ? null
                    : CloneLogisticsMovementConfirmRequest(source.LogisticsMovementConfirmRequest),
                TurnClosingConfirmRequest = source.TurnClosingConfirmRequest == null
                    ? null
                    : CloneTurnClosingConfirmRequest(source.TurnClosingConfirmRequest),
            };

        public static SimulationTurnClosingConfirmRequest CloneTurnClosingConfirmRequest(
            SimulationTurnClosingConfirmRequest source)
            => new SimulationTurnClosingConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                Preview = new SimulationTurnClosingPreviewRequest
                {
                    ExpectedRevision = source.Preview.ExpectedRevision,
                    SelectedCardStableIds = source.Preview.SelectedCardStableIds.ToArray(),
                    SelectedTarotCard = source.Preview.SelectedTarotCard == null
                        ? null
                        : new Simulation타로CardSelectionRequest
                        {
                            OfferStableId = source.Preview.SelectedTarotCard.OfferStableId,
                            CardStableId = source.Preview.SelectedTarotCard.CardStableId,
                            OrientationCode = source.Preview.SelectedTarotCard.OrientationCode,
                        },
                },
            };

        public static 경영SimulationTick진행Request CloneTickRequest(
            경영SimulationTick진행Request source)
            => new 경영SimulationTick진행Request
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                TickCount = source.TickCount,
            };

        public static SimulationDecisionConfirmRequest CloneConfirmRequest(
            SimulationDecisionConfirmRequest source)
            => new SimulationDecisionConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                Preview = ClonePreviewRequest(source.Preview),
            };

        public static SimulationHarvestDispositionImpactConfirmRequest
            CloneHarvestDispositionImpactConfirmRequest(
                SimulationHarvestDispositionImpactConfirmRequest source)
            => new SimulationHarvestDispositionImpactConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                Impact = new SimulationHarvestDispositionImpactPreviewRequest
                {
                    DispositionDecisionStableId = source.Impact.DispositionDecisionStableId,
                    DispositionDecisionRevision = source.Impact.DispositionDecisionRevision,
                    HarvestLotStableId = source.Impact.HarvestLotStableId,
                    HarvestLotRevision = source.Impact.HarvestLotRevision,
                    ProductStableId = source.Impact.ProductStableId,
                    Quantity = source.Impact.Quantity,
                    UnitCode = source.Impact.UnitCode,
                    ChoiceCode = source.Impact.ChoiceCode,
                    NextWorkflowCode = source.Impact.NextWorkflowCode,
                    ActorStableId = source.Impact.ActorStableId,
                    SourceStableIds = source.Impact.SourceStableIds.ToArray(),
                },
            };

        public static SimulationLogisticsMovementConfirmRequest
            CloneLogisticsMovementConfirmRequest(
                SimulationLogisticsMovementConfirmRequest source)
            => new SimulationLogisticsMovementConfirmRequest
            {
                CommandId = source.CommandId,
                ExpectedRevision = source.ExpectedRevision,
                Movement = new SimulationLogisticsMovementPreviewRequest
                {
                    CargoStableId = source.Movement.CargoStableId,
                    CargoRevision = source.Movement.CargoRevision,
                    SourceExportCargoHandoffStableId =
                        source.Movement.SourceExportCargoHandoffStableId,
                    SourceAllocationStableId = source.Movement.SourceAllocationStableId,
                    HarvestLotStableId = source.Movement.HarvestLotStableId,
                    PackageLotStableId = source.Movement.PackageLotStableId,
                    ProductStableId = source.Movement.ProductStableId,
                    Quantity = source.Movement.Quantity,
                    UnitCode = source.Movement.UnitCode,
                    RouteStableId = source.Movement.RouteStableId,
                    OriginFacilityStableId = source.Movement.OriginFacilityStableId,
                    DestinationFacilityStableId = source.Movement.DestinationFacilityStableId,
                    ActorStableId = source.Movement.ActorStableId,
                    RequiredRouteTicks = source.Movement.RequiredRouteTicks,
                    FreightTransport = 경영SimulationSessionAggregate.CloneFreightTransportBinding(
                        source.Movement.FreightTransport),
                    SourceStableIds = source.Movement.SourceStableIds.ToArray(),
                },
            };

        private static SimulationDecisionPreviewRequest ClonePreviewRequest(
            SimulationDecisionPreviewRequest source)
            => new SimulationDecisionPreviewRequest
            {
                DecisionStableId = source.DecisionStableId,
                DecisionTypeCode = source.DecisionTypeCode,
                ActorStableId = source.ActorStableId,
                TargetStableIds = source.TargetStableIds.ToArray(),
                ExpectedCosts = source.ExpectedCosts.Select(CloneValue).ToArray(),
                ExpectedEffects = source.ExpectedEffects.Select(CloneValue).ToArray(),
                Uncertainties = source.Uncertainties.ToArray(),
                BlockReasonCodes = source.BlockReasonCodes.ToArray(),
                SourceStableIds = source.SourceStableIds.ToArray(),
                Task = new SimulationTaskPlanRequest
                {
                    TaskStableId = source.Task.TaskStableId,
                    TaskTypeCode = source.Task.TaskTypeCode,
                    FacilityStableId = source.Task.FacilityStableId,
                    AssignedCapacity = source.Task.AssignedCapacity,
                    AssignedCapacityUnitCode = source.Task.AssignedCapacityUnitCode,
                    DurationTicks = source.Task.DurationTicks,
                    InputLotStableIds = source.Task.InputLotStableIds.ToArray(),
                    OutputCandidateCodes = source.Task.OutputCandidateCodes.ToArray(),
                    SourceStableIds = source.Task.SourceStableIds.ToArray(),
                },
            };

        private static SimulationValueProjection CloneValue(SimulationValueProjection source)
            => new SimulationValueProjection
            {
                ValueTypeCode = source.ValueTypeCode,
                TargetLedgerStableId = source.TargetLedgerStableId,
                BeforeValue = source.BeforeValue,
                Delta = source.Delta,
                AfterValue = source.AfterValue,
                UnitCode = source.UnitCode,
                SourceStableIds = source.SourceStableIds.ToArray(),
            };
    }
}
