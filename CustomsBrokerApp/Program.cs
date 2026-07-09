using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CustomsBrokerApp;
using CustomsBrokerApp.Options;
using CustomsBrokerApp.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.Configure<CustomsBrokerApiOptions>(builder.Configuration.GetSection(CustomsBrokerApiOptions.SectionName));
builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiBaseUrl = configuration[$"{CustomsBrokerApiOptions.SectionName}:BaseUrl"] ??
                     builder.HostEnvironment.BaseAddress;

    return new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
});
builder.Services.AddScoped<CustomsBrokerAuthService>();
builder.Services.AddScoped<HsCodeCorrectionService>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
