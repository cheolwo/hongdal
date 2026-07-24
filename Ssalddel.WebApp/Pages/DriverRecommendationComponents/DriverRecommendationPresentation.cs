using MudBlazor;
using Ssalddel.Contracts.Driver.Recommendation;

namespace Ssalddel.WebApp.Pages.DriverRecommendationComponents;

public static class DriverRecommendationPresentation
{
    public static string DisplayCargo(기사추천수신항목 item)
    {
        var type = Display(item.운송의뢰유형표시, "일반 화물");
        var cargo = Display(item.화물종류, "화물");
        return $"{cargo} · {type}";
    }

    public static string Display(string? value, string fallback = "미확인")
        => string.IsNullOrWhiteSpace(value) ? fallback : value;

    public static string DisplayMoney(decimal? value)
        => value.HasValue ? $"{value.Value:N0}원" : "금액 미정";

    public static string DisplayDistance(decimal? value)
        => value.HasValue ? $"{value.Value:N1}km" : "거리 미정";

    public static string DisplayMinutes(decimal? value)
        => value.HasValue ? $"{value.Value:N0}분" : "시간 미정";

    public static Color ResolveCountdownColor(bool isExpired, int remainingSeconds)
        => isExpired
            ? Color.Error
            : remainingSeconds <= 10
                ? Color.Warning
                : Color.Success;

    public static DateTimeOffset ResolveDeadline(
        기사추천수신항목 item,
        DateTimeOffset? selectedDeadline,
        DateTimeOffset now,
        int defaultResponseSeconds = 60)
    {
        if (selectedDeadline.HasValue)
        {
            return selectedDeadline.Value;
        }

        if (item.추천만료시각.HasValue)
        {
            return ToUtcOffset(item.추천만료시각.Value);
        }

        return now.AddSeconds(defaultResponseSeconds);
    }

    private static DateTimeOffset ToUtcOffset(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => new DateTimeOffset(value),
            DateTimeKind.Local => new DateTimeOffset(value.ToUniversalTime()),
            _ => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
        };
}
