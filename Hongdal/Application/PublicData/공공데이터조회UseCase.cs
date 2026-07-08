using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.PublicData;
using Microsoft.AspNetCore.Http;
using 홍달.Services.External.PublicData;
using 홍달.Services.Versioning;

namespace Hongdal.Application.PublicData;

public interface I공공데이터조회UseCase
{
    Task<Result<PublicDataLookupResponse<RoadAddressItem>>> 도로명주소검색Async(
        string keyword,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Result<PublicDataLookupResponse<주문자집단배송권후보항목>> 주문자집단배송권검색(
        주문자집단배송권조회요청 request);

    Task<Result<PublicDataLookupResponse<ApartmentComplexItem>>> 공동주택단지검색Async(
        ApartmentComplexSearchRequest request,
        CancellationToken cancellationToken);

    Task<Result<PublicDataLookupResponse<ApartmentComplexBasicItem>>> 공동주택기본정보조회Async(
        string complexCode,
        CancellationToken cancellationToken);

    Task<Result<PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem>>> 관리비스냅샷조회Async(
        string complexCode,
        string month,
        CancellationToken cancellationToken);

    Task<Result<ApartmentGroupCommerceOffsetSimulationResult>> 공동커머스관리비상쇄시뮬레이션Async(
        ApartmentGroupCommerceOffsetSimulationRequest request,
        CancellationToken cancellationToken);

    Task<Result<HsCountryImportUnitPriceSimulationResult>> 수입평균단가시뮬레이션Async(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken);
}

[HongdalApiWorkflow(HongdalWorkflow.GroupPurchaseImport)]
[HongdalUseCase("공공 데이터 조회", Summary = "주문자 앱에서 주소, 공동주택, 관리비, 통관 평균 단가 데이터를 조회해 공동주문 판단 자료로 사용합니다.")]
[HongdalUseCaseActor(HongdalActor.Orderer)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
public sealed class 공공데이터조회UseCase : I공공데이터조회UseCase
{
    private readonly IRoadAddressLookupService _roadAddressLookupService;
    private readonly IApartmentComplexLookupService _apartmentComplexLookupService;
    private readonly IApartmentManagementFeeLookupService _apartmentManagementFeeLookupService;
    private readonly I주문자집단배송권조회Service _ordererGroupScopeLookupService;
    private readonly IHsCountryTradeUnitPriceLookupService _hsCountryTradeUnitPriceLookupService;

    public 공공데이터조회UseCase(
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

    public async Task<Result<PublicDataLookupResponse<RoadAddressItem>>> 도로명주소검색Async(
        string keyword,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await _roadAddressLookupService.SearchAsync(new RoadAddressSearchRequest
        {
            Keyword = keyword,
            Page = page,
            PageSize = pageSize
        }, cancellationToken);

        return Result.Ok(result);
    }

    public Result<PublicDataLookupResponse<주문자집단배송권후보항목>> 주문자집단배송권검색(
        주문자집단배송권조회요청 request)
    {
        return Result.Ok(_ordererGroupScopeLookupService.후보검색(request));
    }

    public async Task<Result<PublicDataLookupResponse<ApartmentComplexItem>>> 공동주택단지검색Async(
        ApartmentComplexSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _apartmentComplexLookupService.SearchAsync(request, cancellationToken);
        return Result.Ok(result);
    }

    public async Task<Result<PublicDataLookupResponse<ApartmentComplexBasicItem>>> 공동주택기본정보조회Async(
        string complexCode,
        CancellationToken cancellationToken)
    {
        var result = await _apartmentComplexLookupService.GetBasicInfoAsync(new ApartmentComplexBasicRequest
        {
            ComplexCode = complexCode
        }, cancellationToken);

        return Result.Ok(result);
    }

    public async Task<Result<PublicDataLookupResponse<ApartmentManagementFeeSnapshotItem>>> 관리비스냅샷조회Async(
        string complexCode,
        string month,
        CancellationToken cancellationToken)
    {
        var result = await _apartmentManagementFeeLookupService.GetSnapshotAsync(new ApartmentManagementFeeSnapshotRequest
        {
            ComplexCode = complexCode,
            Month = month
        }, cancellationToken);

        return Result.Ok(result);
    }

    public async Task<Result<ApartmentGroupCommerceOffsetSimulationResult>> 공동커머스관리비상쇄시뮬레이션Async(
        ApartmentGroupCommerceOffsetSimulationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _apartmentManagementFeeLookupService.SimulateGroupCommerceOffsetAsync(request, cancellationToken);
        return Result.Ok(result);
    }

    public async Task<Result<HsCountryImportUnitPriceSimulationResult>> 수입평균단가시뮬레이션Async(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _hsCountryTradeUnitPriceLookupService.SimulateImportUnitPriceAsync(request, cancellationToken);
            return Result.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Ok(new HsCountryImportUnitPriceSimulationResult
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
