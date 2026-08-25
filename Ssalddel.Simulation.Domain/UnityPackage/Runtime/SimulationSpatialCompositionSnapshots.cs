using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    internal static class SimulationSpatialCompositionSnapshots
    {
        internal static SimulationSpatialCompositionStateSnapshot Clone(
            SimulationSpatialCompositionStateSnapshot value)
            => new SimulationSpatialCompositionStateSnapshot
            {
                SchemaVersion = value.SchemaVersion,
                AreaCode = value.AreaCode,
                AreaSetStableId = value.AreaSetStableId,
                PlacementControlRevision = value.PlacementControlRevision,
                RuleCatalogRevision = value.RuleCatalogRevision,
                RuleCatalogHashSha256 = value.RuleCatalogHashSha256,
                WorldTick = value.WorldTick,
                WorldRevision = value.WorldRevision,
                Instances = value.Instances.Select(item =>
                    new SpatialCompositionInstanceSnapshot
                    {
                        SpatialInstanceStableId = item.SpatialInstanceStableId,
                        DefinitionStableId = item.DefinitionStableId,
                        LevelCode = item.LevelCode,
                        StateCode = item.StateCode,
                        ChildSpatialInstanceStableIds =
                            item.ChildSpatialInstanceStableIds.ToArray(),
                        FormedWorldTick = item.FormedWorldTick,
                        LastEvaluatedWorldTick = item.LastEvaluatedWorldTick,
                    }).ToArray(),
                Assessments = value.Assessments.Select(item =>
                    new SpatialCompositionAssessment
                    {
                        RuleStableId = item.RuleStableId,
                        TargetLevelCode = item.TargetLevelCode,
                        TargetDefinitionStableId =
                            item.TargetDefinitionStableId,
                        AuthorityCode = item.AuthorityCode,
                        StateCode = item.StateCode,
                        SpatialInstanceStableId =
                            item.SpatialInstanceStableId,
                        SatisfiedChildDefinitionStableIds =
                            item.SatisfiedChildDefinitionStableIds.ToArray(),
                        MissingChildDefinitionStableIds =
                            item.MissingChildDefinitionStableIds.ToArray(),
                        BlockReasonCodes = item.BlockReasonCodes.ToArray(),
                        SourcePlacementPlanHashes =
                            item.SourcePlacementPlanHashes.ToArray(),
                    }).ToArray(),
                GraphHashSha256 = value.GraphHashSha256,
                SimulationOnly = value.SimulationOnly,
                IsOperationalState = value.IsOperationalState,
            };

        internal static SpatialCompositionGraphHandle CreateHandle(
            SimulationSpatialCompositionStateSnapshot value)
            => new SpatialCompositionGraphHandle
            {
                SchemaVersion = value.SchemaVersion,
                AreaCode = value.AreaCode,
                AreaSetStableId = value.AreaSetStableId,
                RuleCatalogRevision = value.RuleCatalogRevision,
                RuleCatalogHashSha256 = value.RuleCatalogHashSha256,
                SourceWorldRevision = value.WorldRevision,
                GraphHashSha256 = value.GraphHashSha256,
            };
    }
}
