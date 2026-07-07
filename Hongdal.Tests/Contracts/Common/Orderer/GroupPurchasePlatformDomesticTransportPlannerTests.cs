using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Tests.Contracts.Common.Orderer;

public sealed class GroupPurchasePlatformDomesticTransportPlannerTests
{
    [Fact]
    public void Plan_InformalOrdererGroup_UsesPlatformAsShipperAndOrdererGroupAsCostOwner()
    {
        var fulfillment = CreateFulfillmentPlan();

        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            fulfillment,
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                PlatformLegalEntityName = "Hongdal Platform",
                TransportMode = GroupPurchaseDomesticTransportModeCode.Lcl,
                DestinationTypeCode = GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse,
                DriverPerformsApartmentUnitDistribution = false,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                DriverPayoutDelayDays = 5,
                DropoffCompletedAtUtc = new DateTime(2026, 7, 7, 3, 0, 0, DateTimeKind.Utc),
                PickupRoadAddress = "Incheon bonded warehouse",
                PickupContactName = "Bonded warehouse",
                PickupContactPhone = "010-0000-0000",
                DropoffRoadAddress = "Gimpo 3PL center",
                DropoffContactName = "3PL inbound",
                DropoffContactPhone = "010-1111-1111",
                CargoWeightKg = 1200m,
                EstimatedFareKrw = 320000
            });

        Assert.True(result.ReadyForDispatchQueue);
        Assert.Equal(GroupPurchaseDomesticTransportPrincipalTypeCode.Platform, result.PrincipalType);
        Assert.Equal(GroupPurchaseDomesticTransportCostOwnerTypeCode.OrdererGroup, result.CostOwnerType);
        Assert.Equal(
            GroupPurchaseDomesticTransportSettlementPolicyCode.PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff,
            result.SettlementPolicyCode);
        Assert.Equal(GroupPurchaseDomesticTransportSourceRequestTypeCode.LclCargoTransport, result.SourceRequestType);
        Assert.Equal(20, result.DispatchBusinessTypeCode);
        Assert.Equal("platform-ops", result.CargoTransportDraft.PlatformShipperUserId);
        Assert.Equal(fulfillment.OrdererGroupScopeKey, result.CargoTransportDraft.OrdererGroupScopeKey);
        Assert.Equal("PlatformCollectedSettlement", result.CargoTransportDraft.PaymentMethodCode);
        Assert.Contains("Cost owner: orderer group", result.CargoTransportDraft.SettlementMemo);
        Assert.Contains("pays the driver", result.CargoTransportDraft.SettlementMemo);
        Assert.True(result.DriverPayoutPlan.PlatformCollectsOrdererPayments);
        Assert.True(result.DriverPayoutPlan.PlatformHoldsFundsUntilDropoff);
        Assert.True(result.DriverPayoutPlan.DriverSettlementAccountConfirmed);
        Assert.False(result.DriverPayoutPlan.RequireCashReceipt);
        Assert.Equal(5, result.DriverPayoutPlan.DriverPayoutDelayDays);
        Assert.Equal(new DateTime(2026, 7, 12, 3, 0, 0, DateTimeKind.Utc), result.DriverPayoutPlan.DriverPayoutDueAtUtc);
        Assert.Contains(GroupPurchaseDomesticTransportOrdererPaymentMethodCode.Card, result.DriverPayoutPlan.OrdererPaymentMethodCodes);
        Assert.Contains(GroupPurchaseDomesticTransportOrdererPaymentMethodCode.CashLike, result.DriverPayoutPlan.OrdererPaymentMethodCodes);
        Assert.Contains(GroupPurchaseDomesticTransportEvidenceCode.DropoffCompletion, result.DriverPayoutPlan.RequiredEvidenceCodes);
    }

    [Fact]
    public void Plan_CustomsReleaseNotReady_BlocksDispatchQueueDraft()
    {
        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            CreateFulfillmentPlan(),
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse,
                DriverPerformsApartmentUnitDistribution = false,
                PickupRoadAddress = "Pyeongtaek bonded warehouse",
                DropoffRoadAddress = "Hwaseong 3PL center",
                CargoWeightKg = 800m,
                CustomsReleaseReady = false,
                RequireAdminConfirmation = false
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmCustomsReleaseOrBondedRelease,
            result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_PaymentCollectionOrDriverAccountMissing_BlocksDispatchQueueDraft()
    {
        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            CreateFulfillmentPlan(),
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse,
                DriverPerformsApartmentUnitDistribution = false,
                PickupRoadAddress = "Pyeongtaek bonded warehouse",
                DropoffRoadAddress = "Hwaseong 3PL center",
                CargoWeightKg = 800m,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmOrdererPaymentCollection,
            result.RequiredActionCodes);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmDriverSettlementAccount,
            result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_DefaultDestination_UsesDriverHomeDeliveryAfterGroupDecision()
    {
        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            CreateFulfillmentPlan(),
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                ApartmentComplexName = "Hongdal Apartment",
                ApartmentUnitDeliveryCount = 36,
                ApartmentUnitDistributionPlanConfirmed = true,
                UnitSortationBeforePickupConfirmed = true,
                UnitSortationResponsiblePartyCode = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasForwarder,
                UnitDemandBreakdownConfirmed = true,
                ImportedProductInfoRegistered = true,
                ProductInfoRegisteredByPartyCode = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasForwarder,
                ProductInfoStorageLocationCode = GroupPurchaseProductInfoStorageLocationCode.OverseasForwarderSystem,
                ProductInfoStorageConfirmed = true,
                UnitPackageLabelingModeCode = GroupPurchaseUnitPackageLabelingModeCode.ProductInfoSticker,
                UnitProductInfoStickerConfirmed = true,
                ProductInfoStickerMatchesImportedProductConfirmed = true,
                LoadingSequenceConfirmed = true,
                SortedUnitPackageCount = 36,
                RecipientAddressPrivacyConfirmed = true,
                DistributionResponsibilityConfirmed = true,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "Incheon bonded warehouse",
                DropoffRoadAddress = "Seoul Hongdal Apartment 101",
                CargoWeightKg = 500m,
                EstimatedApartmentDirectTransportFareKrw = 170000,
                EstimatedDriverUnitDistributionFeeKrw = 90000
            });

        Assert.True(result.ReadyForDispatchQueue);
        Assert.Equal(
            GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
            result.DestinationPlan.DestinationTypeCode);
        Assert.True(result.DestinationPlan.DriverPerformsApartmentUnitDistribution);
        Assert.Equal(GroupPurchaseApartmentUnitDistributionModeCode.DriverToUnitDoor, result.DestinationPlan.ApartmentUnitDistributionModeCode);
        Assert.Equal(GroupPurchaseApartmentDistributionResponsibilityCode.Driver, result.DestinationPlan.DistributionResponsibilityCode);
        Assert.True(result.DestinationPlan.UnitSortationPlan.UnitSortationBeforePickupConfirmed);
        Assert.Equal(GroupPurchaseUnitSortationResponsiblePartyCode.OverseasForwarder, result.DestinationPlan.UnitSortationPlan.UnitSortationResponsiblePartyCode);
        Assert.True(result.DestinationPlan.UnitSortationPlan.ImportedProductInfoRegistered);
        Assert.Equal(GroupPurchaseProductInfoStorageLocationCode.OverseasForwarderSystem, result.DestinationPlan.UnitSortationPlan.ProductInfoStorageLocationCode);
        Assert.Equal(GroupPurchaseUnitPackageLabelingModeCode.ProductInfoSticker, result.DestinationPlan.UnitSortationPlan.UnitPackageLabelingModeCode);
        Assert.True(result.DestinationPlan.UnitSortationPlan.UnitProductInfoStickerConfirmed);
        Assert.False(result.DestinationPlan.UnitSortationPlan.UnitInvoiceIssuedConfirmed);
        Assert.False(result.DestinationPlan.UnitSortationPlan.UnitBarcodeScanLookupEnabled);
        Assert.Contains("UnitProductInfoStickers", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.DoesNotContain("UnitInvoiceLabels", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.DoesNotContain("UnitBarcodeScanLookup", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.True(result.DestinationPlan.TransportDecisionLocked);
        Assert.Contains(result.DestinationCostOptions, option =>
            option.DestinationTypeCode == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution &&
            option.DistributionResponsibilityCode == GroupPurchaseApartmentDistributionResponsibilityCode.Driver &&
            option.EstimatedTotalCostKrw == 260000);
    }

    [Fact]
    public void Plan_LockedTransportDecisionRevision_BlocksDispatchQueueDraft()
    {
        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            CreateFulfillmentPlan(),
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                ApartmentUnitDeliveryCount = 36,
                ApartmentUnitDistributionPlanConfirmed = true,
                RecipientAddressPrivacyConfirmed = true,
                DistributionResponsibilityConfirmed = true,
                TransportDecisionRevisionRequested = true,
                TransportDecisionRevisionReason = "Switch to 3PL after group decision",
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "Incheon bonded warehouse",
                DropoffRoadAddress = "Seoul Hongdal Apartment 101",
                CargoWeightKg = 500m
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.True(result.DestinationPlan.TransportDecisionLocked);
        Assert.True(result.DestinationPlan.TransportDecisionRevisionRequested);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmTransportDecisionRevision,
            result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_ApartmentDirectDistribution_AllowsDriverToDeliverToUnits()
    {
        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            CreateFulfillmentPlan(),
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                TransportMode = GroupPurchaseDomesticTransportModeCode.GeneralCargo,
                DestinationTypeCode = GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
                ApartmentComplexCode = "APT-001",
                ApartmentComplexName = "Hongdal Apartment",
                DriverPerformsApartmentUnitDistribution = true,
                ApartmentUnitDistributionModeCode = GroupPurchaseApartmentUnitDistributionModeCode.DriverToUnitDoor,
                ApartmentUnitDeliveryCount = 48,
                ApartmentUnitDistributionPlanConfirmed = true,
                UnitSortationBeforePickupConfirmed = true,
                UnitSortationResponsiblePartyCode = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller,
                UnitDemandBreakdownConfirmed = true,
                ImportedProductInfoRegistered = true,
                ProductInfoRegisteredByPartyCode = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller,
                ProductInfoStorageLocationCode = GroupPurchaseProductInfoStorageLocationCode.OverseasSellerSystem,
                ProductInfoStorageConfirmed = true,
                UnitPackageLabelingModeCode = GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel,
                UnitInvoiceIssuedConfirmed = true,
                UnitPackageLabelsConfirmed = true,
                UnitBarcodeScanLookupEnabled = true,
                UnitBarcodeSchemeCode = GroupPurchaseUnitBarcodeSchemeCode.OrderNumberBarcode,
                UnitBarcodeLookupDataConfirmed = true,
                UnitBarcodeMapsToMaskedRecipientConfirmed = true,
                UnitBarcodeMapsToDemandQuantityConfirmed = true,
                LoadingSequenceConfirmed = true,
                SortedUnitPackageCount = 48,
                RecipientAddressPrivacyConfirmed = true,
                DistributionResponsibilityConfirmed = true,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "Incheon bonded warehouse",
                PickupContactName = "Bonded warehouse",
                PickupContactPhone = "010-0000-0000",
                DropoffRoadAddress = "Seoul Hongdal Apartment 101",
                DropoffContactName = "Apartment distribution desk",
                DropoffContactPhone = "010-2222-2222",
                CargoWeightKg = 600m,
                EstimatedFareKrw = 180000,
                EstimatedThreePlTransportFareKrw = 230000,
                EstimatedThreePlInboundFeeKrw = 90000,
                EstimatedApartmentDirectTransportFareKrw = 180000,
                EstimatedDriverUnitDistributionFeeKrw = 120000,
                EstimatedSeparateWorkerDistributionFeeKrw = 70000
            });

        Assert.True(result.ReadyForDispatchQueue);
        Assert.Equal(
            GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
            result.DestinationPlan.DestinationTypeCode);
        Assert.Equal("Hongdal Apartment", result.DestinationPlan.DestinationName);
        Assert.True(result.DestinationPlan.DirectApartmentDistribution);
        Assert.True(result.DestinationPlan.DriverPerformsApartmentUnitDistribution);
        Assert.Equal(GroupPurchaseApartmentUnitDistributionModeCode.DriverToUnitDoor, result.DestinationPlan.ApartmentUnitDistributionModeCode);
        Assert.Equal(48, result.DestinationPlan.ApartmentUnitDeliveryCount);
        Assert.True(result.DestinationPlan.RecipientAddressPrivacyConfirmed);
        Assert.Contains("ApartmentUnitDistributionChecklist", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.Contains("UnitInvoiceLabels", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.Contains("UnitBarcodeScanLookup", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.True(result.DestinationPlan.UnitSortationPlan.UnitSortationBeforePickupConfirmed);
        Assert.Equal(GroupPurchaseUnitSortationResponsiblePartyCode.OverseasSeller, result.DestinationPlan.UnitSortationPlan.UnitSortationResponsiblePartyCode);
        Assert.Equal(GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel, result.DestinationPlan.UnitSortationPlan.UnitPackageLabelingModeCode);
        Assert.True(result.DestinationPlan.UnitSortationPlan.UnitBarcodeLookupDataConfirmed);
        Assert.Contains("bonded-area-to-apartment-direct-distribution", result.CargoTransportDraft.SettlementMemo);
        Assert.Equal(
            GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
            result.CargoTransportDraft.DestinationTypeCode);
        Assert.Equal(
            GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
            result.DispatchQueueDraft.DestinationTypeCode);
        Assert.True(result.DispatchQueueDraft.DriverPerformsApartmentUnitDistribution);
        Assert.Equal(GroupPurchaseApartmentUnitDistributionModeCode.DriverToUnitDoor, result.DispatchQueueDraft.ApartmentUnitDistributionModeCode);
        Assert.Equal(48, result.DispatchQueueDraft.ApartmentUnitDeliveryCount);
        Assert.Equal(GroupPurchaseApartmentDistributionResponsibilityCode.Driver, result.DispatchQueueDraft.DistributionResponsibilityCode);
        Assert.True(result.DispatchQueueDraft.UnitSortationBeforePickupConfirmed);
        Assert.Equal(GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel, result.DispatchQueueDraft.UnitPackageLabelingModeCode);
        Assert.True(result.DispatchQueueDraft.UnitInvoiceIssuedConfirmed);
        Assert.True(result.DispatchQueueDraft.UnitPackageLabelsConfirmed);
        Assert.True(result.DispatchQueueDraft.UnitBarcodeScanLookupEnabled);
        Assert.Equal(GroupPurchaseUnitBarcodeSchemeCode.OrderNumberBarcode, result.DispatchQueueDraft.UnitBarcodeSchemeCode);
        Assert.True(result.DispatchQueueDraft.UnitBarcodeMapsToMaskedRecipientConfirmed);
        Assert.Contains(result.DestinationCostOptions, option =>
            option.DestinationTypeCode == GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse &&
            option.EstimatedTotalCostKrw == 320000);
        Assert.Contains(result.DestinationCostOptions, option =>
            option.DestinationTypeCode == GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution &&
            option.DistributionResponsibilityCode == GroupPurchaseApartmentDistributionResponsibilityCode.Driver &&
            option.EstimatedTotalCostKrw == 300000);
    }

    [Fact]
    public void Plan_ApartmentDirectDistributionMissingUnitPlan_BlocksDispatchQueueDraft()
    {
        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            CreateFulfillmentPlan(),
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
                DriverPerformsApartmentUnitDistribution = true,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "Incheon bonded warehouse",
                DropoffRoadAddress = "Seoul Hongdal Apartment 101",
                CargoWeightKg = 600m
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmApartmentUnitDistributionPlan,
            result.RequiredActionCodes);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmRecipientAddressPrivacy,
            result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_DriverUnitDeliveryWithoutOverseasLabels_BlocksDispatchQueueDraft()
    {
        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            CreateFulfillmentPlan(),
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = GroupPurchaseDomesticTransportDestinationTypeCode.ApartmentComplexDirectDistribution,
                DriverPerformsApartmentUnitDistribution = true,
                ApartmentUnitDistributionModeCode = GroupPurchaseApartmentUnitDistributionModeCode.DriverToUnitDoor,
                ApartmentUnitDeliveryCount = 24,
                ApartmentUnitDistributionPlanConfirmed = true,
                UnitSortationResponsiblePartyCode = GroupPurchaseUnitSortationResponsiblePartyCode.OverseasForwarder,
                UnitDemandBreakdownConfirmed = true,
                UnitPackageLabelingModeCode = GroupPurchaseUnitPackageLabelingModeCode.UnitInvoiceLabel,
                UnitInvoiceIssuedConfirmed = false,
                UnitPackageLabelsConfirmed = false,
                UnitBarcodeScanLookupEnabled = true,
                LoadingSequenceConfirmed = false,
                SortedUnitPackageCount = 0,
                RecipientAddressPrivacyConfirmed = true,
                DistributionResponsibilityConfirmed = true,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "Incheon bonded warehouse",
                DropoffRoadAddress = "Seoul Hongdal Apartment 101",
                CargoWeightKg = 400m
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmUnitSortationBeforePickup,
            result.RequiredActionCodes);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmOverseasUnitInvoiceAndLabeling,
            result.RequiredActionCodes);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmUnitBarcodeScanLookup,
            result.RequiredActionCodes);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmOverseasUnitInvoiceAndLabeling,
            result.DestinationPlan.UnitSortationPlan.RequiredActionCodes);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmUnitBarcodeScanLookup,
            result.DestinationPlan.UnitSortationPlan.RequiredActionCodes);
        Assert.Equal(GroupPurchaseUnitSortationResponsiblePartyCode.OverseasForwarder, result.DestinationPlan.UnitSortationPlan.UnitSortationResponsiblePartyCode);
    }

    [Fact]
    public void Plan_ColdChainThreePlWithoutColdFacility_BlocksDispatchAndShowsCostConfirmation()
    {
        var result = GroupPurchasePlatformDomesticTransportPlanner.Plan(
            CreateFulfillmentPlan(),
            new GroupPurchasePlatformDomesticTransportDraftRequest
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse,
                TemperatureCondition = GroupPurchaseTemperatureCode.Frozen,
                ColdChainVehicleConfirmed = true,
                ThreePlColdChainFacilityConfirmed = false,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "Incheon bonded warehouse",
                DropoffRoadAddress = "Cold 3PL candidate",
                CargoWeightKg = 500m,
                EstimatedThreePlTransportFareKrw = 210000,
                EstimatedThreePlInboundFeeKrw = 80000,
                EstimatedThreePlStorageFeeKrw = 60000
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.True(result.ColdChainPlan.RequiresColdChain);
        Assert.Equal(GroupPurchaseTemperatureCode.Frozen, result.ColdChainPlan.TemperatureCode);
        Assert.False(result.ColdChainPlan.SelectedDestinationColdChainCompatible);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmColdChainThreePlFacility,
            result.RequiredActionCodes);
        Assert.Contains(
            GroupPurchaseDomesticTransportRequiredActionCode.ConfirmColdChainThreePlFacility,
            result.ColdChainPlan.RequiredActionCodes);
        Assert.Contains(result.DestinationCostOptions, option =>
            option.DestinationTypeCode == GroupPurchaseDomesticTransportDestinationTypeCode.ThreePlWarehouse &&
            option.StatusCode == GroupPurchaseDomesticTransportCostOptionStatusCode.NeedsConfirmation &&
            option.EstimatedTotalCostKrw == 350000);
    }

    private static GroupPurchaseCommerceFulfillmentPlanDto CreateFulfillmentPlan()
        => new()
        {
            PlanId = "plan-1",
            GroupPurchaseId = "gp-1",
            OrdererGroupScopeKey = "orderer-group:apt-1",
            OrdererGroupScopeName = "Apartment orderer group",
            DocumentManagementNumber = "HD-GP-IMPORT-2026-0001",
            ProductName = "Imported group purchase product",
            Sku = "GP-IMPORT-SKU-1",
            ExpectedInboundQuantity = 100
        };
}
