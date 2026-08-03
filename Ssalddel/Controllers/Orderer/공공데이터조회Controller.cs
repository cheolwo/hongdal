using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Filters;
using Ssalddel.Application.PublicData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Ssalddel.Contracts.Common.Orderer;
using 살뜰.Services.Versioning;
using Ssalddel.ApiMetadata;

namespace Ssalddel.Controllers.Orderer;

[SsalddelApiVersion(SsalddelProductVersion.V0_0)]
[SsalddelApiCapability(SsalddelCapability.PublicDataDiscovery)]
[SsalddelApiOperation(SsalddelOperation.Browse)]
[ApiController]
[SsalddelApiContractName("PublicDataLookupController")]
[Route("api/v1/orderer/public-data")]
public sealed class 공공데이터조회Controller : OrdererControllerBase
{
    private readonly I공공데이터조회UseCase _공공데이터조회UseCase;

    public 공공데이터조회Controller(I공공데이터조회UseCase 공공데이터조회UseCase)
    {
        _공공데이터조회UseCase = 공공데이터조회UseCase;
    }

    [HttpGet("addresses")]
    [SsalddelApiContractName("SearchAddresses")]
    public async Task<IActionResult> 도로명주소검색(
        [FromQuery(Name = "keyword")] string 검색어,
        [FromQuery(Name = "page")] int 페이지 = 1,
        [FromQuery(Name = "pageSize")] int 페이지크기 = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _공공데이터조회UseCase.도로명주소검색Async(검색어, 페이지, 페이지크기, cancellationToken);
        return this.ToActionResult(result);
    }

    [SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [HttpGet("orderer-group-scopes")]
    public IActionResult 주문자집단배송권검색(
        [FromQuery(Name = "roadAddress")] string? 도로명주소,
        [FromQuery(Name = "jibunAddress")] string? 지번주소,
        [FromQuery(Name = "kakaoRegionLevel1")] string? 카카오광역지역,
        [FromQuery(Name = "kakaoRegionLevel2")] string? 카카오기초지역,
        [FromQuery(Name = "kakaoRegionLevel3")] string? 카카오읍면동,
        [FromQuery(Name = "pageSize")] int 페이지크기 = 5)
    {
        var result = _공공데이터조회UseCase.주문자집단배송권검색(new 주문자집단배송권조회요청
        {
            RoadAddress = 도로명주소,
            JibunAddress = 지번주소,
            KakaoRegionLevel1 = 카카오광역지역,
            KakaoRegionLevel2 = 카카오기초지역,
            KakaoRegionLevel3 = 카카오읍면동,
            PageSize = 페이지크기
        });

        return this.ToActionResult(result);
    }

    [SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [HttpPost("group-purchase/delivery-scopes/resolve")]
    [SsalddelApiContractName("공동주문배송권해결")]
    public async Task<IActionResult> 공동구매배송권해결(
        [FromBody] OperatingMarketDeliveryScopeResolveRequest 요청,
        CancellationToken cancellationToken)
    {
        var result = await _공공데이터조회UseCase.공동주문배송권해결Async(요청, cancellationToken);
        return this.ToActionResult(result);
    }

    [SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [HttpGet("apartment-complexes")]
    [SsalddelApiContractName("SearchApartmentComplexes")]
    public async Task<IActionResult> 공동주택단지검색(
        [FromQuery(Name = "sidoCode")] string? 시도코드,
        [FromQuery(Name = "sigunguCode")] string? 시군구코드,
        [FromQuery(Name = "eupmyeondongCode")] string? 읍면동코드,
        [FromQuery(Name = "roadName")] string? 도로명,
        [FromQuery(Name = "keyword")] string? 검색어,
        [FromQuery(Name = "page")] int 페이지 = 1,
        [FromQuery(Name = "pageSize")] int 페이지크기 = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _공공데이터조회UseCase.공동주택단지검색Async(new ApartmentComplexSearchRequest
        {
            SidoCode = 시도코드,
            SigunguCode = 시군구코드,
            EupmyeondongCode = 읍면동코드,
            RoadName = 도로명,
            Keyword = 검색어,
            Page = 페이지,
            PageSize = 페이지크기
        }, cancellationToken);

        return this.ToActionResult(result);
    }

    [SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [HttpGet("apartment-complexes/{complexCode}/basic")]
    [SsalddelApiContractName("GetApartmentComplexBasicInfo")]
    public async Task<IActionResult> 공동주택기본정보조회(
        [FromRoute(Name = "complexCode")] string 단지코드,
        CancellationToken cancellationToken)
    {
        var result = await _공공데이터조회UseCase.공동주택기본정보조회Async(단지코드, cancellationToken);
        return this.ToActionResult(result);
    }

    [SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [HttpGet("apartment-complexes/{complexCode}/management-fee-snapshot")]
    [SsalddelApiContractName("GetApartmentManagementFeeSnapshot")]
    public async Task<IActionResult> 관리비스냅샷조회(
        [FromRoute(Name = "complexCode")] string 단지코드,
        [FromQuery(Name = "month")] string 기준월,
        CancellationToken cancellationToken)
    {
        var result = await _공공데이터조회UseCase.관리비스냅샷조회Async(단지코드, 기준월, cancellationToken);
        return this.ToActionResult(result);
    }

    [Authorize]
    [SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [HttpPost("apartment-complexes/selected/public-data-snapshots")]
    [SsalddelApiContractName("ArchiveSelectedApartmentPublicDataSnapshot")]
    public async Task<IActionResult> 선택단지공공정보적재(
        [FromBody] SelectedApartmentPublicDataArchiveRequest 요청,
        CancellationToken cancellationToken)
    {
        var scopeKey = User.FindFirstValue(주문자집단배송권ClaimTypes.ScopeKey);
        var complexCode = User.FindFirstValue(주문자집단배송권ClaimTypes.ApartmentComplexCode);
        var complexName = User.FindFirstValue(주문자집단배송권ClaimTypes.ApartmentComplexName);
        if (string.IsNullOrWhiteSpace(scopeKey)
            || string.IsNullOrWhiteSpace(complexCode)
            || string.IsNullOrWhiteSpace(complexName))
        {
            return Forbid();
        }

        var result = await _공공데이터조회UseCase.선택단지공공정보적재Async(
            scopeKey,
            complexCode,
            complexName,
            요청.Month,
            cancellationToken);
        return this.ToActionResult(result);
    }

    [SsalddelApiVersion(SsalddelProductVersion.V1_0, FeatureKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow, WorkflowKey = VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.GroupPurchaseDemandWorkflow)]
    [HttpPost("apartment-complexes/group-commerce-offset-simulation")]
    [SsalddelApiContractName("SimulateGroupCommerceOffset")]
    public async Task<IActionResult> 공동커머스관리비상쇄시뮬레이션(
        [FromBody] ApartmentGroupCommerceOffsetSimulationRequest 요청,
        CancellationToken cancellationToken)
    {
        var result = await _공공데이터조회UseCase.공동커머스관리비상쇄시뮬레이션Async(요청, cancellationToken);
        return this.ToActionResult(result);
    }

    [SsalddelApiVersion(SsalddelProductVersion.V1_5, FeatureKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow, WorkflowKey = VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
    [SsalddelApiWorkflow(SsalddelWorkflow.GroupPurchaseImport)]
    [RequireVersionFeature(VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow)]
    [HttpPost("customs/hs-country-import-unit-price-simulation")]
    [SsalddelApiContractName("SimulateHsCountryImportUnitPrice")]
    public async Task<IActionResult> 국가별수입평균단가시뮬레이션(
        [FromBody] HsCountryMonthlyTradeUnitPriceRequest 요청,
        CancellationToken cancellationToken)
    {
        var result = await _공공데이터조회UseCase.수입평균단가시뮬레이션Async(요청, cancellationToken);
        return this.ToActionResult(result);
    }
}
