using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.Community;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

public interface ICommunityKeywordInboxService
{
    Task<CommunityKeywordNotificationListResponse> ListAsync(
        string userId,
        string? appKey,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<int> GetUnreadCountAsync(string userId, string? appKey, CancellationToken cancellationToken);
    Task<bool> MarkReadAsync(string userId, long id, CancellationToken cancellationToken);
    Task<int> MarkAllReadAsync(string userId, string? appKey, CancellationToken cancellationToken);
}

public sealed class CommunityKeywordInboxService : ICommunityKeywordInboxService
{
    private readonly SsalddelContext _db;

    public CommunityKeywordInboxService(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<CommunityKeywordNotificationListResponse> ListAsync(
        string userId,
        string? appKey,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = CommunityKeywordSubscriptionService.NormalizeUserId(userId);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var scoped = ApplyAppScope(
            _db.CommunityKeywordNotifications.AsNoTracking().Where(x => x.UserId == normalizedUserId),
            appKey);
        var unreadCount = await scoped.CountAsync(x => !x.IsRead, cancellationToken);
        var query = unreadOnly ? scoped.Where(x => !x.IsRead) : scoped;
        var totalCount = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new CommunityKeywordNotificationListResponse(
            entities.Select(ToResponse).ToArray(),
            totalCount,
            unreadCount,
            page,
            pageSize);
    }

    public async Task<int> GetUnreadCountAsync(
        string userId,
        string? appKey,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = CommunityKeywordSubscriptionService.NormalizeUserId(userId);
        var query = ApplyAppScope(
            _db.CommunityKeywordNotifications.AsNoTracking()
                .Where(x => x.UserId == normalizedUserId && !x.IsRead),
            appKey);
        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> MarkReadAsync(
        string userId,
        long id,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = CommunityKeywordSubscriptionService.NormalizeUserId(userId);
        var notification = await _db.CommunityKeywordNotifications
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == normalizedUserId, cancellationToken);
        if (notification is null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            var now = DateTime.UtcNow;
            notification.IsRead = true;
            notification.ReadAtUtc = now;
            notification.UpdatedAtUtc = now;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<int> MarkAllReadAsync(
        string userId,
        string? appKey,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = CommunityKeywordSubscriptionService.NormalizeUserId(userId);
        var query = ApplyAppScope(
            _db.CommunityKeywordNotifications
                .Where(x => x.UserId == normalizedUserId && !x.IsRead),
            appKey);
        var now = DateTime.UtcNow;
        return await query.ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.IsRead, true)
            .SetProperty(x => x.ReadAtUtc, now)
            .SetProperty(x => x.UpdatedAtUtc, now), cancellationToken);
    }

    private static IQueryable<CommunityKeywordNotification> ApplyAppScope(
        IQueryable<CommunityKeywordNotification> query,
        string? appKey)
    {
        if (string.IsNullOrWhiteSpace(appKey))
        {
            return query;
        }

        var normalizedAppKey = CommunityKeywordSubscriptionService.NormalizeAppKey(appKey);
        return query.Where(x => x.PostAppKey == normalizedAppKey || x.PostAppKey == "platform");
    }

    private static CommunityKeywordNotificationResponse ToResponse(CommunityKeywordNotification notification)
        => new(
            notification.Id,
            notification.PostId,
            notification.PostAppKey,
            notification.PostCategory,
            notification.PostTitle,
            notification.PostExcerpt,
            notification.PostAuthorNickname,
            DeserializeKeywords(notification.MatchedKeywordsJson),
            notification.IsRead,
            notification.ReadAtUtc,
            notification.CreatedAtUtc);

    internal static IReadOnlyList<string> DeserializeKeywords(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
