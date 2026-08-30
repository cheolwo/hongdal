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
        "전투 권위가 확정한 Farm 방어 성공 결과를 위협·안전 기간·생산/회복 보정·전리품에 한 번만 발현한다.",
        Boundary = "승패와 결과 수치를 계산하지 않으며 부상·치료·귀환·생산 재합류를 변경하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm방위결과Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위결과Aggregate
    {
        private sealed class State
        {
            public string EncounterStableId = string.Empty;
            public string SquadStableId = string.Empty;
            public bool DefenseSucceeded;
            public int ThreatReductionUnits;
            public long SafeUntilWorldTick;
            public int ProductionModifierMilli;
            public int RecoveryModifierMilli;
            public SimulationFarm방위전리품Definition[] Loot = Array.Empty<SimulationFarm방위전리품Definition>();
            public bool IsResolved;
        }

        private sealed class Applied
        {
            public string Payload = string.Empty;
            public SimulationFarm방위결과ConfirmResult Result = new SimulationFarm방위결과ConfirmResult();
        }

        private readonly object gate = new object();
        private readonly Dictionary<string, State> results = new Dictionary<string, State>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> lootInventory = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, Applied> commands = new Dictionary<string, Applied>(StringComparer.Ordinal);
        private readonly Simulation행위발현Ledger actionLedger;

        public SimulationFarm방위결과Aggregate(SimulationFarm방위결과InitialStateRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmDefenseResolutionInitialStateRequired");
            WorldStableId = Require(request.WorldStableId, "FarmDefenseResolutionWorldStableIdInvalid");
            SessionStableId = Require(request.SessionStableId, "FarmDefenseResolutionSessionStableIdInvalid");
            if (request.InitialWorldRevision < 0) throw new SimulationContractException("FarmDefenseResolutionInitialWorldRevisionInvalid");
            if (request.CurrentWorldTick < 0) throw new SimulationContractException("FarmDefenseResolutionCurrentWorldTickInvalid");
            if (request.ThreatUnits < 0) throw new SimulationContractException("FarmDefenseResolutionThreatUnitsInvalid");
            if (request.ProductionModifierMilli < 0 || request.RecoveryModifierMilli < 0)
                throw new SimulationContractException("FarmDefenseResolutionModifierInvalid");
            WorldRevision = request.InitialWorldRevision;
            CurrentWorldTick = request.CurrentWorldTick;
            ThreatUnits = request.ThreatUnits;
            ProductionModifierMilli = request.ProductionModifierMilli;
            RecoveryModifierMilli = request.RecoveryModifierMilli;

            foreach (var definition in request.PendingResults ?? Array.Empty<SimulationFarm방위확정결과Definition>())
            {
                if (definition == null) throw new SimulationContractException("FarmDefenseResolutionDefinitionRequired");
                var encounterId = Require(definition.EncounterStableId, "FarmDefenseResolutionEncounterStableIdInvalid");
                var squadId = Require(definition.SquadStableId, "FarmDefenseResolutionSquadStableIdInvalid");
                if (definition.ThreatReductionUnits < 0 || definition.SafeUntilWorldTick < CurrentWorldTick ||
                    definition.ProductionModifierMilli < 0 || definition.RecoveryModifierMilli < 0)
                    throw new SimulationContractException("FarmDefenseResolutionDefinitionValueInvalid");
                var loot = NormalizeLoot(definition.Loot);
                if (!results.TryAdd(encounterId, new State
                    {
                        EncounterStableId = encounterId,
                        SquadStableId = squadId,
                        DefenseSucceeded = definition.DefenseSucceeded,
                        ThreatReductionUnits = definition.ThreatReductionUnits,
                        SafeUntilWorldTick = definition.SafeUntilWorldTick,
                        ProductionModifierMilli = definition.ProductionModifierMilli,
                        RecoveryModifierMilli = definition.RecoveryModifierMilli,
                        Loot = loot
                    })) throw new SimulationContractException("FarmDefenseResolutionEncounterDuplicate");
            }
            actionLedger = new Simulation행위발현Ledger(WorldStableId);
        }

        public string WorldStableId { get; }
        public string SessionStableId { get; }
        public long WorldRevision { get; private set; }
        public long CurrentWorldTick { get; }
        public int ThreatUnits { get; private set; }
        public long SafeUntilWorldTick { get; private set; }
        public int ProductionModifierMilli { get; private set; }
        public int RecoveryModifierMilli { get; private set; }

        public SimulationFarm방위결과LedgerSnapshot Snapshot() { lock (gate) return CreateSnapshot(); }

        public SimulationFarm방위결과PreviewSnapshot Preview(SimulationFarm방위결과PreviewRequest request)
        {
            Validate(request);
            lock (gate) return CreatePreview(request);
        }

        public SimulationFarm방위결과ConfirmResult Confirm(SimulationFarm방위결과ConfirmRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmDefenseResolutionConfirmRequired");
            var commandId = Require(request.CommandId, "FarmDefenseResolutionCommandIdInvalid");
            var encounterId = Require(request.EncounterStableId, "FarmDefenseResolutionEncounterStableIdInvalid");
            lock (gate)
            {
                if (commands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.Payload, encounterId, StringComparison.Ordinal))
                        throw new SimulationConflictException(SimulationFarm방위결과Codes.CommandPayloadConflict);
                    return Clone(applied.Result, true);
                }

                var preview = CreatePreview(new SimulationFarm방위결과PreviewRequest
                    { ObservedWorldRevision = request.ExpectedWorldRevision, EncounterStableId = encounterId });
                if (preview.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException(preview.BlockReasonCodes[0]);

                var state = results[encounterId];
                var before = WorldRevision;
                ThreatUnits = Math.Max(0, ThreatUnits - state.ThreatReductionUnits);
                SafeUntilWorldTick = Math.Max(SafeUntilWorldTick, state.SafeUntilWorldTick);
                ProductionModifierMilli = state.ProductionModifierMilli;
                RecoveryModifierMilli = state.RecoveryModifierMilli;
                foreach (var loot in state.Loot)
                    lootInventory[loot.ItemStableId] = lootInventory.TryGetValue(loot.ItemStableId, out var quantity)
                        ? checked(quantity + loot.Quantity)
                        : loot.Quantity;
                state.IsResolved = true;
                WorldRevision++;

                var action = actionLedger.Append(new Simulation행위발현Record
                {
                    WorldStableId = WorldStableId,
                    SessionStableId = SessionStableId,
                    PlayableLoopStableId = SimulationFarm방위결과Codes.PlayableLoopStableId,
                    WorldInteractionId = SimulationFarm방위결과Codes.WorldInteractionId,
                    CommandId = commandId,
                    TriggerSourceCode = "WorldDerived",
                    InitiatorStableId = "rule:farm-defense-result",
                    ActorStableId = state.SquadStableId,
                    ActorKindCode = "NpcSquad",
                    TargetStableIds = new[] { encounterId, "state:farm-threat", "inventory:farm-defense-loot" },
                    OutcomeStableId = "outcome:farm-defense:resolved:" + encounterId,
                    PrimaryOutcomeCode = "FarmDefenseResolved",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    BattleOutcomeStableId = encounterId,
                    변화의미Codes = new[] { Simulation행위변화의미Codes.Actor상태변경, Simulation행위변화의미Codes.재고변경 },
                    BeforeWorldRevision = before,
                    AfterWorldRevision = WorldRevision,
                    RuleRevision = SimulationFarm방위결과Codes.RuleRevision
                });
                var result = new SimulationFarm방위결과ConfirmResult { Ledger = CreateSnapshot(), ActionRecord = action };
                commands.Add(commandId, new Applied { Payload = encounterId, Result = Clone(result, false) });
                return result;
            }
        }

        private SimulationFarm방위결과PreviewSnapshot CreatePreview(SimulationFarm방위결과PreviewRequest request)
        {
            var encounterId = request.EncounterStableId.Trim();
            var blockers = new List<string>();
            if (request.ObservedWorldRevision != WorldRevision) blockers.Add(SimulationFarm방위결과Codes.ExpectedRevisionMismatch);
            if (!results.TryGetValue(encounterId, out var state)) blockers.Add(SimulationFarm방위결과Codes.EncounterUnknown);
            else
            {
                if (state.IsResolved) blockers.Add(SimulationFarm방위결과Codes.EncounterAlreadyResolved);
                if (!state.DefenseSucceeded) blockers.Add(SimulationFarm방위결과Codes.ResultNotSuccessful);
            }
            return new SimulationFarm방위결과PreviewSnapshot
            {
                ObservedWorldRevision = request.ObservedWorldRevision,
                EncounterStableId = encounterId,
                SquadStableId = state?.SquadStableId ?? string.Empty,
                ThreatReductionUnits = state?.ThreatReductionUnits ?? 0,
                SafeUntilWorldTick = state?.SafeUntilWorldTick ?? 0,
                LootLineCount = state?.Loot.Length ?? 0,
                CanConfirm = blockers.Count == 0,
                BlockReasonCodes = blockers.ToArray()
            };
        }

        private SimulationFarm방위결과LedgerSnapshot CreateSnapshot()
        {
            var value = new SimulationFarm방위결과LedgerSnapshot
            {
                WorldStableId = WorldStableId,
                SessionStableId = SessionStableId,
                WorldRevision = WorldRevision,
                CurrentWorldTick = CurrentWorldTick,
                ThreatUnits = ThreatUnits,
                SafeUntilWorldTick = SafeUntilWorldTick,
                ProductionModifierMilli = ProductionModifierMilli,
                RecoveryModifierMilli = RecoveryModifierMilli,
                LootInventory = lootInventory.OrderBy(x => x.Key, StringComparer.Ordinal)
                    .Select(x => new SimulationFarm방위전리품Definition { ItemStableId = x.Key, Quantity = x.Value }).ToArray(),
                Results = results.Values.OrderBy(x => x.EncounterStableId, StringComparer.Ordinal)
                    .Select(x => new SimulationFarm방위결과Snapshot
                    {
                        EncounterStableId = x.EncounterStableId,
                        SquadStableId = x.SquadStableId,
                        ThreatReductionUnits = x.ThreatReductionUnits,
                        SafeUntilWorldTick = x.SafeUntilWorldTick,
                        ProductionModifierMilli = x.ProductionModifierMilli,
                        RecoveryModifierMilli = x.RecoveryModifierMilli,
                        Loot = x.Loot.Select(CloneLoot).ToArray(),
                        IsResolved = x.IsResolved
                    }).ToArray(),
                ActionLedger = actionLedger.Snapshot()
            };
            var canonical = string.Join("\n", value.RuleRevision, value.WorldStableId, value.SessionStableId,
                value.WorldRevision.ToString(CultureInfo.InvariantCulture), value.CurrentWorldTick.ToString(CultureInfo.InvariantCulture),
                value.ThreatUnits.ToString(CultureInfo.InvariantCulture), value.SafeUntilWorldTick.ToString(CultureInfo.InvariantCulture),
                value.ProductionModifierMilli.ToString(CultureInfo.InvariantCulture), value.RecoveryModifierMilli.ToString(CultureInfo.InvariantCulture),
                string.Join("|", value.LootInventory.Select(x => x.ItemStableId + ":" + x.Quantity.ToString(CultureInfo.InvariantCulture))),
                string.Join("|", value.Results.Select(x => string.Join(":", x.EncounterStableId, x.SquadStableId,
                    x.ThreatReductionUnits.ToString(CultureInfo.InvariantCulture), x.SafeUntilWorldTick.ToString(CultureInfo.InvariantCulture),
                    x.ProductionModifierMilli.ToString(CultureInfo.InvariantCulture), x.RecoveryModifierMilli.ToString(CultureInfo.InvariantCulture),
                    x.IsResolved ? "1" : "0", string.Join(",", x.Loot.Select(y => y.ItemStableId + "=" + y.Quantity.ToString(CultureInfo.InvariantCulture)))))),
                value.ActionLedger.StateHashSha256);
            using var sha = SHA256.Create();
            value.StateHashSha256 = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
            return value;
        }

        private static SimulationFarm방위전리품Definition[] NormalizeLoot(SimulationFarm방위전리품Definition[]? source)
        {
            var values = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var item in source ?? Array.Empty<SimulationFarm방위전리품Definition>())
            {
                if (item == null) throw new SimulationContractException("FarmDefenseResolutionLootRequired");
                var id = Require(item.ItemStableId, "FarmDefenseResolutionLootItemStableIdInvalid");
                if (item.Quantity <= 0) throw new SimulationContractException("FarmDefenseResolutionLootQuantityInvalid");
                if (!values.TryAdd(id, item.Quantity)) throw new SimulationContractException("FarmDefenseResolutionLootDuplicate");
            }
            return values.OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => new SimulationFarm방위전리품Definition { ItemStableId = x.Key, Quantity = x.Value }).ToArray();
        }

        private static SimulationFarm방위전리품Definition CloneLoot(SimulationFarm방위전리품Definition value)
            => new SimulationFarm방위전리품Definition { ItemStableId = value.ItemStableId, Quantity = value.Quantity };

        private static void Validate(SimulationFarm방위결과PreviewRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmDefenseResolutionPreviewRequired");
            Require(request.EncounterStableId, "FarmDefenseResolutionEncounterStableIdInvalid");
        }

        private static string Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException(error);
            return value.Trim();
        }

        private static SimulationFarm방위결과ConfirmResult Clone(SimulationFarm방위결과ConfirmResult source, bool reused)
            => new SimulationFarm방위결과ConfirmResult { Ledger = source.Ledger, ActionRecord = source.ActionRecord, Reused = reused };
    }
}
