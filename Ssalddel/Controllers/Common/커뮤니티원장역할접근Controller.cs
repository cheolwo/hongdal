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
[Route("api/v1/community/ledgers/{ledgerId}/role-access")]
[SsalddelApiContractName("CommunityLedgerRoleAccessController")]
public sealed class 커뮤니티원장역할접근Controller : CommunityControllerBase
{
    private readonly ICommunityLedgerRoleAccessService _원장역할접근Service;

    public 커뮤니티원장역할접근Controller(ICommunityLedgerRoleAccessService 원장역할접근Service)
    {
        _원장역할접근Service = 원장역할접근Service;
    }

    [HttpGet]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 조회(string ledgerId, CancellationToken cancellationToken)
        => this.ToActionResult(await _원장역할접근Service.GetSettingsAsync(
            ledgerId,
            CurrentUserId(),
            cancellationToken));

    [HttpPut]
    [SsalddelApiContractName("Update")]
    public async Task<IActionResult> 수정(
        string ledgerId,
        [FromBody] CommunityLedgerRoleAccessUpdateRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _원장역할접근Service.UpdateSettingsAsync(
            ledgerId,
            request,
            CurrentUserId(),
            cancellationToken));

    private string? CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub");
}
