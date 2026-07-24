using MudBlazor;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public sealed class CommunityGroupPurchaseWorkspaceState
{
    public CommunityGroupPurchaseCampaignDraft Proposal { get; } = new();

    public CommunityGroupPurchaseParticipationDraft Participation { get; } = new();

    public CommunityGroupPurchaseObjectionDraft Objection { get; } = new();

    public bool CreatePanelVisible { get; set; }

    public bool ObjectionReviewConfirmed { get; set; }

    public bool SignatureConsent { get; set; }

    public string SelectedStageCode { get; set; } = CommunityGroupPurchasePresentation.StageProposal;

    public string OperatorDisplayName { get; set; } = "공동구매 운영자";

    public string ResolutionTitle { get; set; } = string.Empty;

    public string ResolutionText { get; set; } = string.Empty;

    public string ReviewMemo { get; set; } = string.Empty;

    public string SelectedSignerPartyId { get; set; } = string.Empty;

    public string SelectedSignerDisplayName { get; set; } = string.Empty;

    public string StatusMessage { get; set; } = string.Empty;

    public Severity StatusSeverity { get; set; } = Severity.Info;

    public void ResetSignatureSelection()
    {
        SelectedSignerPartyId = string.Empty;
        SelectedSignerDisplayName = string.Empty;
    }
}

public sealed class CommunityGroupPurchaseCampaignDraft
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Nickname { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string ProductKey { get; set; } = string.Empty;

    public string CommunityScope { get; set; } = "platform";

    public string ParticipationPolicyCode { get; set; } = CommunityVoteParticipationPolicyCodes.Hybrid;

    public string QuantityUnit { get; set; } = "개";

    public bool AllowConsumerPurchases { get; set; } = true;

    public bool AllowBusinessPurchases { get; set; } = true;

    public int MinimumParticipantCount { get; set; } = 3;

    public int MinimumTotalQuantity { get; set; } = 10;

    public int? RadiusMeters { get; set; } = 3000;

    public string PickupPointName { get; set; } = string.Empty;

    public string PickupPointAddress { get; set; } = string.Empty;
}

public sealed class CommunityGroupPurchaseParticipationDraft
{
    private string _transactionTypeCode = 공동구매거래유형코드.B2C;

    public string OptionId { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public string MethodCode { get; set; } = CommunityVoteParticipationMethodCodes.CommunityMember;

    public string? PickupPointId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string TransactionTypeCode
    {
        get => _transactionTypeCode;
        set
        {
            var normalized = 공동구매거래유형코드.정규화(value);
            if (string.Equals(_transactionTypeCode, normalized, StringComparison.Ordinal))
            {
                return;
            }

            _transactionTypeCode = normalized;
            if (normalized == 공동구매거래유형코드.B2B)
            {
                PriceBasisCode = 공동구매가격표시기준코드.부가세별도;
                TaxInvoiceRequired = true;
                return;
            }

            PriceBasisCode = 공동구매가격표시기준코드.부가세포함;
            PurchasingOrganizationReference = string.Empty;
            PurchasingOrganizationName = string.Empty;
            TaxInvoiceRequired = false;
        }
    }

    public bool IsBusinessPurchase => TransactionTypeCode == 공동구매거래유형코드.B2B;

    public string PriceBasisCode { get; set; } = 공동구매가격표시기준코드.부가세포함;

    public string PurchasingOrganizationReference { get; set; } = string.Empty;

    public string PurchasingOrganizationName { get; set; } = string.Empty;

    public bool TaxInvoiceRequired { get; set; }
}

public sealed class CommunityGroupPurchaseObjectionDraft
{
    public string Nickname { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;
}

public sealed record CommunityGroupPurchaseWorkflowStage(
    int Number,
    string Code,
    string Title,
    string Description);
