using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// 하향식 배치 안전 검증을 바꾸지 않고, 검증된 H1 공간 근거를 H2·H3
    /// 성립과 H4 준비도로 집계하는 결정적 Simulation 판정기다.
    /// </summary>
    public sealed class SimulationSpatialCompositionEngine
    {
        public SimulationSpatialCompositionStateSnapshot Evaluate(
            SpatialCompositionEvaluationRequest request)
        {
            ValidateRequest(request);
            var catalogHash = ComputeRuleCatalogHash(request.RuleCatalog);
            if (!string.Equals(catalogHash,
                    request.RuleCatalog.CatalogHashSha256,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationSpatialCompositionCatalogHashMismatch");

            var evidence = request.ChildEvidence
                .OrderBy(value => value.DefinitionStableId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.SpatialInstanceStableId,
                    StringComparer.Ordinal)
                .ToList();
            var previous = (request.PreviousState?.Instances
                    ?? Array.Empty<SpatialCompositionInstanceSnapshot>())
                .ToDictionary(value => value.DefinitionStableId,
                    StringComparer.Ordinal);
            var assessments = new List<SpatialCompositionAssessment>();
            var instances = new List<SpatialCompositionInstanceSnapshot>();

            foreach (var level in new[]
                     {
                         SimulationSpatialCompositionCodes.H2,
                         SimulationSpatialCompositionCodes.H3,
                     })
            {
                foreach (var rule in request.RuleCatalog.Rules
                             .Where(value => value.TargetLevelCode == level)
                             .OrderBy(value => value.TargetDefinitionStableId,
                                 StringComparer.Ordinal))
                {
                    var assessment = EvaluateFormationRule(rule, evidence,
                        request, previous);
                    assessments.Add(assessment);
                    if (assessment.StateCode !=
                            SimulationSpatialCompositionCodes.Formed
                        && assessment.StateCode !=
                            SimulationSpatialCompositionCodes.Degraded)
                        continue;

                    var prior = previous.TryGetValue(
                        rule.TargetDefinitionStableId, out var old)
                        ? old : null;
                    var instance = new SpatialCompositionInstanceSnapshot
                    {
                        SpatialInstanceStableId =
                            assessment.SpatialInstanceStableId,
                        DefinitionStableId = rule.TargetDefinitionStableId,
                        LevelCode = rule.TargetLevelCode,
                        StateCode = assessment.StateCode,
                        ChildSpatialInstanceStableIds = ResolveChildren(rule,
                                evidence)
                            .Select(value => value.SpatialInstanceStableId)
                            .OrderBy(value => value, StringComparer.Ordinal)
                            .ToArray(),
                        FormedWorldTick = prior?.FormedWorldTick
                            ?? request.WorldTick,
                        LastEvaluatedWorldTick = request.WorldTick,
                    };
                    instances.Add(instance);
                    evidence.Add(ToEvidence(instance));
                }
            }

            foreach (var rule in request.RuleCatalog.Rules
                         .Where(value => value.TargetLevelCode ==
                             SimulationSpatialCompositionCodes.H4)
                         .OrderBy(value => value.TargetDefinitionStableId,
                             StringComparer.Ordinal))
                assessments.Add(EvaluateReadinessRule(rule, assessments,
                    evidence));

            var result = new SimulationSpatialCompositionStateSnapshot
            {
                AreaCode = request.AreaCode.Trim(),
                AreaSetStableId = request.AreaSetStableId.Trim(),
                RuleCatalogRevision = request.RuleCatalog.Revision.Trim(),
                RuleCatalogHashSha256 = catalogHash,
                WorldTick = request.WorldTick,
                WorldRevision = request.WorldRevision,
                Instances = instances.OrderBy(value => value.LevelCode,
                        StringComparer.Ordinal)
                    .ThenBy(value => value.DefinitionStableId,
                        StringComparer.Ordinal).ToArray(),
                Assessments = assessments.OrderBy(value =>
                        value.TargetLevelCode, StringComparer.Ordinal)
                    .ThenBy(value => value.TargetDefinitionStableId,
                        StringComparer.Ordinal).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };
            result.GraphHashSha256 = ComputeGraphHash(result);
            return result;
        }

        public static string ComputeRuleCatalogHash(
            SpatialCompositionRuleCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var rows = catalog.Rules
                .OrderBy(value => value.TargetLevelCode,
                    StringComparer.Ordinal)
                .ThenBy(value => value.TargetDefinitionStableId,
                    StringComparer.Ordinal)
                .Select(rule => string.Join("|", new[]
                {
                    rule.RuleStableId.Trim(),
                    rule.TargetLevelCode.Trim(),
                    rule.TargetDefinitionStableId.Trim(),
                    rule.AuthorityCode.Trim(),
                    Join(rule.RequiredChildDefinitionStableIds),
                    Join(rule.OptionalChildDefinitionStableIds),
                    Join(rule.RequiredCapabilityCodes),
                    Join(rule.RequiredPlayableLoopStableIds),
                    rule.MinimumStorageCapacityKgm.ToString(
                        CultureInfo.InvariantCulture),
                    rule.MinimumWorkAreaSlots.ToString(
                        CultureInfo.InvariantCulture),
                    rule.RequiresPlacementValidation ? "1" : "0",
                    string.Join(";", rule.Relations
                        .OrderBy(value => value.RelationStableId,
                            StringComparer.Ordinal)
                        .Select(value => string.Join(",", new[]
                        {
                            value.RelationStableId.Trim(),
                            value.FromChildDefinitionStableId.Trim(),
                            value.FromConnectorRoleCode.Trim(),
                            value.ToChildDefinitionStableId.Trim(),
                            value.ToConnectorRoleCode.Trim(),
                            value.MovementKindCode.Trim(),
                        }))),
                }));
            return Hash(string.Join("\n", new[]
            {
                catalog.SchemaVersion.Trim(), catalog.Revision.Trim(),
                string.Join("\n", rows),
            }));
        }

        public static string ComputeGraphHash(
            SimulationSpatialCompositionStateSnapshot state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var instances = state.Instances
                .OrderBy(value => value.LevelCode, StringComparer.Ordinal)
                .ThenBy(value => value.DefinitionStableId,
                    StringComparer.Ordinal)
                .Select(value => string.Join("|", new[]
                {
                    value.SpatialInstanceStableId,
                    value.DefinitionStableId,
                    value.LevelCode,
                    value.StateCode,
                    Join(value.ChildSpatialInstanceStableIds),
                    value.FormedWorldTick.ToString(CultureInfo.InvariantCulture),
                    value.LastEvaluatedWorldTick.ToString(
                        CultureInfo.InvariantCulture),
                }));
            var assessments = state.Assessments
                .OrderBy(value => value.TargetLevelCode,
                    StringComparer.Ordinal)
                .ThenBy(value => value.TargetDefinitionStableId,
                    StringComparer.Ordinal)
                .Select(value => string.Join("|", new[]
                {
                    value.RuleStableId,
                    value.TargetLevelCode,
                    value.TargetDefinitionStableId,
                    value.AuthorityCode,
                    value.StateCode,
                    value.SpatialInstanceStableId,
                    Join(value.SatisfiedChildDefinitionStableIds),
                    Join(value.MissingChildDefinitionStableIds),
                    Join(value.BlockReasonCodes),
                    Join(value.SourcePlacementPlanHashes),
                }));
            return Hash(string.Join("\n", new[]
            {
                state.SchemaVersion,
                state.AreaCode,
                state.AreaSetStableId,
                state.PlacementControlRevision,
                state.RuleCatalogRevision,
                state.RuleCatalogHashSha256,
                state.WorldTick.ToString(CultureInfo.InvariantCulture),
                state.WorldRevision.ToString(CultureInfo.InvariantCulture),
                string.Join(";", instances),
                string.Join(";", assessments),
                state.SimulationOnly ? "1" : "0",
                state.IsOperationalState ? "1" : "0",
            }));
        }

        private static SpatialCompositionAssessment EvaluateFormationRule(
            SpatialCompositionRule rule,
            IReadOnlyCollection<SpatialCompositionChildEvidence> evidence,
            SpatialCompositionEvaluationRequest request,
            IReadOnlyDictionary<string, SpatialCompositionInstanceSnapshot>
                previous)
        {
            var children = ResolveChildren(rule, evidence);
            var foundDefinitions = children.Select(value =>
                    value.DefinitionStableId)
                .ToHashSet(StringComparer.Ordinal);
            var missing = rule.RequiredChildDefinitionStableIds
                .Where(value => !foundDefinitions.Contains(value))
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var blocks = new List<string>();
            blocks.AddRange(missing.Select(value =>
                "MissingRequiredChild:" + value));

            foreach (var child in children.Where(value =>
                         rule.RequiredChildDefinitionStableIds.Contains(
                             value.DefinitionStableId, StringComparer.Ordinal)))
            {
                if (!child.Operational)
                    blocks.Add("ChildNotOperational:" + child.DefinitionStableId);
                if (rule.RequiresPlacementValidation
                    && !child.PlacementValidated)
                    blocks.Add("PlacementValidationMissing:"
                               + child.DefinitionStableId);
            }

            var capabilities = children.SelectMany(value =>
                    value.CapabilityCodes)
                .ToHashSet(StringComparer.Ordinal);
            blocks.AddRange(rule.RequiredCapabilityCodes
                .Where(value => !capabilities.Contains(value))
                .Select(value => "MissingCapability:" + value));
            if (children.Sum(value => value.StorageCapacityKgm)
                < rule.MinimumStorageCapacityKgm)
                blocks.Add("StorageCapacityInsufficient");
            if (children.Sum(value => value.WorkAreaSlots)
                < rule.MinimumWorkAreaSlots)
                blocks.Add("WorkAreaSlotInsufficient");

            foreach (var relation in rule.Relations)
            {
                var from = children.FirstOrDefault(value =>
                    value.DefinitionStableId ==
                    relation.FromChildDefinitionStableId);
                var to = children.FirstOrDefault(value =>
                    value.DefinitionStableId ==
                    relation.ToChildDefinitionStableId);
                if (from == null || to == null) continue;
                if (!from.ConnectorRoleCodes.Contains(
                        relation.FromConnectorRoleCode,
                        StringComparer.Ordinal)
                    || !to.ConnectorRoleCodes.Contains(
                        relation.ToConnectorRoleCode,
                        StringComparer.Ordinal))
                    blocks.Add("SemanticRelationUnresolved:"
                               + relation.RelationStableId);
            }

            var closedLoops = request.ClosedPlayableLoopStableIds
                .ToHashSet(StringComparer.Ordinal);
            blocks.AddRange(rule.RequiredPlayableLoopStableIds
                .Where(value => !closedLoops.Contains(value))
                .Select(value => "PlayableLoopNotClosed:" + value));
            blocks = blocks.Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToList();

            previous.TryGetValue(rule.TargetDefinitionStableId, out var prior);
            var state = blocks.Count == 0
                ? request.CommitQualifiedFormations
                    ? SimulationSpatialCompositionCodes.Formed
                    : SimulationSpatialCompositionCodes.Qualified
                : prior != null && prior.StateCode ==
                    SimulationSpatialCompositionCodes.Formed
                    ? SimulationSpatialCompositionCodes.Degraded
                    : SimulationSpatialCompositionCodes.Blocked;
            var childIds = children.Select(value =>
                    value.SpatialInstanceStableId)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var instanceId = prior?.SpatialInstanceStableId
                ?? BuildInstanceStableId(request.RuleCatalog.Revision, rule,
                    childIds);
            return new SpatialCompositionAssessment
            {
                RuleStableId = rule.RuleStableId,
                TargetLevelCode = rule.TargetLevelCode,
                TargetDefinitionStableId = rule.TargetDefinitionStableId,
                AuthorityCode = rule.AuthorityCode,
                StateCode = state,
                SpatialInstanceStableId = instanceId,
                SatisfiedChildDefinitionStableIds = foundDefinitions
                    .Where(value => rule.RequiredChildDefinitionStableIds
                        .Contains(value, StringComparer.Ordinal))
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                MissingChildDefinitionStableIds = missing,
                BlockReasonCodes = blocks.ToArray(),
                SourcePlacementPlanHashes = children
                    .Select(value => value.PlacementPlanHashSha256)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            };
        }

        private static SpatialCompositionAssessment EvaluateReadinessRule(
            SpatialCompositionRule rule,
            IReadOnlyCollection<SpatialCompositionAssessment> assessments,
            IReadOnlyCollection<SpatialCompositionChildEvidence> evidence)
        {
            var childAssessments = assessments.Where(value =>
                    rule.RequiredChildDefinitionStableIds.Contains(
                        value.TargetDefinitionStableId,
                        StringComparer.Ordinal))
                .ToArray();
            var ready = childAssessments.Where(value =>
                    value.StateCode == SimulationSpatialCompositionCodes.Formed)
                .Select(value => value.TargetDefinitionStableId)
                .ToHashSet(StringComparer.Ordinal);
            var partiallySatisfied = childAssessments.Any(value =>
                value.SatisfiedChildDefinitionStableIds.Length > 0);
            var missing = rule.RequiredChildDefinitionStableIds
                .Where(value => !ready.Contains(value))
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            var state = missing.Length == 0
                ? SimulationSpatialCompositionCodes.Ready
                : ready.Count > 0 || partiallySatisfied
                    ? SimulationSpatialCompositionCodes.PartiallyReady
                    : SimulationSpatialCompositionCodes.NotReady;
            return new SpatialCompositionAssessment
            {
                RuleStableId = rule.RuleStableId,
                TargetLevelCode = rule.TargetLevelCode,
                TargetDefinitionStableId = rule.TargetDefinitionStableId,
                AuthorityCode = SimulationSpatialCompositionCodes.ReadinessOnly,
                StateCode = state,
                SatisfiedChildDefinitionStableIds = ready
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                MissingChildDefinitionStableIds = missing,
                BlockReasonCodes = missing.Select(value =>
                    "MissingRequiredChild:" + value).ToArray(),
                SourcePlacementPlanHashes = evidence
                    .Select(value => value.PlacementPlanHashSha256)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            };
        }

        private static SpatialCompositionChildEvidence[] ResolveChildren(
            SpatialCompositionRule rule,
            IEnumerable<SpatialCompositionChildEvidence> evidence)
        {
            var accepted = rule.RequiredChildDefinitionStableIds
                .Concat(rule.OptionalChildDefinitionStableIds)
                .ToHashSet(StringComparer.Ordinal);
            return evidence.Where(value => accepted.Contains(
                    value.DefinitionStableId))
                .OrderBy(value => value.DefinitionStableId,
                    StringComparer.Ordinal)
                .ThenBy(value => value.SpatialInstanceStableId,
                    StringComparer.Ordinal).ToArray();
        }

        private static SpatialCompositionChildEvidence ToEvidence(
            SpatialCompositionInstanceSnapshot instance)
            => new SpatialCompositionChildEvidence
            {
                SpatialInstanceStableId = instance.SpatialInstanceStableId,
                DefinitionStableId = instance.DefinitionStableId,
                LevelCode = instance.LevelCode,
                Operational = instance.StateCode ==
                    SimulationSpatialCompositionCodes.Formed,
                PlacementValidated = true,
            };

        private static string BuildInstanceStableId(string catalogRevision,
            SpatialCompositionRule rule, IEnumerable<string> childIds)
            => "spatial-instance:" + rule.TargetLevelCode.ToLowerInvariant()
               + ":" + Hash(string.Join("|", new[]
               {
                   catalogRevision,
                   rule.TargetDefinitionStableId,
                   string.Join(",", childIds),
               }));

        private static void ValidateRequest(
            SpatialCompositionEvaluationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.AreaCode)
                || string.IsNullOrWhiteSpace(request.AreaSetStableId)
                || request.WorldTick < 0 || request.WorldRevision < 0
                || request.RuleCatalog == null
                || request.RuleCatalog.SchemaVersion !=
                SimulationSpatialCompositionCodes.SchemaVersion
                || string.IsNullOrWhiteSpace(request.RuleCatalog.Revision))
                throw new SimulationContractException(
                    "SimulationSpatialCompositionRequestInvalid");
            var rules = request.RuleCatalog.Rules
                ?? Array.Empty<SpatialCompositionRule>();
            if (rules.Length == 0
                || rules.Select(value => value.RuleStableId)
                    .Distinct(StringComparer.Ordinal).Count() != rules.Length
                || rules.Select(value => value.TargetDefinitionStableId)
                    .Distinct(StringComparer.Ordinal).Count() != rules.Length)
                throw new SimulationContractException(
                    "SimulationSpatialCompositionRuleDuplicateOrMissing");
            var targets = rules.Select(value => value.TargetDefinitionStableId)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var rule in rules)
            {
                if (string.IsNullOrWhiteSpace(rule.RuleStableId)
                    || string.IsNullOrWhiteSpace(rule.TargetDefinitionStableId)
                    || rule.TargetLevelCode !=
                        SimulationSpatialCompositionCodes.H2
                    && rule.TargetLevelCode !=
                        SimulationSpatialCompositionCodes.H3
                    && rule.TargetLevelCode !=
                        SimulationSpatialCompositionCodes.H4
                    || rule.RequiredChildDefinitionStableIds == null
                    || rule.RequiredChildDefinitionStableIds.Length == 0
                    || rule.RequiredChildDefinitionStableIds
                        .Distinct(StringComparer.Ordinal).Count() !=
                        rule.RequiredChildDefinitionStableIds.Length
                    || rule.Relations.Select(value => value.RelationStableId)
                        .Distinct(StringComparer.Ordinal).Count() !=
                        rule.Relations.Length)
                    throw new SimulationContractException(
                        "SimulationSpatialCompositionRuleInvalid");
                if (rule.TargetLevelCode ==
                        SimulationSpatialCompositionCodes.H4
                    && rule.RequiredChildDefinitionStableIds.Any(value =>
                        !targets.Contains(value)))
                    throw new SimulationContractException(
                        "SimulationSpatialCompositionUnknownH3Child");
            }
            if (HasCycle(rules))
                throw new SimulationContractException(
                    "SimulationSpatialCompositionCycleDetected");
            var childEvidence = request.ChildEvidence
                ?? Array.Empty<SpatialCompositionChildEvidence>();
            if (childEvidence.Select(value => value.SpatialInstanceStableId)
                    .Distinct(StringComparer.Ordinal).Count()
                != childEvidence.Length
                || childEvidence.Any(value =>
                    string.IsNullOrWhiteSpace(value.SpatialInstanceStableId)
                    || string.IsNullOrWhiteSpace(value.DefinitionStableId)))
                throw new SimulationContractException(
                    "SimulationSpatialCompositionChildEvidenceInvalid");
        }

        private static bool HasCycle(IEnumerable<SpatialCompositionRule> rules)
        {
            var byTarget = rules.ToDictionary(value =>
                value.TargetDefinitionStableId, StringComparer.Ordinal);
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            bool Visit(string target)
            {
                if (visited.Contains(target)) return false;
                if (!visiting.Add(target)) return true;
                foreach (var child in byTarget[target]
                             .RequiredChildDefinitionStableIds)
                    if (byTarget.ContainsKey(child) && Visit(child)) return true;
                visiting.Remove(target);
                visited.Add(target);
                return false;
            }
            return byTarget.Keys.Any(Visit);
        }

        private static string Join(IEnumerable<string> values)
            => string.Join(",", (values ?? Array.Empty<string>())
                .OrderBy(value => value, StringComparer.Ordinal));

        private static string Hash(string text)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(text)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
