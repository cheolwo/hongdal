namespace Hongdal.Contracts.Common.Community;

public static class CommunityVoteWorkflowClassifier
{
    public static bool IsGroupImport(CommunityVoteResponse campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);

        return campaign.Options.Any(option => !string.IsNullOrWhiteSpace(option.HsCode))
               || !string.IsNullOrWhiteSpace(campaign.GroupPurchase?.HsCode);
    }
}
