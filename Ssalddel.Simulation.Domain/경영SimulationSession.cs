using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class SimulationContractException : InvalidOperationException
    {
        public SimulationContractException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }

    public sealed class SimulationNotFoundException : InvalidOperationException
    {
        public SimulationNotFoundException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }

    public sealed class SimulationConflictException : InvalidOperationException
    {
        public SimulationConflictException(string errorCode)
            : base(errorCode)
        {
            ErrorCode = errorCode;
        }

        public string ErrorCode { get; }
    }

    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly object gate = new object();
        private readonly Dictionary<string, 적용된TickCommand> appliedCommands =
            new Dictionary<string, 적용된TickCommand>(StringComparer.Ordinal);

        public 경영SimulationSessionAggregate(경영SimulationSession생성Request request)
        {
            ValidateCreate(request);
            SessionStableId = "simulation-session:" + request.ClientRequestId.ToString("N");
            ClientRequestId = request.ClientRequestId;
            ScenarioStableId = request.ScenarioStableId.Trim();
            ScenarioDataRevision = request.ScenarioDataRevision.Trim();
            ScenarioSeed = request.ScenarioSeed;
            RuleRevision = request.RuleRevision.Trim();
            DurationTicks = request.DurationTicks;
            FactionStableId = request.WorldContext.FactionStableId.Trim();
            TerritoryStableId = request.WorldContext.TerritoryStableId.Trim();
            SettlementStableId = request.WorldContext.SettlementStableId.Trim();
            GameDateStartsOn = request.WorldContext.GameDateStartsOn;
            InitializeSettlement(request.Settlement);
        }

        public string SessionStableId { get; }
        public Guid ClientRequestId { get; }
        public string ScenarioStableId { get; }
        public string ScenarioDataRevision { get; }
        public int ScenarioSeed { get; }
        public string RuleRevision { get; }
        public int CurrentTick { get; private set; }
        public int DurationTicks { get; }
        public string FactionStableId { get; }
        public string TerritoryStableId { get; }
        public string SettlementStableId { get; }
        public DateTimeOffset GameDateStartsOn { get; }
        public long Revision { get; private set; }

        public 경영SimulationSessionSnapshot Snapshot()
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }

        public 경영SimulationSessionSnapshot Advance(경영SimulationTick진행Request request)
        {
            ValidateAdvance(request);
            lock (gate)
            {
                if (HasAppliedDecisionCommand(request.CommandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (appliedCommands.TryGetValue(request.CommandId, out var applied))
                {
                    if (applied.TickCount != request.TickCount)
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }

                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                if (CurrentTick + request.TickCount > DurationTicks)
                    throw new SimulationConflictException("SimulationDurationExceeded");

                CurrentTick += request.TickCount;
                AdvanceDecisionWork(CurrentTick);
                ExpireActiveTurnCardEffects();
                Revision++;
                AppendTickCommand(request);
                var snapshot = CreateSnapshot();
                appliedCommands.Add(
                    request.CommandId,
                    new 적용된TickCommand(request.TickCount, Clone(snapshot)));
                return snapshot;
            }
        }

        public void EnsureSameCreationRequest(경영SimulationSession생성Request request)
        {
            ValidateCreate(request);
            if (ClientRequestId != request.ClientRequestId
                || !string.Equals(ScenarioStableId, request.ScenarioStableId.Trim(), StringComparison.Ordinal)
                || !string.Equals(ScenarioDataRevision, request.ScenarioDataRevision.Trim(), StringComparison.Ordinal)
                || ScenarioSeed != request.ScenarioSeed
                || !string.Equals(RuleRevision, request.RuleRevision.Trim(), StringComparison.Ordinal)
                || DurationTicks != request.DurationTicks
                || !string.Equals(FactionStableId, request.WorldContext.FactionStableId.Trim(), StringComparison.Ordinal)
                || !string.Equals(TerritoryStableId, request.WorldContext.TerritoryStableId.Trim(), StringComparison.Ordinal)
                || !string.Equals(SettlementStableId, request.WorldContext.SettlementStableId.Trim(), StringComparison.Ordinal)
                || GameDateStartsOn != request.WorldContext.GameDateStartsOn
                || !string.Equals(
                    SettlementPayloadKey,
                    BuildSettlementPayloadKey(request.Settlement),
                    StringComparison.Ordinal))
            {
                throw new SimulationConflictException("SimulationCreateRequestPayloadConflict");
            }
        }

        private 경영SimulationSessionSnapshot CreateSnapshot()
            => new 경영SimulationSessionSnapshot
            {
                SessionStableId = SessionStableId,
                ClientRequestId = ClientRequestId,
                ScenarioStableId = ScenarioStableId,
                ScenarioDataRevision = ScenarioDataRevision,
                ScenarioSeed = ScenarioSeed,
                RuleRevision = RuleRevision,
                CurrentTick = CurrentTick,
                DurationTicks = DurationTicks,
                Revision = Revision,
                IsCompleted = CurrentTick == DurationTicks,
                ModeCode = SimulationModeCodes.Simulation,
                IsOperationalState = false,
                WorldContext = new SimulationWorldContextSnapshot
                {
                    FactionStableId = FactionStableId,
                    TerritoryStableId = TerritoryStableId,
                    SettlementStableId = SettlementStableId,
                    WorldTick = CurrentTick,
                    WorldRevision = Revision,
                    GameDateStartsOn = GameDateStartsOn,
                    GameDate = GameDateStartsOn.AddDays(CurrentTick),
                    CalendarRuleCode = "OneTickOneDay",
                },
                Decisions = CreateDecisionSnapshots(),
                Tasks = CreateTaskSnapshots(),
                Effects = CreateEffectSnapshots(),
                LogisticsMovements = CreateLogisticsMovementSnapshots(),
                FreightTransports = CreateFreightTransportSnapshots(),
                GroupOrders = CreateGroupOrderSnapshots(),
                FoodDeliveries = CreateFoodDeliverySnapshots(),
                MarketConsumptions = CreateMarketConsumptionSnapshots(),
                IndividualOrders = CreateIndividualOrderSnapshots(),
                StockReservations = CreateStockReservationSnapshots(),
                ExportPreparations = Create수출준비Snapshots(),
                ExportCargoPreparations = Create수출Cargo준비Snapshots(),
                ExportCargoHandoffs = Create수출Cargo인계Snapshots(),
                ExportPortReceipts = Create수출항만인수Snapshots(),
                ExportReadinessReviews = Create수출준비성검토Snapshots(),
                ExportShipmentPlans = Create수출선적계획Snapshots(),
                ExportShipmentExecutions = Create수출선적실행Snapshots(),
                TurnClosings = CreateTurnClosingSnapshots(),
                ActiveTurnCardEffects = CreateActiveTurnCardEffectSnapshots(),
                Settlement = CreateSettlementSnapshot(),
            };

        internal static 경영SimulationSessionSnapshot Clone(경영SimulationSessionSnapshot source)
            => new 경영SimulationSessionSnapshot
            {
                SessionStableId = source.SessionStableId,
                ClientRequestId = source.ClientRequestId,
                ScenarioStableId = source.ScenarioStableId,
                ScenarioDataRevision = source.ScenarioDataRevision,
                ScenarioSeed = source.ScenarioSeed,
                RuleRevision = source.RuleRevision,
                CurrentTick = source.CurrentTick,
                DurationTicks = source.DurationTicks,
                Revision = source.Revision,
                IsCompleted = source.IsCompleted,
                ModeCode = source.ModeCode,
                IsOperationalState = source.IsOperationalState,
                WorldContext = new SimulationWorldContextSnapshot
                {
                    FactionStableId = source.WorldContext.FactionStableId,
                    TerritoryStableId = source.WorldContext.TerritoryStableId,
                    SettlementStableId = source.WorldContext.SettlementStableId,
                    WorldTick = source.WorldContext.WorldTick,
                    WorldRevision = source.WorldContext.WorldRevision,
                    GameDateStartsOn = source.WorldContext.GameDateStartsOn,
                    GameDate = source.WorldContext.GameDate,
                    CalendarRuleCode = source.WorldContext.CalendarRuleCode,
                },
                Decisions = CloneDecisions(source.Decisions),
                Tasks = CloneTasks(source.Tasks),
                Effects = CloneEffects(source.Effects),
                LogisticsMovements = source.LogisticsMovements.Select(CloneLogisticsMovement).ToArray(),
                FreightTransports = source.FreightTransports.Select(CloneFreightTransport).ToArray(),
                GroupOrders = source.GroupOrders.Select(CloneGroupOrder).ToArray(),
                FoodDeliveries = source.FoodDeliveries.Select(CloneFoodDelivery).ToArray(),
                MarketConsumptions = source.MarketConsumptions.Select(CloneMarketConsumption).ToArray(),
                IndividualOrders = source.IndividualOrders.Select(CloneIndividualOrder).ToArray(),
                StockReservations = source.StockReservations.Select(CloneStockReservation).ToArray(),
                ExportPreparations = source.ExportPreparations.Select(Clone수출준비).ToArray(),
                ExportCargoPreparations = source.ExportCargoPreparations
                    .Select(Clone수출Cargo준비).ToArray(),
                ExportCargoHandoffs = source.ExportCargoHandoffs
                    .Select(Clone수출Cargo인계).ToArray(),
                ExportPortReceipts = source.ExportPortReceipts
                    .Select(Clone수출항만인수).ToArray(),
                ExportReadinessReviews = source.ExportReadinessReviews
                    .Select(Clone수출준비성검토).ToArray(),
                ExportShipmentPlans = source.ExportShipmentPlans
                    .Select(Clone수출선적계획).ToArray(),
                ExportShipmentExecutions = source.ExportShipmentExecutions
                    .Select(Clone수출선적실행).ToArray(),
                TurnClosings = source.TurnClosings.Select(CloneTurnClosing).ToArray(),
                ActiveTurnCardEffects = source.ActiveTurnCardEffects
                    .Select(CloneActiveTurnCardEffect).ToArray(),
                Settlement = CloneSettlementSnapshot(source.Settlement),
            };

        internal static void ValidateCreate(경영SimulationSession생성Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ClientRequestId == Guid.Empty)
                throw new SimulationContractException("SimulationClientRequestIdMissing");
            RequireStableId(request.ScenarioStableId, "SimulationScenarioStableIdInvalid");
            RequireText(request.ScenarioDataRevision, "SimulationScenarioDataRevisionMissing");
            RequireText(request.RuleRevision, "SimulationRuleRevisionMissing");
            if (request.DurationTicks <= 0 || request.DurationTicks > 365)
                throw new SimulationContractException("SimulationDurationTicksInvalid");
            if (request.WorldContext == null)
                throw new SimulationContractException("SimulationWorldContextMissing");
            RequireStableId(request.WorldContext.FactionStableId, "SimulationFactionStableIdInvalid");
            RequireStableId(request.WorldContext.TerritoryStableId, "SimulationTerritoryStableIdInvalid");
            RequireStableId(request.WorldContext.SettlementStableId, "SimulationSettlementStableIdInvalid");
            if (request.WorldContext.GameDateStartsOn == default
                || request.WorldContext.GameDateStartsOn.Offset != TimeSpan.Zero
                || request.WorldContext.GameDateStartsOn.TimeOfDay != TimeSpan.Zero)
                throw new SimulationContractException("SimulationGameDateStartsOnInvalid");
            ValidateSettlementInitialState(request.Settlement, request.WorldContext.SettlementStableId);
        }

        internal static void ValidateAdvance(경영SimulationTick진행Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.TickCount <= 0 || request.TickCount > 28)
                throw new SimulationContractException("SimulationTickCountInvalid");
        }

        private static void RequireStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                throw new SimulationContractException(errorCode);
            }
        }

        private static void RequireText(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(errorCode);
        }

        private sealed class 적용된TickCommand
        {
            public 적용된TickCommand(int tickCount, 경영SimulationSessionSnapshot snapshot)
            {
                TickCount = tickCount;
                Snapshot = snapshot;
            }

            public int TickCount { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }

    public interface I경영SimulationSessionStore
    {
        경영SimulationSessionAggregate CreateOrGet(경영SimulationSession생성Request request);
        경영SimulationSessionAggregate? Find(string sessionStableId);
        경영SimulationSessionAggregate Restore(경영SimulationSessionAggregate session);
    }

    public sealed class InMemory경영SimulationSessionStore : I경영SimulationSessionStore
    {
        private readonly ConcurrentDictionary<string, 경영SimulationSessionAggregate> sessions =
            new ConcurrentDictionary<string, 경영SimulationSessionAggregate>(StringComparer.Ordinal);

        public 경영SimulationSessionAggregate CreateOrGet(경영SimulationSession생성Request request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var candidate = new 경영SimulationSessionAggregate(request);
            var session = sessions.GetOrAdd(candidate.SessionStableId, candidate);
            session.EnsureSameCreationRequest(request);
            return session;
        }

        public 경영SimulationSessionAggregate? Find(string sessionStableId)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId)) return null;
            return sessions.TryGetValue(sessionStableId, out var session) ? session : null;
        }

        public 경영SimulationSessionAggregate Restore(경영SimulationSessionAggregate session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (!sessions.TryAdd(session.SessionStableId, session))
                throw new SimulationConflictException("SimulationSessionAlreadyActive");
            return session;
        }
    }

    public sealed class 경영SimulationSessionService
    {
        private readonly I경영SimulationSessionStore store;
        private readonly ISimulationSessionSaveStore saveStore;

        public 경영SimulationSessionService(I경영SimulationSessionStore store)
            : this(store, new InMemorySimulationSessionSaveStore())
        {
        }

        public 경영SimulationSessionService(
            I경영SimulationSessionStore store,
            ISimulationSessionSaveStore saveStore)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.saveStore = saveStore ?? throw new ArgumentNullException(nameof(saveStore));
        }

        public 경영SimulationSessionSnapshot Create(경영SimulationSession생성Request request)
            => store.CreateOrGet(request).Snapshot();

        public 경영SimulationSessionSnapshot Get(string sessionStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Snapshot();

        public 경영SimulationSessionSnapshot Advance(
            string sessionStableId,
            경영SimulationTick진행Request request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Advance(request);

        public SimulationTurnClosingContextSnapshot GetTurnClosingContext(
            string sessionStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .GetTurnClosingContext();

        public SimulationTurnClosingPreviewSnapshot PreviewTurnClosing(
            string sessionStableId,
            SimulationTurnClosingPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewTurnClosing(request);

        public 경영SimulationSessionSnapshot ConfirmTurnClosing(
            string sessionStableId,
            SimulationTurnClosingConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmTurnClosing(request);

        public SimulationDecisionPreviewSnapshot PreviewDecision(
            string sessionStableId,
            SimulationDecisionPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewDecision(request);

        public 경영SimulationSessionSnapshot ConfirmDecision(
            string sessionStableId,
            SimulationDecisionConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmDecision(request);

        public SimulationIndividualOrderPreviewSnapshot PreviewIndividualOrder(
            string sessionStableId,
            SimulationIndividualOrderPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewIndividualOrder(request);

        public 경영SimulationSessionSnapshot ConfirmIndividualOrder(
            string sessionStableId,
            SimulationIndividualOrderConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmIndividualOrder(request);

        public SimulationDecisionPreviewSnapshot PreviewIndividualOrderCancellation(
            string sessionStableId,
            SimulationIndividualOrderCancelRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewIndividualOrderCancellation(request);

        public 경영SimulationSessionSnapshot ConfirmIndividualOrderCancellation(
            string sessionStableId,
            SimulationIndividualOrderCancelRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmIndividualOrderCancellation(request);

        public SimulationLogisticsMovementPreviewSnapshot PreviewLogisticsMovement(
            string sessionStableId,
            SimulationLogisticsMovementPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewLogisticsMovement(request);

        public 경영SimulationSessionSnapshot ConfirmLogisticsMovement(
            string sessionStableId,
            SimulationLogisticsMovementConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmLogisticsMovement(request);

        public SimulationFreightTransportPreviewSnapshot PreviewFreightTransport(
            string sessionStableId,
            SimulationFreightTransportPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFreightTransport(request);

        public 경영SimulationSessionSnapshot ConfirmFreightTransport(
            string sessionStableId,
            SimulationFreightTransportConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFreightTransport(request);

        public SimulationDecisionPreviewSnapshot PreviewFreightReceipt(
            string sessionStableId,
            SimulationFreightReceiptPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFreightReceipt(request);

        public 경영SimulationSessionSnapshot ConfirmFreightReceipt(
            string sessionStableId,
            SimulationFreightReceiptConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFreightReceipt(request);

        public Simulation같이주문PreviewSnapshot PreviewGroupOrder(
            string sessionStableId,
            Simulation같이주문PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewGroupOrder(request);

        public 경영SimulationSessionSnapshot ConfirmGroupOrder(
            string sessionStableId,
            Simulation같이주문ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmGroupOrder(request);

        public Simulation음식배달PreviewSnapshot PreviewFoodDelivery(
            string sessionStableId,
            Simulation음식배달PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFoodDelivery(request);

        public 경영SimulationSessionSnapshot ConfirmFoodDelivery(
            string sessionStableId,
            Simulation음식배달ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFoodDelivery(request);

        public SimulationDecisionPreviewSnapshot PreviewFoodDeliveryReceipt(
            string sessionStableId,
            Simulation음식배달수령PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewFoodDeliveryReceipt(request);

        public 경영SimulationSessionSnapshot ConfirmFoodDeliveryReceipt(
            string sessionStableId,
            Simulation음식배달수령ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmFoodDeliveryReceipt(request);

        public Simulation시장소비PreviewSnapshot PreviewMarketConsumption(
            string sessionStableId,
            Simulation시장소비PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewMarketConsumption(request);

        public 경영SimulationSessionSnapshot ConfirmMarketConsumption(
            string sessionStableId,
            Simulation시장소비ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmMarketConsumption(request);

        public SimulationSessionSavePackage Save(
            string sessionStableId,
            SimulationSessionSaveRequest request)
        {
            var session = store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound");
            return saveStore.SaveOrGet(session.CreateSavePackage(request));
        }

        public SimulationSessionRestoreResult Restore(SimulationSessionRestoreRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.SaveStableId))
                throw new SimulationContractException("SimulationSaveStableIdInvalid");
            var package = saveStore.Find(request.SaveStableId.Trim())
                ?? throw new SimulationNotFoundException("SimulationSaveNotFound");
            var restored = SimulationSessionReplay.Restore(package);
            store.Restore(restored);
            return new SimulationSessionRestoreResult
            {
                SaveStableId = package.SaveStableId,
                SchemaVersion = package.SchemaVersion,
                ReplayHash = package.ReplayHash,
                ReplayedCommandCount = package.CommandLog.Length,
                Session = restored.Snapshot(),
            };
        }

        public SimulationHarvestDispositionImpactPreviewSnapshot PreviewHarvestDispositionImpact(
            string sessionStableId,
            SimulationHarvestDispositionImpactPreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .PreviewHarvestDispositionImpact(request);

        public 경영SimulationSessionSnapshot ConfirmHarvestDispositionImpact(
            string sessionStableId,
            SimulationHarvestDispositionImpactConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .ConfirmHarvestDispositionImpact(request);

        public Simulation수출준비PreviewSnapshot Preview수출준비(
            string sessionStableId,
            Simulation수출준비PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출준비(request);

        public 경영SimulationSessionSnapshot Confirm수출준비(
            string sessionStableId,
            Simulation수출준비ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출준비(request);

        public Simulation수출준비PreviewSnapshot Preview수출재작업(
            string sessionStableId,
            Simulation수출재작업PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출재작업(request);

        public 경영SimulationSessionSnapshot Confirm수출재작업(
            string sessionStableId,
            Simulation수출재작업ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출재작업(request);

        public Simulation수출Cargo준비PreviewSnapshot Preview수출Cargo준비(
            string sessionStableId,
            Simulation수출Cargo준비PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출Cargo준비(request);

        public 경영SimulationSessionSnapshot Confirm수출Cargo준비(
            string sessionStableId,
            Simulation수출Cargo준비ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출Cargo준비(request);

        public Simulation수출Cargo인계PreviewSnapshot Preview수출Cargo인계(
            string sessionStableId,
            Simulation수출Cargo인계PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출Cargo인계(request);

        public 경영SimulationSessionSnapshot Confirm수출Cargo인계(
            string sessionStableId,
            Simulation수출Cargo인계ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출Cargo인계(request);

        public Simulation수출항만인수PreviewSnapshot Preview수출항만인수(
            string sessionStableId,
            Simulation수출항만인수PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출항만인수(request);

        public 경영SimulationSessionSnapshot Confirm수출항만인수(
            string sessionStableId,
            Simulation수출항만인수ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출항만인수(request);

        public Simulation수출준비성검토PreviewSnapshot Preview수출준비성검토(
            string sessionStableId,
            Simulation수출준비성검토PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출준비성검토(request);

        public 경영SimulationSessionSnapshot Confirm수출준비성검토(
            string sessionStableId,
            Simulation수출준비성검토ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출준비성검토(request);

        public Simulation수출선적계획PreviewSnapshot Preview수출선적계획(
            string sessionStableId,
            Simulation수출선적계획PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출선적계획(request);

        public 경영SimulationSessionSnapshot Confirm수출선적계획(
            string sessionStableId,
            Simulation수출선적계획ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출선적계획(request);

        public Simulation수출선적실행PreviewSnapshot Preview수출선적실행(
            string sessionStableId,
            Simulation수출선적실행PreviewRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Preview수출선적실행(request);

        public 경영SimulationSessionSnapshot Confirm수출선적실행(
            string sessionStableId,
            Simulation수출선적실행ConfirmRequest request)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Confirm수출선적실행(request);

        public Simulation수확판로결과Snapshot Get수확판로결과(
            string sessionStableId,
            string harvestLotStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Get수확판로결과(harvestLotStableId);

        public Simulation수확판로결과Snapshot[] Get수확판로결과목록(string sessionStableId)
            => (store.Find(sessionStableId)
                ?? throw new SimulationNotFoundException("SimulationSessionNotFound"))
                .Get수확판로결과목록();
    }
}
