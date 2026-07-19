using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[Authorize]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[Route("api/v1/orderer/domestic-group-purchases/{campaignId:guid}/vehicle-recommendations")]
public sealed class DomesticGroupPurchaseVehicleRecommendationsController : ControllerBase
{
    private readonly IDomesticGroupPurchaseVehicleRecommendationService _service;

    public DomesticGroupPurchaseVehicleRecommendationsController(
        IDomesticGroupPurchaseVehicleRecommendationService service)
    {
        _service = service;
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(
        Guid campaignId,
        [FromBody] DomesticGroupPurchaseVehicleRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            request.GroupPurchaseCampaignId = campaignId;
            return Ok(await _service.PreviewAsync(request, cancellationToken));
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
