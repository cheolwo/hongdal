using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldStreamingTests
{
    [Fact]
    public void 대관령FarmRecipe는_자료조사기반_3x3상세_5x5활성_9x9준비규칙을_제공한다()
    {
        var first = new SimulationWorldStreamingService();
        var second = new SimulationWorldStreamingService();

        Assert.True(first.TryGetRecipe(SimulationWorldStreamCodes.PyeongchangFarmRecipe, out var a));
        Assert.True(second.TryGetRecipe(SimulationWorldStreamCodes.PyeongchangFarmRecipe, out var b));
        Assert.Equal(121, a.CoverageTileKeys.Length);
        Assert.Equal(1, a.DetailRadius);
        Assert.Equal(2, a.ActiveRadius);
        Assert.Equal(4, a.PrefetchRadius);
        Assert.Equal(4, a.MaxConcurrentTileLoads);
        Assert.Equal(0.25d, a.BoundaryPrefetchFraction);
        Assert.Equal(64, a.RecipeHashSha256.Length);
        Assert.Equal(a.RecipeHashSha256, b.RecipeHashSha256);
        Assert.False(a.IsOperationalState);
    }

    [Fact]
    public void 공간산출물이없으면_주소나해시를꾸며내지않고_자료대기로남긴다()
    {
        var service = new SimulationWorldStreamingService();
        var key = SimulationWorldStreamingService.TileKey(
            SimulationWorldStreamingService.CenterX,
            SimulationWorldStreamingService.CenterY);

        Assert.True(service.TryGetManifest(key, out var manifest));
        Assert.All(manifest.Layers, layer =>
        {
            Assert.Equal(SimulationWorldStreamCodes.WaitingForSpatialArtifact, layer.StatusCode);
            Assert.Null(layer.ArtifactRelativePath);
            Assert.Null(layer.ArtifactHashSha256);
        });
        Assert.Equal(60, manifest.HaloMeters);
    }

    [Fact]
    public void 범위밖타일은_계약에포함하지않는다()
    {
        var service = new SimulationWorldStreamingService();
        Assert.False(service.TryGetManifest("kr5186:l2:900:900", out _));
        Assert.False(service.TryGetActivities("kr5186:l2:900:900", out _));
        Assert.False(service.TryGetObjects("kr5186:l2:900:900", out _));
    }

    [Fact]
    public void Scenario건물은_결정적배치와_비권위근거를가진다()
    {
        var first = new SimulationWorldStreamingService();
        var second = new SimulationWorldStreamingService();
        var key = SimulationWorldStreamingService.TileKey(700, 1145);

        Assert.True(first.TryGetObjects(key, out var a));
        Assert.True(second.TryGetObjects(key, out var b));
        Assert.Equal(2, a.Objects.Length);
        Assert.Equal(a.PlacementHashSha256, b.PlacementHashSha256);
        Assert.All(a.Objects, item =>
        {
            Assert.Equal(SimulationWorldStreamCodes.Scenario, item.EvidenceKindCode);
            Assert.True(item.PresentationOnly);
            Assert.False(item.CollisionEligible);
            Assert.StartsWith("legal.", item.VisualKey);
        });
        Assert.True(a.PresentationOnly);
        Assert.False(a.IsOperationalState);
    }

    [Fact]
    public async Task 읽기API는_RecipeManifestArtifactActivity를_로그인없이제공한다()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var recipeId = Uri.EscapeDataString(SimulationWorldStreamCodes.PyeongchangFarmRecipe);
        var tileKey = Uri.EscapeDataString(SimulationWorldStreamingService.TileKey(700, 1145));

        var recipe = await client.GetFromJsonAsync<SimulationWorldStreamRecipeResponse>(
            "/api/simulation/v1/world-stream/recipes/" + recipeId);
        var manifest = await client.GetFromJsonAsync<SimulationWorldTileStreamManifestResponse>(
            "/api/simulation/v1/world-stream/tiles/" + tileKey + "/manifest");
        var artifact = await client.GetFromJsonAsync<SimulationWorldTileArtifactDescriptorResponse>(
            "/api/simulation/v1/world-stream/tiles/" + tileKey + "/artifacts/elevation");
        var activities = await client.GetFromJsonAsync<SimulationWorldTileActivityProjectionResponse>(
            "/api/simulation/v1/world-stream/tiles/" + tileKey + "/activities");
        var objects = await client.GetFromJsonAsync<SimulationWorldTileObjectProjectionResponse>(
            "/api/simulation/v1/world-stream/tiles/" + tileKey + "/objects");

        Assert.NotNull(recipe);
        Assert.NotNull(manifest);
        Assert.NotNull(artifact);
        Assert.NotNull(activities);
        Assert.NotNull(objects);
        Assert.Equal(SimulationWorldStreamCodes.WaitingForSpatialArtifact, artifact.StatusCode);
        Assert.True(activities.PresentationOnly);
        Assert.Equal(0, activities.WorldTick);
        Assert.Equal(2, objects.Objects.Length);
        Assert.True(objects.PresentationOnly);

        using var missing = await client.GetAsync(
            "/api/simulation/v1/world-stream/tiles/"
            + Uri.EscapeDataString("kr5186:l2:900:900") + "/manifest");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        using var regionWhileDatabaseDisabled = await client.GetAsync(
            "/api/simulation/v1/world-stream/regions/"
            + Uri.EscapeDataString("region:kr:administrative:5176038000"));
        Assert.Equal(HttpStatusCode.ServiceUnavailable, regionWhileDatabaseDisabled.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory()
        => new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["SsalddelExecution:Mode"] = "Simulation",
                        ["SimulationServer:Enabled"] = "true",
                        ["SimulationSharedPublicData:Enabled"] = "false",
                    });
                });
            });
}
