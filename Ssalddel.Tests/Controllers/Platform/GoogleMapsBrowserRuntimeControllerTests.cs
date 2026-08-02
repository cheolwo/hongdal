using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Ssalddel.Controllers.Platform;
using Ssalddel.Contracts.Common.Platform;

namespace Ssalddel.Tests.Controllers.Platform;

public sealed class GoogleMapsBrowserRuntimeControllerTests
{
    private const string BrowserKey = "AIzaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact]
    public void 개발Loopback요청은_전용BrowserKey를_NoStore응답으로제공한다()
    {
        var controller = CreateController(Environments.Development, IPAddress.Loopback, BrowserKey);

        var result = controller.Get();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<GoogleMapsBrowserRuntimeResponse>(ok.Value);
        Assert.Equal(BrowserKey, response.BrowserApiKey);
        Assert.Equal(["http://localhost:5238"], response.AllowedOrigins);
        Assert.Equal("no-store", controller.Response.Headers.CacheControl);
        Assert.Equal("http://localhost:5238", controller.Response.Headers.AccessControlAllowOrigin);
        Assert.Equal("Origin", controller.Response.Headers.Vary);
    }

    [Theory]
    [InlineData("Production", "127.0.0.1")]
    [InlineData("Development", "192.0.2.10")]
    public void 개발Loopback경계밖에서는_RuntimeKey를노출하지않는다(
        string environmentName,
        string remoteAddress)
    {
        var controller = CreateController(environmentName, IPAddress.Parse(remoteAddress), BrowserKey);

        var result = controller.Get();

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-browser-key")]
    public void 전용BrowserKey가없거나형식이다르면_설정없음으로처리한다(string? browserKey)
    {
        var controller = CreateController(Environments.Development, IPAddress.IPv6Loopback, browserKey);

        var result = controller.Get();

        Assert.IsType<NoContentResult>(result.Result);
    }

    private static GoogleMapsBrowserRuntimeController CreateController(
        string environmentName,
        IPAddress remoteAddress,
        string? browserKey)
    {
        var values = new Dictionary<string, string?>();
        if (browserKey is not null)
        {
            values["GoogleMaps:BrowserApiKey"] = browserKey;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = remoteAddress;
        context.Request.Headers.Origin = "http://localhost:5238";

        return new GoogleMapsBrowserRuntimeController(
            configuration,
            new TestHostEnvironment(environmentName))
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Ssalddel.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
