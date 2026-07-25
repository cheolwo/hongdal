using System.Globalization;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Localization;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Community;

public static class PlatformCommunityPostListPresentation
{
    public static string UiText(string displayLanguageCode, string korean, string english)
        => DisplayLanguageCodes.Select(displayLanguageCode, korean, english);

    public static string DisplayFilter(string displayLanguageCode, string filter)
        => IsKoreanDisplay(displayLanguageCode)
            ? filter
            : filter switch
            {
                "전체글" => "All",
                "추천글" => "Recommended",
                "공지" => "Notices",
                _ => filter
            };

    public static string DisplayBoardName(string displayLanguageCode, string boardName)
        => IsKoreanDisplay(displayLanguageCode)
            ? boardName
            : boardName switch
            {
                "전체" => "All boards",
                "공지·안내" => "Notices & Guides",
                "서원·발원" => "Vows & Intentions",
                "자유·생활" => "Life & Community",
                "질문·도움" => "Questions & Help",
                "정보·시세" => "Information & Prices",
                "참여·모집" => "Participation",
                "판매·공급" => "Sales & Supply",
                "원장·진행" => "Ledgers & Progress",
                "완료·후기" => "Results & Reviews",
                "반야" => "Prajna",
                "음식·맛집" => "Food & Places",
                "화물" => "Cargo",
                "시스템 다이어그램" => "System Diagrams",
                "운송 실무" => "Transport Operations",
                "업무 질문" => "Work Questions",
                "업무 기록" => "Work Records",
                "생활 원장" => "Community Ledgers",
                "개선 제안" => "Improvement Proposals",
                "신고/분쟁" => "Reports & Disputes",
                _ => boardName
            };

    public static string DisplayPostNumber(
        string displayLanguageCode,
        PlatformCommunityPostResponse post)
    {
        if (post.IsOperatorPinned)
        {
            return UiText(displayLanguageCode, "공지", "Notice");
        }

        return post.IsSystemGenerated
            ? CommunitySystemPostDisplay.ShortLabel(post.SystemPostKind)
            : post.Id.ToString(CultureInfo.InvariantCulture);
    }

    public static bool IsPostSelected(long? selectedPostId, PlatformCommunityPostResponse post)
        => selectedPostId == post.Id;

    public static bool IsSeedPostSelected(string? selectedTitle, CommunitySeedPost post)
        => string.Equals(selectedTitle, post.Title, StringComparison.Ordinal);

    public static string BuildFilterClass(string selectedFilter, string filter)
        => string.Equals(selectedFilter, filter, StringComparison.OrdinalIgnoreCase)
            ? "platform-community-forum-view-tab platform-community-forum-view-tab--selected"
            : "platform-community-forum-view-tab";

    public static string BuildViewModeClass(
        CommunityPostViewMode selectedViewMode,
        CommunityPostViewMode viewMode)
        => selectedViewMode == viewMode
            ? "platform-community-forum-display-button platform-community-forum-display-button--selected"
            : "platform-community-forum-display-button";

    public static string BuildPostRowClass(
        PlatformCommunityPostResponse post,
        long? selectedPostId)
    {
        var selected = IsPostSelected(selectedPostId, post) ? " platform-community-forum-row--selected" : string.Empty;
        var notice = post.IsOperatorPinned ? " platform-community-forum-row--notice" : string.Empty;
        var completion = post.IsSystemGenerated ? " platform-community-forum-row--completion" : string.Empty;
        var momentum = post.IsCommunityMomentumPromoted ? " platform-community-forum-row--momentum" : string.Empty;
        return $"platform-community-forum-row{selected}{momentum}{notice}{completion}";
    }

    public static string BuildSeedPostRowClass(CommunitySeedPost post, string? selectedTitle)
    {
        var selected = IsSeedPostSelected(selectedTitle, post) ? " platform-community-forum-row--selected" : string.Empty;
        return $"platform-community-forum-row platform-community-forum-row--recommended{selected}";
    }

    public static string BuildPostCardClass(
        PlatformCommunityPostResponse post,
        long? selectedPostId)
    {
        var selected = IsPostSelected(selectedPostId, post) ? " platform-community-forum-card--selected" : string.Empty;
        var notice = post.IsOperatorPinned ? " platform-community-forum-card--notice" : string.Empty;
        var completion = post.IsSystemGenerated ? " platform-community-forum-card--completion" : string.Empty;
        var momentum = post.IsCommunityMomentumPromoted ? " platform-community-forum-card--momentum" : string.Empty;
        return $"platform-community-forum-card{selected}{momentum}{notice}{completion}";
    }

    public static string BuildSeedPostCardClass(CommunitySeedPost post, string? selectedTitle)
    {
        var selected = IsSeedPostSelected(selectedTitle, post) ? " platform-community-forum-card--selected" : string.Empty;
        return $"platform-community-forum-card platform-community-forum-card--recommended{selected}";
    }

    public static string FormatForumDate(DateTime createdAtUtc, DateTime? nowLocal = null)
    {
        var local = createdAtUtc.ToLocalTime();
        var now = nowLocal ?? DateTime.Now;
        if (local.Date == now.Date)
        {
            return local.ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        return local.Year == now.Year
            ? local.ToString("MM.dd", CultureInfo.InvariantCulture)
            : local.ToString("yy.MM.dd", CultureInfo.InvariantCulture);
    }

    public static string FormatSalesPrice(PlatformCommunityPostSalesOfferResponse offer)
        => string.Equals(offer.CurrencyCode, "KRW", StringComparison.OrdinalIgnoreCase)
            ? $"{offer.UnitPrice:N0}원"
            : $"{offer.CurrencyCode.ToUpperInvariant()} {offer.UnitPrice:N2}";

    public static string FormatSalesQuantity(decimal quantity)
        => quantity == decimal.Truncate(quantity)
            ? quantity.ToString("N0", CultureInfo.CurrentCulture)
            : quantity.ToString("N2", CultureInfo.CurrentCulture).TrimEnd('0').TrimEnd('.');

    public static string SalesStatusLabel(string status)
        => status switch
        {
            PlatformCommunitySalesOfferStatuses.SoldOut => "품절",
            PlatformCommunitySalesOfferStatuses.Closed => "판매 종료",
            _ => "판매 중"
        };

    public static string DisplayPostNickname(PlatformCommunityPostResponse post)
        => IsReportPost(post) ? "익명 신고자" : post.Nickname;

    public static bool HasPublicAuthorCountry(PlatformCommunityPostResponse post)
        => post.IsAuthorDisplayCountryPublic
           && !string.IsNullOrWhiteSpace(post.AuthorDisplayCountryCode)
           && !string.IsNullOrWhiteSpace(post.AuthorDisplayCountryName)
           && !IsReportPost(post);

    public static string FormatPostCountryInline(PlatformCommunityPostResponse post)
        => HasPublicAuthorCountry(post)
            ? $" · 활동 국가 {post.AuthorDisplayCountryCode} {post.AuthorDisplayCountryName}"
            : string.Empty;

    public static bool IsReportPost(PlatformCommunityPostResponse post)
        => post.IsReportBoardPost
           || ContainsReportKeyword(post.Category);

    public static bool IsFoodVideoPost(PlatformCommunityPostResponse post)
        => post.Title.StartsWith("[음식 발견]", StringComparison.OrdinalIgnoreCase)
           && IsYouTubeUrl(post.SharedLinkUrl);

    public static string CommunityMomentumLabel(string? code)
        => code switch
        {
            CommunityPostMomentumCodes.ReadyForRealLedgerReview => "전환 검토 준비",
            CommunityPostMomentumCodes.PartyForming => "참여팀 구성중",
            _ => "역할 참여 모집"
        };

    private static bool IsKoreanDisplay(string displayLanguageCode)
        => DisplayLanguageCodes.Normalize(displayLanguageCode) == DisplayLanguageCodes.Korean;

    private static bool IsYouTubeUrl(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var host = uri.Host.TrimStart('.');
        return host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase)
               || host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase)
               || host.EndsWith(".youtube.com", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ContainsReportKeyword(string? category)
        => !string.IsNullOrWhiteSpace(category)
           && (category.Contains("신고", StringComparison.OrdinalIgnoreCase)
               || category.Contains("분쟁", StringComparison.OrdinalIgnoreCase)
               || category.Contains("report", StringComparison.OrdinalIgnoreCase));
}
