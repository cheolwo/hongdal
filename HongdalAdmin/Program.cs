using HongdalAdmin.Components;
using HongdalAdmin.Options;
using HongdalAdmin.Services;
using Hongdal.Ui.Common.Areas.App.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.Configure<관리자ApiOptions>(builder.Configuration.GetSection(관리자ApiOptions.SectionName));
builder.Services.Configure<FoodApiOptions>(builder.Configuration.GetSection(FoodApiOptions.SectionName));
builder.Services.AddScoped<관리자인증세션Service>();
builder.Services.AddScoped<ViewPolicyService>();
builder.Services.AddScoped<ActivityLogService>();
builder.Services.AddHttpClient<PlatformCommunityService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<IConfiguration>()["AdminApi:BaseUrl"] ?? "https://localhost:7117/";
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddScoped<PlatformHomeModeStateService>();
builder.Services.AddSingleton<백오피스메모리Service>();
builder.Services.AddSingleton<문서관리메모리Service>();
builder.Services.AddScoped<차량관리Service>();
builder.Services.AddScoped<음식운영Service>();
builder.Services.AddScoped<탐색캠페인샘플Service>();

builder.Services.AddHttpClient<관리자인증Service>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<관리자ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHttpClient<기사운행현황Service>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<관리자ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHttpClient<백오피스조회Service>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<관리자ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddScoped<I백오피스Service>(sp =>
{
    var useMemory = sp.GetRequiredService<IConfiguration>().GetValue("AdminData:UseMemory", true);
    return useMemory
        ? sp.GetRequiredService<백오피스메모리Service>()
        : sp.GetRequiredService<백오피스조회Service>();
});

builder.Services.AddHttpClient<차량관리Service>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<관리자ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHttpClient<HsCodeOperationsService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<관리자ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHttpClient<AuxiliaryFeatureSettingsService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<관리자ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHttpClient<음식운영Service>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FoodApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddHttpClient<RestaurantSearchPolicyAdminService>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<관리자ApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
