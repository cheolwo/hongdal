using Hongdal.Ui.Common.Areas.App.Services;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using ShipperApp.Services;
using ShipperApp.Services.Samples;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddShipperAppServices(builder.Configuration);
builder.Services.AddScoped<IShipperOperationsService, SampleShipperOperationsService>();
builder.Services.AddHongdalUiCommonAppServices();
builder.Services.AddHongdalDocumentOutputServices();
builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7117/"),
    Timeout = TimeSpan.FromSeconds(2)
});
builder.Services.AddScoped<PlatformCommunityService>();
builder.Services.AddScoped<PlatformHomeModeStateService>();

var app = builder.Build();
var repositoryRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", ".."));
var appWebRoot = Path.Combine(repositoryRoot, "ShipperApp", "wwwroot");
var mudBlazorStaticAssets = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".nuget",
    "packages",
    "mudblazor",
    "9.5.0",
    "staticwebassets");
var commonAppStaticAssets = Path.Combine(repositoryRoot, "Hongdal.Ui.Common", "Areas", "App", "wwwroot");

app.UseStatusCodePagesWithReExecute("/not-found");
if (Directory.Exists(mudBlazorStaticAssets))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(mudBlazorStaticAssets),
        RequestPath = "/_content/MudBlazor"
    });
}

if (Directory.Exists(commonAppStaticAssets))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(commonAppStaticAssets),
        RequestPath = "/_content/Hongdal.Ui.Common/Areas/App"
    });
}

if (Directory.Exists(appWebRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(appWebRoot)
    });
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<ShipperApp.App>()
    .AddInteractiveServerRenderMode();

app.Run();
