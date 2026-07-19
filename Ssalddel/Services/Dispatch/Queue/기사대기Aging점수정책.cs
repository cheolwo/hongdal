namespace 살뜰.Services.Dispatch.Queue;

public static class 기사대기Aging점수정책
{
    public const decimal 최대점수 = 24m;
    public const decimal 점수단위분 = 30m;
    public const decimal 단위점수 = 3m;

    public static decimal 계산(DateTime? 마지막추천상호작용시각Utc, DateTime 기준시각Utc)
    {
        if (!마지막추천상호작용시각Utc.HasValue || 마지막추천상호작용시각Utc.Value >= 기준시각Utc)
        {
            return 0m;
        }

        var 대기분 = (decimal)(기준시각Utc - 마지막추천상호작용시각Utc.Value).TotalMinutes;
        if (대기분 < 점수단위분)
        {
            return 0m;
        }

        var score = Math.Floor(대기분 / 점수단위분) * 단위점수;
        return Math.Clamp(score, 0m, 최대점수);
    }
}
