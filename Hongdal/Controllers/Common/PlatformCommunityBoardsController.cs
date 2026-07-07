using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[HongdalApiGrowthTrack(HongdalApiGrowthTrack.Community)]
[ApiController]
[Route("api/v1/community/boards")]
public sealed class PlatformCommunityBoardsController : ControllerBase
{
    private readonly IPlatformCommunityBoardUseCase _useCase;

    public PlatformCommunityBoardsController(IPlatformCommunityBoardUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List(
        [FromQuery] string? appKey,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.목록Async(appKey, status, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(
        [FromBody] PlatformCommunityBoardCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.신청Async(request, cancellationToken);
        if (result.IsFailed)
        {
            return this.ToActionResult(result);
        }

        return CreatedAtAction(nameof(List), new { appKey = result.Value.AppKey, status = result.Value.Status }, result.Value);
    }

    [HttpPost("{id:long}/approve")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> Approve(
        long id,
        [FromBody] PlatformCommunityBoardReviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.승인Async(id, request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("{id:long}/reject")]
    [Authorize(Policy = "서버관리자전용")]
    public async Task<IActionResult> Reject(
        long id,
        [FromBody] PlatformCommunityBoardReviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.반려Async(id, request, cancellationToken);
        return this.ToActionResult(result);
    }
}
