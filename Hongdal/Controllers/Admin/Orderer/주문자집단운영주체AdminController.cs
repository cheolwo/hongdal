using System.Security.Claims;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Controllers;
using Hongdal.ApiMetadata;
using Hongdal.Filters;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Admin.Orderer;

[ApiController]
[HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Authorize(Policy = "서버관리자전용")]
[Route("api/v1/admin/orderer/orderer-group-operating-entities")]
public sealed class 주문자집단운영주체AdminController : ControllerBase
{
    private readonly I주문자집단운영주체저장소 _store;

    public 주문자집단운영주체AdminController(I주문자집단운영주체저장소 store)
    {
        _store = store;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<주문자집단운영주체Dto>>> List(
        [FromQuery] string? ordererGroupScopeKey,
        [FromQuery] string? entityType,
        [FromQuery] string? businessVerificationStatus,
        [FromQuery] bool? canActAsImporterOfRecord,
        [FromQuery] bool? canEmployWorkers,
        CancellationToken cancellationToken)
    {
        var items = await _store.목록조회Async(new 주문자집단운영주체조회조건
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
    public async Task<IActionResult> Get(string ordererGroupScopeKey, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.배송권키로조회Async(ordererGroupScopeKey, cancellationToken);
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
    public async Task<IActionResult> Upsert(
        [FromBody] 주문자집단운영주체저장요청 request,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.저장Async(request, ResolveUserId(), cancellationToken);
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
