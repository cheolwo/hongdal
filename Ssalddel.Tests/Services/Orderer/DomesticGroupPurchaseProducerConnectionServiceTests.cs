using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class DomesticGroupPurchaseProducerConnectionServiceTests
{
    [Fact]
    public async Task SearchCandidatesAsync_UnconnectedDirectory_ReturnsNoMembersOrContactDetails()
    {
        var service = CreateService();

        var result = await service.SearchCandidatesAsync(null, null, null);

        Assert.Equal(DomesticProducerDirectoryIntegrationStatuses.NotConnected, result.IntegrationStatusCode);
        Assert.Empty(result.Items);
        Assert.False(result.ContactDetailsDisclosed);
    }

    [Fact]
    public async Task CreateDraftAsync_KeepsContactPrivate_AndLimitsReadToOwner()
    {
        var service = CreateService();
        var campaignId = Guid.NewGuid();

        var created = await service.CreateDraftAsync(
            "buyer-1",
            new DomesticProducerContactRequestDraftRequest
            {
                GroupPurchaseCampaignId = campaignId,
                CampaignTitle = "고구마 공동구매",
                ProducerCandidateKey = "producer-1",
                ProducerMaskedDisplayName = "김○○",
                ProductSummary = "고구마",
                RequestedQuantitySummary = "100kg",
                RequiredPackagingFormCode = DomesticProducePackagingFormCodes.CorrugatedBox,
                PackagingUnitSummary = "10kg 골판지 상자",
                QualityGradeSummary = "혼합 크기 허용, 파손 제외",
                RequestedQuantity = 100,
                MaximumAbsorptionQuantity = 150,
                QuantityUnit = "kg",
                CanReceiveSplitShipments = true,
                Message = "공급 가능 여부를 협의하고 싶습니다."
            });

        Assert.Equal(DomesticProducerContactRequestStatuses.Draft, created.StatusCode);
        Assert.False(created.ContactDetailsDisclosed);
        Assert.False(created.IsDurablyPersisted);
        Assert.Equal("고구마", created.ProductSummary);
        Assert.Equal("공급 가능 여부를 협의하고 싶습니다.", created.Message);
        Assert.NotNull(await service.GetDraftAsync("buyer-1", created.DraftId));
        Assert.Null(await service.GetDraftAsync("buyer-2", created.DraftId));
    }

    [Fact]
    public async Task CreateDraftAsync_RejectsBuyerRequestThatExceedsAbsorptionCapacity()
    {
        var service = CreateService();

        var error = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateDraftAsync(
            "buyer-1",
            new DomesticProducerContactRequestDraftRequest
            {
                GroupPurchaseCampaignId = Guid.NewGuid(),
                ProducerCandidateKey = "producer-1",
                RequiredPackagingFormCode = DomesticProducePackagingFormCodes.CorrugatedBox,
                PackagingUnitSummary = "10kg 골판지 상자",
                QualityGradeSummary = "혼합 크기 허용",
                RequestedQuantity = 500,
                MaximumAbsorptionQuantity = 300,
                QuantityUnit = "kg",
                Message = "500kg 출하를 요청합니다."
            }));

        Assert.Contains("최대 인수 물량", error.Message);
    }

    [Fact]
    public async Task SearchRepresentativesAsync_UnconnectedDirectory_ReturnsNoContactDetails()
    {
        var service = CreateService();

        var result = await service.SearchRepresentativesAsync(null, null, null);

        Assert.Equal(DomesticProducerDirectoryIntegrationStatuses.NotConnected, result.IntegrationStatusCode);
        Assert.Empty(result.Items);
        Assert.False(result.ContactDetailsDisclosed);
    }

    [Fact]
    public async Task CreateSupplyOfferDraftAsync_RequiresSafetyConfirmation()
    {
        var service = CreateService();
        var request = CreateSupplyOfferRequest();
        request.FoodSafetyConfirmed = false;

        var error = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateSupplyOfferDraftAsync("producer-1", request));

        Assert.Contains("안전 확인", error.Message);
    }

    [Fact]
    public async Task CreateSupplyOfferDraftAsync_KeepsQualityDisclosure_AndLimitsReadToOwner()
    {
        var service = CreateService();
        var request = CreateSupplyOfferRequest();

        var created = await service.CreateSupplyOfferDraftAsync("producer-1", request);

        Assert.Equal(DomesticProducerContactRequestStatuses.Draft, created.StatusCode);
        Assert.True(created.FoodSafetyConfirmed);
        Assert.Equal("크기가 고르지 않지만 파손과 부패는 없습니다.", created.QualityDisclosure);
        Assert.False(created.ContactDetailsDisclosed);
        Assert.False(created.IsDurablyPersisted);
        Assert.NotNull(await service.GetSupplyOfferDraftAsync("producer-1", created.DraftId));
        Assert.Null(await service.GetSupplyOfferDraftAsync("producer-2", created.DraftId));
    }

    [Fact]
    public async Task CreateSupplyOfferDraftAsync_긴급수확연결은_생산자보호조건을_저장하고_자동구매를_금지한다()
    {
        var service = CreateService();
        var request = CreateUrgentHarvestOfferRequest();

        var created = await service.CreateSupplyOfferDraftAsync(
            "producer-1",
            request);

        Assert.True(created.IsUrgentHarvestConnection);
        Assert.True(created.StandingCropBulkTransferRequested);
        Assert.Equal(
            DomesticProducerSupplyOfferReasonCodes.CropDestructionRisk,
            created.OfferReasonCode);
        Assert.Equal(1_200m, created.MinimumProducerSettlementAmountPerUnit);
        Assert.Equal("KRW", created.SettlementCurrencyCode);
        Assert.True(created.WrittenAgreementRequired);
        Assert.False(created.AutoPurchaseAllowed);
        Assert.False(created.AutoPriceReductionAllowed);
        Assert.Contains("갈아엎기", created.EmergencyReasonEvidenceSummary);
    }

    [Fact]
    public async Task CreateSupplyOfferDraftAsync_긴급수확연결은_최소정산단가와_소유권조건을_요구한다()
    {
        var service = CreateService();
        var request = CreateUrgentHarvestOfferRequest();
        request.MinimumProducerSettlementAmountPerUnit = 0;

        var missingPriceFloor = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateSupplyOfferDraftAsync("producer-1", request));
        Assert.Contains("최소 정산 단가", missingPriceFloor.Message);

        request.MinimumProducerSettlementAmountPerUnit = 1_200m;
        request.OwnershipTransferConditionSummary = string.Empty;
        var missingOwnership = await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateSupplyOfferDraftAsync("producer-1", request));
        Assert.Contains("소유권 이전", missingOwnership.Message);
    }

    [Fact]
    public void PreviewCompatibility_WhenPackagingAndSplitVolumeFit_IsMutuallyFeasible()
    {
        var service = CreateService();

        var result = service.PreviewCompatibility(new DomesticGroupPurchaseSupplyCompatibilityPreviewRequest
        {
            BuyerRequiredPackagingFormCode = DomesticProducePackagingFormCodes.CorrugatedBox,
            BuyerRequestedQuantity = 500,
            BuyerMaximumAbsorptionQuantity = 800,
            BuyerCanReceiveSplitShipments = true,
            ProducerSupportedPackagingFormCodes = [DomesticProducePackagingFormCodes.CorrugatedBox],
            ProducerAvailableQuantity = 1_000,
            ProducerMinimumTakeQuantity = 300,
            ProducerCanSplitShipments = true,
            QuantityUnit = "kg"
        });

        Assert.True(result.ProducerCanMeetPackaging);
        Assert.True(result.ProducerCanMeetRequestedQuantity);
        Assert.True(result.BuyerMeetsMinimumTakeQuantity);
        Assert.False(result.BuyerCanAbsorbFullOffer);
        Assert.True(result.SplitShipmentCanResolveVolumeGap);
        Assert.True(result.IsMutuallyFeasible);
        Assert.Empty(result.UnresolvedConditions);
    }

    [Fact]
    public void PreviewCompatibility_WhenPackagingAndBuyerCapacityDoNotFit_ListsBothConditions()
    {
        var service = CreateService();

        var result = service.PreviewCompatibility(new DomesticGroupPurchaseSupplyCompatibilityPreviewRequest
        {
            BuyerRequiredPackagingFormCode = DomesticProducePackagingFormCodes.CorrugatedBox,
            BuyerRequestedQuantity = 500,
            BuyerMaximumAbsorptionQuantity = 100,
            ProducerSupportedPackagingFormCodes = [DomesticProducePackagingFormCodes.Bulk],
            ProducerAvailableQuantity = 1_000,
            ProducerMinimumTakeQuantity = 300,
            ProducerCanSplitShipments = false,
            QuantityUnit = "kg"
        });

        Assert.False(result.ProducerCanMeetPackaging);
        Assert.False(result.BuyerMeetsMinimumTakeQuantity);
        Assert.False(result.IsMutuallyFeasible);
        Assert.Contains(result.UnresolvedConditions, x => x.Contains("포장 형태", StringComparison.Ordinal));
        Assert.Contains(result.UnresolvedConditions, x => x.Contains("최소 인수", StringComparison.Ordinal));
    }

    [Fact]
    public void PreviewUrgentHarvestConnection_보호단가와_역할이_확인되면_비구속검토를_허용한다()
    {
        var service = CreateService();

        var result = service.PreviewUrgentHarvestConnection(
            CreateUrgentHarvestPreviewRequest());

        Assert.True(result.EligibleForUrgentReview);
        Assert.True(result.ProducerPriceFloorProtected);
        Assert.True(result.ResponsibilitiesDefined);
        Assert.True(result.RequiresWrittenAgreement);
        Assert.False(result.AutoPurchaseAllowed);
        Assert.False(result.AutoPriceReductionAllowed);
        Assert.False(result.UrgencyOverridesConsent);
        Assert.Empty(result.UnresolvedConditions);
    }

    [Fact]
    public void PreviewUrgentHarvestConnection_헐값과_미정책임은_검토조건으로_남긴다()
    {
        var service = CreateService();
        var request = CreateUrgentHarvestPreviewRequest();
        request.BuyerMaximumAmountPerUnit = 900m;
        request.HarvestLaborResponsibilityCode =
            DomesticUrgentHarvestLaborResponsibilityCodes.ToBeAgreed;

        var result = service.PreviewUrgentHarvestConnection(request);

        Assert.False(result.EligibleForUrgentReview);
        Assert.False(result.ProducerPriceFloorProtected);
        Assert.False(result.ResponsibilitiesDefined);
        Assert.Contains(
            result.UnresolvedConditions,
            condition => condition.Contains("최소 정산 단가", StringComparison.Ordinal));
        Assert.Contains(
            result.UnresolvedConditions,
            condition => condition.Contains("수확 노동", StringComparison.Ordinal));
    }

    private static DomesticProducerSupplyOfferDraftRequest CreateSupplyOfferRequest()
        => new()
        {
            GroupPurchaseCampaignId = Guid.NewGuid(),
            CampaignTitle = "못난이 고구마 공동구매",
            RepresentativeCandidateKey = "representative-1",
            RepresentativeMaskedDisplayName = "대표 최○○",
            ProducerMaskedDisplayName = "농가 김○○",
            ProducerRegionSummary = "전남 해남",
            ProductSummary = "규격 외 고구마",
            AvailableQuantitySummary = "10kg 상자 50개",
            SupportedPackagingFormCodes = [DomesticProducePackagingFormCodes.CorrugatedBox],
            AvailableQuantity = 500,
            MinimumTakeQuantity = 200,
            QuantityUnit = "kg",
            CanSplitShipments = true,
            ExpectedPriceSummary = "상자당 18,000원, 협의 가능",
            SupplyDeadlineSummary = "이번 주 금요일까지 출하",
            OfferReasonCode = DomesticProducerSupplyOfferReasonCodes.OffGrade,
            QualityDisclosure = "크기가 고르지 않지만 파손과 부패는 없습니다.",
            FoodSafetyConfirmed = true,
            Message = "공동구매 가능 여부를 검토해 주세요."
        };

    private static DomesticProducerSupplyOfferDraftRequest
        CreateUrgentHarvestOfferRequest()
    {
        var request = CreateSupplyOfferRequest();
        request.CampaignTitle = "양파 긴급 수확 연결";
        request.ProductSummary = "수확 전 양파";
        request.AvailableQuantitySummary = "밭 1구역 약 3,000kg";
        request.AvailableQuantity = 3_000m;
        request.MinimumTakeQuantity = 1_000m;
        request.ExpectedPriceSummary = "생산자 최소 정산 단가 kg당 1,200원";
        request.SupplyDeadlineSummary = "3일 안에 수확·현장 인수 필요";
        request.OfferReasonCode =
            DomesticProducerSupplyOfferReasonCodes.CropDestructionRisk;
        request.IsUrgentHarvestConnection = true;
        request.HarvestDeadlineAtUtc = DateTimeOffset.UtcNow.AddDays(3);
        request.StandingCropBulkTransferRequested = true;
        request.EmergencyReasonEvidenceSummary =
            "판로가 확정되지 않아 4일 뒤 밭 갈아엎기를 검토 중입니다.";
        request.MinimumProducerSettlementAmountPerUnit = 1_200m;
        request.SettlementCurrencyCode = "KRW";
        request.HarvestLaborResponsibilityCode =
            DomesticUrgentHarvestLaborResponsibilityCodes.LicensedContractor;
        request.PickupResponsibilityCode =
            DomesticUrgentHarvestPickupResponsibilityCodes.LogisticsProvider;
        request.OwnershipTransferConditionSummary =
            "서면 합의 후 검수된 수확분의 현장 인수 시 소유권 이전";
        request.WeatherAndYieldRiskDisclosure =
            "강우 시 수확 지연과 실제 수율 변동 가능";
        request.WrittenAgreementRequired = true;
        request.Message =
            "보호 단가와 역할 조건을 확인한 뒤 비구속 검토를 요청합니다.";
        return request;
    }

    private static DomesticUrgentHarvestConnectionPreviewRequest
        CreateUrgentHarvestPreviewRequest()
        => new()
        {
            ProducerVerified = true,
            RepresentativeRoleConfirmed = true,
            FoodSafetyConfirmed = true,
            HarvestDeadlineAtUtc = DateTimeOffset.UtcNow.AddDays(3),
            ProducerAvailableQuantity = 3_000m,
            ProducerMinimumTakeQuantity = 1_000m,
            BuyerGroupMaximumAbsorptionQuantity = 1_500m,
            MinimumProducerSettlementAmountPerUnit = 1_200m,
            BuyerMaximumAmountPerUnit = 1_300m,
            SettlementCurrencyCode = "KRW",
            HarvestLaborResponsibilityCode =
                DomesticUrgentHarvestLaborResponsibilityCodes.LicensedContractor,
            PickupResponsibilityCode =
                DomesticUrgentHarvestPickupResponsibilityCodes.LogisticsProvider,
            OwnershipTransferConditionSummary =
                "검수된 수확분의 현장 인수 시 소유권 이전",
            WeatherAndYieldRiskDisclosure =
                "강우 시 수확 지연과 실제 수율 변동 가능",
            EmergencyReasonEvidenceSummary =
                "판로가 확정되지 않아 4일 뒤 밭 갈아엎기를 검토 중"
        };

    private static DomesticGroupPurchaseProducerConnectionService CreateService()
        => new(
            new UnconnectedCommunityProducerMemberDirectory(),
            new UnconnectedCommunityGroupPurchaseRepresentativeDirectory(),
            new InMemoryDomesticProducerContactRequestDraftStore(),
            new InMemoryDomesticProducerSupplyOfferDraftStore());
}
