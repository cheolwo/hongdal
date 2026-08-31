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
    public static class SimulationWorldLandscapeAssemblyEvidenceCodes
    {
        public const string Derived = "Derived";
        public const string StatisticallyAllocated = "StatisticallyAllocated";
        public const string Scenario = "Scenario";
        public const string Decorative = "Decorative";
    }

    public sealed class SimulationWorldLandscapeGrammarCatalog
    {
        public int SchemaVersion { get; set; }
        public string CatalogRevision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        // D442: 구형 catalog hash가 포함하지 않는 선언도 원파일 바이트로 결속한다.
        public string SourceDocumentHashSha256 { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; }
        public IReadOnlyList<SimulationWorldLandscapeGrammarEntry> Entries { get; set; } =
            Array.Empty<SimulationWorldLandscapeGrammarEntry>();

        public void ValidateCanonicalCatalog()
        {
            if (!PresentationOnly)
                throw new InvalidOperationException("경관 문법 대장은 표현 전용이어야 합니다.");
            if (!string.Equals(CatalogRevision,
                    SimulationWorldLandscapeCompositionCodes.GrammarRevision,
                    StringComparison.Ordinal))
                throw new InvalidOperationException("지원하지 않는 경관 문법 개정입니다.");
            if (Entries.Count != 156)
                throw new InvalidOperationException("통합 경관 문법 대장은 정확히 156개여야 합니다.");
            if (Entries.Select(item => item.CompositionKey).Distinct(StringComparer.Ordinal).Count() != 156)
                throw new InvalidOperationException("CompositionKey가 중복되었습니다.");

            var groups = Entries.GroupBy(
                item => item.FamilyCode + "\u001f" + item.SetName,
                StringComparer.Ordinal).ToArray();
            if (groups.Length != 52 || groups.Any(group => group.Count() != 3))
                throw new InvalidOperationException("52개 의미 모판마다 A/B/C 세 변형이 있어야 합니다.");
            foreach (var group in groups)
            {
                var variants = group.Select(item => item.VariantCode)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                if (!variants.SequenceEqual(new[] { "A", "B", "C" }, StringComparer.Ordinal))
                    throw new InvalidOperationException("각 의미 모판에는 A/B/C 변형이 필요합니다.");
            }
        }
    }

    public sealed class SimulationWorldLandscapeGrammarEntry
    {
        public string CompositionKey { get; set; } = string.Empty;
        public string SetName { get; set; } = string.Empty;
        public string VariantCode { get; set; } = string.Empty;
        public string FamilyCode { get; set; } = string.Empty;
        public string TopologyCode { get; set; } = string.Empty;
        public double FootprintX { get; set; }
        public double FootprintY { get; set; }
        // D442 검토 입력. null은 구형/미관측이며 0 경사나 제약 없음으로 해석하지 않는다.
        // 구형 Assemble/HashCanonical은 이 필드를 소비하지 않는다.
        public double? MinimumSlopeDegrees { get; set; }
        public double? MaximumSlopeDegrees { get; set; }
        public IReadOnlyList<SimulationWorldLandscapeGrammarEdge>? EdgeProfiles { get; set; }
        public IReadOnlyList<string>? AllowedNeighborTopologyCodes { get; set; }
        public IReadOnlyList<string>? ForbiddenNeighborTopologyCodes { get; set; }
        public int MaxConsecutive { get; set; }
        public int RecentWindowSize { get; set; }
        public bool MirrorAllowed { get; set; }
        public IReadOnlyList<string> RotationCodes { get; set; } = Array.Empty<string>();
        public IReadOnlyList<SimulationWorldLandscapeGrammarConnector> Connectors { get; set; } =
            Array.Empty<SimulationWorldLandscapeGrammarConnector>();
        public bool PresentationOnly { get; set; }
    }

    public sealed class SimulationWorldLandscapeGrammarEdge
    {
        public string DirectionCode { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public bool Required { get; set; }
    }

    public sealed class SimulationWorldLandscapeGrammarConnector
    {
        public string ConnectorTypeCode { get; set; } = string.Empty;
        public string DirectionCode { get; set; } = string.Empty;
        public string RouteSignature { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double Width { get; set; }
    }

    public sealed class SimulationWorldLandscapeSkeleton
    {
        public string TileKey { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public IReadOnlyList<SimulationWorldLandscapeSkeletonNode> Nodes { get; set; } =
            Array.Empty<SimulationWorldLandscapeSkeletonNode>();
        public IReadOnlyList<SimulationWorldLandscapeSkeletonEdge> Edges { get; set; } =
            Array.Empty<SimulationWorldLandscapeSkeletonEdge>();
    }

    public sealed class SimulationWorldLandscapeSkeletonNode
    {
        public string NodeStableId { get; set; } = string.Empty;
        public string ParentNodeStableId { get; set; } = string.Empty;
        public string NodeKindCode { get; set; } = string.Empty;
        public string SemanticCode { get; set; } = string.Empty;
        public string PreferredFamilyCode { get; set; } = string.Empty;
        public string PreferredSetName { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public double CenterEastingMeters { get; set; }
        public double CenterNorthingMeters { get; set; }
        public double PhysicalElevationMeters { get; set; }
        public double WidthMeters { get; set; }
        public double DepthMeters { get; set; }
        public double RotationDegrees { get; set; }
    }

    public sealed class SimulationWorldLandscapeSkeletonEdge
    {
        public string EdgeStableId { get; set; } = string.Empty;
        public string FromNodeStableId { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public string ToNodeStableId { get; set; } = string.Empty;
        public string ConnectorTypeCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string? NeighborTileKey { get; set; }
        public string DirectionCode { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "Macro·Meso 공간 골격을 156개 의미 모판의 연결·반복 문법으로 결정적으로 조립한다.",
        StepKey = "domain.landscape-graph-assembler",
        DependsOnStepKeys = new[] { "contract.landscape-composition-tile" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        FlowOrder = 42,
        Boundary = "실제 도로가 없는 연결은 Scenario 근거를 유지하며, Micro 장식 좌표와 Prefab은 Unity wrapper가 결정한다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
        Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
    public sealed class SimulationWorldLandscapeGraphAssembler
    {
        public SimulationWorldLandscapeCompositionTileResponse Assemble(
            SimulationWorldLandscapeSkeleton skeleton,
            SimulationWorldLandscapeGrammarCatalog catalog)
        {
            if (skeleton == null) throw new ArgumentNullException(nameof(skeleton));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            catalog.ValidateCanonicalCatalog();

            var nodes = skeleton.Nodes
                .OrderBy(item => item.NodeStableId, StringComparer.Ordinal)
                .Select(ToResponse)
                .ToArray();
            var edges = skeleton.Edges
                .Where(item => string.IsNullOrWhiteSpace(item.NeighborTileKey))
                .OrderBy(item => item.EdgeStableId, StringComparer.Ordinal)
                .Select(ToResponse)
                .ToArray();
            var placements = new List<SimulationWorldLandscapePlacementResponse>();
            var unresolved = new List<SimulationWorldLandscapeUnresolvedResponse>();
            var recentVariants = new Dictionary<string, Queue<string>>(StringComparer.Ordinal);

            foreach (var node in skeleton.Nodes.OrderBy(item => item.NodeStableId, StringComparer.Ordinal))
            {
                var candidates = catalog.Entries
                    .Where(item => item.PresentationOnly
                        && string.Equals(item.FamilyCode, node.PreferredFamilyCode, StringComparison.Ordinal)
                        && string.Equals(item.SetName, node.PreferredSetName, StringComparison.Ordinal)
                        && string.Equals(item.TopologyCode, node.NodeKindCode, StringComparison.Ordinal))
                    .OrderBy(item => item.VariantCode, StringComparer.Ordinal)
                    .ToArray();
                if (candidates.Length == 0)
                {
                    unresolved.Add(new SimulationWorldLandscapeUnresolvedResponse
                    {
                        UnresolvedStableId = "unresolved:" + node.NodeStableId,
                        NodeStableId = node.NodeStableId,
                        ReasonCode = "NoCompatibleComposition",
                        RequiredSemanticCode = node.SemanticCode,
                        EvidenceKindCode = node.EvidenceKindCode,
                        Detail = node.PreferredFamilyCode + ":" + node.PreferredSetName,
                    });
                    continue;
                }

                var groupKey = node.PreferredFamilyCode + "\u001f" + node.PreferredSetName;
                var seed = StablePositiveInt(
                    skeleton.AreaSetStableId, skeleton.TileKey, node.NodeStableId,
                    node.CenterEastingMeters.ToString("R", CultureInfo.InvariantCulture),
                    node.CenterNorthingMeters.ToString("R", CultureInfo.InvariantCulture),
                    catalog.CatalogHashSha256);
                var selected = SelectVariant(candidates, seed, groupKey, recentVariants);
                var rotation = SelectRotation(selected, node.RotationDegrees, seed);
                placements.Add(new SimulationWorldLandscapePlacementResponse
                {
                    PlacementStableId = "placement:" + node.NodeStableId,
                    NodeStableId = node.NodeStableId,
                    OwnerTileKey = skeleton.TileKey,
                    CompositionKey = selected.CompositionKey,
                    TopologyCode = selected.TopologyCode,
                    EvidenceKindCode = node.EvidenceKindCode,
                    EastingMeters = node.CenterEastingMeters,
                    NorthingMeters = node.CenterNorthingMeters,
                    PhysicalElevationMeters = node.PhysicalElevationMeters,
                    RotationDegrees = rotation,
                    Mirrored = selected.MirrorAllowed && seed % 5 == 0,
                    DeterministicSeed = seed,
                    FootprintWidthMeters = Math.Min(node.WidthMeters, selected.FootprintX),
                    FootprintDepthMeters = Math.Min(node.DepthMeters, selected.FootprintY),
                    PresentationOnly = true,
                });
            }

            var placementByNode = placements.ToDictionary(
                item => item.NodeStableId, item => item, StringComparer.Ordinal);
            var stubs = skeleton.Edges
                .Where(item => !string.IsNullOrWhiteSpace(item.NeighborTileKey))
                .OrderBy(item => item.EdgeStableId, StringComparer.Ordinal)
                .Select(item => CreateStub(item, skeleton, placementByNode, catalog))
                .ToArray();

            var response = new SimulationWorldLandscapeCompositionTileResponse
            {
                TileKey = skeleton.TileKey,
                AreaSetStableId = skeleton.AreaSetStableId,
                GraphBuildStableId = "landscape-graph:" + skeleton.AreaSetStableId + ":" + skeleton.TileKey,
                GrammarRevision = catalog.CatalogRevision,
                GrammarHashSha256 = catalog.CatalogHashSha256,
                StatusCode = unresolved.Count == 0
                    ? SimulationWorldLandscapeCompositionCodes.Available
                    : SimulationWorldLandscapeCompositionCodes.PartialUnresolved,
                Nodes = nodes,
                Edges = edges,
                Placements = placements.OrderBy(item => item.PlacementStableId, StringComparer.Ordinal).ToArray(),
                ExternalConnectorStubs = stubs,
                Unresolved = unresolved.ToArray(),
                PresentationOnly = true,
                IsOperationalState = false,
            };
            response.GraphHashSha256 = HashCanonical(response);
            return response;
        }

        private static SimulationWorldLandscapeGrammarEntry SelectVariant(
            SimulationWorldLandscapeGrammarEntry[] candidates,
            int seed,
            string groupKey,
            IDictionary<string, Queue<string>> recentVariants)
        {
            if (!recentVariants.TryGetValue(groupKey, out var recent))
            {
                recent = new Queue<string>();
                recentVariants[groupKey] = recent;
            }

            var selectedIndex = seed % candidates.Length;
            var maximumConsecutive = Math.Max(1, candidates[selectedIndex].MaxConsecutive);
            if (recent.Count >= maximumConsecutive
                && recent.Reverse().Take(maximumConsecutive)
                    .All(value => string.Equals(value, candidates[selectedIndex].VariantCode,
                        StringComparison.Ordinal)))
                selectedIndex = (selectedIndex + 1) % candidates.Length;

            var selected = candidates[selectedIndex];
            recent.Enqueue(selected.VariantCode);
            var window = Math.Max(1, selected.RecentWindowSize);
            while (recent.Count > window) recent.Dequeue();
            return selected;
        }

        private static double SelectRotation(
            SimulationWorldLandscapeGrammarEntry entry,
            double requestedRotation,
            int seed)
        {
            if (entry.RotationCodes.Count == 0) return requestedRotation;
            var values = entry.RotationCodes
                .Select(value => double.TryParse(value, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out var parsed) ? parsed : 0d)
                .ToArray();
            if (values.Any(value => Math.Abs(value - requestedRotation) < 0.01d))
                return requestedRotation;
            return values[seed % values.Length];
        }

        private static SimulationWorldLandscapeExternalConnectorResponse CreateStub(
            SimulationWorldLandscapeSkeletonEdge edge,
            SimulationWorldLandscapeSkeleton skeleton,
            IReadOnlyDictionary<string, SimulationWorldLandscapePlacementResponse> placements,
            SimulationWorldLandscapeGrammarCatalog catalog)
        {
            placements.TryGetValue(edge.FromNodeStableId, out var placement);
            var entry = placement == null ? null : catalog.Entries.FirstOrDefault(
                item => string.Equals(item.CompositionKey, placement.CompositionKey, StringComparison.Ordinal));
            var rotation = placement?.RotationDegrees ?? 0d;
            var connector = entry?.Connectors.FirstOrDefault(item =>
                string.IsNullOrWhiteSpace(edge.DirectionCode)
                || string.Equals(WorldDirection(item.DirectionCode, rotation),
                    edge.DirectionCode, StringComparison.Ordinal));
            var radians = rotation * Math.PI / 180d;
            var rotatedX = connector == null ? 0d
                : connector.LocalX * Math.Cos(radians) + connector.LocalZ * Math.Sin(radians);
            var rotatedZ = connector == null ? 0d
                : -connector.LocalX * Math.Sin(radians) + connector.LocalZ * Math.Cos(radians);
            return new SimulationWorldLandscapeExternalConnectorResponse
            {
                StubStableId = "stub:" + edge.EdgeStableId,
                PlacementStableId = placement?.PlacementStableId ?? string.Empty,
                NeighborTileKey = edge.NeighborTileKey ?? string.Empty,
                ConnectorTypeCode = connector?.ConnectorTypeCode ?? edge.ConnectorTypeCode,
                RouteSignature = connector?.RouteSignature ?? "scenario.route",
                DirectionCode = edge.DirectionCode,
                EvidenceKindCode = edge.EvidenceKindCode,
                WorldEastingMeters = (placement?.EastingMeters ?? 0d) + rotatedX,
                WorldNorthingMeters = (placement?.NorthingMeters ?? 0d) + rotatedZ,
                WidthMeters = connector?.Width ?? 0d,
            };
        }

        private static string WorldDirection(string localDirection, double rotationDegrees)
        {
            var directions = new[] { "north", "east", "south", "west" };
            var index = Array.IndexOf(directions, localDirection);
            if (index < 0) return localDirection;
            var quarterTurns = ((int)Math.Round(rotationDegrees / 90d) % 4 + 4) % 4;
            return directions[(index + quarterTurns) % 4];
        }

        private static SimulationWorldLandscapeNodeResponse ToResponse(
            SimulationWorldLandscapeSkeletonNode item) => new SimulationWorldLandscapeNodeResponse
        {
            NodeStableId = item.NodeStableId,
            ParentNodeStableId = item.ParentNodeStableId,
            NodeKindCode = item.NodeKindCode,
            SemanticCode = item.SemanticCode,
            EvidenceKindCode = item.EvidenceKindCode,
            CenterEastingMeters = item.CenterEastingMeters,
            CenterNorthingMeters = item.CenterNorthingMeters,
            WidthMeters = item.WidthMeters,
            DepthMeters = item.DepthMeters,
        };

        private static SimulationWorldLandscapeEdgeResponse ToResponse(
            SimulationWorldLandscapeSkeletonEdge item) => new SimulationWorldLandscapeEdgeResponse
        {
            EdgeStableId = item.EdgeStableId,
            FromNodeStableId = item.FromNodeStableId,
            RelationCode = item.RelationCode,
            ToNodeStableId = item.ToNodeStableId,
            ConnectorTypeCode = item.ConnectorTypeCode,
            EvidenceKindCode = item.EvidenceKindCode,
        };

        private static int StablePositiveInt(params string[] values)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\u001f", values)));
            return (BitConverter.ToInt32(bytes, 0) & int.MaxValue);
        }

        public static string HashCanonical(SimulationWorldLandscapeCompositionTileResponse response)
        {
            var text = new StringBuilder()
                .Append(response.SchemaVersion).Append('|')
                .Append(response.TileKey).Append('|')
                .Append(response.AreaSetStableId).Append('|')
                .Append(response.GrammarRevision).Append('|')
                .Append(response.GrammarHashSha256).AppendLine();
            foreach (var item in response.Nodes.OrderBy(value => value.NodeStableId, StringComparer.Ordinal))
                text.Append(item.NodeStableId).Append('|').Append(item.ParentNodeStableId).Append('|')
                    .Append(item.NodeKindCode).Append('|').Append(item.SemanticCode).Append('|')
                    .Append(item.EvidenceKindCode).Append('|')
                    .Append(item.CenterEastingMeters.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.CenterNorthingMeters.ToString("R", CultureInfo.InvariantCulture)).AppendLine();
            foreach (var item in response.Edges.OrderBy(value => value.EdgeStableId, StringComparer.Ordinal))
                text.Append(item.EdgeStableId).Append('|').Append(item.FromNodeStableId).Append('|')
                    .Append(item.RelationCode).Append('|').Append(item.ToNodeStableId).AppendLine();
            foreach (var item in response.Placements.OrderBy(value => value.PlacementStableId, StringComparer.Ordinal))
                text.Append(item.PlacementStableId).Append('|').Append(item.CompositionKey).Append('|')
                    .Append(item.EastingMeters.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.NorthingMeters.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.PhysicalElevationMeters.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.RotationDegrees.ToString("R", CultureInfo.InvariantCulture)).Append('|')
                    .Append(item.Mirrored).Append('|').Append(item.DeterministicSeed).AppendLine();
            foreach (var item in response.ExternalConnectorStubs.OrderBy(value => value.StubStableId, StringComparer.Ordinal))
                text.Append(item.StubStableId).Append('|').Append(item.NeighborTileKey).Append('|')
                    .Append(item.RouteSignature).Append('|').Append(item.DirectionCode).AppendLine();
            foreach (var item in response.Unresolved.OrderBy(value => value.UnresolvedStableId, StringComparer.Ordinal))
                text.Append(item.UnresolvedStableId).Append('|').Append(item.ReasonCode).Append('|')
                    .Append(item.RequiredSemanticCode).AppendLine();
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
