using Ssalddel.WorkflowRules;

namespace 살뜰.Services.Dispatch.Queue;

public static class 기사대기Aging점수정책
{
    public const decimal 최대점수 = 화물배차기사대기점수Policy.최대점수;
    public const decimal 점수단위분 = 화물배차기사대기점수Policy.점수단위분;
    public const decimal 단위점수 = 화물배차기사대기점수Policy.단위점수;

    public static decimal 계산(DateTime? 마지막추천상호작용시각Utc, DateTime 기준시각Utc)
    {
        if (!마지막추천상호작용시각Utc.HasValue || 마지막추천상호작용시각Utc.Value >= 기준시각Utc)
        {
            return 0m;
        }

        var 대기분 = (decimal)(기준시각Utc - 마지막추천상호작용시각Utc.Value).TotalMinutes;
        return 화물배차기사대기점수Policy.계산(대기분);
    }
}
