namespace Hongdal.Contracts.Common.Community;

public sealed record CommunityKeywordSubscriptionUpsertRequest(
    string Keyword,
    string AppKey = "platform");

public sealed record CommunityKeywordSubscriptionResponse(
    long Id,
    string AppKey,
    string Keyword,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CommunityKeywordNotificationResponse(
    long Id,
    long PostId,
    string PostAppKey,
    string PostCategory,
    string PostTitle,
    string PostExcerpt,
    string PostAuthorNickname,
    IReadOnlyList<string> MatchedKeywords,
    bool IsRead,
    DateTime? ReadAtUtc,
    DateTime CreatedAtUtc);

public sealed record CommunityKeywordNotificationListResponse(
    IReadOnlyList<CommunityKeywordNotificationResponse> Items,
    int TotalCount,
    int UnreadCount,
    int Page,
    int PageSize);
