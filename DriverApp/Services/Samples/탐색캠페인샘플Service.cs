using Ssalddel.Contracts.Common.Exploration;

namespace DriverApp.Services.Samples;

public sealed class 탐색캠페인샘플Service : IDriverExplorationCampaignService
{
    public IReadOnlyList<탐색캠페인목록항목응답> 캠페인목록() =>
    [
        new()
        {
            Id = 3001,
            개시자역할 = 탐색캠페인개시자역할값.기사,
            대상역할 = 탐색캠페인대상역할값.화주,
            탐색유형 = 탐색캠페인유형값.운행가능문의,
            탐색명 = "수도권 오전 회차 물량 탐색",
            운행예정일 = DateTime.Today.AddDays(1).AddHours(8),
            출발권역 = "경기 남부",
            희망도착권역 = "서울 서북권",
            희망복귀지주소 = "경기 구리시 인창동",
            복귀지출처 = "오늘복귀지",
            차량종류 = "1톤 카고",
            탐색상태 = 탐색캠페인상태값.응답수집중,
            모집대상수 = 12,
            응답수 = 5,
            있음응답수 = 3,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-15)
        },
        new()
        {
            Id = 3002,
            개시자역할 = 탐색캠페인개시자역할값.기사,
            대상역할 = 탐색캠페인대상역할값.화주,
            탐색유형 = 탐색캠페인유형값.운행가능문의,
            탐색명 = "인천항 오후 냉장 회차 탐색",
            운행예정일 = DateTime.Today.AddDays(2).AddHours(13),
            출발권역 = "인천항",
            희망도착권역 = "경기 북부",
            희망복귀지주소 = "경기 의정부시 호원동",
            복귀지출처 = "기본복귀지",
            차량종류 = "1톤 냉장",
            탐색상태 = 탐색캠페인상태값.실행검토,
            모집대상수 = 8,
            응답수 = 6,
            있음응답수 = 4,
            UpdatedAt = DateTime.UtcNow.AddHours(-2)
        },
        new()
        {
            Id = 3003,
            개시자역할 = 탐색캠페인개시자역할값.기사,
            대상역할 = 탐색캠페인대상역할값.화주,
            탐색유형 = 탐색캠페인유형값.운행가능문의,
            탐색명 = "부천-안양 야간 상온 물량 탐색",
            운행예정일 = DateTime.Today.AddDays(1).AddHours(21),
            출발권역 = "부천",
            희망도착권역 = "안양",
            희망복귀지주소 = "서울 중랑구 면목동",
            복귀지출처 = "오늘복귀지",
            차량종류 = "다마스",
            탐색상태 = 탐색캠페인상태값.초안,
            모집대상수 = 5,
            응답수 = 0,
            있음응답수 = 0,
            UpdatedAt = DateTime.UtcNow.AddHours(-5)
        }
    ];

    public 탐색캠페인상세응답 상세(long id)
    {
        var summary = 캠페인목록().First(x => x.Id == id);
        return new 탐색캠페인상세응답
        {
            Id = summary.Id,
            개시자UserId = "driver-local-sample",
            개시자역할 = summary.개시자역할,
            대상역할 = summary.대상역할,
            탐색유형 = summary.탐색유형,
            탐색명 = summary.탐색명,
            운행예정일 = summary.운행예정일,
            출발권역 = summary.출발권역,
            희망도착권역 = summary.희망도착권역,
            희망복귀지주소 = summary.희망복귀지주소,
            복귀지출처 = summary.복귀지출처,
            차량종류 = summary.차량종류,
            최대적재중량Kg = summary.차량종류.Contains("1톤") ? 1000 : 350,
            최대적재부피Cbm = summary.차량종류.Contains("냉장") ? 6.5m : 4.2m,
            모집대상수 = summary.모집대상수,
            탐색상태 = summary.탐색상태,
            메모 = "샘플 데이터 기반 탐색 캠페인입니다. 복귀지 기준으로 추천이 조정됩니다.",
            실행판단사유 = summary.탐색상태 == 탐색캠페인상태값.실행검토 ? "응답 밀도가 높아 실행 검토 단계로 전환" : null,
            응답수 = summary.응답수,
            있음응답수 = summary.있음응답수,
            예상총중량Kg = 1200,
            예상총부피Cbm = 7.8m,
            CreatedAt = summary.UpdatedAt.AddDays(-1),
            UpdatedAt = summary.UpdatedAt,
            대상자목록 =
            [
                new() { 대상UserId = "shipper-a", 대상역할 = 탐색캠페인대상역할값.화주, 대상명 = "화주 A", 관계점수Snapshot = 0.88m, 대상상태 = 탐색캠페인대상상태값.있음응답, 선정사유 = "최근 거래 빈도 높음", 마지막응답일시 = DateTime.UtcNow.AddMinutes(-50), 응답유형 = 운행문의응답유형.있음.ToString(), 예상중량Kg = 400, 예상부피Cbm = 2.5m },
                new() { 대상UserId = "shipper-b", 대상역할 = 탐색캠페인대상역할값.화주, 대상명 = "화주 B", 관계점수Snapshot = 0.73m, 대상상태 = 탐색캠페인대상상태값.발송됨, 선정사유 = "응답률 우수", 마지막응답일시 = null, 응답유형 = null, 예상중량Kg = null, 예상부피Cbm = null },
                new() { 대상UserId = "shipper-c", 대상역할 = 탐색캠페인대상역할값.화주, 대상명 = "화주 C", 관계점수Snapshot = 0.69m, 대상상태 = 탐색캠페인대상상태값.없음응답, 선정사유 = "선호 도착권역 일치", 마지막응답일시 = DateTime.UtcNow.AddHours(-1), 응답유형 = 운행문의응답유형.없음.ToString(), 예상중량Kg = 0, 예상부피Cbm = 0 }
            ]
        };
    }

    public IReadOnlyList<탐색캠페인추천대상응답> 추천대상(long campaignId) =>
    [
        new() { 대상UserId = "shipper-a", 대상역할 = 탐색캠페인대상역할값.화주, 대상명 = "화주 A", 관계점수 = 0.88m, 반응가능성점수 = 0.84m, 최종추천점수 = 0.868m, 선정사유 = "최근 거래 12건 · 응답률 우수", 선호출발권역 = "경기 남부", 선호도착권역 = "서울 서북권" },
        new() { 대상UserId = "shipper-d", 대상역할 = 탐색캠페인대상역할값.화주, 대상명 = "화주 D", 관계점수 = 0.80m, 반응가능성점수 = 0.70m, 최종추천점수 = 0.77m, 선정사유 = "회차 노선 일치", 선호출발권역 = "인천항", 선호도착권역 = "경기 북부" },
        new() { 대상UserId = "shipper-e", 대상역할 = 탐색캠페인대상역할값.화주, 대상명 = "화주 E", 관계점수 = 0.65m, 반응가능성점수 = 0.72m, 최종추천점수 = 0.671m, 선정사유 = "야간 대응 경험", 선호출발권역 = "부천", 선호도착권역 = "안양" }
    ];
}
