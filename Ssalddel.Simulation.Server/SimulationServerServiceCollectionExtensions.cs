using Microsoft.Extensions.Options;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Infrastructure;
using Ssalddel.Simulation.Persistence;

namespace Ssalddel.Simulation.Server;

public static class SimulationServerServiceCollectionExtensions
{
    public const string ConnectionStringMissingErrorCode =
        "SimulationSharedPublicDataConnectionStringMissing";

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

        services.AddSingleton<I경영SimulationSessionStore, InMemory경영SimulationSessionStore>();
        services.AddSingleton<ISimulationSessionSaveStore, InMemorySimulationSessionSaveStore>();
        services.AddSingleton<경영SimulationSessionService>();
        services.AddSingleton<Simulation타로화물운송PreviewService>();
        services.AddSingleton<Simulation타로객체반응PreviewService>();
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
