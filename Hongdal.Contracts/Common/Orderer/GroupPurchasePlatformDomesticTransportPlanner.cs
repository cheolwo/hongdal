namespace Hongdal.Contracts.Common.Orderer;

public static class GroupPurchaseDomesticTransportPrincipalTypeCode
{
    public const string Platform = "Platform";
    public const string OrdererGroup = "OrdererGroup";
}

public static class GroupPurchaseDomesticTransportCostOwnerTypeCode
{
    public const string OrdererGroup = "OrdererGroup";
    public const string Platform = "Platform";
}

public static class GroupPurchaseDomesticTransportSettlementPolicyCode
{
    public const string PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff = "PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff";
    public const string PlatformPaysAndRechargesOrdererGroup = "PlatformPaysAndRechargesOrdererGroup";
    public const string PlatformAbsorbsAsPromotion = "PlatformAbsorbsAsPromotion";
    public const string ManualSettlement = "ManualSettlement";
}

public static class GroupPurchaseDomesticTransportOrdererPaymentMethodCode
{
    public const string Card = "Card";
    public const string CashLike = "CashLike";
    public const string BankTransfer = "BankTransfer";
    public const string PlatformCredit = "PlatformCredit";
}

public static class GroupPurchaseDomesticTransportDriverPayoutTriggerCode
{
    public const string DropoffCompleted = "DropoffCompleted";
    public const string PickupAndDropoffEvidenceVerified = "PickupAndDropoffEvidenceVerified";
    public const string ManualAdminApproval = "ManualAdminApproval";
}

public static class GroupPurchaseDomesticTransportDriverPayoutAccountPolicyCode
{
    public const string DriverRegisteredSettlementAccount = "DriverRegisteredSettlementAccount";
}

public static class GroupPurchaseDomesticTransportEvidenceCode
{
    public const string PickupPhoto = "PickupPhoto";
    public const string DropoffPhoto = "DropoffPhoto";
    public const string DropoffCompletion = "DropoffCompletion";
    public const string Receipt = "Receipt";
    public const string CashReceipt = "CashReceipt";
}

public static class GroupPurchaseDomesticTransportModeCode
{
    public const string Auto = "Auto";
    public const string Fcl = "Fcl";
    public const string Lcl = "Lcl";
    public const string GeneralCargo = "GeneralCargo";
}

public static class GroupPurchaseDomesticTransportSourceRequestTypeCode
{
    public const string ImportCargoTransport = "ImportCargoTransport";
    public const string FclCargoTransport = "FclCargoTransport";
    public const string LclCargoTransport = "LclCargoTransport";
}

public static class GroupPurchaseDomesticTransportDestinationTypeCode
{
    public const string ThreePlWarehouse = "ThreePlWarehouse";
    public const string ApartmentComplexDirectDistribution = "ApartmentComplexDirectDistribution";
    public const string OrdererGroupRepresentativeDropoff = "OrdererGroupRepresentativeDropoff";
}

public static class GroupPurchaseApartmentUnitDistributionModeCode
{
    public const string None = "None";
    public const string DriverToDesignatedPickupPoint = "DriverToDesignatedPickupPoint";
    public const string DriverToBuildingEntrance = "DriverToBuildingEntrance";
    public const string DriverToUnitDoor = "DriverToUnitDoor";
}

public static class GroupPurchaseApartmentUnitDistributionPrivacyModeCode
{
    public const string MaskedUnitTokens = "MaskedUnitTokens";
    public const string FullUnitAddressAfterAssignment = "FullUnitAddressAfterAssignment";
    public const string ManualChecklist = "ManualChecklist";
}

public static class GroupPurchaseUnitSortationModeCode
{
    public const string NotRequired = "NotRequired";
    public const string ByBuilding = "ByBuilding";
    public const string ByBuildingAndUnit = "ByBuildingAndUnit";
    public const string ByRouteSequence = "ByRouteSequence";
}

public static class GroupPurchaseUnitSortationLocationCode
{
    public const string BondedArea = "BondedArea";
    public const string ThreePlWarehouse = "ThreePlWarehouse";
    public const string ApartmentStagingArea = "ApartmentStagingArea";
}

public static class GroupPurchaseUnitSortationResponsiblePartyCode
{
    public const string OverseasSeller = "OverseasSeller";
    public const string OverseasForwarder = "OverseasForwarder";
    public const string Platform = "Platform";
    public const string OrdererGroup = "OrdererGroup";
    public const string DomesticOperator = "DomesticOperator";
}

public static class GroupPurchaseUnitPackageLabelingModeCode
{
    public const string ProductInfoSticker = "ProductInfoSticker";
    public const string UnitInvoiceLabel = "UnitInvoiceLabel";
    public const string NoUnitLabel = "NoUnitLabel";
}

public static class GroupPurchaseProductInfoStorageLocationCode
{
    public const string OverseasSellerSystem = "OverseasSellerSystem";
    public const string OverseasForwarderSystem = "OverseasForwarderSystem";
    public const string PlatformImportLedger = "PlatformImportLedger";
    public const string ManualDocumentArchive = "ManualDocumentArchive";
}

public static class GroupPurchaseUnitBarcodeSchemeCode
{
    public const string OrderNumberBarcode = "OrderNumberBarcode";
    public const string InvoiceNumberBarcode = "InvoiceNumberBarcode";
    public const string PackageIdBarcode = "PackageIdBarcode";
}

public static class GroupPurchaseApartmentDistributionResponsibilityCode
{
    public const string None = "None";
    public const string Driver = "Driver";
    public const string OrdererGroup = "OrdererGroup";
    public const string SeparateWorker = "SeparateWorker";
    public const string PlatformArrangedWorker = "PlatformArrangedWorker";
}

public static class GroupPurchaseDomesticTransportCostOptionStatusCode
{
    public const string Selectable = "Selectable";
    public const string NeedsConfirmation = "NeedsConfirmation";
    public const string NotCompatible = "NotCompatible";
}

public static class GroupPurchaseDomesticTransportRequiredActionCode
{
    public const string ConfirmPlatformShipperProfile = "ConfirmPlatformShipperProfile";
    public const string ConfirmCustomsReleaseOrBondedRelease = "ConfirmCustomsReleaseOrBondedRelease";
    public const string ConfirmBondedAreaPickupAddress = "ConfirmBondedAreaPickupAddress";
    public const string ConfirmThreePlDropoffAddress = "ConfirmThreePlDropoffAddress";
    public const string ConfirmApartmentComplexDropoffAddress = "ConfirmApartmentComplexDropoffAddress";
    public const string ConfirmApartmentUnitDistributionPlan = "ConfirmApartmentUnitDistributionPlan";
    public const string ConfirmRecipientAddressPrivacy = "ConfirmRecipientAddressPrivacy";
    public const string ConfirmCargoSpecification = "ConfirmCargoSpecification";
    public const string ConfirmPlatformEntrustedTransport = "ConfirmPlatformEntrustedTransport";
    public const string ConfirmOrdererPaymentCollection = "ConfirmOrdererPaymentCollection";
    public const string ConfirmDriverSettlementAccount = "ConfirmDriverSettlementAccount";
    public const string ConfirmDriverPayoutPolicy = "ConfirmDriverPayoutPolicy";
    public const string ConfirmColdChainVehicle = "ConfirmColdChainVehicle";
    public const string ConfirmColdChainThreePlFacility = "ConfirmColdChainThreePlFacility";
    public const string ConfirmDistributionResponsibility = "ConfirmDistributionResponsibility";
    public const string ConfirmTransportDecisionRevision = "ConfirmTransportDecisionRevision";
    public const string ConfirmUnitSortationBeforePickup = "ConfirmUnitSortationBeforePickup";
    public const string ConfirmOverseasUnitInvoiceAndLabeling = "ConfirmOverseasUnitInvoiceAndLabeling";
    public const string ConfirmImportedProductInfoRegistration = "ConfirmImportedProductInfoRegistration";
    public const string ConfirmProductInfoStickerStorage = "ConfirmProductInfoStickerStorage";
    public const string ConfirmUnitProductInfoSticker = "ConfirmUnitProductInfoSticker";
    public const string ConfirmUnitBarcodeScanLookup = "ConfirmUnitBarcodeScanLookup";
}

public sealed class GroupPurchasePlatformDomesticTransportDraftRequest
{
    public string PlatformShipperUserId { get; set; } = "platform";
    public string PlatformLegalEntityName { get; set; } = string.Empty;
    public string TransportMode { get; set; } = GroupPurchaseDomesticTransportModeCode.Auto;
    public bool CustomsReleaseReady { get; set; }
    public bool RequireAdminConfirmation { get; set; } = true;
    public string SettlementPolicyCode { get; set; } = GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff;
    public bool PlatformCollectsOrdererPayments { get; set; } = true;
    public bool PlatformHoldsFundsUntilDropoff { get; set; } = true;
    public bool OrdererPaymentCollectionConfirmed { get; set; }
    public IReadOnlyList<string> OrdererPaymentMethodCodes { get; set; } =
    [
        GroupPurchaseDomesticTransportOrdererPaymentMethodCode.Card,
        GroupPurchaseDomesticTransportOrdererPaymentMethodCode.CashLike
    ];
    public bool DriverSettlementAccountConfirmed { get; set; }
    public string DriverPayoutTriggerCode { get; set; } = GroupPurchaseDomesticTransportDriverPayoutTriggerCode.DropoffCompleted;
    public int DriverPayoutDelayDays { get; set; } = 3;
    public DateTime? DropoffCompletedAtUtc { get; set; }
    public bool RequirePickupEvidence { get; set; } = true;
    public bool RequireDropoffEvidence { get; set; } = true;
    public bool RequireReceiptEvidence { get; set; }
    public bool RequireCashReceipt { get; set; }
    public int? EstimatedFareKrw { get; set; }
    public int? EstimatedThreePlTransportFareKrw { get; set; }
    public int? EstimatedThreePlInboundFeeKrw { get; set; }
    public int? EstimatedThreePlStorageFeeKrw { get; set; }
    public int? EstimatedApartmentDirectTransportFareKrw { get; set; }
    public int? EstimatedDriverUnitDistributionFeeKrw { get; set; }
    public int? EstimatedSeparateWorkerDistributionFeeKrw { get; set; }
    public int? EstimatedRepresentativeDropoffTransportFareKrw { get; set; }
    public string DestinationTypeCode { get; set; } = GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution;
    public bool TransportDecisionConfirmed { get; set; } = true;
    public bool TransportDecisionLocked { get; set; } = true;
    public bool TransportDecisionRevisionRequested { get; set; }
    public string TransportDecisionRevisionReason { get; set; } = string.Empty;
    public string BondedAreaCode { get; set; } = string.Empty;
    public string BondedAreaName { get; set; } = string.Empty;
    public string PickupRoadAddress { get; set; } = string.Empty;
    public string PickupDetailAddress { get; set; } = string.Empty;
    public decimal? PickupLatitude { get; set; }
    public decimal? PickupLongitude { get; set; }
    public string PickupContactName { get; set; } = string.Empty;
    public string PickupContactPhone { get; set; } = string.Empty;
    public DateTime? PickupWindowStartAtUtc { get; set; }
    public DateTime? PickupWindowEndAtUtc { get; set; }
    public string ThreePlWarehouseName { get; set; } = string.Empty;
    public string ApartmentComplexCode { get; set; } = string.Empty;
    public string ApartmentComplexName { get; set; } = string.Empty;
    public bool DriverPerformsApartmentUnitDistribution { get; set; } = true;
    public string ApartmentUnitDistributionModeCode { get; set; } = GroupPurchaseApartmentUnitDistributionModeCode.DriverToUnitDoor;
    public int? ApartmentUnitDeliveryCount { get; set; }
    public bool ApartmentUnitDistributionPlanConfirmed { get; set; }
    public bool UnitSortationBeforePickupRequired { get; set; } = true;
    public bool UnitSortationBeforePickupConfirmed { get; set; }
    public string UnitSortationModeCode { get; set; } = GroupPurchaseUnitSortationModeCode.ByBuildingAndUnit;
    public string UnitSortationLocationCode { get; set; } = GroupPurchaseUnitSortationLocationCode.BondedArea;
    public string UnitSortationResponsiblePartyCode { get; set; } = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller;
    public bool UnitDemandBreakdownConfirmed { get; set; }
    public bool ImportedProductInfoRegistrationRequired { get; set; } = true;
    public bool ImportedProductInfoRegistered { get; set; }
    public string ProductInfoRegisteredByPartyCode { get; set; } = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller;
    public string ProductInfoStorageLocationCode { get; set; } = GroupPurchaseProductInfoStorageLocationCode.PlatformImportLedger;
    public bool ProductInfoStorageConfirmed { get; set; }
    public string UnitPackageLabelingModeCode { get; set; } = GroupPurchaseUnitPackageLabelingModeCode.ProductInfoSticker;
    public bool UnitProductInfoStickerConfirmed { get; set; }
    public bool ProductInfoStickerBarcodeIncluded { get; set; }
    public bool ProductInfoStickerMatchesImportedProductConfirmed { get; set; }
    public bool UnitInvoiceIssuedConfirmed { get; set; }
    public bool UnitPackageLabelsConfirmed { get; set; }
    public bool UnitBarcodeScanLookupEnabled { get; set; }
    public string UnitBarcodeSchemeCode { get; set; } = GroupPurchaseUnitBarcodeSchemeCode.OrderNumberBarcode;
    public bool UnitBarcodeLookupDataConfirmed { get; set; }
    public bool UnitBarcodeMapsToMaskedRecipientConfirmed { get; set; }
    public bool UnitBarcodeMapsToDemandQuantityConfirmed { get; set; }
    public bool LoadingSequenceConfirmed { get; set; }
    public int? SortedUnitPackageCount { get; set; }
    public bool RecipientAddressPrivacyConfirmed { get; set; }
    public string DistributionPrivacyModeCode { get; set; } = GroupPurchaseApartmentUnitDistributionPrivacyModeCode.MaskedUnitTokens;
    public string DistributionResponsibilityCode { get; set; } = GroupPurchaseApartmentDistributionResponsibilityCode.Driver;
    public bool DistributionResponsibilityConfirmed { get; set; }
    public string DropoffRoadAddress { get; set; } = string.Empty;
    public string DropoffDetailAddress { get; set; } = string.Empty;
    public decimal? DropoffLatitude { get; set; }
    public decimal? DropoffLongitude { get; set; }
    public string DropoffContactName { get; set; } = string.Empty;
    public string DropoffContactPhone { get; set; } = string.Empty;
    public DateTime? DropoffWindowStartAtUtc { get; set; }
    public DateTime? DropoffWindowEndAtUtc { get; set; }
    public string CargoDescription { get; set; } = string.Empty;
    public int? CargoQuantity { get; set; }
    public decimal? CargoWeightKg { get; set; }
    public decimal? CargoVolumeCbm { get; set; }
    public int? PalletCount { get; set; }
    public string TemperatureCondition { get; set; } = string.Empty;
    public bool RequiresColdChain { get; set; }
    public bool ColdChainVehicleConfirmed { get; set; }
    public bool ThreePlColdChainFacilityConfirmed { get; set; }
    public bool Fragile { get; set; }
    public string AdminMemo { get; set; } = string.Empty;
}

public sealed class GroupPurchasePlatformDomesticTransportDraftResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PrincipalType { get; set; } = GroupPurchaseDomesticTransportPrincipalTypeCode.Platform;
    public string CostOwnerType { get; set; } = GroupPurchaseDomesticTransportCostOwnerTypeCode.OrdererGroup;
    public string SettlementPolicyCode { get; set; } = GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff;
    public string GroupPurchaseId { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public string SourceRequestType { get; set; } = GroupPurchaseDomesticTransportSourceRequestTypeCode.ImportCargoTransport;
    public int DispatchBusinessTypeCode { get; set; } = 20;
    public bool ReadyForDispatchQueue { get; set; }
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public PlatformEntrustedCargoTransportDraftDto CargoTransportDraft { get; set; } = new();
    public PlatformEntrustedDispatchQueueDraftDto DispatchQueueDraft { get; set; } = new();
    public PlatformEntrustedDestinationPlanDto DestinationPlan { get; set; } = new();
    public PlatformEntrustedColdChainPlanDto ColdChainPlan { get; set; } = new();
    public IReadOnlyList<PlatformEntrustedDestinationCostOptionDto> DestinationCostOptions { get; set; } = [];
    public PlatformEntrustedDriverPayoutPlanDto DriverPayoutPlan { get; set; } = new();
}

public sealed class PlatformEntrustedCargoTransportDraftDto
{
    public string ClientRequestId { get; set; } = string.Empty;
    public string PlatformShipperUserId { get; set; } = string.Empty;
    public string PlatformLegalEntityName { get; set; } = string.Empty;
    public string OrdererGroupScopeKey { get; set; } = string.Empty;
    public string OrdererGroupScopeName { get; set; } = string.Empty;
    public string CargoType { get; set; } = "Imported group purchase cargo";
    public string DestinationTypeCode { get; set; } = GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse;
    public string DestinationName { get; set; } = string.Empty;
    public string CargoDescription { get; set; } = string.Empty;
    public int? CargoQuantity { get; set; }
    public decimal? CargoWeightKg { get; set; }
    public decimal? CargoVolumeCbm { get; set; }
    public int? PalletCount { get; set; }
    public string TemperatureCondition { get; set; } = string.Empty;
    public bool Fragile { get; set; }
    public string PickupRoadAddress { get; set; } = string.Empty;
    public string PickupDetailAddress { get; set; } = string.Empty;
    public decimal? PickupLatitude { get; set; }
    public decimal? PickupLongitude { get; set; }
    public string PickupContactName { get; set; } = string.Empty;
    public string PickupContactPhone { get; set; } = string.Empty;
    public DateTime? PickupWindowStartAtUtc { get; set; }
    public DateTime? PickupWindowEndAtUtc { get; set; }
    public string DropoffRoadAddress { get; set; } = string.Empty;
    public string DropoffDetailAddress { get; set; } = string.Empty;
    public decimal? DropoffLatitude { get; set; }
    public decimal? DropoffLongitude { get; set; }
    public string DropoffContactName { get; set; } = string.Empty;
    public string DropoffContactPhone { get; set; } = string.Empty;
    public DateTime? DropoffWindowStartAtUtc { get; set; }
    public DateTime? DropoffWindowEndAtUtc { get; set; }
    public int? EstimatedFareKrw { get; set; }
    public string PaymentMethodCode { get; set; } = "PlatformSettlement";
    public string SettlementMemo { get; set; } = string.Empty;
}

public sealed class PlatformEntrustedDestinationPlanDto
{
    public string DestinationTypeCode { get; set; } = GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse;
    public string DestinationName { get; set; } = string.Empty;
    public string ThreePlWarehouseName { get; set; } = string.Empty;
    public string ApartmentComplexCode { get; set; } = string.Empty;
    public string ApartmentComplexName { get; set; } = string.Empty;
    public bool DirectApartmentDistribution { get; set; }
    public bool DriverPerformsApartmentUnitDistribution { get; set; }
    public string ApartmentUnitDistributionModeCode { get; set; } = GroupPurchaseApartmentUnitDistributionModeCode.None;
    public int? ApartmentUnitDeliveryCount { get; set; }
    public bool ApartmentUnitDistributionPlanConfirmed { get; set; }
    public PlatformEntrustedUnitSortationPlanDto UnitSortationPlan { get; set; } = new();
    public bool RecipientAddressPrivacyConfirmed { get; set; }
    public string DistributionPrivacyModeCode { get; set; } = GroupPurchaseApartmentUnitDistributionPrivacyModeCode.MaskedUnitTokens;
    public string DistributionResponsibilityCode { get; set; } = GroupPurchaseApartmentDistributionResponsibilityCode.None;
    public bool DistributionResponsibilityConfirmed { get; set; }
    public bool TransportDecisionConfirmed { get; set; } = true;
    public bool TransportDecisionLocked { get; set; } = true;
    public bool TransportDecisionRevisionRequested { get; set; }
    public string TransportDecisionRevisionReason { get; set; } = string.Empty;
    public IReadOnlyList<string> RequiredDistributionEvidenceCodes { get; set; } = [];
    public string DestinationMemo { get; set; } = string.Empty;
}

public sealed class PlatformEntrustedUnitSortationPlanDto
{
    public bool UnitSortationBeforePickupRequired { get; set; }
    public bool UnitSortationBeforePickupConfirmed { get; set; }
    public string UnitSortationModeCode { get; set; } = GroupPurchaseUnitSortationModeCode.NotRequired;
    public string UnitSortationLocationCode { get; set; } = GroupPurchaseUnitSortationLocationCode.BondedArea;
    public string UnitSortationResponsiblePartyCode { get; set; } = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller;
    public bool UnitDemandBreakdownConfirmed { get; set; }
    public bool ImportedProductInfoRegistrationRequired { get; set; } = true;
    public bool ImportedProductInfoRegistered { get; set; }
    public string ProductInfoRegisteredByPartyCode { get; set; } = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller;
    public string ProductInfoStorageLocationCode { get; set; } = GroupPurchaseProductInfoStorageLocationCode.PlatformImportLedger;
    public bool ProductInfoStorageConfirmed { get; set; }
    public string UnitPackageLabelingModeCode { get; set; } = GroupPurchaseUnitPackageLabelingModeCode.ProductInfoSticker;
    public bool UnitProductInfoStickerConfirmed { get; set; }
    public bool ProductInfoStickerBarcodeIncluded { get; set; }
    public bool ProductInfoStickerMatchesImportedProductConfirmed { get; set; }
    public bool UnitInvoiceIssuedConfirmed { get; set; }
    public bool UnitPackageLabelsConfirmed { get; set; }
    public bool UnitBarcodeScanLookupEnabled { get; set; }
    public string UnitBarcodeSchemeCode { get; set; } = GroupPurchaseUnitBarcodeSchemeCode.OrderNumberBarcode;
    public bool UnitBarcodeLookupDataConfirmed { get; set; }
    public bool UnitBarcodeMapsToMaskedRecipientConfirmed { get; set; }
    public bool UnitBarcodeMapsToDemandQuantityConfirmed { get; set; }
    public bool LoadingSequenceConfirmed { get; set; }
    public int? SortedUnitPackageCount { get; set; }
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string Memo { get; set; } = string.Empty;
}

public sealed class PlatformEntrustedColdChainPlanDto
{
    public string TemperatureCode { get; set; } = GroupPurchaseTemperatureCode.Ambient;
    public bool RequiresColdChain { get; set; }
    public bool ColdChainVehicleConfirmed { get; set; }
    public bool ThreePlColdChainFacilityConfirmed { get; set; }
    public bool SelectedDestinationColdChainCompatible { get; set; } = true;
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string Memo { get; set; } = string.Empty;
}

public sealed class PlatformEntrustedDestinationCostOptionDto
{
    public string DestinationTypeCode { get; set; } = GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse;
    public string OptionName { get; set; } = string.Empty;
    public string DistributionResponsibilityCode { get; set; } = GroupPurchaseApartmentDistributionResponsibilityCode.None;
    public bool CompatibleWithTemperature { get; set; } = true;
    public string StatusCode { get; set; } = GroupPurchaseDomesticTransportCostOptionStatusCode.Selectable;
    public int? EstimatedTransportFareKrw { get; set; }
    public int? EstimatedInboundFeeKrw { get; set; }
    public int? EstimatedStorageFeeKrw { get; set; }
    public int? EstimatedDistributionFeeKrw { get; set; }
    public int? EstimatedTotalCostKrw { get; set; }
    public decimal? EstimatedCostPerKgKrw { get; set; }
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string Memo { get; set; } = string.Empty;
}

public sealed class PlatformEntrustedDriverPayoutPlanDto
{
    public bool PlatformCollectsOrdererPayments { get; set; } = true;
    public bool PlatformHoldsFundsUntilDropoff { get; set; } = true;
    public IReadOnlyList<string> OrdererPaymentMethodCodes { get; set; } = [];
    public bool OrdererPaymentCollectionConfirmed { get; set; }
    public string DriverPayoutTriggerCode { get; set; } = GroupPurchaseDomesticTransportDriverPayoutTriggerCode.DropoffCompleted;
    public int DriverPayoutDelayDays { get; set; } = 3;
    public DateTime? DropoffCompletedAtUtc { get; set; }
    public DateTime? DriverPayoutDueAtUtc { get; set; }
    public string PayoutRecipientType { get; set; } = "Driver";
    public string PayoutAccountPolicyCode { get; set; } = GroupPurchaseDomesticTransportDriverPayoutAccountPolicyCode.DriverRegisteredSettlementAccount;
    public bool DriverSettlementAccountConfirmed { get; set; }
    public bool RequirePickupEvidence { get; set; } = true;
    public bool RequireDropoffEvidence { get; set; } = true;
    public bool RequireReceiptEvidence { get; set; }
    public bool RequireCashReceipt { get; set; }
    public IReadOnlyList<string> RequiredEvidenceCodes { get; set; } = [];
}

public sealed class PlatformEntrustedDispatchQueueDraftDto
{
    public string RequestId { get; set; } = string.Empty;
    public string PlatformShipperUserId { get; set; } = string.Empty;
    public int DispatchBusinessTypeCode { get; set; } = 20;
    public string SourceRequestType { get; set; } = GroupPurchaseDomesticTransportSourceRequestTypeCode.ImportCargoTransport;
    public string SourceRequestId { get; set; } = string.Empty;
    public string DestinationTypeCode { get; set; } = GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse;
    public string DestinationName { get; set; } = string.Empty;
    public bool DriverPerformsApartmentUnitDistribution { get; set; }
    public string ApartmentUnitDistributionModeCode { get; set; } = GroupPurchaseApartmentUnitDistributionModeCode.None;
    public int? ApartmentUnitDeliveryCount { get; set; }
    public string DistributionResponsibilityCode { get; set; } = GroupPurchaseApartmentDistributionResponsibilityCode.None;
    public bool UnitSortationBeforePickupRequired { get; set; }
    public bool UnitSortationBeforePickupConfirmed { get; set; }
    public string UnitSortationModeCode { get; set; } = GroupPurchaseUnitSortationModeCode.NotRequired;
    public string UnitSortationLocationCode { get; set; } = GroupPurchaseUnitSortationLocationCode.BondedArea;
    public string UnitSortationResponsiblePartyCode { get; set; } = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller;
    public bool UnitDemandBreakdownConfirmed { get; set; }
    public bool ImportedProductInfoRegistrationRequired { get; set; } = true;
    public bool ImportedProductInfoRegistered { get; set; }
    public string ProductInfoRegisteredByPartyCode { get; set; } = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller;
    public string ProductInfoStorageLocationCode { get; set; } = GroupPurchaseProductInfoStorageLocationCode.PlatformImportLedger;
    public bool ProductInfoStorageConfirmed { get; set; }
    public string UnitPackageLabelingModeCode { get; set; } = GroupPurchaseUnitPackageLabelingModeCode.ProductInfoSticker;
    public bool UnitProductInfoStickerConfirmed { get; set; }
    public bool ProductInfoStickerBarcodeIncluded { get; set; }
    public bool ProductInfoStickerMatchesImportedProductConfirmed { get; set; }
    public bool UnitInvoiceIssuedConfirmed { get; set; }
    public bool UnitPackageLabelsConfirmed { get; set; }
    public bool UnitBarcodeScanLookupEnabled { get; set; }
    public string UnitBarcodeSchemeCode { get; set; } = GroupPurchaseUnitBarcodeSchemeCode.OrderNumberBarcode;
    public bool UnitBarcodeLookupDataConfirmed { get; set; }
    public bool UnitBarcodeMapsToMaskedRecipientConfirmed { get; set; }
    public bool UnitBarcodeMapsToDemandQuantityConfirmed { get; set; }
    public bool LoadingSequenceConfirmed { get; set; }
    public int? SortedUnitPackageCount { get; set; }
    public string PickupRoadAddress { get; set; } = string.Empty;
    public string PickupDetailAddress { get; set; } = string.Empty;
    public decimal? PickupLatitude { get; set; }
    public decimal? PickupLongitude { get; set; }
    public string DropoffRoadAddress { get; set; } = string.Empty;
    public string DropoffDetailAddress { get; set; } = string.Empty;
    public decimal? DropoffLatitude { get; set; }
    public decimal? DropoffLongitude { get; set; }
    public string QueueStatusCode { get; set; } = "Waiting";
}

public static class GroupPurchasePlatformDomesticTransportPlanner
{
    public static GroupPurchasePlatformDomesticTransportDraftResult Plan(
        GroupPurchaseCommerceFulfillmentPlanDto fulfillmentPlan,
        GroupPurchasePlatformDomesticTransportDraftRequest request)
    {
        ArgumentNullException.ThrowIfNull(fulfillmentPlan);
        ArgumentNullException.ThrowIfNull(request);

        var destinationType = NormalizeDestinationType(request.DestinationTypeCode);
        var unitDistributionMode = NormalizeApartmentUnitDistributionMode(
            request.ApartmentUnitDistributionModeCode,
            request.DriverPerformsApartmentUnitDistribution);
        var distributionPrivacyMode = NormalizeDistributionPrivacyMode(request.DistributionPrivacyModeCode);
        var distributionResponsibility = NormalizeDistributionResponsibility(
            request.DistributionResponsibilityCode,
            request.DriverPerformsApartmentUnitDistribution,
            destinationType);
        var temperatureCode = NormalizeTemperatureCode(request.TemperatureCondition);
        var requiresColdChain = ResolveRequiresColdChain(request, temperatureCode);
        var requiredActions = ResolveRequiredActions(request, destinationType, requiresColdChain, distributionResponsibility).ToArray();
        var unitSortationActionCodes = ResolveUnitSortationRequiredActions(request, destinationType, distributionResponsibility).ToArray();
        var sourceRequestType = ResolveSourceRequestType(request.TransportMode);
        var clientRequestId = BuildClientRequestId(fulfillmentPlan, sourceRequestType, destinationType);
        var ready = requiredActions.Length == 0;
        var cargoDescription = string.IsNullOrWhiteSpace(request.CargoDescription)
            ? fulfillmentPlan.ProductName
            : request.CargoDescription.Trim();
        var cargoQuantity = request.CargoQuantity ?? fulfillmentPlan.ExpectedInboundQuantity;
        var settlementPolicy = NormalizeSettlementPolicy(request.SettlementPolicyCode);
        var ordererPaymentMethodCodes = NormalizeOrdererPaymentMethods(request.OrdererPaymentMethodCodes).ToArray();
        var driverPayoutTrigger = NormalizeDriverPayoutTrigger(request.DriverPayoutTriggerCode);
        var driverPayoutDelayDays = NormalizeDriverPayoutDelayDays(request.DriverPayoutDelayDays);
        var requiredEvidenceCodes = ResolveRequiredEvidenceCodes(request).ToArray();
        var destinationName = ResolveDestinationName(fulfillmentPlan, request, destinationType);
        var distributionEvidenceCodes = ResolveRequiredDistributionEvidenceCodes(request, destinationType).ToArray();
        var coldChainActionCodes = ResolveColdChainRequiredActions(request, destinationType, requiresColdChain).ToArray();
        var destinationCostOptions = BuildDestinationCostOptions(
            request,
            fulfillmentPlan,
            destinationType,
            requiresColdChain).ToArray();

        return new GroupPurchasePlatformDomesticTransportDraftResult
        {
            Success = true,
            Message = ready
                ? "Platform-entrusted domestic cargo transport draft is ready for the 1.0 dispatch queue."
                : "Platform-entrusted domestic cargo transport draft requires confirmation before dispatch queue creation.",
            PrincipalType = GroupPurchaseDomesticTransportPrincipalTypeCode.Platform,
            CostOwnerType = GroupPurchaseDomesticTransportCostOwnerTypeCode.OrdererGroup,
            SettlementPolicyCode = settlementPolicy,
            GroupPurchaseId = fulfillmentPlan.GroupPurchaseId,
            OrdererGroupScopeKey = fulfillmentPlan.OrdererGroupScopeKey,
            OrdererGroupScopeName = fulfillmentPlan.OrdererGroupScopeName,
            DocumentManagementNumber = fulfillmentPlan.DocumentManagementNumber,
            SourceRequestType = sourceRequestType,
            DispatchBusinessTypeCode = 20,
            ReadyForDispatchQueue = ready,
            RequiredActionCodes = requiredActions,
            CargoTransportDraft = new PlatformEntrustedCargoTransportDraftDto
            {
                ClientRequestId = clientRequestId,
                PlatformShipperUserId = NormalizePlatformUserId(request.PlatformShipperUserId),
                PlatformLegalEntityName = request.PlatformLegalEntityName.Trim(),
                OrdererGroupScopeKey = fulfillmentPlan.OrdererGroupScopeKey,
                OrdererGroupScopeName = fulfillmentPlan.OrdererGroupScopeName,
                DestinationTypeCode = destinationType,
                DestinationName = destinationName,
                CargoDescription = cargoDescription,
                CargoQuantity = cargoQuantity > 0 ? cargoQuantity : null,
                CargoWeightKg = request.CargoWeightKg,
                CargoVolumeCbm = request.CargoVolumeCbm,
                PalletCount = request.PalletCount,
                TemperatureCondition = request.TemperatureCondition.Trim(),
                Fragile = request.Fragile,
                PickupRoadAddress = request.PickupRoadAddress.Trim(),
                PickupDetailAddress = request.PickupDetailAddress.Trim(),
                PickupLatitude = request.PickupLatitude,
                PickupLongitude = request.PickupLongitude,
                PickupContactName = request.PickupContactName.Trim(),
                PickupContactPhone = request.PickupContactPhone.Trim(),
                PickupWindowStartAtUtc = request.PickupWindowStartAtUtc,
                PickupWindowEndAtUtc = request.PickupWindowEndAtUtc,
                DropoffRoadAddress = request.DropoffRoadAddress.Trim(),
                DropoffDetailAddress = request.DropoffDetailAddress.Trim(),
                DropoffLatitude = request.DropoffLatitude,
                DropoffLongitude = request.DropoffLongitude,
                DropoffContactName = request.DropoffContactName.Trim(),
                DropoffContactPhone = request.DropoffContactPhone.Trim(),
                DropoffWindowStartAtUtc = request.DropoffWindowStartAtUtc,
                DropoffWindowEndAtUtc = request.DropoffWindowEndAtUtc,
                EstimatedFareKrw = request.EstimatedFareKrw,
                PaymentMethodCode = request.PlatformCollectsOrdererPayments ? "PlatformCollectedSettlement" : "PlatformSettlement",
                SettlementMemo = BuildSettlementMemo(
                    fulfillmentPlan,
                    request,
                    settlementPolicy,
                    ordererPaymentMethodCodes,
                    driverPayoutTrigger,
                    driverPayoutDelayDays,
                    destinationType)
            },
            DispatchQueueDraft = new PlatformEntrustedDispatchQueueDraftDto
            {
                RequestId = clientRequestId,
                PlatformShipperUserId = NormalizePlatformUserId(request.PlatformShipperUserId),
                DispatchBusinessTypeCode = 20,
                SourceRequestType = sourceRequestType,
                SourceRequestId = clientRequestId,
                DestinationTypeCode = destinationType,
                DestinationName = destinationName,
                DriverPerformsApartmentUnitDistribution = request.DriverPerformsApartmentUnitDistribution,
                ApartmentUnitDistributionModeCode = unitDistributionMode,
                ApartmentUnitDeliveryCount = request.ApartmentUnitDeliveryCount > 0 ? request.ApartmentUnitDeliveryCount : null,
                DistributionResponsibilityCode = distributionResponsibility,
                UnitSortationBeforePickupRequired = request.UnitSortationBeforePickupRequired,
                UnitSortationBeforePickupConfirmed = request.UnitSortationBeforePickupConfirmed,
                UnitSortationModeCode = NormalizeUnitSortationMode(request.UnitSortationModeCode, request.UnitSortationBeforePickupRequired),
                UnitSortationLocationCode = NormalizeUnitSortationLocation(request.UnitSortationLocationCode),
                UnitSortationResponsiblePartyCode = NormalizeUnitSortationResponsibleParty(request.UnitSortationResponsiblePartyCode),
                UnitDemandBreakdownConfirmed = request.UnitDemandBreakdownConfirmed,
                ImportedProductInfoRegistrationRequired = request.ImportedProductInfoRegistrationRequired,
                ImportedProductInfoRegistered = request.ImportedProductInfoRegistered,
                ProductInfoRegisteredByPartyCode = NormalizeUnitSortationResponsibleParty(request.ProductInfoRegisteredByPartyCode),
                ProductInfoStorageLocationCode = NormalizeProductInfoStorageLocation(request.ProductInfoStorageLocationCode),
                ProductInfoStorageConfirmed = request.ProductInfoStorageConfirmed,
                UnitPackageLabelingModeCode = NormalizeUnitPackageLabelingMode(request.UnitPackageLabelingModeCode),
                UnitProductInfoStickerConfirmed = request.UnitProductInfoStickerConfirmed,
                ProductInfoStickerBarcodeIncluded = request.ProductInfoStickerBarcodeIncluded,
                ProductInfoStickerMatchesImportedProductConfirmed = request.ProductInfoStickerMatchesImportedProductConfirmed,
                UnitInvoiceIssuedConfirmed = request.UnitInvoiceIssuedConfirmed,
                UnitPackageLabelsConfirmed = request.UnitPackageLabelsConfirmed,
                UnitBarcodeScanLookupEnabled = request.UnitBarcodeScanLookupEnabled,
                UnitBarcodeSchemeCode = NormalizeUnitBarcodeScheme(request.UnitBarcodeSchemeCode),
                UnitBarcodeLookupDataConfirmed = request.UnitBarcodeLookupDataConfirmed,
                UnitBarcodeMapsToMaskedRecipientConfirmed = request.UnitBarcodeMapsToMaskedRecipientConfirmed,
                UnitBarcodeMapsToDemandQuantityConfirmed = request.UnitBarcodeMapsToDemandQuantityConfirmed,
                LoadingSequenceConfirmed = request.LoadingSequenceConfirmed,
                SortedUnitPackageCount = request.SortedUnitPackageCount > 0 ? request.SortedUnitPackageCount : null,
                PickupRoadAddress = request.PickupRoadAddress.Trim(),
                PickupDetailAddress = request.PickupDetailAddress.Trim(),
                PickupLatitude = request.PickupLatitude,
                PickupLongitude = request.PickupLongitude,
                DropoffRoadAddress = request.DropoffRoadAddress.Trim(),
                DropoffDetailAddress = request.DropoffDetailAddress.Trim(),
                DropoffLatitude = request.DropoffLatitude,
                DropoffLongitude = request.DropoffLongitude
            },
            DestinationPlan = new PlatformEntrustedDestinationPlanDto
            {
                DestinationTypeCode = destinationType,
                DestinationName = destinationName,
                ThreePlWarehouseName = request.ThreePlWarehouseName.Trim(),
                ApartmentComplexCode = request.ApartmentComplexCode.Trim(),
                ApartmentComplexName = request.ApartmentComplexName.Trim(),
                DirectApartmentDistribution = destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
                DriverPerformsApartmentUnitDistribution = request.DriverPerformsApartmentUnitDistribution,
                ApartmentUnitDistributionModeCode = unitDistributionMode,
                ApartmentUnitDeliveryCount = request.ApartmentUnitDeliveryCount > 0 ? request.ApartmentUnitDeliveryCount : null,
                ApartmentUnitDistributionPlanConfirmed = request.ApartmentUnitDistributionPlanConfirmed,
                UnitSortationPlan = new PlatformEntrustedUnitSortationPlanDto
                {
                    UnitSortationBeforePickupRequired = request.UnitSortationBeforePickupRequired,
                    UnitSortationBeforePickupConfirmed = request.UnitSortationBeforePickupConfirmed,
                    UnitSortationModeCode = NormalizeUnitSortationMode(request.UnitSortationModeCode, request.UnitSortationBeforePickupRequired),
                    UnitSortationLocationCode = NormalizeUnitSortationLocation(request.UnitSortationLocationCode),
                    UnitSortationResponsiblePartyCode = NormalizeUnitSortationResponsibleParty(request.UnitSortationResponsiblePartyCode),
                    UnitDemandBreakdownConfirmed = request.UnitDemandBreakdownConfirmed,
                    ImportedProductInfoRegistrationRequired = request.ImportedProductInfoRegistrationRequired,
                    ImportedProductInfoRegistered = request.ImportedProductInfoRegistered,
                    ProductInfoRegisteredByPartyCode = NormalizeUnitSortationResponsibleParty(request.ProductInfoRegisteredByPartyCode),
                    ProductInfoStorageLocationCode = NormalizeProductInfoStorageLocation(request.ProductInfoStorageLocationCode),
                    ProductInfoStorageConfirmed = request.ProductInfoStorageConfirmed,
                    UnitPackageLabelingModeCode = NormalizeUnitPackageLabelingMode(request.UnitPackageLabelingModeCode),
                    UnitProductInfoStickerConfirmed = request.UnitProductInfoStickerConfirmed,
                    ProductInfoStickerBarcodeIncluded = request.ProductInfoStickerBarcodeIncluded,
                    ProductInfoStickerMatchesImportedProductConfirmed = request.ProductInfoStickerMatchesImportedProductConfirmed,
                    UnitInvoiceIssuedConfirmed = request.UnitInvoiceIssuedConfirmed,
                    UnitPackageLabelsConfirmed = request.UnitPackageLabelsConfirmed,
                    UnitBarcodeScanLookupEnabled = request.UnitBarcodeScanLookupEnabled,
                    UnitBarcodeSchemeCode = NormalizeUnitBarcodeScheme(request.UnitBarcodeSchemeCode),
                    UnitBarcodeLookupDataConfirmed = request.UnitBarcodeLookupDataConfirmed,
                    UnitBarcodeMapsToMaskedRecipientConfirmed = request.UnitBarcodeMapsToMaskedRecipientConfirmed,
                    UnitBarcodeMapsToDemandQuantityConfirmed = request.UnitBarcodeMapsToDemandQuantityConfirmed,
                    LoadingSequenceConfirmed = request.LoadingSequenceConfirmed,
                    SortedUnitPackageCount = request.SortedUnitPackageCount > 0 ? request.SortedUnitPackageCount : null,
                    RequiredActionCodes = unitSortationActionCodes,
                    Memo = BuildUnitSortationMemo(request, destinationType, distributionResponsibility)
                },
                RecipientAddressPrivacyConfirmed = request.RecipientAddressPrivacyConfirmed,
                DistributionPrivacyModeCode = distributionPrivacyMode,
                DistributionResponsibilityCode = distributionResponsibility,
                DistributionResponsibilityConfirmed = request.DistributionResponsibilityConfirmed,
                TransportDecisionConfirmed = request.TransportDecisionConfirmed,
                TransportDecisionLocked = request.TransportDecisionLocked,
                TransportDecisionRevisionRequested = request.TransportDecisionRevisionRequested,
                TransportDecisionRevisionReason = request.TransportDecisionRevisionReason.Trim(),
                RequiredDistributionEvidenceCodes = distributionEvidenceCodes,
                DestinationMemo = BuildDestinationMemo(fulfillmentPlan, request, destinationType, unitDistributionMode)
            },
            ColdChainPlan = new PlatformEntrustedColdChainPlanDto
            {
                TemperatureCode = temperatureCode,
                RequiresColdChain = requiresColdChain,
                ColdChainVehicleConfirmed = request.ColdChainVehicleConfirmed,
                ThreePlColdChainFacilityConfirmed = request.ThreePlColdChainFacilityConfirmed,
                SelectedDestinationColdChainCompatible = IsDestinationColdChainCompatible(request, destinationType, requiresColdChain),
                RequiredActionCodes = coldChainActionCodes,
                Memo = BuildColdChainMemo(destinationType, requiresColdChain, request)
            },
            DestinationCostOptions = destinationCostOptions,
            DriverPayoutPlan = new PlatformEntrustedDriverPayoutPlanDto
            {
                PlatformCollectsOrdererPayments = request.PlatformCollectsOrdererPayments,
                PlatformHoldsFundsUntilDropoff = request.PlatformHoldsFundsUntilDropoff,
                OrdererPaymentMethodCodes = ordererPaymentMethodCodes,
                OrdererPaymentCollectionConfirmed = request.OrdererPaymentCollectionConfirmed,
                DriverPayoutTriggerCode = driverPayoutTrigger,
                DriverPayoutDelayDays = driverPayoutDelayDays,
                DropoffCompletedAtUtc = request.DropoffCompletedAtUtc,
                DriverPayoutDueAtUtc = request.DropoffCompletedAtUtc?.AddDays(driverPayoutDelayDays),
                PayoutRecipientType = "Driver",
                PayoutAccountPolicyCode = GroupPurchaseDomesticTransportDriverPayoutAccountPolicyCode.DriverRegisteredSettlementAccount,
                DriverSettlementAccountConfirmed = request.DriverSettlementAccountConfirmed,
                RequirePickupEvidence = request.RequirePickupEvidence,
                RequireDropoffEvidence = request.RequireDropoffEvidence,
                RequireReceiptEvidence = request.RequireReceiptEvidence,
                RequireCashReceipt = request.RequireCashReceipt,
                RequiredEvidenceCodes = requiredEvidenceCodes
            }
        };
    }

    private static IEnumerable<string> ResolveRequiredActions(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType,
        bool requiresColdChain,
        string distributionResponsibility)
    {
        if (string.IsNullOrWhiteSpace(request.PlatformShipperUserId))
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmPlatformShipperProfile;
        }

        if (!request.CustomsReleaseReady)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmCustomsReleaseOrBondedRelease;
        }

        if (string.IsNullOrWhiteSpace(request.PickupRoadAddress))
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmBondedAreaPickupAddress;
        }

        if (!request.TransportDecisionConfirmed ||
            (request.TransportDecisionLocked && request.TransportDecisionRevisionRequested))
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmTransportDecisionRevision;
        }

        if (string.IsNullOrWhiteSpace(request.DropoffRoadAddress))
        {
            yield return destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution
                ? GroupPurchaseDomesticTransportRequiredActionCode.ConfirmApartmentComplexDropoffAddress
                : GroupPurchaseDomesticTransportRequiredActionCode.ConfirmThreePlDropoffAddress;
        }

        if (destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution &&
            request.DriverPerformsApartmentUnitDistribution)
        {
            if (!request.ApartmentUnitDistributionPlanConfirmed ||
                request.ApartmentUnitDeliveryCount is null or <= 0 ||
                string.Equals(
                    NormalizeApartmentUnitDistributionMode(request.ApartmentUnitDistributionModeCode, request.DriverPerformsApartmentUnitDistribution),
                    GroupPurchaseApartmentUnitDistributionModeCode.None,
                    StringComparison.OrdinalIgnoreCase))
            {
                yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmApartmentUnitDistributionPlan;
            }

            if (!request.RecipientAddressPrivacyConfirmed)
            {
                yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmRecipientAddressPrivacy;
            }
        }

        if (destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution &&
            !request.DistributionResponsibilityConfirmed)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmDistributionResponsibility;
        }

        foreach (var actionCode in ResolveUnitSortationRequiredActions(request, destinationType, distributionResponsibility))
        {
            yield return actionCode;
        }

        foreach (var actionCode in ResolveColdChainRequiredActions(request, destinationType, requiresColdChain))
        {
            yield return actionCode;
        }

        if (!request.CargoWeightKg.HasValue && !request.CargoVolumeCbm.HasValue && !request.PalletCount.HasValue)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmCargoSpecification;
        }

        if (request.RequireAdminConfirmation)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmPlatformEntrustedTransport;
        }

        if (request.PlatformCollectsOrdererPayments &&
            (!request.OrdererPaymentCollectionConfirmed || request.OrdererPaymentMethodCodes.Count == 0))
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmOrdererPaymentCollection;
        }

        if (!request.DriverSettlementAccountConfirmed)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmDriverSettlementAccount;
        }

        if (request.DriverPayoutDelayDays is < 0 or > 30 ||
            string.IsNullOrWhiteSpace(request.DriverPayoutTriggerCode))
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmDriverPayoutPolicy;
        }
    }

    private static IEnumerable<string> ResolveUnitSortationRequiredActions(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType,
        string distributionResponsibility)
    {
        if (destinationType != GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution ||
            distributionResponsibility != GroupPurchaseApartmentDistributionResponsibilityCode.Driver ||
            !request.DriverPerformsApartmentUnitDistribution ||
            !request.UnitSortationBeforePickupRequired)
        {
            yield break;
        }

        var labelingMode = NormalizeUnitPackageLabelingMode(request.UnitPackageLabelingModeCode);
        if (request.ImportedProductInfoRegistrationRequired &&
            !request.ImportedProductInfoRegistered)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmImportedProductInfoRegistration;
        }

        if (request.ImportedProductInfoRegistrationRequired &&
            !request.ProductInfoStorageConfirmed)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmProductInfoStickerStorage;
        }

        if (!request.UnitSortationBeforePickupConfirmed ||
            !request.UnitDemandBreakdownConfirmed ||
            !IsUnitPackageLabelingConfirmed(request, labelingMode) ||
            !request.LoadingSequenceConfirmed ||
            request.SortedUnitPackageCount is null or <= 0)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmUnitSortationBeforePickup;
        }

        var responsibleParty = NormalizeUnitSortationResponsibleParty(request.UnitSortationResponsiblePartyCode);
        if (labelingMode == GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel &&
            responsibleParty is GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller or GroupPurchaseUnitSortationResponsiblePartyCode.OverseasForwarder &&
            (!request.UnitInvoiceIssuedConfirmed || !request.UnitPackageLabelsConfirmed || !request.UnitDemandBreakdownConfirmed))
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmOverseasUnitInvoiceAndLabeling;
        }

        if (labelingMode == GroupPurchaseUnitPackageLabelingModeCode.ProductInfoSticker &&
            (!request.UnitProductInfoStickerConfirmed || !request.ProductInfoStickerMatchesImportedProductConfirmed))
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmUnitProductInfoSticker;
        }

        if (request.UnitBarcodeScanLookupEnabled &&
            (!request.UnitBarcodeLookupDataConfirmed ||
             !request.UnitBarcodeMapsToMaskedRecipientConfirmed ||
             !request.UnitBarcodeMapsToDemandQuantityConfirmed))
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmUnitBarcodeScanLookup;
        }
    }

    private static IEnumerable<string> ResolveColdChainRequiredActions(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType,
        bool requiresColdChain)
    {
        if (!requiresColdChain)
        {
            yield break;
        }

        if (!request.ColdChainVehicleConfirmed)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmColdChainVehicle;
        }

        if (destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse &&
            !request.ThreePlColdChainFacilityConfirmed)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmColdChainThreePlFacility;
        }
    }

    private static IEnumerable<PlatformEntrustedDestinationCostOptionDto> BuildDestinationCostOptions(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        GroupPurchaseCommerceFulfillmentPlanDto fulfillmentPlan,
        string selectedDestinationType,
        bool requiresColdChain)
    {
        yield return BuildCostOption(
            request,
            fulfillmentPlan,
            GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse,
            "3PL warehouse inbound",
            GroupPurchaseApartmentDistributionResponsibilityCode.None,
            ResolveRouteFare(
                request.EstimatedThreePlTransportFareKrw,
                selectedDestinationType,
                GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse,
                request.EstimatedFareKrw),
            request.EstimatedThreePlInboundFeeKrw,
            request.EstimatedThreePlStorageFeeKrw,
            null,
            requiresColdChain);

        yield return BuildCostOption(
            request,
            fulfillmentPlan,
            GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
            "Apartment direct dropoff; distribution by orderer group or separate worker",
            GroupPurchaseApartmentDistributionResponsibilityCode.SeparateWorker,
            ResolveRouteFare(
                request.EstimatedApartmentDirectTransportFareKrw,
                selectedDestinationType,
                GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
                request.EstimatedFareKrw),
            null,
            null,
            request.EstimatedSeparateWorkerDistributionFeeKrw,
            requiresColdChain);

        yield return BuildCostOption(
            request,
            fulfillmentPlan,
            GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
            "Apartment direct; driver performs unit distribution",
            GroupPurchaseApartmentDistributionResponsibilityCode.Driver,
            ResolveRouteFare(
                request.EstimatedApartmentDirectTransportFareKrw,
                selectedDestinationType,
                GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
                request.EstimatedFareKrw),
            null,
            null,
            request.EstimatedDriverUnitDistributionFeeKrw,
            requiresColdChain);

        yield return BuildCostOption(
            request,
            fulfillmentPlan,
            GroupPurchaseDomesticTransportDestinationTypeCode.OrdererGroupRepresentativeDropoff,
            "Orderer group representative dropoff",
            GroupPurchaseApartmentDistributionResponsibilityCode.OrdererGroup,
            ResolveRouteFare(
                request.EstimatedRepresentativeDropoffTransportFareKrw,
                selectedDestinationType,
                GroupPurchaseDomesticTransportDestinationTypeCode.OrdererGroupRepresentativeDropoff,
                request.EstimatedFareKrw),
            null,
            null,
            null,
            requiresColdChain);
    }

    private static PlatformEntrustedDestinationCostOptionDto BuildCostOption(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        GroupPurchaseCommerceFulfillmentPlanDto fulfillmentPlan,
        string destinationType,
        string optionName,
        string distributionResponsibility,
        int? transportFare,
        int? inboundFee,
        int? storageFee,
        int? distributionFee,
        bool requiresColdChain)
    {
        var requiredActions = ResolveCostOptionRequiredActions(request, destinationType, distributionResponsibility, requiresColdChain).ToArray();
        var total = SumKnownCosts(transportFare, inboundFee, storageFee, distributionFee);
        var compatibleWithTemperature = requiredActions.All(action =>
            action != GroupPurchaseDomesticTransportRequiredActionCode.ConfirmColdChainVehicle &&
            action != GroupPurchaseDomesticTransportRequiredActionCode.ConfirmColdChainThreePlFacility);

        return new PlatformEntrustedDestinationCostOptionDto
        {
            DestinationTypeCode = destinationType,
            OptionName = optionName,
            DistributionResponsibilityCode = distributionResponsibility,
            CompatibleWithTemperature = compatibleWithTemperature,
            StatusCode = requiredActions.Length == 0
                ? GroupPurchaseDomesticTransportCostOptionStatusCode.Selectable
                : GroupPurchaseDomesticTransportCostOptionStatusCode.NeedsConfirmation,
            EstimatedTransportFareKrw = transportFare,
            EstimatedInboundFeeKrw = inboundFee,
            EstimatedStorageFeeKrw = storageFee,
            EstimatedDistributionFeeKrw = distributionFee,
            EstimatedTotalCostKrw = total,
            EstimatedCostPerKgKrw = ToCostPerKg(total, request.CargoWeightKg),
            RequiredActionCodes = requiredActions,
            Memo = BuildCostOptionMemo(fulfillmentPlan, destinationType, distributionResponsibility, requiresColdChain)
        };
    }

    private static IEnumerable<string> ResolveCostOptionRequiredActions(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType,
        string distributionResponsibility,
        bool requiresColdChain)
    {
        foreach (var action in ResolveColdChainRequiredActions(request, destinationType, requiresColdChain))
        {
            yield return action;
        }

        if (destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution &&
            distributionResponsibility != GroupPurchaseApartmentDistributionResponsibilityCode.OrdererGroup &&
            !request.DistributionResponsibilityConfirmed)
        {
            yield return GroupPurchaseDomesticTransportRequiredActionCode.ConfirmDistributionResponsibility;
        }
    }

    private static int? ResolveRouteFare(
        int? routeSpecificFare,
        string selectedDestinationType,
        string optionDestinationType,
        int? selectedEstimatedFare)
    {
        if (routeSpecificFare.HasValue)
        {
            return routeSpecificFare;
        }

        return selectedDestinationType == optionDestinationType ? selectedEstimatedFare : null;
    }

    private static int? SumKnownCosts(params int?[] costs)
    {
        var knownCosts = costs.Where(cost => cost.HasValue).Select(cost => Math.Max(0, cost!.Value)).ToArray();
        return knownCosts.Length == 0 ? null : knownCosts.Sum();
    }

    private static decimal? ToCostPerKg(int? totalCost, decimal? cargoWeightKg)
    {
        if (!totalCost.HasValue || !cargoWeightKg.HasValue || cargoWeightKg.Value <= 0m)
        {
            return null;
        }

        return decimal.Round(totalCost.Value / cargoWeightKg.Value, 0, MidpointRounding.AwayFromZero);
    }

    private static string ResolveSourceRequestType(string? transportMode)
    {
        if (string.Equals(transportMode, GroupPurchaseDomesticTransportModeCode.Fcl, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportSourceRequestTypeCode.FclCargoTransport;
        }

        if (string.Equals(transportMode, GroupPurchaseDomesticTransportModeCode.Lcl, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportSourceRequestTypeCode.LclCargoTransport;
        }

        return GroupPurchaseDomesticTransportSourceRequestTypeCode.ImportCargoTransport;
    }

    private static string NormalizeDestinationType(string? value)
    {
        if (string.Equals(value, GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution;
        }

        if (string.Equals(value, GroupPurchaseDomesticTransportDestinationTypeCode.OrdererGroupRepresentativeDropoff, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportDestinationTypeCode.OrdererGroupRepresentativeDropoff;
        }

        return GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse;
    }

    private static string NormalizeApartmentUnitDistributionMode(string? value, bool driverPerformsApartmentUnitDistribution)
    {
        if (!driverPerformsApartmentUnitDistribution)
        {
            return GroupPurchaseApartmentUnitDistributionModeCode.None;
        }

        if (string.Equals(value, GroupPurchaseApartmentUnitDistributionModeCode.DriverToUnitDoor, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseApartmentUnitDistributionModeCode.DriverToUnitDoor;
        }

        if (string.Equals(value, GroupPurchaseApartmentUnitDistributionModeCode.DriverToBuildingEntrance, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseApartmentUnitDistributionModeCode.DriverToBuildingEntrance;
        }

        if (string.Equals(value, GroupPurchaseApartmentUnitDistributionModeCode.DriverToDesignatedPickupPoint, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseApartmentUnitDistributionModeCode.DriverToDesignatedPickupPoint;
        }

        return GroupPurchaseApartmentUnitDistributionModeCode.DriverToBuildingEntrance;
    }

    private static string NormalizeDistributionPrivacyMode(string? value)
    {
        if (string.Equals(value, GroupPurchaseApartmentUnitDistributionPrivacyModeCode.FullUnitAddressAfterAssignment, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseApartmentUnitDistributionPrivacyModeCode.FullUnitAddressAfterAssignment;
        }

        if (string.Equals(value, GroupPurchaseApartmentUnitDistributionPrivacyModeCode.ManualChecklist, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseApartmentUnitDistributionPrivacyModeCode.ManualChecklist;
        }

        return GroupPurchaseApartmentUnitDistributionPrivacyModeCode.MaskedUnitTokens;
    }

    private static string NormalizeUnitSortationMode(string? value, bool unitSortationRequired)
    {
        if (!unitSortationRequired)
        {
            return GroupPurchaseUnitSortationModeCode.NotRequired;
        }

        if (string.Equals(value, GroupPurchaseUnitSortationModeCode.ByRouteSequence, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitSortationModeCode.ByRouteSequence;
        }

        if (string.Equals(value, GroupPurchaseUnitSortationModeCode.ByBuilding, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitSortationModeCode.ByBuilding;
        }

        return GroupPurchaseUnitSortationModeCode.ByBuildingAndUnit;
    }

    private static string NormalizeUnitSortationLocation(string? value)
    {
        if (string.Equals(value, GroupPurchaseUnitSortationLocationCode.ThreePlWarehouse, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitSortationLocationCode.ThreePlWarehouse;
        }

        if (string.Equals(value, GroupPurchaseUnitSortationLocationCode.ApartmentStagingArea, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitSortationLocationCode.ApartmentStagingArea;
        }

        return GroupPurchaseUnitSortationLocationCode.BondedArea;
    }

    private static string NormalizeUnitSortationResponsibleParty(string? value)
    {
        if (string.Equals(value, GroupPurchaseUnitSortationResponsiblePartyCode.OverseasForwarder, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitSortationResponsiblePartyCode.OverseasForwarder;
        }

        if (string.Equals(value, GroupPurchaseUnitSortationResponsiblePartyCode.Platform, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitSortationResponsiblePartyCode.Platform;
        }

        if (string.Equals(value, GroupPurchaseUnitSortationResponsiblePartyCode.OrdererGroup, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitSortationResponsiblePartyCode.OrdererGroup;
        }

        if (string.Equals(value, GroupPurchaseUnitSortationResponsiblePartyCode.DomesticOperator, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitSortationResponsiblePartyCode.DomesticOperator;
        }

        return GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller;
    }

    private static string NormalizeUnitPackageLabelingMode(string? value)
    {
        if (string.Equals(value, GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel;
        }

        if (string.Equals(value, GroupPurchaseUnitPackageLabelingModeCode.NoUnitLabel, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitPackageLabelingModeCode.NoUnitLabel;
        }

        return GroupPurchaseUnitPackageLabelingModeCode.ProductInfoSticker;
    }

    private static bool IsUnitPackageLabelingConfirmed(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string labelingMode)
        => labelingMode switch
        {
            GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel =>
                request.UnitInvoiceIssuedConfirmed && request.UnitPackageLabelsConfirmed,
            GroupPurchaseUnitPackageLabelingModeCode.NoUnitLabel => true,
            _ => request.UnitProductInfoStickerConfirmed &&
                 request.ProductInfoStickerMatchesImportedProductConfirmed
        };

    private static string NormalizeProductInfoStorageLocation(string? value)
    {
        if (string.Equals(value, GroupPurchaseProductInfoStorageLocationCode.OverseasSellerSystem, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseProductInfoStorageLocationCode.OverseasSellerSystem;
        }

        if (string.Equals(value, GroupPurchaseProductInfoStorageLocationCode.OverseasForwarderSystem, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseProductInfoStorageLocationCode.OverseasForwarderSystem;
        }

        if (string.Equals(value, GroupPurchaseProductInfoStorageLocationCode.ManualDocumentArchive, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseProductInfoStorageLocationCode.ManualDocumentArchive;
        }

        return GroupPurchaseProductInfoStorageLocationCode.PlatformImportLedger;
    }

    private static string NormalizeUnitBarcodeScheme(string? value)
    {
        if (string.Equals(value, GroupPurchaseUnitBarcodeSchemeCode.InvoiceNumberBarcode, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitBarcodeSchemeCode.InvoiceNumberBarcode;
        }

        if (string.Equals(value, GroupPurchaseUnitBarcodeSchemeCode.PackageIdBarcode, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseUnitBarcodeSchemeCode.PackageIdBarcode;
        }

        return GroupPurchaseUnitBarcodeSchemeCode.OrderNumberBarcode;
    }

    private static string NormalizeDistributionResponsibility(
        string? value,
        bool driverPerformsApartmentUnitDistribution,
        string destinationType)
    {
        if (destinationType != GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution)
        {
            return GroupPurchaseApartmentDistributionResponsibilityCode.None;
        }

        if (driverPerformsApartmentUnitDistribution ||
            string.Equals(value, GroupPurchaseApartmentDistributionResponsibilityCode.Driver, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseApartmentDistributionResponsibilityCode.Driver;
        }

        if (string.Equals(value, GroupPurchaseApartmentDistributionResponsibilityCode.SeparateWorker, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseApartmentDistributionResponsibilityCode.SeparateWorker;
        }

        if (string.Equals(value, GroupPurchaseApartmentDistributionResponsibilityCode.PlatformArrangedWorker, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseApartmentDistributionResponsibilityCode.PlatformArrangedWorker;
        }

        return GroupPurchaseApartmentDistributionResponsibilityCode.OrdererGroup;
    }

    private static string NormalizeTemperatureCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GroupPurchaseTemperatureCode.Ambient;
        }

        var normalized = value.Trim();
        if (string.Equals(normalized, GroupPurchaseTemperatureCode.Frozen, StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("frozen", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("냉동", StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseTemperatureCode.Frozen;
        }

        if (string.Equals(normalized, GroupPurchaseTemperatureCode.Chilled, StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("chilled", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("refrigerated", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("냉장", StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseTemperatureCode.Chilled;
        }

        return GroupPurchaseTemperatureCode.Ambient;
    }

    private static bool ResolveRequiresColdChain(GroupPurchasePlatformDomesticTransportDraftRequest request, string temperatureCode)
        => request.RequiresColdChain ||
            temperatureCode is GroupPurchaseTemperatureCode.Chilled or GroupPurchaseTemperatureCode.Frozen;

    private static string NormalizeSettlementPolicy(string? value)
    {
        if (string.Equals(value, GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff;
        }

        if (string.Equals(value, GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformPaysAndRechargesOrdererGroup, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformPaysAndRechargesOrdererGroup;
        }

        if (string.Equals(value, GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformAbsorbsAsPromotion, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformAbsorbsAsPromotion;
        }

        if (string.Equals(value, GroupPurchaseDomesticTransportSettlementPolicyCode.ManualSettlement, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportSettlementPolicyCode.ManualSettlement;
        }

        return GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff;
    }

    private static IEnumerable<string> NormalizeOrdererPaymentMethods(IReadOnlyList<string>? values)
    {
        var source = values is { Count: > 0 }
            ? values
            :
            [
                GroupPurchaseDomesticTransportOrdererPaymentMethodCode.Card,
                GroupPurchaseDomesticTransportOrdererPaymentMethodCode.CashLike
            ];

        return source
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(NormalizeOrdererPaymentMethod)
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static string NormalizeOrdererPaymentMethod(string value)
    {
        if (string.Equals(value, GroupPurchaseDomesticTransportOrdererPaymentMethodCode.Card, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportOrdererPaymentMethodCode.Card;
        }

        if (string.Equals(value, GroupPurchaseDomesticTransportOrdererPaymentMethodCode.CashLike, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportOrdererPaymentMethodCode.CashLike;
        }

        if (string.Equals(value, GroupPurchaseDomesticTransportOrdererPaymentMethodCode.BankTransfer, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportOrdererPaymentMethodCode.BankTransfer;
        }

        if (string.Equals(value, GroupPurchaseDomesticTransportOrdererPaymentMethodCode.PlatformCredit, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportOrdererPaymentMethodCode.PlatformCredit;
        }

        return value.Trim();
    }

    private static string NormalizeDriverPayoutTrigger(string? value)
    {
        if (string.Equals(value, GroupPurchaseDomesticTransportDriverPayoutTriggerCode.PickupAndDropoffEvidenceVerified, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportDriverPayoutTriggerCode.PickupAndDropoffEvidenceVerified;
        }

        if (string.Equals(value, GroupPurchaseDomesticTransportDriverPayoutTriggerCode.ManualAdminApproval, StringComparison.OrdinalIgnoreCase))
        {
            return GroupPurchaseDomesticTransportDriverPayoutTriggerCode.ManualAdminApproval;
        }

        return GroupPurchaseDomesticTransportDriverPayoutTriggerCode.DropoffCompleted;
    }

    private static int NormalizeDriverPayoutDelayDays(int value)
        => Math.Clamp(value, 0, 30);

    private static IEnumerable<string> ResolveRequiredEvidenceCodes(GroupPurchasePlatformDomesticTransportDraftRequest request)
    {
        if (request.RequirePickupEvidence)
        {
            yield return GroupPurchaseDomesticTransportEvidenceCode.PickupPhoto;
        }

        if (request.RequireDropoffEvidence)
        {
            yield return GroupPurchaseDomesticTransportEvidenceCode.DropoffPhoto;
        }

        yield return GroupPurchaseDomesticTransportEvidenceCode.DropoffCompletion;

        if (request.RequireReceiptEvidence)
        {
            yield return GroupPurchaseDomesticTransportEvidenceCode.Receipt;
        }

        if (request.RequireCashReceipt)
        {
            yield return GroupPurchaseDomesticTransportEvidenceCode.CashReceipt;
        }
    }

    private static IEnumerable<string> ResolveRequiredDistributionEvidenceCodes(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType)
    {
        if (destinationType != GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution ||
            !request.DriverPerformsApartmentUnitDistribution)
        {
            yield break;
        }

        yield return "UnitSortationManifest";
        var labelingMode = NormalizeUnitPackageLabelingMode(request.UnitPackageLabelingModeCode);
        if (labelingMode == GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel)
        {
            yield return "UnitInvoiceLabels";
        }
        else if (labelingMode == GroupPurchaseUnitPackageLabelingModeCode.ProductInfoSticker)
        {
            yield return "UnitProductInfoStickers";
        }

        if (request.UnitBarcodeScanLookupEnabled)
        {
            yield return "UnitBarcodeScanLookup";
        }

        yield return "LoadingSequenceManifest";
        yield return "ApartmentUnitDistributionChecklist";
        yield return "ApartmentUnitDeliveryCompletion";
    }

    private static string ResolveDestinationName(
        GroupPurchaseCommerceFulfillmentPlanDto fulfillmentPlan,
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType)
    {
        if (destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution)
        {
            if (!string.IsNullOrWhiteSpace(request.ApartmentComplexName))
            {
                return request.ApartmentComplexName.Trim();
            }

            return fulfillmentPlan.OrdererGroupScopeName;
        }

        if (!string.IsNullOrWhiteSpace(request.ThreePlWarehouseName))
        {
            return request.ThreePlWarehouseName.Trim();
        }

        return string.IsNullOrWhiteSpace(request.DropoffRoadAddress)
            ? fulfillmentPlan.OrdererGroupScopeName
            : request.DropoffRoadAddress.Trim();
    }

    private static string NormalizePlatformUserId(string? value)
        => string.IsNullOrWhiteSpace(value) ? "platform" : value.Trim();

    private static string BuildClientRequestId(
        GroupPurchaseCommerceFulfillmentPlanDto fulfillmentPlan,
        string sourceRequestType,
        string destinationType)
        => $"GP-IMPORT-DOMESTIC-{sourceRequestType}-{destinationType}-{fulfillmentPlan.PlanId}".ToUpperInvariant();

    private static string BuildSettlementMemo(
        GroupPurchaseCommerceFulfillmentPlanDto fulfillmentPlan,
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string settlementPolicy,
        IReadOnlyList<string> ordererPaymentMethodCodes,
        string driverPayoutTriggerCode,
        int driverPayoutDelayDays,
        string destinationType)
    {
        var fare = request.EstimatedFareKrw.HasValue ? $"{request.EstimatedFareKrw.Value:N0} KRW" : "fare TBD";
        var routeLabel = destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution
            ? "bonded-area-to-apartment-direct-distribution"
            : "bonded-area-to-3PL";

        if (string.Equals(settlementPolicy, GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff, StringComparison.OrdinalIgnoreCase))
        {
            var paymentMethods = ordererPaymentMethodCodes.Count > 0
                ? string.Join(", ", ordererPaymentMethodCodes)
                : "payment method TBD";

            return $"Platform acts as shipper for {routeLabel} transport. Cost owner: orderer group {fulfillmentPlan.OrdererGroupScopeKey}. Platform collects orderer payments ({paymentMethods}), holds funds until {driverPayoutTriggerCode}, then pays the driver to the registered settlement account after {driverPayoutDelayDays} day(s). Estimated fare: {fare}.";
        }

        return $"Platform acts as shipper for {routeLabel} transport. Cost owner: orderer group {fulfillmentPlan.OrdererGroupScopeKey}. Policy: {settlementPolicy}. Estimated fare: {fare}.";
    }

    private static bool IsDestinationColdChainCompatible(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType,
        bool requiresColdChain)
    {
        if (!requiresColdChain)
        {
            return true;
        }

        return request.ColdChainVehicleConfirmed &&
            (destinationType != GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse ||
             request.ThreePlColdChainFacilityConfirmed);
    }

    private static string BuildColdChainMemo(
        string destinationType,
        bool requiresColdChain,
        GroupPurchasePlatformDomesticTransportDraftRequest request)
    {
        if (!requiresColdChain)
        {
            return "Ambient cargo. Cold-chain vehicle or 3PL cold storage confirmation is not required.";
        }

        if (destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse)
        {
            return request.ThreePlColdChainFacilityConfirmed
                ? "Cold-chain cargo. Cold-chain vehicle and 3PL refrigerated/frozen facility must remain confirmed before dispatch."
                : "Cold-chain cargo. Select only a 3PL warehouse with refrigerated/frozen storage capability.";
        }

        return "Cold-chain cargo. The delegated driver route must use a confirmed refrigerated/frozen-capable vehicle through apartment or representative dropoff.";
    }

    private static string BuildCostOptionMemo(
        GroupPurchaseCommerceFulfillmentPlanDto fulfillmentPlan,
        string destinationType,
        string distributionResponsibility,
        bool requiresColdChain)
    {
        var coldChainMemo = requiresColdChain ? " Cold-chain confirmations are required." : string.Empty;
        return destinationType switch
        {
            GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse
                => $"Inbound to 3PL for group purchase {fulfillmentPlan.GroupPurchaseId}; useful for storage, sales-channel listing, and later outbound batch.{coldChainMemo}",
            GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution
                => $"Direct apartment route with distribution responsibility: {distributionResponsibility}.{coldChainMemo}",
            GroupPurchaseDomesticTransportDestinationTypeCode.OrdererGroupRepresentativeDropoff
                => $"Dropoff to orderer group representative point; internal distribution is outside driver scope.{coldChainMemo}",
            _ => $"Domestic route option for group purchase {fulfillmentPlan.GroupPurchaseId}.{coldChainMemo}"
        };
    }

    private static string BuildUnitSortationMemo(
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType,
        string distributionResponsibility)
    {
        if (destinationType != GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution ||
            distributionResponsibility != GroupPurchaseApartmentDistributionResponsibilityCode.Driver ||
            !request.DriverPerformsApartmentUnitDistribution)
        {
            return "Driver unit delivery is not in scope, so unit-level pre-sortation is not required for dispatch.";
        }

        var responsibleParty = NormalizeUnitSortationResponsibleParty(request.UnitSortationResponsiblePartyCode);
        var sortationMode = NormalizeUnitSortationMode(request.UnitSortationModeCode, request.UnitSortationBeforePickupRequired);
        var location = NormalizeUnitSortationLocation(request.UnitSortationLocationCode);
        var labelingMode = NormalizeUnitPackageLabelingMode(request.UnitPackageLabelingModeCode);
        var productInfoRegisteredBy = NormalizeUnitSortationResponsibleParty(request.ProductInfoRegisteredByPartyCode);
        var productInfoStorage = NormalizeProductInfoStorageLocation(request.ProductInfoStorageLocationCode);
        var barcodeScheme = NormalizeUnitBarcodeScheme(request.UnitBarcodeSchemeCode);
        var packageCount = request.SortedUnitPackageCount > 0 ? request.SortedUnitPackageCount.Value.ToString("N0") : "TBD";
        var barcodeMemo = request.UnitBarcodeScanLookupEnabled
            ? $" Barcode scheme: {barcodeScheme}; scans must resolve to masked recipient, unit, demand quantity, and delivery sequence."
            : " Barcode scan lookup is optional and currently not required.";

        return $"Driver unit-door delivery requires pre-sortation before pickup. Responsible party: {responsibleParty}. Mode: {sortationMode}. Location: {location}. Sorted unit packages: {packageCount}. Labeling mode: {labelingMode}. Product info registered by: {productInfoRegisteredBy}. Product info storage: {productInfoStorage}.{barcodeMemo}";
    }

    private static string BuildDestinationMemo(
        GroupPurchaseCommerceFulfillmentPlanDto fulfillmentPlan,
        GroupPurchasePlatformDomesticTransportDraftRequest request,
        string destinationType,
        string unitDistributionMode)
    {
        if (destinationType == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution)
        {
            var distributionScope = request.DriverPerformsApartmentUnitDistribution
                ? $"driver unit distribution mode: {unitDistributionMode}, unit deliveries: {(request.ApartmentUnitDeliveryCount > 0 ? request.ApartmentUnitDeliveryCount.Value.ToString("N0") : "TBD")}"
                : "driver completes apartment complex dropoff only";

            return $"Direct apartment route for orderer group {fulfillmentPlan.OrdererGroupScopeKey}; {distributionScope}.";
        }

        return $"3PL warehouse route for orderer group {fulfillmentPlan.OrdererGroupScopeKey}.";
    }
}
