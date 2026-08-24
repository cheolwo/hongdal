using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public sealed class SimulationWorld상호작용공간모판Compiler
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private static readonly HashSet<string> ForbiddenPropertyNames = new(
            new[]
            {
                "areaSetStableId", "landscapeGraphStableId", "landscapeNodeStableId",
                "tileKey", "absoluteWorldPosition", "worldEastingMeters",
                "worldNorthingMeters", "eastingMeters", "northingMeters",
                "latitude", "longitude", "prefabPath", "assetGuid", "guid",
                "syntyPackPath", "material", "scenePath",
            },
            StringComparer.OrdinalIgnoreCase);

        private readonly string catalogPath;
        private readonly string worldInteractionCatalogPath;
        private readonly string landscapeGrammarPath;

        public SimulationWorld상호작용공간모판Compiler(
            string catalogPath,
            string worldInteractionCatalogPath,
            string landscapeGrammarPath)
        {
            this.catalogPath = ResolveRequiredPath(catalogPath);
            this.worldInteractionCatalogPath = ResolveRequiredPath(worldInteractionCatalogPath);
            this.landscapeGrammarPath = ResolveRequiredPath(landscapeGrammarPath);
        }

        public SimulationWorld상호작용공간모판Catalog Compile()
        {
            var catalogDocument = Deserialize<CatalogDocument>(catalogPath);
            Require(catalogDocument.SchemaVersion ==
                    SimulationWorld상호작용공간모판Codes.CatalogSchemaVersion,
                "WiSpatialSeedbedCatalogSchemaVersionMismatch");
            RequireText(catalogDocument.Revision, "WiSpatialSeedbedCatalogRevisionMissing");
            RequireDistinct(catalogDocument.DefinitionRefs,
                "WiSpatialSeedbedDefinitionRefDuplicate");
            Require(catalogDocument.DefinitionRefs.Length > 0,
                "WiSpatialSeedbedDefinitionsMissing");
            Require(catalogDocument.PresentationOnly,
                "WiSpatialSeedbedCatalogMustBePresentationOnly");

            var interactions = ReadWorldInteractions(out var worldInteractionRevision);
            Require(worldInteractionRevision == catalogDocument.WorldInteractionCatalogRevision,
                "WiSpatialSeedbedWorldInteractionRevisionMismatch");
            var compositionKeys = ReadLandscapeGrammar(out var grammarRevision);
            Require(grammarRevision == catalogDocument.LandscapeGrammarRevision,
                "WiSpatialSeedbedLandscapeGrammarRevisionMismatch");

            var rootDirectory = Path.GetDirectoryName(catalogPath)!;
            var definitions = catalogDocument.DefinitionRefs.Select(reference =>
                CompileDefinition(
                    Path.GetFullPath(Path.Combine(rootDirectory, reference)),
                    rootDirectory,
                    worldInteractionRevision,
                    grammarRevision,
                    interactions,
                    compositionKeys)).ToArray();

            RequireDistinct(definitions.Select(value => value.StableId),
                "WiSpatialSeedbedStableIdDuplicate");
            RequireDistinct(definitions.SelectMany(value => value.IncludedWiIds),
                "WiSpatialSeedbedWiAssignedMoreThanOnce");
            ValidateAutomaticTransitionParents(definitions, interactions);
            ValidateCrossSeedbedTransitions(definitions, interactions);

            return new SimulationWorld상호작용공간모판Catalog
            {
                Revision = catalogDocument.Revision,
                WorldInteractionCatalogRevision = worldInteractionRevision,
                LandscapeGrammarRevision = grammarRevision,
                CatalogHashSha256 = HashText(string.Join("|", new[]
                {
                    catalogDocument.Revision,
                    worldInteractionRevision,
                    grammarRevision,
                    string.Join("|", definitions.Select(value => value.DefinitionHashSha256)),
                })),
                Definitions = definitions,
                PresentationOnly = true,
                IsOperationalState = false,
            };
        }

        private static SimulationWorld상호작용공간모판Definition CompileDefinition(
            string definitionPath,
            string rootDirectory,
            string worldInteractionRevision,
            string grammarRevision,
            IReadOnlyDictionary<string, WorldInteractionInfo> interactions,
            ISet<string> compositionKeys)
        {
            Require(File.Exists(definitionPath), "WiSpatialSeedbedDefinitionMissing");
            using (var document = JsonDocument.Parse(File.ReadAllBytes(definitionPath)))
                ValidateForbiddenProperties(document.RootElement);
            var definition = Deserialize<SimulationWorld상호작용공간모판Definition>(definitionPath);
            Require(definition.SchemaVersion == SimulationWorld상호작용공간모판Codes.SchemaVersion,
                "WiSpatialSeedbedSchemaVersionMismatch");
            Require(definition.StableId.StartsWith("wi-spatial-seedbed:", StringComparison.Ordinal)
                    && definition.Revision > 0,
                "WiSpatialSeedbedIdentityInvalid");
            RequireText(definition.Title, "WiSpatialSeedbedTitleMissing");
            RequireText(definition.AuthoredDocument, "WiSpatialSeedbedAuthoredDocumentMissing");
            Require(definition.ReviewStatusCode ==
                    SimulationWorld상호작용공간모판Codes.ApprovedForSimulation,
                "WiSpatialSeedbedReviewRequired");
            Require(definition.PresentationOnly && !definition.IsOperationalState,
                "WiSpatialSeedbedAuthorityBoundaryInvalid");
            RequireDistinct(definition.IncludedWiIds, "WiSpatialSeedbedIncludedWiDuplicate");
            Require(definition.IncludedWiIds.Length > 0, "WiSpatialSeedbedIncludedWiMissing");
            RequireDistinct(definition.InternalSpaces.Select(value => value.SpaceCode),
                "WiSpatialSeedbedInternalSpaceDuplicate");
            Require(definition.InternalSpaces.Length > 0, "WiSpatialSeedbedInternalSpaceMissing");
            RequireDistinct(definition.WiBindings.Select(value => value.WorldInteractionId),
                "WiSpatialSeedbedWiBindingDuplicate");
            RequireSet(definition.IncludedWiIds,
                definition.WiBindings.Select(value => value.WorldInteractionId),
                "WiSpatialSeedbedWiBindingMismatch");
            RequireDistinct(definition.ExternalConnectorStubs.Select(value => value.StubCode),
                "WiSpatialSeedbedExternalConnectorDuplicate");

            var spaces = definition.InternalSpaces.ToDictionary(
                value => value.SpaceCode, value => value, StringComparer.Ordinal);
            foreach (var space in definition.InternalSpaces)
            {
                RequireText(space.SpaceCode, "WiSpatialSeedbedInternalSpaceCodeMissing");
                RequireText(space.SpatialRoleCode, "WiSpatialSeedbedSpatialRoleMissing");
                RequireDistinct(space.CapabilityCodes,
                    "WiSpatialSeedbedCapabilityDuplicate");
                RequireDistinct(space.BaseCapacities.Select(value => value.CapacityCode),
                    "WiSpatialSeedbedCapacityDuplicate");
                RequireDistinct(space.AllowedLandscapeCompositionKeys,
                    "WiSpatialSeedbedCompositionKeyDuplicate");
                Require(space.AllowedLandscapeCompositionKeys.All(compositionKeys.Contains),
                    "WiSpatialSeedbedCompositionKeyUnknown");
                foreach (var capacity in space.BaseCapacities)
                    Require(capacity.Quantity > 0m
                            && !string.IsNullOrWhiteSpace(capacity.CapacityCode)
                            && !string.IsNullOrWhiteSpace(capacity.UnitCode),
                        "WiSpatialSeedbedCapacityInvalid");
            }

            foreach (var binding in definition.WiBindings)
            {
                Require(spaces.TryGetValue(binding.InternalSpaceCode, out var space),
                    "WiSpatialSeedbedWiBindingSpaceMissing");
                Require(interactions.TryGetValue(binding.WorldInteractionId, out var interaction),
                    "WiSpatialSeedbedWorldInteractionUnknown");
                Require(interaction!.ImplementationStage == "E3",
                    "WiSpatialSeedbedWorldInteractionNotE3");
                Require(interaction.Kind != "SharedPolicy",
                    "WiSpatialSeedbedSharedPolicyNotPlaceable");
                ValidateCapabilities(interaction, space!);
            }

            foreach (var relation in definition.InternalRelations)
            {
                Require(spaces.ContainsKey(relation.FromSpaceCode)
                        && spaces.ContainsKey(relation.ToSpaceCode)
                        && relation.FromSpaceCode != relation.ToSpaceCode,
                    "WiSpatialSeedbedInternalRelationInvalid");
                RequireText(relation.RelationCode, "WiSpatialSeedbedRelationCodeMissing");
                RequireText(relation.ConnectorTypeCode, "WiSpatialSeedbedRelationConnectorMissing");
            }

            foreach (var connector in definition.ExternalConnectorStubs)
            {
                Require(spaces.ContainsKey(connector.InternalSpaceCode),
                    "WiSpatialSeedbedExternalConnectorSpaceMissing");
                Require(connector.FlowDirectionCode is
                        SimulationWorld상호작용공간모판Codes.Input or
                        SimulationWorld상호작용공간모판Codes.Output or
                        SimulationWorld상호작용공간모판Codes.Bidirectional,
                    "WiSpatialSeedbedExternalConnectorDirectionInvalid");
                RequireText(connector.ConnectorTypeCode,
                    "WiSpatialSeedbedExternalConnectorTypeMissing");
                Require(interactions.ContainsKey(connector.AdjacentWorldInteractionId),
                    "WiSpatialSeedbedExternalConnectorAdjacentWiUnknown");
            }

            ValidateInternalWiTransitions(definition, interactions);
            ValidateTransform(definition.TransformConstraint);
            RequireDistinct(definition.EvidenceRefs, "WiSpatialSeedbedEvidenceRefDuplicate");
            Require(definition.EvidenceRefs.Length > 0, "WiSpatialSeedbedEvidenceMissing");

            var authoredPath = Path.GetFullPath(Path.Combine(rootDirectory,
                definition.AuthoredDocument));
            Require(File.Exists(authoredPath), "WiSpatialSeedbedAuthoredDocumentMissing");
            var authoredText = File.ReadAllText(authoredPath, Encoding.UTF8);
            Require(authoredText.Contains(definition.StableId, StringComparison.Ordinal),
                "WiSpatialSeedbedAuthoredStableIdMismatch");
            Require(definition.IncludedWiIds.All(value =>
                    authoredText.Contains(value, StringComparison.Ordinal)),
                "WiSpatialSeedbedAuthoredWiMismatch");

            definition.SourceFileHashSha256 = HashBytes(File.ReadAllBytes(definitionPath));
            definition.AuthoredDocumentHashSha256 = HashBytes(File.ReadAllBytes(authoredPath));
            definition.DefinitionHashSha256 = HashDefinition(
                definition, worldInteractionRevision, grammarRevision);
            return definition;
        }

        private static void ValidateCapabilities(
            WorldInteractionInfo interaction,
            SimulationWorld상호작용공간모판InternalSpace space)
        {
            foreach (var requirement in interaction.SpatialRequirements)
            {
                if (requirement == "OriginLoading")
                    continue;
                var capability = requirement.StartsWith("Spatial.", StringComparison.Ordinal)
                    ? requirement
                    : "Spatial." + requirement;
                Require(space.CapabilityCodes.Contains(capability, StringComparer.Ordinal),
                    "WiSpatialSeedbedCapabilityMissing:" + interaction.Id + ":" + capability);
            }
        }

        private static void ValidateInternalWiTransitions(
            SimulationWorld상호작용공간모판Definition definition,
            IReadOnlyDictionary<string, WorldInteractionInfo> interactions)
        {
            var bindings = definition.WiBindings.ToDictionary(
                value => value.WorldInteractionId,
                value => value.InternalSpaceCode,
                StringComparer.Ordinal);
            foreach (var fromId in definition.IncludedWiIds)
            {
                foreach (var toId in interactions[fromId].SuccessorWiIds.Where(bindings.ContainsKey))
                {
                    var fromSpace = bindings[fromId];
                    var toSpace = bindings[toId];
                    if (fromSpace == toSpace) continue;
                    Require(HasInternalPath(fromSpace, toSpace, definition.InternalRelations),
                        "WiSpatialSeedbedInternalTransitionUnresolved:" + fromId + ":" + toId);
                }
            }

            foreach (var interaction in definition.IncludedWiIds.Select(value => interactions[value])
                         .Where(value => value.SpatialRequirements.Contains("OriginLoading",
                             StringComparer.Ordinal)))
            {
                Require(definition.InternalSpaces.Any(value =>
                            value.CapabilityCodes.Contains(
                                Simulation공간능력Codes.LoadingWorkArea, StringComparer.Ordinal))
                        && definition.ExternalConnectorStubs.Any(value =>
                            value.FlowDirectionCode ==
                            SimulationWorld상호작용공간모판Codes.Output),
                    "WiSpatialSeedbedOriginLoadingUnresolved:" + interaction.Id);
            }
        }

        private static bool HasInternalPath(
            string from,
            string to,
            IReadOnlyCollection<SimulationWorld상호작용공간모판InternalRelation> relations)
        {
            var visited = new HashSet<string>(StringComparer.Ordinal) { from };
            var queue = new Queue<string>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var next in relations.Where(value => value.FromSpaceCode == current)
                             .Select(value => value.ToSpaceCode))
                {
                    if (next == to) return true;
                    if (visited.Add(next)) queue.Enqueue(next);
                }
            }
            return false;
        }

        private static void ValidateAutomaticTransitionParents(
            IReadOnlyCollection<SimulationWorld상호작용공간모판Definition> definitions,
            IReadOnlyDictionary<string, WorldInteractionInfo> interactions)
        {
            var selected = definitions.SelectMany(value => value.IncludedWiIds)
                .ToHashSet(StringComparer.Ordinal);
            foreach (var interaction in selected.Select(value => interactions[value])
                         .Where(value => value.Kind == "AutomaticTransition"))
            {
                Require(FindParentCommand(interaction, interactions, selected,
                        new HashSet<string>(StringComparer.Ordinal)),
                    "WiSpatialSeedbedAutomaticTransitionParentMissing:" + interaction.Id);
            }
        }

        private static bool FindParentCommand(
            WorldInteractionInfo interaction,
            IReadOnlyDictionary<string, WorldInteractionInfo> interactions,
            ISet<string> selected,
            ISet<string> visited)
        {
            if (!visited.Add(interaction.Id)) return false;
            foreach (var predecessorId in interaction.PredecessorWiIds)
            {
                if (!selected.Contains(predecessorId)
                    || !interactions.TryGetValue(predecessorId, out var predecessor)) continue;
                if (predecessor.Kind == "Command") return true;
                if (FindParentCommand(predecessor, interactions, selected, visited)) return true;
            }
            return false;
        }

        private static void ValidateCrossSeedbedTransitions(
            IReadOnlyCollection<SimulationWorld상호작용공간모판Definition> definitions,
            IReadOnlyDictionary<string, WorldInteractionInfo> interactions)
        {
            var owner = definitions.SelectMany(definition => definition.IncludedWiIds.Select(
                    wiId => (WiId: wiId, Definition: definition)))
                .ToDictionary(value => value.WiId, value => value.Definition,
                    StringComparer.Ordinal);
            foreach (var from in owner.Keys)
            {
                foreach (var to in interactions[from].SuccessorWiIds.Where(owner.ContainsKey))
                {
                    var fromDefinition = owner[from];
                    var toDefinition = owner[to];
                    if (ReferenceEquals(fromDefinition, toDefinition)) continue;
                    var outputs = fromDefinition.ExternalConnectorStubs.Where(value =>
                            value.FlowDirectionCode ==
                            SimulationWorld상호작용공간모판Codes.Output
                            && value.AdjacentWorldInteractionId == to)
                        .ToArray();
                    var inputs = toDefinition.ExternalConnectorStubs.Where(value =>
                            value.FlowDirectionCode ==
                            SimulationWorld상호작용공간모판Codes.Input
                            && value.AdjacentWorldInteractionId == from)
                        .ToArray();
                    Require(outputs.Length > 0 && inputs.Length > 0,
                        "WiSpatialSeedbedExternalTransitionUnresolved:" + from + ":" + to);
                    Require(outputs.Any(output => inputs.Any(input =>
                                input.ConnectorTypeCode == output.ConnectorTypeCode)),
                        "WiSpatialSeedbedExternalConnectorTypeMismatch:" + from + ":" + to);
                }
            }
        }

        private static void ValidateTransform(
            SimulationWorld상호작용공간모판TransformConstraint value)
        {
            if (value == null)
                throw new InvalidOperationException("WiSpatialSeedbedTransformMissing");
            RequireDistinct(value.AllowedRotationCodes, "WiSpatialSeedbedRotationDuplicate");
            Require(value.AllowedRotationCodes.Length > 0
                    && value.ScaleModeCode is
                        SimulationWorld상호작용공간모판Codes.Fixed or
                        SimulationWorld상호작용공간모판Codes.Uniform,
                "WiSpatialSeedbedTransformInvalid");
            Require(value.MinimumWidthMeters > 0d && value.MinimumDepthMeters > 0d
                    && value.PreferredWidthMeters >= value.MinimumWidthMeters
                    && value.PreferredDepthMeters >= value.MinimumDepthMeters
                    && value.MaximumWidthMeters >= value.PreferredWidthMeters
                    && value.MaximumDepthMeters >= value.PreferredDepthMeters,
                "WiSpatialSeedbedSizeConstraintInvalid");
        }

        private Dictionary<string, WorldInteractionInfo> ReadWorldInteractions(
            out string revision)
        {
            using var document = JsonDocument.Parse(File.ReadAllBytes(worldInteractionCatalogPath));
            revision = document.RootElement.GetProperty("revision").GetString() ?? string.Empty;
            var result = new Dictionary<string, WorldInteractionInfo>(StringComparer.Ordinal);
            foreach (var item in document.RootElement.GetProperty("items").EnumerateArray())
            {
                var info = new WorldInteractionInfo
                {
                    Id = RequiredString(item, "id"),
                    Kind = RequiredString(item, "kind"),
                    ImplementationStage = RequiredString(
                        item.GetProperty("implementation"), "currentStage"),
                    SpatialRequirements = ReadStrings(item, "spatialRequirements"),
                    PredecessorWiIds = ReadStrings(item, "predecessorWiIds"),
                    SuccessorWiIds = ReadStrings(item, "successorWiIds"),
                };
                Require(result.TryAdd(info.Id, info),
                    "WiSpatialSeedbedWorldInteractionDuplicate");
            }
            return result;
        }

        private HashSet<string> ReadLandscapeGrammar(out string revision)
        {
            using var document = JsonDocument.Parse(File.ReadAllBytes(landscapeGrammarPath));
            revision = RequiredString(document.RootElement, "catalogRevision");
            return document.RootElement.GetProperty("entries").EnumerateArray()
                .Select(value => RequiredString(value, "compositionKey"))
                .ToHashSet(StringComparer.Ordinal);
        }

        private static string HashDefinition(
            SimulationWorld상호작용공간모판Definition value,
            string worldInteractionRevision,
            string grammarRevision)
        {
            var builder = new StringBuilder();
            builder.Append(value.SchemaVersion).Append('|').Append(value.StableId).Append('|')
                .Append(value.Revision).Append('|').Append(value.Title).Append('|')
                .Append(value.Summary).Append('|').Append(worldInteractionRevision).Append('|')
                .Append(grammarRevision).Append('|')
                .AppendJoin(",", value.IncludedWiIds);
            foreach (var space in value.InternalSpaces)
            {
                builder.Append("|S:").Append(space.SpaceCode).Append(':')
                    .Append(space.SpatialRoleCode).Append(':')
                    .AppendJoin(",", space.CapabilityCodes.OrderBy(item => item,
                        StringComparer.Ordinal));
                foreach (var capacity in space.BaseCapacities.OrderBy(item => item.CapacityCode,
                             StringComparer.Ordinal))
                    builder.Append(':').Append(capacity.CapacityCode).Append('=')
                        .Append(capacity.Quantity.ToString(CultureInfo.InvariantCulture))
                        .Append(capacity.UnitCode);
                builder.Append(':').AppendJoin(",", space.AllowedLandscapeCompositionKeys
                    .OrderBy(item => item, StringComparer.Ordinal));
            }
            foreach (var binding in value.WiBindings)
                builder.Append("|W:").Append(binding.WorldInteractionId).Append('>')
                    .Append(binding.InternalSpaceCode);
            foreach (var relation in value.InternalRelations)
                builder.Append("|R:").Append(relation.RelationCode).Append(':')
                    .Append(relation.FromSpaceCode).Append('>')
                    .Append(relation.ToSpaceCode).Append(':').Append(relation.ConnectorTypeCode);
            foreach (var connector in value.ExternalConnectorStubs)
                builder.Append("|C:").Append(connector.StubCode).Append(':')
                    .Append(connector.InternalSpaceCode).Append(':')
                    .Append(connector.ConnectorTypeCode).Append(':')
                    .Append(connector.FlowDirectionCode).Append(':')
                    .Append(connector.AdjacentWorldInteractionId);
            var transform = value.TransformConstraint;
            builder.Append("|T:").AppendJoin(",", transform.AllowedRotationCodes
                    .OrderBy(item => item, StringComparer.Ordinal))
                .Append(':').Append(transform.ScaleModeCode)
                .Append(':').Append(transform.MinimumWidthMeters.ToString(CultureInfo.InvariantCulture))
                .Append('x').Append(transform.MinimumDepthMeters.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(transform.PreferredWidthMeters.ToString(CultureInfo.InvariantCulture))
                .Append('x').Append(transform.PreferredDepthMeters.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(transform.MaximumWidthMeters.ToString(CultureInfo.InvariantCulture))
                .Append('x').Append(transform.MaximumDepthMeters.ToString(CultureInfo.InvariantCulture))
                .Append('|').Append(value.ReviewStatusCode)
                .Append('|').AppendJoin(",", value.EvidenceRefs.OrderBy(item => item,
                    StringComparer.Ordinal));
            return HashText(builder.ToString());
        }

        private static void ValidateForbiddenProperties(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    Require(!ForbiddenPropertyNames.Contains(property.Name),
                        "WiSpatialSeedbedForbiddenProperty:" + property.Name);
                    ValidateForbiddenProperties(property.Value);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                    ValidateForbiddenProperties(item);
            }
        }

        private static string[] ReadStrings(JsonElement source, string propertyName) =>
            source.GetProperty(propertyName).EnumerateArray()
                .Select(value => value.GetString() ?? string.Empty).ToArray();

        private static string RequiredString(JsonElement source, string propertyName)
        {
            var value = source.GetProperty(propertyName).GetString() ?? string.Empty;
            RequireText(value, "WiSpatialSeedbedRequiredValueMissing:" + propertyName);
            return value;
        }

        private static T Deserialize<T>(string path) =>
            JsonSerializer.Deserialize<T>(File.ReadAllBytes(path), JsonOptions)
            ?? throw new InvalidOperationException("WiSpatialSeedbedJsonInvalid");

        private static string ResolveRequiredPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("공간 모판 경로가 필요합니다.", nameof(value));
            if (Path.IsPathRooted(value) && File.Exists(value)) return Path.GetFullPath(value);
            var direct = Path.GetFullPath(value);
            if (File.Exists(direct)) return direct;
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                var candidate = Path.GetFullPath(Path.Combine(current.FullName, value));
                if (File.Exists(candidate)) return candidate;
                current = current.Parent;
            }
            throw new FileNotFoundException("공간 모판 참조 파일을 찾을 수 없습니다.", value);
        }

        private static void RequireSet(
            IEnumerable<string> expected,
            IEnumerable<string> actual,
            string errorCode) =>
            Require(expected.OrderBy(value => value, StringComparer.Ordinal).SequenceEqual(
                    actual.OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal),
                errorCode);

        private static void RequireDistinct(IEnumerable<string> values, string errorCode)
        {
            var items = values?.ToArray() ?? Array.Empty<string>();
            Require(items.All(value => !string.IsNullOrWhiteSpace(value))
                    && items.Distinct(StringComparer.Ordinal).Count() == items.Length,
                errorCode);
        }

        private static void RequireText(string value, string errorCode) =>
            Require(!string.IsNullOrWhiteSpace(value), errorCode);

        private static void Require(bool condition, string errorCode)
        {
            if (!condition) throw new InvalidOperationException(errorCode);
        }

        private static string HashText(string value) =>
            HashBytes(Encoding.UTF8.GetBytes(value));

        private static string HashBytes(byte[] value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(value)).Replace("-", string.Empty)
                .ToLowerInvariant();
        }

        private sealed class CatalogDocument
        {
            public string SchemaVersion { get; set; } = string.Empty;
            public string Revision { get; set; } = string.Empty;
            public string WorldInteractionCatalogRevision { get; set; } = string.Empty;
            public string LandscapeGrammarRevision { get; set; } = string.Empty;
            public string[] DefinitionRefs { get; set; } = Array.Empty<string>();
            public bool PresentationOnly { get; set; } = true;
        }

        private sealed class WorldInteractionInfo
        {
            public string Id { get; set; } = string.Empty;
            public string Kind { get; set; } = string.Empty;
            public string ImplementationStage { get; set; } = string.Empty;
            public string[] SpatialRequirements { get; set; } = Array.Empty<string>();
            public string[] PredecessorWiIds { get; set; } = Array.Empty<string>();
            public string[] SuccessorWiIds { get; set; } = Array.Empty<string>();
        }
    }

    public sealed class SimulationWorld상호작용공간모판ScenarioProfile
    {
        public string Revision { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public SimulationWorld상호작용공간모판ScenarioSpaceBinding[] SpaceBindings { get; set; } =
            Array.Empty<SimulationWorld상호작용공간모판ScenarioSpaceBinding>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationWorld상호작용공간모판ScenarioSpaceBinding
    {
        public string SeedbedStableId { get; set; } = string.Empty;
        public string InternalSpaceCode { get; set; } = string.Empty;
        public string SpatialStableId { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public string AreaStableId { get; set; } = string.Empty;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "WI와 공간 능력·예약·연결 책임을 결속한다.",
        Boundary = "H 포함 깊이와 E 증거 성숙도를 서로 대신하지 않는다.")]
    public static class SimulationWorld상호작용공간모판ScenarioBuilder
    {
        public static Simulation공간세계InitialStateRequest Build(
            SimulationWorld상호작용공간모판Catalog catalog,
            SimulationWorld상호작용공간모판ScenarioProfile profile)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (string.IsNullOrWhiteSpace(profile.Revision))
                throw new InvalidOperationException("WiSpatialSeedbedScenarioRevisionMissing");
            var definitionMap = catalog.Definitions.ToDictionary(
                value => value.StableId, value => value, StringComparer.Ordinal);
            var boundSpaces = catalog.Definitions.SelectMany(definition =>
                    definition.WiBindings.Select(binding =>
                        (definition.StableId, binding.InternalSpaceCode)))
                .Distinct().ToArray();
            var profileMap = profile.SpaceBindings.ToDictionary(
                value => value.SeedbedStableId + "\u001f" + value.InternalSpaceCode,
                value => value,
                StringComparer.Ordinal);
            if (profileMap.Count != profile.SpaceBindings.Length)
                throw new InvalidOperationException("WiSpatialSeedbedScenarioBindingDuplicate");

            var definitions = boundSpaces.Select(key =>
            {
                var mapKey = key.StableId + "\u001f" + key.InternalSpaceCode;
                if (!profileMap.TryGetValue(mapKey, out var binding))
                    throw new InvalidOperationException(
                        "WiSpatialSeedbedScenarioBindingMissing:" + key.StableId + ":" +
                        key.InternalSpaceCode);
                var seedbed = definitionMap[key.StableId];
                var space = seedbed.InternalSpaces.Single(value =>
                    value.SpaceCode == key.InternalSpaceCode);
                return new Simulation공간정의InitialRequest
                {
                    SpatialStableId = binding.SpatialStableId,
                    FacilityStableId = binding.FacilityStableId,
                    AreaStableId = binding.AreaStableId,
                    AreaSetStableId = profile.AreaSetStableId,
                    EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                    AccessStateCode = Simulation공간접근상태Codes.Available,
                    CapabilityCodes = space.CapabilityCodes.ToArray(),
                    BaseCapacities = space.BaseCapacities.Select(value =>
                        new Simulation공간용량Snapshot
                        {
                            CapacityCode = value.CapacityCode,
                            Quantity = value.Quantity,
                            UnitCode = value.UnitCode,
                        }).ToArray(),
                    DefinitionRevision = "seedbed:" + seedbed.Revision + ";profile:" +
                        profile.Revision,
                    DefinitionHashSha256 = Hash(seedbed.DefinitionHashSha256 + "|" +
                        profile.Revision + "|" + key.InternalSpaceCode + "|" +
                        binding.SpatialStableId),
                    SourceStableIds = profile.SourceStableIds.Concat(new[]
                    {
                        seedbed.StableId,
                        "seedbed-sha256:" + seedbed.DefinitionHashSha256,
                        "limitation:scenario-spatial-seedbed-not-landscape-graph",
                    }).Distinct(StringComparer.Ordinal).OrderBy(value => value,
                        StringComparer.Ordinal).ToArray(),
                };
            }).OrderBy(value => value.SpatialStableId, StringComparer.Ordinal).ToArray();
            if (definitions.Select(value => value.SpatialStableId).Distinct(StringComparer.Ordinal)
                    .Count() != definitions.Length)
                throw new InvalidOperationException("WiSpatialSeedbedScenarioSpatialIdDuplicate");
            return new Simulation공간세계InitialStateRequest { Definitions = definitions };
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
