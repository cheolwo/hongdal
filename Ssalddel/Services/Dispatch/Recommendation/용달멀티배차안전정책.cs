using 살뜰.Services.Dispatch.Coordination;

namespace 살뜰.Services.Dispatch.Recommendation;

public static class 용달멀티배차안전정책
{
    public static 용달멀티배차안전판정 판정(용달멀티배차안전검토요청 request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var blockers = new List<string>();
        var warnings = new List<string>();
        var penalty = 0m;

        if (request.작업수 < 2)
        {
            blockers.Add("용달 멀티배차는 두 건 이상일 때만 검토합니다.");
        }

        if (request.작업수 > request.최대작업수)
        {
            blockers.Add($"용달 멀티배차는 현재 최대 {request.최대작업수}건까지만 검토합니다.");
        }

        if (!request.기사명시동의)
        {
            blockers.Add("기사님이 용달 멀티배차를 명시적으로 허용하지 않았습니다.");
        }

        if (!request.화주혼적허용)
        {
            blockers.Add("화주가 혼적 또는 경유 운송을 허용하지 않았습니다.");
        }

        if (request.독차필수)
        {
            blockers.Add("독차 필수 화물은 멀티배차 후보에서 제외합니다.");
        }

        if (request.민감화물)
        {
            blockers.Add("온도, 파손, 위험물 등 민감 화물은 용달 멀티배차에서 제외합니다.");
        }

        if (request.시간창위반예상)
        {
            blockers.Add("상차 또는 하차 시간창 위반이 예상됩니다.");
        }

        if (request.총운행거리Km.HasValue)
        {
            if (request.총운행거리Km.Value > request.최대총운행거리Km)
            {
                blockers.Add($"총 운행거리가 용달 멀티배차 상한을 넘습니다. 예상={request.총운행거리Km.Value:0.##}km, 상한={request.최대총운행거리Km:0.##}km");
            }

            penalty += Math.Clamp((request.총운행거리Km.Value - 60m) / 10m, 0m, 18m);
        }
        else
        {
            warnings.Add("총 운행거리 추정값이 없어 낮은 우선순위로만 검토해야 합니다.");
            penalty += 10m;
        }

        if (request.예상연속운전분.HasValue)
        {
            if (request.예상연속운전분.Value > request.최대무휴식연속운전분 && !request.휴식삽입가능)
            {
                blockers.Add($"휴식 없이 연속 운전 시간이 길어집니다. 예상={request.예상연속운전분.Value:0}분, 상한={request.최대무휴식연속운전분:0}분");
            }

            penalty += Math.Clamp((request.예상연속운전분.Value - 150m) / 20m, 0m, 16m);
        }
        else
        {
            warnings.Add("연속 운전 시간 추정값이 없어 기사 휴식 관점에서 보수적으로 검토해야 합니다.");
            penalty += 8m;
        }

        if (request.심야운전분.HasValue && request.심야운전분.Value > request.최대심야운전분)
        {
            blockers.Add($"심야 운전 시간이 길어 수면 부담이 큽니다. 예상={request.심야운전분.Value:0}분, 상한={request.최대심야운전분:0}분");
        }

        if (request.하차후복귀거리Km.HasValue && request.하차후복귀거리Km.Value >= request.복귀부담주의거리Km)
        {
            var preference = 기사복귀선호코드.Normalize(request.기사복귀선호);
            if (preference == 기사복귀선호코드.복귀우선)
            {
                blockers.Add($"복귀 우선 기사에게 하차 후 복귀 부담이 큽니다. 복귀거리={request.하차후복귀거리Km.Value:0.##}km");
            }
            else
            {
                warnings.Add($"하차 후 복귀 부담이 큽니다. 복귀거리={request.하차후복귀거리Km.Value:0.##}km");
                penalty += 12m;
            }
        }

        if (request.상하차대기예상분.HasValue && request.상하차대기예상분.Value >= request.상하차대기주의분)
        {
            warnings.Add($"상하차 대기 시간이 길어 추가 운임 또는 기사 확인이 필요합니다. 예상={request.상하차대기예상분.Value:0}분");
            penalty += 8m;
        }

        return new 용달멀티배차안전판정(
            blockers.Count == 0,
            blockers.Distinct(StringComparer.Ordinal).ToArray(),
            warnings.Distinct(StringComparer.Ordinal).ToArray(),
            Math.Round(Math.Max(0m, penalty), 2));
    }
}

public sealed record 용달멀티배차안전검토요청(
    int 작업수,
    bool 기사명시동의,
    bool 화주혼적허용,
    bool 독차필수 = false,
    bool 민감화물 = false,
    bool 시간창위반예상 = false,
    decimal? 총운행거리Km = null,
    decimal? 예상연속운전분 = null,
    bool 휴식삽입가능 = false,
    decimal? 심야운전분 = null,
    decimal? 하차후복귀거리Km = null,
    string? 기사복귀선호 = null,
    decimal? 상하차대기예상분 = null,
    int 최대작업수 = 2,
    decimal 최대총운행거리Km = 180m,
    decimal 최대무휴식연속운전분 = 240m,
    decimal 최대심야운전분 = 120m,
    decimal 복귀부담주의거리Km = 80m,
    decimal 상하차대기주의분 = 90m);

public sealed record 용달멀티배차안전판정(
    bool 허용여부,
    IReadOnlyList<string> 차단사유,
    IReadOnlyList<string> 경고,
    decimal 우선순위감점);
