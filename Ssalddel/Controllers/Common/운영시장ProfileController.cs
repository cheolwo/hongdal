using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Services.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[Route("api/v1/operations/market-profile")]
[SsalddelApiContractName("OperatingMarketProfileController")]
public sealed class 운영시장ProfileController : ControllerBase
{
    private readonly IOperatingMarketRuntimeProfileService _운영시장ProfileService;

    public 운영시장ProfileController(IOperatingMarketRuntimeProfileService 운영시장ProfileService)
    {
        _운영시장ProfileService = 운영시장ProfileService;
    }

    [HttpGet]
    [AllowAnonymous]
    [SsalddelApiContractName("Get")]
    public ActionResult<OperatingMarketRuntimeProfileResponse> 조회()
        => Ok(_운영시장ProfileService.GetCurrent());
}
