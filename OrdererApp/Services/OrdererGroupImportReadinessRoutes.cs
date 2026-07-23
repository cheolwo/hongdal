using Ssalddel.Contracts.Common.Orderer;

namespace OrdererApp.Services;

public enum GroupImportReadinessSection
{
    Overview,
    Suppliers,
    Costs,
    Classification,
    Handoff,
    Consent
}

public static class OrdererGroupImportReadinessRoutes
{
    public const string OverviewTemplate = GroupPurchasePageRoutes.ImportOverviewTemplate;
    public const string SuppliersTemplate = GroupPurchasePageRoutes.ImportSuppliersTemplate;
    public const string CostsTemplate = GroupPurchasePageRoutes.ImportCostsTemplate;
    public const string ClassificationTemplate = GroupPurchasePageRoutes.ImportClassificationTemplate;
    public const string HandoffTemplate = GroupPurchasePageRoutes.ImportHandoffTemplate;
    public const string ConsentTemplate = GroupPurchasePageRoutes.ImportConsentTemplate;

    public static string For(
        GroupImportReadinessSection section,
        string ledgerId,
        string autoGroupId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ledgerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(autoGroupId);

        var path = section switch
        {
            GroupImportReadinessSection.Suppliers => GroupPurchasePageRoutes.ImportSuppliersFor(ledgerId),
            GroupImportReadinessSection.Costs => GroupPurchasePageRoutes.ImportCostsFor(ledgerId),
            GroupImportReadinessSection.Classification => GroupPurchasePageRoutes.ImportClassificationFor(ledgerId),
            GroupImportReadinessSection.Handoff => GroupPurchasePageRoutes.ImportHandoffFor(ledgerId),
            GroupImportReadinessSection.Consent => GroupPurchasePageRoutes.ImportConsentFor(ledgerId),
            _ => GroupPurchasePageRoutes.ImportOverviewFor(ledgerId)
        };
        return $"{path}?autoGroupId={Uri.EscapeDataString(autoGroupId.Trim())}";
    }
}
