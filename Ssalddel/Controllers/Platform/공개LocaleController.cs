using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Services.Localization;

namespace Ssalddel.Controllers.Platform;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[AllowAnonymous]
[ApiController]
[Route("api/v1/public/localization")]
[SsalddelApiContractName("PublicLocaleController")]
public sealed class 공개LocaleController(
    IPublicLocaleRecommendationUseCase 공개Locale추천UseCase,
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

        return Ok(공개Locale추천UseCase.Recommend(
            Request.Headers.AcceptLanguage.ToString(),
            trustedCountryCode));
    }
}
