using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 살뜰.Data;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public interface ICommunityKeywordSubscriptionService
{
    Task<IReadOnlyList<CommunityKeywordSubscriptionResponse>> ListAsync(
        string userId,
        string? appKey,
        CancellationToken cancellationToken);

    Task<CommunityKeywordSubscriptionResponse> SubscribeAsync(
        string userId,
        CommunityKeywordSubscriptionUpsertRequest request,
        CancellationToken cancellationToken);

    Task<bool> UnsubscribeAsync(string userId, long id, CancellationToken cancellationToken);
}

public sealed class CommunityKeywordSubscriptionService : ICommunityKeywordSubscriptionService
{
    private readonly SsalddelContext _db;
    private readonly ICommunityKeywordMatcher _matcher;
    private readonly CommunityKeywordNotificationOptions _options;

    public CommunityKeywordSubscriptionService(
        SsalddelContext db,
        ICommunityKeywordMatcher matcher,
        IOptions<CommunityKeywordNotificationOptions> options)
    {
        _db = db;
        _matcher = matcher;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<CommunityKeywordSubscriptionResponse>> ListAsync(
        string userId,
        string? appKey,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = NormalizeUserId(userId);
        var query = _db.CommunityKeywordSubscriptions
            .AsNoTracking()
            .Where(x => x.UserId == normalizedUserId && x.IsActive);
        if (!string.IsNullOrWhiteSpace(appKey))
        {
            var normalizedAppKey = NormalizeAppKey(appKey);
            query = query.Where(x => x.AppKey == normalizedAppKey);
        }

        return await query
            .OrderBy(x => x.AppKey)
            .ThenBy(x => x.Keyword)
            .Select(x => new CommunityKeywordSubscriptionResponse(
                x.Id,
                x.AppKey,
                x.Keyword,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<CommunityKeywordSubscriptionResponse> SubscribeAsync(
        string userId,
        CommunityKeywordSubscriptionUpsertRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var normalizedUserId = NormalizeUserId(userId);
        var normalizedAppKey = NormalizeAppKey(request.AppKey);
        var normalizedKeyword = _matcher.NormalizeAndValidate(request.Keyword);
        var displayKeyword = request.Keyword.Trim();
        if (displayKeyword.Length > 40)
        {
            displayKeyword = normalizedKeyword;
        }
        var now = DateTime.UtcNow;

        var existing = await _db.CommunityKeywordSubscriptions
            .SingleOrDefaultAsync(
                x => x.UserId == normalizedUserId
                     && x.AppKey == normalizedAppKey
                     && x.NormalizedKeyword == normalizedKeyword,
                cancellationToken);
        if (existing is not null)
        {
            if (!existing.IsActive)
            {
                await EnsureWithinLimitAsync(normalizedUserId, cancellationToken);
            }

            existing.Keyword = displayKeyword;
            existing.IsActive = true;
            existing.UpdatedAtUtc = now;
            await _db.SaveChangesAsync(cancellationToken);
            return ToResponse(existing);
        }

        await EnsureWithinLimitAsync(normalizedUserId, cancellationToken);
        var subscription = new CommunityKeywordSubscription
        {
            UserId = normalizedUserId,
            AppKey = normalizedAppKey,
            Keyword = displayKeyword,
            NormalizedKeyword = normalizedKeyword,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        _db.CommunityKeywordSubscriptions.Add(subscription);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return ToResponse(subscription);
        }
        catch (DbUpdateException)
        {
            _db.Entry(subscription).State = EntityState.Detached;
            var racedSubscription = await _db.CommunityKeywordSubscriptions
                .SingleOrDefaultAsync(
                    x => x.UserId == normalizedUserId
                         && x.AppKey == normalizedAppKey
                         && x.NormalizedKeyword == normalizedKeyword,
                    cancellationToken);
            if (racedSubscription is null)
            {
                throw;
            }

            if (!racedSubscription.IsActive)
            {
                racedSubscription.Keyword = displayKeyword;
                racedSubscription.IsActive = true;
                racedSubscription.UpdatedAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);
            }

            return ToResponse(racedSubscription);
        }
    }

    public async Task<bool> UnsubscribeAsync(
        string userId,
        long id,
        CancellationToken cancellationToken)
    {
        var normalizedUserId = NormalizeUserId(userId);
        var subscription = await _db.CommunityKeywordSubscriptions
            .SingleOrDefaultAsync(x => x.Id == id && x.UserId == normalizedUserId, cancellationToken);
        if (subscription is null)
        {
            return false;
        }

        if (subscription.IsActive)
        {
            subscription.IsActive = false;
            subscription.UpdatedAtUtc = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private async Task EnsureWithinLimitAsync(string userId, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(_options.MaxSubscriptionsPerUser, 1, 500);
        var count = await _db.CommunityKeywordSubscriptions
            .CountAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
        if (count >= limit)
        {
            throw new InvalidOperationException($"사용자당 활성 키워드는 최대 {limit}개까지 구독할 수 있습니다.");
        }
    }

    private static CommunityKeywordSubscriptionResponse ToResponse(CommunityKeywordSubscription subscription)
        => new(
            subscription.Id,
            subscription.AppKey,
            subscription.Keyword,
            subscription.CreatedAtUtc,
            subscription.UpdatedAtUtc);

    internal static string NormalizeUserId(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var normalized = userId.Trim();
        if (normalized.Length > 450)
        {
            throw new ArgumentOutOfRangeException(nameof(userId), "사용자 식별자가 너무 깁니다.");
        }

        return normalized;
    }

    internal static string NormalizeAppKey(string? appKey)
    {
        var normalized = string.IsNullOrWhiteSpace(appKey) ? "platform" : appKey.Trim();
        if (normalized.Length > 80
            || normalized.Any(character =>
                !char.IsAsciiLetterOrDigit(character) && character is not '.' and not '-' and not '_'))
        {
            throw new ArgumentException("appKey 형식이 올바르지 않습니다.", nameof(appKey));
        }

        return normalized;
    }
}
