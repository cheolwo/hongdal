using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldLayoutTests
{
    private const string LayoutId = "world-layout:sim:pyeongchang:nature-farm-hub-town.v1";

    [Fact]
    public void H5는_네지역과_세물리회랑을_부모좌표계로읽는다()
    {
        Assert.True(Reader().TryRead(out var catalog, out var errorCode), errorCode);

        Assert.Equal(LayoutId, catalog.Definition.WorldLayoutStableId);
        Assert.Equal(4, catalog.Definition.AreaSetInstances.Length);
        Assert.Equal(3, catalog.Definition.CorridorInstances.Length);
        Assert.All(catalog.Definition.AreaSetInstances, area =>
        {
            Assert.Equal(SimulationWorldLayoutCodes.ScenarioLocalMeters,
                area.PlacementTransform.CoordinateSpaceCode);
            Assert.All(area.GraphInstances, graph => Assert.Equal(
                SimulationWorldLayoutCodes.ParentLocalMeters,
                graph.PlacementTransform.CoordinateSpaceCode));
        });
        Assert.Equal(3, catalog.Definition.Relations.Count(item =>
            item.SpatialRealizationCode == SimulationWorldLayoutCodes.PhysicalCorridor));
        Assert.Equal(5, catalog.Definition.Relations.Count(item =>
            item.SpatialRealizationCode == SimulationWorldLayoutCodes.AbstractTravel));
    }

    [Fact]
    public void 선택형E6는_H5해시를바꾸지않고_권위적용과준비도를분리한다()
    {
        Assert.True(Reader().TryRead(out var catalog, out var errorCode), errorCode);

        Assert.Equal(SimulationWorldLayoutCodes.Optional,
            catalog.Definition.WorldGroundingPolicyCode);
        Assert.Equal(catalog.Definition.WorldLayoutHashSha256,
            catalog.GroundingBinding.WorldLayoutHashSha256);
        Assert.Equal(SimulationWorldLayoutCodes.ScenarioRelative,
            catalog.GroundingBinding.PlacementAuthorityCode);
        Assert.Equal(SimulationWorldLayoutCodes.NotApplied,
            catalog.GroundingBinding.WorldGroundingStateCode);
        Assert.Empty(catalog.GroundingBinding.E6AnchorStableId);
        Assert.Equal(SimulationWorldLayoutCodes.Partial,
            catalog.GroundingReadiness.GroundingReadinessStateCode);
        Assert.False(catalog.GroundingReadiness.AppliesAuthority);
    }

    [Fact]
    public void LH는_E6미적용이어도_H5권위셀내용을사용한다()
    {
        var source = new H5AuthoritativeSimulationLhCellContentSource(Reader());

        var cell = source.CreateCellPlan(new SimulationLhWindowCell
        {
            CellKey = SimulationLhWorldService.L3CellKey(
                SimulationLhWorldService.CenterL3X, SimulationLhWorldService.CenterL3Y),
            CellX = SimulationLhWorldService.CenterL3X,
            CellY = SimulationLhWorldService.CenterL3Y,
            WindowRoleCode = SimulationLhWorldCodes.Detail,
        }, new SimulationLhCellContentContext
        {
            Season = SimulationLhWorldService.CreateSeason(1),
        });

        Assert.Equal(SimulationLhWorldCodes.AuthoritativeWorld, source.ContentSourceCode);
        Assert.Equal(SimulationLhWorldCodes.AuthoritativeWorld, cell.ContentSourceCode);
        Assert.Contains(cell.HBindings, item => item.HLevelCode == "H4");
        Assert.Contains(cell.HBindings, item => item.HLevelCode == "H3");
        Assert.All(cell.Placements, item => Assert.Equal(1d, item.UniformScale));
    }

    [Fact]
    public void H5대장이없으면_LH가절차생성으로조용히후퇴하지않는다()
    {
        var missing = new FileSimulationWorldLayoutCatalogReader(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json"));

        var error = Assert.Throws<InvalidOperationException>(() =>
            new H5AuthoritativeSimulationLhCellContentSource(missing));

        Assert.Equal("WorldLayoutCatalogUnavailable", error.Message);
    }

    [Fact]
    public async Task WorldStream_API가_H5정의와_E6결속_준비도를분리해제공한다()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var escaped = Uri.EscapeDataString(LayoutId);

        var definitionResponse = await client.GetAsync(
            "/api/simulation/v1/world-stream/world-layouts/" + escaped);
        var bindingResponse = await client.GetAsync(
            "/api/simulation/v1/world-stream/world-layouts/" + escaped + "/grounding-binding");
        var readinessResponse = await client.GetAsync(
            "/api/simulation/v1/world-stream/world-layouts/" + escaped + "/grounding-readiness");

        Assert.Equal(HttpStatusCode.OK, definitionResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, bindingResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readinessResponse.StatusCode);
        Assert.Equal(4, (await definitionResponse.Content
            .ReadFromJsonAsync<SimulationWorldLayoutDefinitionResponse>())!.AreaSetInstances.Length);
        Assert.Equal(SimulationWorldLayoutCodes.NotApplied, (await bindingResponse.Content
            .ReadFromJsonAsync<SimulationWorldGroundingBindingResponse>())!.WorldGroundingStateCode);
        Assert.Equal(SimulationWorldLayoutCodes.Partial, (await readinessResponse.Content
            .ReadFromJsonAsync<SimulationWorldGroundingReadinessResponse>())!.GroundingReadinessStateCode);
    }

    private static FileSimulationWorldLayoutCatalogReader Reader() => new(CatalogPath());

    private static string CatalogPath()
    {
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current != null; current = current.Parent)
        {
            var candidate = Path.Combine(current.FullName, "eng", "world-seedbeds", "generated", "h5-world-layout.v1.json");
            if (File.Exists(candidate)) return candidate;
        }
        throw new FileNotFoundException("h5-world-layout.v1.json");
    }
}
