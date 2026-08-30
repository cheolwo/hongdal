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
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "WI-FARM-DEFENSE-MOBILIZE의 분대 출동과 생산 기여 중단 불변 규칙을 소유한다.",
        Boundary = "방어 결과·전리품·부상·치료·귀환은 별도 WI가 소유한다.",
        WorldInteractionIds = new[] { SimulationFarm방위소집Codes.WorldInteractionId })]
    public sealed class SimulationFarm방위소집Aggregate
    {
        private sealed class SquadState
        {
            public string SquadStableId = string.Empty;
            public string[] WorkerStableIds = Array.Empty<string>();
            public bool IsReady;
            public string StatusCode = SimulationFarm방위소집Codes.대기;
            public string ThreatStableId = string.Empty;
        }

        private sealed class AppliedCommand
        {
            public string PayloadKey = string.Empty;
            public SimulationFarm방위소집ConfirmResult Result =
                new SimulationFarm방위소집ConfirmResult();
        }

        private readonly object gate = new object();
        private readonly Dictionary<string, SquadState> squads =
            new Dictionary<string, SquadState>(StringComparer.Ordinal);
        private readonly HashSet<string> approachingThreats =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> suspendedWorkers =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedCommand> commands =
            new Dictionary<string, AppliedCommand>(StringComparer.Ordinal);
        private readonly Simulation행위발현Ledger actionLedger;

        public SimulationFarm방위소집Aggregate(
            SimulationFarm방위소집InitialStateRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "FarmDefenseMobilizationInitialStateRequired");
            WorldStableId = Require(request.WorldStableId,
                "FarmDefenseWorldStableIdInvalid");
            SessionStableId = Require(request.SessionStableId,
                "FarmDefenseSessionStableIdInvalid");
            if (request.InitialWorldRevision < 0)
                throw new SimulationContractException(
                    "FarmDefenseInitialWorldRevisionInvalid");
            WorldRevision = request.InitialWorldRevision;
            foreach (var threat in request.ApproachingThreatStableIds
                         ?? Array.Empty<string>())
                approachingThreats.Add(Require(threat,
                    "FarmDefenseThreatStableIdInvalid"));
            foreach (var definition in request.Squads
                         ?? Array.Empty<SimulationFarm방위분대Definition>())
            {
                if (definition == null)
                    throw new SimulationContractException(
                        "FarmDefenseSquadDefinitionRequired");
                var id = Require(definition.SquadStableId,
                    "FarmDefenseSquadStableIdInvalid");
                if (squads.ContainsKey(id))
                    throw new SimulationContractException(
                        "FarmDefenseSquadDuplicate");
                var workers = (definition.AssignedWorkerStableIds
                               ?? Array.Empty<string>())
                    .Select(value => Require(value,
                        "FarmDefenseWorkerStableIdInvalid"))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                squads.Add(id, new SquadState
                {
                    SquadStableId = id,
                    WorkerStableIds = workers,
                    IsReady = definition.IsReady,
                });
            }
            actionLedger = new Simulation행위발현Ledger(WorldStableId);
        }

        public string WorldStableId { get; }
        public string SessionStableId { get; }
        public long WorldRevision { get; private set; }

        public SimulationFarm방위소집LedgerSnapshot Snapshot()
        {
            lock (gate) return CreateSnapshot();
        }

        public SimulationFarm방위소집PreviewSnapshot Preview(
            SimulationFarm방위소집PreviewRequest request)
        {
            Validate(request);
            lock (gate) return CreatePreview(request);
        }

        public SimulationFarm방위소집ConfirmResult Confirm(
            SimulationFarm방위소집ConfirmRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "FarmDefenseMobilizationConfirmRequired");
            var commandId = Require(request.CommandId,
                "FarmDefenseMobilizationCommandIdInvalid");
            Validate(new SimulationFarm방위소집PreviewRequest
            {
                ObservedWorldRevision = request.ExpectedWorldRevision,
                SquadStableId = request.SquadStableId,
                ThreatStableId = request.ThreatStableId,
            });
            lock (gate)
            {
                var payload = Payload(request.SquadStableId,
                    request.ThreatStableId);
                if (commands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payload,
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            SimulationFarm방위소집Codes.CommandPayloadConflict);
                    return Clone(applied.Result, true);
                }
                var preview = CreatePreview(new SimulationFarm방위소집PreviewRequest
                {
                    ObservedWorldRevision = request.ExpectedWorldRevision,
                    SquadStableId = request.SquadStableId,
                    ThreatStableId = request.ThreatStableId,
                });
                if (preview.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException(
                        preview.BlockReasonCodes[0]);

                var squad = squads[request.SquadStableId.Trim()];
                var before = WorldRevision;
                squad.StatusCode = SimulationFarm방위소집Codes.출동;
                squad.ThreatStableId = request.ThreatStableId.Trim();
                foreach (var worker in squad.WorkerStableIds)
                    suspendedWorkers.Add(worker);
                WorldRevision++;
                var action = actionLedger.Append(new Simulation행위발현Record
                {
                    WorldStableId = WorldStableId,
                    SessionStableId = SessionStableId,
                    PlayableLoopStableId =
                        SimulationFarm방위소집Codes.PlayableLoopStableId,
                    WorldInteractionId =
                        SimulationFarm방위소집Codes.WorldInteractionId,
                    CommandId = commandId,
                    TriggerSourceCode = "WorldDerived",
                    InitiatorStableId = "engine:farm-defense",
                    ActorStableId = squad.SquadStableId,
                    ActorKindCode = "NpcSquad",
                    TargetStableIds = new[] { squad.ThreatStableId },
                    OutcomeStableId = "outcome:farm-defense:mobilized:" +
                                      commandId,
                    PrimaryOutcomeCode = "FarmDefenseSquadMobilized",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = new[]
                    {
                        Simulation행위변화의미Codes.Actor상태변경,
                    },
                    BeforeWorldRevision = before,
                    AfterWorldRevision = WorldRevision,
                    RuleRevision = SimulationFarm방위소집Codes.RuleRevision,
                });
                var result = new SimulationFarm방위소집ConfirmResult
                {
                    Ledger = CreateSnapshot(),
                    ActionRecord = action,
                };
                commands.Add(commandId, new AppliedCommand
                {
                    PayloadKey = payload,
                    Result = Clone(result, false),
                });
                return result;
            }
        }

        private SimulationFarm방위소집PreviewSnapshot CreatePreview(
            SimulationFarm방위소집PreviewRequest request)
        {
            var squadId = request.SquadStableId.Trim();
            var threatId = request.ThreatStableId.Trim();
            var blockers = new List<string>();
            if (request.ObservedWorldRevision != WorldRevision)
                blockers.Add(SimulationFarm방위소집Codes.ExpectedRevisionMismatch);
            if (!squads.TryGetValue(squadId, out var squad))
                blockers.Add(SimulationFarm방위소집Codes.SquadUnknown);
            else
            {
                if (!squad.IsReady)
                    blockers.Add(SimulationFarm방위소집Codes.SquadNotReady);
                if (squad.StatusCode == SimulationFarm방위소집Codes.출동)
                    blockers.Add(SimulationFarm방위소집Codes.AlreadyMobilized);
            }
            if (!approachingThreats.Contains(threatId))
                blockers.Add(SimulationFarm방위소집Codes.ThreatUnknown);
            return new SimulationFarm방위소집PreviewSnapshot
            {
                ObservedWorldRevision = request.ObservedWorldRevision,
                SquadStableId = squadId,
                ThreatStableId = threatId,
                CanConfirm = blockers.Count == 0,
                SuspendedWorkerStableIds = squad?.WorkerStableIds
                    ?? Array.Empty<string>(),
                BlockReasonCodes = blockers.ToArray(),
            };
        }

        private SimulationFarm방위소집LedgerSnapshot CreateSnapshot()
        {
            var snapshot = new SimulationFarm방위소집LedgerSnapshot
            {
                WorldStableId = WorldStableId,
                SessionStableId = SessionStableId,
                WorldRevision = WorldRevision,
                Squads = squads.Values.OrderBy(value => value.SquadStableId,
                        StringComparer.Ordinal)
                    .Select(value => new SimulationFarm방위분대Snapshot
                    {
                        SquadStableId = value.SquadStableId,
                        StatusCode = value.StatusCode,
                        IsReady = value.IsReady,
                        MobilizedThreatStableId = value.ThreatStableId,
                        AssignedWorkerStableIds = value.WorkerStableIds.ToArray(),
                    }).ToArray(),
                ApproachingThreatStableIds = approachingThreats
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SuspendedProductionWorkerStableIds = suspendedWorkers
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                ActionLedger = actionLedger.Snapshot(),
            };
            snapshot.StateHashSha256 = Hash(snapshot);
            return snapshot;
        }

        private static string Hash(SimulationFarm방위소집LedgerSnapshot value)
        {
            var canonical = string.Join("\n", value.RuleRevision,
                value.WorldStableId, value.SessionStableId,
                value.WorldRevision.ToString(CultureInfo.InvariantCulture),
                string.Join("|", value.Squads.Select(squad => string.Join(":",
                    squad.SquadStableId, squad.StatusCode, squad.IsReady,
                    squad.MobilizedThreatStableId,
                    string.Join(",", squad.AssignedWorkerStableIds)))),
                string.Join("|", value.ApproachingThreatStableIds),
                string.Join("|", value.SuspendedProductionWorkerStableIds),
                value.ActionLedger.StateHashSha256);
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string Payload(string squad, string threat)
            => Require(squad, "FarmDefenseSquadStableIdInvalid") + "|" +
               Require(threat, "FarmDefenseThreatStableIdInvalid");

        private static void Validate(SimulationFarm방위소집PreviewRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "FarmDefenseMobilizationPreviewRequired");
            Require(request.SquadStableId,
                "FarmDefenseSquadStableIdInvalid");
            Require(request.ThreatStableId,
                "FarmDefenseThreatStableIdInvalid");
        }

        private static string Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(error);
            return value.Trim();
        }

        private static SimulationFarm방위소집ConfirmResult Clone(
            SimulationFarm방위소집ConfirmResult source, bool reused)
            => new SimulationFarm방위소집ConfirmResult
            {
                Ledger = source.Ledger,
                ActionRecord = source.ActionRecord,
                Reused = reused,
            };
    }
}
