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

}
