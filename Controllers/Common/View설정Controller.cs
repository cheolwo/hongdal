using System.Security.Claims;
using Hongdal.Contracts.Common.ViewSettings;
using Microsoft.AspNetCore.Mvc;
using 홍달.Data;
using 홍달.Services.Audit;
using 홍달.Services.ViewSettings;

namespace Hongdal.Controllers.Common;

[ApiController]
[Route("api/v1/view-settings")]
public sealed class View설정Controller : ControllerBase
{
    private const string UserIdHeaderName = "X-View-UserId";
    private const string RoleHeaderName = "X-View-Role";

    private readonly IView가시성Service _viewVisibilityService;
    private readonly I사용자행위로그Service _activityLogService;

    public View설정Controller(IView가시성Service viewVisibilityService, I사용자행위로그Service activityLogService)
    {
        _viewVisibilityService = viewVisibilityService;
        _activityLogService = activityLogService;
    }

    [HttpGet("effective")]
    public async Task<ActionResult<View가시성목록응답>> 조회([FromQuery] string appKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            return BadRequest("appKey is required");
        }

        var normalizedAppKey = appKey.Trim();
        var roleName = ResolveRoleName(normalizedAppKey);
        var userId = ResolveUserId();
        var items = await _viewVisibilityService.GetEffectiveViewsAsync(normalizedAppKey, roleName, userId, cancellationToken);

        return Ok(new View가시성목록응답
        {
            Items = items
        });
    }

    [HttpPut("user")]
    public async Task<IActionResult> 저장([FromBody] 사용자View가시성수정요청 request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest("request body is required");
        }

        if (string.IsNullOrWhiteSpace(request.AppKey))
        {
            return BadRequest("appKey is required");
        }

        if (string.IsNullOrWhiteSpace(request.ViewKey))
        {
            return BadRequest("viewKey is required");
        }

        var appKey = request.AppKey.Trim();
        var roleName = ResolveRoleName(appKey);
        var userId = ResolveUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("userId could not be resolved");
        }

        try
        {
            await _viewVisibilityService.SetUserVisibilityAsync(appKey, roleName, userId, request.ViewKey.Trim(), request.IsVisible, cancellationToken);
            await _activityLogService.기록Async(new 사용자행위로그기록
            {
                AppKey = appKey,
                UserId = userId,
                RoleName = roleName,
                ActionType = "ViewSettings",
                ActionName = "UserVisibilityChanged",
                Route = Request.Path.Value ?? string.Empty,
                TraceId = HttpContext.TraceIdentifier,
                IsSuccess = true,
                ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = Request.Headers.UserAgent.ToString(),
                OccurredAtUtc = DateTime.UtcNow,
                MetadataJson = $"{{\"viewKey\":\"{request.ViewKey.Trim()}\",\"isVisible\":{request.IsVisible.ToString().ToLowerInvariant()}}}"
            }, cancellationToken);
            return NoContent();
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

    private string? ResolveUserId()
    {
        var claimUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(claimUserId))
        {
            return claimUserId;
        }

        var headerUserId = Request.Headers[UserIdHeaderName].ToString();
        return string.IsNullOrWhiteSpace(headerUserId) ? null : headerUserId.Trim();
    }

    private string ResolveRoleName(string appKey)
    {
        var claimRole = User.FindFirstValue(ClaimTypes.Role);
        if (!string.IsNullOrWhiteSpace(claimRole))
        {
            return claimRole;
        }

        var headerRole = Request.Headers[RoleHeaderName].ToString();
        if (!string.IsNullOrWhiteSpace(headerRole))
        {
            return headerRole.Trim();
        }

        return appKey switch
        {
            App식별자.DriverApp => 역할명.기사,
            App식별자.ShipperApp => 역할명.화주,
            App식별자.HongdalAdmin => 역할명.서버관리자,
            _ => throw new InvalidOperationException("지원하지 않는 appKey 입니다.")
        };
    }
}
