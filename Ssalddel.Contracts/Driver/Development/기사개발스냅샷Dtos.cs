namespace Ssalddel.Contracts.Driver.Development;

public sealed class 기사개발스냅샷응답
{
    public 기사개발현재위치응답 현재위치 { get; set; } = new();
    public 기사개발근무상태응답 근무상태 { get; set; } = new();
    public 기사개발정산요약응답 정산요약 { get; set; } = new();
    public IReadOnlyList<기사개발추천의뢰응답> 추천의뢰목록 { get; set; } = [];
    public IReadOnlyList<기사개발예약응답> 예약목록 { get; set; } = [];
    public IReadOnlyList<기사개발운송응답> 운송목록 { get; set; } = [];
    public IReadOnlyList<기사개발알림응답> 알림목록 { get; set; } = [];
}

public sealed class 기사개발현재위치응답
{
    public string 위치명 { get; set; } = string.Empty;
    public decimal 위도 { get; set; }
    public decimal 경도 { get; set; }
    public DateTime 갱신시각 { get; set; }
}

public sealed class 기사개발근무상태응답
{
    public string 기사명 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public string 시작모드 { get; set; } = string.Empty;
    public string 시작위치 { get; set; } = string.Empty;
    public string? 복귀지 { get; set; }
    public DateTime 시작시각 { get; set; }
    public int 추천콜수 { get; set; }
    public int 오늘예약수 { get; set; }
}

public sealed class 기사개발정산요약응답
{
    public int 년도 { get; set; }
    public int 월 { get; set; }
    public int 배차건수 { get; set; }
    public decimal 이용료 { get; set; }
    public decimal 월상한 { get; set; }
    public bool 결제완료 { get; set; }
    public IReadOnlyList<기사개발정산상세응답> 상세항목 { get; set; } = [];
}

public sealed class 기사개발정산상세응답
{
    public string 항목명 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public decimal 금액 { get; set; }
}

public sealed class 기사개발추천의뢰응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
    public string 운송방식 { get; set; } = string.Empty;
    public string 운송의뢰유형코드 { get; set; } = "GeneralCargoTransport";
    public string 운송의뢰유형표시 { get; set; } = "일반 화물";
    public bool 당일상차필수 { get; set; }
    public bool 당일하차필수 { get; set; }
    public string 차량톤수 { get; set; } = string.Empty;
    public string 차량형태 { get; set; } = string.Empty;
    public bool 인수증필요 { get; set; }
    public bool 공동주문운송여부 { get; set; }
    public bool 세대배송포함여부 { get; set; }
    public int? 세대배송건수 { get; set; }
    public string 세대배송업무표시 { get; set; } = "상하차";
    public string 결제방식 { get; set; } = string.Empty;
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
    public DateTime? 추천시작시각 { get; set; }
    public DateTime? 추천만료시각 { get; set; }
}

public sealed class 기사개발예약응답
{
    public long Id { get; set; }
    public DateTime 시작시각 { get; set; }
    public string 시작모드 { get; set; } = string.Empty;
    public string 시작위치 { get; set; } = string.Empty;
    public string? 복귀지 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
}

public sealed class 기사개발운송응답
{
    public long Id { get; set; }
    public string 의뢰Id { get; set; } = string.Empty;
    public string 화물종류 { get; set; } = string.Empty;
    public string 픽업지 { get; set; } = string.Empty;
    public string 하차지 { get; set; } = string.Empty;
    public decimal? 픽업위도 { get; set; }
    public decimal? 픽업경도 { get; set; }
    public decimal? 하차위도 { get; set; }
    public decimal? 하차경도 { get; set; }
    public string 현재단계 { get; set; } = string.Empty;
    public DateTime 예정시각 { get; set; }
    public decimal 운송거리Km { get; set; }
    public decimal 예상수익 { get; set; }
    public bool 인수증필요 { get; set; }
    public bool 인수증서명필수 { get; set; }
    public string 결제방식 { get; set; } = string.Empty;
    public string 다음행동 { get; set; } = string.Empty;
}

public sealed class 기사개발알림응답
{
    public long Id { get; set; }
    public string 종류 { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 내용 { get; set; } = string.Empty;
    public DateTime 발생시각 { get; set; }
    public bool 읽음 { get; set; }
}
