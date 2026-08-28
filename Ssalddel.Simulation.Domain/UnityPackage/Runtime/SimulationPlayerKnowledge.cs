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
        "플레이어별 처방 지식과 WorldRevision·행위 기록의 원자적 불변 규칙을 소유한다.",
        Boundary = "Save·RemoteHost·Unity 표현 없이 Logic 권위만 변경한다.")]
    public sealed class Simulation플레이어지식Aggregate
    {
        private readonly object gate = new object();
        private readonly HashSet<string> knownRecipeStableIds =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, Simulation처방지식SourceDefinition>
            sources = new Dictionary<string, Simulation처방지식SourceDefinition>(
                StringComparer.Ordinal);
        private readonly Dictionary<string, AppliedCommand> appliedCommands =
            new Dictionary<string, AppliedCommand>(StringComparer.Ordinal);
        private readonly Simulation행위발현Ledger actionLedger;

        public Simulation플레이어지식Aggregate(
            Simulation플레이어지식InitialStateRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "PlayerKnowledgeInitialStateRequired");
            WorldStableId = Require(request.WorldStableId,
                "PlayerKnowledgeWorldStableIdInvalid");
            SessionStableId = Require(request.SessionStableId,
                "PlayerKnowledgeSessionStableIdInvalid");
            PlayerStableId = Require(request.PlayerStableId,
                "PlayerKnowledgePlayerStableIdInvalid");
            if (request.InitialWorldRevision < 0)
                throw new SimulationContractException(
                    "PlayerKnowledgeInitialWorldRevisionInvalid");
            WorldRevision = request.InitialWorldRevision;

            foreach (var recipeStableId in request.KnownRecipeStableIds
                         ?? Array.Empty<string>())
                knownRecipeStableIds.Add(RequireRecipe(recipeStableId));
            foreach (var source in request.KnowledgeSources
                         ?? Array.Empty<Simulation처방지식SourceDefinition>())
            {
                if (source == null)
                    throw new SimulationContractException(
                        "PlayerKnowledgeSourceDefinitionRequired");
                var sourceStableId = Require(source.KnowledgeSourceStableId,
                    "PlayerKnowledgeSourceStableIdInvalid");
                if (sources.ContainsKey(sourceStableId))
                    throw new SimulationContractException(
                        "PlayerKnowledgeSourceDuplicate");
                sources.Add(sourceStableId, new Simulation처방지식SourceDefinition
                {
                    KnowledgeSourceStableId = sourceStableId,
                    IsAccessible = source.IsAccessible,
                    ApprovedRecipeStableIds = (source.ApprovedRecipeStableIds
                            ?? Array.Empty<string>())
                        .Select(RequireRecipe).Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                });
            }
            actionLedger = new Simulation행위발현Ledger(WorldStableId);
        }

        public string WorldStableId { get; }
        public string SessionStableId { get; }
        public string PlayerStableId { get; }
        public long WorldRevision { get; private set; }

        public Simulation플레이어지식LedgerSnapshot Snapshot()
        {
            lock (gate)
                return CreateSnapshot();
        }

        public Simulation지식습득PreviewSnapshot Preview(
            Simulation지식습득PreviewRequest request)
        {
            ValidatePreviewRequest(request);
            lock (gate)
                return CreatePreview(request);
        }

        public Simulation지식습득ConfirmResult Confirm(
            Simulation지식습득ConfirmRequest request)
        {
            ValidateConfirmRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildPayloadKey(request);
                if (appliedCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey,
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            Simulation플레이어지식Codes.CommandPayloadConflict);
                    return Clone(applied.Result, reused: true);
                }

                var preview = CreatePreview(new Simulation지식습득PreviewRequest
                {
                    ObservedWorldRevision = request.ExpectedWorldRevision,
                    PlayerStableId = request.PlayerStableId,
                    RecipeStableId = request.RecipeStableId,
                    KnowledgeSourceStableId = request.KnowledgeSourceStableId,
                });
                if (preview.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException(
                        preview.BlockReasonCodes[0]);

                if (preview.AlreadyKnown)
                {
                    var reused = new Simulation지식습득ConfirmResult
                    {
                        KnowledgeLedger = CreateSnapshot(),
                        Added = false,
                        Reused = true,
                    };
                    appliedCommands.Add(commandId,
                        new AppliedCommand(payloadKey,
                            Clone(reused, reused: true)));
                    return Clone(reused, reused: true);
                }

                var beforeRevision = WorldRevision;
                knownRecipeStableIds.Add(preview.RecipeStableId);
                WorldRevision++;
                var action = actionLedger.Append(new Simulation행위발현Record
                {
                    WorldStableId = WorldStableId,
                    SessionStableId = SessionStableId,
                    PlayableLoopStableId =
                        Simulation플레이어지식Codes.PlayableLoopStableId,
                    WorldInteractionId =
                        Simulation플레이어지식Codes.지식습득WorldInteractionId,
                    CommandId = commandId,
                    TriggerSourceCode =
                        SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                    InitiatorStableId = PlayerStableId,
                    ActorStableId = PlayerStableId,
                    ActorKindCode = "Player",
                    TargetStableIds = new[] { preview.RecipeStableId },
                    OutcomeStableId = "outcome:knowledge-acquired:" + commandId,
                    PrimaryOutcomeCode = "RecipeKnowledgeAdded",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = new[]
                    {
                        Simulation행위변화의미Codes.플레이어진척변경,
                    },
                    SourceReferenceIds = new[]
                    {
                        preview.KnowledgeSourceStableId,
                        preview.RecipeStableId,
                    },
                    BeforeWorldRevision = beforeRevision,
                    AfterWorldRevision = WorldRevision,
                    AppliedWorldTick = 0,
                    RuleRevision = Simulation플레이어지식Codes.RuleRevision,
                });
                var result = new Simulation지식습득ConfirmResult
                {
                    KnowledgeLedger = CreateSnapshot(),
                    ActionRecord = action,
                    Added = true,
                };
                appliedCommands.Add(commandId,
                    new AppliedCommand(payloadKey, Clone(result, reused: false)));
                return Clone(result, reused: false);
            }
        }

        private Simulation지식습득PreviewSnapshot CreatePreview(
            Simulation지식습득PreviewRequest request)
        {
            var playerStableId = request.PlayerStableId.Trim();
            var recipeStableId = request.RecipeStableId.Trim();
            var sourceStableId = request.KnowledgeSourceStableId.Trim();
            var blockers = new List<string>();
            if (!string.Equals(playerStableId, PlayerStableId,
                    StringComparison.Ordinal))
                blockers.Add(Simulation플레이어지식Codes.PlayerMismatch);
            if (request.ObservedWorldRevision != WorldRevision)
                blockers.Add(
                    Simulation플레이어지식Codes.ExpectedRevisionMismatch);
            if (!sources.TryGetValue(sourceStableId, out var source)
                || !source.IsAccessible)
                blockers.Add(
                    Simulation플레이어지식Codes.KnowledgeSourceUnavailable);
            else if (!source.ApprovedRecipeStableIds.Contains(recipeStableId,
                         StringComparer.Ordinal))
                blockers.Add(Simulation플레이어지식Codes.RecipeUnknown);
            var alreadyKnown = knownRecipeStableIds.Contains(recipeStableId);
            return new Simulation지식습득PreviewSnapshot
            {
                ObservedWorldRevision = request.ObservedWorldRevision,
                PlayerStableId = playerStableId,
                RecipeStableId = recipeStableId,
                KnowledgeSourceStableId = sourceStableId,
                AlreadyKnown = alreadyKnown,
                CanConfirm = blockers.Count == 0 && !alreadyKnown,
                BlockReasonCodes = blockers.ToArray(),
            };
        }

        private Simulation플레이어지식LedgerSnapshot CreateSnapshot()
        {
            var snapshot = new Simulation플레이어지식LedgerSnapshot
            {
                WorldStableId = WorldStableId,
                SessionStableId = SessionStableId,
                PlayerStableId = PlayerStableId,
                WorldRevision = WorldRevision,
                KnownRecipeStableIds = knownRecipeStableIds
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                ActionLedger = actionLedger.Snapshot(),
            };
            snapshot.StateHashSha256 = CalculateHash(snapshot);
            return snapshot;
        }

        private static string CalculateHash(
            Simulation플레이어지식LedgerSnapshot snapshot)
        {
            var canonical = string.Join("\n", new[]
            {
                snapshot.RuleRevision,
                snapshot.WorldStableId,
                snapshot.SessionStableId,
                snapshot.PlayerStableId,
                snapshot.WorldRevision.ToString(CultureInfo.InvariantCulture),
                string.Join("|", snapshot.KnownRecipeStableIds),
                snapshot.ActionLedger.StateHashSha256,
            });
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static string BuildPayloadKey(
            Simulation지식습득ConfirmRequest request)
            => string.Join("|", request.PlayerStableId.Trim(),
                request.RecipeStableId.Trim(),
                request.KnowledgeSourceStableId.Trim());

        private static void ValidatePreviewRequest(
            Simulation지식습득PreviewRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "PlayerKnowledgePreviewRequired");
            Require(request.PlayerStableId,
                "PlayerKnowledgePlayerStableIdInvalid");
            RequireRecipe(request.RecipeStableId);
            Require(request.KnowledgeSourceStableId,
                "PlayerKnowledgeSourceStableIdInvalid");
        }

        private static void ValidateConfirmRequest(
            Simulation지식습득ConfirmRequest request)
        {
            if (request == null)
                throw new SimulationContractException(
                    "PlayerKnowledgeConfirmRequired");
            Require(request.CommandId, "PlayerKnowledgeCommandIdInvalid");
            ValidatePreviewRequest(new Simulation지식습득PreviewRequest
            {
                PlayerStableId = request.PlayerStableId,
                RecipeStableId = request.RecipeStableId,
                KnowledgeSourceStableId = request.KnowledgeSourceStableId,
            });
        }

        private static string RequireRecipe(string value)
        {
            var normalized = Require(value, "PlayerKnowledgeRecipeStableIdInvalid");
            if (!normalized.StartsWith("recipe:", StringComparison.Ordinal))
                throw new SimulationContractException(
                    "PlayerKnowledgeRecipeStableIdInvalid");
            return normalized;
        }

        private static string Require(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(errorCode);
            return value.Trim();
        }

        private static Simulation지식습득ConfirmResult Clone(
            Simulation지식습득ConfirmResult source, bool reused)
            => new Simulation지식습득ConfirmResult
            {
                KnowledgeLedger = Clone(source.KnowledgeLedger),
                ActionRecord = source.ActionRecord == null ? null :
                    Clone(source.ActionRecord),
                Added = source.Added,
                Reused = reused,
            };

        private static Simulation플레이어지식LedgerSnapshot Clone(
            Simulation플레이어지식LedgerSnapshot source)
            => new Simulation플레이어지식LedgerSnapshot
            {
                RuleRevision = source.RuleRevision,
                WorldStableId = source.WorldStableId,
                SessionStableId = source.SessionStableId,
                PlayerStableId = source.PlayerStableId,
                WorldRevision = source.WorldRevision,
                KnownRecipeStableIds = source.KnownRecipeStableIds.ToArray(),
                ActionLedger = Simulation행위발현Ledger.Restore(
                    source.ActionLedger).Snapshot(),
                StateHashSha256 = source.StateHashSha256,
            };

        private static Simulation행위발현Record Clone(
            Simulation행위발현Record source)
            => new Simulation행위발현Record
            {
                SchemaCode = source.SchemaCode,
                행위기록StableId = source.행위기록StableId,
                이전기록HashSha256 = source.이전기록HashSha256,
                기록HashSha256 = source.기록HashSha256,
                WorldStableId = source.WorldStableId,
                SessionStableId = source.SessionStableId,
                PlayableLoopStableId = source.PlayableLoopStableId,
                WorldInteractionId = source.WorldInteractionId,
                CommandId = source.CommandId,
                TriggerSourceCode = source.TriggerSourceCode,
                InitiatorStableId = source.InitiatorStableId,
                ActorStableId = source.ActorStableId,
                ActorKindCode = source.ActorKindCode,
                TargetStableIds = source.TargetStableIds.ToArray(),
                OutcomeStableId = source.OutcomeStableId,
                PrimaryOutcomeCode = source.PrimaryOutcomeCode,
                결과분류Code = source.결과분류Code,
                변화의미Codes = source.변화의미Codes.ToArray(),
                SourceReferenceIds = source.SourceReferenceIds.ToArray(),
                BeforeWorldRevision = source.BeforeWorldRevision,
                AfterWorldRevision = source.AfterWorldRevision,
                AppliedWorldTick = source.AppliedWorldTick,
                RuleRevision = source.RuleRevision,
            };

        private sealed class AppliedCommand
        {
            public AppliedCommand(string payloadKey,
                Simulation지식습득ConfirmResult result)
            {
                PayloadKey = payloadKey;
                Result = result;
            }

            public string PayloadKey { get; }
            public Simulation지식습득ConfirmResult Result { get; }
        }
    }
}
