using Ssalddel.Controllers;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.Orderer;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Filters;
using Microsoft.AspNetCore.Mvc;
using 살뜰.Services.Versioning;

namespace Ssalddel.Controllers.Orderer;

[ApiController]
[SsalddelApiVersion(SsalddelProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
[RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
[Route("api/v1/orderer/group-purchase-overseas-shipments")]
public sealed class 공동구매해외선적추적Controller : ControllerBase
{
    private readonly I공동구매해외선적추적UseCase _useCase;

    public 공동구매해외선적추적Controller(I공동구매해외선적추적UseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup(
        [FromQuery] string documentManagementNumber,
        CancellationToken cancellationToken)
    {
        var result = await _useCase.공개조회Async(documentManagementNumber, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("import-logistics-references")]
    public IActionResult 수입물류참조검색(
        [FromQuery] string? keyword,
        [FromQuery] string? transportMode,
        [FromQuery] string? codeType,
        [FromQuery] int pageSize = 20)
    {
        var result = _useCase.수입물류참조검색(new 수입물류참조조회요청
        {
            검색어 = keyword,
            운송수단 = transportMode,
            코드유형 = codeType,
            페이지크기 = pageSize
        });

        return this.ToActionResult(result);
    }

    [HttpPost("import-logistics-normalization-simulation")]
    public IActionResult 수입물류정규화시뮬레이션(
        [FromBody] 수입물류정규화시뮬레이션요청 request)
    {
        var result = _useCase.수입물류정규화시뮬레이션(request);
        return this.ToActionResult(result);
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
        var result = await _useCase.원장기반정규화시뮬레이션Async(
            documentManagementNumber,
            customsOfficeCode,
            customsOfficeName,
            bondedAreaCode,
            bondedAreaName,
            cancellationToken);

        return this.ToActionResult(result);
    }
}
