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
[SsalddelApiContractName("CommunityPostIngredientPriceHintsController")]
public sealed class 커뮤니티게시글식재료가격참고Controller(
    ICommunityPostIngredientPriceHintService 식재료가격참고Service) : CommunityControllerBase
{
    [HttpPost]
    [SsalddelApiContractName("GetHints")]
    public async Task<ActionResult<CommunityPostIngredientPriceHintResponse>> 가격참고조회(
        [FromBody] CommunityPostIngredientPriceHintRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return Ok(await 식재료가격참고Service.GetHintsAsync(request, cancellationToken));
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
