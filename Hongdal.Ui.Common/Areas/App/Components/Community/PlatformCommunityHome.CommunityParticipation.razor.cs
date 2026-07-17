using System.Globalization;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Versioning;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Ui.Common.Areas.App.Models;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private bool IsPostParticipationPending(long postId)
        => Engagement.IsParticipationPending(postId);

    private bool IsParticipationRoleSelected(long postId, string optionId)
        => Engagement.IsParticipationRoleSelected(postId, optionId);

    private string BuildParticipationRoleClass(long postId, string optionId)
        => IsParticipationRoleSelected(postId, optionId)
            ? "platform-community-participation-role platform-community-participation-role--selected"
            : "platform-community-participation-role";

    private async Task StartPostParticipationAsync(PlatformCommunityPostResponse post)
        => await ApplyCommandResultAsync(
            await Engagement.StartParticipationAsync(post.Id));

    private async Task ToggleParticipationRoleAsync(
        PlatformCommunityPostResponse post,
        CommunityPostParticipationRoleOptionResponse role)
        => await ApplyCommandResultAsync(
            await Engagement.ToggleParticipationRoleAsync(post.Id, role));

    private async Task PromotePostParticipationAsync(
        PlatformCommunityPostResponse post,
        string intentTypeCode)
    {
        if (!postOpportunities.TryGetValue(post.Id, out var opportunity)
            || opportunity.Participation.InterestVoteId is not Guid voteId
            || !pendingPostParticipationIds.Add(post.Id))
        {
            return;
        }

        var parameters = new DialogParameters
        {
            [nameof(CommunityProvisionalLedgerDialog.InterestVoteId)] = voteId,
            [nameof(CommunityProvisionalLedgerDialog.IntentTypeCode)] = intentTypeCode
        };
        var dialog = await DialogService.ShowAsync<CommunityProvisionalLedgerDialog>(
            ProvisionalLedgerDialogTitle(intentTypeCode),
            parameters,
            new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseButton = true,
                CloseOnEscapeKey = true
            });
        var dialogResult = await dialog.Result;
        if (dialogResult is null
            || dialogResult.Canceled
            || dialogResult.Data is not PromoteCommunityPostParticipationRequest request)
        {
            pendingPostParticipationIds.Remove(post.Id);
            return;
        }

        try
        {
            await ApplyCommandResultAsync(
                await Engagement.PromoteParticipationAsync(post.Id, request));
        }
        finally
        {
            pendingPostParticipationIds.Remove(post.Id);
        }
    }

    private async Task JoinPostProfessionalRoleAsync(
        PlatformCommunityPostResponse post,
        string roleCode)
    {
        if (!postOpportunities.TryGetValue(post.Id, out var opportunity)
            || string.IsNullOrWhiteSpace(opportunity.Participation.ProvisionalLedgerId)
            || !pendingPostParticipationIds.Add(post.Id))
        {
            return;
        }

        var opening = opportunity.Participation.ProfessionalParticipation.RoleOpenings.FirstOrDefault(candidate =>
            string.Equals(candidate.RoleCode, roleCode, StringComparison.OrdinalIgnoreCase));
        var confirmed = await DialogService.ShowMessageBoxAsync(
            opening?.Label ?? "거래 참여팀 역할",
            "이 참여는 가원장 검토 단계의 자발적·비구속 참여입니다. 플랫폼 프로필 확인은 관할기관 면허·등록 확인이나 계약·수임을 대신하지 않습니다.",
            yesText: "역할로 참여",
            cancelText: "취소");
        if (confirmed is not true)
        {
            pendingPostParticipationIds.Remove(post.Id);
            return;
        }

        try
        {
            await ApplyCommandResultAsync(
                await Engagement.JoinProfessionalRoleAsync(
                    post.Id,
                    opportunity.Participation.ProvisionalLedgerId,
                    roleCode));
        }
        finally
        {
            pendingPostParticipationIds.Remove(post.Id);
        }
    }

    private async Task JoinPostPartyRoleAsync(
        PlatformCommunityPostResponse post,
        string roleCode)
    {
        if (!postOpportunities.TryGetValue(post.Id, out var opportunity)
            || string.IsNullOrWhiteSpace(opportunity.Participation.ProvisionalLedgerId)
            || !pendingPostParticipationIds.Add(post.Id))
        {
            return;
        }

        var slot = opportunity.Participation.PartyFormation.RoleSlots.FirstOrDefault(candidate =>
            string.Equals(candidate.RoleCode, roleCode, StringComparison.OrdinalIgnoreCase));
        var confirmed = await DialogService.ShowMessageBoxAsync(
            slot?.Label ?? "거래 당사자 역할",
            "이 수락은 가원장의 비구속적 검토 참여를 기록합니다. 주문·계약·결제, 수출입 책임 또는 최종 업무 배정은 별도 합의 전까지 확정되지 않습니다.",
            yesText: "역할 수락",
            cancelText: "취소");
        if (confirmed is not true)
        {
            pendingPostParticipationIds.Remove(post.Id);
            return;
        }

        try
        {
            await ApplyCommandResultAsync(
                await Engagement.JoinPartyRoleAsync(
                    post.Id,
                    opportunity.Participation.ProvisionalLedgerId,
                    roleCode));
        }
        finally
        {
            pendingPostParticipationIds.Remove(post.Id);
        }
    }

    private static string ProvisionalLedgerDialogTitle(string intentTypeCode)
        => intentTypeCode switch
        {
            CommunityCollectiveIntentTypeCodes.GroupImportCandidate => "공동수입 검토 가원장",
            CommunityCollectiveIntentTypeCodes.GroupExportCandidate => "공동수출 검토 가원장",
            _ => "공동구매 가원장"
        };

    private static string TradeDirectionLabel(string code)
        => code switch
        {
            CommunityTradeDirectionCodes.Import => "수입 검토",
            CommunityTradeDirectionCodes.Export => "수출 검토",
            _ => "국내 거래 검토"
        };

    private static string CommunityMomentumLabel(string? code)
        => code switch
        {
            CommunityPostMomentumCodes.ReadyForRealLedgerReview => "전환 검토 준비",
            CommunityPostMomentumCodes.PartyForming => "참여팀 구성중",
            _ => "역할 참여 모집"
        };

    private static string CountryCodeLabel(string? code)
        => string.IsNullOrWhiteSpace(code) ? "미정" : code.Trim().ToUpperInvariant();

    private static string TransportModeLabel(string code)
        => code switch
        {
            CommunityTransportModeCodes.Ocean => "해상",
            CommunityTransportModeCodes.Air => "항공",
            CommunityTransportModeCodes.Road => "도로",
            CommunityTransportModeCodes.Rail => "철도",
            CommunityTransportModeCodes.Multimodal => "복합운송",
            _ => code
        };

    private static string BuildPartySlotClass(CommunityPostPartyRoleSlotResponse slot)
        => slot.StateCode switch
        {
            CommunityPartyRoleSlotStateCodes.RoleAccepted =>
                "platform-community-party-slot platform-community-party-slot--confirmed",
            CommunityPartyRoleSlotStateCodes.InterestExpressed =>
                "platform-community-party-slot platform-community-party-slot--interest",
            _ => "platform-community-party-slot"
        };

    private static string PartySlotIcon(CommunityPostPartyRoleSlotResponse slot)
        => slot.StateCode switch
        {
            CommunityPartyRoleSlotStateCodes.RoleAccepted => Icons.Material.Filled.CheckCircle,
            CommunityPartyRoleSlotStateCodes.InterestExpressed => Icons.Material.Filled.Schedule,
            _ => Icons.Material.Outlined.RadioButtonUnchecked
        };

    private async Task RefreshPostOpportunitiesAsync(long postId)
        => await Engagement.RefreshOpportunityAsync(postId);

    private void HandleCommunityPostSearchChanged(string value)
    {
        communityPostSearchText = value ?? string.Empty;
        selectedForumPostId = null;
        selectedForumSeedPostTitle = null;
    }

    private static bool IsDiagramBoardPost(string? category, string? body)
    {
        return (!string.IsNullOrWhiteSpace(category) &&
                category.Contains("다이어그램", StringComparison.OrdinalIgnoreCase))
            || (!string.IsNullOrWhiteSpace(body) &&
                (body.Contains("->", StringComparison.Ordinal) ||
                 body.Contains("-->", StringComparison.Ordinal) ||
                 body.Contains("```mermaid", StringComparison.OrdinalIgnoreCase)));
    }

    private static IReadOnlyList<string> BuildCommunityDiagramPreviewNodes(string? body, string fallbackLabel)
    {
        var nodes = (body ?? string.Empty)
            .Replace("-->", "->", StringComparison.Ordinal)
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.Contains("->", StringComparison.Ordinal))
            .SelectMany(line => line.Split("->", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Select(NormalizeCommunityDiagramNodeLabel)
            .Where(label => !string.IsNullOrWhiteSpace(label))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(4)
            .ToArray();

        return nodes.Length > 1
            ? nodes
            : new[] { fallbackLabel, "사람 확인", "업무 처리", "상태 공유" };
    }

    private static string NormalizeCommunityDiagramNodeLabel(string value)
    {
        var label = value.Trim().Trim('-', '>', '`', '*', '#', ' ', ';');
        var openBracket = label.IndexOf('[');
        var closeBracket = label.LastIndexOf(']');
        if (openBracket >= 0 && closeBracket > openBracket)
        {
            label = label[(openBracket + 1)..closeBracket];
        }

        return label.Length > 24 ? label[..24] + "…" : label;
    }

    private bool IsPostCommentsExpanded(long postId) => Engagement.IsCommentsExpanded(postId);

    private void TogglePostComments(long postId)
        => Engagement.ToggleComments(postId);

    private static string BuildMobileNavigationClass(bool active)
        => active
            ? "platform-community-mobile-nav-item platform-community-mobile-nav-item--active"
            : "platform-community-mobile-nav-item";

}
