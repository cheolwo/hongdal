using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// 권위 원장을 바꾸지 않고 현재 플레이어가 실제로 행하고 성찰한 관계만 투영한다.
    /// </summary>
    public static class Simulation플레이어이데아맵Projection
    {
        public static Simulation플레이어이데아맵ProjectionSnapshot Build(
            string sessionStableId,
            string playerStableId,
            long worldRevision,
            int worldTick,
            Simulation행위기록LedgerSnapshot? actionLedger,
            Simulation플레이어분야ProfileSnapshot? profile,
            Simulation학습중점StateSnapshot? learningFocus)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new SimulationContractException(
                    "SimulationIdeaMapSessionStableIdInvalid");
            if (string.IsNullOrWhiteSpace(playerStableId))
                throw new SimulationContractException(
                    "SimulationIdeaMapPlayerStableIdInvalid");

            var player = playerStableId.Trim();
            var actions = (actionLedger?.TailRecords
                    ?? Array.Empty<Simulation행위발현Record>())
                .Where(value => IsQualifyingAction(value, player))
                .OrderBy(value => value.AfterWorldRevision)
                .ThenBy(value => value.AppliedWorldTick)
                .ThenBy(value => value.행위기록StableId, StringComparer.Ordinal)
                .ToArray();
            var actionIds = new HashSet<string>(actions.Select(value =>
                value.행위기록StableId), StringComparer.Ordinal);
            var contributions = (profile?.기여기록들
                    ?? Array.Empty<Simulation분야진척기여Snapshot>())
                .Where(value => actionIds.Contains(
                    value.SourceActionRecordStableId))
                .OrderBy(value => value.ContributionStableId,
                    StringComparer.Ordinal).ToArray();
            var meditations = (profile?.명상기여기록들
                    ?? Array.Empty<Simulation명상숙련기여Snapshot>())
                .Where(value => actionIds.Contains(
                    value.SourceActionRecordStableId))
                .OrderBy(value => value.ContributionStableId,
                    StringComparer.Ordinal).ToArray();
            var mentorReceipts = (learningFocus?.EffectReceipts
                    ?? Array.Empty<Simulation학습효과ReceiptSnapshot>())
                .Where(value => actionIds.Contains(
                    value.SourceActionRecordStableId))
                .GroupBy(value => BuildDomainKey(value.DomainStableId,
                        value.SkillStableId,
                        value.SourceActionRecordStableId),
                    StringComparer.Ordinal)
                .ToDictionary(group => group.Key,
                    group => group.OrderBy(value => value.ReceiptStableId,
                        StringComparer.Ordinal).First(),
                    StringComparer.Ordinal);

            var nodes = new List<Simulation이데아맵NodeSnapshot>();
            var edges = new List<Simulation이데아맵EdgeSnapshot>();
            foreach (var action in actions)
            {
                var experienceId = "idea:experience:" +
                    action.행위기록StableId;
                nodes.Add(new Simulation이데아맵NodeSnapshot
                {
                    NodeStableId = experienceId,
                    NodeKindCode = Simulation플레이어이데아맵Codes
                        .RecentExperience,
                    Title = action.WorldInteractionId,
                    SourceActionRecordStableId = action.행위기록StableId,
                });

                foreach (var contributionGroup in contributions.Where(value =>
                             value.SourceActionRecordStableId ==
                             action.행위기록StableId).GroupBy(value =>
                             BuildDomainKey(value.분야StableId,
                                 value.세부숙련StableId,
                                 value.SourceActionRecordStableId),
                             StringComparer.Ordinal))
                {
                    var contribution = contributionGroup.First();
                    var progress = FindProgress(profile,
                        contribution.분야StableId,
                        contribution.세부숙련StableId);
                    var mentorKey = BuildDomainKey(
                        contribution.분야StableId,
                        contribution.세부숙련StableId,
                        action.행위기록StableId);
                    mentorReceipts.TryGetValue(mentorKey, out var mentor);
                    var nodeKind = progress.field > 0 || progress.operation > 0
                        ? Simulation플레이어이데아맵Codes.VerifiedKnowledgeSkill
                        : mentor != null
                            ? Simulation플레이어이데아맵Codes.FragmentCandidate
                            : Simulation플레이어이데아맵Codes.ObservedUnskilled;
                    var domainId = "idea:domain:" + Hash(string.Join("|",
                        contribution.분야StableId,
                        contribution.세부숙련StableId,
                        action.행위기록StableId));
                    nodes.Add(new Simulation이데아맵NodeSnapshot
                    {
                        NodeStableId = domainId,
                        NodeKindCode = nodeKind,
                        Title = string.IsNullOrWhiteSpace(
                            contribution.세부숙련StableId)
                            ? contribution.분야StableId
                            : contribution.세부숙련StableId,
                        분야StableId = contribution.분야StableId,
                        세부숙련StableId = contribution.세부숙련StableId,
                        이해도 = progress.understanding,
                        현장숙련도 = progress.field,
                        운영숙련도 = progress.operation,
                        SourceActionRecordStableId =
                            action.행위기록StableId,
                        SourceContributionStableId = string.Join(",",
                            contributionGroup.Select(value =>
                                value.ContributionStableId).OrderBy(value =>
                                value, StringComparer.Ordinal)),
                        SourceMentorActorStableId = mentor?.SourceActorStableId
                            ?? string.Empty,
                    });
                    edges.Add(Edge(Simulation플레이어이데아맵Codes
                        .ObservedFrom, experienceId, domainId, action,
                        string.Empty));
                    if (progress.field > 0 || progress.operation > 0)
                        edges.Add(Edge(Simulation플레이어이데아맵Codes
                            .VerifiedBy, experienceId, domainId, action,
                            string.Empty));
                    if (mentor != null)
                        edges.Add(Edge(Simulation플레이어이데아맵Codes
                            .MentoredBy, experienceId, domainId, action,
                            mentor.SourceActorStableId));
                }
            }

            foreach (var meditation in meditations)
            {
                var experienceId = "idea:experience:" +
                    meditation.SourceActionRecordStableId;
                var reflectionId = "idea:reflection:" +
                    meditation.ContributionStableId;
                nodes.Add(new Simulation이데아맵NodeSnapshot
                {
                    NodeStableId = reflectionId,
                    NodeKindCode = Simulation플레이어이데아맵Codes.ReflectionSeed,
                    Title = meditation.ChallengeStableId,
                    분야StableId = meditation.분야StableId,
                    세부숙련StableId = meditation.세부숙련StableId,
                    SourceActionRecordStableId =
                        meditation.SourceActionRecordStableId,
                    SourceContributionStableId =
                        meditation.ContributionStableId,
                });
                edges.Add(Edge(Simulation플레이어이데아맵Codes.ReflectionOf,
                    reflectionId, experienceId,
                    actions.First(value => value.행위기록StableId ==
                        meditation.SourceActionRecordStableId), string.Empty));
            }

            var result = new Simulation플레이어이데아맵ProjectionSnapshot
            {
                SessionStableId = sessionStableId.Trim(),
                PlayerStableId = player,
                WorldRevision = worldRevision,
                WorldTick = worldTick,
                BasicViewAvailable = actions.Length > 0,
                MeditationProficiency = profile?.명상숙련도 ?? 0,
                MeditationStageCode = profile?.명상숙련도단계Code
                    ?? Simulation분야단계Codes.미경험,
                Nodes = nodes.OrderBy(value => value.NodeStableId,
                    StringComparer.Ordinal).ToArray(),
                Edges = edges.OrderBy(value => value.EdgeStableId,
                    StringComparer.Ordinal).ToArray(),
                SourceActionLedgerHashSha256 = actionLedger?.StateHashSha256
                    ?? string.Empty,
                SourceDomainProfileHashSha256 = profile?.StateHashSha256
                    ?? string.Empty,
                SourceLearningFocusHashSha256 = learningFocus?.StateHashSha256
                    ?? string.Empty,
                ChangesWorldState = false,
            };
            result.StateHashSha256 = CalculateHash(result);
            return result;
        }

        public static string CalculateHash(
            Simulation플레이어이데아맵ProjectionSnapshot value)
        {
            var text = new StringBuilder();
            Add(text, value.SchemaVersion); Add(text, value.ProjectionRevision);
            Add(text, value.SessionStableId); Add(text, value.PlayerStableId);
            Add(text, value.WorldRevision); Add(text, value.WorldTick);
            Add(text, value.BasicViewAvailable); Add(text,
                value.MeditationProficiency); Add(text,
                value.MeditationStageCode);
            Add(text, value.SourceActionLedgerHashSha256);
            Add(text, value.SourceDomainProfileHashSha256);
            Add(text, value.SourceLearningFocusHashSha256);
            foreach (var node in value.Nodes.OrderBy(item => item.NodeStableId,
                         StringComparer.Ordinal))
            {
                Add(text, node.NodeStableId); Add(text, node.NodeKindCode);
                Add(text, node.Title); Add(text, node.분야StableId);
                Add(text, node.세부숙련StableId); Add(text, node.이해도);
                Add(text, node.현장숙련도); Add(text, node.운영숙련도);
                Add(text, node.SourceActionRecordStableId);
                Add(text, node.SourceContributionStableId);
                Add(text, node.SourceMentorActorStableId);
            }
            foreach (var edge in value.Edges.OrderBy(item => item.EdgeStableId,
                         StringComparer.Ordinal))
            {
                Add(text, edge.EdgeStableId); Add(text, edge.EdgeKindCode);
                Add(text, edge.FromNodeStableId); Add(text, edge.ToNodeStableId);
                Add(text, edge.SourceActionRecordStableId);
                Add(text, edge.SourceMentorActorStableId);
            }
            Add(text, value.ChangesWorldState);
            return Hash(text.ToString());
        }

        private static bool IsQualifyingAction(
            Simulation행위발현Record value, string playerStableId)
            => string.Equals(value.ActorStableId, playerStableId,
                   StringComparison.Ordinal)
               && string.Equals(value.TriggerSourceCode,
                   SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                   StringComparison.Ordinal)
               && !string.IsNullOrWhiteSpace(value.행위기록StableId)
               && value.결과분류Code is
                   Simulation행위결과분류Codes.성공 or
                   Simulation행위결과분류Codes.의미있는실패 or
                   Simulation행위결과분류Codes.후퇴복구;

        private static (int understanding, int field, int operation)
            FindProgress(Simulation플레이어분야ProfileSnapshot? profile,
                string domainStableId, string skillStableId)
        {
            var domain = profile?.분야진척들.FirstOrDefault(value =>
                value.분야StableId == domainStableId);
            if (domain == null) return (0, 0, 0);
            if (string.IsNullOrWhiteSpace(skillStableId))
                return (domain.이해도, domain.현장숙련도, domain.운영숙련도);
            var skill = domain.세부숙련진척들.FirstOrDefault(value =>
                value.세부숙련StableId == skillStableId);
            return skill == null ? (0, 0, 0)
                : (skill.이해도, skill.현장숙련도, skill.운영숙련도);
        }

        private static Simulation이데아맵EdgeSnapshot Edge(string kind,
            string from, string to, Simulation행위발현Record action,
            string mentor)
            => new Simulation이데아맵EdgeSnapshot
            {
                EdgeStableId = "idea:edge:" + Hash(string.Join("|", kind,
                    from, to, mentor)),
                EdgeKindCode = kind,
                FromNodeStableId = from,
                ToNodeStableId = to,
                SourceActionRecordStableId = action.행위기록StableId,
                SourceMentorActorStableId = mentor,
            };

        private static string BuildDomainKey(string domain, string skill,
            string action) => string.Join("|", domain, skill, action);

        private static void Add(StringBuilder target, object? value)
            => target.Append(value?.ToString() ?? string.Empty).Append('\n');

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
