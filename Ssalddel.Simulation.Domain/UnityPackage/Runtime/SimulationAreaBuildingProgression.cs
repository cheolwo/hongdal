using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private Simulation영역건물발전CatalogSnapshot? areaBuildingCatalog;
        private readonly Dictionary<string, Simulation건물발전NodeSnapshot>
            natureBuildingNodes = new(StringComparer.Ordinal);
        private Simulation학습방문Snapshot? natureLearningVisit;

        public Simulation영역건물발전Snapshot GetAreaBuildingProgression(
            string areaCode)
        {
            lock (gate)
            {
                EnsureNatureSurvivalEnabled();
                return CreateAreaBuildingProgressionSnapshot(areaCode);
            }
        }

        private void InitializeAreaBuildingProgression(
            SimulationNatureSurvivalInitialStateRequest request)
        {
            if (!SimulationNatureSurvivalCodes.IsR3(request.ProfileRevision)) return;
            if (request.BuildingProgressionCatalog == null)
                throw new SimulationContractException(
                    Simulation영역건물발전Codes.CatalogInvalid);

            Simulation영역건물발전Catalog.Validate(
                request.BuildingProgressionCatalog);
            areaBuildingCatalog = Simulation영역건물발전Catalog.Clone(
                request.BuildingProgressionCatalog);
            foreach (var blueprint in areaBuildingCatalog.Blueprints.Where(value =>
                         value.AreaCode == Simulation영역건물발전Codes.Nature))
            {
                natureBuildingNodes.Add(blueprint.BlueprintStableId,
                    new Simulation건물발전NodeSnapshot
                    {
                        BlueprintStableId = blueprint.BlueprintStableId,
                        AreaCode = blueprint.AreaCode,
                        StageCode = blueprint.StageCode,
                        KoreanName = blueprint.KoreanName,
                        H1StableId = blueprint.H1StableId,
                        FacilityStableId = blueprint.FacilityStableId,
                        StateCode = blueprint.BlueprintStableId ==
                            Simulation영역건물발전Codes.NatureCabinBlueprint
                            ? Simulation영역건물발전Codes.Planned
                            : Simulation영역건물발전Codes.Locked,
                        RequiredWorkSeconds = blueprint.ConstructionSeconds,
                    });
            }
        }

        private void AppendNatureBuildingPreview(
            SimulationNatureSurvivalActionPreviewRequest request,
            ICollection<string> reasons,
            out Simulation건물청사진Definition? blueprint)
        {
            blueprint = null;
            if (!IsNatureR3 || areaBuildingCatalog == null)
            {
                reasons.Add(SimulationNatureSurvivalCodes.ActionBlocked);
                return;
            }

            var blueprintId = NormalizeOptional(request.TargetStableId);
            blueprint = areaBuildingCatalog.Blueprints.SingleOrDefault(value =>
                value.BlueprintStableId == blueprintId
                && value.AreaCode == Simulation영역건물발전Codes.Nature);
            if (blueprint == null
                || blueprint.BlueprintStableId ==
                    Simulation영역건물발전Codes.NatureCabinBlueprint)
            {
                reasons.Add(Simulation영역건물발전Codes.BlueprintInvalid);
                return;
            }

            if (!natureDay2Ready)
                reasons.Add(Simulation영역건물발전Codes.Day2Required);
            if (!naturePlayerInsideCabin)
                reasons.Add(Simulation영역건물발전Codes.CabinAccessRequired);
            if (natureActiveWork != null)
                reasons.Add(Simulation영역건물발전Codes.ConstructionActive);

            var node = natureBuildingNodes[blueprint.BlueprintStableId];
            if (node.StateCode == Simulation영역건물발전Codes.Operational)
                reasons.Add(Simulation영역건물발전Codes.AlreadyOperational);
            foreach (var required in blueprint.RequiredOperationalBlueprintStableIds)
            {
                if (!natureBuildingNodes.TryGetValue(required, out var requiredNode)
                    || requiredNode.StateCode !=
                        Simulation영역건물발전Codes.Operational)
                    reasons.Add(Simulation영역건물발전Codes.BlueprintLocked);
            }

            if (NatureAvailableTimberQuantity() < blueprint.RequiredTimberQuantity)
                reasons.Add(Simulation영역건물발전Codes.TimberInsufficient);
            if (NaturePlayerItemQuantity(
                    SimulationNatureSurvivalCodes.RebuildPartItemCode)
                < blueprint.RequiredRebuildPartQuantity)
                reasons.Add(Simulation영역건물발전Codes.RebuildPartInsufficient);
            if (Math.Abs(request.LocalX - natureCabin.LocalX) > 25d
                || Math.Abs(request.LocalZ - natureCabin.LocalZ) > 25d)
                reasons.Add(Simulation영역건물발전Codes.PlacementOutsideHome);
            if (OverlapsOperationalNatureBuilding(blueprint, request.LocalX,
                    request.LocalZ))
                reasons.Add(Simulation영역건물발전Codes.PlacementOverlap);
        }

        private void BeginNatureBuildingConstruction(
            SimulationNatureSurvivalCommandRequest request)
        {
            var blueprint = areaBuildingCatalog!.Blueprints.Single(value =>
                value.BlueprintStableId == request.TargetStableId.Trim());
            ConsumeNatureBuildingTimber(blueprint.RequiredTimberQuantity);
            ConsumeNaturePlayerItem(
                SimulationNatureSurvivalCodes.RebuildPartItemCode,
                blueprint.RequiredRebuildPartQuantity);

            var node = natureBuildingNodes[blueprint.BlueprintStableId];
            node.StateCode = Simulation영역건물발전Codes.Building;
            node.LocalX = request.LocalX;
            node.LocalZ = request.LocalZ;
            node.YawDegrees = NormalizeYaw(request.YawDegrees);
            node.CompletedWorkSeconds = 0;
            natureActiveWork = new SimulationNatureActiveWorkSnapshot
            {
                WorkKindCode = Simulation영역건물발전Codes.ExpansionBuildWorkKind,
                TargetStableId = blueprint.BlueprintStableId,
                RequiredWorkSeconds = blueprint.ConstructionSeconds,
            };
        }

        private void CompleteNatureBuildingConstruction()
        {
            var node = natureBuildingNodes[natureActiveWork!.TargetStableId];
            node.CompletedWorkSeconds = natureActiveWork.CompletedWorkSeconds;
            node.StateCode = Simulation영역건물발전Codes.Operational;
            natureNoiseEventCount++;
            CompleteLatestWorldInteractionManifestation(
                Simulation영역건물발전Codes.ConstructionWorldInteractionId,
                new[] { "effect:nature:building-operational:" + node.BlueprintStableId },
                new[] { "AreaBuildingOperational" }, Revision + 1L);
        }

        private void CancelNatureBuildingConstruction(
            SimulationNatureActiveWorkSnapshot cancelled)
        {
            if (areaBuildingCatalog == null
                || !natureBuildingNodes.TryGetValue(cancelled.TargetStableId,
                    out var node)) return;
            var blueprint = areaBuildingCatalog.Blueprints.Single(value =>
                value.BlueprintStableId == cancelled.TargetStableId);
            AddNaturePlayerItem(natureSurvivalCreationState!.PlayerStableId,
                SimulationNatureSurvivalCodes.TimberItemCode, "통나무",
                blueprint.RequiredTimberQuantity);
            AddNaturePlayerItem(natureSurvivalCreationState.PlayerStableId,
                SimulationNatureSurvivalCodes.RebuildPartItemCode, "재건 부품",
                blueprint.RequiredRebuildPartQuantity);
            node.StateCode = Simulation영역건물발전Codes.Available;
            node.CompletedWorkSeconds = 0;
        }

        private Simulation영역건물발전Snapshot?
            CreateNatureBuildingProgressionSnapshot()
            => areaBuildingCatalog == null ? null
                : CreateAreaBuildingProgressionSnapshot(
                    Simulation영역건물발전Codes.Nature);

        private Simulation영역건물발전Snapshot
            CreateAreaBuildingProgressionSnapshot(string areaCode)
        {
            if (areaBuildingCatalog == null)
                return new Simulation영역건물발전Snapshot
                {
                    AreaCode = NormalizeOptional(areaCode),
                    AreaSetStableId = natureSurvivalCreationState?.AreaSetStableId
                        ?? string.Empty,
                };
            if (!areaBuildingCatalog.Blueprints.Any(value =>
                    value.AreaCode == areaCode))
                throw new SimulationContractException(
                    Simulation영역건물발전Codes.BlueprintInvalid);

            SyncNatureCabinBuildingNode();
            var nodes = areaBuildingCatalog.Blueprints
                .Where(value => value.AreaCode == areaCode)
                .OrderBy(value => StageOrder(value.StageCode))
                .ThenBy(value => value.BlueprintStableId, StringComparer.Ordinal)
                .Select(value => CreateBuildingNodeSnapshot(value, areaCode))
                .ToArray();
            return new Simulation영역건물발전Snapshot
            {
                CatalogRevision = areaBuildingCatalog.Revision,
                CatalogHashSha256 = areaBuildingCatalog.HashSha256,
                AreaCode = areaCode,
                AreaSetStableId = areaCode == Simulation영역건물발전Codes.Nature
                    ? natureSurvivalCreationState!.AreaSetStableId : string.Empty,
                Nodes = nodes,
                ApprovedTeachingMaterials = areaCode ==
                    Simulation영역건물발전Codes.Nature
                    ? areaBuildingCatalog.ApprovedTeachingMaterials
                        .Where(value => value.AdminApproved)
                        .OrderBy(value => value.TeachingMaterialStableId,
                            StringComparer.Ordinal)
                        .Select(Simulation영역건물발전Catalog.Clone).ToArray()
                    : Array.Empty<Simulation승인가르침자료Snapshot>(),
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private Simulation건물발전NodeSnapshot CreateBuildingNodeSnapshot(
            Simulation건물청사진Definition blueprint, string areaCode)
        {
            if (areaCode == Simulation영역건물발전Codes.Nature
                && natureBuildingNodes.TryGetValue(blueprint.BlueprintStableId,
                    out var existing))
            {
                var copy = CloneAreaBuildingNode(existing);
                var blocks = BuildingNodeBlocks(blueprint);
                if (copy.StateCode == Simulation영역건물발전Codes.Locked
                    || copy.StateCode == Simulation영역건물발전Codes.Available)
                    copy.StateCode = blocks.Length == 0
                        ? Simulation영역건물발전Codes.Available
                        : Simulation영역건물발전Codes.Locked;
                copy.BlockingReasonCodes = blocks;
                copy.IsDay2Priority = IsNatureDay2Priority(blueprint.BlueprintStableId);
                return copy;
            }

            var externalBlocks = blueprint.RequiredOperationalBlueprintStableIds.Length == 0
                ? Array.Empty<string>()
                : new[] { Simulation영역건물발전Codes.BlueprintLocked };
            return new Simulation건물발전NodeSnapshot
            {
                BlueprintStableId = blueprint.BlueprintStableId,
                AreaCode = blueprint.AreaCode,
                StageCode = blueprint.StageCode,
                KoreanName = blueprint.KoreanName,
                H1StableId = blueprint.H1StableId,
                FacilityStableId = blueprint.FacilityStableId,
                StateCode = externalBlocks.Length == 0
                    ? Simulation영역건물발전Codes.Available
                    : Simulation영역건물발전Codes.Locked,
                BlockingReasonCodes = externalBlocks,
                RequiredWorkSeconds = blueprint.ConstructionSeconds,
            };
        }

        private string[] BuildingNodeBlocks(
            Simulation건물청사진Definition blueprint)
        {
            if (blueprint.BlueprintStableId ==
                Simulation영역건물발전Codes.NatureCabinBlueprint)
                return Array.Empty<string>();
            var blocks = new List<string>();
            if (!natureDay2Ready)
                blocks.Add(Simulation영역건물발전Codes.Day2Required);
            if (blueprint.RequiredOperationalBlueprintStableIds.Any(required =>
                    !natureBuildingNodes.TryGetValue(required, out var requiredNode)
                    || requiredNode.StateCode !=
                        Simulation영역건물발전Codes.Operational))
                blocks.Add(Simulation영역건물발전Codes.BlueprintLocked);
            return blocks.Distinct().ToArray();
        }

        private void SyncNatureCabinBuildingNode()
        {
            if (!natureBuildingNodes.TryGetValue(
                    Simulation영역건물발전Codes.NatureCabinBlueprint,
                    out var cabinNode)) return;
            cabinNode.StateCode = natureCabin.StateCode switch
            {
                SimulationNatureSurvivalCodes.Completed =>
                    Simulation영역건물발전Codes.Operational,
                SimulationNatureSurvivalCodes.Building =>
                    Simulation영역건물발전Codes.Building,
                _ => Simulation영역건물발전Codes.Planned,
            };
            cabinNode.CompletedWorkSeconds = natureCabin.CompletedWorkSeconds;
            cabinNode.LocalX = natureCabin.LocalX;
            cabinNode.LocalZ = natureCabin.LocalZ;
            cabinNode.YawDegrees = natureCabin.YawDegrees;
        }

        private void AdvanceNatureLearningVisit()
        {
            if (!IsNatureR3 || areaBuildingCatalog == null
                || !natureBuildingNodes.TryGetValue(
                    Simulation영역건물발전Codes.NatureLearningLodgeBlueprint,
                    out var lodge)
                || lodge.StateCode != Simulation영역건물발전Codes.Operational)
                return;

            var absolute = natureCycleIndex * NatureSurvivalRules.CycleSeconds
                + natureElapsedSecondsInCycle;
            if (natureLearningVisit == null)
            {
                if (NatureSurvivalRules.PhaseAt(natureElapsedSecondsInCycle)
                    != NatureSurvivalClockPhaseCodes.Daylight) return;
                var material = areaBuildingCatalog.ApprovedTeachingMaterials
                    .Where(value => value.AdminApproved)
                    .OrderBy(value => value.TeachingMaterialStableId,
                        StringComparer.Ordinal).First();
                natureLearningVisit = new Simulation학습방문Snapshot
                {
                    VisitStableId = "learning-visit:nature:" + natureCycleIndex,
                    NpcStableId = "npc:nature:learner-01",
                    BuildingFacilityStableId = lodge.FacilityStableId,
                    TeachingMaterialStableId = material.TeachingMaterialStableId,
                    StateCode = "Learning",
                    StartedCycleIndex = natureCycleIndex,
                    StartedAtSecond = natureElapsedSecondsInCycle,
                    CompletedAtSecond = -1,
                    SimulationOnly = true,
                };
                return;
            }

            if (natureLearningVisit.StateCode == "Learning")
            {
                var startedAbsolute = natureLearningVisit.StartedCycleIndex
                    * NatureSurvivalRules.CycleSeconds
                    + natureLearningVisit.StartedAtSecond;
                if (absolute - startedAbsolute < 30) return;
                natureLearningVisit.StateCode = "Completed";
                natureLearningVisit.CompletedAtSecond =
                    natureElapsedSecondsInCycle;
                lodge.CompletedLearningVisitCount++;
            }
        }

        private int NatureAvailableTimberQuantity()
            => NaturePlayerItemQuantity(SimulationNatureSurvivalCodes.TimberItemCode)
                + NatureCabinStoredTimberQuantity();

        private void ConsumeNatureBuildingTimber(int quantity)
        {
            var fromStored = Math.Min(quantity, NatureCabinStoredTimberQuantity());
            if (fromStored > 0)
                worldInventoryItemStacks[
                    SimulationNatureSurvivalCodes.CabinStorageTimberStackStableId]
                    .Quantity -= fromStored;
            var carried = quantity - fromStored;
            if (carried > 0)
                ConsumeNaturePlayerItem(
                    SimulationNatureSurvivalCodes.TimberItemCode, carried);
        }

        private bool OverlapsOperationalNatureBuilding(
            Simulation건물청사진Definition candidate, double localX, double localZ)
        {
            if (areaBuildingCatalog == null) return false;
            foreach (var node in natureBuildingNodes.Values.Where(value =>
                         value.StateCode == Simulation영역건물발전Codes.Operational
                         || value.StateCode == Simulation영역건물발전Codes.Building))
            {
                if (node.BlueprintStableId ==
                    Simulation영역건물발전Codes.NatureCabinBlueprint
                    && natureCabin.StateCode != SimulationNatureSurvivalCodes.Completed)
                    continue;
                var existing = areaBuildingCatalog.Blueprints.Single(value =>
                    value.BlueprintStableId == node.BlueprintStableId);
                var required = (Math.Max(candidate.FootprintWidthCentimeters,
                                    candidate.FootprintDepthCentimeters)
                                + Math.Max(existing.FootprintWidthCentimeters,
                                    existing.FootprintDepthCentimeters)
                                + candidate.ClearanceCentimeters
                                + existing.ClearanceCentimeters) / 200d;
                var dx = node.LocalX - localX;
                var dz = node.LocalZ - localZ;
                if (dx * dx + dz * dz < required * required) return true;
            }
            return false;
        }

        private bool IsNatureDay2Priority(string blueprintId)
            => natureSelectedExpansionPlanCode switch
            {
                SimulationNatureSurvivalCodes.Workbench => blueprintId ==
                    Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                SimulationNatureSurvivalCodes.StorageRack => blueprintId ==
                    Simulation영역건물발전Codes.NatureStorageRackBlueprint,
                SimulationNatureSurvivalCodes.Palisade => blueprintId ==
                    Simulation영역건물발전Codes.NaturePalisadeBlueprint,
                _ => false,
            };

        private static int StageOrder(string stageCode)
            => stageCode switch
            {
                Simulation영역건물발전Codes.Foundation => 0,
                Simulation영역건물발전Codes.Operations => 1,
                Simulation영역건물발전Codes.Specialization => 2,
                Simulation영역건물발전Codes.Resilience => 3,
                Simulation영역건물발전Codes.Landmark => 4,
                _ => int.MaxValue,
            };

        private bool IsNatureR3 => natureSurvivalCreationState != null
            && SimulationNatureSurvivalCodes.IsR3(
                natureSurvivalCreationState.ProfileRevision);

        private static Simulation건물발전NodeSnapshot CloneAreaBuildingNode(
            Simulation건물발전NodeSnapshot source)
            => new Simulation건물발전NodeSnapshot
            {
                BlueprintStableId = source.BlueprintStableId,
                AreaCode = source.AreaCode,
                StageCode = source.StageCode,
                KoreanName = source.KoreanName,
                H1StableId = source.H1StableId,
                FacilityStableId = source.FacilityStableId,
                StateCode = source.StateCode,
                BlockingReasonCodes = source.BlockingReasonCodes.ToArray(),
                IsDay2Priority = source.IsDay2Priority,
                CompletedWorkSeconds = source.CompletedWorkSeconds,
                RequiredWorkSeconds = source.RequiredWorkSeconds,
                LocalX = source.LocalX,
                LocalZ = source.LocalZ,
                YawDegrees = source.YawDegrees,
                CompletedLearningVisitCount = source.CompletedLearningVisitCount,
            };

        private static Simulation영역건물발전Snapshot CloneAreaBuildingProgression(
            Simulation영역건물발전Snapshot source)
            => new Simulation영역건물발전Snapshot
            {
                CatalogRevision = source.CatalogRevision,
                CatalogHashSha256 = source.CatalogHashSha256,
                AreaCode = source.AreaCode,
                AreaSetStableId = source.AreaSetStableId,
                Nodes = source.Nodes.Select(CloneAreaBuildingNode).ToArray(),
                ApprovedTeachingMaterials = source.ApprovedTeachingMaterials
                    .Select(Simulation영역건물발전Catalog.Clone).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private static Simulation학습방문Snapshot? CloneLearningVisit(
            Simulation학습방문Snapshot? source)
            => source == null ? null : new Simulation학습방문Snapshot
            {
                VisitStableId = source.VisitStableId,
                NpcStableId = source.NpcStableId,
                BuildingFacilityStableId = source.BuildingFacilityStableId,
                TeachingMaterialStableId = source.TeachingMaterialStableId,
                StateCode = source.StateCode,
                StartedCycleIndex = source.StartedCycleIndex,
                StartedAtSecond = source.StartedAtSecond,
                CompletedAtSecond = source.CompletedAtSecond,
                SimulationOnly = source.SimulationOnly,
            };
    }
}
