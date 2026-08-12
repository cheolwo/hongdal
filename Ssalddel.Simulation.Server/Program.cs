using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Infrastructure;
using Ssalddel.Simulation.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.Configure<SimulationServerOptions>(
    builder.Configuration.GetSection(SimulationServerOptions.SectionName));
builder.Services.AddSingleton<I경영SimulationSessionStore, InMemory경영SimulationSessionStore>();
builder.Services.AddSingleton<ISimulationSessionSaveStore, InMemorySimulationSessionSaveStore>();
builder.Services.AddSingleton<경영SimulationSessionService>();

var executionMode = builder.Configuration["SsalddelExecution:Mode"];
if (!string.Equals(executionMode, "Simulation", StringComparison.Ordinal))
    throw new InvalidOperationException("Ssalddel.Simulation.Server requires SsalddelExecution:Mode=Simulation.");

var app = builder.Build();
var simulationOptions = app.Configuration
    .GetSection(SimulationServerOptions.SectionName)
    .Get<SimulationServerOptions>() ?? new SimulationServerOptions();

app.MapHealthChecks("/health");
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
