using Ssalddel.Contracts.Common.Orderer;
using 살뜰.Services.External.Customs;
using 살뜰.도메인.통관;

namespace Ssalddel.Services.Orderer;

public static class 공동구매해외선적통관단계Mapper
{
    public static 공동구매해외선적추적이벤트추가요청 To선적Event(
        화물통관진행조회Result source,
        bool isOrdererVisible)
        => new()
        {
            이벤트코드 = To선적StatusCode(source.진행단계),
            표시명 = Resolve표시명(source.처리단계명),
            위치요약 = Resolve위치요약(source.장치장명),
            발생시각Utc = source.조회시각.UtcDateTime,
            출처주체코드 = 공동구매물류워크플로우주체코드.관세사,
            증빙참조 = "KCS CargoTracking OpenAPI",
            메모 = "관세청 화물통관진행정보 조회 결과를 공동구매 해외 선적 원장에 반영했습니다.",
            주문자공개여부 = isOrdererVisible
        };

    public static string To선적StatusCode(통관진행단계 stage)
        => stage switch
        {
            통관진행단계.반입전 => 공동구매선적상태코드.운송중,
            통관진행단계.반입완료 => 공동구매선적상태코드.항만도착,
            통관진행단계.신고진행중 => 공동구매선적상태코드.통관진행중,
            통관진행단계.검사대상 => 공동구매선적상태코드.통관진행중,
            통관진행단계.신고수리 => 공동구매선적상태코드.통관완료,
            통관진행단계.반출가능 => 공동구매선적상태코드.통관완료,
            통관진행단계.반출완료 => 공동구매선적상태코드.통관완료,
            통관진행단계.완료 => 공동구매선적상태코드.통관완료,
            통관진행단계.보류 => 공동구매선적상태코드.예외,
            _ => 공동구매선적상태코드.통관진행중
        };

    public static string Resolve위치요약(string? customsLocation)
        => string.IsNullOrWhiteSpace(customsLocation)
            ? "관세청 위치 정보 미제공"
            : customsLocation.Trim();

    private static string Resolve표시명(string? customsStageName)
        => string.IsNullOrWhiteSpace(customsStageName)
            ? "관세청 통관 상태 조회"
            : $"관세청 통관 상태: {customsStageName.Trim()}";
}
