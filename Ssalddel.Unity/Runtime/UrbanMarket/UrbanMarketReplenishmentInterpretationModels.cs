using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class 도심마트ReplenishmentNeedCodes
    {
        public const string NoActionNeeded = "NoActionNeeded";
        public const string ReplenishmentCandidate = "ReplenishmentCandidate";
        public const string InboundRequired = "InboundRequired";
        public const string TaskAlreadyActive = "TaskAlreadyActive";
        public const string DataInsufficient = "DataInsufficient";
    }

    public static class 도심마트ReplenishmentBlockReasonCodes
    {
        public const string DisplayInventoryMissing = "DisplayInventoryMissing";
        public const string DisplayInventoryAmbiguous = "DisplayInventoryAmbiguous";
        public const string BackroomInventoryMissing = "BackroomInventoryMissing";
        public const string QuantityUnitMismatch = "QuantityUnitMismatch";
        public const string ActiveTaskExists = "ActiveTaskExists";
        public const string InventoryOversubscribed = "InventoryOversubscribed";
        public const string AllocationUnitMismatch = "AllocationUnitMismatch";
        public const string AvailableQuantityInsufficient = "AvailableQuantityInsufficient";
        public const string SourcePlanIncomplete = "SourcePlanIncomplete";
        public const string ServerCapabilityMissing = "ServerCapabilityMissing";
    }

    public sealed class 도심마트ReplenishmentRuleSet
    {
        public 도심마트ReplenishmentRuleSet(int targetFillPercent, string revision)
        {
            if (targetFillPercent <= 0 || targetFillPercent > 100)
                throw new ArgumentOutOfRangeException(nameof(targetFillPercent));
            if (string.IsNullOrWhiteSpace(revision))
                throw new ArgumentException("ReplenishmentRuleRevisionMissing", nameof(revision));
            TargetFillPercent = targetFillPercent;
            Revision = revision.Trim();
        }

        public int TargetFillPercent { get; }
        public string Revision { get; }

        public static 도심마트ReplenishmentRuleSet SimulationDefault()
            => new 도심마트ReplenishmentRuleSet(80, "urban-market-replenishment-simulation.v1");
    }

    public sealed class 도심마트진열보충WorldState
    {
        public WorldStableId ShelfWorldId { get; set; }
        public WorldStableId ProductWorldId { get; set; }
        public int DisplayQuantity { get; set; }
        public int DisplayCapacity { get; set; }
        public int TargetQuantity { get; set; }
        public int BackroomOnHandQuantity { get; set; }
        public int BackroomAllocatedQuantity { get; set; }
        public int BackroomAvailableQuantity { get; set; }
        public int ActiveTaskQuantity { get; set; }
        public int CandidateQuantity { get; set; }
        public 도심마트진열보충SourcePlanSegment[] SourcePlan { get; set; } = Array.Empty<도심마트진열보충SourcePlanSegment>();
        public bool IsSourcePlanComplete { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
        public string NeedCode { get; set; } = 도심마트ReplenishmentNeedCodes.DataInsufficient;
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public bool CanPreviewRequest { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public WorldStableId[] SourceWorldIds { get; set; } = Array.Empty<WorldStableId>();
    }

    public sealed class 도심마트진열보충SourcePlanSegment
    {
        public WorldStableId InventoryWorldId { get; set; }
        public WorldStableId LocationWorldId { get; set; }
        public int Quantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
    }

    /// <summary>
    /// 하나의 원천 재고에 대해 모든 비종료 작업의 점유량을 반영한 공유 World 상태입니다.
    /// 운영 예약의 최종 권위가 아니라, 허용된 Data Snapshot을 일관되게 해석한 결과입니다.
    /// </summary>
    public sealed class 도심마트재고가용성WorldState
    {
        public WorldStableId InventoryWorldId { get; set; }
        public WorldStableId ProductWorldId { get; set; }
        public WorldStableId LocationWorldId { get; set; }
        public int OnHandQuantity { get; set; }
        public int AllocatedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
        public bool IsOversubscribed { get; set; }
        public bool HasAllocationUnitMismatch { get; set; }
        public WorldStableId[] AllocatingTaskWorldIds { get; set; } = Array.Empty<WorldStableId>();
        public WorldStableId[] AllocatingAllocationWorldIds { get; set; } = Array.Empty<WorldStableId>();
    }

    public sealed class 도심마트운영업무WorldState
    {
        public 도심마트운영업무WorldState(
            도심마트SharedWorldState sharedWorld,
            IEnumerable<도심마트재고가용성WorldState> inventoryAvailabilities,
            IEnumerable<도심마트진열보충WorldState> replenishments,
            InterpretationLineage lineage)
        {
            SharedWorld = sharedWorld ?? throw new ArgumentNullException(nameof(sharedWorld));
            InventoryAvailabilities = (inventoryAvailabilities ?? throw new ArgumentNullException(nameof(inventoryAvailabilities)))
                .OrderBy(value => value.InventoryWorldId)
                .ToArray();
            Replenishments = (replenishments ?? throw new ArgumentNullException(nameof(replenishments)))
                .OrderBy(value => value.ShelfWorldId)
                .ToArray();
            Lineage = lineage ?? throw new ArgumentNullException(nameof(lineage));
        }

        public 도심마트SharedWorldState SharedWorld { get; }
        public 도심마트재고가용성WorldState[] InventoryAvailabilities { get; }
        public 도심마트진열보충WorldState[] Replenishments { get; }
        public InterpretationLineage Lineage { get; }
    }

    /// <summary>
    /// 현재 World에서 진열 보충 후보를 계산합니다.
    /// 후보와 preview 가능성만 만들며 서버 작업·재고 상태를 변경하지 않습니다.
    /// </summary>
    public sealed class 도심마트진열보충Interpreter
    {
        public 도심마트운영업무WorldState Interpret(
            도심마트SharedWorldState world,
            도심마트ReplenishmentRuleSet rules)
        {
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            var products = world.Nodes.OfType<도심마트운영상품WorldNode>()
                .ToDictionary(value => value.StableId);
            var locations = world.Nodes.OfType<도심마트위치WorldNode>()
                .ToDictionary(value => value.StableId);
            var inventories = world.Nodes.OfType<도심마트재고WorldNode>().ToArray();
            var tasks = world.Nodes.OfType<도심마트작업WorldNode>().ToArray();
            var allocations = world.Nodes.OfType<도심마트작업재고할당WorldNode>().ToArray();
            var inventoryAvailabilities = InterpretInventoryAvailabilities(
                inventories,
                tasks,
                allocations);
            var inventoryAvailabilityById = inventoryAvailabilities
                .ToDictionary(value => value.InventoryWorldId);
            var hasCapability = world.ServerCapabilityCodes.Contains(
                도심마트CapabilityCodes.CreateShelfReplenishment,
                StringComparer.Ordinal);

            var results = world.Nodes.OfType<도심마트진열대WorldNode>()
                .Select(shelf => InterpretShelf(
                    shelf,
                    products,
                    locations,
                    inventories,
                    tasks,
                    inventoryAvailabilityById,
                    hasCapability,
                    rules))
                .ToArray();
            var revision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                world.Lineage.Inputs,
                "urban-market-replenishment-world.v3",
                rules.Revision,
                string.Join("|", new[]
                {
                    world.Lineage.InterpretationRevision,
                    rules.TargetFillPercent.ToString(CultureInfo.InvariantCulture),
                }));
            var lineage = new InterpretationLineage(
                world.Lineage.Inputs,
                "urban-market-replenishment-world.v3",
                rules.Revision,
                revision,
                world.Lineage.EvidenceCardIds,
                world.Lineage.LimitationCodes);
            return new 도심마트운영업무WorldState(
                world,
                inventoryAvailabilities,
                results,
                lineage);
        }

        private static 도심마트재고가용성WorldState[] InterpretInventoryAvailabilities(
            IReadOnlyCollection<도심마트재고WorldNode> inventories,
            IReadOnlyCollection<도심마트작업WorldNode> tasks,
            IReadOnlyCollection<도심마트작업재고할당WorldNode> allocations)
        {
            var tasksById = tasks.ToDictionary(value => value.StableId);
            var activeAllocationsByInventory = allocations
                .Where(allocation => allocation.StateCode == 도심마트AllocationStateCodes.Active
                                     && tasksById.TryGetValue(allocation.TaskWorldId, out var task)
                                     && IsAllocatingTask(task))
                .GroupBy(value => value.InventoryWorldId)
                .ToDictionary(group => group.Key, group => group.ToArray());

            return inventories
                .Select(inventory =>
                {
                    activeAllocationsByInventory.TryGetValue(inventory.StableId, out var activeAllocations);
                    activeAllocations ??= Array.Empty<도심마트작업재고할당WorldNode>();
                    var unitMismatch = activeAllocations.Any(allocation => !string.Equals(
                        allocation.QuantityUnit,
                        inventory.QuantityUnit,
                        StringComparison.Ordinal));
                    var allocated = activeAllocations
                        .Where(allocation => string.Equals(
                            allocation.QuantityUnit,
                            inventory.QuantityUnit,
                            StringComparison.Ordinal))
                        .Sum(allocation => allocation.Quantity);
                    var oversubscribed = allocated > inventory.Quantity;
                    return new 도심마트재고가용성WorldState
                    {
                        InventoryWorldId = inventory.StableId,
                        ProductWorldId = inventory.ProductWorldId,
                        LocationWorldId = inventory.LocationWorldId,
                        OnHandQuantity = inventory.Quantity,
                        AllocatedQuantity = allocated,
                        AvailableQuantity = unitMismatch
                            ? 0
                            : Math.Max(0, inventory.Quantity - allocated),
                        QuantityUnit = inventory.QuantityUnit,
                        IsOversubscribed = oversubscribed,
                        HasAllocationUnitMismatch = unitMismatch,
                        AllocatingTaskWorldIds = activeAllocations
                            .Select(allocation => allocation.TaskWorldId)
                            .Distinct()
                            .OrderBy(value => value)
                            .ToArray(),
                        AllocatingAllocationWorldIds = activeAllocations
                            .Select(allocation => allocation.StableId)
                            .OrderBy(value => value)
                            .ToArray(),
                    };
                })
                .OrderBy(value => value.InventoryWorldId)
                .ToArray();
        }

        private static bool IsAllocatingTask(도심마트작업WorldNode task)
            => task.TaskKindCode == 도심마트TaskKindCodes.ShelfReplenishment
               && task.StateCode != 도심마트TaskStateCodes.Completed;

        private static 도심마트진열보충WorldState InterpretShelf(
            도심마트진열대WorldNode shelf,
            IReadOnlyDictionary<WorldStableId, 도심마트운영상품WorldNode> products,
            IReadOnlyDictionary<WorldStableId, 도심마트위치WorldNode> locations,
            IReadOnlyCollection<도심마트재고WorldNode> inventories,
            IReadOnlyCollection<도심마트작업WorldNode> tasks,
            IReadOnlyDictionary<WorldStableId, 도심마트재고가용성WorldState> inventoryAvailabilityById,
            bool hasCapability,
            도심마트ReplenishmentRuleSet rules)
        {
            if (!products.ContainsKey(shelf.ProductWorldId))
                throw new WorldGraphContractException("ShelfProductWorldNodeMissing", shelf.StableId.Value);
            if (!locations.TryGetValue(shelf.LocationWorldId, out var shelfLocation))
                throw new WorldGraphContractException("ShelfLocationWorldNodeMissing", shelf.StableId.Value);

            var sourceWorldIds = new List<WorldStableId>
            {
                shelf.StableId,
                shelf.ProductWorldId,
                shelf.LocationWorldId,
            };
            var blockers = new List<string>();
            var displayCandidates = inventories
                .Where(value => value.ProductWorldId == shelf.ProductWorldId
                                && value.LocationWorldId == shelf.LocationWorldId)
                .ToArray();
            if (displayCandidates.Length == 0)
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.DisplayInventoryMissing);
            else if (displayCandidates.Length > 1)
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.DisplayInventoryAmbiguous);

            var display = displayCandidates.Length == 1 ? displayCandidates[0] : null;
            if (display != null)
            {
                sourceWorldIds.Add(display.StableId);
                if (!string.Equals(display.QuantityUnit, shelf.QuantityUnit, StringComparison.Ordinal))
                    blockers.Add(도심마트ReplenishmentBlockReasonCodes.QuantityUnitMismatch);
            }

            var backroom = inventories
                .Where(value => value.ProductWorldId == shelf.ProductWorldId
                                && locations.TryGetValue(value.LocationWorldId, out var location)
                                && location.LocationKindCode == 도심마트LocationKindCodes.Backroom)
                .ToArray();
            sourceWorldIds.AddRange(backroom.Select(value => value.StableId));
            sourceWorldIds.AddRange(backroom.Select(value => value.LocationWorldId));
            if (backroom.Any(value => !string.Equals(value.QuantityUnit, shelf.QuantityUnit, StringComparison.Ordinal)))
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.QuantityUnitMismatch);

            var backroomAvailabilities = backroom
                .Select(value => inventoryAvailabilityById[value.StableId])
                .ToArray();
            sourceWorldIds.AddRange(backroomAvailabilities
                .SelectMany(value => value.AllocatingTaskWorldIds));
            sourceWorldIds.AddRange(backroomAvailabilities
                .SelectMany(value => value.AllocatingAllocationWorldIds));
            if (backroomAvailabilities.Any(value => value.IsOversubscribed))
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.InventoryOversubscribed);
            if (backroomAvailabilities.Any(value => value.HasAllocationUnitMismatch))
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.AllocationUnitMismatch);

            var activeTasks = tasks
                .Where(value => value.TaskKindCode == 도심마트TaskKindCodes.ShelfReplenishment
                                && value.TargetShelfWorldId == shelf.StableId
                                && value.StateCode != 도심마트TaskStateCodes.Completed)
                .ToArray();
            sourceWorldIds.AddRange(activeTasks.Select(value => value.StableId));
            if (activeTasks.Length > 0)
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.ActiveTaskExists);
            if (activeTasks.Any(value => !string.Equals(value.QuantityUnit, shelf.QuantityUnit, StringComparison.Ordinal)))
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.QuantityUnitMismatch);

            var displayQuantity = display?.Quantity ?? 0;
            var backroomOnHandQuantity = backroomAvailabilities.Sum(value => value.OnHandQuantity);
            var backroomAllocatedQuantity = backroomAvailabilities.Sum(value => value.AllocatedQuantity);
            var backroomAvailableQuantity = backroomAvailabilities.Sum(value => value.AvailableQuantity);
            var activeTaskQuantity = activeTasks.Sum(value => value.Quantity);
            var target = (int)Math.Ceiling(shelf.Capacity * (rules.TargetFillPercent / 100m));
            var shortfall = Math.Max(0, target - displayQuantity - activeTaskQuantity);
            var capacityRemaining = Math.Max(0, shelf.Capacity - displayQuantity - activeTaskQuantity);
            var candidate = Math.Min(shortfall, Math.Min(backroomAvailableQuantity, capacityRemaining));

            string needCode;
            if (blockers.Contains(도심마트ReplenishmentBlockReasonCodes.DisplayInventoryMissing)
                || blockers.Contains(도심마트ReplenishmentBlockReasonCodes.DisplayInventoryAmbiguous)
                || blockers.Contains(도심마트ReplenishmentBlockReasonCodes.QuantityUnitMismatch)
                || blockers.Contains(도심마트ReplenishmentBlockReasonCodes.InventoryOversubscribed)
                || blockers.Contains(도심마트ReplenishmentBlockReasonCodes.AllocationUnitMismatch))
            {
                needCode = 도심마트ReplenishmentNeedCodes.DataInsufficient;
                candidate = 0;
            }
            else if (activeTasks.Length > 0)
            {
                needCode = 도심마트ReplenishmentNeedCodes.TaskAlreadyActive;
                candidate = 0;
            }
            else if (displayQuantity >= target)
            {
                needCode = 도심마트ReplenishmentNeedCodes.NoActionNeeded;
                candidate = 0;
            }
            else if (backroomAvailableQuantity <= 0)
            {
                needCode = 도심마트ReplenishmentNeedCodes.InboundRequired;
                blockers.Add(backroomOnHandQuantity <= 0
                    ? 도심마트ReplenishmentBlockReasonCodes.BackroomInventoryMissing
                    : 도심마트ReplenishmentBlockReasonCodes.AvailableQuantityInsufficient);
                candidate = 0;
            }
            else
            {
                needCode = 도심마트ReplenishmentNeedCodes.ReplenishmentCandidate;
            }

            var sourcePlan = BuildSourcePlan(
                backroomAvailabilities,
                candidate,
                shelf.QuantityUnit);
            var isSourcePlanComplete = candidate > 0
                                       && sourcePlan.Sum(value => value.Quantity) == candidate;
            if (needCode == 도심마트ReplenishmentNeedCodes.ReplenishmentCandidate
                && !isSourcePlanComplete)
            {
                needCode = 도심마트ReplenishmentNeedCodes.DataInsufficient;
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.SourcePlanIncomplete);
                candidate = 0;
            }

            if (!hasCapability && needCode == 도심마트ReplenishmentNeedCodes.ReplenishmentCandidate)
                blockers.Add(도심마트ReplenishmentBlockReasonCodes.ServerCapabilityMissing);

            var normalizedBlockers = blockers
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            return new 도심마트진열보충WorldState
            {
                ShelfWorldId = shelf.StableId,
                ProductWorldId = shelf.ProductWorldId,
                DisplayQuantity = displayQuantity,
                DisplayCapacity = shelf.Capacity,
                TargetQuantity = target,
                BackroomOnHandQuantity = backroomOnHandQuantity,
                BackroomAllocatedQuantity = backroomAllocatedQuantity,
                BackroomAvailableQuantity = backroomAvailableQuantity,
                ActiveTaskQuantity = activeTaskQuantity,
                CandidateQuantity = candidate,
                SourcePlan = sourcePlan,
                IsSourcePlanComplete = isSourcePlanComplete,
                QuantityUnit = shelf.QuantityUnit,
                NeedCode = needCode,
                BlockReasonCodes = normalizedBlockers,
                CanPreviewRequest = needCode == 도심마트ReplenishmentNeedCodes.ReplenishmentCandidate
                                    && candidate > 0
                                    && isSourcePlanComplete
                                    && hasCapability,
                RuleRevision = rules.Revision,
                SourceWorldIds = sourceWorldIds
                    .Distinct()
                    .OrderBy(value => value)
                    .ToArray(),
            };
        }

        private static 도심마트진열보충SourcePlanSegment[] BuildSourcePlan(
            IEnumerable<도심마트재고가용성WorldState> availabilities,
            int candidateQuantity,
            string quantityUnit)
        {
            if (candidateQuantity <= 0) return Array.Empty<도심마트진열보충SourcePlanSegment>();
            var remaining = candidateQuantity;
            var result = new List<도심마트진열보충SourcePlanSegment>();
            foreach (var availability in availabilities
                         .Where(value => value.AvailableQuantity > 0
                                         && string.Equals(value.QuantityUnit, quantityUnit, StringComparison.Ordinal))
                         .OrderBy(value => value.InventoryWorldId))
            {
                if (remaining <= 0) break;
                var quantity = Math.Min(remaining, availability.AvailableQuantity);
                result.Add(new 도심마트진열보충SourcePlanSegment
                {
                    InventoryWorldId = availability.InventoryWorldId,
                    LocationWorldId = availability.LocationWorldId,
                    Quantity = quantity,
                    QuantityUnit = quantityUnit,
                });
                remaining -= quantity;
            }
            return result.ToArray();
        }
    }

    /// <summary>UM2 graph와 UM3 진열 보충 해석을 하나의 Shared Runtime port로 조합합니다.</summary>
    public sealed class 도심마트운영업무SharedWorldInterpreter
    {
        private readonly 도심마트운영SharedWorldInterpreter graphInterpreter;
        private readonly 도심마트진열보충Interpreter replenishmentInterpreter;
        private readonly 도심마트ReplenishmentRuleSet rules;

        public 도심마트운영업무SharedWorldInterpreter(
            도심마트운영SharedWorldInterpreter graphInterpreter,
            도심마트진열보충Interpreter replenishmentInterpreter,
            도심마트ReplenishmentRuleSet rules)
        {
            this.graphInterpreter = graphInterpreter ?? throw new ArgumentNullException(nameof(graphInterpreter));
            this.replenishmentInterpreter = replenishmentInterpreter ?? throw new ArgumentNullException(nameof(replenishmentInterpreter));
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public 도심마트운영업무WorldState Interpret(
            도심마트운영DataSnapshot data,
            도심마트SharedInterpretationContext context)
            => replenishmentInterpreter.Interpret(
                graphInterpreter.Interpret(data, context),
                rules);
    }
}
