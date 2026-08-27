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
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "변화 Context가 적용된 환경 발생 가중치를 결정적으로 판정한다.",
        StepKey = "domain.environment-spawn-decision",
        DependsOnStepKeys = new[] { "contract.environment-spawn-decision" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 27,
        Boundary = "결정은 후보이며 SimulationEntity는 WorldTick Effect가 별도로 확정해야 한다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "환경 발생 가중치와 seed를 같은 Core에서 결정적으로 계산한다.",
        Boundary = "결정 결과는 WorldTick Confirm 전 권위 Spawn이 아니다.")]
    public sealed class Simulation결정적환경발생DecisionEngine
        : ISimulation환경발생DecisionEngine
    {
        public Simulation환경발생DecisionPlan Decide(
            string worldSeed,
            Simulation환경발생RuleCatalog catalog,
            Simulation환경발생ContextSnapshot context)
        {
            Validate(worldSeed, catalog, context);
            var catalogHash = Simulation세계자산CanonicalHash
                .ComputeSpawnCatalogHash(catalog);
            if (!string.Equals(catalog.CatalogHashSha256, catalogHash,
                    StringComparison.Ordinal))
                throw new ArgumentException(
                    "SimulationEnvironmentSpawnCatalogHashMismatch",
                    nameof(catalog));

            var contextValues = context.Values.ToDictionary(
                value => value.ContextCode.Trim(), value => value.Value,
                StringComparer.Ordinal);
            var decisions = new List<Simulation환경발생Decision>();
            foreach (var candidate in catalog.Candidates
                         .OrderBy(value => value.CandidateStableId,
                             StringComparer.Ordinal))
            {
                var effectiveWeight = EffectiveWeight(candidate,
                    contextValues, catalog.MaximumWeight);
                for (var slotIndex = 0;
                     slotIndex < candidate.MaximumInstancesPerCell;
                     slotIndex++)
                {
                    var fingerprint = string.Join("|", new[]
                    {
                        worldSeed.Trim(),
                        context.CellStableId.Trim(),
                        context.SpawnEpoch.ToString(
                            CultureInfo.InvariantCulture),
                        catalog.Revision.Trim(),
                        candidate.CandidateStableId.Trim(),
                        slotIndex.ToString(CultureInfo.InvariantCulture),
                        context.SourceChangeProjectionHashSha256.Trim(),
                    });
                    var decisionHash = Simulation세계자산CanonicalHash
                        .Hash(fingerprint);
                    var roll = Roll(decisionHash, catalog.MaximumWeight);
                    decisions.Add(new Simulation환경발생Decision
                    {
                        DecisionStableId = "environment-spawn-decision:"
                            + decisionHash.Substring(0, 24),
                        CandidateStableId = candidate.CandidateStableId.Trim(),
                        CategoryCode = candidate.CategoryCode.Trim(),
                        CompositionKey = candidate.CompositionKey.Trim(),
                        AuthorityKindCode = candidate.AuthorityKindCode.Trim(),
                        PersistenceKindCode = candidate.PersistenceKindCode.Trim(),
                        SlotIndex = slotIndex,
                        EffectiveWeight = effectiveWeight,
                        DeterministicRoll = roll,
                        Selected = effectiveWeight > roll,
                        RequiresWorldTickCommit = string.Equals(
                            candidate.AuthorityKindCode,
                            Simulation세계자산배치Codes.SimulationEntity,
                            StringComparison.Ordinal),
                        PresentationOnly = candidate.PresentationOnly,
                    });
                }
            }

            var plan = new Simulation환경발생DecisionPlan
            {
                RuleRevision = catalog.Revision.Trim(),
                RuleCatalogHashSha256 = catalogHash,
                WorldSeed = worldSeed.Trim(),
                CellStableId = context.CellStableId.Trim(),
                SpawnEpoch = context.SpawnEpoch,
                SourceWorldRevision = context.SourceWorldRevision,
                SourceChangeProjectionHashSha256 =
                    context.SourceChangeProjectionHashSha256.Trim(),
                Decisions = decisions
                    .OrderBy(value => value.DecisionStableId,
                        StringComparer.Ordinal).ToArray(),
            };
            plan.DecisionPlanHashSha256 = Simulation세계자산CanonicalHash
                .ComputeSpawnDecisionPlanHash(plan);
            return plan;
        }

        private static int EffectiveWeight(
            Simulation환경발생Candidate candidate,
            IReadOnlyDictionary<string, int> contextValues,
            int maximumWeight)
        {
            long value = candidate.BaseWeight;
            foreach (var modifier in candidate.WeightModifiers
                         .OrderBy(item => item.ContextCode,
                             StringComparer.Ordinal)
                         .ThenBy(item => item.MinimumContextValue))
            {
                contextValues.TryGetValue(modifier.ContextCode.Trim(),
                    out var contextValue);
                if (contextValue < modifier.MinimumContextValue) continue;
                var steps = contextValue - modifier.MinimumContextValue + 1L;
                value += steps * modifier.WeightDeltaPerStep;
            }

            if (value < 0L) return 0;
            if (value > maximumWeight) return maximumWeight;
            return (int) value;
        }

        private static int Roll(string hash, int maximumWeight)
        {
            var value = uint.Parse(hash.Substring(0, 8),
                NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return (int) (value % (uint) maximumWeight);
        }

        private static void Validate(
            string worldSeed,
            Simulation환경발생RuleCatalog catalog,
            Simulation환경발생ContextSnapshot context)
        {
            if (string.IsNullOrWhiteSpace(worldSeed))
                throw new ArgumentException(
                    "SimulationEnvironmentSpawnWorldSeedMissing",
                    nameof(worldSeed));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (!string.Equals(catalog.SchemaVersion,
                    Simulation세계자산배치Codes.SpawnSchemaVersion,
                    StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(catalog.Revision)
                || catalog.MaximumWeight <= 0
                || catalog.Candidates == null)
                throw new ArgumentException(
                    "SimulationEnvironmentSpawnCatalogInvalid",
                    nameof(catalog));
            if (string.IsNullOrWhiteSpace(context.CellStableId)
                || context.SpawnEpoch < 0
                || context.SourceWorldRevision < 0
                || !IsSha256(context.SourceChangeProjectionHashSha256)
                || context.Values == null
                || context.Values.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.ContextCode))
                || context.Values.Select(value => value.ContextCode.Trim())
                    .Distinct(StringComparer.Ordinal).Count()
                    != context.Values.Length)
                throw new ArgumentException(
                    "SimulationEnvironmentSpawnContextInvalid",
                    nameof(context));
            if (catalog.Candidates.Any(candidate => candidate == null
                    || string.IsNullOrWhiteSpace(candidate.CandidateStableId)
                    || string.IsNullOrWhiteSpace(candidate.CategoryCode)
                    || string.IsNullOrWhiteSpace(candidate.CompositionKey)
                    || candidate.BaseWeight < 0
                    || candidate.BaseWeight > catalog.MaximumWeight
                    || candidate.MaximumInstancesPerCell < 1
                    || candidate.MinimumSpacingMeters < 0d
                    || !IsAuthorityKind(candidate.AuthorityKindCode)
                    || !IsPersistenceKind(candidate.PersistenceKindCode)
                    || candidate.WeightModifiers == null
                    || candidate.WeightModifiers.Any(modifier =>
                        modifier == null
                        || string.IsNullOrWhiteSpace(
                            modifier.ContextCode)))
                || catalog.Candidates.Select(value =>
                        value.CandidateStableId.Trim())
                    .Distinct(StringComparer.Ordinal).Count()
                    != catalog.Candidates.Length)
                throw new ArgumentException(
                    "SimulationEnvironmentSpawnCandidateInvalid",
                    nameof(catalog));
        }

        private static bool IsAuthorityKind(string value)
            => string.Equals(value,
                    Simulation세계자산배치Codes.SimulationEntity,
                    StringComparison.Ordinal)
               || string.Equals(value,
                    Simulation세계자산배치Codes.DerivedWorldProp,
                    StringComparison.Ordinal)
               || string.Equals(value,
                    Simulation세계자산배치Codes.AmbientPresentation,
                    StringComparison.Ordinal);

        private static bool IsPersistenceKind(string value)
            => string.Equals(value,
                    Simulation세계자산배치Codes.Persistent,
                    StringComparison.Ordinal)
               || string.Equals(value,
                    Simulation세계자산배치Codes.DerivedPersistent,
                    StringComparison.Ordinal)
               || string.Equals(value,
                    Simulation세계자산배치Codes.Transient,
                    StringComparison.Ordinal);

        private static bool IsSha256(string value)
            => !string.IsNullOrWhiteSpace(value) && value.Length == 64
               && value.All(character =>
                   character >= '0' && character <= '9'
                   || character >= 'a' && character <= 'f'
                   || character >= 'A' && character <= 'F');
    }

    public static class Simulation세계자산CanonicalHash
    {
        public static string ComputeMapPlanHash(Simulation지도구성Plan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            var canonical = new StringBuilder();
            Add(canonical, plan.SchemaVersion);
            Add(canonical, plan.GeneratorRevision);
            Add(canonical, plan.WorldSeed);
            Add(canonical, plan.CellStableId);
            Add(canonical, plan.CellX);
            Add(canonical, plan.CellY);
            Add(canonical, plan.WindowRoleCode);
            Add(canonical, plan.SourceWorldRevision);
            Add(canonical, plan.SurfaceModeCode);
            foreach (var binding in plan.HBindings.OrderBy(value =>
                         value.HLevelCode + "|" + value.SpatialStableId,
                         StringComparer.Ordinal))
            {
                Add(canonical, binding.HLevelCode);
                Add(canonical, binding.SpatialStableId);
                Add(canonical, binding.StateCode);
                AddStrings(canonical, binding.WorldInteractionIds);
            }
            foreach (var connector in plan.Connectors.OrderBy(value =>
                         value.ConnectorStableId, StringComparer.Ordinal))
            {
                Add(canonical, connector.ConnectorStableId);
                Add(canonical, connector.SideCode);
                Add(canonical, connector.NeighborCellStableId);
                Add(canonical, connector.BoundaryHashSha256);
                Add(canonical, connector.Passable);
            }
            foreach (var anchor in plan.Anchors.OrderBy(value =>
                         value.AnchorStableId, StringComparer.Ordinal))
            {
                Add(canonical, anchor.AnchorStableId);
                Add(canonical, anchor.AnchorRoleCode);
                Add(canonical, anchor.H1StableId);
                Add(canonical, anchor.PreferredCompositionKey);
                Add(canonical, anchor.LocalXMeters);
                Add(canonical, anchor.LocalZMeters);
                Add(canonical, anchor.RotationDegrees);
                Add(canonical, anchor.MaximumSlopeDegrees);
                AddStrings(canonical, anchor.AllowedAssetCategoryCodes);
                Add(canonical, anchor.FixedAnchor);
            }
            AddStrings(canonical, plan.RequiredCapabilityCodes);
            return Hash(canonical.ToString());
        }

        public static string ComputeChangeProjectionHash(
            Simulation공간변화ProjectionSnapshot projection)
        {
            if (projection == null)
                throw new ArgumentNullException(nameof(projection));
            var canonical = new StringBuilder();
            Add(canonical, projection.SchemaVersion);
            Add(canonical, projection.ProjectionRevision);
            Add(canonical, projection.AreaSetStableId);
            Add(canonical, projection.CellStableId);
            Add(canonical, projection.SourceWorldRevision);
            foreach (var fact in projection.Facts.OrderBy(value =>
                         value.ChangeStableId, StringComparer.Ordinal))
            {
                Add(canonical, fact.ChangeStableId);
                Add(canonical, fact.TriggerSourceCode);
                Add(canonical, fact.WorldInteractionId);
                Add(canonical, fact.EffectCode);
                Add(canonical, fact.AreaSetStableId);
                Add(canonical, fact.SpatialStableId);
                Add(canonical, fact.TargetStableId);
                Add(canonical, fact.ChangeCode);
                Add(canonical, fact.ChangeValue);
                Add(canonical, fact.Quantity);
                Add(canonical, fact.StateCode);
                Add(canonical, fact.LocalXMeters);
                Add(canonical, fact.LocalZMeters);
                Add(canonical, fact.RotationDegrees);
                Add(canonical, fact.FormationModeCode);
                Add(canonical, fact.AppliedWorldRevision);
            }
            return Hash(canonical.ToString());
        }

        public static string ComputeSpawnCatalogHash(
            Simulation환경발생RuleCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            var canonical = new StringBuilder();
            Add(canonical, catalog.SchemaVersion);
            Add(canonical, catalog.Revision);
            Add(canonical, catalog.MaximumWeight);
            foreach (var candidate in catalog.Candidates.OrderBy(value =>
                         value.CandidateStableId, StringComparer.Ordinal))
            {
                Add(canonical, candidate.CandidateStableId);
                Add(canonical, candidate.CategoryCode);
                Add(canonical, candidate.CompositionKey);
                Add(canonical, candidate.AuthorityKindCode);
                Add(canonical, candidate.PersistenceKindCode);
                Add(canonical, candidate.BaseWeight);
                Add(canonical, candidate.MaximumInstancesPerCell);
                Add(canonical, candidate.MinimumSpacingMeters);
                AddStrings(canonical, candidate.AllowedHLevelCodes);
                foreach (var modifier in candidate.WeightModifiers
                             .OrderBy(value => value.ContextCode,
                                 StringComparer.Ordinal)
                             .ThenBy(value => value.MinimumContextValue))
                {
                    Add(canonical, modifier.ContextCode);
                    Add(canonical, modifier.MinimumContextValue);
                    Add(canonical, modifier.WeightDeltaPerStep);
                }
                Add(canonical, candidate.PresentationOnly);
            }
            return Hash(canonical.ToString());
        }

        public static string ComputeSpawnDecisionPlanHash(
            Simulation환경발생DecisionPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            var canonical = new StringBuilder();
            Add(canonical, plan.SchemaVersion);
            Add(canonical, plan.RuleRevision);
            Add(canonical, plan.RuleCatalogHashSha256);
            Add(canonical, plan.WorldSeed);
            Add(canonical, plan.CellStableId);
            Add(canonical, plan.SpawnEpoch);
            Add(canonical, plan.SourceWorldRevision);
            Add(canonical, plan.SourceChangeProjectionHashSha256);
            foreach (var decision in plan.Decisions.OrderBy(value =>
                         value.DecisionStableId, StringComparer.Ordinal))
            {
                Add(canonical, decision.DecisionStableId);
                Add(canonical, decision.CandidateStableId);
                Add(canonical, decision.CategoryCode);
                Add(canonical, decision.CompositionKey);
                Add(canonical, decision.AuthorityKindCode);
                Add(canonical, decision.PersistenceKindCode);
                Add(canonical, decision.SlotIndex);
                Add(canonical, decision.EffectiveWeight);
                Add(canonical, decision.DeterministicRoll);
                Add(canonical, decision.Selected);
                Add(canonical, decision.RequiresWorldTickCommit);
                Add(canonical, decision.PresentationOnly);
            }
            return Hash(canonical.ToString());
        }

        public static string ComputeAssetPlacementPlanHash(
            Simulation세계자산배치Plan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            var canonical = new StringBuilder();
            Add(canonical, plan.SchemaVersion);
            Add(canonical, plan.RuleRevision);
            Add(canonical, plan.CellStableId);
            Add(canonical, plan.SourceWorldRevision);
            Add(canonical, plan.MapPlanHashSha256);
            Add(canonical, plan.ChangeProjectionHashSha256);
            Add(canonical, plan.SpawnDecisionPlanHashSha256);
            foreach (var placement in plan.Placements.OrderBy(value =>
                         value.PlacementStableId, StringComparer.Ordinal))
            {
                Add(canonical, placement.PlacementStableId);
                Add(canonical, placement.ParentPlacementStableId);
                Add(canonical, placement.OwnerCellStableId);
                Add(canonical, placement.PlacementKindCode);
                Add(canonical, placement.LayerCode);
                Add(canonical, placement.CategoryCode);
                Add(canonical, placement.CompositionKey);
                Add(canonical, placement.H1StableId);
                Add(canonical, placement.AuthorityKindCode);
                Add(canonical, placement.PersistenceKindCode);
                Add(canonical, placement.StateCode);
                Add(canonical, placement.LocalXMeters);
                Add(canonical, placement.LocalYMeters);
                Add(canonical, placement.LocalZMeters);
                Add(canonical, placement.RotationDegrees);
                Add(canonical, placement.UniformScale);
                Add(canonical, placement.FixedAnchor);
                Add(canonical, placement.CollisionEligible);
                Add(canonical, placement.PresentationOnly);
                Add(canonical, placement.SourceSpawnDecisionStableId);
                AddStrings(canonical, placement.SourceChangeStableIds);
            }
            foreach (var handle in plan.InteriorPlanHandles.OrderBy(value =>
                         value.BuildingPlacementStableId,
                         StringComparer.Ordinal))
            {
                Add(canonical, handle.SchemaVersion);
                Add(canonical, handle.BuildingPlacementStableId);
                Add(canonical, handle.H1StableId);
                Add(canonical, handle.InteriorDefinitionRevision);
                Add(canonical, handle.ReferenceCatalogRevision);
                Add(canonical, handle.ReferenceCatalogHashSha256);
                Add(canonical, handle.PlacementControlRuleRevision);
                Add(canonical, handle.VisualMetricCatalogRevision);
                Add(canonical, handle.VisualMetricCatalogHashSha256);
                Add(canonical, handle.AdjustmentRevision);
                Add(canonical, handle.InteriorPlacementPlanHashSha256);
            }
            AddStrings(canonical, plan.InteriorPlanBodies.Select(value =>
                value.BodyHashSha256));
            return Hash(canonical.ToString());
        }

        public static string ComputeInteriorBodyHash(
            SimulationInteriorPlacementPlanBodySnapshot body)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            var canonical = new StringBuilder();
            Add(canonical, body.SchemaVersion);
            Add(canonical, body.BuildingPlacementStableId);
            Add(canonical, body.H1StableId);
            Add(canonical, body.SourceInteriorPlanSchemaVersion);
            Add(canonical, body.InteriorPlacementPlanHashSha256);
            foreach (var placement in body.Placements.OrderBy(value =>
                         value.PlacementStableId, StringComparer.Ordinal))
            {
                Add(canonical, placement.PlacementStableId);
                Add(canonical, placement.ParentPlacementStableId);
                Add(canonical, placement.ZoneStableId);
                Add(canonical, placement.OwningH1StableId);
                Add(canonical, placement.PlacementLayerCode);
                Add(canonical, placement.PlacementRoleCode);
                Add(canonical, placement.VisualKey);
                Add(canonical, placement.LocalX);
                Add(canonical, placement.LocalY);
                Add(canonical, placement.LocalZ);
                Add(canonical, placement.LocalRotationDegrees);
                Add(canonical, placement.UniformScale);
                Add(canonical, placement.ReferenceStableId);
                AddStrings(canonical, placement.PresentationFlags);
            }
            return Hash(canonical.ToString());
        }

        public static string ComputeStateHash(
            SimulationWorldAssetPlacementStateSnapshot state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            var canonical = new StringBuilder();
            Add(canonical, state.SchemaVersion);
            Add(canonical, state.SourceWorldRevision);
            AddStrings(canonical, state.MapPlans.Select(value =>
                value.MapPlanHashSha256));
            AddStrings(canonical, state.ChangeProjections.Select(value =>
                value.ProjectionHashSha256));
            AddStrings(canonical, state.SpawnDecisionPlans.Select(value =>
                value.DecisionPlanHashSha256));
            AddStrings(canonical, state.AssetPlacementPlans.Select(value =>
                value.AssetPlacementPlanHashSha256));
            AddStrings(canonical, state.InteriorPlanBodies.Select(value =>
                value.BodyHashSha256));
            return Hash(canonical.ToString());
        }

        public static string Hash(string text)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(text ?? string.Empty)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        private static void AddStrings(
            StringBuilder target, IEnumerable<string>? values)
        {
            var ordered = (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            Add(target, ordered.Length);
            foreach (var value in ordered) Add(target, value);
        }

        private static void Add(StringBuilder target, object? value)
            => target.Append(value switch
                {
                    null => string.Empty,
                    bool boolean => boolean ? "1" : "0",
                    double number => number.ToString("R",
                        CultureInfo.InvariantCulture),
                    IFormattable formattable => formattable.ToString(
                        null, CultureInfo.InvariantCulture),
                    _ => value.ToString(),
                })
                .Append('\n');
    }
}
