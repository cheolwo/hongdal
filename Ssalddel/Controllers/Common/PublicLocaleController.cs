using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Services.Localization;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[AllowAnonymous]
[ApiController]
[Route("api/v1/public/localization")]
public sealed class PublicLocaleController(
    IPublicLocaleRecommendationUseCase useCase,
    IConfiguration configuration) : ControllerBase
{
    private const string DefaultCountryHeaderName = "X-Ssalddel-Country-Code";

    [HttpGet("recommendation")]
    public IActionResult Recommendation()
    {
        var trustCountryHeader = configuration.GetValue<bool>(
            "WebLocalization:TrustProxyCountryHeader");
        var countryHeaderName = configuration["WebLocalization:CountryHeaderName"]
                                ?? DefaultCountryHeaderName;
        var trustedCountryCode = trustCountryHeader
            ? Request.Headers[countryHeaderName].FirstOrDefault()
            : null;

        return Ok(useCase.Recommend(
            Request.Headers.AcceptLanguage.ToString(),
            trustedCountryCode));
    }
}
