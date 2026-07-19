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
public sealed class CommunityLedgerRoleAccessController : ControllerBase
{
    private readonly ICommunityLedgerRoleAccessService _service;

    public CommunityLedgerRoleAccessController(ICommunityLedgerRoleAccessService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(string ledgerId, CancellationToken cancellationToken)
        => this.ToActionResult(await _service.GetSettingsAsync(
            ledgerId,
            CurrentUserId(),
            cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Update(
        string ledgerId,
        [FromBody] CommunityLedgerRoleAccessUpdateRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _service.UpdateSettingsAsync(
            ledgerId,
            request,
            CurrentUserId(),
            cancellationToken));

    private string? CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub");
}
