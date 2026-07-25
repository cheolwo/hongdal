using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(SsalddelProductVersion.V2_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.DomesticTransport)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.CoreLogistics)]
[SsalddelApiContractName("DomesticGroupPurchaseVehicleRecommendationsController")]
[Route("api/v1/orderer/domestic-group-purchases/{campaignId:guid}/vehicle-recommendations")]
public sealed class 국내공동구매차량추천Controller : OrdererControllerBase
{
    private readonly IDomesticGroupPurchaseVehicleRecommendationService _차량추천Service;

    public 국내공동구매차량추천Controller(
        IDomesticGroupPurchaseVehicleRecommendationService 차량추천Service)
    {
        _차량추천Service = 차량추천Service;
    }

    [HttpPost("preview")]
    [SsalddelApiContractName("Preview")]
    public async Task<IActionResult> 미리보기(
        [FromRoute(Name = "campaignId")] Guid 모집Id,
        [FromBody] DomesticGroupPurchaseVehicleRecommendationRequest 요청,
        CancellationToken cancellationToken)
    {
        try
        {
            요청.GroupPurchaseCampaignId = 모집Id;
            return Ok(await _차량추천Service.PreviewAsync(요청, cancellationToken));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
