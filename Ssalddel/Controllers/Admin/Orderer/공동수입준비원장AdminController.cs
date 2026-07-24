using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.Orderer;

[ApiController]
[Authorize(Policy = "서버관리자전용")]
[SsalddelApiVersion(
    SsalddelProductVersion.V1_5,
    FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow,
    WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
[Route("api/v1/admin/orderer/group-purchase-demand-os/groups/{autoGroupId}/trade-readiness")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.Api,
    "관리자가 승인된 1.0 수요 집단의 1.5 공급·가격·무역 준비 자료를 미리 보고 영속 원장으로 저장합니다.",
    ContractType = typeof(I공동수입준비원장Service),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "서버 관리자와 1.5 기능 플래그를 모두 요구하며 계약, 결제, 신고, 운송 또는 창고 API를 호출하지 않습니다.")]
public sealed class 공동수입준비원장AdminController : ControllerBase
{
    private readonly I공동수입준비원장Service _service;
    private readonly I공동수입준비OS _operatingSystem;

    public 공동수입준비원장AdminController(
        I공동수입준비원장Service service,
        I공동수입준비OS operatingSystem)
    {
        _service = service;
        _operatingSystem = operatingSystem;
    }

    [HttpGet]
    [ProducesResponseType(typeof(공동수입준비원장응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> 조회(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        CancellationToken cancellationToken)
    {
        var result = await _service.조회Async(자동집단Id, cancellationToken);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("preview")]
    [ProducesResponseType(typeof(공동수입준비원장응답), StatusCodes.Status200OK)]
    public Task<IActionResult> 미리보기(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        [FromBody] 공동수입준비원장저장요청 request,
        CancellationToken cancellationToken)
        => ExecuteAsync(async () => Ok(await _service.미리보기Async(
            자동집단Id,
            request,
            cancellationToken)));

    [HttpPut]
    [ProducesResponseType(typeof(공동수입준비원장응답), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(공동수입준비원장응답), StatusCodes.Status201Created)]
    public Task<IActionResult> 저장(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        [FromBody] 공동수입준비원장저장요청 request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.요청멱등키 = idempotencyKey.Trim();
        }

        return ExecuteAsync(async () =>
        {
            var result = await _service.저장Async(
                자동집단Id,
                request,
                CurrentUserId(),
                CurrentUserDisplayName(),
                cancellationToken);
            return result.생성됨
                ? CreatedAtAction(nameof(조회), new { autoGroupId = 자동집단Id }, result)
                : Ok(result);
        });
    }

    [HttpGet("os")]
    [ProducesResponseType(typeof(공동수입준비Os상태응답), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Os상태조회(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        CancellationToken cancellationToken)
    {
        var result = await _operatingSystem.운영상태조회Async(자동집단Id, cancellationToken);
        return result is null
            ? NotFound()
            : Ok(result);
    }

    [HttpPost("os/workloads/run")]
    [ProducesResponseType(typeof(공동수입준비Os상태응답), StatusCodes.Status200OK)]
    public Task<IActionResult> Os작업실행(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        [FromBody] 공동수입준비Os작업실행요청 request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.요청멱등키 = idempotencyKey.Trim();
        }

        return ExecuteAsync(async () => Ok(await _operatingSystem.작업실행Async(
            자동집단Id,
            request,
            CurrentUserId(),
            CurrentUserDisplayName(),
            cancellationToken)));
    }

    [HttpPost("os/qualified-review-handoff")]
    [ProducesResponseType(typeof(공동수입준비Os상태응답), StatusCodes.Status200OK)]
    public Task<IActionResult> 전문검토인계(
        [FromRoute(Name = "autoGroupId")] string 자동집단Id,
        [FromBody] 공동수입준비Os전문검토인계요청 request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.요청멱등키 = idempotencyKey.Trim();
        }

        return ExecuteAsync(async () => Ok(await _operatingSystem.전문검토인계Async(
            자동집단Id,
            request,
            CurrentUserId(),
            CurrentUserDisplayName(),
            cancellationToken)));
    }

    private static async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (KeyNotFoundException exception)
        {
            return new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "공동수입 준비 대상을 찾을 수 없습니다.",
                Detail = exception.Message
            })
            {
                StatusCode = StatusCodes.Status404NotFound
            };
        }
        catch (ArgumentException exception)
        {
            return new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "공동수입 준비 요청이 올바르지 않습니다.",
                Detail = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return new ConflictObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "현재 상태에서는 공동수입 원장의 1.5 준비 블록을 저장할 수 없습니다.",
                Detail = exception.Message
            });
        }
    }

    private string CurrentUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.FindFirstValue("sub")
           ?? User.Identity?.Name
           ?? throw new UnauthorizedAccessException("관리자 식별자를 찾을 수 없습니다.");

    private string CurrentUserDisplayName()
        => User.FindFirstValue(ClaimTypes.Name)
           ?? User.Identity?.Name
           ?? "1.5 준비 관리자";
}
