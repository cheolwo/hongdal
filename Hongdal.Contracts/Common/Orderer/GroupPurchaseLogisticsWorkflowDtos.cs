namespace Hongdal.Contracts.Common.Orderer;

public static class GroupPurchaseLogisticsWorkflowPartyCode
{
    public const string Platform = "Platform";
    public const string Seller = "Seller";
    public const string OverseasSeller = "OverseasSeller";
    public const string Importer = "Importer";
    public const string CustomsBroker = "CustomsBroker";
    public const string DomesticWarehouse = "DomesticWarehouse";
    public const string DomesticLogisticsProxy = "DomesticLogisticsProxy";
    public const string SalesChannelOperator = "SalesChannelOperator";
    public const string Carrier = "Carrier";
    public const string GroupRepresentative = "GroupRepresentative";
    public const string IndividualOrderer = "IndividualOrderer";
}

public static class GroupPurchaseSellerOriginTypeCode
{
    public const string Domestic = "Domestic";
    public const string Overseas = "Overseas";
}

public static class GroupPurchaseLogisticsEvidenceCode
{
    public const string SellerPackingList = "SellerPackingList";
    public const string OverseasSellerPackingList = "OverseasSellerPackingList";
    public const string ExportInvoice = "ExportInvoice";
    public const string CustomsDeclaration = "CustomsDeclaration";
    public const string ImportInspectionResult = "ImportInspectionResult";
    public const string DomesticWarehouseReceivingReport = "DomesticWarehouseReceivingReport";
    public const string LogisticsProxyInboundReceipt = "LogisticsProxyInboundReceipt";
    public const string InventoryLotSnapshot = "InventoryLotSnapshot";
    public const string SalesChannelListingSnapshot = "SalesChannelListingSnapshot";
    public const string OutboundBatchPlanSnapshot = "OutboundBatchPlanSnapshot";
    public const string PickupPhoto = "PickupPhoto";
    public const string PickupHandoverReceipt = "PickupHandoverReceipt";
    public const string DropoffPhoto = "DropoffPhoto";
    public const string GroupRepresentativeReceipt = "GroupRepresentativeReceipt";
    public const string UnitDistributionChecklist = "UnitDistributionChecklist";
    public const string IndividualReceiptConfirmation = "IndividualReceiptConfirmation";
    public const string TemperatureLog = "TemperatureLog";
}

public sealed class GroupPurchaseLogisticsWorkflowStepDto
{
    public string StepCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Sequence { get; set; }
    public string ResponsiblePartyCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IReadOnlyList<string> RequiredEvidenceCodes { get; set; } = [];
    public IReadOnlyList<string> FailureHandlingCodes { get; set; } = [];
}

public sealed class GroupPurchaseResponsibilitySegmentDto
{
    public string SegmentCode { get; set; } = string.Empty;
    public string FromStepCode { get; set; } = string.Empty;
    public string ToStepCode { get; set; } = string.Empty;
    public string ResponsiblePartyCode { get; set; } = string.Empty;
    public string ResponsibilityScope { get; set; } = string.Empty;
    public IReadOnlyList<string> RequiredEvidenceCodes { get; set; } = [];
}

public sealed class GroupPurchaseLogisticsWorkflowDefinitionDto
{
    public string WorkflowId { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string DisplayName { get; set; } = string.Empty;
    public string ProductCategoryCode { get; set; } = string.Empty;
    public string TemperatureCode { get; set; } = string.Empty;
    public string LogisticsMode { get; set; } = string.Empty;
    public string SellerOriginType { get; set; } = GroupPurchaseSellerOriginTypeCode.Domestic;
    public string OrdererGroupScopeType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<GroupPurchaseLogisticsWorkflowStepDto> Steps { get; set; } = [];
    public IReadOnlyList<GroupPurchaseResponsibilitySegmentDto> ResponsibilitySegments { get; set; } = [];
    public IReadOnlyList<string> Tags { get; set; } = [];
    public string Memo { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class GroupPurchaseLogisticsWorkflowQuery
{
    public string? ProductCategoryCode { get; set; }
    public string? TemperatureCode { get; set; }
    public string? LogisticsMode { get; set; }
    public string? SellerOriginType { get; set; }
    public string? OrdererGroupScopeType { get; set; }
    public bool ActiveOnly { get; set; } = true;
}

public sealed class GroupPurchaseLogisticsWorkflowUpsertRequest
{
    public string? WorkflowId { get; set; }
    public string? Version { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string ProductCategoryCode { get; set; } = string.Empty;
    public string TemperatureCode { get; set; } = string.Empty;
    public string LogisticsMode { get; set; } = string.Empty;
    public string SellerOriginType { get; set; } = GroupPurchaseSellerOriginTypeCode.Domestic;
    public string OrdererGroupScopeType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public IReadOnlyList<GroupPurchaseLogisticsWorkflowStepDto> Steps { get; set; } = [];
    public IReadOnlyList<GroupPurchaseResponsibilitySegmentDto> ResponsibilitySegments { get; set; } = [];
    public IReadOnlyList<string> Tags { get; set; } = [];
    public string Memo { get; set; } = string.Empty;
}
