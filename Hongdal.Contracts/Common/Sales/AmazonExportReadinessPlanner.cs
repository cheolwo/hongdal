namespace Hongdal.Contracts.Common.Sales;

public static class AmazonExportFulfillmentModeCode
{
    public const string FbmInternationalShipping = "FbmInternationalShipping";
    public const string FbaInbound = "FbaInbound";
    public const string ExportForwarderHandover = "ExportForwarderHandover";
    public const string Manual = "Manual";
}

public static class AmazonExportRequiredActionCode
{
    public const string ConfirmImportParticipantEligibility = "ConfirmImportParticipantEligibility";
    public const string ConfirmAmazonSellerAccount = "ConfirmAmazonSellerAccount";
    public const string ConfirmMarketplaceAndSellerId = "ConfirmMarketplaceAndSellerId";
    public const string ConfirmProductTypeDefinition = "ConfirmProductTypeDefinition";
    public const string ConfirmListingPayloadMapping = "ConfirmListingPayloadMapping";
    public const string ConfirmKoreanLogisticsTrace = "ConfirmKoreanLogisticsTrace";
    public const string ConfirmReviewUsageConsent = "ConfirmReviewUsageConsent";
    public const string ConfirmAmazonDetailPageImageAsset = "ConfirmAmazonDetailPageImageAsset";
    public const string ConfirmHsExportReview = "ConfirmHsExportReview";
    public const string ConfirmCustomsBrokerEngagement = "ConfirmCustomsBrokerEngagement";
    public const string ConfirmCustomsBrokerFee = "ConfirmCustomsBrokerFee";
    public const string ConfirmExportDocuments = "ConfirmExportDocuments";
    public const string ConfirmInventoryAndOutboundBatch = "ConfirmInventoryAndOutboundBatch";
    public const string ConfirmFulfillmentRoute = "ConfirmFulfillmentRoute";
    public const string ConfirmAmazonFbaInboundEligibility = "ConfirmAmazonFbaInboundEligibility";
    public const string ConfirmNonFbaColdChainFulfillmentRoute = "ConfirmNonFbaColdChainFulfillmentRoute";
    public const string ConfirmInternationalShippingPlan = "ConfirmInternationalShippingPlan";
    public const string ConfirmReturnsAndSettlementPolicy = "ConfirmReturnsAndSettlementPolicy";
}

public sealed class AmazonExportReadinessDraftRequest
{
    public long SalesProductId { get; set; }
    public long SalesChannelAccountId { get; set; }
    public string SalesSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string MarketplaceId { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public bool AmazonSellerAccountConnected { get; set; }
    public bool ProductTypeDefinitionConfirmed { get; set; }
    public bool ListingPayloadMapped { get; set; }
    public bool ImageAndDescriptionReady { get; set; }
    public bool ImportParticipantEligibilityRequired { get; set; } = true;
    public bool SourceImportParticipantConfirmed { get; set; }
    public string SourceImportGroupPurchaseId { get; set; } = string.Empty;
    public bool KoreanLogisticsHistoryRecorded { get; set; }
    public bool ProductJourneyEvidenceReady { get; set; }
    public bool UserReviewUsageConsentConfirmed { get; set; }
    public int EligibleUserReviewCount { get; set; }
    public bool DetailPageImageAssetGenerated { get; set; }
    public bool DetailPageImageAssetApproved { get; set; }
    public bool AdvertisingCreativeReady { get; set; }
    public string HsCode { get; set; } = string.Empty;
    public string OriginCountryCode { get; set; } = "KR";
    public bool HsExportReviewConfirmed { get; set; }
    public bool ExportRestrictionReviewed { get; set; }
    public bool CustomsBrokerConsultationRequested { get; set; }
    public bool CustomsBrokerAssigned { get; set; }
    public bool CustomsBrokerFeeConfirmed { get; set; }
    public bool ExportDeclarationPlanConfirmed { get; set; }
    public bool CommercialInvoiceReady { get; set; }
    public bool PackingListReady { get; set; }
    public bool InventoryReserved { get; set; }
    public bool OutboundBatchReady { get; set; }
    public string FulfillmentModeCode { get; set; } = AmazonExportFulfillmentModeCode.FbmInternationalShipping;
    public bool FulfillmentRouteConfirmed { get; set; }
    public bool InternationalShippingPlanConfirmed { get; set; }
    public bool ReturnPolicyConfirmed { get; set; }
    public bool SettlementCurrencyAndFeesConfirmed { get; set; }
    public bool RequiresChilledOrFrozenHandling { get; set; }
    public bool AmazonFbaInboundEligibilityConfirmed { get; set; }
}

public sealed class AmazonExportReadinessResult
{
    public string ChannelType { get; set; } = CommerceChannelKeys.Amazon;
    public bool ReadyForAmazonListingDraft { get; set; }
    public bool ReadyForExportFulfillment { get; set; }
    public AmazonExportListingPlanDto ListingPlan { get; set; } = new();
    public AmazonExportMarketStoryPlanDto MarketStoryPlan { get; set; } = new();
    public AmazonExportCustomsPlanDto CustomsPlan { get; set; } = new();
    public AmazonExportFulfillmentPlanDto FulfillmentPlan { get; set; } = new();
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
    public string Memo { get; set; } = string.Empty;
}

public sealed class AmazonExportListingPlanDto
{
    public long SalesProductId { get; set; }
    public long SalesChannelAccountId { get; set; }
    public string SalesSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string MarketplaceId { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public bool AmazonSellerAccountConnected { get; set; }
    public bool ProductTypeDefinitionConfirmed { get; set; }
    public bool ListingPayloadMapped { get; set; }
    public bool ImageAndDescriptionReady { get; set; }
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
}

public sealed class AmazonExportMarketStoryPlanDto
{
    public bool ImportParticipantEligibilityRequired { get; set; } = true;
    public bool SourceImportParticipantConfirmed { get; set; }
    public string SourceImportGroupPurchaseId { get; set; } = string.Empty;
    public bool KoreanLogisticsHistoryRecorded { get; set; }
    public bool ProductJourneyEvidenceReady { get; set; }
    public bool UserReviewUsageConsentConfirmed { get; set; }
    public int EligibleUserReviewCount { get; set; }
    public bool DetailPageImageAssetGenerated { get; set; }
    public bool DetailPageImageAssetApproved { get; set; }
    public bool AdvertisingCreativeReady { get; set; }
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
}

public sealed class AmazonExportCustomsPlanDto
{
    public string HsCode { get; set; } = string.Empty;
    public string OriginCountryCode { get; set; } = "KR";
    public bool HsExportReviewConfirmed { get; set; }
    public bool ExportRestrictionReviewed { get; set; }
    public bool CustomsBrokerConsultationRequested { get; set; }
    public bool CustomsBrokerAssigned { get; set; }
    public bool CustomsBrokerFeeConfirmed { get; set; }
    public bool ExportDeclarationPlanConfirmed { get; set; }
    public bool CommercialInvoiceReady { get; set; }
    public bool PackingListReady { get; set; }
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
}

public sealed class AmazonExportFulfillmentPlanDto
{
    public string FulfillmentModeCode { get; set; } = AmazonExportFulfillmentModeCode.FbmInternationalShipping;
    public bool InventoryReserved { get; set; }
    public bool OutboundBatchReady { get; set; }
    public bool FulfillmentRouteConfirmed { get; set; }
    public bool RequiresChilledOrFrozenHandling { get; set; }
    public bool AmazonFbaInboundEligibilityConfirmed { get; set; }
    public bool FbaRouteCompatibleWithTemperature { get; set; } = true;
    public bool InternationalShippingPlanConfirmed { get; set; }
    public bool ReturnPolicyConfirmed { get; set; }
    public bool SettlementCurrencyAndFeesConfirmed { get; set; }
    public IReadOnlyList<string> RequiredActionCodes { get; set; } = [];
}

public static class AmazonExportReadinessPlanner
{
    public static AmazonExportReadinessResult Plan(AmazonExportReadinessDraftRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var listingActions = ResolveListingRequiredActions(request).ToArray();
        var marketStoryActions = ResolveMarketStoryRequiredActions(request).ToArray();
        var customsActions = ResolveCustomsRequiredActions(request).ToArray();
        var fulfillmentActions = ResolveFulfillmentRequiredActions(request).ToArray();
        var allActions = listingActions
            .Concat(marketStoryActions)
            .Concat(customsActions)
            .Concat(fulfillmentActions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var readyForListing = listingActions.Length == 0 && marketStoryActions.Length == 0;
        var readyForFulfillment = readyForListing && customsActions.Length == 0 && fulfillmentActions.Length == 0;
        var fulfillmentMode = NormalizeFulfillmentMode(request.FulfillmentModeCode);

        return new AmazonExportReadinessResult
        {
            ReadyForAmazonListingDraft = readyForListing,
            ReadyForExportFulfillment = readyForFulfillment,
            ListingPlan = new AmazonExportListingPlanDto
            {
                SalesProductId = request.SalesProductId,
                SalesChannelAccountId = request.SalesChannelAccountId,
                SalesSku = request.SalesSku.Trim(),
                ProductName = request.ProductName.Trim(),
                MarketplaceId = request.MarketplaceId.Trim(),
                SellerId = request.SellerId.Trim(),
                ProductType = request.ProductType.Trim(),
                AmazonSellerAccountConnected = request.AmazonSellerAccountConnected,
                ProductTypeDefinitionConfirmed = request.ProductTypeDefinitionConfirmed,
                ListingPayloadMapped = request.ListingPayloadMapped,
                ImageAndDescriptionReady = request.ImageAndDescriptionReady,
                RequiredActionCodes = listingActions
            },
            MarketStoryPlan = new AmazonExportMarketStoryPlanDto
            {
                ImportParticipantEligibilityRequired = request.ImportParticipantEligibilityRequired,
                SourceImportParticipantConfirmed = request.SourceImportParticipantConfirmed,
                SourceImportGroupPurchaseId = request.SourceImportGroupPurchaseId.Trim(),
                KoreanLogisticsHistoryRecorded = request.KoreanLogisticsHistoryRecorded,
                ProductJourneyEvidenceReady = request.ProductJourneyEvidenceReady,
                UserReviewUsageConsentConfirmed = request.UserReviewUsageConsentConfirmed,
                EligibleUserReviewCount = Math.Max(0, request.EligibleUserReviewCount),
                DetailPageImageAssetGenerated = request.DetailPageImageAssetGenerated,
                DetailPageImageAssetApproved = request.DetailPageImageAssetApproved,
                AdvertisingCreativeReady = request.AdvertisingCreativeReady,
                RequiredActionCodes = marketStoryActions
            },
            CustomsPlan = new AmazonExportCustomsPlanDto
            {
                HsCode = request.HsCode.Trim(),
                OriginCountryCode = NormalizeCountryCode(request.OriginCountryCode),
                HsExportReviewConfirmed = request.HsExportReviewConfirmed,
                ExportRestrictionReviewed = request.ExportRestrictionReviewed,
                CustomsBrokerConsultationRequested = request.CustomsBrokerConsultationRequested,
                CustomsBrokerAssigned = request.CustomsBrokerAssigned,
                CustomsBrokerFeeConfirmed = request.CustomsBrokerFeeConfirmed,
                ExportDeclarationPlanConfirmed = request.ExportDeclarationPlanConfirmed,
                CommercialInvoiceReady = request.CommercialInvoiceReady,
                PackingListReady = request.PackingListReady,
                RequiredActionCodes = customsActions
            },
            FulfillmentPlan = new AmazonExportFulfillmentPlanDto
            {
                FulfillmentModeCode = fulfillmentMode,
                InventoryReserved = request.InventoryReserved,
                OutboundBatchReady = request.OutboundBatchReady,
                FulfillmentRouteConfirmed = request.FulfillmentRouteConfirmed,
                RequiresChilledOrFrozenHandling = request.RequiresChilledOrFrozenHandling,
                AmazonFbaInboundEligibilityConfirmed = request.AmazonFbaInboundEligibilityConfirmed,
                FbaRouteCompatibleWithTemperature = IsFbaRouteCompatibleWithTemperature(request),
                InternationalShippingPlanConfirmed = request.InternationalShippingPlanConfirmed,
                ReturnPolicyConfirmed = request.ReturnPolicyConfirmed,
                SettlementCurrencyAndFeesConfirmed = request.SettlementCurrencyAndFeesConfirmed,
                RequiredActionCodes = fulfillmentActions
            },
            RequiredActionCodes = allActions,
            Memo = BuildMemo(request, readyForListing, readyForFulfillment)
        };
    }

    private static IEnumerable<string> ResolveListingRequiredActions(AmazonExportReadinessDraftRequest request)
    {
        if (!request.AmazonSellerAccountConnected || request.SalesChannelAccountId <= 0)
        {
            yield return AmazonExportRequiredActionCode.ConfirmAmazonSellerAccount;
        }

        if (string.IsNullOrWhiteSpace(request.MarketplaceId) || string.IsNullOrWhiteSpace(request.SellerId))
        {
            yield return AmazonExportRequiredActionCode.ConfirmMarketplaceAndSellerId;
        }

        if (string.IsNullOrWhiteSpace(request.ProductType) || !request.ProductTypeDefinitionConfirmed)
        {
            yield return AmazonExportRequiredActionCode.ConfirmProductTypeDefinition;
        }

        if (string.IsNullOrWhiteSpace(request.SalesSku) ||
            string.IsNullOrWhiteSpace(request.ProductName) ||
            !request.ListingPayloadMapped ||
            !request.ImageAndDescriptionReady)
        {
            yield return AmazonExportRequiredActionCode.ConfirmListingPayloadMapping;
        }
    }

    private static IEnumerable<string> ResolveMarketStoryRequiredActions(AmazonExportReadinessDraftRequest request)
    {
        if (request.ImportParticipantEligibilityRequired &&
            (!request.SourceImportParticipantConfirmed || string.IsNullOrWhiteSpace(request.SourceImportGroupPurchaseId)))
        {
            yield return AmazonExportRequiredActionCode.ConfirmImportParticipantEligibility;
        }

        if (!request.KoreanLogisticsHistoryRecorded || !request.ProductJourneyEvidenceReady)
        {
            yield return AmazonExportRequiredActionCode.ConfirmKoreanLogisticsTrace;
        }

        if (!request.UserReviewUsageConsentConfirmed || request.EligibleUserReviewCount <= 0)
        {
            yield return AmazonExportRequiredActionCode.ConfirmReviewUsageConsent;
        }

        if (!request.DetailPageImageAssetGenerated ||
            !request.DetailPageImageAssetApproved ||
            !request.AdvertisingCreativeReady)
        {
            yield return AmazonExportRequiredActionCode.ConfirmAmazonDetailPageImageAsset;
        }
    }

    private static IEnumerable<string> ResolveCustomsRequiredActions(AmazonExportReadinessDraftRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HsCode) ||
            !request.HsExportReviewConfirmed ||
            !request.ExportRestrictionReviewed)
        {
            yield return AmazonExportRequiredActionCode.ConfirmHsExportReview;
        }

        if (!request.CustomsBrokerConsultationRequested ||
            !request.CustomsBrokerAssigned ||
            !request.ExportDeclarationPlanConfirmed)
        {
            yield return AmazonExportRequiredActionCode.ConfirmCustomsBrokerEngagement;
        }

        if (!request.CustomsBrokerFeeConfirmed)
        {
            yield return AmazonExportRequiredActionCode.ConfirmCustomsBrokerFee;
        }

        if (string.IsNullOrWhiteSpace(request.OriginCountryCode) ||
            !request.CommercialInvoiceReady ||
            !request.PackingListReady)
        {
            yield return AmazonExportRequiredActionCode.ConfirmExportDocuments;
        }
    }

    private static IEnumerable<string> ResolveFulfillmentRequiredActions(AmazonExportReadinessDraftRequest request)
    {
        if (!request.InventoryReserved || !request.OutboundBatchReady)
        {
            yield return AmazonExportRequiredActionCode.ConfirmInventoryAndOutboundBatch;
        }

        if (!request.FulfillmentRouteConfirmed)
        {
            yield return AmazonExportRequiredActionCode.ConfirmFulfillmentRoute;
        }

        var fulfillmentMode = NormalizeFulfillmentMode(request.FulfillmentModeCode);
        if (fulfillmentMode == AmazonExportFulfillmentModeCode.FbaInbound &&
            !request.AmazonFbaInboundEligibilityConfirmed)
        {
            yield return AmazonExportRequiredActionCode.ConfirmAmazonFbaInboundEligibility;
        }

        if (fulfillmentMode == AmazonExportFulfillmentModeCode.FbaInbound &&
            request.RequiresChilledOrFrozenHandling)
        {
            yield return AmazonExportRequiredActionCode.ConfirmNonFbaColdChainFulfillmentRoute;
        }

        if (!request.InternationalShippingPlanConfirmed)
        {
            yield return AmazonExportRequiredActionCode.ConfirmInternationalShippingPlan;
        }

        if (!request.ReturnPolicyConfirmed || !request.SettlementCurrencyAndFeesConfirmed)
        {
            yield return AmazonExportRequiredActionCode.ConfirmReturnsAndSettlementPolicy;
        }
    }

    private static string NormalizeFulfillmentMode(string? value)
    {
        if (string.Equals(value, AmazonExportFulfillmentModeCode.FbaInbound, StringComparison.OrdinalIgnoreCase))
        {
            return AmazonExportFulfillmentModeCode.FbaInbound;
        }

        if (string.Equals(value, AmazonExportFulfillmentModeCode.ExportForwarderHandover, StringComparison.OrdinalIgnoreCase))
        {
            return AmazonExportFulfillmentModeCode.ExportForwarderHandover;
        }

        if (string.Equals(value, AmazonExportFulfillmentModeCode.Manual, StringComparison.OrdinalIgnoreCase))
        {
            return AmazonExportFulfillmentModeCode.Manual;
        }

        return AmazonExportFulfillmentModeCode.FbmInternationalShipping;
    }

    private static string NormalizeCountryCode(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? "KR"
            : value.Trim().ToUpperInvariant();

    private static bool IsFbaRouteCompatibleWithTemperature(AmazonExportReadinessDraftRequest request)
        => NormalizeFulfillmentMode(request.FulfillmentModeCode) != AmazonExportFulfillmentModeCode.FbaInbound ||
           !request.RequiresChilledOrFrozenHandling;

    private static string BuildMemo(
        AmazonExportReadinessDraftRequest request,
        bool readyForListing,
        bool readyForFulfillment)
    {
        var mode = NormalizeFulfillmentMode(request.FulfillmentModeCode);
        if (readyForFulfillment)
        {
            return $"Amazon export flow is ready for listing and export fulfillment. Fulfillment mode: {mode}.";
        }

        if (readyForListing)
        {
            return $"Amazon listing draft is ready, but export fulfillment gates remain. Fulfillment mode: {mode}.";
        }

        return $"Amazon export listing is not ready yet. Confirm seller account, marketplace, product type, and listing payload before export fulfillment. Fulfillment mode: {mode}.";
    }
}
