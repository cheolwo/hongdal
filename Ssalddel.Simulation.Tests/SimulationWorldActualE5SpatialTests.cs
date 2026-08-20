using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldActualE5SpatialTests
{
    [Fact]
    public void 이론공간을_네개의실제AreaSet과_하나의Network로읽는다()
    {
        var reader = Reader();

        Assert.True(reader.TryRead(out var catalog, out var errorCode), errorCode);
        Assert.Equal(PyeongchangAreaSetStableIds.ActualNetwork,
            catalog.Network.NetworkStableId);
        Assert.Equal(4, catalog.AreaSets.Count);
        Assert.Equal(17, catalog.Graphs.Count);
        Assert.Equal(8, catalog.Network.Relations.Length);
        Assert.Equal(3, catalog.Network.RouteGraphs.Length);
        Assert.All(catalog.AreaSets.Values, areaSet =>
        {
            Assert.Equal(catalog.Network.NetworkStableId,
                areaSet.CanonicalNetworkStableId);
            Assert.Equal(SimulationWorldLandscapeCompositionCodes.ScenarioLocalMeters,
                areaSet.CoordinateSpaceCode);
        });
        Assert.All(catalog.Graphs.Values, graph =>
        {
            Assert.Equal(SimulationWorldLandscapeCompositionCodes.Available,
                graph.StatusCode);
            Assert.Empty(graph.Unresolved);
            Assert.True(graph.PresentationOnly);
            Assert.False(graph.IsOperationalState);
        });
    }

    [Fact]
    public async Task 마흔한개WI를_직접30_문맥5_비공간6으로완전분류한다()
    {
        var service = new SimulationWorld상호작용NetworkService(Reader());

        var result = await service.EvaluateAsync(PyeongchangAreaSetStableIds.ActualNetwork);

        Assert.Equal(SimulationWorld상호작용Graph상태Codes.Ready,
            result.OverallStatusCode);
        Assert.Equal(41, result.TotalWorldInteractionCount);
        Assert.Equal(30, result.DirectBindings.Length);
        Assert.Equal(5, result.ContextualBindings.Length);
        Assert.Equal(6, result.NonSpatialBindings.Length);
        Assert.Equal(17, result.GraphAudits.Length);
        Assert.All(result.DirectBindings, item =>
        {
            Assert.Equal(SimulationWorld상호작용Graph상태Codes.Ready,
                item.StatusCode);
            Assert.True(item.SpatialClosedLoop);
            Assert.NotNull(item.SpatialDefinition);
            Assert.StartsWith("h1-", item.H1Ref);
            Assert.StartsWith("h2-", item.H2Ref);
            Assert.StartsWith("h3-", item.H3Ref);
        });
        Assert.All(result.ContextualBindings, item => Assert.Equal(
            SimulationWorld상호작용Graph상태Codes.ContextBound, item.StatusCode));
        Assert.All(result.NonSpatialBindings, item => Assert.Equal(
            SimulationWorld상호작용Graph상태Codes.NotSpatiallyApplicable,
            item.StatusCode));
        Assert.DoesNotContain(result.Transitions, item =>
            item.StatusCode == SimulationWorld상호작용Graph상태Codes.PathUnresolved);
    }

    [Fact]
    public async Task 실제E5_AreaSet은_시나리오지역Graph인덱스를제공한다()
    {
        var service = new SimulationWorldActualE5SpatialService(Reader());

        var index = await service.ReadGraphIndexAsync(
            PyeongchangAreaSetStableIds.FarmAreaSet, null, 4);

        Assert.NotNull(index);
        Assert.Equal(4, index!.Graphs.Length);
        Assert.All(index.CoveredTileKeys,
            item => Assert.StartsWith("scenario-local:", item));
    }

    [Fact]
    public async Task WorldStream_API에서_Network와마흔한개WI준비도를조회한다()
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var escaped = Uri.EscapeDataString(PyeongchangAreaSetStableIds.ActualNetwork);

        var networkResponse = await client.GetAsync(
            "/api/simulation/v1/world-stream/area-set-networks/" + escaped);
        var readinessResponse = await client.GetAsync(
            "/api/simulation/v1/world-stream/area-set-networks/" + escaped
            + "/interaction-readiness");

        Assert.Equal(HttpStatusCode.OK, networkResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readinessResponse.StatusCode);
        var network = await networkResponse.Content
            .ReadFromJsonAsync<SimulationWorldAreaSetNetworkResponse>();
        var readiness = await readinessResponse.Content
            .ReadFromJsonAsync<SimulationWorld상호작용Network준비도Response>();
        Assert.Equal(4, network!.AreaSets.Length);
        Assert.Equal(41, readiness!.TotalWorldInteractionCount);
        Assert.Equal(SimulationWorld상호작용Graph상태Codes.Ready,
            readiness.OverallStatusCode);
    }

    private static FileSimulationWorldActualE5SpatialCatalogReader Reader() =>
        new(CatalogPath());

    private static string CatalogPath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, "eng", "world-seedbeds",
                "generated", "actual-e5-spatial.v1.json");
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        throw new FileNotFoundException("actual-e5-spatial.v1.json");
    }
}
