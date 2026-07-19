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
public sealed class CommunityLedgerBlockAssignmentsController : ControllerBase
{
    private readonly ICommunityLedgerBlockAssignmentService _service;

    public CommunityLedgerBlockAssignmentsController(ICommunityLedgerBlockAssignmentService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        string ledgerId,
        string blockId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _service.GetAsync(
            ledgerId,
            blockId,
            CurrentUserId(),
            cancellationToken));

    [HttpPut]
    public async Task<IActionResult> Update(
        string ledgerId,
        string blockId,
        [FromBody] CommunityLedgerBlockAssignmentUpdateRequest request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _service.UpdateAsync(
            ledgerId,
            blockId,
            request,
            CurrentUserId(),
            cancellationToken));

    private string? CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub");
}
