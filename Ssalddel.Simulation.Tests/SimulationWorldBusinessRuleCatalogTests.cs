using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldBusinessRuleCatalogTests
{
    private const string BuildId = "world-build:test:pyeongchang";
    private const string Hash = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private const string AreaSetId = "area-set:sim:pyeongchang:farm-hub-town.v1";

    [Fact]
    public void 평창군_시설기능과_업무규칙을_하나의_결정적대장으로_구성한다()
    {
        var catalog = PyeongchangSimulationWorld업무규칙CatalogFactory.Create(BuildId, Hash, AreaSetId);

        SimulationWorld업무규칙집결Validator.Validate(catalog);
        var firstHash = SimulationWorld업무규칙집결Validator.ComputeHash(catalog);
        var secondHash = SimulationWorld업무규칙집결Validator.ComputeHash(catalog);

        Assert.Equal(4, catalog.Facilities.Count);
        Assert.Equal(18, catalog.Capabilities.Count);
        Assert.Equal(18, catalog.Rules.Count);
        Assert.Equal(11, catalog.Bindings.Count);
        Assert.Contains(catalog.Rules, value =>
            value.StableId == PyeongchangSimulationWorldStableIds.창고적재규칙
            && value.InputContractKey == nameof(SimulationWarehousePutAwayPreviewRequest));
        Assert.Contains(catalog.Rules, value =>
            value.StableId == PyeongchangSimulationWorldStableIds.팀역할Card장착규칙
            && value.InputContractKey == nameof(SimulationTeamRoleCardEquipRequest));
        Assert.Contains(catalog.Rules, value =>
            value.StableId == PyeongchangSimulationWorldStableIds.팀활동시작규칙
            && value.OutputContractKey == nameof(SimulationTeamRoleCardStateSnapshot));
        Assert.Contains(catalog.Rules, value =>
            value.StableId == PyeongchangSimulationWorldStableIds.팀활동종료규칙);
        Assert.Contains(catalog.Rules, value =>
            value.StableId == PyeongchangSimulationWorldStableIds.L2타일발견보상규칙
            && value.InputContractKey == nameof(SimulationTileTraversalConfirmRequest));
        Assert.Contains(catalog.Rules, value =>
            value.StableId == PyeongchangSimulationWorldStableIds.수집Card뽑기규칙
            && value.OutputContractKey == nameof(SimulationCollectibleCardDrawResponse));
        Assert.Single(catalog.ScenarioRuleSets);
        Assert.Equal(18, catalog.ScenarioRuleSets[0].Items.Count);
        Assert.Equal(firstHash, secondHash);
        Assert.All(catalog.Rules, rule => Assert.True(rule.SimulationOnly));
    }

    [Fact]
    public void 시설에_없는_기능을_규칙에_연결하면_거부한다()
    {
        var catalog = PyeongchangSimulationWorld업무규칙CatalogFactory.Create(BuildId, Hash, AreaSetId);
        var binding = catalog.Bindings[0];
        binding.CapabilityCode = "UnsupportedCapability";

        var exception = Assert.Throws<InvalidOperationException>(() =>
            SimulationWorld업무규칙집결Validator.Validate(catalog));

        Assert.StartsWith(SimulationWorld업무규칙집결Validator.InvalidCode, exception.Message);
    }

    [Fact]
    public async Task 공간실행의_Area노드가_모두_있을때만_집결원장을_저장한다()
    {
        var spatial = Snapshot(includeTown: true);
        var store = new RecordingStore();
        var shell = new SimulationWorld업무규칙집결JobShell(new FixedReader(spatial), store);

        var result = await shell.실행Async(BuildId, CancellationToken.None);

        Assert.True(result.Inserted);
        Assert.NotNull(store.Catalog);
        Assert.Equal(BuildId, store.Catalog!.SpatialBuildStableId);
        Assert.Equal(4, result.FacilityCount);
    }

    [Fact]
    public async Task 공간실행에_시설Area가_없으면_규칙집결을_거부한다()
    {
        var shell = new SimulationWorld업무규칙집결JobShell(
            new FixedReader(Snapshot(includeTown: false)), new RecordingStore());

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            shell.실행Async(BuildId, CancellationToken.None));

        Assert.Equal(SimulationWorld업무규칙집결JobShell.SpatialNodeNotFoundCode, exception.Message);
    }

    private static SimulationWorld공간실행Snapshot Snapshot(bool includeTown)
    {
        var nodes = new List<SimulationWorld파생Node>
        {
            Node("area:sim:pyeongchang:daegwallyeong-farm"),
            Node("area:sim:pyeongchang:jinbu-hub"),
        };
        if (includeTown) nodes.Add(Node("area:sim:pyeongchang:pyeongchang-town"));
        return new SimulationWorld공간실행Snapshot
        {
            BuildStableId = BuildId, AreaSetStableId = AreaSetId,
            OutputHashSha256 = Hash, Nodes = nodes,
        };
    }

    private static SimulationWorld파생Node Node(string id) => new()
    {
        StableId = id, NodeKindCode = "Area", DisplayName = id,
        EvidenceKindCode = SimulationWorld근거종류Codes.시나리오,
        SourceStableId = "scenario:test",
    };

    private sealed class FixedReader(SimulationWorld공간실행Snapshot snapshot)
        : ISimulationWorld공간실행Reader
    {
        public Task<SimulationWorld공간실행Snapshot?> 조회Async(
            string buildStableId, CancellationToken cancellationToken) =>
            Task.FromResult<SimulationWorld공간실행Snapshot?>(
                buildStableId == snapshot.BuildStableId ? snapshot : null);
    }

    private sealed class RecordingStore : ISimulationWorld업무규칙집결Store
    {
        public SimulationWorld업무규칙집결원장? Catalog { get; private set; }

        public Task<SimulationWorld업무규칙집결저장결과> 저장Async(
            SimulationWorld업무규칙집결원장 catalog,
            CancellationToken cancellationToken)
        {
            Catalog = catalog;
            return Task.FromResult(new SimulationWorld업무규칙집결저장결과
            {
                Inserted = true, CatalogRevision = catalog.CatalogRevision,
                CatalogHashSha256 = SimulationWorld업무규칙집결Validator.ComputeHash(catalog),
                FacilityCount = catalog.Facilities.Count, CapabilityCount = catalog.Capabilities.Count,
                RuleCount = catalog.Rules.Count, BindingCount = catalog.Bindings.Count,
                ScenarioRuleSetCount = catalog.ScenarioRuleSets.Count,
            });
        }
    }
}
