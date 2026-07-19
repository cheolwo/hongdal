using Ssalddel.Contracts.Common.ViewSettings;
using Ssalddel.Application.ViewSettings;
using Ssalddel.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin;

[SsalddelApiVersion(SsalddelProductVersion.V1_0)]
[ApiController]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/view-policies")]
public sealed class View정책Controller : ControllerBase
{
    private readonly I관리자View정책UseCase _useCase;

    public View정책Controller(I관리자View정책UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> 조회([FromQuery] string? appKey, CancellationToken cancellationToken)
    {
        var result = await _useCase.조회Async(appKey, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> 수정(long id, [FromBody] 관리자View정책수정요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.수정Async(id, request, new 관리자View정책Context(
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            User.Identity?.Name ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Request.Path.Value ?? string.Empty,
            HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Request.Headers.UserAgent.ToString()), cancellationToken);

        return this.ToActionResult(result);
    }
}
