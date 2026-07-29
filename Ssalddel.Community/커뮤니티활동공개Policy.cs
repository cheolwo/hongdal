namespace Ssalddel.Community;

public static class 커뮤니티활동공개Policy
{
    public const int 최소공개활동수 = 5;
    public const string 공개범위 = "PublicAggregated";
    public const string 개인정보PolicyVersion = "2026-07-28-v1";
    public const string 시간정밀도 = "Week";

    public static bool 공개가능한가(int activityCount)
        => activityCount >= 최소공개활동수;

    public static DateTime 주간시작Utc(DateTime occurredAtUtc)
    {
        var normalized = NormalizeUtc(occurredAtUtc).Date;
        var daysSinceMonday = ((int)normalized.DayOfWeek + 6) % 7;
        return normalized.AddDays(-daysSinceMonday);
    }

    public static DateTime 주간종료Utc(DateTime bucketStartUtc)
        => 주간시작Utc(bucketStartUtc).AddDays(7);

    public static string 주간표시(DateTime bucketStartUtc)
        => $"{주간시작Utc(bucketStartUtc):yyyy-MM-dd} 주간";

    private static DateTime NormalizeUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
