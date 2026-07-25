using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Orderer;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_0, FeatureKey = VersionFeatureFlagKeys.DomesticTransportWorkflow, WorkflowKey = VersionFeatureFlagKeys.DomesticTransportWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.DomesticTransportWorkflow)]
[Route("api/v1/orderer/group-purchase-overseas-shipments")]
public sealed class 공동구매해외선적추적Controller : OrdererControllerBase
{
    private readonly I공동구매해외선적추적UseCase _선적추적UseCase;

    public 공동구매해외선적추적Controller(I공동구매해외선적추적UseCase 선적추적UseCase)
    {
        _선적추적UseCase = 선적추적UseCase;
    }

    [HttpGet("lookup")]
    [SsalddelApiContractName("Lookup")]
    public async Task<IActionResult> 문서관리번호조회(
        [FromQuery(Name = "documentManagementNumber")] string 문서관리번호,
        CancellationToken cancellationToken)
    {
        var result = await _선적추적UseCase.공개조회Async(문서관리번호, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("import-logistics-references")]
    public IActionResult 수입물류참조검색(
        [FromQuery(Name = "keyword")] string? 검색어,
        [FromQuery(Name = "transportMode")] string? 운송수단,
        [FromQuery(Name = "codeType")] string? 코드유형,
        [FromQuery(Name = "pageSize")] int 페이지크기 = 20)
    {
        var result = _선적추적UseCase.수입물류참조검색(new 수입물류참조조회요청
        {
            검색어 = 검색어,
            운송수단 = 운송수단,
            코드유형 = 코드유형,
            페이지크기 = 페이지크기
        });

        return this.ToActionResult(result);
    }

    [HttpPost("import-logistics-normalization-simulation")]
    public IActionResult 수입물류정규화시뮬레이션(
        [FromBody] 수입물류정규화시뮬레이션요청 요청)
    {
        var result = _선적추적UseCase.수입물류정규화시뮬레이션(요청);
        return this.ToActionResult(result);
    }

    [HttpGet("lookup-normalized")]
    [SsalddelApiContractName("LookupNormalized")]
    public async Task<IActionResult> 원장기반정규화조회(
        [FromQuery(Name = "documentManagementNumber")] string 문서관리번호,
        [FromQuery(Name = "customsOfficeCode")] string? 세관코드,
        [FromQuery(Name = "customsOfficeName")] string? 세관명,
        [FromQuery(Name = "bondedAreaCode")] string? 보세구역코드,
        [FromQuery(Name = "bondedAreaName")] string? 보세구역명,
        CancellationToken cancellationToken)
    {
        var result = await _선적추적UseCase.원장기반정규화시뮬레이션Async(
            문서관리번호,
            세관코드,
            세관명,
            보세구역코드,
            보세구역명,
            cancellationToken);

        return this.ToActionResult(result);
    }
}
