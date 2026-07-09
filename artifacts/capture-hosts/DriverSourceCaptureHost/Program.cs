using DriverApp.Services;
using DriverApp.Services.Samples;
using Hongdal.Ui.Common.Areas.App.Services;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddDriverAppServices(builder.Configuration);
builder.Services.AddSingleton<IDriverSampleDataService>(sp => sp.GetRequiredService<기사샘플데이터Service>());
builder.Services.AddSingleton<IDriverTransportCompletionPhotoService, SampleDriverTransportCompletionPhotoService>();
builder.Services.AddHongdalUiCommonAppServices();
builder.Services.AddHongdalDocumentOutputServices();

var app = builder.Build();
DriverAppServiceProvider.Initialize(app.Services);

var repositoryRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", ".."));
var appWebRoot = Path.Combine(repositoryRoot, "DriverApp", "wwwroot");
var commonAppStaticAssets = Path.Combine(repositoryRoot, "Hongdal.Ui.Common", "Areas", "App", "wwwroot");
var mudBlazorStaticAssets = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "mudblazor", "9.5.0", "staticwebassets");

app.UseStatusCodePagesWithReExecute("/not-found");
if (Directory.Exists(mudBlazorStaticAssets))
{
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(mudBlazorStaticAssets), RequestPath = "/_content/MudBlazor" });
}

if (Directory.Exists(commonAppStaticAssets))
{
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(commonAppStaticAssets), RequestPath = "/_content/Hongdal.Ui.Common/Areas/App" });
}

if (Directory.Exists(appWebRoot))
{
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(appWebRoot) });
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<DriverApp.App>()
    .AddInteractiveServerRenderMode();

app.Run();
