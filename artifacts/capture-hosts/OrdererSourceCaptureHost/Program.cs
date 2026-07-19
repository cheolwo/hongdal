using Ssalddel.Ui.Common.Areas.App.Services;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using OrdererApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddSsalddelUiCommonAppServices();
builder.Services.AddScoped<PlatformHomeModeStateService>();
builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://localhost:7117/") });
builder.Services.AddScoped<PlatformCommunityService>();
builder.Services.AddSingleton<IRestaurantSearchPolicyService, HttpRestaurantSearchPolicyService>();
builder.Services.AddSingleton<IGroupPurchaseShipmentTrackingService, HttpGroupPurchaseShipmentTrackingService>();

var app = builder.Build();
var repositoryRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", ".."));
var appWebRoot = Path.Combine(repositoryRoot, "OrdererApp", "wwwroot");
var mudBlazorStaticAssets = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "mudblazor", "9.5.0", "staticwebassets");

app.UseStatusCodePagesWithReExecute("/not-found");
if (Directory.Exists(mudBlazorStaticAssets))
{
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(mudBlazorStaticAssets), RequestPath = "/_content/MudBlazor" });
}

if (Directory.Exists(appWebRoot))
{
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(appWebRoot) });
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<OrdererApp.App>()
    .AddInteractiveServerRenderMode();

app.Run();
