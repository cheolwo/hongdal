using Hongdal.Contracts.Admin.Exploration;

namespace HongdalAdmin.Services;

public sealed class 탐색캠페인샘플Service
{
    public IReadOnlyList<탐색캠페인관리목록응답> 캠페인목록() =>
    [
        new() { Id = 3001, 개시자UserId = "driver-local-sample", 개시자명 = "김기사", 개시자역할 = "기사", 대상역할 = "화주", 탐색유형 = "운행가능문의", 탐색명 = "수도권 오전 회차 물량 탐색", 운행예정일 = DateTime.Today.AddDays(1), 출발권역 = "경기 남부", 희망도착권역 = "서울 서북권", 차량종류 = "1톤 카고", 탐색상태 = "응답수집중", 모집대상수 = 12, 응답수 = 5, 있음응답수 = 3, UpdatedAt = DateTime.UtcNow.AddMinutes(-20) },
        new() { Id = 3002, 개시자UserId = "shipper-sample", 개시자명 = "샘플화주", 개시자역할 = "화주", 대상역할 = "기사", 탐색유형 = "물량문의", 탐색명 = "인천 냉장차 긴급 확보", 운행예정일 = DateTime.Today.AddDays(2), 출발권역 = "인천", 희망도착권역 = "남양주", 차량종류 = "1톤 냉장", 탐색상태 = "실행검토", 모집대상수 = 20, 응답수 = 11, 있음응답수 = 6, UpdatedAt = DateTime.UtcNow.AddHours(-1) },
        new() { Id = 3003, 개시자UserId = "driver-3", 개시자명 = "이기사", 개시자역할 = "기사", 대상역할 = "화주", 탐색유형 = "운행가능문의", 탐색명 = "부천-안양 야간 상온 물량 탐색", 운행예정일 = DateTime.Today.AddDays(1), 출발권역 = "부천", 희망도착권역 = "안양", 차량종류 = "다마스", 탐색상태 = "초안", 모집대상수 = 5, 응답수 = 0, 있음응답수 = 0, UpdatedAt = DateTime.UtcNow.AddHours(-6) }
    ];

    public 탐색캠페인응답통계응답 통계() => new()
    {
        총탐색수 = 3,
        총발송대상수 = 37,
        총응답수 = 16,
        있음응답수 = 9,
        전체응답률 = 0.432m,
        있음응답률 = 0.243m
    };

    public IReadOnlyList<기사화주관계집계응답> 관계집계() =>
    [
        new() { Id = 1, 기사Id = "driver-local-sample", 기사명 = "김기사", 화주UserId = "shipper-a", 화주명 = "화주 A", 최근거래일시 = DateTime.UtcNow.AddDays(-3), 누적운송건수 = 18, 기사발신응답률 = 0.81m, 화주발신응답률 = 0.62m, 최근30일접점수 = 9, 취소율 = 0.04m, 양방향관계점수 = 0.86m, 기사발신최근접촉일시 = DateTime.UtcNow.AddDays(-2), 화주발신최근접촉일시 = DateTime.UtcNow.AddDays(-7) },
        new() { Id = 2, 기사Id = "driver-2", 기사명 = "박기사", 화주UserId = "shipper-b", 화주명 = "화주 B", 최근거래일시 = DateTime.UtcNow.AddDays(-5), 누적운송건수 = 11, 기사발신응답률 = 0.74m, 화주발신응답률 = 0.58m, 최근30일접점수 = 6, 취소율 = 0.07m, 양방향관계점수 = 0.72m, 기사발신최근접촉일시 = DateTime.UtcNow.AddDays(-3), 화주발신최근접촉일시 = DateTime.UtcNow.AddDays(-6) },
        new() { Id = 3, 기사Id = "driver-3", 기사명 = "이기사", 화주UserId = "shipper-c", 화주명 = "화주 C", 최근거래일시 = DateTime.UtcNow.AddDays(-10), 누적운송건수 = 5, 기사발신응답률 = 0.55m, 화주발신응답률 = 0.41m, 최근30일접점수 = 3, 취소율 = 0.10m, 양방향관계점수 = 0.49m, 기사발신최근접촉일시 = DateTime.UtcNow.AddDays(-9), 화주발신최근접촉일시 = DateTime.UtcNow.AddDays(-12) }
    ];
}
