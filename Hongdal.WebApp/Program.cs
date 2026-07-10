using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Hongdal.WebApp;
using Hongdal.WebApp.Models;
using Hongdal.WebApp.Services;
using Hongdal.Ui.Common.Areas.App.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.Configuration["HongdalApiBaseAddress"];
if (string.IsNullOrWhiteSpace(apiBaseAddress))
{
    apiBaseAddress = builder.HostEnvironment.BaseAddress;
}

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
builder.Services.AddHongdalUiCommonAppServices();
builder.Services.AddHongdalDocumentOutputServices();
builder.Services.AddScoped<PlatformCommunityService>();
builder.Services.AddScoped<PlatformHomeModeStateService>();
builder.Services.AddScoped<WebAuthSessionService>();
builder.Services.AddScoped<운송의뢰자동저장Service>();
builder.Services.AddScoped<화주운송의뢰등록Service>();
builder.Services.AddScoped<운송의뢰작성ViewModel>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
