using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Ssalddel.Simulation.Application
{
    internal sealed class SimulationLhSpatialKnowledgeProvider
    {
        private const string StatusResourceName =
            "Ssalddel.Simulation.SpatialKnowledge.wi-eh-status.v1.json";
        private const string P1PlanResourceName =
            "Ssalddel.Simulation.SpatialKnowledge.reference-play-01-harvest-shipping.v1.json";

        private readonly IReadOnlyDictionary<string, SimulationLhSpatialKnowledgeSpace> spaces;

        private SimulationLhSpatialKnowledgeProvider(
            string statusRevision,
            string compositionPlanStableId,
            SimulationLhSpatialKnowledgeH1Binding[] h1Bindings,
            IReadOnlyDictionary<string, SimulationLhSpatialKnowledgeSpace> spaces)
        {
            StatusRevision = statusRevision;
            CompositionPlanStableId = compositionPlanStableId;
            H1Bindings = h1Bindings;
            this.spaces = spaces;
        }

        public string StatusRevision { get; }
        public string CompositionPlanStableId { get; }
        public SimulationLhSpatialKnowledgeH1Binding[] H1Bindings { get; }

        public static SimulationLhSpatialKnowledgeProvider LoadEmbedded()
        {
            using var status = ReadResource(StatusResourceName);
            using var plan = ReadResource(P1PlanResourceName);
            return Parse(status.RootElement, plan.RootElement);
        }

        public SimulationLhSpatialKnowledgeSpace GetSpace(string spaceCode)
        {
            if (!spaces.TryGetValue(spaceCode, out var value))
                throw new InvalidOperationException(
                    "SimulationLhSpatialKnowledgeSpaceMissing:" + spaceCode);
            return value;
        }

        private static SimulationLhSpatialKnowledgeProvider Parse(
            JsonElement status,
            JsonElement plan)
        {
            Require(String(status, "schemaVersion") ==
                    "simulation-world-interaction-eh-status.v1",
                "StatusSchemaInvalid");
            Require(String(plan, "schemaVersion") ==
                    "simulation-world-interaction-spatial-composition-plan.v1",
                "PlanSchemaInvalid");
            var summary = status.GetProperty("summary");
            Require(summary.GetProperty("totalWorldInteractions").GetInt32() == 41,
                "StatusInteractionCountInvalid");
            Require(summary.GetProperty("establishedH1Count").GetInt32() == 13,
                "StatusEstablishedH1CountInvalid");
            Require(summary.GetProperty("officialH2DefinitionCount").GetInt32() == 0,
                "StatusMustNotClaimH2");

            var statusByWi = status.GetProperty("items").EnumerateArray()
                .ToDictionary(
                    value => String(value, "worldInteractionId"),
                    value => value.Clone(),
                    StringComparer.Ordinal);
            var planWiIds = Strings(plan.GetProperty("worldInteractionSequence"));
            Require(planWiIds.SequenceEqual(new[]
            {
                "WI-FARM-04", "WI-FARM-05", "WI-FARM-06", "WI-LOG-01",
            }), "PlanSequenceInvalid");

            var parsedSpaces = new Dictionary<string, SimulationLhSpatialKnowledgeSpace>(
                StringComparer.Ordinal);
            foreach (var value in plan.GetProperty("spaces").EnumerateArray())
            {
                var spaceCode = String(value, "spaceCode");
                var h1DefinitionRef = String(value, "h1DefinitionRef");
                var interactionH1Ref = String(value, "interactionH1Ref");
                var wiIds = Strings(value.GetProperty("worldInteractionIds"));
                var preferredCompositionKey = String(value, "preferredCompositionKey");
                Require(!parsedSpaces.ContainsKey(spaceCode),
                    "PlanSpaceDuplicate:" + spaceCode);
                Require(wiIds.Length > 0 && wiIds.All(planWiIds.Contains),
                    "PlanSpaceWiInvalid:" + spaceCode);
                foreach (var wiId in wiIds)
                {
                    Require(statusByWi.TryGetValue(wiId, out var wiStatus),
                        "StatusWiMissing:" + wiId);
                    Require(String(wiStatus, "lhEngineHandoffStateCode") ==
                            "ReadyForApprovedH1Input",
                        "StatusWiNotApprovedForHandoff:" + wiId);
                    Require(Strings(wiStatus.GetProperty("approvedH1DefinitionRefs"))
                            .Contains(h1DefinitionRef, StringComparer.Ordinal),
                        "StatusApprovedH1Mismatch:" + wiId);
                }
                var allowedPrefixes = Strings(value.GetProperty("expressionGrammarSetRefs"))
                    .Select(reference => reference + ":")
                    .ToArray();
                Require(allowedPrefixes.Any(prefix =>
                        preferredCompositionKey.StartsWith(prefix, StringComparison.Ordinal)),
                    "PlanCompositionNotAllowed:" + spaceCode);
                parsedSpaces.Add(spaceCode, new SimulationLhSpatialKnowledgeSpace(
                    spaceCode,
                    h1DefinitionRef,
                    interactionH1Ref,
                    wiIds,
                    preferredCompositionKey));
            }

            var h1Bindings = parsedSpaces.Values
                .GroupBy(value => value.InteractionH1Ref, StringComparer.Ordinal)
                .Select(group => new SimulationLhSpatialKnowledgeH1Binding(
                    group.Key,
                    group.SelectMany(value => value.WorldInteractionIds)
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal)
                        .ToArray()))
                .OrderBy(value => value.InteractionH1Ref, StringComparer.Ordinal)
                .ToArray();
            Require(h1Bindings.Length == 3, "PlanH1BindingCountInvalid");
            return new SimulationLhSpatialKnowledgeProvider(
                String(status, "revision"),
                String(plan, "planStableId"),
                h1Bindings,
                parsedSpaces);
        }

        private static JsonDocument ReadResource(string resourceName)
        {
            var assembly = typeof(SimulationLhSpatialKnowledgeProvider).GetTypeInfo().Assembly;
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    "SimulationLhSpatialKnowledgeResourceMissing:" + resourceName);
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return JsonDocument.Parse(memory.ToArray());
        }

        private static string String(JsonElement value, string propertyName)
            => value.GetProperty(propertyName).GetString()
               ?? throw new InvalidOperationException(
                   "SimulationLhSpatialKnowledgeTextMissing:" + propertyName);

        private static string[] Strings(JsonElement value)
            => value.EnumerateArray()
                .Select(item => item.GetString() ?? string.Empty)
                .Where(item => item.Length > 0)
                .ToArray();

        private static void Require(bool condition, string code)
        {
            if (!condition)
                throw new InvalidOperationException(
                    "SimulationLhSpatialKnowledgeInvalid:" + code);
        }
    }

    internal sealed class SimulationLhSpatialKnowledgeH1Binding
    {
        public SimulationLhSpatialKnowledgeH1Binding(
            string interactionH1Ref,
            string[] worldInteractionIds)
        {
            InteractionH1Ref = interactionH1Ref;
            WorldInteractionIds = worldInteractionIds;
        }

        public string InteractionH1Ref { get; }
        public string[] WorldInteractionIds { get; }
    }

    internal sealed class SimulationLhSpatialKnowledgeSpace
    {
        public SimulationLhSpatialKnowledgeSpace(
            string spaceCode,
            string h1DefinitionRef,
            string interactionH1Ref,
            string[] worldInteractionIds,
            string preferredCompositionKey)
        {
            SpaceCode = spaceCode;
            H1DefinitionRef = h1DefinitionRef;
            InteractionH1Ref = interactionH1Ref;
            WorldInteractionIds = worldInteractionIds;
            PreferredCompositionKey = preferredCompositionKey;
        }

        public string SpaceCode { get; }
        public string H1DefinitionRef { get; }
        public string InteractionH1Ref { get; }
        public string[] WorldInteractionIds { get; }
        public string PreferredCompositionKey { get; }
    }
}
