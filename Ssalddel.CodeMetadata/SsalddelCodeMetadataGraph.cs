using System;
using System.Collections.Generic;
using System.Linq;

namespace Ssalddel.Contracts.Common.Metadata
{

public enum SsalddelCodeMetadataDiagnosticSeverity
{
    Warning,
    Error
}

public sealed record SsalddelCodeMetadataDiagnostic(
    SsalddelCodeMetadataDiagnosticSeverity Severity,
    string Code,
    string FeatureKey,
    string StepKey,
    string Message);

public sealed class SsalddelCodeMetadataGraph
{
    internal SsalddelCodeMetadataGraph(IReadOnlyList<SsalddelCodeMetadataDescriptor> steps)
    {
        Steps = steps;
    }

    public IReadOnlyList<SsalddelCodeMetadataDescriptor> Steps { get; }
}

public static class SsalddelCodeMetadataGraphBuilder
{
    public static SsalddelCodeMetadataGraph Build(IEnumerable<SsalddelCodeMetadataDescriptor> descriptors)
    {
        if (descriptors is null)
        {
            throw new ArgumentNullException(nameof(descriptors));
        }

        return new SsalddelCodeMetadataGraph(descriptors
            .OrderBy(item => item.FeatureKey, StringComparer.Ordinal)
            .ThenBy(item => item.FlowOrder)
            .ThenBy(item => item.StepKey, StringComparer.Ordinal)
            .ThenBy(item => item.ComponentType.FullName, StringComparer.Ordinal)
            .ToArray());
    }
}

public static class SsalddelCodeMetadataValidator
{
    public static IReadOnlyList<SsalddelCodeMetadataDiagnostic> Validate(
        SsalddelCodeMetadataGraph graph,
        bool requireNavigationFields = false)
    {
        if (graph is null)
        {
            throw new ArgumentNullException(nameof(graph));
        }

        var diagnostics = new List<SsalddelCodeMetadataDiagnostic>();
        foreach (var feature in graph.Steps.GroupBy(step => step.FeatureKey, StringComparer.Ordinal))
        {
            ValidateFeature(feature.Key, feature.ToArray(), requireNavigationFields, diagnostics);
        }

        return diagnostics
            .OrderBy(item => item.Severity)
            .ThenBy(item => item.FeatureKey, StringComparer.Ordinal)
            .ThenBy(item => item.StepKey, StringComparer.Ordinal)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateFeature(
        string featureKey,
        IReadOnlyList<SsalddelCodeMetadataDescriptor> steps,
        bool requireNavigationFields,
        ICollection<SsalddelCodeMetadataDiagnostic> diagnostics)
    {
        if (requireNavigationFields)
        {
            foreach (var step in steps)
            {
                if (string.IsNullOrWhiteSpace(step.StepKey)) AddError(diagnostics, "CODEMAP001", step, "StepKey가 비어 있습니다.");
                if (step.ExecutionStage == SsalddelCodeExecutionStage.Unspecified) AddError(diagnostics, "CODEMAP002", step, "ExecutionStage가 지정되지 않았습니다.");
                if (string.IsNullOrWhiteSpace(step.Boundary)) AddError(diagnostics, "CODEMAP003", step, "Boundary가 비어 있습니다.");
            }
        }

        var keyedSteps = steps
            .Where(step => !string.IsNullOrWhiteSpace(step.StepKey))
            .GroupBy(step => step.StepKey, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        foreach (var duplicate in keyedSteps.Where(pair => pair.Value.Length > 1))
        {
            diagnostics.Add(new SsalddelCodeMetadataDiagnostic(
                SsalddelCodeMetadataDiagnosticSeverity.Error,
                "CODEMAP004",
                featureKey,
                duplicate.Key,
                "같은 기능 안에서 StepKey가 중복되었습니다."));
        }

        foreach (var step in steps)
        {
            foreach (var dependencyKey in step.DependsOnStepKeys)
            {
                if (!keyedSteps.TryGetValue(dependencyKey, out var dependencies))
                {
                    AddError(diagnostics, "CODEMAP005", step, $"의존 단계 '{dependencyKey}'를 찾을 수 없습니다.");
                    continue;
                }

                if (dependencies.Any(dependency => dependency.FlowOrder >= step.FlowOrder))
                {
                    AddError(diagnostics, "CODEMAP006", step, $"의존 단계 '{dependencyKey}'의 FlowOrder가 현재 단계보다 앞서지 않습니다.");
                }
            }

            if (step.WritesTo != SsalddelCodeDataScope.None
                && (step.Effects & SsalddelCodeEffect.PersistentWrite) == 0
                && (step.Effects & SsalddelCodeEffect.UiStateMutation) == 0
                && (step.Effects & SsalddelCodeEffect.StateMutation) == 0)
            {
                AddError(diagnostics, "CODEMAP007", step, "WritesTo가 있지만 쓰기 부수효과가 표시되지 않았습니다.");
            }
        }

        DetectCycles(featureKey, keyedSteps, diagnostics);
    }

    private static void DetectCycles(
        string featureKey,
        IReadOnlyDictionary<string, SsalddelCodeMetadataDescriptor[]> keyedSteps,
        ICollection<SsalddelCodeMetadataDiagnostic> diagnostics)
    {
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        foreach (var stepKey in keyedSteps.Keys.OrderBy(key => key, StringComparer.Ordinal))
        {
            Visit(stepKey, featureKey, keyedSteps, visiting, visited, diagnostics);
        }
    }

    private static void Visit(
        string stepKey,
        string featureKey,
        IReadOnlyDictionary<string, SsalddelCodeMetadataDescriptor[]> keyedSteps,
        ISet<string> visiting,
        ISet<string> visited,
        ICollection<SsalddelCodeMetadataDiagnostic> diagnostics)
    {
        if (visited.Contains(stepKey)) return;
        if (!visiting.Add(stepKey))
        {
            diagnostics.Add(new SsalddelCodeMetadataDiagnostic(
                SsalddelCodeMetadataDiagnosticSeverity.Error,
                "CODEMAP008",
                featureKey,
                stepKey,
                "단계 의존성에 순환이 있습니다."));
            return;
        }

        if (keyedSteps.TryGetValue(stepKey, out var steps))
        {
            foreach (var dependency in steps.SelectMany(step => step.DependsOnStepKeys))
            {
                if (keyedSteps.ContainsKey(dependency)) Visit(dependency, featureKey, keyedSteps, visiting, visited, diagnostics);
            }
        }

        visiting.Remove(stepKey);
        visited.Add(stepKey);
    }

    private static void AddError(
        ICollection<SsalddelCodeMetadataDiagnostic> diagnostics,
        string code,
        SsalddelCodeMetadataDescriptor step,
        string message)
        => diagnostics.Add(new SsalddelCodeMetadataDiagnostic(
            SsalddelCodeMetadataDiagnosticSeverity.Error,
            code,
            step.FeatureKey,
            step.StepKey,
            message));
}
}
