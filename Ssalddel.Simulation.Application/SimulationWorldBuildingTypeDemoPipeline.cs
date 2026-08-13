using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
public sealed class SimulationWorld건물종류Demo표현
{
    public string BuildingNodeStableId { get; set; } = string.Empty;
    public string BuildingCategoryCode { get; set; } = string.Empty;
    public int RepresentedRecordCount { get; set; }
    public string FixtureSimulationStateCode { get; set; } = string.Empty;
    public string DefaultCompositionKey { get; set; } = string.Empty;
    public string DynamicIntentBundleKey { get; set; } = string.Empty;
}

public sealed class SimulationWorld건물종류Demo결과
{
    public string RuleCatalogRevision { get; set; } = string.Empty;
    public string InterpretationStableId { get; set; } = string.Empty;
    public bool RuleCatalogInserted { get; set; }
    public bool InterpretationInserted { get; set; }
    public string OutputHashSha256 { get; set; } = string.Empty;
    public IReadOnlyList<SimulationWorld건물종류Demo표현> Presentations { get; set; } =
        Array.Empty<SimulationWorld건물종류Demo표현>();
}

public sealed class SimulationWorld건물종류DemoPipeline
{
    public const string RepresentativeBuildingNotFoundCode = "SimulationWorldRepresentativeBuildingNotFound";
    private const int FixtureSeed = 51760;
    private static readonly string[] FixtureStates =
        { "Idle", "Operating", "Loading", "Maintenance" };

    private readonly ISimulationWorld공간실행Reader _spatialReader;
    private readonly ISimulationWorld객체표현규칙Store _store;
    private readonly SimulationWorld객체표현해석JobShell _shell;

    public SimulationWorld건물종류DemoPipeline(
        ISimulationWorld공간실행Reader spatialReader,
        ISimulationWorld객체표현규칙Store store,
        SimulationWorld객체표현해석JobShell shell)
    {
        _spatialReader = spatialReader;
        _store = store;
        _shell = shell;
    }

    public async Task<SimulationWorld건물종류Demo결과> 실행Async(
        string spatialBuildStableId,
        CancellationToken cancellationToken)
    {
        var spatial = await _spatialReader.조회Async(spatialBuildStableId, cancellationToken)
            ?? throw new InvalidOperationException(SimulationWorld객체표현해석JobShell.SpatialBuildNotFoundCode);
        var representatives = spatial.Nodes
            .Where(node => node.NodeKindCode == "Building"
                && node.RepresentativeGroupCode != null
                && node.RepresentativeGroupCode.StartsWith("building-category:", StringComparison.Ordinal)
                && node.RepresentedRecordCount.HasValue)
            .OrderBy(node => node.RepresentativeGroupCode, StringComparer.Ordinal)
            .ToArray();
        if (representatives.Length == 0)
            throw new InvalidOperationException(RepresentativeBuildingNotFoundCode);

        var catalogRevision = "pyeongchang-building-type-demo."
            + spatial.OutputHashSha256.Substring(0, 16) + ".v1";
        var simulationSessionStableId = "simulation-session:scenario-fixture:pyeongchang-building-type-demo:v1";
        var spatialRules = new List<SimulationWorld공간규칙Metadata>();
        var simulationRules = new List<SimulationWorldSimulation규칙Metadata>();
        var bindingRules = new List<SimulationWorld객체표현결합규칙>();
        var targets = new List<SimulationWorld객체표현대상사실>();
        var presentations = new List<SimulationWorld건물종류Demo표현>();
        foreach (var representative in representatives)
        {
            var category = representative.RepresentativeGroupCode!["building-category:".Length..];
            var state = FixtureState(category);
            var semanticCode = "Building:" + category;
            var spatialRuleId = "spatial-rule:building-category:" + category;
            var simulationRuleId = "simulation-rule:scenario-fixture:building-activity:" + category;
            var bindingRuleId = "binding-rule:building-category-demo:" + category;
            var compositionKey = "composition.building." + category + ".representative.v1";
            var intentKey = "intent-bundle.building." + state.ToLowerInvariant() + ".scenario-fixture.v1";
            spatialRules.Add(new SimulationWorld공간규칙Metadata
            {
                StableId = spatialRuleId, Revision = "r1", StatusCode = SimulationWorld규칙상태Codes.활성,
                SpatialFactKindCode = "BuildingCategory", OperatorCode = "Equals",
                ExpectedValueCode = category, RequiredEvidenceKindCode = SimulationWorld근거종류Codes.파생,
                Description = "공공데이터 건물 용도 대표군에 속하는 대표 건물이다.",
            });
            simulationRules.Add(new SimulationWorldSimulation규칙Metadata
            {
                StableId = simulationRuleId, Revision = "r1", StatusCode = SimulationWorld규칙상태Codes.활성,
                StateTypeCode = "ScenarioFixtureBuildingActivity", ExpectedStateCode = state,
                Description = "실제 회사 활동이 아닌 고정 seed 기반 화면 검증용 Simulation 상태다.",
            });
            bindingRules.Add(new SimulationWorld객체표현결합규칙
            {
                StableId = bindingRuleId, Revision = "r1", StatusCode = SimulationWorld규칙상태Codes.활성,
                ObjectSemanticCode = semanticCode, ScopeCode = SimulationWorld객체표현적용범위Codes.건물,
                SpatialRuleStableId = spatialRuleId, SpatialRuleRevision = "r1",
                SimulationRuleStableId = simulationRuleId, SimulationRuleRevision = "r1",
                SimulationRuleRequired = true, MinimumEvidenceKindCode = SimulationWorld근거종류Codes.파생,
                DefaultCompositionKey = compositionKey, DynamicIntentBundleKey = intentKey,
                UnmetRuleHandlingCode = SimulationWorld규칙미충족처리Codes.공간표현만,
                Priority = 100, PresentationOnly = true,
            });
            targets.Add(new SimulationWorld객체표현대상사실
            {
                TargetNodeStableId = representative.StableId, ObjectSemanticCode = semanticCode,
                ScopeCode = SimulationWorld객체표현적용범위Codes.건물,
                EvidenceKindCode = SimulationWorld근거종류Codes.파생,
                MatchedSpatialRuleStableIds = new[] { spatialRuleId },
                MatchedSimulationRuleStableIds = new[] { simulationRuleId },
            });
            presentations.Add(new SimulationWorld건물종류Demo표현
            {
                BuildingNodeStableId = representative.StableId, BuildingCategoryCode = category,
                RepresentedRecordCount = representative.RepresentedRecordCount!.Value,
                FixtureSimulationStateCode = state, DefaultCompositionKey = compositionKey,
                DynamicIntentBundleKey = intentKey,
            });
        }
        var catalog = new SimulationWorld객체표현규칙대장
        {
            CatalogRevision = catalogRevision, CreatedAtUtc = spatial.Nodes.Count > 0
                ? DateTimeOffset.Parse("2026-08-13T00:00:00Z") : DateTimeOffset.UnixEpoch,
            SpatialRules = spatialRules, SimulationRules = simulationRules, BindingRules = bindingRules,
        };
        var catalogStored = await _store.규칙대장저장Async(catalog, cancellationToken);
        var interpretationStableId = "object-presentation-interpretation:pyeongchang-building-type-demo:"
            + spatial.OutputHashSha256.Substring(0, 16) + ":v1";
        var interpreted = await _shell.실행Async(new SimulationWorld객체표현해석요청
        {
            InterpretationStableId = interpretationStableId,
            SpatialBuildStableId = spatial.BuildStableId, SpatialOutputHashSha256 = spatial.OutputHashSha256,
            SimulationSessionStableId = simulationSessionStableId, SimulationSessionRevision = 1, WorldTick = 0,
            RuleCatalogRevision = catalogRevision, InterpretedAtUtc = DateTimeOffset.Parse("2026-08-13T00:00:00Z"),
            Targets = targets,
        }, catalog, cancellationToken);
        return new SimulationWorld건물종류Demo결과
        {
            RuleCatalogRevision = catalogRevision, InterpretationStableId = interpretationStableId,
            RuleCatalogInserted = catalogStored.Inserted, InterpretationInserted = interpreted.Inserted,
            OutputHashSha256 = interpreted.OutputHashSha256, Presentations = presentations,
        };
    }

    private static string FixtureState(string category)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(FixtureSeed + "|" + category));
        return FixtureStates[bytes[0] % FixtureStates.Length];
    }
}
}
