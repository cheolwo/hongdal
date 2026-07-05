namespace DriverApp.Models.Driver;

public sealed class DriverRequestItem
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
    public string 운송방식 { get; set; } = "혼적";
    public bool 당일상차필수 { get; set; }
    public bool 당일하차필수 { get; set; }
    public string 차량톤수 { get; set; } = string.Empty;
    public string 차량형태 { get; set; } = string.Empty;
    public bool 인수증필요 { get; set; }
    public string 결제방식 { get; set; } = "하차 후 계좌";
    public string 픽업지 { get; set; } = string.Empty;
    public string 하차지 { get; set; } = string.Empty;
    public decimal? 픽업_위도 { get; set; }
    public decimal? 픽업_경도 { get; set; }
    public decimal? 하차_위도 { get; set; }
    public decimal? 하차_경도 { get; set; }
    public decimal? 직선거리Km { get; set; }
    public decimal? 픽업거리Km { get; set; }
    public decimal? 공차거리Km { get; set; }
    public decimal? 운송거리Km { get; set; }
    public decimal? 복귀예상거리Km { get; set; }
    public decimal? 지금바로복귀거리Km { get; set; }
    public decimal? 복귀우회증가거리Km { get; set; }
    public decimal? 총공차거리Km { get; set; }
    public decimal? 주행거리Km { get; set; }
    public decimal? 예상톨비 { get; set; }
    public decimal? 예상연료비 { get; set; }
    public decimal? 예상총비용 { get; set; }
    public decimal? 예상수익 { get; set; }
    public decimal? 추천점수 { get; set; }
    public string 추천사유 { get; set; } = string.Empty;
    public bool 복귀지기준추천여부 { get; set; }
    public string? 복귀지출처 { get; set; }
    public string? 복귀추천사유 { get; set; }
    public string 요약설명 { get; set; } = string.Empty;
    public string 상세설명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 배차상태 { get; set; } = string.Empty;

    public string 경로표시 => $"{픽업지} → {하차지}";
    public string 운송조건표시 => string.IsNullOrWhiteSpace(운송방식) ? "운송방식 미정" : 운송방식;
    public string 시간조건표시 => (당일상차필수, 당일하차필수) switch
    {
        (true, true) => "당상·당착",
        (true, false) => "당상",
        (false, true) => "당착",
        _ => "일반"
    };
    public string 차량조건표시 => string.IsNullOrWhiteSpace(차량톤수) && string.IsNullOrWhiteSpace(차량형태)
        ? "차량조건 미정"
        : $"{차량톤수}/{차량형태}".Trim('/');
    public string 거리표시 => 직선거리Km.HasValue || 주행거리Km.HasValue
        ? $"직선 {직선거리Km?.ToString("0.0") ?? "-"}km / 주행 {주행거리Km?.ToString("0.0") ?? "-"}km"
        : "거리 미정";

    public string 인수증표시 => 인수증필요 ? "인수증 필요" : "인수증 없음";

    public string 상차시간표시 => 당일상차필수 ? "당일 상차" : "상차 시간 협의";

    public string 하차시간표시 => 당일하차필수 ? "당일 하차" : "하차 시간 협의";

    public string 하차정산표시 => $"{결제방식} · {인수증표시}";

    public string 복귀표시 => !복귀지기준추천여부
        ? "복귀 기준 미적용"
        : $"복귀 {복귀예상거리Km?.ToString("0.0") ?? "-"}km / 우회 {복귀우회증가거리Km?.ToString("+0.0;-0.0;0.0") ?? "-"}km";

    public double 픽업위도
    {
        get => (double)(픽업_위도 ?? 0m);
        set => 픽업_위도 = (decimal)value;
    }

    public double 픽업경도
    {
        get => (double)(픽업_경도 ?? 0m);
        set => 픽업_경도 = (decimal)value;
    }

    public double 하차위도
    {
        get => (double)(하차_위도 ?? 0m);
        set => 하차_위도 = (decimal)value;
    }

    public double 하차경도
    {
        get => (double)(하차_경도 ?? 0m);
        set => 하차_경도 = (decimal)value;
    }
}
