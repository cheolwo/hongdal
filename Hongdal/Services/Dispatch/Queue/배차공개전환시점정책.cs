namespace 홍달.Services.Dispatch.Queue;

public static class 배차공개전환시점정책
{
    public static DateTime? 공개전환기준시각(
        DateTime 배차대기생성시각Utc,
        DateTime? 상차시간창시작Utc,
        int 당일미배정공개전환분,
        int 예약상차전공개전환시간,
        int 예약최소추천유지분)
    {
        if (당일미배정공개전환분 <= 0)
        {
            return null;
        }

        var immediateThreshold = 배차대기생성시각Utc.AddMinutes(당일미배정공개전환분);
        if (!상차시간창시작Utc.HasValue || 예약상차전공개전환시간 <= 0)
        {
            return immediateThreshold;
        }

        var reservationThreshold = 상차시간창시작Utc.Value.AddHours(-예약상차전공개전환시간);
        var pickupGap = 상차시간창시작Utc.Value - 배차대기생성시각Utc;
        if (pickupGap.TotalHours < 예약상차전공개전환시간)
        {
            return Later(reservationThreshold, immediateThreshold);
        }

        var reservationMinimumThreshold = 배차대기생성시각Utc.AddMinutes(Math.Max(당일미배정공개전환분, 예약최소추천유지분));
        return Later(reservationThreshold, reservationMinimumThreshold);
    }

    public static bool 공개전환대상(
        DateTime 기준시각Utc,
        DateTime 배차대기생성시각Utc,
        DateTime? 상차시간창시작Utc,
        int 당일미배정공개전환분,
        int 예약상차전공개전환시간,
        int 예약최소추천유지분)
    {
        var threshold = 공개전환기준시각(
            배차대기생성시각Utc,
            상차시간창시작Utc,
            당일미배정공개전환분,
            예약상차전공개전환시간,
            예약최소추천유지분);

        return threshold.HasValue && 기준시각Utc >= threshold.Value;
    }

    private static DateTime Later(DateTime first, DateTime second)
        => first >= second ? first : second;
}
