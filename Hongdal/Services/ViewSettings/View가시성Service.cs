using Hongdal.Contracts.Common.ViewSettings;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.설정;

namespace 홍달.Services.ViewSettings;

public sealed class View가시성Service : IView가시성Service
{
    private readonly HongdalContext _db;

    public View가시성Service(HongdalContext db)
    {
        _db = db;
    }

    public async Task SeedPoliciesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var existing = await _db.플랫폼View정책
            .ToDictionaryAsync(x => $"{x.AppKey}|{x.RoleName}|{x.ViewKey}", cancellationToken);

        var changed = false;
        foreach (var item in View카탈로그.전체())
        {
            var key = $"{item.AppKey}|{item.RoleName}|{item.ViewKey}";
            if (!existing.TryGetValue(key, out var entity))
            {
                _db.플랫폼View정책.Add(new 플랫폼View정책
                {
                    AppKey = item.AppKey,
                    ViewKey = item.ViewKey,
                    DisplayName = item.DisplayName,
                    Route = item.Route,
                    IconKey = item.IconKey,
                    RoleName = item.RoleName,
                    IsRequired = item.IsRequired,
                    PolicyEnabled = item.IsRequired || item.DefaultPolicyEnabled,
                    SortOrder = item.SortOrder,
                    CreatedAt = now,
                    UpdatedAt = now
                });
                changed = true;
                continue;
            }

            if (entity.DisplayName != item.DisplayName ||
                entity.Route != item.Route ||
                entity.IconKey != item.IconKey ||
                entity.IsRequired != item.IsRequired ||
                entity.SortOrder != item.SortOrder ||
                (item.IsRequired && !entity.PolicyEnabled))
            {
                entity.DisplayName = item.DisplayName;
                entity.Route = item.Route;
                entity.IconKey = item.IconKey;
                entity.IsRequired = item.IsRequired;
                entity.SortOrder = item.SortOrder;
                entity.PolicyEnabled = item.IsRequired || entity.PolicyEnabled;
                entity.UpdatedAt = now;
                changed = true;
            }
        }

        if (changed)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IReadOnlyList<View가시성항목응답>> GetEffectiveViewsAsync(string appKey, string roleName, string? userId, CancellationToken cancellationToken = default)
    {
        var definitions = View카탈로그.앱별(appKey, roleName);
        var viewKeys = definitions.Select(x => x.ViewKey).ToArray();
        var policies = await _db.플랫폼View정책.AsNoTracking()
            .Where(x => x.AppKey == appKey && x.RoleName == roleName && viewKeys.Contains(x.ViewKey))
            .ToDictionaryAsync(x => x.ViewKey, cancellationToken);

        Dictionary<string, 사용자View설정> userSettings = [];
        if (!string.IsNullOrWhiteSpace(userId))
        {
            userSettings = await _db.사용자View설정.AsNoTracking()
                .Where(x => x.UserId == userId && x.AppKey == appKey && viewKeys.Contains(x.ViewKey))
                .ToDictionaryAsync(x => x.ViewKey, cancellationToken);
        }

        return definitions.Select(definition =>
        {
            policies.TryGetValue(definition.ViewKey, out var policy);
            userSettings.TryGetValue(definition.ViewKey, out var userSetting);

            var policyEnabled = definition.IsRequired || policy?.PolicyEnabled != false;
            var userVisible = definition.IsRequired || userSetting?.IsVisible != false;

            return new View가시성항목응답
            {
                AppKey = definition.AppKey,
                ViewKey = definition.ViewKey,
                DisplayName = policy?.DisplayName ?? definition.DisplayName,
                Route = policy?.Route ?? definition.Route,
                IconKey = policy?.IconKey ?? definition.IconKey,
                RoleName = definition.RoleName,
                IsRequired = definition.IsRequired,
                PolicyEnabled = policyEnabled,
                UserVisible = userVisible,
                EffectiveVisible = definition.IsRequired || (policyEnabled && userVisible),
                SortOrder = policy?.SortOrder ?? definition.SortOrder
            };
        }).OrderBy(x => x.SortOrder).ToArray();
    }

    public async Task SetUserVisibilityAsync(string appKey, string roleName, string userId, string viewKey, bool isVisible, CancellationToken cancellationToken = default)
    {
        var definition = View카탈로그.찾기(appKey, roleName, viewKey)
                         ?? throw new InvalidOperationException("View 정의를 찾을 수 없습니다.");

        if (definition.IsRequired)
        {
            throw new InvalidOperationException("필수 View는 숨길 수 없습니다.");
        }

        var policy = await _db.플랫폼View정책.FirstOrDefaultAsync(
            x => x.AppKey == appKey && x.RoleName == roleName && x.ViewKey == viewKey,
            cancellationToken);

        if (policy is not null && !policy.PolicyEnabled)
        {
            throw new InvalidOperationException("관리자 정책에서 비활성화된 View입니다.");
        }

        var entity = await _db.사용자View설정.FirstOrDefaultAsync(
            x => x.UserId == userId && x.AppKey == appKey && x.ViewKey == viewKey,
            cancellationToken);

        var now = DateTime.UtcNow;
        if (entity is null)
        {
            _db.사용자View설정.Add(new 사용자View설정
            {
                UserId = userId,
                AppKey = appKey,
                ViewKey = viewKey,
                IsVisible = isVisible,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        else
        {
            entity.IsVisible = isVisible;
            entity.UpdatedAt = now;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<관리자View정책항목응답>> GetPoliciesAsync(string? appKey = null, CancellationToken cancellationToken = default)
    {
        var query = _db.플랫폼View정책.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(appKey))
        {
            query = query.Where(x => x.AppKey == appKey);
        }

        return await query
            .OrderBy(x => x.AppKey)
            .ThenBy(x => x.SortOrder)
            .Select(x => new 관리자View정책항목응답
            {
                Id = x.Id,
                AppKey = x.AppKey,
                ViewKey = x.ViewKey,
                DisplayName = x.DisplayName,
                Route = x.Route,
                IconKey = x.IconKey,
                RoleName = x.RoleName,
                IsRequired = x.IsRequired,
                PolicyEnabled = x.PolicyEnabled,
                SortOrder = x.SortOrder
            })
            .ToArrayAsync(cancellationToken);
    }

    public async Task<관리자View정책항목응답?> UpdatePolicyAsync(long id, bool policyEnabled, CancellationToken cancellationToken = default)
    {
        var entity = await _db.플랫폼View정책.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        if (entity.IsRequired && !policyEnabled)
        {
            throw new InvalidOperationException("필수 View 정책은 비활성화할 수 없습니다.");
        }

        entity.PolicyEnabled = entity.IsRequired || policyEnabled;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return new 관리자View정책항목응답
        {
            Id = entity.Id,
            AppKey = entity.AppKey,
            ViewKey = entity.ViewKey,
            DisplayName = entity.DisplayName,
            Route = entity.Route,
            IconKey = entity.IconKey,
            RoleName = entity.RoleName,
            IsRequired = entity.IsRequired,
            PolicyEnabled = entity.PolicyEnabled,
            SortOrder = entity.SortOrder
        };
    }
}
