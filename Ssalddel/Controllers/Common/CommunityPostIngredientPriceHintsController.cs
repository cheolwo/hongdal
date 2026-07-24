using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Controllers.Common;

[ApiController]
[AllowAnonymous]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[Route("api/v1/community/post-authoring/ingredient-price-hints")]
public sealed class CommunityPostIngredientPriceHintsController(
    ICommunityPostIngredientPriceHintService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CommunityPostIngredientPriceHintResponse>> GetHints(
        [FromBody] CommunityPostIngredientPriceHintRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await service.GetHintsAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "가격 힌트 요청을 확인해 주세요.",
                Detail = exception.Message
            });
        }
    }
}
