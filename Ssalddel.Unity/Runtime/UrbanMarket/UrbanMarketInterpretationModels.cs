using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class 도심마트WorldNodeKindCodes
    {
        public const string Market = "Market";
        public const string PublicProduct = "PublicProduct";
        public const string OperationProduct = "OperationProduct";
        public const string Location = "Location";
        public const string Inventory = "Inventory";
        public const string Shelf = "Shelf";
        public const string Task = "Task";
        public const string TaskAllocation = "TaskAllocation";
    }

    public static class 도심마트InterpretationVersions
    {
        public const string PublicContract = "urban-market-public-world.v1";
        public const string PublicRules = "urban-market-public-rules.v1";
        public const string OperationContract = "urban-market-operation-world.v2";
        public const string OperationRules = "urban-market-operation-rules.v1";
    }

    public static class 도심마트LimitationCodes
    {
        public const string PhysicalInventoryNotProvided = "PhysicalInventoryNotProvided";
        public const string SimulationOnly = "SimulationOnly";
    }

    public sealed class 도심마트SharedInterpretationContext
    {
        public 도심마트SharedInterpretationContext(
            string contractVersion,
            string ruleSetRevision)
        {
            ContractVersion = Require(contractVersion, nameof(contractVersion));
            RuleSetRevision = Require(ruleSetRevision, nameof(ruleSetRevision));
        }

        public string ContractVersion { get; }
        public string RuleSetRevision { get; }

        public static 도심마트SharedInterpretationContext PublicProducts()
            => new 도심마트SharedInterpretationContext(
                도심마트InterpretationVersions.PublicContract,
                도심마트InterpretationVersions.PublicRules);

        public static 도심마트SharedInterpretationContext Operations()
            => new 도심마트SharedInterpretationContext(
                도심마트InterpretationVersions.OperationContract,
                도심마트InterpretationVersions.OperationRules);

        private static string Require(string value, string name)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Value is required.", name)
                : value.Trim();
    }

    public abstract class 도심마트WorldNode : IWorldNode
    {
        protected 도심마트WorldNode(
            string stableId,
            string kindCode,
            IEnumerable<string> sourceStableIds)
        {
            StableId = new WorldStableId(stableId);
            KindCode = string.IsNullOrWhiteSpace(kindCode)
                ? throw new ArgumentException("MarketWorldNodeKindMissing", nameof(kindCode))
                : kindCode.Trim();
            IdentityLineage = new WorldIdentityLineage(
                StableId,
                (sourceStableIds ?? throw new ArgumentNullException(nameof(sourceStableIds)))
                    .Select(value => new SourceStableId(value)));
        }

        public WorldStableId StableId { get; }
        public string KindCode { get; }
        public WorldIdentityLineage IdentityLineage { get; }
    }

    public sealed class 도심마트RootWorldNode : 도심마트WorldNode
    {
        public 도심마트RootWorldNode(string stableId, string marketName)
            : base(stableId, 도심마트WorldNodeKindCodes.Market, new[] { stableId })
        {
            마트명 = marketName ?? string.Empty;
        }

        public string 마트명 { get; }
    }

    public sealed class 도심마트공개상품WorldNode : 도심마트WorldNode
    {
        public 도심마트공개상품WorldNode(도심마트공개상품Data source)
            : base(
                (source ?? throw new ArgumentNullException(nameof(source))).StableId,
                도심마트WorldNodeKindCodes.PublicProduct,
                new[] { source.StableId })
        {
            상품명 = source.상품명;
            판매단위 = source.판매단위;
            판매가 = source.판매가;
            통화Code = source.통화Code;
            투영판매가능수량 = source.투영판매가능수량;
            투영수량단위 = source.투영수량단위;
            서버판매가능여부 = source.서버판매가능여부;
            QuantityMeaningCode = source.QuantityMeaningCode;
            EvidenceAsOf = source.EvidenceAsOf;
        }

        public string 상품명 { get; }
        public string 판매단위 { get; }
        public decimal 판매가 { get; }
        public string 통화Code { get; }
        public int 투영판매가능수량 { get; }
        public string 투영수량단위 { get; }
        public bool 서버판매가능여부 { get; }
        public string QuantityMeaningCode { get; }
        public DateTimeOffset EvidenceAsOf { get; }
    }

    public sealed class 도심마트운영상품WorldNode : 도심마트WorldNode
    {
        public 도심마트운영상품WorldNode(도심마트운영상품Data source)
            : base(
                (source ?? throw new ArgumentNullException(nameof(source))).StableId,
                도심마트WorldNodeKindCodes.OperationProduct,
                new[] { source.StableId })
        {
            상품명 = source.상품명;
            판매단위 = source.판매단위;
        }

        public string 상품명 { get; }
        public string 판매단위 { get; }
    }

    public sealed class 도심마트위치WorldNode : 도심마트WorldNode
    {
        public 도심마트위치WorldNode(도심마트운영위치Data source)
            : base(
                (source ?? throw new ArgumentNullException(nameof(source))).StableId,
                도심마트WorldNodeKindCodes.Location,
                new[] { source.StableId })
        {
            이름 = source.이름;
            LocationKindCode = source.KindCode;
        }

        public string 이름 { get; }
        public string LocationKindCode { get; }
    }

    public sealed class 도심마트재고WorldNode : 도심마트WorldNode
    {
        public 도심마트재고WorldNode(도심마트운영재고Data source)
            : base(
                (source ?? throw new ArgumentNullException(nameof(source))).StableId,
                도심마트WorldNodeKindCodes.Inventory,
                new[] { source.StableId })
        {
            ProductWorldId = new WorldStableId(source.ProductStableId);
            LocationWorldId = new WorldStableId(source.LocationStableId);
            Quantity = source.Quantity;
            QuantityUnit = source.QuantityUnit;
        }

        public WorldStableId ProductWorldId { get; }
        public WorldStableId LocationWorldId { get; }
        public int Quantity { get; }
        public string QuantityUnit { get; }
    }

    public sealed class 도심마트진열대WorldNode : 도심마트WorldNode
    {
        public 도심마트진열대WorldNode(도심마트운영진열대Data source)
            : base(
                (source ?? throw new ArgumentNullException(nameof(source))).StableId,
                도심마트WorldNodeKindCodes.Shelf,
                new[] { source.StableId })
        {
            ProductWorldId = new WorldStableId(source.ProductStableId);
            LocationWorldId = new WorldStableId(source.LocationStableId);
            Capacity = source.Capacity;
            QuantityUnit = source.QuantityUnit;
        }

        public WorldStableId ProductWorldId { get; }
        public WorldStableId LocationWorldId { get; }
        public int Capacity { get; }
        public string QuantityUnit { get; }
    }

    public sealed class 도심마트작업WorldNode : 도심마트WorldNode
    {
        public 도심마트작업WorldNode(도심마트운영작업Data source)
            : base(
                (source ?? throw new ArgumentNullException(nameof(source))).StableId,
                도심마트WorldNodeKindCodes.Task,
                new[] { source.StableId })
        {
            TaskKindCode = source.KindCode;
            StateCode = source.StateCode;
            ProductWorldId = new WorldStableId(source.ProductStableId);
            SourceInventoryWorldId = new WorldStableId(source.SourceInventoryStableId);
            TargetShelfWorldId = new WorldStableId(source.TargetShelfStableId);
            Quantity = source.Quantity;
            QuantityUnit = source.QuantityUnit;
        }

        public string TaskKindCode { get; }
        public string StateCode { get; }
        public WorldStableId ProductWorldId { get; }
        public WorldStableId SourceInventoryWorldId { get; }
        public WorldStableId TargetShelfWorldId { get; }
        public int Quantity { get; }
        public string QuantityUnit { get; }
    }

    public sealed class 도심마트작업재고할당WorldNode : 도심마트WorldNode
    {
        public 도심마트작업재고할당WorldNode(도심마트운영작업재고할당Data source)
            : base(
                (source ?? throw new ArgumentNullException(nameof(source))).StableId,
                도심마트WorldNodeKindCodes.TaskAllocation,
                new[] { source.StableId })
        {
            TaskWorldId = new WorldStableId(source.TaskStableId);
            InventoryWorldId = new WorldStableId(source.InventoryStableId);
            Quantity = source.Quantity;
            QuantityUnit = source.QuantityUnit;
            StateCode = source.StateCode;
            Revision = source.Revision;
        }

        public WorldStableId TaskWorldId { get; }
        public WorldStableId InventoryWorldId { get; }
        public int Quantity { get; }
        public string QuantityUnit { get; }
        public string StateCode { get; }
        public string Revision { get; }
    }

    public sealed class 도심마트SharedWorldState
    {
        public 도심마트SharedWorldState(
            string stableId,
            DataRuntimeMode mode,
            IEnumerable<도심마트WorldNode> nodes,
            IEnumerable<WorldRelation> relations,
            InterpretationLineage lineage,
            IEnumerable<string>? serverCapabilityCodes = null)
        {
            StableId = new WorldStableId(stableId);
            Mode = mode;
            Nodes = (nodes ?? throw new ArgumentNullException(nameof(nodes))).ToArray();
            Relations = (relations ?? throw new ArgumentNullException(nameof(relations))).ToArray();
            Lineage = lineage ?? throw new ArgumentNullException(nameof(lineage));
            ServerCapabilityCodes = (serverCapabilityCodes ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            Graph = new WorldGraphIndex<도심마트WorldNode>(Nodes, Relations);
            if (!Graph.NodesById.ContainsKey(StableId))
                throw new WorldGraphContractException("MarketRootNodeMissing", StableId.Value);
        }

        public WorldStableId StableId { get; }
        public DataRuntimeMode Mode { get; }
        public 도심마트WorldNode[] Nodes { get; }
        public WorldRelation[] Relations { get; }
        public InterpretationLineage Lineage { get; }
        public string[] ServerCapabilityCodes { get; }
        public WorldGraphIndex<도심마트WorldNode> Graph { get; }
    }

    public sealed class 도심마트공개상품SharedWorldInterpreter :
        ISharedWorldInterpreter<도심마트공개상품DataSnapshot, 도심마트SharedInterpretationContext, 도심마트SharedWorldState>
    {
        private readonly 도심마트공개상품DataSnapshotValidator validator =
            new 도심마트공개상품DataSnapshotValidator();

        public 도심마트SharedWorldState Interpret(
            도심마트공개상품DataSnapshot data,
            도심마트SharedInterpretationContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var errors = validator.Validate(data);
            if (errors.Length > 0) throw new InvalidOperationException(errors[0]);

            var nodes = new List<도심마트WorldNode>
            {
                new 도심마트RootWorldNode(data.StableId, data.마트명),
            };
            nodes.AddRange(data.상품목록.Select(value => new 도심마트공개상품WorldNode(value)));
            var marketId = new WorldStableId(data.StableId);
            var relations = data.상품목록.Select(product => new WorldRelation(
                marketId,
                new WorldStableId(product.StableId),
                WorldRelationKind.Contains));
            var lineage = 도심마트InterpretationLineageFactory.Create(
                data.StableId,
                data.DataRevision,
                data.GeneratedAt,
                data.Mode,
                context,
                new[] { 도심마트LimitationCodes.PhysicalInventoryNotProvided });
            return new 도심마트SharedWorldState(
                data.StableId,
                data.Mode,
                nodes,
                relations,
                lineage);
        }
    }

    public sealed class 도심마트운영SharedWorldInterpreter :
        ISharedWorldInterpreter<도심마트운영DataSnapshot, 도심마트SharedInterpretationContext, 도심마트SharedWorldState>
    {
        private readonly 도심마트운영DataSnapshotValidator validator =
            new 도심마트운영DataSnapshotValidator();

        public 도심마트SharedWorldState Interpret(
            도심마트운영DataSnapshot data,
            도심마트SharedInterpretationContext context)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (context == null) throw new ArgumentNullException(nameof(context));
            var errors = validator.Validate(data);
            if (errors.Length > 0) throw new InvalidOperationException(errors[0]);

            var nodes = new List<도심마트WorldNode>
            {
                new 도심마트RootWorldNode(data.StableId, data.마트명),
            };
            nodes.AddRange(data.상품목록.Select(value => new 도심마트운영상품WorldNode(value)));
            nodes.AddRange(data.위치목록.Select(value => new 도심마트위치WorldNode(value)));
            nodes.AddRange(data.재고목록.Select(value => new 도심마트재고WorldNode(value)));
            nodes.AddRange(data.진열대목록.Select(value => new 도심마트진열대WorldNode(value)));
            nodes.AddRange(data.작업목록.Select(value => new 도심마트작업WorldNode(value)));
            var normalizedAllocations = NormalizeTaskAllocations(data);
            nodes.AddRange(normalizedAllocations.Select(value => new 도심마트작업재고할당WorldNode(value)));

            var marketId = new WorldStableId(data.StableId);
            var relations = new List<WorldRelation>();
            relations.AddRange(nodes
                .Where(node => node.StableId != marketId)
                .Select(node => new WorldRelation(marketId, node.StableId, WorldRelationKind.Contains)));
            relations.AddRange(data.재고목록.Select(inventory => new WorldRelation(
                new WorldStableId(inventory.StableId),
                new WorldStableId(inventory.LocationStableId),
                WorldRelationKind.LocatedAt)));
            relations.AddRange(data.진열대목록.Select(shelf => new WorldRelation(
                new WorldStableId(shelf.StableId),
                new WorldStableId(shelf.LocationStableId),
                WorldRelationKind.LocatedAt)));
            var tasksWithExplicitAllocations = data.작업재고할당목록
                .Select(value => value.TaskStableId)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var task in data.작업목록)
            {
                var taskId = new WorldStableId(task.StableId);
                relations.Add(new WorldRelation(
                    taskId,
                    new WorldStableId(task.ProductStableId),
                    WorldRelationKind.Targets));
                if (!tasksWithExplicitAllocations.Contains(task.StableId))
                {
                    relations.Add(new WorldRelation(
                        taskId,
                        new WorldStableId(task.SourceInventoryStableId),
                        WorldRelationKind.Targets));
                }
                relations.Add(new WorldRelation(
                    taskId,
                    new WorldStableId(task.TargetShelfStableId),
                    WorldRelationKind.Targets));
            }
            foreach (var allocation in normalizedAllocations)
            {
                var allocationId = new WorldStableId(allocation.StableId);
                relations.Add(new WorldRelation(
                    allocationId,
                    new WorldStableId(allocation.TaskStableId),
                    WorldRelationKind.DerivedFrom));
                relations.Add(new WorldRelation(
                    allocationId,
                    new WorldStableId(allocation.InventoryStableId),
                    WorldRelationKind.Targets));
            }

            var limitations = data.Mode == DataRuntimeMode.Simulation
                ? new[] { 도심마트LimitationCodes.SimulationOnly }
                : Array.Empty<string>();
            var lineage = 도심마트InterpretationLineageFactory.Create(
                data.StableId,
                data.DataRevision,
                data.GeneratedAt,
                data.Mode,
                context,
                limitations);
            return new 도심마트SharedWorldState(
                data.StableId,
                data.Mode,
                nodes,
                relations,
                lineage,
                data.ServerCapabilityCodes);
        }

        private static 도심마트운영작업재고할당Data[] NormalizeTaskAllocations(
            도심마트운영DataSnapshot data)
        {
            var explicitByTask = data.작업재고할당목록
                .GroupBy(value => value.TaskStableId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
            var result = new List<도심마트운영작업재고할당Data>();
            foreach (var task in data.작업목록)
            {
                if (explicitByTask.TryGetValue(task.StableId, out var explicitAllocations))
                {
                    result.AddRange(explicitAllocations);
                    continue;
                }

                result.Add(new 도심마트운영작업재고할당Data
                {
                    StableId = task.StableId + ":allocation:legacy",
                    TaskStableId = task.StableId,
                    InventoryStableId = task.SourceInventoryStableId,
                    Quantity = task.Quantity,
                    QuantityUnit = task.QuantityUnit,
                    StateCode = task.StateCode == 도심마트TaskStateCodes.Completed
                        ? 도심마트AllocationStateCodes.Consumed
                        : 도심마트AllocationStateCodes.Active,
                    Revision = data.DataRevision,
                });
            }
            return result
                .OrderBy(value => value.StableId, StringComparer.Ordinal)
                .ToArray();
        }
    }

    internal static class 도심마트InterpretationLineageFactory
    {
        public static InterpretationLineage Create(
            string sourceStableId,
            string dataRevision,
            DateTimeOffset evidenceAsOf,
            DataRuntimeMode mode,
            도심마트SharedInterpretationContext context,
            IEnumerable<string> limitations)
        {
            var inputs = new DataRevisionSet(new[]
            {
                new DataRevisionReference(
                    sourceStableId,
                    dataRevision,
                    evidenceAsOf,
                    mode == DataRuntimeMode.Simulation
                        ? DataQualityCodes.Estimated
                        : DataQualityCodes.Observed),
            });
            var revision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                inputs,
                context.ContractVersion,
                context.RuleSetRevision,
                mode.ToString());
            return new InterpretationLineage(
                inputs,
                context.ContractVersion,
                context.RuleSetRevision,
                revision,
                limitationCodes: limitations);
        }
    }

}
