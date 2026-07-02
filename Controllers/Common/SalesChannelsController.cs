using Hongdal.Contracts.Common.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using 홍달.Services.Audit;
using 홍달.Services.Sales;

namespace Hongdal.Controllers.Common;

[ApiController]
[Authorize(Policy = "운영사용자전용")]
[Route("api/v1/sales-channels")]
public sealed class SalesChannelsController : ControllerBase
{
    private readonly ISalesChannelService _salesChannelService;
    private readonly I사용자행위로그Service _activityLogService;

    public SalesChannelsController(ISalesChannelService salesChannelService, I사용자행위로그Service activityLogService)
    {
        _salesChannelService = salesChannelService;
        _activityLogService = activityLogService;
    }

    [HttpGet("accounts")]
    public async Task<ActionResult<판매채널계정목록응답>> 계정목록(CancellationToken cancellationToken)
    {
        return Ok(await _salesChannelService.GetAccountsAsync(cancellationToken));
    }

    [HttpPost("accounts")]
    public async Task<ActionResult<판매채널계정항목응답>> 계정생성([FromBody] 판매채널계정저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _salesChannelService.CreateAccountAsync(request, cancellationToken);
        await LogAsync("SalesChannel", "AccountCreated", $"{{\"accountId\":{result.Id},\"channelType\":\"{result.채널종류}\"}}", cancellationToken);
        return Ok(result);
    }

    [HttpGet("products")]
    public async Task<ActionResult<판매상품목록응답>> 상품목록(CancellationToken cancellationToken)
    {
        return Ok(await _salesChannelService.GetProductsAsync(cancellationToken));
    }

    [HttpPost("products")]
    public async Task<ActionResult<판매상품항목응답>> 상품생성([FromBody] 판매상품저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _salesChannelService.CreateProductAsync(request, cancellationToken);
        await LogAsync("SalesProduct", "ProductCreated", $"{{\"productId\":{result.Id},\"inboundItemId\":{result.입고상품Id}}}", cancellationToken);
        return Ok(result);
    }

    [HttpPost("products/seed-samples")]
    public async Task<ActionResult<판매상품목록응답>> 샘플상품시드([FromBody] 판매상품샘플시드요청 request, CancellationToken cancellationToken)
    {
        var result = await _salesChannelService.SeedSampleProductsAsync(request, cancellationToken);
        await LogAsync("SalesProduct", "SampleProductsSeeded", $"{{\"count\":{result.Items.Count}}}", cancellationToken);
        return Ok(result);
    }

    [HttpGet("listings")]
    public async Task<ActionResult<채널출품목록응답>> 출품목록(CancellationToken cancellationToken)
    {
        return Ok(await _salesChannelService.GetListingsAsync(cancellationToken));
    }

    [HttpPost("listings")]
    public async Task<ActionResult<채널출품항목응답>> 출품생성([FromBody] 채널출품저장요청 request, CancellationToken cancellationToken)
    {
        var result = await _salesChannelService.CreateListingAsync(request, cancellationToken);
        await LogAsync("Listing", "ListingCreated", $"{{\"listingId\":{result.Id},\"salesProductId\":{result.판매상품Id},\"accountId\":{result.판매채널계정Id}}}", cancellationToken);
        return Ok(result);
    }

    private async Task LogAsync(string actionType, string actionName, string metadataJson, CancellationToken cancellationToken)
    {
        await _activityLogService.기록Async(new 사용자행위로그기록
        {
            AppKey = Request.Headers["X-App-Key"].ToString(),
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            UserName = User.Identity?.Name ?? string.Empty,
            RoleName = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            ActionType = actionType,
            ActionName = actionName,
            Route = Request.Path.Value ?? string.Empty,
            TraceId = HttpContext.TraceIdentifier,
            IsSuccess = true,
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = Request.Headers.UserAgent.ToString(),
            OccurredAtUtc = DateTime.UtcNow,
            MetadataJson = metadataJson
        }, cancellationToken);
    }
}
