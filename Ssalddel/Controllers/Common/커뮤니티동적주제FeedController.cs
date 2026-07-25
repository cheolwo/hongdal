using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[Route("api/v1/community/dynamic-topic-feeds")]
[SsalddelApiContractName("CommunityDynamicTopicFeedsController")]
public sealed class 커뮤니티동적주제FeedController : CommunityControllerBase
{
    private readonly ICommunityDynamicDiscoveryService _동적주제FeedService;

    public 커뮤니티동적주제FeedController(ICommunityDynamicDiscoveryService 동적주제FeedService)
    {
        _동적주제FeedService = 동적주제FeedService;
    }

    [HttpGet]
    [AllowAnonymous]
    [SsalddelApiContractName("GetCatalog")]
    public ActionResult<CommunityDynamicTopicCatalogResponse> 주제목록조회()
        => Ok(_동적주제FeedService.GetCatalog());

    [HttpGet("{topicKey}")]
    [AllowAnonymous]
    [SsalddelApiContractName("Get")]
    public async Task<ActionResult<CommunityDynamicTopicFeedResponse>> 피드조회(
        string topicKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _동적주제FeedService.GetFeedAsync(topicKey, page, pageSize, cancellationToken);
        return result is null
            ? NotFound()
            : Ok(result);
    }
}
