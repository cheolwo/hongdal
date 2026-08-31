using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ssalddel.ApiMetadata;
using Ssalddel.Controllers.Admin.Content07;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;
using 살뜰.Data;

namespace Ssalddel.Tests.Controllers;

public sealed class 농사로감자자료수집ControllerTests
{
    private const string Route = "api/v1/admin/content/nongsaro-potato";
    // 실제 제공처/키가 아닌 누출 거부 검사용 문자열이다.
    private const string SensitiveFixture = "synthetic-private-provider-detail-do-not-return";

    [Fact]
    public void 메타데이터는_기존관리자정책과_0점0경로이며_승인입력을받지않는다()
    {
        var type = typeof(농사로감자자료수집Controller);
        Assert.Equal("서버관리자전용", type.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(Route, type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal(SsalddelProductVersion.V0_0,
            type.GetCustomAttribute<SsalddelApiVersionAttribute>()?.Version);
        Assert.Null(type.GetCustomAttribute<AllowAnonymousAttribute>());
        var collect = type.GetMethod(nameof(농사로감자자료수집Controller.수집))!;
        var read = type.GetMethod(nameof(농사로감자자료수집Controller.최신승인자료조회))!;
        Assert.Equal("collections", collect.GetCustomAttribute<HttpPostAttribute>()?.Template);
        Assert.Equal("latest-approved", read.GetCustomAttribute<HttpGetAttribute>()?.Template);
        foreach (var method in new[] { collect, read })
        {
            Assert.Null(method.GetCustomAttribute<AllowAnonymousAttribute>());
            Assert.Equal(typeof(CancellationToken), Assert.Single(method.GetParameters()).ParameterType);
        }
    }

    [Fact]
    public async Task 직접호출_수집은_false와취소토큰을_한번전달하고_원자료를반환하지않는다()
    {
        var archive = Fixture(approved: false);
        var service = new FakeArchive { Collected = archive };
        using var cts = new CancellationTokenSource();

        var result = await new 농사로감자자료수집Controller(service).수집(cts.Token);

        var response = Assert.IsType<농사로감자자료상태Response>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(1, service.CollectCalls);
        Assert.False(service.LastApproval!.Value);
        Assert.Equal(cts.Token, service.LastToken);
        Assert.Equal(0, service.ReadCalls);
        Assert.Equal(archive.Id, response.ArchiveId);
        Assert.Equal(archive.Revision, response.Revision);
        Assert.Equal(archive.RetrievedAtUtc, response.RetrievedAtUtc);
        Assert.Equal(archive.ArchivedAtUtc, response.ArchivedAtUtc);
        Assert.False(response.ApprovedForSimulationContext);
        AssertSafeSummary(response);
    }

    [Fact]
    public async Task 직접호출_GET은_승인조회만하고_승인자료상태를반환한다()
    {
        var service = new FakeArchive { Latest = Fixture(approved: true) };
        using var cts = new CancellationTokenSource();

        var result = await new 농사로감자자료수집Controller(service).최신승인자료조회(cts.Token);

        var response = Assert.IsType<농사로감자자료상태Response>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.True(response.ApprovedForSimulationContext);
        Assert.Equal(1, service.ReadCalls);
        Assert.Equal(0, service.CollectCalls);
        Assert.Equal(cts.Token, service.LastToken);
        AssertSafeSummary(response);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task 직접호출_GET미확보와_보류자료는_404이며_수집으로대체하지않는다(bool held)
    {
        var service = new FakeArchive { Latest = held ? Fixture(approved: false) : null };

        var result = await new 농사로감자자료수집Controller(service)
            .최신승인자료조회(CancellationToken.None);

        var missing = Assert.IsType<NotFoundObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(missing.Value);
        Assert.Equal(404, problem.Status);
        Assert.Equal("ApprovedNongsaroPotatoProfileUnavailable", problem.Extensions["code"]);
        Assert.Equal(1, service.ReadCalls);
        Assert.Equal(0, service.CollectCalls);
        Assert.DoesNotContain(SensitiveFixture, JsonSerializer.Serialize(problem));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task 직접호출_이미취소된요청은_서비스를호출하지않는다(bool get)
    {
        var service = new FakeArchive();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await Invoke(new 농사로감자자료수집Controller(service), get, cts.Token));

        Assert.Equal(0, service.CollectCalls + service.ReadCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task 직접호출_처리중요청취소는_503이나성공으로숨기지않는다(bool get)
    {
        using var cts = new CancellationTokenSource();
        var service = new FakeArchive
        {
            BeforeCall = () => cts.Cancel(),
            Failure = new OperationCanceledException(SensitiveFixture, cts.Token)
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await Invoke(new 농사로감자자료수집Controller(service), get, cts.Token));

        Assert.Equal(1, service.CollectCalls + service.ReadCalls);
        Assert.Equal(cts.Token, service.LastToken);
    }

    [Theory]
    [InlineData(false, "http")]
    [InlineData(false, "validation")]
    [InlineData(false, "timeout")]
    [InlineData(false, "unexpected")]
    [InlineData(true, "http")]
    [InlineData(true, "validation")]
    [InlineData(true, "timeout")]
    [InlineData(true, "unexpected")]
    public async Task 직접호출_서비스실패는_고정503이며_비밀과성공을내보내지않고_재시도하지않는다(
        bool get, string failureKind)
    {
        Exception exception = failureKind switch
        {
            "http" => new HttpRequestException(SensitiveFixture),
            "validation" => new InvalidOperationException(SensitiveFixture),
            "timeout" => new TaskCanceledException(SensitiveFixture),
            _ => new Exception(SensitiveFixture)
        };
        var service = new FakeArchive { Failure = exception };

        var result = await Invoke(new 농사로감자자료수집Controller(service), get, CancellationToken.None);

        var response = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(503, response.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(response.Value);
        Assert.Equal(503, problem.Status);
        Assert.Equal(get ? "NongsaroPotatoReadUnavailable" : "NongsaroPotatoCollectionUnavailable",
            problem.Extensions["code"]);
        Assert.Null(problem.Instance);
        Assert.DoesNotContain(SensitiveFixture, JsonSerializer.Serialize(problem));
        Assert.Equal(1, service.CollectCalls + service.ReadCalls);
    }

    // 실제 HTTP 미들웨어를 통과하지만, 인증은 시험용이다. 운영 로그인/DI/DB 증거가 아니다.
    // TestServer 새 패키지 없이 루프백 Kestrel과 가짜 Archive만 사용한다.
    [Theory]
    [InlineData("GET", null, 401)]
    [InlineData("POST", null, 401)]
    [InlineData("GET", "member", 403)]
    [InlineData("POST", "member", 403)]
    [InlineData("GET", "admin", 200)]
    [InlineData("POST", "admin", 200)]
    public async Task 시험용HTTP_익명과비관리자는차단하고_관리자만서비스를호출한다(
        string method, string? actor, int expectedStatus)
    {
        var service = new FakeArchive { Latest = Fixture(approved: true) };
        var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing",
            ApplicationName = typeof(농사로감자자료수집Controller).Assembly.GetName().Name,
            ContentRootPath = AppContext.BaseDirectory
        });
        builder.WebHost.UseKestrel(options => options.Listen(IPAddress.Loopback, 0));
        builder.Services.AddLogging();
        builder.Services.AddRouting();
        builder.Services.AddSingleton<INongsaro감자ProfileArchiveService>(service);
        builder.Services.AddControllers().ConfigureApplicationPartManager(manager =>
        {
            manager.ApplicationParts.Clear();
            manager.FeatureProviders.Clear();
            manager.FeatureProviders.Add(new OnlyPotatoController());
        });
        builder.Services.AddAuthentication("Fixture")
            .AddScheme<AuthenticationSchemeOptions, FixtureAuthentication>("Fixture", _ => { });
        builder.Services.AddAuthorization(options => options.AddPolicy("서버관리자전용",
            policy => policy.RequireRole(역할명.서버관리자)));

        await using var app = builder.Build();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await app.StartAsync(deadline.Token);
        try
        {
            var addresses = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!;
            using var handler = new HttpClientHandler { AllowAutoRedirect = false, UseProxy = false };
            using var client = new HttpClient(handler) { BaseAddress = new Uri(Assert.Single(addresses.Addresses)) };
            var target = method == "GET" ? "latest-approved" : "collections?approveForSimulationContext=true";
            using var request = new HttpRequestMessage(new HttpMethod(method), $"/{Route}/{target}");
            if (actor is not null) request.Headers.Add("X-Fixture-Actor", actor);
            if (method == "POST")
                request.Content = new StringContent("{\"approveForSimulationContext\":true}", Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request, deadline.Token);
            Assert.Equal(expectedStatus, (int)response.StatusCode);
            var body = await response.Content.ReadAsStringAsync(deadline.Token);
            Assert.DoesNotContain(SensitiveFixture, body);
            if (expectedStatus == 200)
            {
                Assert.Equal(1, service.CollectCalls + service.ReadCalls);
                using var json = JsonDocument.Parse(body);
                Assert.Equal(5, json.RootElement.EnumerateObject().Count());
                if (method == "POST") Assert.False(service.LastApproval!.Value);
            }
            else
            {
                Assert.Equal(0, service.CollectCalls + service.ReadCalls);
            }
        }
        finally
        {
            using var stopDeadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await app.StopAsync(stopDeadline.Token);
        }
    }

    private static Task<ActionResult<농사로감자자료상태Response>> Invoke(
        농사로감자자료수집Controller controller, bool get, CancellationToken token)
        => get ? controller.최신승인자료조회(token) : controller.수집(token);

    private static void AssertSafeSummary(농사로감자자료상태Response response)
    {
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(response));
        Assert.Equal(new[] { "ApprovedForSimulationContext", "ArchiveId", "ArchivedAtUtc", "RetrievedAtUtc", "Revision" },
            json.RootElement.EnumerateObject().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
        Assert.DoesNotContain(SensitiveFixture, json.RootElement.GetRawText());
    }

    private static Nongsaro감자ProfileArchive Fixture(bool approved) => new()
    {
        Id = 31,
        Revision = 4,
        StableId = "crop-requirement-profile:nongsaro.potato.1",
        CanonicalProductStableId = "product:potato",
        WorkScheduleGroupCode = "210005",
        WorkScheduleContentNo = "30699",
        ProductRelationStatusCode = "Unlinked",
        ApprovedForSimulationContext = approved,
        RetrievedAtUtc = new DateTime(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc),
        ArchivedAtUtc = new DateTime(2026, 8, 31, 0, 1, 0, DateTimeKind.Utc),
        ProfileJson = SensitiveFixture,
        SourceSetHashSha256 = SensitiveFixture,
        DisasterPreventionHashSha256 = SensitiveFixture,
        ReviewStatusCode = SensitiveFixture
    };

    private sealed class FakeArchive : INongsaro감자ProfileArchiveService
    {
        public Nongsaro감자ProfileArchive Collected { get; init; } = Fixture(approved: false);
        public Nongsaro감자ProfileArchive? Latest { get; init; }
        public Exception? Failure { get; init; }
        public Action? BeforeCall { get; init; }
        public int CollectCalls { get; private set; }
        public int ReadCalls { get; private set; }
        public bool? LastApproval { get; private set; }
        public CancellationToken LastToken { get; private set; }

        public Task<Nongsaro감자ProfileArchive> CollectAndArchiveAsync(bool approveForSimulationContext,
            CancellationToken cancellationToken = default)
        {
            CollectCalls++;
            LastApproval = approveForSimulationContext;
            LastToken = cancellationToken;
            BeforeCall?.Invoke();
            return Failure is null ? Task.FromResult(Collected) : Task.FromException<Nongsaro감자ProfileArchive>(Failure);
        }

        public Task<Nongsaro감자ProfileArchive?> 최신자료승인조회Async(CancellationToken cancellationToken = default)
        {
            ReadCalls++;
            LastToken = cancellationToken;
            BeforeCall?.Invoke();
            return Failure is null ? Task.FromResult(Latest) : Task.FromException<Nongsaro감자ProfileArchive?>(Failure);
        }
    }

    private sealed class OnlyPotatoController : IApplicationFeatureProvider<ControllerFeature>
    {
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            => feature.Controllers.Add(typeof(농사로감자자료수집Controller).GetTypeInfo());
    }

    public sealed class FixtureAuthentication(
        IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var actor = Request.Headers["X-Fixture-Actor"].ToString();
            if (string.IsNullOrEmpty(actor)) return Task.FromResult(AuthenticateResult.NoResult());
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "fixture-user") };
            if (actor == "admin") claims.Add(new Claim(ClaimTypes.Role, 역할명.서버관리자));
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
    }
}
