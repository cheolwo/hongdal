using Ssalddel.Application.Sales;
using Ssalddel.Controllers;
using Ssalddel.Contracts.Common.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Common;

[SsalddelApiVersion(SsalddelProductVersion.V2_5)]
[SsalddelApiWorkflow(SsalddelWorkflow.SalesChannelFulfillment)]
[ApiController]
[Authorize(Policy = "운영사용자전용")]
[Route("api/v1/sales-channels")]
[SsalddelApiContractName("SalesChannelsController")]
public sealed class 판매채널Controller : ControllerBase
{
    private readonly I판매채널UseCase _판매채널UseCase;
    private readonly I판매페이지UseCase _salesPageUseCase;
    private readonly I판매채널주문조회UseCase _orderReadUseCase;

    public 판매채널Controller(
        I판매채널UseCase 판매채널UseCase,
        I판매페이지UseCase salesPageUseCase,
        I판매채널주문조회UseCase orderReadUseCase)
    {
        _판매채널UseCase = 판매채널UseCase;
        _salesPageUseCase = salesPageUseCase;
        _orderReadUseCase = orderReadUseCase;
    }

    [HttpGet("product-pages/drafts")]
    [SsalddelApiVersion(SsalddelProductVersion.V0_0)]
    public async Task<IActionResult> 판매페이지초안목록(CancellationToken cancellationToken)
        => this.ToActionResult(await _salesPageUseCase.초안목록Async(요청Context생성(), cancellationToken));

    [HttpGet("product-pages/drafts/{pageId}")]
    [SsalddelApiVersion(SsalddelProductVersion.V0_0)]
    public async Task<IActionResult> 판매페이지초안조회(string pageId, CancellationToken cancellationToken)
        => this.ToActionResult(await _salesPageUseCase.초안조회Async(pageId, 요청Context생성(), cancellationToken));

    [HttpPost("product-pages/drafts")]
    [SsalddelApiVersion(SsalddelProductVersion.V0_0)]
    public async Task<IActionResult> 판매페이지초안생성(
        [FromBody] 판매페이지초안생성요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _salesPageUseCase.초안생성Async(request, 요청Context생성(), cancellationToken));

    [HttpPut("product-pages/drafts/{pageId}")]
    [SsalddelApiVersion(SsalddelProductVersion.V0_0)]
    public async Task<IActionResult> 판매페이지초안수정(
        string pageId,
        [FromBody] 판매페이지초안수정요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _salesPageUseCase.초안수정Async(pageId, request, 요청Context생성(), cancellationToken));

    [HttpGet("accounts")]
    [SsalddelApiVersion(
        SsalddelProductVersion.V2_5,
        FeatureKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow,
        WorkflowKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    [RequireVersionFeature(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    public async Task<IActionResult> 계정목록(CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.계정목록Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("accounts/{accountId:long}")]
    [SsalddelApiVersion(
        SsalddelProductVersion.V2_5,
        FeatureKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow,
        WorkflowKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    [RequireVersionFeature(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    public async Task<IActionResult> 계정상세(long accountId, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.계정상세Async(accountId, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("accounts")]
    [SsalddelApiVersion(
        SsalddelProductVersion.V2_5,
        FeatureKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow,
        WorkflowKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    [RequireVersionFeature(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    public async Task<IActionResult> 계정생성([FromBody] 판매채널계정저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.계정생성Async(request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("accounts/{accountId:long}")]
    [SsalddelApiVersion(
        SsalddelProductVersion.V2_5,
        FeatureKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow,
        WorkflowKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    [RequireVersionFeature(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    public async Task<IActionResult> 계정수정(long accountId, [FromBody] 판매채널계정저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.계정수정Async(accountId, request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("accounts/{accountId:long}")]
    [SsalddelApiVersion(
        SsalddelProductVersion.V2_5,
        FeatureKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow,
        WorkflowKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    [RequireVersionFeature(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    public async Task<IActionResult> 계정삭제(long accountId, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.계정삭제Async(accountId, 요청Context생성(), cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpGet("orders")]
    [SsalddelApiVersion(
        SsalddelProductVersion.V2_5,
        FeatureKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow,
        WorkflowKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    [RequireVersionFeature(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    public async Task<IActionResult> 주문출고후보목록(
        [FromQuery] 판매채널주문목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _orderReadUseCase.목록Async(request, cancellationToken));

    [HttpGet("orders/{orderId:long}")]
    [SsalddelApiVersion(
        SsalddelProductVersion.V2_5,
        FeatureKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow,
        WorkflowKey = VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    [RequireVersionFeature(VersionFeatureFlagKeys.SalesChannelFulfillmentWorkflow)]
    public async Task<IActionResult> 주문출고후보상세(
        long orderId,
        CancellationToken cancellationToken)
        => this.ToActionResult(await _orderReadUseCase.상세Async(orderId, cancellationToken));

    [HttpGet("products")]
    public async Task<IActionResult> 상품목록(CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.상품목록Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("products")]
    public async Task<IActionResult> 상품생성([FromBody] 판매상품저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.상품생성Async(request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("products/{productId:long}")]
    public async Task<IActionResult> 상품수정(long productId, [FromBody] 판매상품저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.상품수정Async(productId, request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("products/{productId:long}")]
    public async Task<IActionResult> 상품삭제(long productId, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.상품삭제Async(productId, 요청Context생성(), cancellationToken);
        return this.ToNoContentActionResult(result);
    }

    [HttpPost("products/seed-samples")]
    public async Task<IActionResult> 샘플상품시드([FromBody] 판매상품샘플시드요청 request, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.샘플상품시드Async(request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("listings")]
    public async Task<IActionResult> 출품목록(CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.출품목록Async(cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("listings")]
    public async Task<IActionResult> 출품생성([FromBody] 채널출품저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.출품생성Async(request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPut("listings/{listingId:long}")]
    public async Task<IActionResult> 출품수정(long listingId, [FromBody] 채널출품저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.출품수정Async(listingId, request, 요청Context생성(), cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpDelete("listings/{listingId:long}")]
    public async Task<IActionResult> 출품삭제(long listingId, CancellationToken cancellationToken)
    {
        var result = await _판매채널UseCase.출품삭제Async(listingId, 요청Context생성(), cancellationToken);
        return this.ToNoContentActionResult(result);
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
