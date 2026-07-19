using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.ViewSettings;
using Ssalddel.Contracts.CommandSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Admin;

[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/auxiliary-feature-settings")]
public sealed class 보조기능설정Controller : ControllerBase
{
    private readonly I보조기능설정UseCase _useCase;

    public 보조기능설정Controller(I보조기능설정UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? userId,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.목록Async(userId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("global/{targetType}/{targetName}/{featureName}")]
    public async Task<IActionResult> UpdateGlobal(
        string targetType,
        string targetName,
        string featureName,
        [FromBody] AuxiliaryFeatureSettingUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.전역설정Async(targetType, targetName, featureName, request, 감사Context생성(), cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpDelete("global/{targetType}/{targetName}/{featureName}")]
    public async Task<IActionResult> ResetGlobal(
        string targetType,
        string targetName,
        string featureName,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.전역초기화Async(targetType, targetName, featureName, 감사Context생성(), cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpPut("users/{userId}/{targetType}/{targetName}/{featureName}")]
    public async Task<IActionResult> UpdateUser(
        string userId,
        string targetType,
        string targetName,
        string featureName,
        [FromBody] AuxiliaryFeatureSettingUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.사용자설정Async(userId, targetType, targetName, featureName, request, 감사Context생성(), cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpDelete("users/{userId}/{targetType}/{targetName}/{featureName}")]
    public async Task<IActionResult> ResetUser(
        string userId,
        string targetType,
        string targetName,
        string featureName,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.사용자초기화Async(userId, targetType, targetName, featureName, 감사Context생성(), cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    private 보조기능설정감사Context 감사Context생성()
        => new(
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            User.Identity?.Name ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Request.Path.Value ?? string.Empty,
            HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Request.Headers.UserAgent.ToString());
}
