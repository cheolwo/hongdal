using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationServerHttpBoundaryTests
{
    [Fact]
    public async Task API가_비활성화되어도_상태확인은_가능하다()
    {
        using var factory = CreateFactory(enabled: false);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task API가_비활성화되면_Simulation경로를_공개하지_않는다()
    {
        using var factory = CreateFactory(enabled: false);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/simulation/v1/sessions/simulation-session:missing");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task 존재하지_않는_세션은_오류코드와_함께_404를_반환한다()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/simulation/v1/sessions/simulation-session:missing");
        var error = await response.Content.ReadFromJsonAsync<SimulationErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("SimulationSessionNotFound", error.ErrorCode);
    }

    [Fact]
    public async Task 세션생성은_201과_비운영_Simulation상태사본을_반환한다()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var request = CreateValidRequest();

        using var response = await client.PostAsJsonAsync(
            "/api/simulation/v1/sessions",
            request);
        var snapshot = await response.Content
            .ReadFromJsonAsync<경영SimulationSessionSnapshot>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(snapshot);
        Assert.Equal(
            "simulation-session:" + request.ClientRequestId.ToString("N"),
            snapshot.SessionStableId);
        Assert.Equal(SimulationModeCodes.Simulation, snapshot.ModeCode);
        Assert.False(snapshot.IsOperationalState);
        Assert.Equal(0, snapshot.Revision);
        Assert.Equal(0, snapshot.WorldContext.WorldRevision);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal(
            $"/api/simulation/v1/sessions/{snapshot.SessionStableId}",
            Uri.UnescapeDataString(response.Headers.Location.AbsolutePath));
    }

    [Fact]
    public async Task 잘못된_생성요청은_오류코드와_함께_400을_반환한다()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var request = CreateValidRequest();
        request.ClientRequestId = Guid.Empty;

        using var response = await client.PostAsJsonAsync(
            "/api/simulation/v1/sessions",
            request);
        var error = await response.Content.ReadFromJsonAsync<SimulationErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("SimulationClientRequestIdMissing", error.ErrorCode);
    }

    [Fact]
    public async Task 같은_요청식별자의_다른_내용은_오류코드와_함께_409를_반환한다()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        var request = CreateValidRequest();

        using var created = await client.PostAsJsonAsync(
            "/api/simulation/v1/sessions",
            request);
        created.EnsureSuccessStatusCode();

        request.ScenarioStableId = "scenario:http-boundary-conflict";
        using var response = await client.PostAsJsonAsync(
            "/api/simulation/v1/sessions",
            request);
        var error = await response.Content.ReadFromJsonAsync<SimulationErrorResponse>();

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("SimulationCreateRequestPayloadConflict", error.ErrorCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(bool enabled)
        => new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["SsalddelExecution:Mode"] = "Simulation",
                            ["SimulationServer:Enabled"] = enabled.ToString(),
                        });
                });
            });

    private static 경영SimulationSession생성Request CreateValidRequest()
        => new()
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:http-boundary",
            ScenarioDataRevision = "scenario-data-r1",
            ScenarioSeed = 1208,
            RuleRevision = "simulation-rule-r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:test",
                TerritoryStableId = "territory:test",
                SettlementStableId = "settlement:test",
                GameDateStartsOn = new DateTimeOffset(
                    2026,
                    8,
                    12,
                    0,
                    0,
                    0,
                    TimeSpan.Zero),
            },
        };
}
