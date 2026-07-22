using MudBlazor;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.ContractManagement;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public static class CommunityGroupPurchasePresentation
{
    public const string StageProposal = "proposal";
    public const string StageRecruitment = "recruitment";
    public const string StageObjection = "objection";
    public const string StageResolution = "resolution";
    public const string StageSignature = "signature";
    public const string StageExecution = "execution";

    public static IReadOnlyList<CommunityGroupPurchaseWorkflowStage> Stages { get; } =
    [
        new(1, StageProposal, "제안 글", "상품과 운영 조건 공개"),
        new(2, StageRecruitment, "수요 모집", "지역·픽업별 참여"),
        new(3, StageObjection, "이의 검토", "단계별 의견과 조정"),
        new(4, StageResolution, "확정안", "모집 마감과 결의문"),
        new(5, StageSignature, "전자서명", "구성원 전원 동의"),
        new(6, StageExecution, "실행", "구매·물류 업무 전달")
    ];

    public static bool RequiresAuthenticatedCommand(string stageCode)
        => stageCode is StageRecruitment or StageResolution or StageSignature;

    public static string CampaignClass(Guid campaignId, Guid? selectedCampaignId)
        => campaignId == selectedCampaignId
            ? "group-purchase-card group-purchase-card--selected"
            : "group-purchase-card";

    public static string StageClass(
        CommunityGroupPurchaseWorkflowStage stage,
        CommunityVoteResponse campaign,
        string selectedStageCode)
    {
        var state = StageState(stage.Code, campaign);
        var selected = stage.Code == selectedStageCode ? " group-purchase-stage--selected" : string.Empty;
        return $"group-purchase-stage group-purchase-stage--{state}{selected}";
    }

    public static string StageState(string stageCode, CommunityVoteResponse campaign)
    {
        var resolution = campaign.ResolutionDocument;
        return stageCode switch
        {
            StageProposal => "complete",
            StageRecruitment => campaign.Status == CommunityVoteStatusCodes.Open ? "active" : "complete",
            StageObjection => campaign.Status == CommunityVoteStatusCodes.Open ? "active" : "complete",
            StageResolution => resolution is null
                ? campaign.Status == CommunityVoteStatusCodes.Open ? "waiting" : "active"
                : "complete",
            StageSignature => resolution?.Status == CommunityVoteResolutionStatusCodes.Signed
                ? "complete"
                : resolution?.Status is CommunityVoteResolutionStatusCodes.ReadyToSign
                    or CommunityVoteResolutionStatusCodes.PartiallySigned
                    ? "active"
                    : "waiting",
            StageExecution => resolution?.Status == CommunityVoteResolutionStatusCodes.Signed ? "active" : "waiting",
            _ => "waiting"
        };
    }

    public static string StageCaption(
        CommunityGroupPurchaseWorkflowStage stage,
        CommunityVoteResponse campaign)
        => StageState(stage.Code, campaign) switch
        {
            "complete" => $"완료 · {stage.Description}",
            "active" => $"진행 중 · {stage.Description}",
            _ => $"대기 · {stage.Description}"
        };

    public static string StageTitle(string stageCode)
        => Stages.FirstOrDefault(stage => stage.Code == stageCode)?.Title ?? "절차";

    public static string CampaignStateLabel(CommunityVoteResponse campaign)
    {
        if (campaign.ResolutionDocument?.Status == CommunityVoteResolutionStatusCodes.Signed)
        {
            return "서명 완료";
        }

        if (campaign.ResolutionDocument is not null)
        {
            return "확정 진행";
        }

        return campaign.Status == CommunityVoteStatusCodes.Open ? "수요 모집" : "모집 마감";
    }

    public static Color CampaignColor(CommunityVoteResponse campaign)
        => campaign.ResolutionDocument?.Status == CommunityVoteResolutionStatusCodes.Signed
            ? Color.Success
            : campaign.Status == CommunityVoteStatusCodes.Open ? Color.Primary : Color.Warning;

    public static string ProductSummary(CommunityVoteResponse campaign)
        => string.Join(", ", campaign.Options.Select(option => option.Text));

    public static string QuantitySummary(CommunityVoteResponse campaign)
        => $"{campaign.GroupPurchase?.TotalRequestedQuantity ?? 0}{campaign.GroupPurchase?.QuantityUnit}";

    public static string DestinationLabel(CommunityVoteResponse campaign)
        => campaign.GroupPurchase?.PickupPoints.FirstOrDefault()?.Name ?? campaign.CommunityScope;

    public static string SignatureProgress(CommunityVoteResponse campaign)
    {
        var plan = campaign.ResolutionDocument?.SignaturePlan;
        return plan is null ? "준비 전" : $"{plan.SignedRequiredSignerCount}/{plan.RequiredSignerCount}";
    }

    public static string ResolutionStatusLabel(CommunityVoteResponse campaign)
        => campaign.ResolutionDocument?.Status switch
        {
            CommunityVoteResolutionStatusCodes.LegalReviewRequired => "운영 검토 필요",
            CommunityVoteResolutionStatusCodes.ReadyToSign => "서명 대기",
            CommunityVoteResolutionStatusCodes.PartiallySigned => "일부 서명 완료",
            CommunityVoteResolutionStatusCodes.Signed => "전원 서명 완료",
            _ => "초안"
        };

    public static Severity ExecutionSeverity(CommunityVoteResponse campaign)
        => campaign.ResolutionDocument?.Status == CommunityVoteResolutionStatusCodes.Signed
            && campaign.GroupPurchase?.DemandHandoffFailedCount == 0
                ? Severity.Success
                : Severity.Info;

    public static string ExecutionMessage(CommunityVoteResponse campaign)
    {
        if (campaign.ResolutionDocument?.Status != CommunityVoteResolutionStatusCodes.Signed)
        {
            return "필수 구성원의 전자서명이 완료되면 공동구매 실행 단계가 열립니다.";
        }

        if (campaign.GroupPurchase?.DemandHandoffFailedCount > 0)
        {
            return "일부 수요 전달이 실패했습니다. 운영자가 전달 상태를 확인해야 합니다.";
        }

        return "서명된 수요를 바탕으로 이행 초안을 검토할 수 있습니다. 실제 구매·배차·계약은 당사자 확인 전 실행되지 않습니다.";
    }

    public static IReadOnlyList<ContractSignatureRequest> MissingSigners(CommunityVoteResponse campaign)
    {
        var plan = campaign.ResolutionDocument?.SignaturePlan;
        if (plan is null)
        {
            return [];
        }

        var missing = plan.MissingRequiredPartyIds.ToHashSet(StringComparer.Ordinal);
        return plan.Bundle.SignatureRequests.Where(request => missing.Contains(request.PartyId)).ToArray();
    }

    public static IEnumerable<PlatformCommunityPostCommentResponse> StageObjections(
        IEnumerable<PlatformCommunityPostCommentResponse> comments,
        string stageCode)
        => comments.Where(comment => comment.Body.StartsWith(
            $"[이의제기:{stageCode}]",
            StringComparison.OrdinalIgnoreCase));

    public static int StageObjectionCount(
        IEnumerable<PlatformCommunityPostCommentResponse> comments,
        string stageCode)
        => StageObjections(comments, stageCode).Count();

    public static int ObjectionCount(IEnumerable<PlatformCommunityPostCommentResponse> comments)
        => comments.Count(comment => comment.Body.StartsWith(
            "[이의제기:",
            StringComparison.OrdinalIgnoreCase));

    public static string StripObjectionPrefix(string body)
    {
        var end = body.IndexOf(']');
        return end >= 0 && end + 1 < body.Length ? body[(end + 1)..].Trim() : body;
    }

    public static string DefaultResolutionText(CommunityVoteResponse campaign)
        => string.Join(
            Environment.NewLine,
            $"'{campaign.Title}' 공동구매 수요 모집 결과를 다음과 같이 확정합니다.",
            $"참여자: {campaign.TotalVoteCount}명",
            $"총 요청 수량: {campaign.GroupPurchase?.TotalRequestedQuantity ?? 0}{campaign.GroupPurchase?.QuantityUnit}",
            $"수령 범위: {campaign.GroupPurchase?.ServiceAreaLabel}",
            "접수된 이의와 운영 조건을 검토했으며, 필수 구성원 전자서명 완료 후 구매와 물류 절차를 시작합니다.");
}
