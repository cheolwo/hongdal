namespace Hongdal.Contracts.Common.Orderer;

public static class ImportLogisticsReferenceCodeType
{
    public const string Port = "Port";
    public const string Airport = "Airport";
    public const string CustomsOffice = "CustomsOffice";
    public const string BondedArea = "BondedArea";
    public const string Unknown = "Unknown";
}

public static class ImportLogisticsSimulationRiskCode
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string NeedsReview = "NeedsReview";
}

public sealed class ImportLogisticsReferenceLookupRequest
{
    public string? Keyword { get; set; }
    public string? TransportMode { get; set; }
    public string? CodeType { get; set; }
    public int PageSize { get; set; } = 20;
}

public sealed class ImportLogisticsReferenceItem
{
    public string Code { get; set; } = string.Empty;
    public string CodeType { get; set; } = ImportLogisticsReferenceCodeType.Unknown;
    public string Name { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public string TransportMode { get; set; } = string.Empty;
    public string RelatedPortOrAirportCode { get; set; } = string.Empty;
    public string RelatedCustomsOfficeCode { get; set; } = string.Empty;
    public string SourceName { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public bool RequiresOfficialVerification { get; set; }
}

public sealed class ImportLogisticsNormalizationSimulationRequest
{
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public string TransportDocumentType { get; set; } = GroupPurchaseShipmentDocumentTypeCode.BillOfLading;
    public string TransportDocumentNumber { get; set; } = string.Empty;
    public string TransportMode { get; set; } = GroupPurchaseShipmentTransportModeCode.Ocean;
    public string OriginCountryCode { get; set; } = string.Empty;
    public string OriginPortCode { get; set; } = string.Empty;
    public string DestinationPortCode { get; set; } = string.Empty;
    public string DestinationPortOrAirportName { get; set; } = string.Empty;
    public string CustomsOfficeCode { get; set; } = string.Empty;
    public string CustomsOfficeName { get; set; } = string.Empty;
    public string BondedAreaCode { get; set; } = string.Empty;
    public string BondedAreaName { get; set; } = string.Empty;
    public string CurrentLocationSummary { get; set; } = string.Empty;
    public string CustomsStageName { get; set; } = string.Empty;
    public string HsCode { get; set; } = string.Empty;
    public decimal? CargoInvoiceUsd { get; set; }
    public decimal? CargoWeightKg { get; set; }
    public decimal? ExpectedDomesticInboundCostKrw { get; set; }
    public int? ExpectedBondedStorageDays { get; set; }
}

public sealed class ImportLogisticsNormalizationSimulationResult
{
    public bool Success { get; set; }
    public string DocumentManagementNumber { get; set; } = string.Empty;
    public IReadOnlyList<ImportLogisticsReferenceItem> NormalizedReferences { get; set; } = [];
    public IReadOnlyList<ImportLogisticsFlowStepDto> SuggestedFlow { get; set; } = [];
    public ImportLogisticsCostAndRiskSimulationDto Simulation { get; set; } = new();
    public IReadOnlyList<string> Warnings { get; set; } = [];
    public IReadOnlyList<ImportLogisticsSourceDto> Sources { get; set; } = [];
}

public sealed class ImportLogisticsFlowStepDto
{
    public int Sequence { get; set; }
    public string StepCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ResponsiblePartyCode { get; set; } = string.Empty;
    public string ReferenceCode { get; set; } = string.Empty;
    public string ReferenceName { get; set; } = string.Empty;
    public bool IsConfirmedByOfficialCode { get; set; }
}

public sealed class ImportLogisticsCostAndRiskSimulationDto
{
    public decimal? InvoiceUnitValueUsdPerKg { get; set; }
    public decimal? ExpectedDomesticInboundCostKrwPerKg { get; set; }
    public string ClearanceRouteRiskCode { get; set; } = ImportLogisticsSimulationRiskCode.NeedsReview;
    public string ConfidenceCode { get; set; } = ImportLogisticsSimulationRiskCode.NeedsReview;
    public string Summary { get; set; } = string.Empty;
}

public sealed class ImportLogisticsSourceDto
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Usage { get; set; } = string.Empty;
}
