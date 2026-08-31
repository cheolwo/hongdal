using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Controllers.Admin.Content;
using Ssalddel.Infrastructure.Persistence.WorldProjection;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class 개체시각대응HttpTests
{
    [Theory]
    [InlineData("GET", null, 401)]
    [InlineData("POST", null, 401)]
    [InlineData("GET", "member", 403)]
    [InlineData("POST", "member", 403)]
    [InlineData("GET", "admin", 200)]
    [InlineData("POST", "admin", 200)]
    [InlineData("GET_ASSETS", null, 401)]
    [InlineData("POST_ASSETS", null, 401)]
    [InlineData("GET_ASSETS", "member", 403)]
    [InlineData("POST_ASSETS", "member", 403)]
    [InlineData("GET_ASSETS", "admin", 200)]
    [InlineData("POST_ASSETS", "admin", 400)]
    [InlineData("GET_COMPOSITIONS", null, 401)]
    [InlineData("POST_COMPOSITIONS", null, 401)]
    [InlineData("GET_COMPOSITIONS", "member", 403)]
    [InlineData("POST_COMPOSITIONS", "member", 403)]
    [InlineData("GET_COMPOSITIONS", "admin", 200)]
    [InlineData("POST_COMPOSITIONS", "admin", 200)]
    [InlineData("GET_INVENTORY", null, 401)]
    [InlineData("POST_INVENTORY", null, 401)]
    [InlineData("GET_INVENTORY", "member", 403)]
    [InlineData("POST_INVENTORY", "member", 403)]
    [InlineData("GET_INVENTORY", "admin", 200)]
    [InlineData("POST_INVENTORY", "admin", 400)]
    public async Task 실제HTTP미들웨어_조회와저장에관리자정책을적용한다(string method, string? actor, int expected)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions { EnvironmentName = "Testing" });
        builder.WebHost.UseKestrel(o => o.Listen(IPAddress.Loopback, 0));
        builder.Services.AddLogging();
        builder.Services.AddRouting();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        builder.Services.AddSingleton<IOptionsMonitor<개체시각자산Options>>(new 개체시각대응Tests.Monitor());
        builder.Services.AddSingleton<I개체시각자산Catalog, 개체시각자산Catalog>();
        builder.Services.AddSingleton<I개체시각대상Reader, Source>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddDbContext<개체시각대응DbContext>(o => o.UseSqlite(connection));
        builder.Services.AddScoped<개체시각대응UseCase>();
        builder.Services.AddScoped<개체시각목록UseCase>();
        builder.Services.AddScoped<게임객체시각구성UseCase>();
        builder.Services.AddScoped<게임객체WI참여UseCase>();
        builder.Services.AddScoped<보유시각자산목록UseCase>();
        builder.Services.AddControllers().ConfigureApplicationPartManager(m =>
        {
            m.ApplicationParts.Clear(); m.FeatureProviders.Clear(); m.FeatureProviders.Add(new OnlyController());
        });
        builder.Services.AddAuthentication("Fixture").AddScheme<AuthenticationSchemeOptions, FixtureAuthentication>("Fixture", _ => { });
        builder.Services.AddAuthorization(o => o.AddPolicy(개체시각대응Codes.Policy, p => p.RequireRole(살뜰.Data.역할명.서버관리자)));
        await using var app = builder.Build();
        app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
        using (var scope = app.Services.CreateScope()) await scope.ServiceProvider.GetRequiredService<개체시각대응DbContext>().Database.EnsureCreatedAsync();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        await app.StartAsync(deadline.Token);
        try
        {
            var address = Assert.Single(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses);
            using var handler = new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false };
            using var client = new HttpClient(handler) { BaseAddress = new Uri(address) };
            var url = "/" + 개체시각대응Codes.Route + (method == "GET" ? "?Kind=food.product&StableId=product%3Apotato&Purpose=Inventory" : "");
            if (method == "GET_ASSETS") url = "/" + 개체시각대응Codes.Route + "/assets?skip=0";
            if (method == "POST_ASSETS") url = "/" + 개체시각대응Codes.Route + "/assets/import";
            if (method.EndsWith("_COMPOSITIONS")) url = "/" + 개체시각대응Codes.Route + "/compositions";
            if (method == "GET_INVENTORY") url = "/" + 개체시각대응Codes.Route + "/inventory";
            if (method == "POST_INVENTORY") url = "/" + 개체시각대응Codes.Route + "/inventory/import";
            using var request = new HttpRequestMessage(new HttpMethod(method.Split('_')[0]), url);
            if (actor is not null) request.Headers.Add("X-Fixture-Actor", actor);
            if (method == "POST") request.Content = JsonContent.Create(new 개체시각대응Request("http:1", 0, "request:1",
                개체시각대응Action.SaveDraft, "Fixture", 개체시각대응Tests.Query, false));
            if (method == "POST_ASSETS") request.Content = JsonContent.Create(Array.Empty<개체시각자산입력>());
            if (method == "POST_COMPOSITIONS") request.Content = JsonContent.Create(게임객체시각구성Tests.Request());
            if (method == "POST_INVENTORY") request.Content = JsonContent.Create(new 보유시각자산반입Request("docs/test.md", new('A',64), []));
            using var response = await client.SendAsync(request, deadline.Token);
            Assert.Equal(expected, (int)response.StatusCode);
            using var verify = app.Services.CreateScope();
            Assert.Equal(method == "POST" && actor == "admin" ? 1 : 0,
                await verify.ServiceProvider.GetRequiredService<개체시각대응DbContext>().Bindings.CountAsync());
        }
        finally { await app.StopAsync(CancellationToken.None); }
    }
    private sealed class OnlyController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            => feature.Controllers.Add(typeof(개체시각대응Controller).GetTypeInfo());
    }
    private sealed class Source : I개체시각대상Reader
    {
        public Task<개체시각대상ReadResult> ReadAsync(개체시각대상Query query, CancellationToken ct)
            => Task.FromResult(new 개체시각대상ReadResult("Found", 개체시각대응Tests.Target));
    }
    private sealed class FixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var actor = Request.Headers["X-Fixture-Actor"].ToString();
            if (string.IsNullOrEmpty(actor)) return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, actor),
                new Claim(ClaimTypes.Role, actor == "admin" ? 살뜰.Data.역할명.서버관리자 : "User")], Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new(identity), Scheme.Name)));
        }
    }
}
