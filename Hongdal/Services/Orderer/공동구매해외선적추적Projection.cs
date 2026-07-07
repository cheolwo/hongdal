using Hongdal.Contracts.Common.Orderer;

namespace Hongdal.Services.Orderer;

public static class 공동구매해외선적추적Projection
{
    public static 공동구매해외선적공개Dto ToPublicDto(
        공동구매해외선적추적Dto source)
        => new()
        {
            공동구매Id = source.공동구매Id,
            주문자집단배송권키 = source.주문자집단배송권키,
            주문자집단배송권명 = source.주문자집단배송권명,
            상품요약 = source.상품요약,
            문서관리번호 = source.문서관리번호,
            운송문서유형 = source.운송문서유형,
            운송문서번호 = source.운송문서번호,
            운송수단 = source.운송수단,
            운송사명 = source.운송사명,
            선박명 = source.선박명,
            항차번호 = source.항차번호,
            항공편번호 = source.항공편번호,
            출발국가코드 = source.출발국가코드,
            출발항코드 = source.출발항코드,
            도착항코드 = source.도착항코드,
            예상출발시각Utc = source.예상출발시각Utc,
            실제출발시각Utc = source.실제출발시각Utc,
            예상도착시각Utc = source.예상도착시각Utc,
            실제도착시각Utc = source.실제도착시각Utc,
            현재상태코드 = source.현재상태코드,
            현재위치요약 = source.현재위치요약,
            마지막단계시각Utc = source.마지막단계시각Utc,
            이벤트목록 = source.이벤트목록
                .Where(x => x.주문자공개여부)
                .OrderBy(x => x.발생시각Utc)
                .Select(ToPublicDto)
                .ToArray(),
            수정시각Utc = source.수정시각Utc
        };

    private static 공동구매해외선적공개이벤트Dto ToPublicDto(
        공동구매해외선적추적이벤트Dto source)
        => new()
        {
            이벤트코드 = source.이벤트코드,
            표시명 = source.표시명,
            위치요약 = source.위치요약,
            발생시각Utc = source.발생시각Utc,
            출처주체코드 = source.출처주체코드
        };
}
