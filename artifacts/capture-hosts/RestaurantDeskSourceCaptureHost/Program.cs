using Hongdal.Ui.Common.Areas.App.Services;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using RestaurantDeskApp.Options;
using RestaurantDeskApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<FoodApiOptions>(builder.Configuration.GetSection(FoodApiOptions.SectionName));
builder.Services.AddSingleton<RestaurantDeskSampleService>();
builder.Services.AddSingleton<I주문알림Service, 주문알림Service>();
builder.Services.AddHongdalUiCommonAppServices();
builder.Services.AddScoped<배차주소ApiService>();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("https://localhost:7117/") });
builder.Services.AddScoped<PlatformCommunityService>();
builder.Services.AddScoped<PlatformHomeModeStateService>();
builder.Services.AddMudServices();

var app = builder.Build();
var repositoryRoot = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", ".."));
var appWebRoot = Path.Combine(repositoryRoot, "RestaurantDeskApp", "wwwroot");
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

app.MapRazorComponents<RestaurantDeskApp.App>()
    .AddInteractiveServerRenderMode();

app.Run();
