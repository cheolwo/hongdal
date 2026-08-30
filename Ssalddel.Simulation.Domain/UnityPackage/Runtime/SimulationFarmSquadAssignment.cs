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
        "WI-SQUAD-ASSIGN의 초소 슬롯 단일 점유와 분대 단일 배정 불변 규칙을 소유한다.",
        Boundary = "분대 출동·교전·보급·귀환은 다른 WI가 소유한다.",
        WorldInteractionIds = new[] { SimulationFarm분대배정Codes.WorldInteractionId })]
    public sealed class SimulationFarm분대배정Aggregate
    {
        private sealed class Applied { public string Payload = string.Empty; public SimulationFarm분대배정ConfirmResult Result = new SimulationFarm분대배정ConfirmResult(); }
        private readonly object gate = new object();
        private readonly HashSet<string> squads = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> slots = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationFarm분대배정Snapshot> assignmentsBySlot = new Dictionary<string, SimulationFarm분대배정Snapshot>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> slotBySquad = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, Applied> commands = new Dictionary<string, Applied>(StringComparer.Ordinal);
        private readonly Simulation행위발현Ledger actionLedger;

        public SimulationFarm분대배정Aggregate(SimulationFarm분대배정InitialStateRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmSquadAssignmentInitialStateRequired");
            WorldStableId = Require(request.WorldStableId, "FarmSquadAssignmentWorldStableIdInvalid");
            SessionStableId = Require(request.SessionStableId, "FarmSquadAssignmentSessionStableIdInvalid");
            if (request.InitialWorldRevision < 0) throw new SimulationContractException("FarmSquadAssignmentInitialWorldRevisionInvalid");
            WorldRevision = request.InitialWorldRevision;
            foreach (var squad in request.SquadStableIds ?? Array.Empty<string>())
                if (!squads.Add(Require(squad, "FarmSquadAssignmentSquadStableIdInvalid"))) throw new SimulationContractException("FarmSquadAssignmentSquadDuplicate");
            foreach (var outpost in request.Outposts ?? Array.Empty<SimulationFarm경비초소Definition>())
            {
                if (outpost == null) throw new SimulationContractException("FarmSquadAssignmentOutpostDefinitionRequired");
                var outpostId = Require(outpost.OutpostStableId, "FarmSquadAssignmentOutpostStableIdInvalid");
                if (slots.ContainsKey(outpostId)) throw new SimulationContractException("FarmSquadAssignmentOutpostDuplicate");
                var values = new HashSet<string>(StringComparer.Ordinal);
                foreach (var slot in outpost.SlotStableIds ?? Array.Empty<string>())
                    if (!values.Add(Require(slot, "FarmSquadAssignmentSlotStableIdInvalid"))) throw new SimulationContractException("FarmSquadAssignmentSlotDuplicate");
                slots.Add(outpostId, values);
            }
            actionLedger = new Simulation행위발현Ledger(WorldStableId);
        }

        public string WorldStableId { get; }
        public string SessionStableId { get; }
        public long WorldRevision { get; private set; }
        public SimulationFarm분대배정LedgerSnapshot Snapshot() { lock (gate) return CreateSnapshot(); }
        public SimulationFarm분대배정PreviewSnapshot Preview(SimulationFarm분대배정PreviewRequest request) { Validate(request); lock (gate) return CreatePreview(request); }

        public SimulationFarm분대배정ConfirmResult Confirm(SimulationFarm분대배정ConfirmRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmSquadAssignmentConfirmRequired");
            var commandId = Require(request.CommandId, "FarmSquadAssignmentCommandIdInvalid");
            Validate(new SimulationFarm분대배정PreviewRequest { ObservedWorldRevision = request.ExpectedWorldRevision, OutpostStableId = request.OutpostStableId, SlotStableId = request.SlotStableId, SquadStableId = request.SquadStableId });
            lock (gate)
            {
                var payload = Key(request.OutpostStableId, request.SlotStableId, request.SquadStableId);
                if (commands.TryGetValue(commandId, out var applied))
                {
                    if (applied.Payload != payload) throw new SimulationConflictException(SimulationFarm분대배정Codes.CommandPayloadConflict);
                    return Clone(applied.Result, true);
                }
                var preview = CreatePreview(new SimulationFarm분대배정PreviewRequest { ObservedWorldRevision = request.ExpectedWorldRevision, OutpostStableId = request.OutpostStableId, SlotStableId = request.SlotStableId, SquadStableId = request.SquadStableId });
                if (preview.BlockReasonCodes.Length > 0) throw new SimulationConflictException(preview.BlockReasonCodes[0]);
                var before = WorldRevision;
                var assignment = new SimulationFarm분대배정Snapshot { OutpostStableId = preview.OutpostStableId, SlotStableId = preview.SlotStableId, SquadStableId = preview.SquadStableId };
                assignmentsBySlot.Add(SlotKey(preview.OutpostStableId, preview.SlotStableId), assignment);
                slotBySquad.Add(preview.SquadStableId, SlotKey(preview.OutpostStableId, preview.SlotStableId));
                WorldRevision++;
                var action = actionLedger.Append(new Simulation행위발현Record {
                    WorldStableId = WorldStableId, SessionStableId = SessionStableId,
                    PlayableLoopStableId = SimulationFarm분대배정Codes.PlayableLoopStableId,
                    WorldInteractionId = SimulationFarm분대배정Codes.WorldInteractionId,
                    CommandId = commandId, TriggerSourceCode = "PlayerDriven", InitiatorStableId = "actor:host-player",
                    ActorStableId = preview.SquadStableId, ActorKindCode = "NpcSquad",
                    TargetStableIds = new[] { preview.OutpostStableId, preview.SlotStableId },
                    OutcomeStableId = "outcome:farm-defense:squad-assigned:" + commandId,
                    PrimaryOutcomeCode = "FarmDefenseSquadAssigned", 결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = new[] { Simulation행위변화의미Codes.Actor상태변경 },
                    BeforeWorldRevision = before, AfterWorldRevision = WorldRevision, RuleRevision = SimulationFarm분대배정Codes.RuleRevision });
                var result = new SimulationFarm분대배정ConfirmResult { Ledger = CreateSnapshot(), ActionRecord = action };
                commands.Add(commandId, new Applied { Payload = payload, Result = Clone(result, false) });
                return result;
            }
        }

        private SimulationFarm분대배정PreviewSnapshot CreatePreview(SimulationFarm분대배정PreviewRequest request)
        {
            var outpost = request.OutpostStableId.Trim(); var slot = request.SlotStableId.Trim(); var squad = request.SquadStableId.Trim();
            var blockers = new List<string>();
            if (request.ObservedWorldRevision != WorldRevision) blockers.Add(SimulationFarm분대배정Codes.ExpectedRevisionMismatch);
            if (!slots.TryGetValue(outpost, out var outpostSlots)) blockers.Add(SimulationFarm분대배정Codes.OutpostUnknown);
            else if (!outpostSlots.Contains(slot)) blockers.Add(SimulationFarm분대배정Codes.SlotUnknown);
            if (!squads.Contains(squad)) blockers.Add(SimulationFarm분대배정Codes.SquadUnknown);
            if (slotBySquad.ContainsKey(squad)) blockers.Add(SimulationFarm분대배정Codes.SquadAlreadyAssigned);
            if (assignmentsBySlot.ContainsKey(SlotKey(outpost, slot))) blockers.Add(SimulationFarm분대배정Codes.SlotOccupied);
            return new SimulationFarm분대배정PreviewSnapshot { ObservedWorldRevision = request.ObservedWorldRevision, OutpostStableId = outpost, SlotStableId = slot, SquadStableId = squad, CanConfirm = blockers.Count == 0, BlockReasonCodes = blockers.ToArray() };
        }

        private SimulationFarm분대배정LedgerSnapshot CreateSnapshot()
        {
            var value = new SimulationFarm분대배정LedgerSnapshot { WorldStableId = WorldStableId, SessionStableId = SessionStableId, WorldRevision = WorldRevision,
                SquadStableIds = squads.OrderBy(x => x, StringComparer.Ordinal).ToArray(),
                Slots = slots.OrderBy(x => x.Key, StringComparer.Ordinal).SelectMany(x => x.Value.OrderBy(y => y, StringComparer.Ordinal).Select(y => new SimulationFarm초소SlotSnapshot { OutpostStableId = x.Key, SlotStableId = y })).ToArray(),
                Assignments = assignmentsBySlot.Values.OrderBy(x => x.OutpostStableId, StringComparer.Ordinal).ThenBy(x => x.SlotStableId, StringComparer.Ordinal).Select(x => new SimulationFarm분대배정Snapshot { OutpostStableId = x.OutpostStableId, SlotStableId = x.SlotStableId, SquadStableId = x.SquadStableId }).ToArray(),
                ActionLedger = actionLedger.Snapshot() };
            var canonical = string.Join("\n", value.RuleRevision, value.WorldStableId, value.SessionStableId, value.WorldRevision.ToString(CultureInfo.InvariantCulture), string.Join("|", value.SquadStableIds), string.Join("|", value.Slots.Select(x => SlotKey(x.OutpostStableId, x.SlotStableId))), string.Join("|", value.Assignments.Select(x => Key(x.OutpostStableId, x.SlotStableId, x.SquadStableId))), value.ActionLedger.StateHashSha256);
            using var sha = SHA256.Create(); value.StateHashSha256 = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical))).Replace("-", string.Empty).ToLowerInvariant(); return value;
        }

        private static void Validate(SimulationFarm분대배정PreviewRequest request) { if (request == null) throw new SimulationContractException("FarmSquadAssignmentPreviewRequired"); Require(request.OutpostStableId, "FarmSquadAssignmentOutpostStableIdInvalid"); Require(request.SlotStableId, "FarmSquadAssignmentSlotStableIdInvalid"); Require(request.SquadStableId, "FarmSquadAssignmentSquadStableIdInvalid"); }
        private static string SlotKey(string outpost, string slot) => Require(outpost, "FarmSquadAssignmentOutpostStableIdInvalid") + "|" + Require(slot, "FarmSquadAssignmentSlotStableIdInvalid");
        private static string Key(string outpost, string slot, string squad) => SlotKey(outpost, slot) + "|" + Require(squad, "FarmSquadAssignmentSquadStableIdInvalid");
        private static string Require(string value, string error) { if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException(error); return value.Trim(); }
        private static SimulationFarm분대배정ConfirmResult Clone(SimulationFarm분대배정ConfirmResult source, bool reused) => new SimulationFarm분대배정ConfirmResult { Ledger = source.Ledger, ActionRecord = source.ActionRecord, Reused = reused };
    }
}
