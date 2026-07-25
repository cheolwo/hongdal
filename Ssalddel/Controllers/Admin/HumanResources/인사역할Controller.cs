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
[SsalddelApiContractName("HrRolesController")]
public sealed class 인사역할Controller : ControllerBase
{
    private readonly IHrRoleAssignmentStore _roleAssignmentStore;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 인사역할Controller(IHrRoleAssignmentStore roleAssignmentStore, ICurrentUserAccessor currentUserAccessor)
    {
        _roleAssignmentStore = roleAssignmentStore;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<HrRoleAssignmentListResponse>> 목록조회(
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
    [SsalddelApiContractName("Assign")]
    public async Task<ActionResult<HrRoleAssignmentResponse>> 배정(
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
    [SsalddelApiContractName("Revoke")]
    public async Task<IActionResult> 회수(Guid assignmentId, CancellationToken cancellationToken)
    {
        var removed = await _roleAssignmentStore.RevokeAsync(assignmentId, cancellationToken);
        return removed ? NoContent() : this.ToNotFoundProblem("HR 역할 배정을 찾을 수 없습니다.");
    }
}
