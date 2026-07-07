using Hongdal.Contracts.Common.Orderer;
using Hongdal.Hubs;
using 홍달.Services.Dispatch.Recommendation;

namespace Hongdal.Tests.Services.Dispatch.Recommendation;

public sealed class 공동구매운송추천표시연결Tests
{
    [Fact]
    public void 공동구매_세대배송_국내운송초안은_기사추천에_공동주문과_세대배송범위를_표시한다()
    {
        var plan = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
                DriverPerformsApartmentUnitDistribution = true,
                ApartmentUnitDistributionModeCode = 공동구매공동주택세대배송방식코드.DriverToUnitDoor,
                ApartmentUnitDeliveryCount = 33,
                ApartmentUnitDistributionPlanConfirmed = true,
                UnitSortationBeforePickupConfirmed = true,
                UnitDemandBreakdownConfirmed = true,
                ImportedProductInfoRegistered = true,
                ProductInfoStorageConfirmed = true,
                UnitProductInfoStickerConfirmed = true,
                ProductInfoStickerMatchesImportedProductConfirmed = true,
                LoadingSequenceConfirmed = true,
                SortedUnitPackageCount = 33,
                RecipientAddressPrivacyConfirmed = true,
                DistributionResponsibilityConfirmed = true,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "인천 보세창고",
                DropoffRoadAddress = "홍달아파트 101동",
                CargoWeightKg = 100m
            });

        var recommendation = new DispatchRecommendationDto();
        DispatchRecommendationRequestTypeClassifier.ApplyTo(recommendation, plan.DispatchQueueDraft);

        Assert.True(plan.ReadyForDispatchQueue);
        Assert.Equal(공동구매국내운송원천의뢰유형코드.ImportCargoTransport, plan.DispatchQueueDraft.SourceRequestType);
        Assert.Equal("GroupPurchaseCargoTransport", recommendation.운송의뢰유형코드);
        Assert.Equal("공동주문 운송", recommendation.운송의뢰유형표시);
        Assert.True(recommendation.공동주문운송여부);
        Assert.True(recommendation.세대배송포함여부);
        Assert.Equal(33, recommendation.세대배송건수);
        Assert.Equal("상하차 + 세대 문앞 33건", recommendation.세대배송업무표시);
    }

    [Fact]
    public void 공동구매_3PL입고_국내운송초안은_공동주문이지만_세대배송은_표시하지_않는다()
    {
        var plan = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                TransportMode = 공동구매국내운송방식코드.LCL,
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ThreePlWarehouse,
                DriverPerformsApartmentUnitDistribution = false,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "평택항 보세구역",
                DropoffRoadAddress = "김포 3PL 입고장",
                CargoWeightKg = 800m
            });

        var recommendation = new DispatchRecommendationDto();
        DispatchRecommendationRequestTypeClassifier.ApplyTo(recommendation, plan.DispatchQueueDraft);

        Assert.True(plan.ReadyForDispatchQueue);
        Assert.Equal(공동구매국내운송원천의뢰유형코드.LclCargoTransport, plan.DispatchQueueDraft.SourceRequestType);
        Assert.Equal("GroupPurchaseCargoTransport", recommendation.운송의뢰유형코드);
        Assert.True(recommendation.공동주문운송여부);
        Assert.False(recommendation.세대배송포함여부);
        Assert.Null(recommendation.세대배송건수);
        Assert.Equal("상하차 + 3PL 입고", recommendation.세대배송업무표시);
    }

    [Fact]
    public void 일반화물_원본의뢰유형은_기존처럼_일반화물로_표시한다()
    {
        var recommendation = new DispatchRecommendationDto();

        DispatchRecommendationRequestTypeClassifier.ApplyTo(recommendation, "CargoTransport");

        Assert.Equal("GeneralCargoTransport", recommendation.운송의뢰유형코드);
        Assert.Equal("일반 화물", recommendation.운송의뢰유형표시);
        Assert.False(recommendation.공동주문운송여부);
        Assert.False(recommendation.세대배송포함여부);
        Assert.Null(recommendation.세대배송건수);
        Assert.Equal("상하차", recommendation.세대배송업무표시);
    }

    [Fact]
    public void 배차대기에_저장된_공동구매_업무범위로_기사추천_표시를_복원한다()
    {
        var recommendation = new DispatchRecommendationDto();

        DispatchRecommendationRequestTypeClassifier.ApplyTo(
            recommendation,
            공동구매국내운송원천의뢰유형코드.ImportCargoTransport,
            공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
            true,
            30);

        Assert.Equal("GroupPurchaseCargoTransport", recommendation.운송의뢰유형코드);
        Assert.True(recommendation.공동주문운송여부);
        Assert.True(recommendation.세대배송포함여부);
        Assert.Equal(30, recommendation.세대배송건수);
        Assert.Equal("상하차 + 세대 문앞 30건", recommendation.세대배송업무표시);
    }

    private static 공동구매커머스이행계획Dto CreateFulfillment계획()
        => new()
        {
            계획Id = "plan-1",
            공동구매Id = "gp-1",
            주문자집단배송권키 = "orderer-group:apt-1",
            주문자집단배송권명 = "홍달아파트 공동주문 집단",
            문서관리번호 = "HD-GP-IMPORT-2026-0001",
            상품명 = "공동구매 수입 삼겹살",
            Sku = "GP-IMPORT-PORK-001",
            예상입고수량 = 33
        };
}
