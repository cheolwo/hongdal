namespace 홍달.Services.Exploration;

public interface I탐색캠페인상태전이Service
{
    bool 전이가능(string 현재상태, string 목표상태);
    void 전이(탐색캠페인 campaign, string 목표상태, string? 실행판단사유 = null);
}

public sealed class 탐색캠페인상태전이Service : I탐색캠페인상태전이Service
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedTransitions = new Dictionary<string, string[]>
    {
        [상태값.탐색캠페인상태.초안] = [상태값.탐색캠페인상태.탐색중, 상태값.탐색캠페인상태.취소],
        [상태값.탐색캠페인상태.탐색중] = [상태값.탐색캠페인상태.응답수집중, 상태값.탐색캠페인상태.응답부족, 상태값.탐색캠페인상태.취소],
        [상태값.탐색캠페인상태.응답수집중] = [상태값.탐색캠페인상태.실행검토, 상태값.탐색캠페인상태.응답부족, 상태값.탐색캠페인상태.취소],
        [상태값.탐색캠페인상태.응답부족] = [상태값.탐색캠페인상태.실행검토, 상태값.탐색캠페인상태.종료, 상태값.탐색캠페인상태.취소],
        [상태값.탐색캠페인상태.실행검토] = [상태값.탐색캠페인상태.확정연결대기, 상태값.탐색캠페인상태.종료, 상태값.탐색캠페인상태.취소],
        [상태값.탐색캠페인상태.확정연결대기] = [상태값.탐색캠페인상태.종료, 상태값.탐색캠페인상태.취소],
        [상태값.탐색캠페인상태.종료] = [],
        [상태값.탐색캠페인상태.취소] = []
    };

    public bool 전이가능(string 현재상태, string 목표상태)
    {
        if (string.Equals(현재상태, 목표상태, StringComparison.Ordinal))
        {
            return true;
        }

        return AllowedTransitions.TryGetValue(현재상태, out var targets)
            && targets.Contains(목표상태, StringComparer.Ordinal);
    }

    public void 전이(탐색캠페인 campaign, string 목표상태, string? 실행판단사유 = null)
    {
        if (!전이가능(campaign.탐색상태, 목표상태))
        {
            throw new InvalidOperationException($"탐색캠페인 상태를 '{campaign.탐색상태}'에서 '{목표상태}'로 변경할 수 없습니다.");
        }

        campaign.탐색상태 = 목표상태;
        if (!string.IsNullOrWhiteSpace(실행판단사유))
        {
            campaign.실행판단사유 = 실행판단사유;
        }

        campaign.UpdatedAt = DateTime.UtcNow;
    }
}
