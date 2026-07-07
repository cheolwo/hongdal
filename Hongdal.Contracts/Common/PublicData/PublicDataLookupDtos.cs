namespace Hongdal.Contracts.Common.PublicData;

public sealed class PublicDataLookupResponse<TItem>
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int? TotalCount { get; init; }

    public IReadOnlyList<TItem> Items { get; init; } = [];
}

public sealed class RoadAddressSearchRequest
{
    public string Keyword { get; init; } = string.Empty;

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public sealed class RoadAddressItem
{
    public string RoadAddress { get; init; } = string.Empty;

    public string JibunAddress { get; init; } = string.Empty;

    public string ZipCode { get; init; } = string.Empty;

    public string AdministrativeCode { get; init; } = string.Empty;

    public string RoadNameManagementNo { get; init; } = string.Empty;

    public string BuildingManagementNo { get; init; } = string.Empty;

    public string? RelatedJibun { get; init; }

    public string? EnglishAddress { get; init; }
}

public sealed class ApartmentComplexSearchRequest
{
    public string? SidoCode { get; init; }

    public string? SigunguCode { get; init; }

    public string? EupmyeondongCode { get; init; }

    public string? RoadName { get; init; }

    public string? Keyword { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;
}

public sealed class ApartmentComplexItem
{
    public string ComplexCode { get; init; } = string.Empty;

    public string ComplexName { get; init; } = string.Empty;

    public string? Sido { get; init; }

    public string? Sigungu { get; init; }

    public string? Eupmyeondong { get; init; }

    public string? RoadAddress { get; init; }

    public string? LegalDongAddress { get; init; }
}

public sealed class ApartmentComplexBasicRequest
{
    public string ComplexCode { get; init; } = string.Empty;
}

public sealed class ApartmentComplexBasicItem
{
    public string ComplexCode { get; init; } = string.Empty;

    public string ComplexName { get; init; } = string.Empty;

    public int? HouseholdCount { get; init; }

    public int? BuildingCount { get; init; }

    public string? ManagementType { get; init; }

    public string? HeatingType { get; init; }

    public string? ApprovalDate { get; init; }

    public string? RoadAddress { get; init; }

    public string? LegalDongAddress { get; init; }
}

public sealed class ApartmentManagementFeeSnapshotRequest
{
    public string ComplexCode { get; init; } = string.Empty;

    public string Month { get; init; } = string.Empty;
}

public sealed class ApartmentManagementFeeSnapshotItem
{
    public string ComplexCode { get; init; } = string.Empty;

    public string Month { get; init; } = string.Empty;

    public int? HouseholdCount { get; init; }

    public decimal PublicManagementFeeAmount { get; init; }

    public decimal IndividualUsageFeeAmount { get; init; }

    public decimal LongTermRepairReserveMonthlyAmount { get; init; }

    public decimal EstimatedTotalMonthlyFeeAmount { get; init; }

    public decimal? EstimatedFeePerHousehold { get; init; }

    public IReadOnlyList<ApartmentManagementFeeLineItem> LineItems { get; init; } = [];

    public string DataSource { get; init; } = "K-apt public data";
}

public sealed class ApartmentManagementFeeLineItem
{
    public string Category { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public decimal Amount { get; init; }
}

public sealed class ApartmentGroupCommerceOffsetSimulationRequest
{
    public string ComplexCode { get; init; } = string.Empty;

    public string Month { get; init; } = string.Empty;

    public int ParticipantHouseholdCount { get; init; }

    public decimal ExpectedSalesAmount { get; init; }

    public decimal ExpectedPurchaseCost { get; init; }

    public decimal ExpectedLogisticsCost { get; init; }

    public decimal ExpectedPlatformFee { get; init; }

    public decimal ExpectedOtherCost { get; init; }

    public decimal ProfitSharingRate { get; init; } = 1m;
}

public sealed class ApartmentGroupCommerceOffsetSimulationResult
{
    public ApartmentManagementFeeSnapshotItem FeeSnapshot { get; init; } = new();

    public int ParticipantHouseholdCount { get; init; }

    public decimal ExpectedSalesAmount { get; init; }

    public decimal ExpectedTotalCost { get; init; }

    public decimal ExpectedGrossProfit { get; init; }

    public decimal ProfitSharingRate { get; init; }

    public decimal ExpectedSharedProfit { get; init; }

    public decimal? ExpectedMonthlyOffsetPerParticipant { get; init; }

    public decimal? EstimatedManagementFeeOffsetRate { get; init; }

    public string Summary { get; init; } = string.Empty;
}

public sealed class HsCountryMonthlyTradeUnitPriceRequest
{
    public string HsCode { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;

    public string Month { get; init; } = string.Empty;

    public int LookbackMonths { get; init; } = 1;

    public decimal? ExpectedFxRateKrwPerUsd { get; init; }

    public decimal? ExpectedPurchaseUnitPriceKrwPerKg { get; init; }

    public decimal? ExpectedDomesticLogisticsCostKrwPerKg { get; init; }

    public decimal? ExpectedSellingUnitPriceKrwPerKg { get; init; }

    public decimal? ParticipantQuantityKg { get; init; }
}

public sealed class HsCountryMonthlyTradeUnitPriceItem
{
    public string HsCode { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;

    public string Month { get; init; } = string.Empty;

    public decimal ImportWeightKg { get; init; }

    public decimal ImportValueUsd { get; init; }

    public decimal? AverageImportUnitValueUsdPerKg { get; init; }

    public decimal? AverageImportUnitValueKrwPerKg { get; init; }

    public string DataSource { get; init; } = "Korea Customs Service import/export statistics";
}

public sealed class HsCountryImportUnitPriceSimulationResult
{
    public bool Success { get; init; }

    public string? ErrorMessage { get; init; }

    public string HsCode { get; init; } = string.Empty;

    public string CountryCode { get; init; } = string.Empty;

    public string StartMonth { get; init; } = string.Empty;

    public string EndMonth { get; init; } = string.Empty;

    public IReadOnlyList<HsCountryMonthlyTradeUnitPriceItem> MonthlyItems { get; init; } = [];

    public decimal TotalImportWeightKg { get; init; }

    public decimal TotalImportValueUsd { get; init; }

    public decimal? AverageImportUnitValueUsdPerKg { get; init; }

    public decimal? AverageImportUnitValueKrwPerKg { get; init; }

    public decimal? ExpectedLandedCostKrwPerKg { get; init; }

    public decimal? ExpectedGrossMarginKrwPerKg { get; init; }

    public decimal? ExpectedGrossMarginRate { get; init; }

    public decimal? ExpectedParticipantGrossMarginKrw { get; init; }

    public string PriceSignalCode { get; init; } = "Unknown";

    public string Summary { get; init; } = string.Empty;
}

public sealed class 주문자집단배송권조회요청
{
    public string? RoadAddress { get; init; }

    public string? JibunAddress { get; init; }

    public string? KakaoRegionLevel1 { get; init; }

    public string? KakaoRegionLevel2 { get; init; }

    public string? KakaoRegionLevel3 { get; init; }

    public int PageSize { get; init; } = 5;
}

public sealed class 주문자집단배송권후보항목
{
    public string ScopeKey { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Basis { get; init; } = string.Empty;

    public string RoadAddressLevel1 { get; init; } = string.Empty;

    public string RoadAddressLevel2 { get; init; } = string.Empty;

    public string? RoadAddressLevel3 { get; init; }

    public string AddressHint { get; init; } = string.Empty;

    public bool IsDefaultScope { get; init; }

    public bool SupportsApartmentSubScope { get; init; }

    public string PrivacyNote { get; init; } = string.Empty;
}
