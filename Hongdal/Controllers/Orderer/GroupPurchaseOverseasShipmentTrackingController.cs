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
public sealed class GroupPurchaseOverseasShipmentTrackingController : ControllerBase
{
    private readonly IGroupPurchaseOverseasShipmentTrackingStore _store;
    private readonly IGroupPurchaseImportLogisticsNormalizationService _normalizationService;

    public GroupPurchaseOverseasShipmentTrackingController(
        IGroupPurchaseOverseasShipmentTrackingStore store,
        IGroupPurchaseImportLogisticsNormalizationService normalizationService)
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
            var item = await _store.GetByDocumentManagementNumberAsync(documentManagementNumber, cancellationToken);
            return item is null
                ? this.ToNotFoundProblem("문서관리번호에 해당하는 공동주문 해외 선적 정보를 찾을 수 없습니다.")
                : Ok(GroupPurchaseOverseasShipmentTrackingProjection.ToPublicDto(item));
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "문서관리번호가 올바르지 않습니다.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }

    [HttpGet("import-logistics-references")]
    public ActionResult<IReadOnlyList<ImportLogisticsReferenceItem>> SearchImportLogisticsReferences(
        [FromQuery] string? keyword,
        [FromQuery] string? transportMode,
        [FromQuery] string? codeType,
        [FromQuery] int pageSize = 20)
    {
        var items = _normalizationService.SearchReferences(new ImportLogisticsReferenceLookupRequest
        {
            Keyword = keyword,
            TransportMode = transportMode,
            CodeType = codeType,
            PageSize = pageSize
        });

        return Ok(items);
    }

    [HttpPost("import-logistics-normalization-simulation")]
    public ActionResult<ImportLogisticsNormalizationSimulationResult> SimulateImportLogisticsNormalization(
        [FromBody] ImportLogisticsNormalizationSimulationRequest request)
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
            var item = await _store.GetByDocumentManagementNumberAsync(documentManagementNumber, cancellationToken);
            if (item is null)
            {
                return this.ToNotFoundProblem("Document management number was not found in the group purchase overseas shipment ledger.");
            }

            var currentLocation = item.Events.LastOrDefault()?.LocationSummary ?? item.CurrentLocationSummary;
            var result = _normalizationService.Simulate(new ImportLogisticsNormalizationSimulationRequest
            {
                DocumentManagementNumber = item.DocumentManagementNumber,
                TransportDocumentType = item.TransportDocumentType,
                TransportDocumentNumber = item.TransportDocumentNumber,
                TransportMode = item.TransportMode,
                OriginCountryCode = item.OriginCountryCode,
                OriginPortCode = item.OriginPortCode,
                DestinationPortCode = item.DestinationPortCode,
                DestinationPortOrAirportName = item.CurrentLocationSummary,
                CustomsOfficeCode = customsOfficeCode ?? string.Empty,
                CustomsOfficeName = customsOfficeName ?? string.Empty,
                BondedAreaCode = bondedAreaCode ?? string.Empty,
                BondedAreaName = bondedAreaName ?? string.Empty,
                CurrentLocationSummary = currentLocation,
                CustomsStageName = item.CurrentStatusCode
            });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Problem(title: "Document management number is invalid.", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
    }
}
