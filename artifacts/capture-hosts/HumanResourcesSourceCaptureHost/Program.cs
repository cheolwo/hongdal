using Hongdal.Ui.Common.Areas.App.Services;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddHongdalUiCommonAppServices();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://localhost:7117/") });
builder.Services.AddScoped<PlatformCommunityService>();
builder.Services.AddScoped<PlatformHomeModeStateService>();

var app = builder.Build();
var repositoryRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", ".."));
var appWebRoot = Path.Combine(repositoryRoot, "HumanResourcesManagerApp", "wwwroot");
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

app.MapRazorComponents<HumanResourcesManagerApp.App>()
    .AddInteractiveServerRenderMode();

app.Run();
