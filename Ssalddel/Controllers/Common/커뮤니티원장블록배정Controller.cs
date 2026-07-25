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
[Route("api/v1/community/ledgers/{ledgerId}/blocks/{blockId}/assignees")]
[SsalddelApiContractName("CommunityLedgerBlockAssignmentsController")]
public sealed class 커뮤니티원장블록배정Controller : CommunityControllerBase
{
    private readonly ICommunityLedgerBlockAssignmentService _원장블록배정Service;

    public 커뮤니티원장블록배정Controller(ICommunityLedgerBlockAssignmentService 원장블록배정Service)
    {
        _원장블록배정Service = 원장블록배정Service;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 조회(
        string ledgerId,
        string blockId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _원장블록배정Service.GetAsync(
            ledgerId,
            blockId,
            CurrentUserId(),
            cancellationToken));

    [HttpPut]
    [SsalddelApiContractName("Update")]
    public async Task<IActionResult> 수정(
        string ledgerId,
        string blockId,
        [FromBody] CommunityLedgerBlockAssignmentUpdateRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _원장블록배정Service.UpdateAsync(
            ledgerId,
            blockId,
            request,
            CurrentUserId(),
            cancellationToken));

    private string? CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub");
}
