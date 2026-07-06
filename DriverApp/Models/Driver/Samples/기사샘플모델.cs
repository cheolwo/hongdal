namespace DriverApp.Models.Driver.Samples;

public sealed record 기사현재위치샘플(
    string 위치명,
    decimal 위도,
    decimal 경도,
    DateTime 갱신시각);

public sealed record 추천의뢰표시항목(
    DriverRequestItem 의뢰,
    decimal 상차지까지거리Km,
    int 가까운순위)
{
    public string 상차지까지거리표시 => $"{상차지까지거리Km:0.0}km";
    public string 추천점수표시 => $"{의뢰.추천점수 ?? 0m:0}점";
    public string 예상수익표시 => $"{의뢰.예상수익 ?? 0m:0}원";
    public string 운송거리표시 => $"{의뢰.운송거리Km ?? 0m:0.0}km";
}

public sealed record 기사근무샘플상태(
    string 기사명,
    string 운행상태,
    string 시작모드,
    string 시작위치,
    string? 복귀지,
    DateTime 시작시각,
    int 추천콜수,
    int 오늘예약수);

public sealed record 기사예약샘플항목(
    long Id,
    DateTime 시작시각,
    string 시작모드,
    string 시작위치,
    string? 복귀지,
    string 상태,
    string 메모);

public sealed record 기사운송샘플항목(
    long Id,
    string 의뢰Id,
    string 화물종류,
    string 픽업지,
    string 하차지,
    decimal? 픽업위도,
    decimal? 픽업경도,
    decimal? 하차위도,
    decimal? 하차경도,
    string 현재단계,
    DateTime 예정시각,
    decimal 운송거리Km,
    decimal 예상수익,
    string 다음행동);

public sealed record 기사정산샘플요약(
    int 년도,
    int 월,
    int 배차건수,
    decimal 이용료,
    decimal 월상한,
    bool 결제완료,
    IReadOnlyList<기사정산샘플상세항목> 상세항목);

public sealed record 기사정산샘플상세항목(
    string 항목명,
    string 설명,
    decimal 금액);

public sealed record 기사알림샘플항목(
    long Id,
    string 종류,
    string 제목,
    string 내용,
    DateTime 발생시각,
    bool 읽음);
