using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Platform;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Controllers.Platform;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[ApiController]
[Route(GoogleMapsBrowserRuntimeRoutes.LocalDevelopment)]
public sealed class GoogleMapsBrowserRuntimeController(
    IConfiguration configuration,
    IHostEnvironment environment) : ControllerBase
{
    private const string BrowserApiKeyConfigurationName = "GoogleMaps:BrowserApiKey";

    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public ActionResult<GoogleMapsBrowserRuntimeResponse> Get()
    {
        if (!environment.IsDevelopment()
            || !IsLoopback(Request.HttpContext.Connection.RemoteIpAddress)
            || !TryGetLoopbackOrigin(Request.Headers.Origin, out var origin))
        {
            return NotFound();
        }

        var browserApiKey = configuration[BrowserApiKeyConfigurationName]?.Trim();
        if (string.IsNullOrWhiteSpace(browserApiKey)
            || !Regex.IsMatch(browserApiKey, "^AIza[0-9A-Za-z_-]{35}$", RegexOptions.CultureInvariant))
        {
            return NoContent();
        }

        Response.Headers.CacheControl = "no-store";
        Response.Headers.Pragma = "no-cache";
        Response.Headers.Vary = "Origin";
        Response.Headers.AccessControlAllowOrigin = origin;

        return Ok(new GoogleMapsBrowserRuntimeResponse
        {
            BrowserApiKey = browserApiKey,
            AllowedOrigins = [origin]
        });
    }

    private static bool IsLoopback(IPAddress? address)
        => address is not null && IPAddress.IsLoopback(address);

    private static bool TryGetLoopbackOrigin(string? value, out string origin)
    {
        origin = string.Empty;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || !uri.IsLoopback
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || uri.AbsolutePath != "/"
            || !string.IsNullOrEmpty(uri.Query)
            || !string.IsNullOrEmpty(uri.Fragment)
            || !string.IsNullOrEmpty(uri.UserInfo))
        {
            return false;
        }

        origin = uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        return true;
    }
}
