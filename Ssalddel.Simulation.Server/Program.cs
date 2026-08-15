using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Server;

var builder = WebApplication.CreateBuilder(args);

var requiresWorldDerivationDatabase = args.Any(argument =>
    string.Equals(argument, "--migrate-simulation-world-database", StringComparison.OrdinalIgnoreCase)
    || string.Equals(argument, "--build-pyeongchang-world-derived", StringComparison.OrdinalIgnoreCase)
    || string.Equals(argument, "--build-pyeongchang-synty-landscape", StringComparison.OrdinalIgnoreCase)
    || string.Equals(argument, "--assemble-pyeongchang-world-business-rules", StringComparison.OrdinalIgnoreCase));
requiresWorldDerivationDatabase = requiresWorldDerivationDatabase || args.Any(argument =>
    string.Equals(argument, "--plan-pyeongchang-world-ui", StringComparison.OrdinalIgnoreCase));
if (requiresWorldDerivationDatabase)
    builder.Configuration["SimulationWorldDerivationDatabase:Enabled"] = "true";

if (args.Contains("--migrate-simulation-session-database",
        StringComparer.OrdinalIgnoreCase))
{
    builder.Configuration["SimulationSessionDatabase:Enabled"] = "true";
}

if (args.Contains("--build-pyeongchang-world-derived", StringComparer.OrdinalIgnoreCase))
    builder.Configuration["SimulationSharedPublicData:Enabled"] = "true";

builder.Services.AddControllers();
builder.Services.AddSimulationServerServices(builder.Configuration);
SimulationServerServiceCollectionExtensions.RequireSimulationExecutionMode(
    builder.Configuration);

var app = builder.Build();
var simulationOptions = app.Services
    .GetRequiredService<IOptions<SimulationServerOptions>>()
    .Value;

if (args.Contains("--migrate-simulation-session-database",
        StringComparer.OrdinalIgnoreCase))
{
    var factory = app.Services.GetRequiredService<IDbContextFactory<
        Ssalddel.Simulation.Persistence.SimulationSessionDbContext>>();
    await using var sessionDb = await factory.CreateDbContextAsync();
    await sessionDb.Database.MigrateAsync(CancellationToken.None);
    Console.WriteLine("SimulationSessionDBMigration완료");
    return;
}

if (args.Contains("--migrate-simulation-world-database", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var worldDb = scope.ServiceProvider.GetRequiredService<
        Ssalddel.Simulation.Persistence.SimulationWorld파생DbContext>();
    await worldDb.Database.MigrateAsync(CancellationToken.None);
    Console.WriteLine("SimulationWorld파생DBMigration완료");
    return;
}

if (args.Contains("--build-pyeongchang-world-derived", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var pipeline = scope.ServiceProvider.GetRequiredService<
        Ssalddel.Simulation.Persistence.평창군공간파생Pipeline>();
    var tileManifestArgument = args.FirstOrDefault(argument =>
        argument.StartsWith("--tile-manifest=", StringComparison.OrdinalIgnoreCase));
    var result = await pipeline.실행Async(
        tileManifestArgument?["--tile-manifest=".Length..],
        CancellationToken.None);
    Console.WriteLine(
        $"평창군공간파생완료:상태={result.상태코드};새실행본={result.새실행본저장여부};" +
        $"원본건축물={result.건축물수};대표건축물={result.대표건축물수};대표군={result.대표군수};" +
        $"표현제외건축물={result.표현제외건축물수};원본공개사업장={result.공개사업장수};" +
        $"대표공개사업장={result.대표공개사업장수};" +
        $"연결={result.사업장건물연결수};미배치건축물={result.미배치건축물수};" +
        $"Unity변환Profile={result.Unity공간변환Profile수};Unity타일Manifest={result.Unity타일Manifest수};" +
        $"Unity산출물={result.Unity산출물수};" +
        $"파생실행={result.파생실행고유식별자};출력SHA256={result.출력해시SHA256}");
    return;
}

if (args.Contains("--build-pyeongchang-synty-landscape", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var spatialBuildArgument = args.FirstOrDefault(argument =>
        argument.StartsWith("--spatial-build=", StringComparison.OrdinalIgnoreCase));
    if (spatialBuildArgument == null)
        throw new InvalidOperationException("--spatial-build is required.");
    var spatialBuildStableId = spatialBuildArgument["--spatial-build=".Length..];
    var spatialReader = scope.ServiceProvider.GetRequiredService<ISimulationWorld공간실행Reader>();
    var spatialBuild = await spatialReader.조회Async(spatialBuildStableId, CancellationToken.None)
        ?? throw new InvalidOperationException(
            SimulationWorldSynty경관JobShell.SpatialBuildNotFoundCode);
    var shell = scope.ServiceProvider.GetRequiredService<SimulationWorldSynty경관JobShell>();
    var request = new SimulationWorldSynty경관Job요청
    {
        JobStableId = "synty-job:pyeongchang:" + spatialBuild.OutputHashSha256[..16] + ":pc-high:v1",
        SpatialBuildStableId = spatialBuild.BuildStableId,
        SpatialOutputHashSha256 = spatialBuild.OutputHashSha256,
        AreaSetStableId = spatialBuild.AreaSetStableId,
        ScopeKindCode = SimulationWorldSynty범위Codes.영역묶음,
        ScopeStableId = spatialBuild.AreaSetStableId,
        LandscapeRuleRevision = "pyeongchang-synty-landscape.v1",
        VisualCatalogRevision = "synty-world-catalog.v1",
        UrpProfileCatalogRevision = "urp-world-profile.v1",
        Seed = 51760,
        TargetPlatformCode = SimulationWorldSynty대상플랫폼Codes.PC,
        QualityTierCode = "PC-High",
    };
    var result = await shell.실행Async(request, CancellationToken.None);
    Console.WriteLine(
        $"평창군Synty경관Job완료:상태={result.StatusCode};새실행본={result.Inserted};" +
        $"그래픽계획={result.GraphicsPlanCount};시각배치={result.VisualPlacementCount};" +
        $"배치거부={result.RejectionCount};시각실행={result.VisualBuildStableId};" +
        $"출력SHA256={result.OutputHashSha256}");
    return;
}

if (args.Contains("--interpret-pyeongchang-building-type-demo", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var spatialBuildArgument = args.FirstOrDefault(argument =>
        argument.StartsWith("--spatial-build=", StringComparison.OrdinalIgnoreCase));
    if (spatialBuildArgument == null)
        throw new InvalidOperationException("--spatial-build is required.");
    var pipeline = scope.ServiceProvider.GetRequiredService<SimulationWorld건물종류DemoPipeline>();
    var result = await pipeline.실행Async(
        spatialBuildArgument["--spatial-build=".Length..], CancellationToken.None);
    Console.WriteLine(
        $"평창군건물종류Demo완료:규칙대장신규={result.RuleCatalogInserted};" +
        $"해석실행신규={result.InterpretationInserted};건물종류={result.Presentations.Count};" +
        $"해석실행={result.InterpretationStableId};출력SHA256={result.OutputHashSha256}");
    foreach (var item in result.Presentations)
        Console.WriteLine(
            $"건물종류Demo:{item.BuildingCategoryCode};대표원본={item.RepresentedRecordCount};" +
            $"시험상태={item.FixtureSimulationStateCode};기본구성={item.DefaultCompositionKey};" +
            $"동적의도={item.DynamicIntentBundleKey}");
    return;
}

if (args.Contains("--assemble-pyeongchang-world-business-rules", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var spatialBuildArgument = args.FirstOrDefault(argument =>
        argument.StartsWith("--spatial-build=", StringComparison.OrdinalIgnoreCase));
    if (spatialBuildArgument == null)
        throw new InvalidOperationException("--spatial-build is required.");
    var shell = scope.ServiceProvider.GetRequiredService<SimulationWorld업무규칙집결JobShell>();
    var result = await shell.실행Async(
        spatialBuildArgument["--spatial-build=".Length..], CancellationToken.None);
    Console.WriteLine(
        $"평창군업무Simulation규칙집결완료:신규={result.Inserted};시설={result.FacilityCount};" +
        $"기능={result.CapabilityCount};규칙={result.RuleCount};연결={result.BindingCount};" +
        $"Scenario규칙묶음={result.ScenarioRuleSetCount};대장={result.CatalogRevision};" +
        $"SHA256={result.CatalogHashSha256}");
    return;
}

if (args.Contains("--plan-pyeongchang-world-ui", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var ruleCatalogArgument = args.FirstOrDefault(argument =>
        argument.StartsWith("--business-rule-catalog=", StringComparison.OrdinalIgnoreCase));
    if (ruleCatalogArgument == null)
        throw new InvalidOperationException("--business-rule-catalog is required.");
    var shell = scope.ServiceProvider.GetRequiredService<SimulationWorldUI기획JobShell>();
    var result = await shell.실행Async(
        ruleCatalogArgument["--business-rule-catalog=".Length..], CancellationToken.None);
    Console.WriteLine(
        $"평창군SimulationWorldUI기획완료:신규={result.Inserted};화면영역={result.SurfaceCount};" +
        $"정보항목={result.InformationItemCount};상태표현={result.StatePresentationCount};" +
        $"행동후보={result.ActionCandidateCount};규칙연결={result.RuleBindingCount};" +
        $"대장={result.CatalogRevision};SHA256={result.CatalogHashSha256}");
    return;
}

app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live")
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready")
}).AllowAnonymous();
if (simulationOptions.Enabled)
{
    app.MapControllers();
}
else
{
    app.Logger.LogWarning(
        "Simulation API is disabled. Set SimulationServer:Enabled=true only in an approved Simulation environment.");
}

app.Run();

public partial class Program;
