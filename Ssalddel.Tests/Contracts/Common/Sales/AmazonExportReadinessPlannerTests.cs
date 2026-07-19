using Ssalddel.Contracts.Common.Sales;

namespace Ssalddel.Tests.Contracts.Common.Sales;

public sealed class AmazonExportReadinessPlannerTests
{
    [Fact]
    public void Plan_CompleteAmazonExportDraft_IsReadyForListingAndFulfillment()
    {
        var result = AmazonExportReadinessPlanner.Plan(new AmazonExportReadinessDraftRequest
        {
            SalesProductId = 1001,
            SalesChannelAccountId = 4001,
            SalesSku = "KR-CHAIR-001",
            ProductName = "Korean Wood Chair",
            MarketplaceId = "ATVPDKIKX0DER",
            SellerId = "A1SELLER",
            ProductType = "CHAIR",
            AmazonSellerAccountConnected = true,
            ProductTypeDefinitionConfirmed = true,
            ListingPayloadMapped = true,
            ImageAndDescriptionReady = true,
            SourceImportParticipantConfirmed = true,
            SourceImportGroupPurchaseId = "GP-IMPORT-001",
            KoreanLogisticsHistoryRecorded = true,
            ProductJourneyEvidenceReady = true,
            UserReviewUsageConsentConfirmed = true,
            EligibleUserReviewCount = 12,
            DetailPageImageAssetGenerated = true,
            DetailPageImageAssetApproved = true,
            AdvertisingCreativeReady = true,
            HsCode = "9401.69",
            OriginCountryCode = "kr",
            HsExportReviewConfirmed = true,
            ExportRestrictionReviewed = true,
            CustomsBrokerConsultationRequested = true,
            CustomsBrokerAssigned = true,
            CustomsBrokerFeeConfirmed = true,
            ExportDeclarationPlanConfirmed = true,
            CommercialInvoiceReady = true,
            PackingListReady = true,
            InventoryReserved = true,
            OutboundBatchReady = true,
            FulfillmentModeCode = AmazonExportFulfillmentModeCode.FbaInbound,
            FulfillmentRouteConfirmed = true,
            AmazonFbaInboundEligibilityConfirmed = true,
            InternationalShippingPlanConfirmed = true,
            ReturnPolicyConfirmed = true,
            SettlementCurrencyAndFeesConfirmed = true
        });

        Assert.True(result.ReadyForAmazonListingDraft);
        Assert.True(result.ReadyForExportFulfillment);
        Assert.Empty(result.RequiredActionCodes);
        Assert.Equal(CommerceChannelKeys.Amazon, result.ChannelType);
        Assert.Equal("KR", result.CustomsPlan.OriginCountryCode);
        Assert.Equal(AmazonExportFulfillmentModeCode.FbaInbound, result.FulfillmentPlan.FulfillmentModeCode);
        Assert.True(result.MarketStoryPlan.DetailPageImageAssetApproved);
        Assert.Equal(12, result.MarketStoryPlan.EligibleUserReviewCount);
        Assert.True(result.FulfillmentPlan.FbaRouteCompatibleWithTemperature);
    }

    [Fact]
    public void Plan_MissingAmazonAndExportFields_ReturnsRequiredActions()
    {
        var result = AmazonExportReadinessPlanner.Plan(new AmazonExportReadinessDraftRequest
        {
            SalesProductId = 1001,
            SalesChannelAccountId = 0,
            SalesSku = string.Empty,
            ProductName = "Korean Wood Chair",
            MarketplaceId = string.Empty,
            SellerId = string.Empty,
            ProductType = string.Empty,
            AmazonSellerAccountConnected = false,
            ProductTypeDefinitionConfirmed = false,
            ListingPayloadMapped = false,
            ImageAndDescriptionReady = false,
            SourceImportParticipantConfirmed = false,
            SourceImportGroupPurchaseId = string.Empty,
            KoreanLogisticsHistoryRecorded = false,
            ProductJourneyEvidenceReady = false,
            UserReviewUsageConsentConfirmed = false,
            EligibleUserReviewCount = 0,
            DetailPageImageAssetGenerated = false,
            DetailPageImageAssetApproved = false,
            AdvertisingCreativeReady = false,
            HsCode = string.Empty,
            OriginCountryCode = string.Empty,
            HsExportReviewConfirmed = false,
            ExportRestrictionReviewed = false,
            CustomsBrokerConsultationRequested = false,
            CustomsBrokerAssigned = false,
            CustomsBrokerFeeConfirmed = false,
            ExportDeclarationPlanConfirmed = false,
            CommercialInvoiceReady = false,
            PackingListReady = false,
            InventoryReserved = false,
            OutboundBatchReady = false,
            FulfillmentRouteConfirmed = false,
            AmazonFbaInboundEligibilityConfirmed = false,
            InternationalShippingPlanConfirmed = false,
            ReturnPolicyConfirmed = false,
            SettlementCurrencyAndFeesConfirmed = false
        });

        Assert.False(result.ReadyForAmazonListingDraft);
        Assert.False(result.ReadyForExportFulfillment);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmAmazonSellerAccount, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmMarketplaceAndSellerId, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmProductTypeDefinition, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmListingPayloadMapping, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmImportParticipantEligibility, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmKoreanLogisticsTrace, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmReviewUsageConsent, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmAmazonDetailPageImageAsset, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmHsExportReview, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmCustomsBrokerEngagement, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmCustomsBrokerFee, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmExportDocuments, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmInventoryAndOutboundBatch, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmFulfillmentRoute, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmInternationalShippingPlan, result.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmReturnsAndSettlementPolicy, result.RequiredActionCodes);
    }

    [Fact]
    public void Plan_ListingReadyButFulfillmentMissing_SplitsReadiness()
    {
        var result = AmazonExportReadinessPlanner.Plan(new AmazonExportReadinessDraftRequest
        {
            SalesProductId = 1001,
            SalesChannelAccountId = 4001,
            SalesSku = "KR-CHAIR-001",
            ProductName = "Korean Wood Chair",
            MarketplaceId = "ATVPDKIKX0DER",
            SellerId = "A1SELLER",
            ProductType = "CHAIR",
            AmazonSellerAccountConnected = true,
            ProductTypeDefinitionConfirmed = true,
            ListingPayloadMapped = true,
            ImageAndDescriptionReady = true,
            SourceImportParticipantConfirmed = true,
            SourceImportGroupPurchaseId = "GP-IMPORT-001",
            KoreanLogisticsHistoryRecorded = true,
            ProductJourneyEvidenceReady = true,
            UserReviewUsageConsentConfirmed = true,
            EligibleUserReviewCount = 8,
            DetailPageImageAssetGenerated = true,
            DetailPageImageAssetApproved = true,
            AdvertisingCreativeReady = true,
            HsCode = "9401.69",
            HsExportReviewConfirmed = true,
            ExportRestrictionReviewed = true,
            CustomsBrokerConsultationRequested = true,
            CustomsBrokerAssigned = true,
            CustomsBrokerFeeConfirmed = true,
            ExportDeclarationPlanConfirmed = true,
            CommercialInvoiceReady = true,
            PackingListReady = true,
            InventoryReserved = true,
            OutboundBatchReady = false,
            FulfillmentRouteConfirmed = false,
            AmazonFbaInboundEligibilityConfirmed = true,
            InternationalShippingPlanConfirmed = false,
            ReturnPolicyConfirmed = true,
            SettlementCurrencyAndFeesConfirmed = true
        });

        Assert.True(result.ReadyForAmazonListingDraft);
        Assert.False(result.ReadyForExportFulfillment);
        Assert.Empty(result.ListingPlan.RequiredActionCodes);
        Assert.Empty(result.MarketStoryPlan.RequiredActionCodes);
        Assert.Empty(result.CustomsPlan.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmInventoryAndOutboundBatch, result.FulfillmentPlan.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmFulfillmentRoute, result.FulfillmentPlan.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmInternationalShippingPlan, result.FulfillmentPlan.RequiredActionCodes);
    }

    [Fact]
    public void Plan_MissingReviewsAndImageAsset_BlocksAmazonListingDraft()
    {
        var result = AmazonExportReadinessPlanner.Plan(new AmazonExportReadinessDraftRequest
        {
            SalesProductId = 1001,
            SalesChannelAccountId = 4001,
            SalesSku = "KR-CHAIR-001",
            ProductName = "Korean Wood Chair",
            MarketplaceId = "ATVPDKIKX0DER",
            SellerId = "A1SELLER",
            ProductType = "CHAIR",
            AmazonSellerAccountConnected = true,
            ProductTypeDefinitionConfirmed = true,
            ListingPayloadMapped = true,
            ImageAndDescriptionReady = true,
            SourceImportParticipantConfirmed = true,
            SourceImportGroupPurchaseId = "GP-IMPORT-001",
            KoreanLogisticsHistoryRecorded = true,
            ProductJourneyEvidenceReady = true,
            UserReviewUsageConsentConfirmed = false,
            EligibleUserReviewCount = 0,
            DetailPageImageAssetGenerated = false,
            DetailPageImageAssetApproved = false,
            AdvertisingCreativeReady = false,
            HsCode = "9401.69",
            HsExportReviewConfirmed = true,
            ExportRestrictionReviewed = true,
            CustomsBrokerConsultationRequested = true,
            CustomsBrokerAssigned = true,
            CustomsBrokerFeeConfirmed = true,
            ExportDeclarationPlanConfirmed = true,
            CommercialInvoiceReady = true,
            PackingListReady = true,
            InventoryReserved = true,
            OutboundBatchReady = true,
            FulfillmentRouteConfirmed = true,
            AmazonFbaInboundEligibilityConfirmed = false,
            InternationalShippingPlanConfirmed = true,
            ReturnPolicyConfirmed = true,
            SettlementCurrencyAndFeesConfirmed = true
        });

        Assert.False(result.ReadyForAmazonListingDraft);
        Assert.False(result.ReadyForExportFulfillment);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmReviewUsageConsent, result.MarketStoryPlan.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmAmazonDetailPageImageAsset, result.MarketStoryPlan.RequiredActionCodes);
    }

    [Fact]
    public void Plan_ChilledCargoForFba_BlocksFbaColdChainRoute()
    {
        var result = AmazonExportReadinessPlanner.Plan(new AmazonExportReadinessDraftRequest
        {
            SalesProductId = 2001,
            SalesChannelAccountId = 4001,
            SalesSku = "KR-COLD-001",
            ProductName = "Korean Chilled Product",
            MarketplaceId = "ATVPDKIKX0DER",
            SellerId = "A1SELLER",
            ProductType = "GROCERY",
            AmazonSellerAccountConnected = true,
            ProductTypeDefinitionConfirmed = true,
            ListingPayloadMapped = true,
            ImageAndDescriptionReady = true,
            SourceImportParticipantConfirmed = true,
            SourceImportGroupPurchaseId = "GP-IMPORT-COLD-001",
            KoreanLogisticsHistoryRecorded = true,
            ProductJourneyEvidenceReady = true,
            UserReviewUsageConsentConfirmed = true,
            EligibleUserReviewCount = 10,
            DetailPageImageAssetGenerated = true,
            DetailPageImageAssetApproved = true,
            AdvertisingCreativeReady = true,
            HsCode = "2106.90",
            HsExportReviewConfirmed = true,
            ExportRestrictionReviewed = true,
            CustomsBrokerConsultationRequested = true,
            CustomsBrokerAssigned = true,
            CustomsBrokerFeeConfirmed = true,
            ExportDeclarationPlanConfirmed = true,
            CommercialInvoiceReady = true,
            PackingListReady = true,
            InventoryReserved = true,
            OutboundBatchReady = true,
            FulfillmentModeCode = AmazonExportFulfillmentModeCode.FbaInbound,
            FulfillmentRouteConfirmed = true,
            InternationalShippingPlanConfirmed = true,
            ReturnPolicyConfirmed = true,
            SettlementCurrencyAndFeesConfirmed = true,
            RequiresChilledOrFrozenHandling = true,
            AmazonFbaInboundEligibilityConfirmed = false
        });

        Assert.False(result.ReadyForExportFulfillment);
        Assert.False(result.FulfillmentPlan.FbaRouteCompatibleWithTemperature);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmAmazonFbaInboundEligibility, result.FulfillmentPlan.RequiredActionCodes);
        Assert.Contains(AmazonExportRequiredActionCode.ConfirmNonFbaColdChainFulfillmentRoute, result.FulfillmentPlan.RequiredActionCodes);
    }
}
