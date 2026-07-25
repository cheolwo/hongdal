using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Admin.Content07;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[ApiController]
[Route("api/v1/admin/community-post-schedules")]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiContractName("CommunityPostScheduleController")]
public sealed class 커뮤니티게시글일정Controller : ControllerBase
{
    private readonly I커뮤니티게시글예약발행UseCase _커뮤니티게시글일정UseCase;

    public 커뮤니티게시글일정Controller(I커뮤니티게시글예약발행UseCase 커뮤니티게시글일정UseCase)
    {
        _커뮤니티게시글일정UseCase = 커뮤니티게시글일정UseCase;
    }

    [HttpPost]
    [SsalddelApiContractName("Create")]
    public async Task<IActionResult> 생성(
        [FromBody] PlatformCommunityPostScheduleCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _커뮤니티게시글일정UseCase.예약Async(request, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToActionResult(result);
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? status,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
        => this.ToActionResult(await _커뮤니티게시글일정UseCase.예약목록Async(status, take, cancellationToken));

    [HttpDelete("{id:long}")]
    [SsalddelApiContractName("Cancel")]
    public async Task<IActionResult> 취소(long id, CancellationToken cancellationToken)
        => this.ToActionResult(await _커뮤니티게시글일정UseCase.예약취소Async(id, cancellationToken));
}
