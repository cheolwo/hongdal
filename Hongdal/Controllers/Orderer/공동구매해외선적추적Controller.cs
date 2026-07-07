using Hongdal.Controllers;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Filters;
using Hongdal.Services.Orderer;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Versioning;

namespace Hongdal.Controllers.Orderer;

[ApiController]
[HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Route("api/v1/orderer/group-purchase-overseas-shipments")]
public sealed class 공동구매해외선적추적Controller : ControllerBase
{
    private readonly I공동구매해외선적추적저장소 _store;
    private readonly I공동구매수입물류정규화Service _normalizationService;

    public 공동구매해외선적추적Controller(
        I공동구매해외선적추적저장소 store,
        I공동구매수입물류정규화Service normalizationService)
    {
        _store = store;
        _normalizationService = normalizationService;
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(
        [FromQuery] string documentManagementNumber,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetBy문서관리번호Async(documentManagementNumber, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("문서관리번호에 해당하는 공동주문 해외 선적 정보를 찾을 수 없습니다.")
                : Ok(공동구매해외선적추적Projection.ToPublicDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "문서관리번호가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("import-logistics-references")]
    public ActionResult<IReadOnlyList<수입물류참조항목>> 수입물류참조검색(
        [FromQuery] string? keyword,
        [FromQuery] string? transportMode,
        [FromQuery] string? codeType,
        [FromQuery] int pageSize = 20)
    {
        var items = _normalizationService.SearchReferences(new 수입물류참조조회요청
        {
            검색어 = keyword,
            운송수단 = transportMode,
            코드유형 = codeType,
            페이지크기 = pageSize
        });

        return Ok(items);
    }

    [HttpPost("import-logistics-normalization-simulation")]
    public ActionResult<수입물류정규화시뮬레이션결과> 수입물류정규화시뮬레이션(
        [FromBody] 수입물류정규화시뮬레이션요청 request)
    {
        var result = _normalizationService.Simulate(request);
        return Ok(result);
    }

    [HttpGet("lookup-normalized")]
    public async Task<IActionResult> LookupNormalized(
        [FromQuery] string documentManagementNumber,
        [FromQuery] string? customsOfficeCode,
        [FromQuery] string? customsOfficeName,
        [FromQuery] string? bondedAreaCode,
        [FromQuery] string? bondedAreaName,
        CancellationToken cancellationToken)
    {
        try
        {
            var item = await _store.GetBy문서관리번호Async(documentManagementNumber, cancellationToken);
            if (item is null)
            {
                return this.ToNotFoundProblem("Document management number was not found in the group purchase overseas shipment ledger.");
            }

            var currentLocation = item.이벤트목록.LastOrDefault()?.위치요약 ?? item.현재위치요약;
            var result = _normalizationService.Simulate(new 수입물류정규화시뮬레이션요청
            {
                문서관리번호 = item.문서관리번호,
                운송문서유형 = item.운송문서유형,
                운송문서번호 = item.운송문서번호,
                운송수단 = item.운송수단,
                출발국가코드 = item.출발국가코드,
                출발항코드 = item.출발항코드,
                도착항코드 = item.도착항코드,
                도착항만공항명 = item.현재위치요약,
                세관코드 = customsOfficeCode ?? string.Empty,
                세관명 = customsOfficeName ?? string.Empty,
                보세구역코드 = bondedAreaCode ?? string.Empty,
                보세구역명 = bondedAreaName ?? string.Empty,
                현재위치요약 = currentLocation,
                통관단계명 = item.현재상태코드
            });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "Document management number is invalid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
