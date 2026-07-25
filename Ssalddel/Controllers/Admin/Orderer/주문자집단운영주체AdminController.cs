using System.Security.Claims;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Filters;
using Ssalddel.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Admin.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/orderer-group-operating-entities")]
public sealed class 주문자집단운영주체AdminController : ControllerBase
{
    private readonly I주문자집단운영주체저장소 _운영주체Store;

    public 주문자집단운영주체AdminController(I주문자집단운영주체저장소 운영주체Store)
    {
        _운영주체Store = 운영주체Store;
    }

    [HttpGet]
    [SsalddelApiContractName("List")]
    public async Task<ActionResult<IReadOnlyList<주문자집단운영주체Dto>>> 목록조회(
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? entityType,
        [FromQuery] string? businessVerificationStatus,
        [FromQuery] bool? canActAsImporterOfRecord,
        [FromQuery] bool? canEmployWorkers,
        CancellationToken cancellationToken)
    {
        var items = await _운영주체Store.목록조회Async(new 주문자집단운영주체조회조건
        {
            주문자집단배송권키 = ordererGroupScopeKey,
            운영주체유형 = entityType,
            사업자검증상태 = businessVerificationStatus,
            수입자역할가능 = canActAsImporterOfRecord,
            고용가능 = canEmployWorkers
        }, cancellationToken);

        return Ok(items);
    }

    [HttpGet("{ordererGroupScopeKey}")]
    [SsalddelApiContractName("Get")]
    public async Task<IActionResult> 상세조회(string ordererGroupScopeKey, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _운영주체Store.배송권키로조회Async(ordererGroupScopeKey, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("주문자 집단 운영 주체 프로필을 찾을 수 없습니다.")
                : Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "주문자 집단 식별자가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpPost]
    [SsalddelApiContractName("Upsert")]
    public async Task<IActionResult> 등록또는수정(
        [FromBody] 주문자집단운영주체저장요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _운영주체Store.저장Async(request, ResolveUserId(), cancellationToken);
            return Ok(item);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "주문자 집단 운영 주체 입력값이 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private string ResolveUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? User.Identity?.Name
           ?? "admin";
}
