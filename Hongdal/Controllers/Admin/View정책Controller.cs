using Hongdal.Contracts.Common.ViewSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using 홍달.Services.Audit;
using 홍달.Services.ViewSettings;

namespace Hongdal.Controllers.Admin;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/view-policies")]
public sealed class View정책Controller : ControllerBase
{
    private readonly IView가시성Service _viewVisibilityService;
    private readonly I사용자행위로그Service _activityLogService;

    public View정책Controller(IView가시성Service viewVisibilityService, I사용자행위로그Service activityLogService)
    {
        _viewVisibilityService = viewVisibilityService;
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<ActionResult<관리자View정책목록응답>> 조회([FromQuery] string? appKey, CancellationToken cancellationToken)
    {
        var items = await _viewVisibilityService.GetPoliciesAsync(appKey, cancellationToken);
        return Ok(new 관리자View정책목록응답
        {
            Items = items
        });
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<관리자View정책항목응답>> 수정(long id, [FromBody] 관리자View정책수정요청 request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request body is required");
        }

        try
        {
            var updated = await _viewVisibilityService.UpdatePolicyAsync(id, request.PolicyEnabled, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            await _activityLogService.기록Async(new 사용자행위로그기록
            {
                AppKey = App식별자.HongdalAdmin,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
                UserName = User.Identity?.Name ?? string.Empty,
                RoleName = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
                ActionType = "ViewPolicy",
                ActionName = "PolicyChanged",
                Route = Request.Path.Value ?? string.Empty,
                TraceId = HttpContext.TraceIdentifier,
                IsSuccess = true,
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = Request.Headers.UserAgent.ToString(),
                OccurredAtUtc = DateTime.UtcNow,
                MetadataJson = $"{{\"policyId\":{id},\"enabled\":{request.PolicyEnabled.ToString().ToLowerInvariant()}}}"
            }, cancellationToken);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
