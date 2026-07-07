using Hongdal.Contracts.Common.PublicData;
using Hongdal.Filters;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.External.PublicData;
using 홍달.Services.Versioning;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Orderer;

[HongdalApiVersion(HongdalProductVersion.V1_0)]
[ApiController]
[Route("api/v1/orderer/public-data")]
public sealed class PublicDataLookupController : ControllerBase
{
    private readonly IRoadAddressLookupService _roadAddressLookupService;
    private readonly IApartmentComplexLookupService _apartmentComplexLookupService;
    private readonly IApartmentManagementFeeLookupService _apartmentManagementFeeLookupService;
    private readonly I주문자집단배송권조회Service _ordererGroupScopeLookupService;
    private readonly IHsCountryTradeUnitPriceLookupService _hsCountryTradeUnitPriceLookupService;

    public PublicDataLookupController(
        IRoadAddressLookupService roadAddressLookupService,
        IApartmentComplexLookupService apartmentComplexLookupService,
        IApartmentManagementFeeLookupService apartmentManagementFeeLookupService,
        I주문자집단배송권조회Service ordererGroupScopeLookupService,
        IHsCountryTradeUnitPriceLookupService hsCountryTradeUnitPriceLookupService)
    {
        _roadAddressLookupService = roadAddressLookupService;
        _apartmentComplexLookupService = apartmentComplexLookupService;
        _apartmentManagementFeeLookupService = apartmentManagementFeeLookupService;
        _ordererGroupScopeLookupService = ordererGroupScopeLookupService;
        _hsCountryTradeUnitPriceLookupService = hsCountryTradeUnitPriceLookupService;
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<PublicDataLookupResponse<RoadAddressItem>>> SearchAddresses(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _roadAddressLookupService.SearchAsync(new RoadAddressSearchRequest
        {
            Keyword = keyword,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    [HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HttpGet("orderer-group-scopes")]
    public ActionResult<PublicDataLookupResponse<주문자집단배송권후보항목>> 주문자집단배송권검색(
        [FromQuery] string? roadAddress,
        [FromQuery] string? jibunAddress,
        [FromQuery] string? kakaoRegionLevel1,
        [FromQuery] string? kakaoRegionLevel2,
        [FromQuery] string? kakaoRegionLevel3,
        [FromQuery] int pageSize = 5)
    {
        var result = _ordererGroupScopeLookupService.후보검색(new 주문자집단배송권조회요청
        {
            RoadAddress = roadAddress,
            JibunAddress = jibunAddress,
            KakaoRegionLevel1 = kakaoRegionLevel1,
            KakaoRegionLevel2 = kakaoRegionLevel2,
            KakaoRegionLevel3 = kakaoRegionLevel3,
            PageSize = pageSize
        });

        return Ok(result);
    }

    [HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HttpGet("apartment-complexes")]
    public async Task<ActionResult<PublicDataLookupResponse<ApartmentComplexItem>>> SearchApartmentComplexes(
        [FromQuery] string? sidoCode,
        [FromQuery] string? sigunguCode,
        [FromQuery] string? eupmyeondongCode,
        [FromQuery] string? roadName,
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _apartmentComplexLookupService.SearchAsync(new ApartmentComplexSearchRequest
        {
            SidoCode = sidoCode,
            SigunguCode = sigunguCode,
            EupmyeondongCode = eupmyeondongCode,
            RoadName = roadName,
            Keyword = keyword,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Ok(result);
    }

    [HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HttpGet("apartment-complexes/{complexCode}/basic")]
    public async Task<ActionResult<PublicDataLookupResponse<ApartmentComplexBasicItem>>> GetApartmentComplexBasicInfo(
        string complexCode,
        CancellationToken cancellationToken)
    {
        var result = await _apartmentComplexLookupService.GetBasicInfoAsync(new ApartmentComplexBasicRequest
        {
            ComplexCode = complexCode
        }, cancellationToken);

        return Ok(result);
    }

    [HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HttpGet("apartment-complexes/{complexCode}/management-fee-snapshot")]
    public async Task<ActionResult<PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem>>> GetApartmentManagementFeeSnapshot(
        string complexCode,
        [FromQuery] string month,
        CancellationToken cancellationToken)
    {
        var result = await _apartmentManagementFeeLookupService.GetSnapshotAsync(new ApartmentManagementFeeSnapshotRequest
        {
            ComplexCode = complexCode,
            Month = month
        }, cancellationToken);

        return Ok(result);
    }

    [HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HttpPost("apartment-complexes/group-commerce-offset-simulation")]
    public async Task<ActionResult<ApartmentGroupCommerceOffsetSimulationResult>> SimulateGroupCommerceOffset(
        [FromBody] ApartmentGroupCommerceOffsetSimulationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _apartmentManagementFeeLookupService.SimulateGroupCommerceOffsetAsync(request, cancellationToken);
        return Ok(result);
    }

    [HongdalApiVersion(HongdalProductVersion.V2_5, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseImportWorkflow)]
    [HttpPost("customs/hs-country-import-unit-price-simulation")]
    public async Task<ActionResult<HsCountryImportUnitPriceSimulationResult>> SimulateHsCountryImportUnitPrice(
        [FromBody] HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _hsCountryTradeUnitPriceLookupService.SimulateImportUnitPriceAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Ok(new HsCountryImportUnitPriceSimulationResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                HsCode = request.HsCode,
                CountryCode = request.CountryCode,
                EndMonth = request.Month,
                Summary = ex.Message
            });
        }
    }
}
