using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 공동구매플랫폼국내운송계획기Tests
{
    [Fact]
    public void Plan_InformalOrdererGroup_UsesPlatformAsShipperAndOrdererGroupAsCostOwner()
    {
        var fulfillment = CreateFulfillment계획();

        var result = 공동구매플랫폼국내운송계획기.계획(
            fulfillment,
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                PlatformLegalEntityName = "Ssalddel Platform",
                TransportMode = 공동구매국내운송방식코드.LCL,
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ThreePlWarehouse,
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
        Assert.Equal(공동구매국내운송의뢰주체유형코드.플랫폼, result.PrincipalType);
        Assert.Equal(공동구매국내운송비용부담주체유형코드.주문자집단, result.CostOwnerType);
        Assert.Equal(
            공동구매국내운송정산정책코드.PlatformCollectsOrdererPaymentsAndPaysDriverAfterDropoff,
            result.SettlementPolicyCode);
        Assert.Equal(공동구매국내운송원천의뢰유형코드.LclCargoTransport, result.SourceRequestType);
        Assert.Equal(20, result.DispatchBusinessTypeCode);
        Assert.Equal("platform-ops", result.CargoTransportDraft.PlatformShipperUserId);
        Assert.Equal(fulfillment.주문자집단배송권키, result.CargoTransportDraft.주문자집단배송권키);
        Assert.Equal("PlatformCollectedSettlement", result.CargoTransportDraft.PaymentMethodCode);
        Assert.Contains("Cost owner: orderer group", result.CargoTransportDraft.SettlementMemo);
        Assert.Contains("pays the driver", result.CargoTransportDraft.SettlementMemo);
        Assert.True(result.DriverPayoutPlan.PlatformCollectsOrdererPayments);
        Assert.True(result.DriverPayoutPlan.PlatformHoldsFundsUntilDropoff);
        Assert.True(result.DriverPayoutPlan.DriverSettlementAccountConfirmed);
        Assert.False(result.DriverPayoutPlan.RequireCashReceipt);
        Assert.Equal(5, result.DriverPayoutPlan.DriverPayoutDelayDays);
        Assert.Equal(new DateTime(2026, 7, 12, 3, 0, 0, DateTimeKind.Utc), result.DriverPayoutPlan.DriverPayoutDueAtUtc);
        Assert.Contains(공동구매국내운송주문자결제수단코드.Card, result.DriverPayoutPlan.OrdererPaymentMethodCodes);
        Assert.Contains(공동구매국내운송주문자결제수단코드.CashLike, result.DriverPayoutPlan.OrdererPaymentMethodCodes);
        Assert.Contains(공동구매국내운송증빙코드.DropoffCompletion, result.DriverPayoutPlan.RequiredEvidenceCodes);
    }

    [Fact]
    public void Plan_CustomsReleaseNotReady_BlocksDispatchQueueDraft()
    {
        var result = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ThreePlWarehouse,
                DriverPerformsApartmentUnitDistribution = false,
                PickupRoadAddress = "Pyeongtaek bonded warehouse",
                DropoffRoadAddress = "Hwaseong 3PL center",
                CargoWeightKg = 800m,
                CustomsReleaseReady = false,
                RequireAdminConfirmation = false
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmCustomsReleaseOrBondedRelease,
            result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_PaymentCollectionOrDriverAccountMissing_BlocksDispatchQueueDraft()
    {
        var result = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ThreePlWarehouse,
                DriverPerformsApartmentUnitDistribution = false,
                PickupRoadAddress = "Pyeongtaek bonded warehouse",
                DropoffRoadAddress = "Hwaseong 3PL center",
                CargoWeightKg = 800m,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmOrdererPaymentCollection,
            result.RequiredActionCodes);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmDriverSettlementAccount,
            result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_DefaultDestination_UsesDriverHomeDeliveryAfterGroupDecision()
    {
        var result = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                ApartmentComplexName = "Ssalddel Apartment",
                ApartmentUnitDeliveryCount = 36,
                ApartmentUnitDistributionPlanConfirmed = true,
                UnitSortationBeforePickupConfirmed = true,
                UnitSortationResponsiblePartyCode = 공동구매세대별분류책임주체코드.OverseasForwarder,
                UnitDemandBreakdownConfirmed = true,
                ImportedProductInfoRegistered = true,
                ProductInfoRegisteredByPartyCode = 공동구매세대별분류책임주체코드.OverseasForwarder,
                ProductInfoStorageLocationCode = 공동구매상품정보보관위치코드.OverseasForwarderSystem,
                ProductInfoStorageConfirmed = true,
                단위포장라벨링방식코드 = 공동구매단위포장라벨링방식코드.상품정보스티커,
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
                DropoffRoadAddress = "Seoul Ssalddel Apartment 101",
                CargoWeightKg = 500m,
                EstimatedApartmentDirectTransportFareKrw = 170000,
                EstimatedDriverUnitDistributionFeeKrw = 90000
            });

        Assert.True(result.ReadyForDispatchQueue);
        Assert.Equal(
            공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
            result.DestinationPlan.DestinationTypeCode);
        Assert.True(result.DestinationPlan.DriverPerformsApartmentUnitDistribution);
        Assert.Equal(공동구매공동주택세대배송방식코드.DriverToUnitDoor, result.DestinationPlan.ApartmentUnitDistributionModeCode);
        Assert.Equal(공동구매공동주택분배책임코드.Driver, result.DestinationPlan.DistributionResponsibilityCode);
        Assert.True(result.DestinationPlan.UnitSortationPlan.UnitSortationBeforePickupConfirmed);
        Assert.Equal(공동구매세대별분류책임주체코드.OverseasForwarder, result.DestinationPlan.UnitSortationPlan.UnitSortationResponsiblePartyCode);
        Assert.True(result.DestinationPlan.UnitSortationPlan.ImportedProductInfoRegistered);
        Assert.Equal(공동구매상품정보보관위치코드.OverseasForwarderSystem, result.DestinationPlan.UnitSortationPlan.ProductInfoStorageLocationCode);
        Assert.Equal(공동구매단위포장라벨링방식코드.상품정보스티커, result.DestinationPlan.UnitSortationPlan.단위포장라벨링방식코드);
        Assert.True(result.DestinationPlan.UnitSortationPlan.UnitProductInfoStickerConfirmed);
        Assert.False(result.DestinationPlan.UnitSortationPlan.UnitInvoiceIssuedConfirmed);
        Assert.False(result.DestinationPlan.UnitSortationPlan.UnitBarcodeScanLookupEnabled);
        Assert.Contains("UnitProductInfoStickers", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.DoesNotContain("UnitInvoiceLabels", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.DoesNotContain("UnitBarcodeScanLookup", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.True(result.DestinationPlan.TransportDecisionLocked);
        Assert.Contains(result.DestinationCostOptions, option =>
            option.DestinationTypeCode == 공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution &&
            option.DistributionResponsibilityCode == 공동구매공동주택분배책임코드.Driver &&
            option.EstimatedTotalCostKrw == 260000);
    }

    [Fact]
    public void Plan_LockedTransportDecisionRevision_BlocksDispatchQueueDraft()
    {
        var result = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
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
                DropoffRoadAddress = "Seoul Ssalddel Apartment 101",
                CargoWeightKg = 500m
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.True(result.DestinationPlan.TransportDecisionLocked);
        Assert.True(result.DestinationPlan.TransportDecisionRevisionRequested);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmTransportDecisionRevision,
            result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_ApartmentDirectDistribution_AllowsDriverToDeliverToUnits()
    {
        var result = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                TransportMode = 공동구매국내운송방식코드.GeneralCargo,
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
                ApartmentComplexCode = "APT-001",
                ApartmentComplexName = "Ssalddel Apartment",
                DriverPerformsApartmentUnitDistribution = true,
                ApartmentUnitDistributionModeCode = 공동구매공동주택세대배송방식코드.DriverToUnitDoor,
                ApartmentUnitDeliveryCount = 48,
                ApartmentUnitDistributionPlanConfirmed = true,
                UnitSortationBeforePickupConfirmed = true,
                UnitSortationResponsiblePartyCode = 공동구매세대별분류책임주체코드.OverseasSeller,
                UnitDemandBreakdownConfirmed = true,
                ImportedProductInfoRegistered = true,
                ProductInfoRegisteredByPartyCode = 공동구매세대별분류책임주체코드.OverseasSeller,
                ProductInfoStorageLocationCode = 공동구매상품정보보관위치코드.OverseasSellerSystem,
                ProductInfoStorageConfirmed = true,
                단위포장라벨링방식코드 = 공동구매단위포장라벨링방식코드.세대별송장라벨,
                UnitInvoiceIssuedConfirmed = true,
                UnitPackageLabelsConfirmed = true,
                UnitBarcodeScanLookupEnabled = true,
                UnitBarcodeSchemeCode = 공동구매세대단위바코드체계코드.OrderNumberBarcode,
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
                DropoffRoadAddress = "Seoul Ssalddel Apartment 101",
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
            공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
            result.DestinationPlan.DestinationTypeCode);
        Assert.Equal("Ssalddel Apartment", result.DestinationPlan.DestinationName);
        Assert.True(result.DestinationPlan.DirectApartmentDistribution);
        Assert.True(result.DestinationPlan.DriverPerformsApartmentUnitDistribution);
        Assert.Equal(공동구매공동주택세대배송방식코드.DriverToUnitDoor, result.DestinationPlan.ApartmentUnitDistributionModeCode);
        Assert.Equal(48, result.DestinationPlan.ApartmentUnitDeliveryCount);
        Assert.True(result.DestinationPlan.RecipientAddressPrivacyConfirmed);
        Assert.Contains("ApartmentUnitDistributionChecklist", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.Contains("UnitInvoiceLabels", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.Contains("UnitBarcodeScanLookup", result.DestinationPlan.RequiredDistributionEvidenceCodes);
        Assert.True(result.DestinationPlan.UnitSortationPlan.UnitSortationBeforePickupConfirmed);
        Assert.Equal(공동구매세대별분류책임주체코드.OverseasSeller, result.DestinationPlan.UnitSortationPlan.UnitSortationResponsiblePartyCode);
        Assert.Equal(공동구매단위포장라벨링방식코드.세대별송장라벨, result.DestinationPlan.UnitSortationPlan.단위포장라벨링방식코드);
        Assert.True(result.DestinationPlan.UnitSortationPlan.UnitBarcodeLookupDataConfirmed);
        Assert.Contains("bonded-area-to-apartment-direct-distribution", result.CargoTransportDraft.SettlementMemo);
        Assert.Equal(
            공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
            result.CargoTransportDraft.DestinationTypeCode);
        Assert.Equal(
            공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
            result.DispatchQueueDraft.DestinationTypeCode);
        Assert.True(result.DispatchQueueDraft.DriverPerformsApartmentUnitDistribution);
        Assert.Equal(공동구매공동주택세대배송방식코드.DriverToUnitDoor, result.DispatchQueueDraft.ApartmentUnitDistributionModeCode);
        Assert.Equal(48, result.DispatchQueueDraft.ApartmentUnitDeliveryCount);
        Assert.Equal(공동구매공동주택분배책임코드.Driver, result.DispatchQueueDraft.DistributionResponsibilityCode);
        Assert.True(result.DispatchQueueDraft.UnitSortationBeforePickupConfirmed);
        Assert.Equal(공동구매단위포장라벨링방식코드.세대별송장라벨, result.DispatchQueueDraft.단위포장라벨링방식코드);
        Assert.True(result.DispatchQueueDraft.UnitInvoiceIssuedConfirmed);
        Assert.True(result.DispatchQueueDraft.UnitPackageLabelsConfirmed);
        Assert.True(result.DispatchQueueDraft.UnitBarcodeScanLookupEnabled);
        Assert.Equal(공동구매세대단위바코드체계코드.OrderNumberBarcode, result.DispatchQueueDraft.UnitBarcodeSchemeCode);
        Assert.True(result.DispatchQueueDraft.UnitBarcodeMapsToMaskedRecipientConfirmed);
        Assert.Contains(result.DestinationCostOptions, option =>
            option.DestinationTypeCode == 공동구매국내운송도착지유형코드.ThreePlWarehouse &&
            option.EstimatedTotalCostKrw == 320000);
        Assert.Contains(result.DestinationCostOptions, option =>
            option.DestinationTypeCode == 공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution &&
            option.DistributionResponsibilityCode == 공동구매공동주택분배책임코드.Driver &&
            option.EstimatedTotalCostKrw == 300000);
    }

    [Fact]
    public void Plan_ApartmentDirectDistributionMissingUnitPlan_BlocksDispatchQueueDraft()
    {
        var result = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
                DriverPerformsApartmentUnitDistribution = true,
                CustomsReleaseReady = true,
                RequireAdminConfirmation = false,
                OrdererPaymentCollectionConfirmed = true,
                DriverSettlementAccountConfirmed = true,
                PickupRoadAddress = "Incheon bonded warehouse",
                DropoffRoadAddress = "Seoul Ssalddel Apartment 101",
                CargoWeightKg = 600m
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmApartmentUnitDistributionPlan,
            result.RequiredActionCodes);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmRecipientAddressPrivacy,
            result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_DriverUnitDeliveryWithoutOverseasLabels_BlocksDispatchQueueDraft()
    {
        var result = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ApartmentComplexDirectDistribution,
                DriverPerformsApartmentUnitDistribution = true,
                ApartmentUnitDistributionModeCode = 공동구매공동주택세대배송방식코드.DriverToUnitDoor,
                ApartmentUnitDeliveryCount = 24,
                ApartmentUnitDistributionPlanConfirmed = true,
                UnitSortationResponsiblePartyCode = 공동구매세대별분류책임주체코드.OverseasForwarder,
                UnitDemandBreakdownConfirmed = true,
                단위포장라벨링방식코드 = 공동구매단위포장라벨링방식코드.세대별송장라벨,
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
                DropoffRoadAddress = "Seoul Ssalddel Apartment 101",
                CargoWeightKg = 400m
            });

        Assert.False(result.ReadyForDispatchQueue);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmUnitSortationBeforePickup,
            result.RequiredActionCodes);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmOverseasUnitInvoiceAndLabeling,
            result.RequiredActionCodes);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmUnitBarcodeScanLookup,
            result.RequiredActionCodes);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmOverseasUnitInvoiceAndLabeling,
            result.DestinationPlan.UnitSortationPlan.RequiredActionCodes);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmUnitBarcodeScanLookup,
            result.DestinationPlan.UnitSortationPlan.RequiredActionCodes);
        Assert.Equal(공동구매세대별분류책임주체코드.OverseasForwarder, result.DestinationPlan.UnitSortationPlan.UnitSortationResponsiblePartyCode);
    }

    [Fact]
    public void Plan_ColdChainThreePlWithoutColdFacility_BlocksDispatchAndShowsCostConfirmation()
    {
        var result = 공동구매플랫폼국내운송계획기.계획(
            CreateFulfillment계획(),
            new 공동구매플랫폼국내운송초안요청
            {
                PlatformShipperUserId = "platform-ops",
                DestinationTypeCode = 공동구매국내운송도착지유형코드.ThreePlWarehouse,
                TemperatureCondition = 공동구매온도코드.냉동,
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
        Assert.Equal(공동구매온도코드.냉동, result.ColdChainPlan.TemperatureCode);
        Assert.False(result.ColdChainPlan.SelectedDestinationColdChainCompatible);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmColdChainThreePlFacility,
            result.RequiredActionCodes);
        Assert.Contains(
            공동구매국내운송필요조치코드.ConfirmColdChainThreePlFacility,
            result.ColdChainPlan.RequiredActionCodes);
        Assert.Contains(result.DestinationCostOptions, option =>
            option.DestinationTypeCode == 공동구매국내운송도착지유형코드.ThreePlWarehouse &&
            option.StatusCode == 공동구매국내운송비용옵션상태코드.NeedsConfirmation &&
            option.EstimatedTotalCostKrw == 350000);
    }

    private static 공동구매커머스이행계획Dto CreateFulfillment계획()
        => new()
        {
            계획Id = "plan-1",
            공동구매Id = "gp-1",
            주문자집단배송권키 = "orderer-group:apt-1",
            주문자집단배송권명 = "Apartment orderer group",
            문서관리번호 = "HD-GP-IMPORT-2026-0001",
            상품명 = "Imported group purchase product",
            Sku = "GP-IMPORT-SKU-1",
            예상입고수량 = 100
        };
}
