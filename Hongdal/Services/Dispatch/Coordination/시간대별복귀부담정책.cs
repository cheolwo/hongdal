namespace 홍달.Services.Dispatch.Coordination;

public static class 시간대별복귀부담정책
{
    public static 복귀부담평가 평가(DateTime 기준시각Utc, decimal? 하차후복귀거리Km, string? 복귀콜선호 = null)
    {
        if (!하차후복귀거리Km.HasValue || 하차후복귀거리Km.Value <= 0m)
        {
            return new 복귀부담평가(0m, 0m, false, 기사복귀선호코드.Normalize(복귀콜선호), null);
        }

        var 한국시각 = ToKoreaTime(기준시각Utc);
        var 거리 = 하차후복귀거리Km.Value;
        var 시간 = 한국시각.TimeOfDay;
        var 선호 = 기사복귀선호코드.Normalize(복귀콜선호);

        if (시간 < TimeSpan.FromHours(12))
        {
            return ApplyPreference(
                Math.Clamp((거리 - 120m) / 20m, 0m, 6m),
                0m,
                false,
                선호,
                거리 >= 120m ? "오전 장거리 운행이지만 복귀 부담은 낮게 반영했습니다." : null);
        }

        if (시간 < TimeSpan.FromHours(16))
        {
            return ApplyPreference(
                Math.Clamp((거리 - 80m) / 15m, 0m, 10m),
                거리 <= 25m ? 3m : 0m,
                false,
                선호,
                거리 >= 80m ? "오후 장거리 운행이라 복귀 부담을 일부 반영했습니다." : null);
        }

        if (시간 < TimeSpan.FromHours(21))
        {
            return ApplyPreference(
                Math.Clamp((거리 - 25m) / 5m, 0m, 28m),
                거리 <= 25m ? 10m : 0m,
                거리 >= 25m,
                선호,
                거리 >= 25m ? "퇴근 시간대에는 기본 복귀지에서 멀어지는 운송을 강하게 감점합니다." : "퇴근 시간대 복귀지와 가까운 운송입니다.");
        }

        return ApplyPreference(
            Math.Clamp((거리 - 40m) / 8m, 0m, 18m),
            거리 <= 30m ? 6m : 0m,
            거리 >= 40m,
            선호,
            거리 >= 40m ? "야간에는 운행 종료 후 복귀 부담을 반영합니다." : "야간 복귀지와 가까운 운송입니다.");
    }

    private static 복귀부담평가 ApplyPreference(
        decimal baseBurden,
        decimal baseBonus,
        bool eveningReturnBurden,
        string preference,
        string? reason)
    {
        var burden = preference switch
        {
            기사복귀선호코드.복귀우선 => baseBurden * 1.35m,
            기사복귀선호코드.수익우선 => baseBurden * 0.25m,
            _ => baseBurden
        };
        var bonus = preference switch
        {
            기사복귀선호코드.복귀우선 => baseBonus * 1.5m,
            기사복귀선호코드.수익우선 => baseBonus * 0.25m,
            _ => baseBonus
        };

        var preferenceReason = preference switch
        {
            기사복귀선호코드.복귀우선 => "기사 선호가 복귀 우선이라 복귀 방향 콜을 더 높게 봅니다.",
            기사복귀선호코드.수익우선 => "기사 선호가 수익 우선이라 복귀 부담 감점을 낮췄습니다.",
            _ => null
        };
        var mergedReason = string.Join(" · ", new[] { reason, preferenceReason }.Where(x => !string.IsNullOrWhiteSpace(x)));

        return new 복귀부담평가(
            Math.Round(Math.Max(0m, burden), 2),
            Math.Round(Math.Max(0m, bonus), 2),
            eveningReturnBurden && burden > 0m,
            preference,
            string.IsNullOrWhiteSpace(mergedReason) ? null : mergedReason);
    }

    private static DateTime ToKoreaTime(DateTime utc)
    {
        var normalized = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        try
        {
            var timezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Seoul");
            return TimeZoneInfo.ConvertTimeFromUtc(normalized, timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            var timezone = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(normalized, timezone);
        }
        catch (InvalidTimeZoneException)
        {
            return normalized.AddHours(9);
        }
    }
}

public sealed record 복귀부담평가(
    decimal 부담점수,
    decimal 보너스점수,
    bool 퇴근시간대부담여부,
    string 복귀콜선호,
    string? 사유);
