using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "WI-SQUAD-SUPPLY의 식량·장비 내구도 자원 동시 충족 불변 규칙을 소유한다.",
        Boundary = "한 분대의 준비 상태만 바꾸며 개별 장비·훈련·출동·전투를 변경하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm분대보급Codes.WorldInteractionId })]
    public sealed class SimulationFarm분대보급Aggregate
    {
        private sealed class State
        {
            public string SquadStableId = string.Empty;
            public int RequiredFoodUnits;
            public int RequiredDurabilityRestoreUnits;
            public bool IsSupplied;
        }

        private sealed class Applied
        {
            public string Payload = string.Empty;
            public SimulationFarm분대보급ConfirmResult Result = new SimulationFarm분대보급ConfirmResult();
        }

        private readonly object gate = new object();
        private readonly Dictionary<string, State> squads = new Dictionary<string, State>(StringComparer.Ordinal);
        private readonly Dictionary<string, Applied> commands = new Dictionary<string, Applied>(StringComparer.Ordinal);
        private readonly Simulation행위발현Ledger actionLedger;

        public SimulationFarm분대보급Aggregate(SimulationFarm분대보급InitialStateRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmSquadSupplyInitialStateRequired");
            WorldStableId = Require(request.WorldStableId, "FarmSquadSupplyWorldStableIdInvalid");
            SessionStableId = Require(request.SessionStableId, "FarmSquadSupplySessionStableIdInvalid");
            if (request.InitialWorldRevision < 0) throw new SimulationContractException("FarmSquadSupplyInitialWorldRevisionInvalid");
            if (request.FoodStockUnits < 0) throw new SimulationContractException("FarmSquadSupplyFoodStockInvalid");
            if (request.DurabilityRestoreCapacityUnits < 0) throw new SimulationContractException("FarmSquadSupplyDurabilityCapacityInvalid");
            WorldRevision = request.InitialWorldRevision;
            FoodStockUnits = request.FoodStockUnits;
            DurabilityRestoreCapacityUnits = request.DurabilityRestoreCapacityUnits;
            foreach (var requirement in request.SquadRequirements ?? Array.Empty<SimulationFarm분대보급Requirement>())
            {
                if (requirement == null) throw new SimulationContractException("FarmSquadSupplyRequirementRequired");
                var squadId = Require(requirement.SquadStableId, "FarmSquadSupplySquadStableIdInvalid");
                if (requirement.RequiredFoodUnits < 0 || requirement.RequiredDurabilityRestoreUnits < 0)
                    throw new SimulationContractException("FarmSquadSupplyRequirementAmountInvalid");
                if (!squads.TryAdd(squadId, new State
                    {
                        SquadStableId = squadId,
                        RequiredFoodUnits = requirement.RequiredFoodUnits,
                        RequiredDurabilityRestoreUnits = requirement.RequiredDurabilityRestoreUnits
                    })) throw new SimulationContractException("FarmSquadSupplySquadDuplicate");
            }
            actionLedger = new Simulation행위발현Ledger(WorldStableId);
        }

        public string WorldStableId { get; }
        public string SessionStableId { get; }
        public long WorldRevision { get; private set; }
        public int FoodStockUnits { get; private set; }
        public int DurabilityRestoreCapacityUnits { get; private set; }

        public SimulationFarm분대보급LedgerSnapshot Snapshot() { lock (gate) return CreateSnapshot(); }
        public SimulationFarm분대보급PreviewSnapshot Preview(SimulationFarm분대보급PreviewRequest request)
        {
            Validate(request);
            lock (gate) return CreatePreview(request);
        }

        public SimulationFarm분대보급ConfirmResult Confirm(SimulationFarm분대보급ConfirmRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmSquadSupplyConfirmRequired");
            var commandId = Require(request.CommandId, "FarmSquadSupplyCommandIdInvalid");
            var squadId = Require(request.SquadStableId, "FarmSquadSupplySquadStableIdInvalid");
            lock (gate)
            {
                if (commands.TryGetValue(commandId, out var applied))
                {
                    if (applied.Payload != squadId) throw new SimulationConflictException(SimulationFarm분대보급Codes.CommandPayloadConflict);
                    return Clone(applied.Result, true);
                }
                var preview = CreatePreview(new SimulationFarm분대보급PreviewRequest
                    { ObservedWorldRevision = request.ExpectedWorldRevision, SquadStableId = squadId });
                if (preview.BlockReasonCodes.Length > 0) throw new SimulationConflictException(preview.BlockReasonCodes[0]);
                var state = squads[squadId];
                var before = WorldRevision;
                FoodStockUnits -= state.RequiredFoodUnits;
                DurabilityRestoreCapacityUnits -= state.RequiredDurabilityRestoreUnits;
                state.IsSupplied = true;
                WorldRevision++;
                var action = actionLedger.Append(new Simulation행위발현Record
                {
                    WorldStableId = WorldStableId,
                    SessionStableId = SessionStableId,
                    PlayableLoopStableId = SimulationFarm분대보급Codes.PlayableLoopStableId,
                    WorldInteractionId = SimulationFarm분대보급Codes.WorldInteractionId,
                    CommandId = commandId,
                    TriggerSourceCode = "PlayerDriven",
                    InitiatorStableId = "actor:host-player",
                    ActorStableId = squadId,
                    ActorKindCode = "NpcSquad",
                    TargetStableIds = new[] { "inventory:farm-defense-food", "resource:farm-defense-durability-restore" },
                    OutcomeStableId = "outcome:farm-defense:squad-supplied:" + commandId,
                    PrimaryOutcomeCode = "FarmDefenseSquadSupplied",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = new[] { Simulation행위변화의미Codes.재고변경, Simulation행위변화의미Codes.Actor상태변경 },
                    BeforeWorldRevision = before,
                    AfterWorldRevision = WorldRevision,
                    RuleRevision = SimulationFarm분대보급Codes.RuleRevision
                });
                var result = new SimulationFarm분대보급ConfirmResult { Ledger = CreateSnapshot(), ActionRecord = action };
                commands.Add(commandId, new Applied { Payload = squadId, Result = Clone(result, false) });
                return result;
            }
        }

        private SimulationFarm분대보급PreviewSnapshot CreatePreview(SimulationFarm분대보급PreviewRequest request)
        {
            var squadId = request.SquadStableId.Trim();
            var blockers = new List<string>();
            if (request.ObservedWorldRevision != WorldRevision) blockers.Add(SimulationFarm분대보급Codes.ExpectedRevisionMismatch);
            if (!squads.TryGetValue(squadId, out var state)) blockers.Add(SimulationFarm분대보급Codes.SquadUnknown);
            else
            {
                if (state.IsSupplied) blockers.Add(SimulationFarm분대보급Codes.SquadAlreadySupplied);
                if (FoodStockUnits < state.RequiredFoodUnits) blockers.Add(SimulationFarm분대보급Codes.FoodInsufficient);
                if (DurabilityRestoreCapacityUnits < state.RequiredDurabilityRestoreUnits) blockers.Add(SimulationFarm분대보급Codes.DurabilityRestoreInsufficient);
            }
            return new SimulationFarm분대보급PreviewSnapshot
            {
                ObservedWorldRevision = request.ObservedWorldRevision,
                SquadStableId = squadId,
                RequiredFoodUnits = state?.RequiredFoodUnits ?? 0,
                RequiredDurabilityRestoreUnits = state?.RequiredDurabilityRestoreUnits ?? 0,
                AvailableFoodUnits = FoodStockUnits,
                AvailableDurabilityRestoreUnits = DurabilityRestoreCapacityUnits,
                CanConfirm = blockers.Count == 0,
                BlockReasonCodes = blockers.ToArray()
            };
        }

        private SimulationFarm분대보급LedgerSnapshot CreateSnapshot()
        {
            var value = new SimulationFarm분대보급LedgerSnapshot
            {
                WorldStableId = WorldStableId,
                SessionStableId = SessionStableId,
                WorldRevision = WorldRevision,
                FoodStockUnits = FoodStockUnits,
                DurabilityRestoreCapacityUnits = DurabilityRestoreCapacityUnits,
                Squads = squads.Values.OrderBy(x => x.SquadStableId, StringComparer.Ordinal).Select(x => new SimulationFarm분대보급StateSnapshot
                    {
                        SquadStableId = x.SquadStableId,
                        RequiredFoodUnits = x.RequiredFoodUnits,
                        RequiredDurabilityRestoreUnits = x.RequiredDurabilityRestoreUnits,
                        IsSupplied = x.IsSupplied
                    }).ToArray(),
                ActionLedger = actionLedger.Snapshot()
            };
            var canonical = string.Join("\n", value.RuleRevision, value.WorldStableId, value.SessionStableId,
                value.WorldRevision.ToString(CultureInfo.InvariantCulture), value.FoodStockUnits.ToString(CultureInfo.InvariantCulture),
                value.DurabilityRestoreCapacityUnits.ToString(CultureInfo.InvariantCulture),
                string.Join("|", value.Squads.Select(x => string.Join(":", x.SquadStableId,
                    x.RequiredFoodUnits.ToString(CultureInfo.InvariantCulture),
                    x.RequiredDurabilityRestoreUnits.ToString(CultureInfo.InvariantCulture), x.IsSupplied ? "1" : "0"))),
                value.ActionLedger.StateHashSha256);
            using var sha = SHA256.Create();
            value.StateHashSha256 = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
            return value;
        }

        private static void Validate(SimulationFarm분대보급PreviewRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmSquadSupplyPreviewRequired");
            Require(request.SquadStableId, "FarmSquadSupplySquadStableIdInvalid");
        }

        private static string Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException(error);
            return value.Trim();
        }

        private static SimulationFarm분대보급ConfirmResult Clone(SimulationFarm분대보급ConfirmResult source, bool reused)
            => new SimulationFarm분대보급ConfirmResult { Ledger = source.Ledger, ActionRecord = source.ActionRecord, Reused = reused };
    }
}
