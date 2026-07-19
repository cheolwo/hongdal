using Ssalddel.Contracts.Common.Community;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;
using 살뜰.도메인.설정;

namespace Ssalddel.Services.Community;

public interface ICommunityActivitySignalService
{
    Task<CommunityActivitySignalListResponse> GetSignalsAsync(
        CommunityActivitySignalQuery query,
        CancellationToken cancellationToken);
}

public sealed class CommunityActivitySignalService : ICommunityActivitySignalService
{
    private readonly SsalddelContext _db;

    public CommunityActivitySignalService(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<CommunityActivitySignalListResponse> GetSignalsAsync(
        CommunityActivitySignalQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 50);
        var toUtc = request.ToUtc ?? DateTime.UtcNow;
        var fromUtc = request.FromUtc ?? toUtc.AddDays(-7);

        var query = _db.사용자행위로그
            .AsNoTracking()
            .Where(x => x.IsSuccess)
            .Where(x => x.OccurredAtUtc >= fromUtc && x.OccurredAtUtc <= toUtc)
            .Where(x => !x.Route.StartsWith("/api/v1/admin"))
            .Where(x => !x.Route.StartsWith("/api/v1/auth"))
            .Where(x => !x.Route.StartsWith("/api/v1/community"))
            .Where(x => !x.Route.StartsWith("/api/v1/security"));

        if (!request.IncludeRead)
        {
            query = query.Where(x => x.ActionType != "Read");
        }

        if (!string.IsNullOrWhiteSpace(request.AppKey))
        {
            var appKey = request.AppKey.Trim();
            query = query.Where(x => x.AppKey == appKey);
        }

        var candidates = await query
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(Math.Max(page * pageSize * 4, pageSize))
            .ToArrayAsync(cancellationToken);

        var projected = candidates
            .Select(CommunityActivitySignalProjector.TryProject)
            .OfType<CommunityActivitySignalResponse>();

        if (!string.IsNullOrWhiteSpace(request.CommunityScope))
        {
            var communityScope = request.CommunityScope.Trim();
            projected = projected.Where(x => string.Equals(x.CommunityScope, communityScope, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tag = request.Tag.Trim();
            projected = projected.Where(x => x.TopicTags.Any(y => string.Equals(y, tag, StringComparison.OrdinalIgnoreCase)));
        }

        var items = projected.ToArray();

        return new CommunityActivitySignalListResponse
        {
            Items = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToArray(),
            Page = page,
            PageSize = pageSize,
            TotalCount = items.Length
        };
    }
}

public static class CommunityActivitySignalProjector
{
    public static CommunityActivitySignalResponse? TryProject(사용자행위로그 source)
    {
        if (!source.IsSuccess || IsPrivateRoute(source.Route))
        {
            return null;
        }

        var route = Normalize(source.Route);
        var actionType = Normalize(source.ActionType);
        var appKey = Normalize(source.AppKey);
        var roleLabel = ResolveRoleLabel(source.RoleName);
        var timeBucket = ResolveTimeBucket(source.OccurredAtUtc);

        if (route.StartsWith("/api/v1/driver/transports", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSignal(source, CommunityActivityScopes.DriverWork, "DriverTransport", "기사 운송 진행 신호", $"{roleLabel} 사용자가 운송 진행 단계를 갱신했습니다.", ["driver", "transport", "work"]);
        }

        if (route.StartsWith("/api/v1/driver/dispatch-actions", StringComparison.OrdinalIgnoreCase)
            || route.StartsWith("/api/v1/driver/recommendations", StringComparison.OrdinalIgnoreCase)
            || route.StartsWith("/api/v1/driver/requests", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSignal(source, CommunityActivityScopes.DriverWork, "DriverDispatch", "기사 배차 활동 신호", $"{roleLabel} 사용자가 배차 후보나 운송 의뢰 흐름을 확인했습니다.", ["driver", "dispatch", "recommendation"]);
        }

        if (route.StartsWith("/api/v1/shipper/requests", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSignal(source, CommunityActivityScopes.ShipperTransport, "ShipperRequest", "화주 운송 의뢰 신호", $"{roleLabel} 사용자가 운송 의뢰 흐름을 진행했습니다.", ["shipper", "request", "transport"]);
        }

        if (route.StartsWith("/api/v1/warehouse-operations", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSignal(source, CommunityActivityScopes.WarehouseWork, "WarehouseOperation", "창고 작업 흐름 신호", $"{roleLabel} 사용자가 입고, 재고, 출고 관련 작업을 진행했습니다.", ["warehouse", "inbound", "inventory"]);
        }

        if (route.StartsWith("/api/v1/products", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSignal(source, CommunityActivityScopes.ProductJourney, "ProductJourney", "상품 여정 확인 신호", $"{roleLabel} 사용자가 상품의 이동과 이력 흐름을 확인했습니다.", ["product", "journey", "review"]);
        }

        if (route.StartsWith("/api/v1/sales-channels", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSignal(source, CommunityActivityScopes.SalesCommerce, "SalesCommerce", "판매 채널 작업 신호", $"{roleLabel} 사용자가 판매상품이나 출품 흐름을 진행했습니다.", ["sales", "commerce", "listing"]);
        }

        if (route.StartsWith("/api/v1/gratitude", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSignal(source, CommunityActivityScopes.CommunityTrust, "Gratitude", "감사와 후기 신호", $"{roleLabel} 사용자가 업무 경험을 감사나 후기로 남겼습니다.", ["community", "gratitude", "review"]);
        }

        if (route.StartsWith("/api/v1/connections", StringComparison.OrdinalIgnoreCase))
        {
            return CreateSignal(source, CommunityActivityScopes.CommunityTrust, "Connection", "인연 연결 신호", $"{roleLabel} 사용자가 업무 인연 연결 흐름을 진행했습니다.", ["community", "connection", "trust"]);
        }

        return null;

        CommunityActivitySignalResponse CreateSignal(
            사용자행위로그 activity,
            string scope,
            string activityKind,
            string title,
            string summary,
            IReadOnlyList<string> tags)
        {
            return new CommunityActivitySignalResponse
            {
                SignalId = $"activity-{activity.Id}",
                AppKey = appKey,
                CommunityScope = scope,
                ActivityKind = activityKind,
                Title = title,
                Summary = $"{summary} {timeBucket}의 비슷한 흐름을 참고할 수 있습니다.",
                ActorRoleLabel = roleLabel,
                TopicTags = [.. tags, actionType.ToLowerInvariant()],
                TimeBucketLabel = timeBucket,
                OccurredAtUtc = activity.OccurredAtUtc
            };
        }
    }

    private static bool IsPrivateRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return true;
        }

        return route.StartsWith("/api/v1/admin", StringComparison.OrdinalIgnoreCase)
            || route.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase)
            || route.StartsWith("/api/v1/community", StringComparison.OrdinalIgnoreCase)
            || route.StartsWith("/api/v1/security", StringComparison.OrdinalIgnoreCase)
            || route.StartsWith("/api/v1/files", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRoleLabel(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return "익명 참여자";
        }

        return roleName.Trim() switch
        {
            "기사" => "익명 기사",
            "화주" => "익명 화주",
            "주문자" => "익명 주문자",
            "창고관리자" => "익명 창고 작업자",
            "관세사" => "익명 통관 참여자",
            "서버관리자" => "익명 운영자",
            var value => $"익명 {value}"
        };
    }

    private static string ResolveTimeBucket(DateTime occurredAtUtc)
    {
        var age = DateTime.UtcNow - occurredAtUtc;
        if (age <= TimeSpan.FromHours(1))
        {
            return "최근 1시간";
        }

        if (age <= TimeSpan.FromDays(1))
        {
            return "오늘";
        }

        if (age <= TimeSpan.FromDays(7))
        {
            return "최근 7일";
        }

        return occurredAtUtc.ToString("yyyy-MM-dd");
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
