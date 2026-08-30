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
        "결과 확정 분대의 초소 귀환과 치료·생산 재합류 후속 대기열 인계를 한 번만 수행한다.",
        Boundary = "치료·휴식·생산 재합류와 전리품 지급을 실행하지 않는다.",
        WorldInteractionIds = new[] { SimulationFarm방위귀환Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위귀환Aggregate
    {
        private sealed class State
        {
            public string ReturnStableId = string.Empty;
            public string EncounterStableId = string.Empty;
            public string SquadStableId = string.Empty;
            public string OutpostStableId = string.Empty;
            public bool ResultResolved;
            public bool IsReturned;
            public string[] Treatment = Array.Empty<string>();
            public string[] Rejoin = Array.Empty<string>();
        }
        private sealed class Applied { public string Payload = string.Empty; public SimulationFarm방위귀환ConfirmResult Result = new(); }

        private readonly object gate = new();
        private readonly Dictionary<string, State> returns = new(StringComparer.Ordinal);
        private readonly Dictionary<string, Applied> commands = new(StringComparer.Ordinal);
        private readonly SortedSet<string> pendingTreatment = new(StringComparer.Ordinal);
        private readonly SortedSet<string> pendingRejoin = new(StringComparer.Ordinal);
        private readonly Simulation행위발현Ledger actionLedger;

        public SimulationFarm방위귀환Aggregate(SimulationFarm방위귀환InitialStateRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmDefenseReturnInitialStateRequired");
            WorldStableId = Require(request.WorldStableId, "FarmDefenseReturnWorldStableIdInvalid");
            SessionStableId = Require(request.SessionStableId, "FarmDefenseReturnSessionStableIdInvalid");
            if (request.InitialWorldRevision < 0) throw new SimulationContractException("FarmDefenseReturnInitialWorldRevisionInvalid");
            WorldRevision = request.InitialWorldRevision;
            foreach (var definition in request.Returns ?? Array.Empty<SimulationFarm방위귀환Definition>())
            {
                if (definition == null) throw new SimulationContractException("FarmDefenseReturnDefinitionRequired");
                var id = Require(definition.ReturnStableId, "FarmDefenseReturnStableIdInvalid");
                var state = new State
                {
                    ReturnStableId = id,
                    EncounterStableId = Require(definition.EncounterStableId, "FarmDefenseReturnEncounterStableIdInvalid"),
                    SquadStableId = Require(definition.SquadStableId, "FarmDefenseReturnSquadStableIdInvalid"),
                    OutpostStableId = Require(definition.OutpostStableId, "FarmDefenseReturnOutpostStableIdInvalid"),
                    ResultResolved = definition.ResultResolved,
                    Treatment = NormalizeIds(definition.TreatmentRequiredActorStableIds, "FarmDefenseReturnTreatmentActorInvalid"),
                    Rejoin = NormalizeIds(definition.ProductionRejoinCandidateActorStableIds, "FarmDefenseReturnRejoinActorInvalid")
                };
                if (state.Treatment.Intersect(state.Rejoin, StringComparer.Ordinal).Any())
                    throw new SimulationContractException("FarmDefenseReturnActorRoleOverlap");
                if (!returns.TryAdd(id, state)) throw new SimulationContractException("FarmDefenseReturnDuplicate");
            }
            actionLedger = new Simulation행위발현Ledger(WorldStableId);
        }

        public string WorldStableId { get; }
        public string SessionStableId { get; }
        public long WorldRevision { get; private set; }
        public SimulationFarm방위귀환LedgerSnapshot Snapshot() { lock (gate) return CreateSnapshot(); }
        public SimulationFarm방위귀환PreviewSnapshot Preview(SimulationFarm방위귀환PreviewRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmDefenseReturnPreviewRequired");
            Require(request.ReturnStableId, "FarmDefenseReturnStableIdInvalid");
            lock (gate) return CreatePreview(request);
        }
        public SimulationFarm방위귀환ConfirmResult Confirm(SimulationFarm방위귀환ConfirmRequest request)
        {
            if (request == null) throw new SimulationContractException("FarmDefenseReturnConfirmRequired");
            var commandId = Require(request.CommandId, "FarmDefenseReturnCommandIdInvalid");
            var returnId = Require(request.ReturnStableId, "FarmDefenseReturnStableIdInvalid");
            lock (gate)
            {
                if (commands.TryGetValue(commandId, out var applied))
                {
                    if (applied.Payload != returnId) throw new SimulationConflictException(SimulationFarm방위귀환Codes.CommandPayloadConflict);
                    return Clone(applied.Result, true);
                }
                var preview = CreatePreview(new SimulationFarm방위귀환PreviewRequest { ObservedWorldRevision = request.ExpectedWorldRevision, ReturnStableId = returnId });
                if (preview.BlockReasonCodes.Length > 0) throw new SimulationConflictException(preview.BlockReasonCodes[0]);
                var state = returns[returnId]; var before = WorldRevision;
                state.IsReturned = true;
                foreach (var actor in state.Treatment) pendingTreatment.Add(actor);
                foreach (var actor in state.Rejoin) pendingRejoin.Add(actor);
                WorldRevision++;
                var action = actionLedger.Append(new Simulation행위발현Record
                {
                    WorldStableId = WorldStableId, SessionStableId = SessionStableId,
                    PlayableLoopStableId = SimulationFarm방위귀환Codes.PlayableLoopStableId,
                    WorldInteractionId = SimulationFarm방위귀환Codes.WorldInteractionId,
                    CommandId = commandId, TriggerSourceCode = "WorldDerived", InitiatorStableId = "rule:farm-defense-return",
                    ActorStableId = state.SquadStableId, ActorKindCode = "NpcSquad",
                    TargetStableIds = new[] { state.OutpostStableId, state.EncounterStableId },
                    OutcomeStableId = "outcome:farm-defense:return:" + returnId,
                    PrimaryOutcomeCode = "FarmDefenseSquadReturned", 결과분류Code = Simulation행위결과분류Codes.후퇴복구,
                    BattleOutcomeStableId = state.EncounterStableId,
                    변화의미Codes = new[] { Simulation행위변화의미Codes.Actor상태변경 },
                    BeforeWorldRevision = before, AfterWorldRevision = WorldRevision, RuleRevision = SimulationFarm방위귀환Codes.RuleRevision
                });
                var result = new SimulationFarm방위귀환ConfirmResult { Ledger = CreateSnapshot(), ActionRecord = action };
                commands.Add(commandId, new Applied { Payload = returnId, Result = Clone(result, false) });
                return result;
            }
        }

        private SimulationFarm방위귀환PreviewSnapshot CreatePreview(SimulationFarm방위귀환PreviewRequest request)
        {
            var id = request.ReturnStableId.Trim(); var blockers = new List<string>();
            if (request.ObservedWorldRevision != WorldRevision) blockers.Add(SimulationFarm방위귀환Codes.ExpectedRevisionMismatch);
            if (!returns.TryGetValue(id, out var state)) blockers.Add(SimulationFarm방위귀환Codes.ReturnUnknown);
            else { if (!state.ResultResolved) blockers.Add(SimulationFarm방위귀환Codes.ResultNotResolved); if (state.IsReturned) blockers.Add(SimulationFarm방위귀환Codes.AlreadyReturned); }
            return new SimulationFarm방위귀환PreviewSnapshot
            {
                ObservedWorldRevision = request.ObservedWorldRevision, ReturnStableId = id,
                SquadStableId = state?.SquadStableId ?? string.Empty, OutpostStableId = state?.OutpostStableId ?? string.Empty,
                TreatmentRequiredCount = state?.Treatment.Length ?? 0, ProductionRejoinCandidateCount = state?.Rejoin.Length ?? 0,
                CanConfirm = blockers.Count == 0, BlockReasonCodes = blockers.ToArray()
            };
        }

        private SimulationFarm방위귀환LedgerSnapshot CreateSnapshot()
        {
            var value = new SimulationFarm방위귀환LedgerSnapshot
            {
                WorldStableId = WorldStableId, SessionStableId = SessionStableId, WorldRevision = WorldRevision,
                Returns = returns.Values.OrderBy(x => x.ReturnStableId, StringComparer.Ordinal).Select(x => new SimulationFarm방위귀환StateSnapshot
                {
                    ReturnStableId = x.ReturnStableId, EncounterStableId = x.EncounterStableId, SquadStableId = x.SquadStableId,
                    OutpostStableId = x.OutpostStableId, ResultResolved = x.ResultResolved, IsReturned = x.IsReturned,
                    TreatmentRequiredActorStableIds = x.Treatment.ToArray(), ProductionRejoinCandidateActorStableIds = x.Rejoin.ToArray()
                }).ToArray(),
                PendingTreatmentActorStableIds = pendingTreatment.ToArray(), PendingProductionRejoinActorStableIds = pendingRejoin.ToArray(),
                ActionLedger = actionLedger.Snapshot()
            };
            var canonical = string.Join("\n", value.RuleRevision, value.WorldStableId, value.SessionStableId, value.WorldRevision.ToString(CultureInfo.InvariantCulture),
                string.Join("|", value.Returns.Select(x => string.Join(":", x.ReturnStableId, x.EncounterStableId, x.SquadStableId, x.OutpostStableId, x.ResultResolved ? "1" : "0", x.IsReturned ? "1" : "0", string.Join(",", x.TreatmentRequiredActorStableIds), string.Join(",", x.ProductionRejoinCandidateActorStableIds)))),
                string.Join("|", value.PendingTreatmentActorStableIds), string.Join("|", value.PendingProductionRejoinActorStableIds), value.ActionLedger.StateHashSha256);
            using var sha = SHA256.Create();
            value.StateHashSha256 = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical))).Replace("-", string.Empty).ToLowerInvariant();
            return value;
        }
        private static string[] NormalizeIds(string[]? values, string error)
        {
            var result = new SortedSet<string>(StringComparer.Ordinal);
            foreach (var value in values ?? Array.Empty<string>()) if (!result.Add(Require(value, error))) throw new SimulationContractException(error);
            return result.ToArray();
        }
        private static string Require(string value, string error) { if (string.IsNullOrWhiteSpace(value)) throw new SimulationContractException(error); return value.Trim(); }
        private static SimulationFarm방위귀환ConfirmResult Clone(SimulationFarm방위귀환ConfirmResult source, bool reused) => new() { Ledger = source.Ledger, ActionRecord = source.ActionRecord, Reused = reused };
    }
}
