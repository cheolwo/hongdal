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
        "WI-COMMUNITY-VISITOR-STAY의 수용·거절과 공동체 마음 계보 불변 규칙을 소유한다.",
        Boundary = "체류 기간·정식 편입·NPC 이동·마음 점수는 후속 WI가 소유한다.",
        WorldInteractionIds = new[] { Simulation공동체방문자체류Codes.WorldInteractionId })]
    public sealed class Simulation공동체방문자체류Aggregate
    {
        private sealed class VisitorState
        {
            public string VisitorStableId = string.Empty;
            public string StatusCode = Simulation공동체방문자체류Codes.결정대기;
            public string MindTraceCode = string.Empty;
        }

        private sealed class AppliedCommand
        {
            public string PayloadKey = string.Empty;
            public Simulation공동체방문자체류ConfirmResult Result =
                new Simulation공동체방문자체류ConfirmResult();
        }

        private readonly object gate = new object();
        private readonly Dictionary<string, VisitorState> visitors =
            new Dictionary<string, VisitorState>(StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedCommand> commands =
            new Dictionary<string, AppliedCommand>(StringComparer.Ordinal);
        private readonly Simulation행위발현Ledger actionLedger;

        public Simulation공동체방문자체류Aggregate(
            Simulation공동체방문자체류InitialStateRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "CommunityVisitorStayInitialStateRequired");
            WorldStableId = Require(request.WorldStableId,
                "CommunityVisitorWorldStableIdInvalid");
            SessionStableId = Require(request.SessionStableId,
                "CommunityVisitorSessionStableIdInvalid");
            HostPlayerStableId = Require(request.HostPlayerStableId,
                "CommunityVisitorHostPlayerStableIdInvalid");
            if (request.InitialWorldRevision < 0)
                throw new SimulationContractException(
                    "CommunityVisitorInitialWorldRevisionInvalid");
            if (request.GuestCapacity < 0 || request.OccupiedGuestCapacity < 0 ||
                request.OccupiedGuestCapacity > request.GuestCapacity)
                throw new SimulationContractException(
                    "CommunityVisitorGuestCapacityInvalid");
            WorldRevision = request.InitialWorldRevision;
            GuestCapacity = request.GuestCapacity;
            OccupiedGuestCapacity = request.OccupiedGuestCapacity;
            foreach (var definition in request.Visitors ??
                     Array.Empty<Simulation공동체방문자Definition>())
            {
                if (definition == null)
                    throw new SimulationContractException(
                        "CommunityVisitorDefinitionRequired");
                var id = Require(definition.VisitorStableId,
                    "CommunityVisitorStableIdInvalid");
                if (visitors.ContainsKey(id))
                    throw new SimulationContractException(
                        "CommunityVisitorDuplicate");
                visitors.Add(id, new VisitorState { VisitorStableId = id });
            }
            actionLedger = new Simulation행위발현Ledger(WorldStableId);
        }

        public string WorldStableId { get; }
        public string SessionStableId { get; }
        public string HostPlayerStableId { get; }
        public long WorldRevision { get; private set; }
        public int GuestCapacity { get; }
        public int OccupiedGuestCapacity { get; private set; }

        public Simulation공동체방문자체류LedgerSnapshot Snapshot()
        {
            lock (gate) return CreateSnapshot();
        }

        public Simulation공동체방문자체류PreviewSnapshot Preview(
            Simulation공동체방문자체류PreviewRequest request)
        {
            Validate(request);
            lock (gate) return CreatePreview(request);
        }

        public Simulation공동체방문자체류ConfirmResult Confirm(
            Simulation공동체방문자체류ConfirmRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "CommunityVisitorStayConfirmRequired");
            var commandId = Require(request.CommandId,
                "CommunityVisitorStayCommandIdInvalid");
            var previewRequest = new Simulation공동체방문자체류PreviewRequest
            {
                ObservedWorldRevision = request.ExpectedWorldRevision,
                VisitorStableId = request.VisitorStableId,
                DecisionCode = request.DecisionCode,
            };
            Validate(previewRequest);
            lock (gate)
            {
                var payload = Payload(request.VisitorStableId,
                    request.DecisionCode);
                if (commands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payload,
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            Simulation공동체방문자체류Codes.CommandPayloadConflict);
                    return Clone(applied.Result, true);
                }

                var preview = CreatePreview(previewRequest);
                if (preview.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException(
                        preview.BlockReasonCodes[0]);

                var visitor = visitors[request.VisitorStableId.Trim()];
                var accept = IsAccept(request.DecisionCode);
                var before = WorldRevision;
                visitor.StatusCode = accept
                    ? Simulation공동체방문자체류Codes.임시체류
                    : Simulation공동체방문자체류Codes.거절;
                visitor.MindTraceCode = accept
                    ? Simulation공동체방문자체류Codes.환대확인
                    : Simulation공동체방문자체류Codes.경계보호;
                if (accept) OccupiedGuestCapacity++;
                WorldRevision++;
                var action = actionLedger.Append(new Simulation행위발현Record
                {
                    WorldStableId = WorldStableId,
                    SessionStableId = SessionStableId,
                    PlayableLoopStableId =
                        Simulation공동체방문자체류Codes.PlayableLoopStableId,
                    WorldInteractionId =
                        Simulation공동체방문자체류Codes.WorldInteractionId,
                    CommandId = commandId,
                    TriggerSourceCode = "PlayerDriven",
                    InitiatorStableId = HostPlayerStableId,
                    ActorStableId = "community:nature-camp",
                    ActorKindCode = "Community",
                    TargetStableIds = new[] { visitor.VisitorStableId },
                    OutcomeStableId = "outcome:community-visitor-stay:" + commandId,
                    PrimaryOutcomeCode = accept
                        ? "CommunityVisitorTemporaryStayAccepted"
                        : "CommunityVisitorRejected",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = new[]
                    {
                        Simulation행위변화의미Codes.Actor상태변경,
                    },
                    BeforeWorldRevision = before,
                    AfterWorldRevision = WorldRevision,
                    RuleRevision = Simulation공동체방문자체류Codes.RuleRevision,
                });
                var result = new Simulation공동체방문자체류ConfirmResult
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

        private Simulation공동체방문자체류PreviewSnapshot CreatePreview(
            Simulation공동체방문자체류PreviewRequest request)
        {
            var visitorId = request.VisitorStableId.Trim();
            var decision = request.DecisionCode.Trim();
            var blockers = new List<string>();
            if (request.ObservedWorldRevision != WorldRevision)
                blockers.Add(Simulation공동체방문자체류Codes.ExpectedRevisionMismatch);
            if (!visitors.TryGetValue(visitorId, out var visitor))
                blockers.Add(Simulation공동체방문자체류Codes.VisitorUnknown);
            else if (!string.Equals(visitor.StatusCode,
                         Simulation공동체방문자체류Codes.결정대기,
                         StringComparison.Ordinal))
                blockers.Add(Simulation공동체방문자체류Codes.VisitorAlreadyDecided);
            var validDecision = IsAccept(decision) || IsReject(decision);
            if (!validDecision)
                blockers.Add(Simulation공동체방문자체류Codes.DecisionInvalid);
            if (IsAccept(decision) && OccupiedGuestCapacity >= GuestCapacity)
                blockers.Add(Simulation공동체방문자체류Codes.CapacityUnavailable);

            return new Simulation공동체방문자체류PreviewSnapshot
            {
                ObservedWorldRevision = request.ObservedWorldRevision,
                VisitorStableId = visitorId,
                DecisionCode = decision,
                ProjectedStatusCode = IsAccept(decision)
                    ? Simulation공동체방문자체류Codes.임시체류
                    : IsReject(decision)
                        ? Simulation공동체방문자체류Codes.거절
                        : string.Empty,
                ProjectedMindTraceCode = IsAccept(decision)
                    ? Simulation공동체방문자체류Codes.환대확인
                    : IsReject(decision)
                        ? Simulation공동체방문자체류Codes.경계보호
                        : string.Empty,
                RemainingGuestCapacity = Math.Max(0,
                    GuestCapacity - OccupiedGuestCapacity),
                CanConfirm = blockers.Count == 0,
                BlockReasonCodes = blockers.ToArray(),
            };
        }

        private Simulation공동체방문자체류LedgerSnapshot CreateSnapshot()
        {
            var snapshot = new Simulation공동체방문자체류LedgerSnapshot
            {
                WorldStableId = WorldStableId,
                SessionStableId = SessionStableId,
                HostPlayerStableId = HostPlayerStableId,
                WorldRevision = WorldRevision,
                GuestCapacity = GuestCapacity,
                OccupiedGuestCapacity = OccupiedGuestCapacity,
                Visitors = visitors.Values.OrderBy(value => value.VisitorStableId,
                        StringComparer.Ordinal)
                    .Select(value => new Simulation공동체방문자Snapshot
                    {
                        VisitorStableId = value.VisitorStableId,
                        StatusCode = value.StatusCode,
                        MindTraceCode = value.MindTraceCode,
                    }).ToArray(),
                ActionLedger = actionLedger.Snapshot(),
            };
            snapshot.StateHashSha256 = Hash(snapshot);
            return snapshot;
        }

        private static string Hash(
            Simulation공동체방문자체류LedgerSnapshot value)
        {
            var canonical = string.Join("\n", value.RuleRevision,
                value.WorldStableId, value.SessionStableId,
                value.HostPlayerStableId,
                value.WorldRevision.ToString(CultureInfo.InvariantCulture),
                value.GuestCapacity.ToString(CultureInfo.InvariantCulture),
                value.OccupiedGuestCapacity.ToString(CultureInfo.InvariantCulture),
                string.Join("|", value.Visitors.Select(visitor => string.Join(":",
                    visitor.VisitorStableId, visitor.StatusCode,
                    visitor.MindTraceCode))),
                value.ActionLedger.StateHashSha256);
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string Payload(string visitor, string decision)
            => Require(visitor, "CommunityVisitorStableIdInvalid") + "|" +
               Require(decision, "CommunityVisitorDecisionInvalid");

        private static void Validate(
            Simulation공동체방문자체류PreviewRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "CommunityVisitorStayPreviewRequired");
            Require(request.VisitorStableId,
                "CommunityVisitorStableIdInvalid");
            Require(request.DecisionCode,
                "CommunityVisitorDecisionInvalid");
        }

        private static bool IsAccept(string value) => string.Equals(value?.Trim(),
            Simulation공동체방문자체류Codes.임시체류수용,
            StringComparison.Ordinal);

        private static bool IsReject(string value) => string.Equals(value?.Trim(),
            Simulation공동체방문자체류Codes.거절선택,
            StringComparison.Ordinal);

        private static string Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(error);
            return value.Trim();
        }

        private static Simulation공동체방문자체류ConfirmResult Clone(
            Simulation공동체방문자체류ConfirmResult source, bool reused)
            => new Simulation공동체방문자체류ConfirmResult
            {
                Ledger = source.Ledger,
                ActionRecord = source.ActionRecord,
                Reused = reused,
            };
    }
}
