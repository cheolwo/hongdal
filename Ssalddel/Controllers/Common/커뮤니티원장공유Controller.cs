using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[ApiController]
[Authorize]
[Route("api/v1/community/ledgers/{원장Id}/sharing")]
public sealed class 커뮤니티원장공유Controller : CommunityControllerBase
{
    private readonly I커뮤니티원장공유Service _service;

    public 커뮤니티원장공유Controller(I커뮤니티원장공유Service service)
    {
        _service = service;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 조회(string 원장Id, CancellationToken cancellationToken)
        => this.ToActionResult(await _service.설정조회Async(원장Id, CurrentUserId(), cancellationToken));

    [HttpPut]
    [SsalddelApiContractName("Update")]
    public async Task<IActionResult> 수정(
        string 원장Id,
        [FromBody] 커뮤니티원장공개설정변경Request request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _service.설정변경Async(원장Id, request, CurrentUserId(), cancellationToken));

    [HttpPost("reuse")]
    [SsalddelApiContractName("Reuse")]
    public async Task<IActionResult> 재사용(
        string 원장Id,
        [FromBody] 커뮤니티원장재사용Request request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _service.재사용Async(원장Id, request, CurrentUserId(), cancellationToken));

    private string? CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub");
}
