using Hongdal.Application.Sales;
using Hongdal.Controllers;
using Hongdal.Contracts.Common.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Common;

[HongdalApiVersion(HongdalProductVersion.V2_5)]
[HongdalApiWorkflow(HongdalWorkflow.SalesChannelFulfillment)]
[ApiController]
[Authorize(Policy = "운영사용자전용")]
[Route("api/v1/sales-channels")]
public sealed class SalesChannelsController : ControllerBase
{
    private readonly I판매채널UseCase _useCase;

    public SalesChannelsController(I판매채널UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> 계정목록(CancellationToken cancellationToken)
    {
        var result = await _useCase.계정목록Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> 계정생성([FromBody] 판매채널계정저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.계정생성Async(request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("products")]
    public async Task<IActionResult> 상품목록(CancellationToken cancellationToken)
    {
        var result = await _useCase.상품목록Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("products")]
    public async Task<IActionResult> 상품생성([FromBody] 판매상품저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.상품생성Async(request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("products/seed-samples")]
    public async Task<IActionResult> 샘플상품시드([FromBody] 판매상품샘플시드요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.샘플상품시드Async(request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("listings")]
    public async Task<IActionResult> 출품목록(CancellationToken cancellationToken)
    {
        var result = await _useCase.출품목록Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("listings")]
    public async Task<IActionResult> 출품생성([FromBody] 채널출품저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _useCase.출품생성Async(request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    private 판매채널요청Context 요청Context생성()
        => new(
            Request.Headers["X-App-Key"].ToString(),
            User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            User.Identity?.Name ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            Request.Path.Value ?? string.Empty,
            HttpContext.TraceIdentifier,
            HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            Request.Headers.UserAgent.ToString());
}
