using MudBlazor;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public sealed record PlatformCommunityComposerPostingAccess(
    string Code,
    string Label,
    string Notice,
    Severity Severity,
    bool UsesAutomaticAnonymousNickname,
    string AutomaticAnonymousNicknameDisplay);

public sealed record PlatformCommunityComposerStatus(string? Message, Severity Severity);

public static class PlatformCommunityComposerPresentation
{
    public static string BuildComposeCardClass(bool isOpen)
        => isOpen
            ? "pa-4 platform-community-compose-card platform-community-compose-card--open"
            : "pa-4 platform-community-compose-card";

    public static string ResolvePublishActionText(
        bool isSaving,
        long? editingPostId,
        bool isScheduledPublication)
        => isSaving
            ? "저장 중"
            : editingPostId is not null
                ? "수정"
                : isScheduledPublication
                    ? "예약"
                    : "등록";

    public static PlatformCommunityComposerStatus ResolveStatus(
        string? viewModelMessage,
        CommunityComposerMessageKind viewModelKind,
        string? externalMessage,
        Severity externalSeverity)
        => new(
            viewModelMessage ?? externalMessage,
            viewModelMessage is null
                ? externalSeverity
                : viewModelKind switch
                {
                    CommunityComposerMessageKind.Success => Severity.Success,
                    CommunityComposerMessageKind.Warning => Severity.Warning,
                    CommunityComposerMessageKind.Error => Severity.Error,
                    _ => Severity.Info
                });

    public static PlatformCommunityComposerPostingAccess ResolvePostingAccess(
        string category,
        bool isAuthenticated,
        long? editingPostId,
        string? draftNickname)
    {
        var definition = CommunityBoardCatalog.Find(category);
        var code = definition?.PostingAccessCode
                   ?? CommunityBoardPostingAccessCodes.Authenticated;
        var usesAutomaticNickname = code == CommunityBoardPostingAccessCodes.Anonymous
                                    && !isAuthenticated;
        var automaticNicknameDisplay = editingPostId is not null
                                       && !string.IsNullOrWhiteSpace(draftNickname)
            ? draftNickname
            : CommunityAnonymousNicknameCatalog.Preview(category);
        var notice = code switch
        {
            CommunityBoardPostingAccessCodes.Anonymous =>
                usesAutomaticNickname
                    ? $"비로그인 작성자는 {CommunityAnonymousNicknameCatalog.Preview(category)} 형태의 이름을 자동으로 받습니다."
                    : "로그인한 작성자는 계정의 공개 닉네임을 사용합니다.",
            CommunityBoardPostingAccessCodes.OperatorOnly =>
                "일반 사용자가 직접 작성할 수 없는 운영 게시판입니다.",
            _ => "계정 확인이 필요하지만 공개 화면에는 실명 대신 닉네임이 표시됩니다."
        };
        var severity = code switch
        {
            CommunityBoardPostingAccessCodes.Anonymous => Severity.Success,
            CommunityBoardPostingAccessCodes.OperatorOnly => Severity.Warning,
            _ => Severity.Info
        };

        return new PlatformCommunityComposerPostingAccess(
            code,
            CommunityBoardPostingAccessCodes.DisplayName(code),
            notice,
            severity,
            usesAutomaticNickname,
            automaticNicknameDisplay);
    }

    public static string BuildCategoryClass(string selectedCategory, string category)
        => string.Equals(selectedCategory, category, StringComparison.OrdinalIgnoreCase)
            ? "platform-community-compose-board-tab platform-community-compose-board-tab--active"
            : "platform-community-compose-board-tab";

    public static bool IsCategoryLocked(
        CommunityPostComposerViewModel model,
        string category)
        => model.Draft.IsSalesPost
           && !string.Equals(
               category,
               PlatformCommunityPostCategories.Sales,
               StringComparison.OrdinalIgnoreCase);
}
