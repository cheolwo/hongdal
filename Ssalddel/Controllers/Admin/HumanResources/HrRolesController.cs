using Ssalddel.Application.CommandProcessing;
using Ssalddel.Controllers;
using Ssalddel.Application.HumanResources;
using Ssalddel.Contracts.Common.Hr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin.HumanResources;

[SsalddelApiVersion(SsalddelProductVersion.V2_5)]
[ApiController]
[Authorize(Roles = "서버관리자")]
[Route("api/v1/admin/hr-roles")]
public sealed class HrRolesController : ControllerBase
{
    private readonly IHrRoleAssignmentStore _roleAssignmentStore;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public HrRolesController(IHrRoleAssignmentStore roleAssignmentStore, ICurrentUserAccessor currentUserAccessor)
    {
        _roleAssignmentStore = roleAssignmentStore;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<HrRoleAssignmentListResponse>> List(
        [FromQuery] string? userId,
        [FromQuery] string? scopeType,
        [FromQuery] string? scopeId,
        CancellationToken cancellationToken)
    {
        var assignments = await _roleAssignmentStore.ListAsync(userId, scopeType, scopeId, cancellationToken);
        return Ok(new HrRoleAssignmentListResponse
        {
            Items = assignments.Select(HrRoleAssignmentMapping.ToResponse).ToArray()
        });
    }

    [HttpPost]
    public async Task<ActionResult<HrRoleAssignmentResponse>> Assign(
        [FromBody] HrRoleAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var assignment = await _roleAssignmentStore.AssignAsync(
            request.UserId,
            request.ScopeType,
            request.ScopeId,
            request.ParticipantCategory,
            request.RoleCode,
            request.RoleName,
            _currentUserAccessor.UserId ?? "system",
            request.WorkScheduleEnabled,
            request.TimeZoneId,
            request.AllowedDaysOfWeek,
            request.WorkStartLocalTime,
            request.WorkEndLocalTime,
            request.WorksiteIpRestrictionEnabled,
            request.AllowedWorksiteIpRanges,
            cancellationToken);

        return Ok(HrRoleAssignmentMapping.ToResponse(assignment));
    }

    [HttpDelete("{assignmentId:guid}")]
    public async Task<IActionResult> Revoke(Guid assignmentId, CancellationToken cancellationToken)
    {
        var removed = await _roleAssignmentStore.RevokeAsync(assignmentId, cancellationToken);
        return removed ? NoContent() : this.ToNotFoundProblem("HR 역할 배정을 찾을 수 없습니다.");
    }
}
