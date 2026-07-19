using Ssalddel.Contracts.Common.PlatformProfit;
using Ssalddel.Application.Settlement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Admin.Settlement;

[SsalddelApiVersion(SsalddelProductVersion.V2_5)]
[ApiController]
[Authorize(Roles = "서버관리자")]
[Route("api/v1/admin/platform-profit-returns")]
public sealed class PlatformProfitReturnsController : ControllerBase
{
    private readonly I플랫폼수익환급UseCase _useCase;

    public PlatformProfitReturnsController(I플랫폼수익환급UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost("revenues")]
    public async Task<IActionResult> RecordRevenue(
        [FromBody] PlatformRevenueEntryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.수익기록Async(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("policies")]
    public async Task<IActionResult> CreatePolicy(
        [FromBody] PlatformProfitReturnPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.정책생성Async(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpPost("schedules")]
    public async Task<IActionResult> CreateSchedules(
        [FromBody] PlatformProfitReturnScheduleCreateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.스케줄생성Async(request, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> ListSchedules(
        [FromQuery] string? participantUserId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.스케줄목록Async(participantUserId, from, to, cancellationToken);
        return this.ToActionResult(result);
    }
}
