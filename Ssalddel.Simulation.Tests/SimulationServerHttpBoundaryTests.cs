using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationServerHttpBoundaryTests
{
    [Fact]
    public async Task 세션_API_경로와_HTTP방식은_호환기준을_유지한다()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();
        using var health = await client.GetAsync("/health");
        health.EnsureSuccessStatusCode();

        var endpoints = factory.Services.GetRequiredService<EndpointDataSource>();
        var manifest = endpoints.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText?.StartsWith(
                "api/simulation/v1/sessions",
                StringComparison.Ordinal) == true)
            .SelectMany(endpoint => endpoint.Metadata
                .GetMetadata<IHttpMethodMetadata>()?
                .HttpMethods
                .Select(method => $"{method} {endpoint.RoutePattern.RawText}")
                ?? Array.Empty<string>())
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();
        var hash = Convert.ToHexString(SHA256.HashData(
                Encoding.UTF8.GetBytes(string.Join("\n", manifest))))
            .ToLowerInvariant();
        Assert.Equal(155, manifest.Length);
        Assert.Equal(
            "827a8edc77ef5e908dc9daa75f26af99a4e9f2de7f2ef41f01c9d547010a8f5c",
            hash);
    }

    [Fact]
    public async Task API가_비활성화되어도_상태확인은_가능하다()
    {
        using var factory = CreateFactory(enabled: false);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task 운영서버와_같은_상태확인_경로를_제공한다(string path)
    {
        using var factory = CreateFactory(enabled: false);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(path);

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
    public async Task 분리된_턴Controller도_공통예외Filter로_404를_반환한다()
    {
        using var factory = CreateFactory(enabled: true);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(
            "/api/simulation/v1/sessions/simulation-session:missing/turn-closing-context");
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
                            ["SimulationSharedPublicData:Enabled"] = "false",
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
