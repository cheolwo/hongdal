using Hongdal.Contracts.Common.PlatformProfit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Settlement;

namespace Hongdal.Controllers.Admin.Settlement;

[ApiController]
[Authorize(Roles = "서버관리자")]
[Route("api/v1/admin/platform-profit-returns")]
public sealed class PlatformProfitReturnsController : ControllerBase
{
    private readonly IPlatformProfitReturnService _profitReturnService;

    public PlatformProfitReturnsController(IPlatformProfitReturnService profitReturnService)
    {
        _profitReturnService = profitReturnService;
    }

    [HttpPost("revenues")]
    public async Task<ActionResult<PlatformRevenueEntryResponse>> RecordRevenue(
        [FromBody] PlatformRevenueEntryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _profitReturnService.RecordRevenueAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("policies")]
    public async Task<ActionResult<PlatformProfitReturnPolicyResponse>> CreatePolicy(
        [FromBody] PlatformProfitReturnPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _profitReturnService.CreatePolicyAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("schedules")]
    public async Task<ActionResult<PlatformProfitReturnPlanResponse>> CreateSchedules(
        [FromBody] PlatformProfitReturnScheduleCreateRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _profitReturnService.CreateReturnSchedulesAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("schedules")]
    public async Task<ActionResult<PlatformProfitReturnScheduleListResponse>> ListSchedules(
        [FromQuery] string? participantUserId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var items = await _profitReturnService.ListSchedulesAsync(participantUserId, from, to, cancellationToken);
        return Ok(new PlatformProfitReturnScheduleListResponse { Items = items });
    }
}
