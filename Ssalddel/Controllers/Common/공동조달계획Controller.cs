using System.Security.Claims;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.CollectiveProcurement;
using Ssalddel.Services.CollectiveProcurement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ssalddel.Controllers.Common;

[ApiController]
[Authorize]
[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelApiGrowthTrack(SsalddelApiGrowthTrack.Community)]
[Route("api/v1/collective-procurement/plans")]
[SsalddelApiContractName("CollectiveProcurementPlansController")]
public sealed class 공동조달계획Controller : CommunityControllerBase
{
    private readonly ICollectiveProcurementPlanningService 공동조달계획Service;

    public 공동조달계획Controller(ICollectiveProcurementPlanningService 공동조달계획Service)
    {
        this.공동조달계획Service = 공동조달계획Service;
    }

    [HttpPost]
    [SsalddelApiContractName("Create")]
    public Task<IActionResult> 생성(
        [FromBody] CreateCollectiveProcurementPlanRequest request,
        CancellationToken cancellationToken)
        => ExecuteAsync(async () =>
        {
            var created = await 공동조달계획Service.CreateAsync(request, CurrentUserId(), cancellationToken);
            return CreatedAtAction(nameof(조회), new { planId = created.PlanId }, created);
        });

    [HttpGet("{planId:guid}")]
    [SsalddelApiContractName("Get")]
    public Task<IActionResult> 조회(Guid planId, CancellationToken cancellationToken)
        => ExecuteAsync(async () =>
        {
            var result = await 공동조달계획Service.GetAsync(planId, CurrentUserId(), cancellationToken);
            return result is null ? NotFound() : Ok(result);
        });

    [HttpPost("{planId:guid}/revisions")]
    [SsalddelApiContractName("Recalculate")]
    public Task<IActionResult> 재계산(
        Guid planId,
        [FromBody] RecalculateCollectiveProcurementPlanRequest request,
        CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await 공동조달계획Service.RecalculateAsync(
            planId,
            request,
            CurrentUserId(),
            cancellationToken)));

    [HttpPut("{planId:guid}/disclosure-consent")]
    [SsalddelApiContractName("UpdateDisclosureConsent")]
    public Task<IActionResult> 정보공개동의수정(
        Guid planId,
        [FromBody] UpdateCollectiveProcurementDisclosureConsentRequest request,
        CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await 공동조달계획Service.UpdateDisclosureConsentAsync(
            planId,
            request,
            CurrentUserId(),
            cancellationToken)));

    [HttpPost("{planId:guid}/revisions/{calculationRevision:int}/acceptance")]
    [SsalddelApiContractName("AcceptRevision")]
    public Task<IActionResult> 개정수락(
        Guid planId,
        int calculationRevision,
        [FromBody] AcceptCollectiveProcurementRevisionRequest request,
        CancellationToken cancellationToken)
    {
        request.CalculationRevision = calculationRevision;
        return ExecuteAsync(async () => Ok(await 공동조달계획Service.AcceptRevisionAsync(
            planId,
            request,
            CurrentUserId(),
            cancellationToken)));
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "공동조달계획을 찾을 수 없습니다.",
                detail: ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "공동조달계획에 접근할 권한이 없습니다.",
                detail: ex.Message);
        }
        catch (CollectiveProcurementPlanConcurrencyException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "공동조달계획이 이미 변경되었습니다.",
                detail: ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "공동조달계획 요청이 올바르지 않습니다.",
                detail: ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "현재 공동조달계획 상태에서는 요청을 처리할 수 없습니다.",
                detail: ex.Message);
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? throw new UnauthorizedAccessException("사용자 식별자를 찾을 수 없습니다.");
}
