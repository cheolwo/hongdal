using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Services.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Admin.Master06;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/operations/third-party-logistics/outreach")]
[SsalddelApiContractName("ThirdPartyLogisticsProviderOutreachController")]
public sealed class 제3자물류사업자접촉Controller : ControllerBase
{
    private readonly IThirdPartyLogisticsProviderOutreachPreparationService _제3자물류사업자접촉Service;

    public 제3자물류사업자접촉Controller(
        IThirdPartyLogisticsProviderOutreachPreparationService 제3자물류사업자접촉Service)
    {
        _제3자물류사업자접촉Service = 제3자물류사업자접촉Service;
    }

    [HttpPost("preview")]
    [SsalddelApiContractName("Preview")]
    public ActionResult<ThirdPartyLogisticsProviderOutreachPreparationResponse>
        미리보기([FromBody] PrepareThirdPartyLogisticsProviderOutreachRequest request)
    {
        var response = _제3자물류사업자접촉Service.Prepare(request);
        if (response.Success)
        {
            return Ok(response);
        }

        return response.ErrorCode ==
               ThirdPartyLogisticsProviderOutreachErrorCodes
                   .MarketNotAvailableInDeployment
            ? NotFound(response)
            : BadRequest(response);
    }
}
