namespace Ssalddel.Contracts.Common.Orderer;

public static class DomesticProducerDirectoryIntegrationStatuses
{
    public const string NotConnected = "not-connected";
    public const string Connected = "connected";
}

public static class DomesticProducerContactRequestStatuses
{
    public const string Draft = "draft";
    public const string Requested = "requested";
    public const string Accepted = "accepted";
    public const string Rejected = "rejected";
}

public static class DomesticProducerSupplyOfferReasonCodes
{
    public const string Overproduction = "overproduction";
    public const string OffGrade = "off-grade";
    public const string ShippingDeadline = "shipping-deadline";
    public const string SalesChannelGap = "sales-channel-gap";
    public const string CropDestructionRisk = "crop-destruction-risk";
    public const string Other = "other";
}

public static class DomesticUrgentHarvestLaborResponsibilityCodes
{
    public const string Producer = "producer";
    public const string BuyerGroup = "buyer-group";
    public const string LicensedContractor = "licensed-contractor";
    public const string ToBeAgreed = "to-be-agreed";
}

public static class DomesticUrgentHarvestPickupResponsibilityCodes
{
    public const string Producer = "producer";
    public const string BuyerGroup = "buyer-group";
    public const string LogisticsProvider = "logistics-provider";
    public const string ToBeAgreed = "to-be-agreed";
}

public static class DomesticProducePackagingFormCodes
{
    public const string CorrugatedBox = "corrugated-box";
    public const string ReusableCrate = "reusable-crate";
    public const string Pallet = "pallet";
    public const string Bulk = "bulk";
    public const string Other = "other";
}

public sealed class DomesticProducerCandidateQueryResponse
{
    public string IntegrationStatusCode { get; set; } = DomesticProducerDirectoryIntegrationStatuses.NotConnected;
    public string IntegrationMessage { get; set; } = string.Empty;
    public bool ContactDetailsDisclosed { get; set; }
    public IReadOnlyList<DomesticProducerCandidateResponse> Items { get; set; } = [];
}

public sealed class DomesticProducerCandidateResponse
{
    public string CandidateKey { get; set; } = string.Empty;
    public string MaskedDisplayName { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public string RegionLabel { get; set; } = string.Empty;
    public IReadOnlyList<string> ProductTags { get; set; } = [];
    public IReadOnlyList<string> SupportedPackagingFormCodes { get; set; } = [];
    public string SupplyCapacitySummary { get; set; } = string.Empty;
    public string AvailabilitySummary { get; set; } = string.Empty;
    public string VerificationLabel { get; set; } = string.Empty;
    public bool ThirdPartySharingConsentConfirmed { get; set; }
    public bool ContactRequestConsentConfirmed { get; set; }
}

public sealed class DomesticGroupPurchaseRepresentativeCandidateQueryResponse
{
    public string IntegrationStatusCode { get; set; } = DomesticProducerDirectoryIntegrationStatuses.NotConnected;
    public string IntegrationMessage { get; set; } = string.Empty;
    public bool ContactDetailsDisclosed { get; set; }
    public IReadOnlyList<DomesticGroupPurchaseRepresentativeCandidateResponse> Items { get; set; } = [];
}

public sealed class DomesticGroupPurchaseRepresentativeCandidateResponse
{
    public string CandidateKey { get; set; } = string.Empty;
    public string MaskedDisplayName { get; set; } = string.Empty;
    public string OperatingAreaCode { get; set; } = string.Empty;
    public string OperatingAreaLabel { get; set; } = string.Empty;
    public string CommunitySummary { get; set; } = string.Empty;
    public IReadOnlyList<string> InterestedProductTags { get; set; } = [];
    public string TypicalAbsorptionCapacitySummary { get; set; } = string.Empty;
    public IReadOnlyList<string> SupportedReceiptMethods { get; set; } = [];
    public string RecruitmentSummary { get; set; } = string.Empty;
    public string VerificationLabel { get; set; } = string.Empty;
    public bool RepresentativeRoleConfirmed { get; set; }
    public bool ContactRequestConsentConfirmed { get; set; }
}

public sealed class DomesticProducerContactRequestDraftRequest
{
    public Guid GroupPurchaseCampaignId { get; set; }
    public string CampaignTitle { get; set; } = string.Empty;
    public string ProducerCandidateKey { get; set; } = string.Empty;
    public string ProducerMaskedDisplayName { get; set; } = string.Empty;
    public string ProductSummary { get; set; } = string.Empty;
    public string RequestedQuantitySummary { get; set; } = string.Empty;
    public string RequiredPackagingFormCode { get; set; } = DomesticProducePackagingFormCodes.CorrugatedBox;
    public string PackagingUnitSummary { get; set; } = string.Empty;
    public string QualityGradeSummary { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public decimal MaximumAbsorptionQuantity { get; set; }
    public string QuantityUnit { get; set; } = string.Empty;
    public bool CanReceiveSplitShipments { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class DomesticProducerContactRequestDraftResponse
{
    public Guid DraftId { get; set; }
    public Guid GroupPurchaseCampaignId { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public string ProducerCandidateKey { get; set; } = string.Empty;
    public string ProducerMaskedDisplayName { get; set; } = string.Empty;
    public string CampaignTitle { get; set; } = string.Empty;
    public string ProductSummary { get; set; } = string.Empty;
    public string RequestedQuantitySummary { get; set; } = string.Empty;
    public string RequiredPackagingFormCode { get; set; } = DomesticProducePackagingFormCodes.CorrugatedBox;
    public string PackagingUnitSummary { get; set; } = string.Empty;
    public string QualityGradeSummary { get; set; } = string.Empty;
    public decimal RequestedQuantity { get; set; }
    public decimal MaximumAbsorptionQuantity { get; set; }
    public string QuantityUnit { get; set; } = string.Empty;
    public bool CanReceiveSplitShipments { get; set; }
    public string Message { get; set; } = string.Empty;
    public string StatusCode { get; set; } = DomesticProducerContactRequestStatuses.Draft;
    public bool ContactDetailsDisclosed { get; set; }
    public bool IsDurablyPersisted { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string GuidanceMessage { get; set; } = string.Empty;
}

public sealed class DomesticProducerSupplyOfferDraftRequest
{
    public Guid GroupPurchaseCampaignId { get; set; }
    public string CampaignTitle { get; set; } = string.Empty;
    public string RepresentativeCandidateKey { get; set; } = string.Empty;
    public string RepresentativeMaskedDisplayName { get; set; } = string.Empty;
    public string ProducerMaskedDisplayName { get; set; } = string.Empty;
    public string ProducerRegionSummary { get; set; } = string.Empty;
    public string ProductSummary { get; set; } = string.Empty;
    public string AvailableQuantitySummary { get; set; } = string.Empty;
    public IReadOnlyList<string> SupportedPackagingFormCodes { get; set; } = [];
    public decimal AvailableQuantity { get; set; }
    public decimal MinimumTakeQuantity { get; set; }
    public string QuantityUnit { get; set; } = string.Empty;
    public bool CanSplitShipments { get; set; }
    public string ExpectedPriceSummary { get; set; } = string.Empty;
    public string SupplyDeadlineSummary { get; set; } = string.Empty;
    public string OfferReasonCode { get; set; } = DomesticProducerSupplyOfferReasonCodes.Overproduction;
    public string QualityDisclosure { get; set; } = string.Empty;
    public bool FoodSafetyConfirmed { get; set; }
    public bool IsUrgentHarvestConnection { get; set; }
    public DateTimeOffset? HarvestDeadlineAtUtc { get; set; }
    public bool StandingCropBulkTransferRequested { get; set; }
    public string EmergencyReasonEvidenceSummary { get; set; } = string.Empty;
    public decimal MinimumProducerSettlementAmountPerUnit { get; set; }
    public string SettlementCurrencyCode { get; set; } = "KRW";
    public string HarvestLaborResponsibilityCode { get; set; } =
        DomesticUrgentHarvestLaborResponsibilityCodes.ToBeAgreed;
    public string PickupResponsibilityCode { get; set; } =
        DomesticUrgentHarvestPickupResponsibilityCodes.ToBeAgreed;
    public string OwnershipTransferConditionSummary { get; set; } = string.Empty;
    public string WeatherAndYieldRiskDisclosure { get; set; } = string.Empty;
    public bool WrittenAgreementRequired { get; set; } = true;
    public string Message { get; set; } = string.Empty;
}

public sealed class DomesticProducerSupplyOfferDraftResponse
{
    public Guid DraftId { get; set; }
    public Guid GroupPurchaseCampaignId { get; set; }
    public string OfferedByUserId { get; set; } = string.Empty;
    public string RepresentativeCandidateKey { get; set; } = string.Empty;
    public string RepresentativeMaskedDisplayName { get; set; } = string.Empty;
    public string ProducerMaskedDisplayName { get; set; } = string.Empty;
    public string CampaignTitle { get; set; } = string.Empty;
    public string ProductSummary { get; set; } = string.Empty;
    public string AvailableQuantitySummary { get; set; } = string.Empty;
    public IReadOnlyList<string> SupportedPackagingFormCodes { get; set; } = [];
    public decimal AvailableQuantity { get; set; }
    public decimal MinimumTakeQuantity { get; set; }
    public string QuantityUnit { get; set; } = string.Empty;
    public bool CanSplitShipments { get; set; }
    public string ExpectedPriceSummary { get; set; } = string.Empty;
    public string SupplyDeadlineSummary { get; set; } = string.Empty;
    public string OfferReasonCode { get; set; } = string.Empty;
    public string QualityDisclosure { get; set; } = string.Empty;
    public bool FoodSafetyConfirmed { get; set; }
    public bool IsUrgentHarvestConnection { get; set; }
    public DateTimeOffset? HarvestDeadlineAtUtc { get; set; }
    public bool StandingCropBulkTransferRequested { get; set; }
    public string EmergencyReasonEvidenceSummary { get; set; } = string.Empty;
    public decimal MinimumProducerSettlementAmountPerUnit { get; set; }
    public string SettlementCurrencyCode { get; set; } = string.Empty;
    public string HarvestLaborResponsibilityCode { get; set; } = string.Empty;
    public string PickupResponsibilityCode { get; set; } = string.Empty;
    public string OwnershipTransferConditionSummary { get; set; } = string.Empty;
    public string WeatherAndYieldRiskDisclosure { get; set; } = string.Empty;
    public bool WrittenAgreementRequired { get; set; }
    public bool AutoPurchaseAllowed { get; set; }
    public bool AutoPriceReductionAllowed { get; set; }
    public string Message { get; set; } = string.Empty;
    public string StatusCode { get; set; } = DomesticProducerContactRequestStatuses.Draft;
    public bool ContactDetailsDisclosed { get; set; }
    public bool IsDurablyPersisted { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public string GuidanceMessage { get; set; } = string.Empty;
}

public sealed class DomesticGroupPurchaseSupplyCompatibilityPreviewRequest
{
    public string BuyerRequiredPackagingFormCode { get; set; } = DomesticProducePackagingFormCodes.CorrugatedBox;
    public decimal BuyerRequestedQuantity { get; set; }
    public decimal BuyerMaximumAbsorptionQuantity { get; set; }
    public bool BuyerCanReceiveSplitShipments { get; set; }
    public IReadOnlyList<string> ProducerSupportedPackagingFormCodes { get; set; } = [];
    public decimal ProducerAvailableQuantity { get; set; }
    public decimal ProducerMinimumTakeQuantity { get; set; }
    public bool ProducerCanSplitShipments { get; set; }
    public string QuantityUnit { get; set; } = string.Empty;
}

public sealed class DomesticGroupPurchaseSupplyCompatibilityPreviewResponse
{
    public bool ProducerCanMeetPackaging { get; set; }
    public bool ProducerCanMeetRequestedQuantity { get; set; }
    public bool BuyerMeetsMinimumTakeQuantity { get; set; }
    public bool BuyerCanAbsorbFullOffer { get; set; }
    public bool SplitShipmentCanResolveVolumeGap { get; set; }
    public bool IsMutuallyFeasible { get; set; }
    public IReadOnlyList<string> UnresolvedConditions { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
}

public sealed class DomesticUrgentHarvestConnectionPreviewRequest
{
    public bool ProducerVerified { get; set; }
    public bool RepresentativeRoleConfirmed { get; set; }
    public bool FoodSafetyConfirmed { get; set; }
    public DateTimeOffset HarvestDeadlineAtUtc { get; set; }
    public decimal ProducerAvailableQuantity { get; set; }
    public decimal ProducerMinimumTakeQuantity { get; set; }
    public decimal BuyerGroupMaximumAbsorptionQuantity { get; set; }
    public decimal MinimumProducerSettlementAmountPerUnit { get; set; }
    public decimal BuyerMaximumAmountPerUnit { get; set; }
    public string SettlementCurrencyCode { get; set; } = "KRW";
    public string HarvestLaborResponsibilityCode { get; set; } =
        DomesticUrgentHarvestLaborResponsibilityCodes.ToBeAgreed;
    public string PickupResponsibilityCode { get; set; } =
        DomesticUrgentHarvestPickupResponsibilityCodes.ToBeAgreed;
    public string OwnershipTransferConditionSummary { get; set; } = string.Empty;
    public string WeatherAndYieldRiskDisclosure { get; set; } = string.Empty;
    public string EmergencyReasonEvidenceSummary { get; set; } = string.Empty;
}

public sealed class DomesticUrgentHarvestConnectionPreviewResponse
{
    public bool EligibleForUrgentReview { get; set; }
    public bool HarvestWindowFeasible { get; set; }
    public bool BuyerCapacityFeasible { get; set; }
    public bool ProducerPriceFloorProtected { get; set; }
    public bool ResponsibilitiesDefined { get; set; }
    public bool EvidenceReady { get; set; }
    public bool RequiresWrittenAgreement { get; set; } = true;
    public bool AutoPurchaseAllowed { get; set; }
    public bool AutoPriceReductionAllowed { get; set; }
    public bool UrgencyOverridesConsent { get; set; }
    public IReadOnlyList<string> UnresolvedConditions { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
}
