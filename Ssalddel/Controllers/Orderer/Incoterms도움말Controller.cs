using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/orderer/trade/incoterms/help")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Api,
    "주문자 App의 물음표 도움말에 FOB, CIF, DDP 설명과 그림 구간을 제공합니다.",
    ContractType = typeof(IIncoterms도움말조회UseCase),
    FlowOrder = 30,
    Effects = SsalddelCodeEffect.None,
    Boundary = "조회만 제공하며 Incoterms 선택, 계약, 결제, 신고 또는 외부 전송을 수행하지 않습니다.")]
public sealed class Incoterms도움말Controller(
    IIncoterms도움말조회UseCase Incoterms도움말조회UseCase) : OrdererControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(Incoterms도움말응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult 조회(
        [FromQuery(Name = "termCode")] string? 선택코드,
        [FromQuery(Name = "languageCode")] string? 언어코드)
    {
        try
        {
            return Ok(Incoterms도움말조회UseCase.조회(선택코드, 언어코드));
        }
        catch (ArgumentException exception)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "지원하지 않는 인코텀즈 코드입니다.",
                detail: exception.Message);
        }
    }
}
