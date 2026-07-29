using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Services.AgriculturalFisheries.ImportReadiness;

namespace Ssalddel.Controllers.Common;

[ApiController]
[AllowAnonymous]
[SsalddelApiVersion(SsalddelProductVersion.V1_5)]
[Route("api/v1/agricultural-fisheries/packaging-fcl")]
[SsalddelApiContractName("AgriculturalFisheriesPackagingFclAnalysisController")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Api,
    "품목별 대표 포장과 FCL 적재 추정치를 근거 수준과 함께 공개한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(농수산물포장Fcl분석목록Response),
    FlowOrder = 40,
    Boundary = "정보 조회만 제공하며 계약, 발주, 운송 또는 선적 예약을 생성하지 않는다.")]
public sealed class 농수산물포장Fcl분석Controller : ControllerBase
{
    private readonly I농수산물포장Fcl분석Service _service;

    public 농수산물포장Fcl분석Controller(I농수산물포장Fcl분석Service service)
    {
        _service = service;
    }

    [HttpGet]
    [SsalddelApiContractName("ListPackagingFclEstimates")]
    public async Task<ActionResult<농수산물포장Fcl분석목록Response>> 목록조회(
        [FromQuery] int? sourceYear,
        [FromQuery] string? itemCode,
        [FromQuery] string? categoryCode,
        CancellationToken cancellationToken)
        => Ok(await _service.조회Async(
            sourceYear,
            itemCode,
            categoryCode,
            cancellationToken));
}
