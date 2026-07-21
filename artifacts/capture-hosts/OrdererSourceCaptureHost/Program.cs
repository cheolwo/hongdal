using Ssalddel.Ui.Common.Areas.App.Services;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using OrdererApp.Services;
using OrdererApp.ViewModels;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddSsalddelUiCommonAppServices();
builder.Services.AddScoped<PlatformHomeModeStateService>();
builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("https://localhost:7117/") });
builder.Services.AddScoped<PlatformCommunityService>();
builder.Services.AddScoped<IGroupPurchaseShipmentTrackingService, CaptureGroupPurchaseService>();
builder.Services.AddScoped<I주문자앱인증Service, CaptureOrdererAuthenticationService>();
builder.Services.AddScoped<GroupPurchaseIntentPageViewModel>();

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

app.MapGet("/mobile-capture", () => Results.Content(
    """
    <!doctype html>
    <html lang="ko">
    <head>
        <meta charset="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <style>
            html, body { width: 390px; margin: 0; padding: 0; background: #fff; }
            iframe { display: block; width: 390px; min-height: 844px; border: 0; }
        </style>
    </head>
    <body>
        <iframe id="mobile-preview" src="/group-purchase" title="공동구매 모바일 검증"></iframe>
        <script>
            const preview = document.getElementById('mobile-preview');
            setInterval(() => {
                const documentRoot = preview.contentDocument;
                const drawer = documentRoot?.querySelector('.mud-drawer--open');
                const menuButton = documentRoot?.querySelector('.mud-appbar button');
                const previousAttempt = Number(preview.dataset.drawerAttempt ?? 0);
                if (drawer && menuButton && Date.now() - previousAttempt > 750) {
                    menuButton.click();
                    preview.dataset.drawerAttempt = String(Date.now());
                }

                const height = documentRoot?.documentElement?.scrollHeight;
                if (height) preview.style.height = `${height}px`;
            }, 200);
        </script>
    </body>
    </html>
    """,
    "text/html; charset=utf-8"));

app.MapStaticAssets();
app.MapRazorComponents<OrdererApp.App>()
    .AddInteractiveServerRenderMode();

app.Run();
