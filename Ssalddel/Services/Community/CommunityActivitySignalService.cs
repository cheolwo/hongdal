using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.Community;
using 살뜰.Data;

namespace Ssalddel.Services.Community;

public interface ICommunityActivitySignalService
{
    Task<CommunityActivitySignalListResponse> GetSignalsAsync(
        CommunityActivitySignalQuery query,
        CancellationToken cancellationToken);
}

public sealed class CommunityActivitySignalService(SsalddelContext db)
    : ICommunityActivitySignalService
{
    public async Task<CommunityActivitySignalListResponse> GetSignalsAsync(
        CommunityActivitySignalQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 50);
        var toUtc = NormalizeUtc(request.ToUtc ?? DateTime.UtcNow);
        var fromUtc = NormalizeUtc(request.FromUtc ?? toUtc.AddDays(-28));

        var query = db.Set<커뮤니티활동공개Projection>()
            .AsNoTracking()
            .Where(x => x.VisibilityScope == 커뮤니티활동공개Policy.공개범위)
            .Where(x => x.ActivityCount >= 커뮤니티활동공개Policy.최소공개활동수)
            .Where(x => x.TimeBucketEndUtc > fromUtc && x.TimeBucketStartUtc <= toUtc);

        if (!string.IsNullOrWhiteSpace(request.AppKey))
        {
            var appKey = request.AppKey.Trim();
            query = query.Where(x => x.AppKey == appKey);
        }

        if (!string.IsNullOrWhiteSpace(request.CommunityScope))
        {
            var communityScope = request.CommunityScope.Trim();
            query = query.Where(x => x.CommunityScope == communityScope);
        }

        var projected = (await query
                .OrderByDescending(x => x.TimeBucketStartUtc)
                .ThenBy(x => x.CommunityScope)
                .ThenBy(x => x.ActivityKind)
                .ToArrayAsync(cancellationToken))
            .Select(ToResponse);

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            var tag = request.Tag.Trim();
            projected = projected.Where(x =>
                x.TopicTags.Any(candidate =>
                    string.Equals(candidate, tag, StringComparison.OrdinalIgnoreCase)));
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

    private static CommunityActivitySignalResponse ToResponse(
        커뮤니티활동공개Projection source)
        => new()
        {
            SignalId = $"activity-aggregate-{source.Id}",
            AppKey = source.AppKey,
            CommunityScope = source.CommunityScope,
            ActivityKind = source.ActivityKind,
            Title = source.Title,
            Summary =
                $"{source.PublicSummary} 해당 주간에 공개 기준을 충족한 활동 {source.ActivityCount}건이 집계되었습니다.",
            ActorRoleLabel = "집계된 참여 활동",
            TopicTags = DeserializeTags(source.TopicTagsJson),
            TimeBucketLabel = 커뮤니티활동공개Policy.주간표시(source.TimeBucketStartUtc),
            OccurredAtUtc = source.TimeBucketStartUtc,
            TimePrecision = 커뮤니티활동공개Policy.시간정밀도,
            AggregationCount = source.ActivityCount,
            VisibilityScope = source.VisibilityScope,
            PrivacyPolicyVersion = source.PrivacyPolicyVersion
        };

    private static IReadOnlyList<string> DeserializeTags(string value)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(value) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static DateTime NormalizeUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
