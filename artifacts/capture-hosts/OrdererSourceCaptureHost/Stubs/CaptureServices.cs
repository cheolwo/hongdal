using OrdererApp.Services;
using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace OrdererApp.Services;

internal sealed class CaptureOrdererAuthenticationService : I주문자앱인증Service
{
    public Task<주문자앱인증결과> 복원Async(CancellationToken cancellationToken = default)
        => Task.FromResult(new 주문자앱인증결과(주문자앱세션상태.익명));

    public Task<주문자앱인증결과> 로그인Async(
        string userNameOrEmail,
        string password,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new 주문자앱인증결과(
            new 주문자앱세션상태(true, "capture-orderer", "검증 주문자")));

    public Task 로그아웃Async(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

internal sealed class CaptureGroupPurchaseService : IGroupPurchaseShipmentTrackingService
{
    private const string GroupId = "group-hs-food-0203-kr-41117";

    public Task<공동구매해외선적공개Dto?> LookupAsync(
        string documentManagementNumber,
        CancellationToken cancellationToken = default)
        => Task.FromResult<공동구매해외선적공개Dto?>(null);

    public Task<HsCountryImportUnitPriceSimulationResult?> SimulateImportUnitPriceAsync(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult<HsCountryImportUnitPriceSimulationResult?>(null);

    public Task<OperatingMarketRuntimeProfileResponse?> GetOperatingMarketAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<OperatingMarketRuntimeProfileResponse?>(new()
        {
            MarketCode = OperatingMarketCodes.Korea,
            CountryCode = OperatingMarketCodes.Korea,
            AddressProviderCode = OperatingAddressProviderCodes.KoreaRoadNameAddress,
            MapProviderCode = OperatingMapProviderCodes.NaverMaps
        });

    public Task<OperatingMarketDeliveryScopePlan?> ResolveDeliveryScopesAsync(
        OperatingMarketDeliveryScopeResolveRequest request,
        CancellationToken cancellationToken = default)
        => Task.FromResult<OperatingMarketDeliveryScopePlan?>(new()
        {
            Success = true,
            MarketCode = OperatingMarketCodes.Korea,
            RecommendedScopeKey = "kr-admin2:41117",
            RecommendedDemandConsolidationScopeKey = "kr-admin2:41117",
            ProviderCode = OperatingAddressProviderCodes.KoreaRoadNameAddress,
            Items =
            [
                new OperatingMarketDeliveryScopeCandidate
                {
                    MarketCode = OperatingMarketCodes.Korea,
                    ScopeKey = "kr-admin2:41117",
                    ScopeTypeCode = OperatingDeliveryScopeTypeCodes.AdministrativeLevel2Recruitment,
                    DisplayName = "경기도 수원시 영통구 주문자 집단권",
                    IsRecommendedRecruitmentScope = true,
                    IsRecommendedDemandConsolidationScope = true,
                    MinimumParticipantsForPublicDisplay = 3,
                    SupportsLastMileBatching = true
                },
                new OperatingMarketDeliveryScopeCandidate
                {
                    MarketCode = OperatingMarketCodes.Korea,
                    ScopeKey = "kr-admin3:4111710500",
                    ScopeTypeCode = OperatingDeliveryScopeTypeCodes.AdministrativeLevel3Delivery,
                    DisplayName = "경기도 수원시 영통구 이의동 세부 주문자 집단권",
                    ParentScopeKey = "kr-admin2:41117",
                    IsFineGrained = true,
                    MinimumParticipantsForPublicDisplay = 5,
                    SupportsLastMileBatching = true
                }
            ]
        });

    public Task<공동구매자동집단사용자응답?> RegisterDemandAsync(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default)
        => Task.FromResult<공동구매자동집단사용자응답?>(new()
        {
            자동집단Id = GroupId,
            상품키 = request.상품키,
            상품명 = request.상품명,
            배송권키 = request.배송권키,
            배송권명 = request.배송권명,
            현재상태 = 공동구매자동집단상태코드.수요수집중,
            수요건수 = 4,
            총희망수량 = 85m,
            수량단위 = request.수량단위
        });

    public Task<IReadOnlyList<공동구매자동집단요약응답>?> ListGroupsAsync(
        string productKey,
        string deliveryScopeKey,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<공동구매자동집단요약응답>?>(
        [
            new 공동구매자동집단요약응답
            {
                자동집단Id = GroupId,
                상품키 = productKey,
                상품명 = "냉동 삼겹살",
                배송권키 = deliveryScopeKey,
                배송권명 = "경기도 수원시 영통구 주문자 집단권",
                현재상태 = 공동구매자동집단상태코드.수요수집중,
                수요건수 = 4,
                총희망수량 = 85m,
                수량단위 = "kg"
            }
        ]);
}
