using Hongdal.Contracts.Common.Exploration;
using Microsoft.EntityFrameworkCore;

namespace 홍달.Services.Exploration;

public interface I탐색대상추천Service
{
    Task<IReadOnlyList<탐색캠페인추천대상응답>> 추천Async(탐색캠페인 campaign, CancellationToken cancellationToken = default);
}

public sealed class 탐색대상추천Service : I탐색대상추천Service
{
    private readonly HongdalContext _db;

    public 탐색대상추천Service(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<탐색캠페인추천대상응답>> 추천Async(탐색캠페인 campaign, CancellationToken cancellationToken = default)
    {
        if (campaign is null)
        {
            return Array.Empty<탐색캠페인추천대상응답>();
        }

        if (!string.Equals(campaign.개시자역할, 탐색캠페인개시자역할값.기사, StringComparison.Ordinal)
            || !string.Equals(campaign.대상역할, 탐색캠페인대상역할값.화주, StringComparison.Ordinal))
        {
            return Array.Empty<탐색캠페인추천대상응답>();
        }

        var relations = await _db.기사화주관계집계
            .AsNoTracking()
            .Where(x => x.기사Id == campaign.개시자UserId)
            .OrderByDescending(x => x.양방향관계점수)
            .ThenByDescending(x => x.기사발신응답률)
            .ThenByDescending(x => x.최근거래일시)
            .Take(Math.Max(1, campaign.모집대상수))
            .ToListAsync(cancellationToken);

        return relations
            .Select(x => new 탐색캠페인추천대상응답
            {
                대상UserId = x.화주UserId,
                대상역할 = 탐색캠페인대상역할값.화주,
                대상명 = x.화주UserId,
                관계점수 = x.양방향관계점수,
                반응가능성점수 = x.기사발신응답률,
                최종추천점수 = decimal.Round((x.양방향관계점수 * 0.7m) + (x.기사발신응답률 * 0.3m), 3, MidpointRounding.AwayFromZero),
                선정사유 = BuildReason(x),
                선호출발권역 = x.선호출발권역,
                선호도착권역 = x.선호도착권역
            })
            .ToArray();
    }

    private static string BuildReason(기사화주관계집계 relation)
    {
        var reasons = new List<string>();

        if (relation.양방향관계점수 > 0)
        {
            reasons.Add($"관계점수 {relation.양방향관계점수:0.###}");
        }

        if (relation.기사발신응답률 > 0)
        {
            reasons.Add($"기사발신응답률 {relation.기사발신응답률:P0}");
        }

        if (relation.최근거래일시.HasValue)
        {
            reasons.Add($"최근거래 {relation.최근거래일시.Value:yyyy-MM-dd}");
        }

        return reasons.Count == 0 ? "관계 데이터 기반 기본 추천" : string.Join(" · ", reasons);
    }
}
