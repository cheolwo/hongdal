using MudBlazor;
using Ssalddel.Contracts.Common.Community;

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

    public int MinimumParticipantCount { get; set; } = 3;

    public int MinimumTotalQuantity { get; set; } = 10;

    public int? RadiusMeters { get; set; } = 3000;

    public string PickupPointName { get; set; } = string.Empty;

    public string PickupPointAddress { get; set; } = string.Empty;
}

public sealed class CommunityGroupPurchaseParticipationDraft
{
    public string OptionId { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public string MethodCode { get; set; } = CommunityVoteParticipationMethodCodes.CommunityMember;

    public string? PickupPointId { get; set; }

    public string DisplayName { get; set; } = string.Empty;
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
