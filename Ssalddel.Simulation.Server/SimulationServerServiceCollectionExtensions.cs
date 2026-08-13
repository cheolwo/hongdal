using Microsoft.Extensions.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Infrastructure;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Server;

public static class SimulationServerServiceCollectionExtensions
{
    public const string ConnectionStringMissingErrorCode =
        "SimulationSharedPublicDataConnectionStringMissing";
    public const string WorldDerivationConnectionStringMissingErrorCode =
        "SimulationWorldDerivationConnectionStringMissing";

    public static IServiceCollection AddSimulationServerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SimulationServerOptions>()
            .Bind(configuration.GetSection(SimulationServerOptions.SectionName))
            .ValidateOnStart();
        services.AddOptions<SimulationSharedPublicDataOptions>()
            .Bind(configuration.GetSection(SimulationSharedPublicDataOptions.SectionName))
            .Validate(options => !options.Enabled
                || !string.IsNullOrWhiteSpace(options.ConnectionStringName),
                "공유 공공데이터 연결 문자열 이름이 필요합니다.")
            .Validate(options => options.MaxItems is >= 1 and <= 200,
                "공유 공공데이터 최대 조회 건수는 1~200이어야 합니다.")
            .ValidateOnStart();
        services.AddOptions<SimulationWorldDerivationDatabaseOptions>()
            .Bind(configuration.GetSection(SimulationWorldDerivationDatabaseOptions.SectionName))
            .Validate(options => !options.Enabled
                || !string.IsNullOrWhiteSpace(options.ConnectionStringName),
                "Simulation World 파생 DB 연결 문자열 이름이 필요합니다.")
            .ValidateOnStart();

        var sharedOptions = configuration
            .GetSection(SimulationSharedPublicDataOptions.SectionName)
            .Get<SimulationSharedPublicDataOptions>() ?? new SimulationSharedPublicDataOptions();

        if (sharedOptions.Enabled)
        {
            var connectionString = ResolveConnectionString(configuration, sharedOptions);
            services.AddSimulationSharedPublicDataPersistence(
                connectionString,
                sharedOptions.MaxItems);
        }
        else
        {
            services.AddSingleton<ISimulation공유공공데이터조회Port,
                DisabledSimulation공유공공데이터Reader>();
        }

        var derivationOptions = configuration
            .GetSection(SimulationWorldDerivationDatabaseOptions.SectionName)
            .Get<SimulationWorldDerivationDatabaseOptions>()
            ?? new SimulationWorldDerivationDatabaseOptions();
        if (derivationOptions.Enabled)
        {
            var connectionString = configuration.GetConnectionString(
                derivationOptions.ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    WorldDerivationConnectionStringMissingErrorCode);
            services.AddSimulationWorldDerivationPersistence(connectionString);
            if (sharedOptions.Enabled)
                services.Add평창군공간파생Pipeline();
        }

        services.AddSingleton<I경영SimulationSessionStore, InMemory경영SimulationSessionStore>();
        services.AddSingleton<ISimulationSessionSaveStore, InMemorySimulationSessionSaveStore>();
        services.AddSingleton<경영SimulationSessionService>();
        services.AddSingleton<Simulation타로화물운송PreviewService>();
        services.AddSingleton<Simulation타로객체반응PreviewService>();
        services.AddSingleton<SimulationFreight렌더링의도Projector>();
        services.AddSingleton<Simulation렌더링의도합성Policy>();
        services.AddSingleton<Simulation기본Urp표현Catalog>();
        services.AddSingleton<SimulationRuntimeWorldPresentationService>();
        services.AddHealthChecks()
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: ["live"])
            .AddCheck<SimulationServerReadinessHealthCheck>(
                "simulation-persistence",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"],
                timeout: TimeSpan.FromSeconds(15));
        return services;
    }

    public static void RequireSimulationExecutionMode(IConfiguration configuration)
    {
        var executionMode = configuration["SsalddelExecution:Mode"];
        if (!string.Equals(executionMode, "Simulation", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Ssalddel.Simulation.Server requires SsalddelExecution:Mode=Simulation.");
        }
    }

    private static string ResolveConnectionString(
        IConfiguration configuration,
        SimulationSharedPublicDataOptions options)
    {
        var connectionString = configuration.GetConnectionString(options.ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString)
            && !string.IsNullOrWhiteSpace(options.FallbackConnectionStringName))
        {
            connectionString = configuration.GetConnectionString(
                options.FallbackConnectionStringName);
        }

        return !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new InvalidOperationException(ConnectionStringMissingErrorCode);
    }
}
