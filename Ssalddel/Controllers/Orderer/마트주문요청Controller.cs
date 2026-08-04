using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Mart;
using Ssalddel.Contracts.Mart;
using Ssalddel.Filters;
using 살뜰.Services.Versioning;
using System.Security.Claims;
using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Services.Privacy;

namespace Ssalddel.Controllers.Orderer;

[SsalddelApiVersion(
    SsalddelProductVersion.V3_5,
    FeatureKey = VersionFeatureFlagKeys.SsalddelMartWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.SsalddelMartWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.SsalddelMart)]
[RequireVersionFeature(VersionFeatureFlagKeys.SsalddelMartWorkflow)]
[ApiController]
[Authorize]
[Route("api/v1/orderer/mart/order-requests")]
public sealed class 마트주문요청Controller(
    I마트주문요청조회UseCase queryUseCase,
    I마트주문요청작성UseCase commandUseCase,
    I신청개인정보동의증적Service applicationPrivacyConsent) : OrdererControllerBase
{
    [HttpGet]
    public async Task<IActionResult> 목록(
        [FromQuery] 마트주문요청목록조회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await queryUseCase.목록Async(request, cancellationToken));

    [HttpGet("{orderRequestId:guid}")]
    public async Task<IActionResult> 상세(Guid orderRequestId, CancellationToken cancellationToken)
        => this.ToActionResult(await queryUseCase.상세Async(orderRequestId, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> 등록(
        [FromBody] 마트주문요청등록요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            await applicationPrivacyConsent.유효한동의요구Async(
                request.신청개인정보동의증적Id,
                신청개인정보업무Codes.개별주문,
                request.신청출처Code,
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.Identity?.Name
                ?? string.Empty,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "개인정보 동의를 확인할 수 없습니다.", Detail = ex.Message });
        }

        return this.ToActionResult(await commandUseCase.등록Async(request, cancellationToken));
    }

    [HttpPut("{orderRequestId:guid}/quantity")]
    public async Task<IActionResult> 수량변경(
        Guid orderRequestId,
        [FromBody] 마트주문요청수량변경요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await commandUseCase.수량변경Async(
            orderRequestId,
            request,
            cancellationToken));

    [HttpPost("{orderRequestId:guid}/withdrawal")]
    public async Task<IActionResult> 철회(
        Guid orderRequestId,
        [FromBody] 마트주문요청철회요청 request,
        CancellationToken cancellationToken)
        => this.ToActionResult(await commandUseCase.철회Async(
            orderRequestId,
            request,
            cancellationToken));
}
