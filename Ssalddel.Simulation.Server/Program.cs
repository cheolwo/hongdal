using Microsoft.Extensions.Options;
using Ssalddel.Simulation.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddSimulationServerServices(builder.Configuration);
SimulationServerServiceCollectionExtensions.RequireSimulationExecutionMode(
    builder.Configuration);

var app = builder.Build();
var simulationOptions = app.Services
    .GetRequiredService<IOptions<SimulationServerOptions>>()
    .Value;

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
