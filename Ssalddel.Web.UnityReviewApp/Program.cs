using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Web.UnityReviewApp;
using Ssalddel.Web.UnityReviewApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

const string apiBaseAddressKey = "SsalddelApiBaseAddress";
var configuredBaseAddress = builder.Configuration[apiBaseAddressKey]
                            ?? "https://localhost:7117/";
if (!Uri.TryCreate(configuredBaseAddress, UriKind.Absolute, out var apiBaseAddress)
    || !(string.Equals(apiBaseAddress.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
         || string.Equals(apiBaseAddress.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
{
    throw new InvalidOperationException(
        $"{apiBaseAddressKey}는 절대 HTTP(S) 주소여야 합니다.");
}

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiBaseAddress });
builder.Services.AddSingleton<IClientSessionGuard, ClientSessionGuard>();
builder.Services.AddScoped<UnityReviewAuthSessionService>();
builder.Services.AddScoped<Synty공간조립오프라인검토Store>();
builder.Services.AddScoped<ISynty공간조립모바일검토Client, Synty공간조립모바일검토Client>();
builder.Services.AddScoped<Synty공간조립검토Workspace>();

await builder.Build().RunAsync();
